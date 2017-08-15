using System.Collections.Generic;
using TrackerEnabledDbContext.EFCore.Models;

namespace TrackerEnabledDbContext.EFCore.Tests.Common
{
    public class LogDetailsEqualityComparer : IEqualityComparer<AuditLogDetail>
    {
        public bool Equals(AuditLogDetail x, AuditLogDetail y)
        {
            return (x.PropertyName == y.PropertyName &&
                    x.OriginalValue == y.OriginalValue &&
                    x.NewValue == y.NewValue);
        }

        public int GetHashCode(AuditLogDetail obj)
        {
            return obj.NewValue.GetHashCode() + obj.OriginalValue.GetHashCode() + obj.PropertyName.GetHashCode();
        }
    }
}