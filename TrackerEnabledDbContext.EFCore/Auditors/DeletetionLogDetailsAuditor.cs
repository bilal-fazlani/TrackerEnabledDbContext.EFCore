using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TrackerEnabledDbContext.EFCore.Auditors.Comparators;
using TrackerEnabledDbContext.EFCore.Configuration;
using TrackerEnabledDbContext.EFCore.Extensions;
using TrackerEnabledDbContext.EFCore.Models;

namespace TrackerEnabledDbContext.EFCore.Auditors
{
    public class DeletetionLogDetailsAuditor: ChangeLogDetailsAuditor
    {
        public DeletetionLogDetailsAuditor(EntityEntry dbEntry, AuditLog log) : base(dbEntry, log)
        {
        }

        protected override bool IsValueChanged(string propertyName)
        {
            if (GlobalTrackingConfig.TrackEmptyPropertiesOnAdditionAndDeletion)
                return true;

            Type propertyType = DbEntry.Entity.GetType().GetProperty(propertyName).PropertyType;
            object defaultValue = propertyType.DefaultValue();
            object orginalvalue = OriginalValue(propertyName);

            Comparator comparator = ComparatorFactory.GetComparator(propertyType);

            return !comparator.AreEqual(defaultValue, orginalvalue);
        }

        protected override object CurrentValue(string propertyName)
        {
            return null;
        }
    }
}
