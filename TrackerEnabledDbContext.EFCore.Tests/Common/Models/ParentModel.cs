using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackerEnabledDbContext.EFCore.Tests.Common.Models
{
    [TrackChanges]
    public class ParentModel
    {
        [Key]
        public int Id { get; set; }

        public virtual ICollection<ChildModel> Children { get; set; } = new List<ChildModel>();
    }
}