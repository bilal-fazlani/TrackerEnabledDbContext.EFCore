using System.ComponentModel.DataAnnotations.Schema;

namespace TrackerEnabledDbContext.EFCore.Tests.Common.Models
{
    [Table("ModelWithCustomTableAndColumnNames")]
    public class TrackedModelWithCustomTableAndColumnNames
    {
        public long Id { get; set; }

        [Column("MagnitudeOfForce")]
        public int Magnitude { get; set; }

        public string Direction { get; set; }

        public string Subject { get; set; }
    }
}
