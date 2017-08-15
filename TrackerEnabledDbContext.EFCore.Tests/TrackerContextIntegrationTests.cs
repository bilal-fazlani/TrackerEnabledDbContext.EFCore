using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerEnabledDbContext.EFCore.Auditors;
using TrackerEnabledDbContext.EFCore.Configuration;
using TrackerEnabledDbContext.EFCore.Models;
using TrackerEnabledDbContext.EFCore.Tests.Common;
using TrackerEnabledDbContext.EFCore.Tests.Common.Extensions;
using TrackerEnabledDbContext.EFCore.Tests.Common.Models;

namespace TrackerEnabledDbContext.EFCore.Tests
{
    [TestClass]
    public class TrackerContextIntegrationTests : PersistanceTests<TestTrackerContext>
    {
        [TestMethod]
        public void Can_track_addition_when_username_provided()
        {
            string randomText = RandomText;
            string userName = RandomText;

            NormalModel normalModel = ObjectFactory.Create<NormalModel>();
            normalModel.Description = randomText;
            Db.NormalModels.Add(normalModel);
            Db.SaveChanges(userName);

            normalModel.AssertAuditForAddition(Db, normalModel.Id, userName,
                x => x.Description,
                x => x.Id);
        }

        [TestMethod]
        public void Can_track_addition_when_usermane_not_provided()
        {
            string randomText = RandomText;

            NormalModel normalModel = ObjectFactory.Create<NormalModel>();
            normalModel.Description = randomText;
            Db.NormalModels.Add(normalModel);
            Db.SaveChanges();

            normalModel.AssertAuditForAddition(Db, normalModel.Id, null,
                x => x.Description,
                x => x.Id);
        }

        [TestMethod]
        public void Can_track_addition_when_state_changed_directly()
        {
            string randomText = RandomText;
            string userName = RandomText;

            NormalModel model = ObjectFactory.Create<NormalModel>();
            model.Description = randomText;
            Db.Entry(model).State = EntityState.Added;
            Db.SaveChanges(userName);

            model.AssertAuditForAddition(Db, model.Id, userName,
                x => x.Description,
                x => x.Id);
        }

        [TestMethod]
        public void Can_track_deletion()
        {
            string description = RandomText;
            string userName = RandomText;

            //add
            NormalModel normalModel = ObjectFactory.Create<NormalModel>();
            normalModel.Description = description;
            Db.NormalModels.Add(normalModel);
            Db.SaveChanges(userName);


            //remove
            Db.NormalModels.Remove(normalModel);
            Db.SaveChanges(userName);

            normalModel.AssertAuditForDeletion(Db, normalModel.Id, userName,
                x => x.Description,
                x => x.Id);
        }

        [TestMethod]
        public void Can_track_deletion_when_state_changed()
        {
            string description = RandomText;

            //add
            NormalModel normalModel = ObjectFactory.Create<NormalModel>();
            normalModel.Description = description;
            Db.NormalModels.Add(normalModel);
            Db.SaveChanges();


            //remove
            Db.Entry(normalModel).State = EntityState.Deleted;
            Db.SaveChanges();


            //assert
            normalModel.AssertAuditForDeletion(Db, normalModel.Id, null,
                x => x.Description,
                x => x.Id);
        }

        [TestMethod]
        public void Can_track_local_propery_change()
        {
            //add enity
            string oldDescription = RandomText;
            string newDescription = RandomText;
            NormalModel entity = new NormalModel {Description = oldDescription};
            Db.Entry(entity).State = EntityState.Added;
            Db.SaveChanges();

            //modify entity
            entity.Description = newDescription;
            Db.SaveChanges();

            AuditLogDetail[] expectedLog = new List<AuditLogDetail>
            {
                new AuditLogDetail
                {
                    NewValue = newDescription,
                    OriginalValue = oldDescription,
                    PropertyName = "Description"
                }
            }.ToArray();


            //assert
            entity.AssertAuditForModification(Db, entity.Id, null, expectedLog);
        }

        [TestMethod]
        public void Can_track_navigational_property_change()
        {
            //add enitties
            ParentModel parent1 = new ParentModel();
            ChildModel child = new ChildModel {Parent = parent1};
            Db.Children.Add(child);
            Db.SaveChanges();

            child.Id.AssertIsNotZeroOrNegative(); //assert child saved
            parent1.Id.AssertIsNotZeroOrNegative(); //assert parent1 saved

            //save parent 2
            ParentModel parent2 = new ParentModel();
            Db.ParentModels.Add(parent2);
            Db.SaveChanges();

            parent2.Id.AssertIsNotZeroOrNegative(); //assert parent2 saved

            //change parent
            child.Parent = parent2;
            Db.SaveChanges();

            AuditLogDetail[] expectedLog = new List<AuditLogDetail>
            {
                new AuditLogDetail
                {
                    NewValue = parent2.Id.ToString(),
                    OriginalValue = parent1.Id.ToString(),
                    PropertyName = "ParentId"
                }
            }.ToArray();

            //assert change
            child.AssertAuditForModification(Db, child.Id, null, expectedLog);
        }

