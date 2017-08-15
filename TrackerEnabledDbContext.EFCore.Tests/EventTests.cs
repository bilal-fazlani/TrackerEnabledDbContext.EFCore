using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerEnabledDbContext.EFCore.Configuration;
using TrackerEnabledDbContext.EFCore.Models;
using TrackerEnabledDbContext.EFCore.Tests.Common;
using TrackerEnabledDbContext.EFCore.Tests.Common.Extensions;
using TrackerEnabledDbContext.EFCore.Tests.Common.Models;

namespace TrackerEnabledDbContext.EFCore.Tests
{
    [TestClass]
    public class EventTests : PersistanceTests<TestTrackerContext>
    {
        [TestMethod]
        public void CanRaiseAddEvent()
        {
            using (TestTrackerContext context = GetNewContextInstance())
            {
                EntityTracker.TrackAllProperties<TrackedModelWithMultipleProperties>();

                bool eventRaised = false;

                context.OnAuditLogGenerated += (sender, args) =>
                {
                    TrackedModelWithMultipleProperties eventEntity = args.Entity as TrackedModelWithMultipleProperties;

                    if (args.Log.EventType == EventType.Added &&
                        args.Log.TypeFullName == typeof (TrackedModelWithMultipleProperties).FullName &&
                        eventEntity != null)
                    {
                        eventRaised = true;
                    }
                };

                TrackedModelWithMultipleProperties entity = ObjectFactory.Create<TrackedModelWithMultipleProperties>(false);

                entity.Description = RandomText;

                context.TrackedModelsWithMultipleProperties.Add(entity);

                context.SaveChanges();

                //assert
                Assert.IsTrue(eventRaised);

                //make sure log is saved in database
                entity.AssertAuditForAddition(context, entity.Id, null,
                    x => x.Id,
                    x => x.Description);
            }
        }

        [TestMethod]
        public void CanRaiseModifyEvent()
        {
            //TODO: modify test tracker context and identity test tracker context so that on disposal they revert the changes
            using (TestTrackerContext context = GetNewContextInstance())
            {
                EntityTracker.TrackAllProperties<TrackedModelWithMultipleProperties>();

                bool modifyEventRaised = false;

                context.OnAuditLogGenerated += (sender, args) =>
                {
                    TrackedModelWithMultipleProperties eventEntity = args.Entity as TrackedModelWithMultipleProperties;

                    if (args.Log.EventType == EventType.Modified &&
                        args.Log.TypeFullName == typeof(TrackedModelWithMultipleProperties).FullName &&
                        eventEntity != null)
                    {
                        modifyEventRaised = true;
                    }
                };

                TrackedModelWithMultipleProperties existingEntity = ObjectFactory
                    .Create<TrackedModelWithMultipleProperties>(save: true, testDbContext:context);

                string originalValue = existingEntity.Name;
                existingEntity.Name = RandomText;

                context.SaveChanges();

                //assert
                Assert.IsTrue(modifyEventRaised);

                existingEntity.AssertAuditForModification(context, existingEntity.Id, null,
                    new AuditLogDetail
                    {
                        PropertyName = nameof(existingEntity.Name),
                        OriginalValue = originalValue,
                        NewValue = existingEntity.Name
                    });
            }
        }

        [TestMethod]
        public void CanRaiseDeleteEvent()
        {
            using (TestTrackerContext context = GetNewContextInstance())
            {
                EntityTracker.TrackAllProperties<NormalModel>();

                bool eventRaised = false;

                context.OnAuditLogGenerated += (sender, args) =>
                {
                    NormalModel eventEntity = args.Entity as NormalModel;

                    if (args.Log.EventType == EventType.Deleted &&
                        args.Log.TypeFullName == typeof(NormalModel).FullName &&
                        eventEntity != null)
                    {
                        eventRaised = true;
                    }
                };

                NormalModel existingEntity = ObjectFactory
                    .Create<NormalModel>(save: true, testDbContext: context);

                context.NormalModels.Remove(existingEntity);
                context.SaveChanges();

                //assert
                Assert.IsTrue(eventRaised);

                existingEntity.AssertAuditForDeletion(context, existingEntity.Id, null,
                    x => x.Description,
                    x => x.Id);
            }
        }

