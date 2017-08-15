namespace TrackerEnabledDbContext.EFCore.Tools
{
    public class MigrationJobStatus
    {
        public string EntityFullName { get; set; }
        public int Percent { get; set; }
    }
}