        [TestMethod]
        public async Task Can_skip_tracking_of_property()
        {
            string username = RandomText;

            //add enitties
            ModelWithSkipTracking entity = new ModelWithSkipTracking {TrackedProperty = Guid.NewGuid(), UnTrackedProperty = RandomText};
            Db.ModelsWithSkipTracking.Add(entity);
            await Db.SaveChangesAsync(username, CancellationToken.None);

            //assert enity added
            entity.Id.AssertIsNotZeroOrNegative();

            //assert addtion
            entity.AssertAuditForAddition(Db, entity.Id, username,
                x => x.TrackedProperty,
                x => x.Id);
        }

        [TestMethod]
        public void Can_track_composite_keys()
        {
            string key1 = RandomText;
            string key2 = RandomText;
            string userName = RandomText;
            string descr = RandomText;


            ModelWithCompositeKey entity = ObjectFactory.Create<ModelWithCompositeKey>();
            entity.Description = descr;
            entity.Key1 = key1;
            entity.Key2 = key2;

            Db.ModelsWithCompositeKey.Add(entity);
            Db.SaveChanges(userName);

            string expectedKey = $"[{key1},{key2}]";

            entity.AssertAuditForAddition(Db, expectedKey, userName,
                x => x.Description,
                x => x.Key1,
                x=>x.Key2);
        }

        [TestMethod]
        public async Task Can_save_changes_with_userID()
        {
            int userId = RandomNumber;

            //add enity
            string oldDescription = RandomText;
            string newDescription = RandomText;
            NormalModel entity = new NormalModel {Description = oldDescription};
            Db.Entry(entity).State = EntityState.Added;
            Db.SaveChanges();

            //modify entity
            entity.Description = newDescription;
            await Db.SaveChangesAsync(userId);

            AuditLogDetail[] expectedLog = new List<AuditLogDetail>
            {
                new AuditLogDetail
                {
                    NewValue = newDescription,
                    OriginalValue = oldDescription,
                    PropertyName = "Description"
                }
            }.ToArray();


            //assert
            entity.AssertAuditForModification(Db, entity.Id, userId.ToString(), expectedLog);
        }

        [TestMethod, Ignore("Testing strategy TBD")]
        public void Can_Create_AuditLogDetail_ForAddedEntity_WithoutQueryingDatabase()
        {
            NormalModel model = ObjectFactory.Create<NormalModel>();
            Db.NormalModels.Add(model);
            Db.ChangeTracker.DetectChanges();
            EntityEntry entry = Db.ChangeTracker.Entries().First();
            AdditionLogDetailsAuditor auditor = new AdditionLogDetailsAuditor(entry, null);

//            Db.Database.Log = sql => Assert.Fail("Expected no database queries but the following query was executed: {0}", sql);
//            var auditLogDetails = auditor.CreateLogDetails().ToList();
//            Db.Database.Log = null;
        }

        [TestMethod, Ignore("Testing strategy TBD")]
        public void Can_Create_AuditLogDetail_ForModifiedEntity_WithoutQueryingDatabase()
        {
            NormalModel model = ObjectFactory.Create<NormalModel>();
            Db.NormalModels.Add(model);
            Db.SaveChanges();
            model.Description += RandomText;
            Db.ChangeTracker.DetectChanges();
            EntityEntry entry = Db.ChangeTracker.Entries().First();
            ChangeLogDetailsAuditor auditor = new ChangeLogDetailsAuditor(entry, null);
//
//            Db.Database.Log = sql => Assert.Fail("Expected no database queries but the following query was executed: {0}", sql);
//            var auditLogDetails = auditor.CreateLogDetails().ToList();
//            Db.Database.Log = null;
        }

        [TestMethod, Ignore("Testing strategy TBD")]
        public void Can_Create_AuditLogDetail_ForDeletedEntity_WithoutQueryingDatabase()
        {
            NormalModel model = ObjectFactory.Create<NormalModel>();
            Db.NormalModels.Add(model);
            Db.SaveChanges();
            Db.NormalModels.Remove(model);
            Db.ChangeTracker.DetectChanges();
            EntityEntry entry = Db.ChangeTracker.Entries().First();
            ChangeLogDetailsAuditor auditor = new ChangeLogDetailsAuditor(entry, null);

//            Db.Database.Log = sql => Assert.Fail("Expected no database queries but the following query was executed: {0}", sql);
//            var auditLogDetails = auditor.CreateLogDetails().ToList();
//            Db.Database.Log = null;
        }

        [TestMethod]
        public void Should_Not_Log_When_Value_Not_changed()
        {
            //arrange
            EntityTracker.TrackAllProperties<TrackedModelWithMultipleProperties>();

            string oldDescription = RandomText;

            TrackedModelWithMultipleProperties entity = new TrackedModelWithMultipleProperties()
            {
                Description = oldDescription,
                StartDate = RandomDate,
            };
            Db.TrackedModelsWithMultipleProperties.Add(entity);
            Db.SaveChanges();

            entity.AssertAuditForAddition(Db, entity.Id,
                null,
                x => x.Id,
                x => x.Description,
                x => x.StartDate);

            //make change to state
            Db.Entry(entity).State = EntityState.Modified;
            Db.SaveChanges();

            //make sure there are no unnecessaary logs
            entity.AssertNoLogs(Db, entity.Id, EventType.Modified);
        }

        [TestMethod]
        public void Can_recognise_context_tracking_indicator_when_disabled()
        {

            NormalModel model = ObjectFactory.Create<NormalModel>();
            Db.NormalModels.Add(model);

            Db.TrackingEnabled = false;
            Db.SaveChanges();

            model.AssertNoLogs(Db, model.Id);
        }
    }
}