        [TestMethod]
        public void CanRaiseSoftDeleteEvent()
        {
            GlobalTrackingConfig.SetSoftDeletableCriteria<ISoftDeletable>
                (x=>x.IsDeleted);

            using (TestTrackerContext context = GetNewContextInstance())
            {
                EntityTracker.TrackAllProperties<SoftDeletableModel>();

                bool eventRaised = false;

                context.OnAuditLogGenerated += (sender, args) =>
                {
                    SoftDeletableModel eventEntity = args.Entity as SoftDeletableModel;

                    if (args.Log.EventType == EventType.SoftDeleted &&
                        args.Log.TypeFullName == typeof(SoftDeletableModel).FullName &&
                        eventEntity != null)
                    {
                        eventRaised = true;
                    }
                };

                SoftDeletableModel existingEntity = ObjectFactory
                    .Create<SoftDeletableModel>(save: true, testDbContext: context);

                existingEntity.Delete();

                context.SaveChanges();

                //assert
                Assert.IsTrue(eventRaised);

                existingEntity.AssertAuditForSoftDeletion(context, existingEntity.Id, null,
                    new AuditLogDetail
                    {
                        PropertyName = nameof(existingEntity.IsDeleted),
                        OriginalValue = false.ToString(),
                        NewValue = true.ToString()
                    });
            }
        }

        [TestMethod]
        public void CanRaiseUnDeleteEvent()
        {
            GlobalTrackingConfig.SetSoftDeletableCriteria<ISoftDeletable>
                (x => x.IsDeleted);

            using (TestTrackerContext context = GetNewContextInstance())
            {
                EntityTracker.TrackAllProperties<SoftDeletableModel>();

                bool eventRaised = false;

                context.OnAuditLogGenerated += (sender, args) =>
                {
                    SoftDeletableModel eventEntity = args.Entity as SoftDeletableModel;

                    if (args.Log.EventType == EventType.UnDeleted &&
                        args.Log.TypeFullName == typeof(SoftDeletableModel).FullName &&
                        eventEntity != null)
                    {
                        eventRaised = true;
                    }
                };

                SoftDeletableModel existingEntity = ObjectFactory
                    .Create<SoftDeletableModel>(save: true, testDbContext: context);
                
                existingEntity.Delete();

                context.SaveChanges();

                //now undelete
                existingEntity.IsDeleted = false;
                context.SaveChanges();

                //assert
                Assert.IsTrue(eventRaised);

                existingEntity.AssertAuditForUndeletion(context, existingEntity.Id, null,
                    new AuditLogDetail
                    {
                        PropertyName = nameof(existingEntity.IsDeleted),
                        OriginalValue = true.ToString(),
                        NewValue = false.ToString()
                    });
            }
        }

        [TestMethod]
        public void CanSkipTrackingUsingEvent()
        {
            using (TestTrackerContext context = GetNewContextInstance())
            {
                EntityTracker.TrackAllProperties<TrackedModelWithMultipleProperties>();

                bool eventRaised = false;

                context.OnAuditLogGenerated += (sender, args) =>
                {
                    TrackedModelWithMultipleProperties eventEntity = args.Entity as TrackedModelWithMultipleProperties;

                    if (args.Log.EventType == EventType.Added &&
                        args.Log.TypeFullName == typeof(TrackedModelWithMultipleProperties).FullName &&
                        eventEntity != null)
                    {
                        eventRaised = true;
                        args.SkipSavingLog = true;
                    }
                };

                TrackedModelWithMultipleProperties entity = ObjectFactory.Create<TrackedModelWithMultipleProperties>(save:true, testDbContext:context);

                //assert
                Assert.IsTrue(eventRaised);

                //make sure log is saved in database
                entity.AssertNoLogs(context, entity.Id, EventType.Added);
            }
        }

        [TestMethod]
        public void CanChangeEntityInEvent()
        {
            using (TestTrackerContext context = GetNewContextInstance())
            {
                EntityTracker.TrackAllProperties<TrackedModelWithMultipleProperties>();

                bool eventRaised = false;

                string modifiedValue = RandomText;

                context.OnAuditLogGenerated += (sender, args) =>
                {
                    TrackedModelWithMultipleProperties eventEntity = args.Entity as TrackedModelWithMultipleProperties;

                    if (args.Log.EventType == EventType.Modified &&
                        args.Log.TypeFullName == typeof(TrackedModelWithMultipleProperties).FullName &&
                        eventEntity != null)
                    {
                        eventEntity.Name = modifiedValue;
                        eventRaised = true;
                    }
                };

                TrackedModelWithMultipleProperties existingEntity = ObjectFactory
                    .Create<TrackedModelWithMultipleProperties>(save: true, testDbContext: context);

                string originalValue = existingEntity.Name;
                string newValue = RandomText;
                existingEntity.Name = newValue;

                context.SaveChanges();

                //assert
                Assert.IsTrue(eventRaised);

                existingEntity.AssertAuditForModification(context, existingEntity.Id, null,
                    new AuditLogDetail
                    {
                        PropertyName = nameof(existingEntity.Name),
                        OriginalValue = originalValue,
                        NewValue = newValue
                    });

                context.Entry(existingEntity).Reload();
                Assert.AreEqual(existingEntity.Name, modifiedValue);
            }
        }

        private TestTrackerContext GetNewContextInstance()
        {
            return new TestTrackerContext();
        }
    }
}
