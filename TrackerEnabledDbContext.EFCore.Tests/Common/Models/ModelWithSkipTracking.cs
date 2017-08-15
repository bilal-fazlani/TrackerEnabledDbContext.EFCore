using System;
using System.ComponentModel.DataAnnotations;

namespace TrackerEnabledDbContext.EFCore.Tests.Common.Models
{
    [TrackChanges]
    public class ModelWithSkipTracking
    {
        public int Id { get; set; }
        public Guid TrackedProperty { get; set; }

        [SkipTracking]
        public string UnTrackedProperty { get; set; }
    }
}