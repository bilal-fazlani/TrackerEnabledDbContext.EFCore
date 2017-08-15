﻿using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerEnabledDbContext.EFCore.Configuration;
using TrackerEnabledDbContext.EFCore.Models;
using TrackerEnabledDbContext.EFCore.Tests.Common;
using TrackerEnabledDbContext.EFCore.Tests.Common.Extensions;
using TrackerEnabledDbContext.EFCore.Tests.Common.Models;

namespace TrackerEnabledDbContext.EFCore.Tests
{
    [TestClass]
    public class SoftDeleteTests : PersistanceTests<TestTrackerContext>
    {
        [TestInitialize]
        public void InitializeSoftDeletionTests()
        {
            //setup soft deletable config
            GlobalTrackingConfig.SetSoftDeletableCriteria<ISoftDeletable>
                (entity => entity.IsDeleted);
        }

        [TestMethod]
        public void ShouldCreateSoftDeleteLog()
        {
            //create a softdeletable entity and soft delete it
            SoftDeletableModel deletable = new SoftDeletableModel();

            Db.SoftDeletableModels.Add(deletable);

            //save it to database
            Db.SaveChanges();

            deletable.AssertAuditForAddition(Db, deletable.Id,
                null, x=>x.Id);
            
            //soft delete entity
            deletable.Delete();

            //save changes
            Db.SaveChanges();

            //assert for soft deletion
            deletable.AssertAuditForSoftDeletion(Db, deletable.Id, null, new AuditLogDetail
            {
                NewValue = true.ToString(),
                OriginalValue = false.ToString(),
                PropertyName = nameof(deletable.IsDeleted)
            });
        }

        [TestMethod]
        public void ShouldCreateSoftDeleteLogForMultiplePropertiesChanged()
        { 
            //create a softdeletable entity and soft delete it
            SoftDeletableModel deletable = new SoftDeletableModel();

            Db.SoftDeletableModels.Add(deletable);

            //save it to database
            Db.SaveChanges();

            deletable.AssertAuditForAddition(Db, deletable.Id,
                null, x => x.Id);

            //soft delete entity
            deletable.Delete();
            deletable.Description = RandomText;

            //save changes
            Db.SaveChanges();

            //assert for soft deletion
            deletable.AssertAuditForSoftDeletion(Db, deletable.Id, null, new AuditLogDetail
            {
                NewValue = true.ToString(),
                OriginalValue = false.ToString(),
                PropertyName = nameof(deletable.IsDeleted)
            },
            new AuditLogDetail
            {
                NewValue = deletable.Description,
                OriginalValue = null,
                PropertyName = nameof(deletable.Description)
            });
        }

        [TestMethod]
        public void ShouldCreateUnDeletedLog()
        {
            SoftDeletableModel deletable = new SoftDeletableModel
            {
                Description = RandomText,
            };

            Db.Set<SoftDeletableModel>().Attach(deletable);
            Db.Entry(deletable).State = EntityState.Added;
            Db.SaveChanges();

            deletable.AssertAuditForAddition(Db, deletable.Id, null,
                x => x.Id, x => x.Description);

            deletable.Delete();
            Db.SaveChanges();

            deletable.AssertAuditForSoftDeletion(Db, deletable.Id, null,
                new AuditLogDetail
                {
                    PropertyName = nameof(deletable.IsDeleted),
                    OriginalValue = false.ToString(),
                    NewValue = true.ToString()
                });

            deletable.IsDeleted = false;
            Db.SaveChanges();

            deletable.AssertAuditForUndeletion(Db, deletable.Id, null,
                new AuditLogDetail
                {
                    PropertyName = nameof(deletable.IsDeleted),
                    OriginalValue = true.ToString(),
                    NewValue = false.ToString()
                });
        }

        [TestMethod]
        public async Task ShouldCreateUnDeletedLogForMultiplePropertiesChanged()
        {
            string oldDescription = RandomText;
            string newDescription = RandomText;

            SoftDeletableModel deletable = new SoftDeletableModel
            {
                Description = oldDescription
            };

            Db.Set<SoftDeletableModel>().Attach(deletable);
            Db.Entry(deletable).State = EntityState.Added;
            await Db.SaveChangesAsync();

            deletable.AssertAuditForAddition(Db, deletable.Id, null,
                x => x.Id, x => x.Description);

            deletable.Delete();
            await Db.SaveChangesAsync();

            deletable.AssertAuditForSoftDeletion(Db, deletable.Id, null,
                new AuditLogDetail
                {
                    PropertyName = nameof(deletable.IsDeleted),
                    OriginalValue = false.ToString(),
                    NewValue = true.ToString()
                });

            deletable.IsDeleted = false;
            deletable.Description = newDescription;
            await Db.SaveChangesAsync();

            deletable.AssertAuditForUndeletion(Db, deletable.Id, null,
                new AuditLogDetail
                {
                    PropertyName = nameof(deletable.IsDeleted),
                    OriginalValue = true.ToString(),
                    NewValue = false.ToString()
                },
                new AuditLogDetail
                {
                    PropertyName = nameof(deletable.Description),
                    OriginalValue = oldDescription,
                    NewValue = newDescription
                });
        }
    }
}
