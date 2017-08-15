﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackerEnabledDbContext.EFCore.Interfaces;
using TrackerEnabledDbContext.EFCore.Models;

namespace TrackerEnabledDbContext.EFCore.Tools
{
    public class LogDataMigration
    {
        private readonly ITrackerContext _trackerContext;

        public LogDataMigration(ITrackerContext trackerContext)
        {
            _trackerContext = trackerContext ?? throw new ArgumentNullException(nameof(trackerContext));
        }

        public async Task MigrateLegacyLogDataAsync(
            IProgress<MigrationJobStatus> migrationProgress = null)
        {
            await MigrateLegacyLogDataAsync(CancellationToken.None, migrationProgress);
        }

        public async Task MigrateLegacyLogDataAsync(CancellationToken cancellationToken,
            IProgress<MigrationJobStatus> migrationProgress = null)
        {
            await Task.Run(() =>
            {
                MigrateDataWithProgress(migrationProgress);
            }, cancellationToken);
        }

        public void MigrateLegacyLogData()
        {
            MigrateDataWithProgress(null);
        }

        public event EventHandler<NameChangedEventArgs> AuditLogUpdated;

        public event EventHandler<NameChangedEventArgs> AuditLogDetailUpdated;

        //public void 

        #region private

        private void MigrateDataWithProgress(IProgress<MigrationJobStatus> progress)
        {
            Type contextType = _trackerContext.GetType();

            IList<Type> entityTypes = GetEntityTypes(contextType);
            int totalEntitiesCount = entityTypes.Count;
            int entityIndex = 0;

            foreach (Type entityType in entityTypes)
            {
                MigrateEntity(progress, CancellationToken.None, entityType,
                    ref entityIndex, totalEntitiesCount);
            }
        }

        private void MigrateEntity(IProgress<MigrationJobStatus> migrationProgress, CancellationToken cancellationToken,
            Type entityType, ref int entityIndex, int totalEntitiesCount)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string typeFullname = entityType.FullName;

            string tableName = GetTableName(entityType);

            IEnumerable<AuditLog> auditLogRows = GetAuditLogRows(tableName);

            foreach (AuditLog auditLogRow in auditLogRows)
            {
                UpdateEditLogRow(cancellationToken, entityType, auditLogRow, typeFullname);
            }

            migrationProgress?.Report(new MigrationJobStatus
            {
                EntityFullName = typeFullname,
                Percent = (++entityIndex*100)/totalEntitiesCount
            });
        }

        private void UpdateEditLogRow(CancellationToken cancellationToken, Type entityType, AuditLog auditLogRow,
            string typeFullname)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RenameTableNameToTypeFullname(auditLogRow, typeFullname);

            IEnumerable<PropertyInfo> properties = GetProperties(entityType);

            foreach (PropertyInfo propertyInfo in properties)
            {
                MigrateProperty(cancellationToken, typeFullname, propertyInfo);
            }
        }

        private void MigrateProperty(CancellationToken cancellationToken, string typeFullname, PropertyInfo propertyInfo)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string columnName = GetColumnName(propertyInfo);

            string propertyName = propertyInfo.Name;

            IEnumerable<AuditLogDetail> logDetails = GetAuditLogDetailRows(columnName, typeFullname);

            foreach (AuditLogDetail auditLogDetail in logDetails)
            {
                cancellationToken.ThrowIfCancellationRequested();
                RenameColumnNameToPropertyName(auditLogDetail, propertyName);
            }
        }

        private void RenameColumnNameToPropertyName(AuditLogDetail auditLogDetail, string propertyName)
        {
            string oldName = auditLogDetail.PropertyName;

            if (oldName == propertyName) return;

            auditLogDetail.PropertyName = propertyName;
            _trackerContext.SaveChanges();

            OnAuditLogDetailUpdated(new NameChangedEventArgs
            {
                OldName = oldName,
                NewName = propertyName,
                RecordId = auditLogDetail.Id
            });
        }

        private IEnumerable<AuditLogDetail> GetAuditLogDetailRows(string columnName, string typeFullname)
        {
            return _trackerContext.LogDetails
                .Where(x => x.PropertyName == columnName
                            && x.Log.TypeFullName == typeFullname);
        }

        private string GetColumnName(PropertyInfo propertyInfo)
        {
            ColumnAttribute attr = propertyInfo.GetCustomAttribute<ColumnAttribute>(false);
            if (attr != null)
            {
                return attr.Name;
            }

            return propertyInfo.Name;
        }

        private IEnumerable<PropertyInfo> GetProperties(Type entityType)
        {
            return entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        private void RenameTableNameToTypeFullname(AuditLog auditLogRow, string typeFullname)
        {
            string oldName = auditLogRow.TypeFullName;

            auditLogRow.TypeFullName = typeFullname;
            _trackerContext.SaveChanges();

            OnAuditLogUpdated(new NameChangedEventArgs
            {
                OldName = oldName,
                NewName = typeFullname,
                RecordId = auditLogRow.AuditLogId
            });
        }

        private IEnumerable<AuditLog> GetAuditLogRows(string tableName)
        {
            return _trackerContext.AuditLog
                .Where(x => x.TypeFullName == tableName);
        }

        private string GetTableName(Type entityType)
        {
            TableAttribute attr = entityType.GetCustomAttribute<TableAttribute>();
            if (attr != null)
            {
                return attr.Name;
            }

            return GetDbSetName(entityType);
        }

        private string GetDbSetName(Type entityType)
        {
            PropertyInfo[] allProperties = _trackerContext.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            IEnumerable<PropertyInfo> dbsets = allProperties.Where(x =>
                IsAssignableToGenericType(x.PropertyType, typeof (DbSet<>)));

            PropertyInfo requiredProperty = dbsets.Single(x => x.PropertyType.GenericTypeArguments[0] == entityType);

            return requiredProperty.Name;
        }

        private IList<Type> GetEntityTypes(Type contextType)
        {
            Type ignoreInterfaceType =
                typeof (AuditLog).Assembly.GetType("TrackerEnabledDbContext.Common.Interfaces.IUnTrackable");

            PropertyInfo[] allProperties = contextType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            IEnumerable<PropertyInfo> dbsets = allProperties.Where(x =>
                IsAssignableToGenericType(x.PropertyType, typeof (DbSet<>)));

            return dbsets
                .Select(x => x.PropertyType.GetGenericArguments().Single())
                .Where(x => !ignoreInterfaceType.IsAssignableFrom(x))
                .ToList();
        }

        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            Type[] interfaceTypes = givenType.GetInterfaces();

            foreach (Type it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        private void OnAuditLogDetailUpdated(NameChangedEventArgs e)
        {
            AuditLogDetailUpdated?.Invoke(this, e);
        }

        private void OnAuditLogUpdated(NameChangedEventArgs e)
        {
            AuditLogUpdated?.Invoke(this, e);
        }

        #endregion
    }
}
