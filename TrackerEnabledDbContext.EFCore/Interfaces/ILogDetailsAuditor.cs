using System.Collections.Generic;
using TrackerEnabledDbContext.EFCore.Models;

namespace TrackerEnabledDbContext.EFCore.Interfaces
{
    public interface ILogDetailsAuditor
    {
        IEnumerable<AuditLogDetail> CreateLogDetails();
    }
}