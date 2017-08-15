﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TrackerEnabledDbContext.EFCore.Extensions;
using TrackerEnabledDbContext.EFCore.Interfaces;
using TrackerEnabledDbContext.EFCore.Models;

namespace TrackerEnabledDbContext.EFCore.Tests.Common.Extensions
{
    public static class AuditAssertExtensions
    {
        public static T AssertAuditForAddition<T>(this T entity, ITrackerContext db, object entityId,
            string userName = null, params Expression<Func<T, object>>[] propertyExpressions)
        {
            IEnumerable<AuditLog> logs = db.GetLogs<T>(entityId)
                .Where(x => x.EventType == EventType.Added && userName == x.UserName)
                .AssertCountIsNotZero("log count is zero");

            AuditLog lastLog = logs.LastOrDefault()
                .AssertIsNotNull("log not found");

            lastLog.LogDetails
                .AssertCountIsNotZero("no log details found")
                .AssertCount(propertyExpressions.Count());

            foreach (Expression<Func<T, object>> expression in propertyExpressions)
            {
                KeyValuePair<string, string> keyValuePair = entity.GetKeyValuePair(expression);

                lastLog.LogDetails.AssertAny(x => x.NewValue == keyValuePair.Value
                                                  && x.PropertyName == keyValuePair.Key);
            }

            return entity;
        }

        public static T AssertMetadata<T>(this T entity, ITrackerContext db, object entityId,
            Dictionary<string, string> metadataCollection = null)
        {
            IEnumerable<AuditLog> logs = db.GetLogs<T>(entityId)
                .AssertCountIsNotZero("log count is zero");

            AuditLog lastLog = logs.LastOrDefault()
                .AssertIsNotNull("log not found");

            if (metadataCollection != null)
            {
                lastLog.Metadata.AssertCount(metadataCollection.Count, "Count of metadata is different.");

                foreach (KeyValuePair<string, string> metadata in metadataCollection)
                {
                    lastLog.Metadata.AssertAny(x => x.Key == metadata.Key && x.Value == metadata.Value,
                        $"metadata not found: {metadata.Key} - {metadata.Value}");
                }
            }

            return entity;
        }

        public static T AssertAuditForDeletion<T>(this T entity, ITrackerContext db, object entityId,
            string userName = null, params Expression<Func<T, object>>[] oldValueProperties)
        {
            IEnumerable<AuditLog> logs = db.GetLogs<T>(entityId)
                .Where(x => x.EventType == EventType.Deleted && x.UserName == userName)
                .AssertCountIsNotZero("log count is zero");

            AuditLog lastLog = logs.Last()
                .AssertIsNotNull("log not found");

            lastLog.LogDetails
                .AssertCountIsNotZero("no log details found")
                .AssertCount(oldValueProperties.Count());

            foreach (Expression<Func<T, object>> property in oldValueProperties)
            {
                KeyValuePair<string, string> keyValuePair = entity.GetKeyValuePair(property);
                lastLog.LogDetails.AssertAny(x => x.OriginalValue == keyValuePair.Value
                                                  && x.PropertyName == keyValuePair.Key);
            }

            return entity;
        }

        public static T AssertAuditForSoftDeletion<T>(this T entity, ITrackerContext db, object entityId,
            string userName = null, params AuditLogDetail[] logdetails)
        {
            return AssertChange(entity, db, entityId, userName, EventType.SoftDeleted, logdetails);
        }

        public static T AssertAuditForUndeletion<T>(this T entity, ITrackerContext db, object entityId,
            string userName = null, params AuditLogDetail[] logdetails)
        {
            return AssertChange(entity, db, entityId, userName, EventType.UnDeleted, logdetails);
        }

        private static T AssertChange<T>(T entity, ITrackerContext db, object entityId, string userName, EventType eventType,
            AuditLogDetail[] logdetails)
        {
            IQueryable<AuditLog> logs = db.GetLogs<T>(entityId)
                .Where(x => x.EventType == eventType && x.UserName == userName);

            logs.AssertCountIsNotZero(
                $"no logs found for {typeof (T).Name} with id {entityId} & username {userName ?? "null"}");

            AuditLog lastLog = logs.OrderByDescending(x => x.AuditLogId)
                .FirstOrDefault()
                .AssertIsNotNull();

            lastLog.LogDetails
                .AssertCountIsNotZero("no log details found")
                .AssertCount(logdetails.Count());

            foreach (AuditLogDetail auditLogDetail in logdetails)
            {
                lastLog.LogDetails.AssertAny(x => x.OriginalValue == auditLogDetail.OriginalValue
                                          && x.NewValue == auditLogDetail.NewValue
                                          && x.PropertyName == auditLogDetail.PropertyName,
                    $"cound not find log detail with original value: {auditLogDetail.OriginalValue}, " +
                    $"new value: {auditLogDetail.NewValue} " +
                    $"and propertyname: {auditLogDetail.PropertyName}");
            }

            return entity;
        }

        public static T AssertAuditForModification<T>(this T entity, ITrackerContext db, object entityId,
            string userName = null, params AuditLogDetail[] logdetails)
        {
            return AssertChange(entity, db, entityId, userName, EventType.Modified, logdetails);
        }

        public static T AssertNoLogs<T>(this T entity, ITrackerContext db, object entityId)
        {
            IQueryable<AuditLog> logs = db.GetLogs<T>(entityId);
            logs.AssertCount(0, "Logs found when logs were not expected");

            return entity;
        }

        public static T AssertNoLogs<T>(this T entity, ITrackerContext db, object entityId, EventType eventType)
        {
            IQueryable<AuditLog> logs = db.GetLogs<T>(entityId)
                .Where(x=>x.EventType == eventType);

            logs.AssertCount(0, "Logs found when logs were not expected");

            return entity;
        }
    }
}