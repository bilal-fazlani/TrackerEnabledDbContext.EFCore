using System;

namespace TrackerEnabledDbContext.EFCore.Tests.Common.Models
{
    public class POCO
    {
        public int Id { get; set; }

        public string Color { get; set; }

        public double Height { get; set; }

        public DateTime? StartTime { get; set; }
    }
}
