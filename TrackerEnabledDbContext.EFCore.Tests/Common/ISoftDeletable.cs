namespace TrackerEnabledDbContext.EFCore.Tests.Common
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }

        void Delete();
    }
}
