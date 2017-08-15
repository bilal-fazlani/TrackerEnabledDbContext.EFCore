using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TrackerEnabledDbContext.EFCore.Auditors.Comparators;
using TrackerEnabledDbContext.EFCore.Configuration;
using TrackerEnabledDbContext.EFCore.Extensions;
using TrackerEnabledDbContext.EFCore.Interfaces;
using TrackerEnabledDbContext.EFCore.Models;

namespace TrackerEnabledDbContext.EFCore.Auditors
{
    public class ChangeLogDetailsAuditor : ILogDetailsAuditor
    {
        protected readonly EntityEntry DbEntry;
        private readonly AuditLog _log;

        public ChangeLogDetailsAuditor(EntityEntry dbEntry, AuditLog log)
        {
            DbEntry = dbEntry;
            _log = log;
        }

        public IEnumerable<AuditLogDetail> CreateLogDetails()
        {
            Type entityType = DbEntry.Entity.GetType().GetEntityType();

            foreach (string propertyName in PropertyNamesOfEntity())
            {
                if (PropertyTrackingConfiguration.IsTrackingEnabled(
                    new PropertyConfiguerationKey(propertyName, entityType.FullName), entityType)
                    && IsValueChanged(propertyName))
                {
                    if (IsComplexType(propertyName))
                    {
                        foreach (AuditLogDetail auditLogDetail in CreateComplexTypeLogDetails(propertyName))
                        {
                            yield return auditLogDetail;
                        }
                    }
                    else
                    {
                        yield return new AuditLogDetail
                        {
                            PropertyName = propertyName,
                            OriginalValue = OriginalValue(propertyName)?.ToString(),
                            NewValue = CurrentValue(propertyName)?.ToString(),
                            Log = _log
                        };
                    }
                }
            }
        }

        protected internal virtual EntityState StateOfEntity()
        {
            return DbEntry.State;
        }

        private IEnumerable<string> PropertyNamesOfEntity()
        {
            PropertyValues propertyValues = (StateOfEntity() == EntityState.Added)
                ? DbEntry.CurrentValues
                : DbEntry.OriginalValues;
            return propertyValues.Properties.Select(x=>x.Name);
        }

        protected virtual bool IsValueChanged(string propertyName)
        {
            PropertyEntry prop = DbEntry.Property(propertyName);
            Type propertyType = DbEntry.Entity.GetType().GetProperty(propertyName).PropertyType;

            object originalValue = OriginalValue(propertyName);

            Comparator comparator = ComparatorFactory.GetComparator(propertyType);

            bool changed = (StateOfEntity() == EntityState.Modified
                && prop.IsModified && !comparator.AreEqual(CurrentValue(propertyName), originalValue));
            return changed;
        }

        protected virtual object OriginalValue(string propertyName)
        {
            object originalValue = null;

            if (GlobalTrackingConfig.DisconnectedContext)
            {
                originalValue = DbEntry.GetDatabaseValues().GetValue<object>(propertyName);
            }
            else
            {
                originalValue = DbEntry.Property(propertyName).OriginalValue;
            }

            return originalValue;
        }

        protected virtual object CurrentValue(string propertyName)
        {
            object value = DbEntry.Property(propertyName).CurrentValue;
            return value;
        }

        private bool IsComplexType(string propertyName)
        {
            NavigationEntry entryMember = DbEntry.Member(propertyName) as NavigationEntry;

            return entryMember != null;
        }

        private IEnumerable<AuditLogDetail> CreateComplexTypeLogDetails(string propertyName)
        {
            NavigationEntry entryMember = DbEntry.Member(propertyName) as NavigationEntry;

            if (entryMember != null)
            {
                Type complexTypeObj = entryMember.CurrentValue.GetType();

                foreach (PropertyInfo pi in complexTypeObj.GetProperties())
                {
                    string complexTypePropertyName = $"{propertyName}_{pi.Name}";
                    object complexTypeOrigValue = OriginalValue(propertyName);
                    object complexTypeNewValue = CurrentValue(propertyName);

                    object origValue = complexTypeOrigValue == null ? null : pi.GetValue(complexTypeOrigValue);
                    object newValue = complexTypeNewValue == null ? null : pi.GetValue(complexTypeNewValue);

                    Comparator comparator = ComparatorFactory.GetComparator(complexTypeObj);

                    if (!comparator.AreEqual(newValue, origValue))
                    {
                        yield return new AuditLogDetail
                        {
                            PropertyName = complexTypePropertyName,
                            OriginalValue = origValue?.ToString(),
                            NewValue = newValue?.ToString(),
                            Log = _log
                        };
                    }
                }
            }
        }

    }
}