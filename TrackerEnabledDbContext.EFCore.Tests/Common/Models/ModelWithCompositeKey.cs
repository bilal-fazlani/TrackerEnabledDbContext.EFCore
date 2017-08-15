using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackerEnabledDbContext.EFCore.Tests.Common.Models
{
    [TrackChanges]
    public class ModelWithCompositeKey
    {
        public string Key1 { get; set; }

        public string Key2 { get; set; }

        public string Description { get; set; }
    }
}