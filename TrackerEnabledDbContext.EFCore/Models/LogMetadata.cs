using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackerEnabledDbContext.EFCore.Interfaces;

namespace TrackerEnabledDbContext.EFCore.Models
{
    [Table("LogMetadata")]
    public class LogMetadata : IUnTrackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public virtual long AuditLogId { get; set; }

        [ForeignKey("AuditLogId")]
        public virtual AuditLog AuditLog { get; set; }
        
        public string Key { get; set; }

        public string Value { get; set; }
    }
}