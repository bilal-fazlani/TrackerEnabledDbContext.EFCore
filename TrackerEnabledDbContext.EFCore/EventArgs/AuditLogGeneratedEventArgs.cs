using System;
using System.Dynamic;
using TrackerEnabledDbContext.EFCore.Models;

namespace TrackerEnabledDbContext.EFCore.EventArgs
{
    public class AuditLogGeneratedEventArgs : System.EventArgs
    {
        public AuditLogGeneratedEventArgs(AuditLog log, object entity, ExpandoObject metadata)
        {
            Log = log;
            Entity = entity;
            Metadata = metadata;
        }

        public AuditLog Log { get; internal set; }

        public object Entity { get; internal set; }

        /// <summary>
        /// Skips saving of log to database.
        /// </summary>
        public bool SkipSavingLog { get; set; }

        public dynamic Metadata { get; internal set; }
    }
}
