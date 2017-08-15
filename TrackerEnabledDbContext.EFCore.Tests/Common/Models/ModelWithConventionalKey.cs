using System.ComponentModel.DataAnnotations;

namespace TrackerEnabledDbContext.EFCore.Tests.Common.Models
{
    [TrackChanges]
    public class ModelWithConventionalKey
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}