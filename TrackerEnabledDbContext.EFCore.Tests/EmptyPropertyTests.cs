using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerEnabledDbContext.EFCore.Configuration;
using TrackerEnabledDbContext.EFCore.Tests.Common;
using TrackerEnabledDbContext.EFCore.Tests.Common.Extensions;
using TrackerEnabledDbContext.EFCore.Tests.Common.Models;

namespace TrackerEnabledDbContext.EFCore.Tests
{
    [TestClass]
    public class EmptyPropertyTests : PersistanceTests<TestTrackerContext>
    {
        [TestMethod]
        public void Shoud_Not_Log_EmptyProperties_OnAddition()
        {
            //arrange
            EntityTracker.TrackAllProperties<TrackedModelWithMultipleProperties>();
            TrackedModelWithMultipleProperties entity = new TrackedModelWithMultipleProperties();

            Db.TrackedModelsWithMultipleProperties.Add(entity);

            //act
            Db.SaveChanges();

            //assert
            entity.AssertAuditForAddition(Db, entity.Id, null,
                x => x.Id);
        }

        [TestMethod]
        public void Shoud_Not_Log_EmptyProperties_On_Deletions()
        {
            //arrange
            EntityTracker.TrackAllProperties<TrackedModelWithMultipleProperties>();
            TrackedModelWithMultipleProperties entity = new TrackedModelWithMultipleProperties();
            Db.TrackedModelsWithMultipleProperties.Add(entity);
            Db.SaveChanges();

            //act (delete)
            Db.TrackedModelsWithMultipleProperties.Remove(entity);
            Db.SaveChanges();

            //assert
            entity.AssertAuditForDeletion(Db, entity.Id, null,
                x => x.Id);
        }

        [TestMethod]
        public void Should_Log_EmptyProperties_When_Configured_WhileAdding()
        {
            //arrange
            EntityTracker.TrackAllProperties<TrackedModelWithMultipleProperties>();
            GlobalTrackingConfig.TrackEmptyPropertiesOnAdditionAndDeletion = true;

            TrackedModelWithMultipleProperties entity = new TrackedModelWithMultipleProperties();
            Db.TrackedModelsWithMultipleProperties.Add(entity);

            //act
            Db.SaveChanges();

            //assert
            entity.AssertAuditForAddition(Db, entity.Id, null,
                x => x.Id,
                x => x.Description,
                x => x.Category,
                x => x.IsSpecial,
                x => x.Name,
                x => x.StartDate,
                x => x.Value);
        }

        [TestMethod]
        public void Should_Log_EmptyProperties_When_Configured_WhileDeleting()
        {
            //arrange
            EntityTracker.TrackAllProperties<TrackedModelWithMultipleProperties>();
            GlobalTrackingConfig.TrackEmptyPropertiesOnAdditionAndDeletion = true;

            TrackedModelWithMultipleProperties entity = new TrackedModelWithMultipleProperties();
            Db.TrackedModelsWithMultipleProperties.Add(entity);
            Db.SaveChanges();


            //act
            Db.TrackedModelsWithMultipleProperties.Remove(entity);
            Db.SaveChanges();

            //assert
            entity.AssertAuditForDeletion(Db, entity.Id, null,
                x => x.Id,
                x => x.Description,
                x => x.Category,
                x => x.IsSpecial,
                x => x.Name,
                x => x.StartDate,
                x => x.Value);
        }
    }
}
