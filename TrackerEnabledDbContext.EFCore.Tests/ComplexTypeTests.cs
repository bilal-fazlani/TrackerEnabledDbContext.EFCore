using System.Collections.Generic;
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
    public class ComplexTypeTests : PersistanceTests<TestTrackerContext>
    {
        [TestMethod]
        public void Can_track_complex_type_property_change()
        {
            //add enity
            string oldDescription = RandomText;
            string newDescription = RandomText;

            //only set one of the properties on the complex type
            ComplexType complexType = new ComplexType { Property1 = oldDescription };
            ModelWithComplexType entity = new ModelWithComplexType { ComplexType = complexType };

            Db.ModelsWithComplexType.Add(entity);

            Db.SaveChanges();

            //modify entity
            entity.ComplexType.Property1 = newDescription;
            Db.SaveChanges();

            AuditLogDetail[] expectedLog = new List<AuditLogDetail>
            {
                new AuditLogDetail
                {
                    NewValue = newDescription,
                    OriginalValue = oldDescription,
                    PropertyName = "ComplexType_Property1"
                }
            }.ToArray();

            //assert
            entity.AssertAuditForModification(Db, entity.Id, null, expectedLog);
        }

        [TestMethod]
        public void Can_track_complex_type_property_change_With_disconnected_context()
        {
            GlobalTrackingConfig.DisconnectedContext = true;

            //add enity
            string oldDescription = RandomText;
            string newDescription = RandomText;

            //only set one of the properties on the complex type
            ComplexType complexType = new ComplexType { Property1 = oldDescription };
            ModelWithComplexType entity = new ModelWithComplexType { ComplexType = complexType };

            Db.Entry(entity).State = EntityState.Added;

            Db.SaveChanges();

            //modify entity
            entity.ComplexType.Property1 = newDescription;
            Db.SaveChanges();

            AuditLogDetail[] expectedLog = new List<AuditLogDetail>
            {
                new AuditLogDetail
                {
                    NewValue = newDescription,
                    OriginalValue = oldDescription,
                    PropertyName = "ComplexType_Property1"
                }
            }.ToArray();


            //assert
            entity.AssertAuditForModification(Db, entity.Id, null, expectedLog);
        }
    }
}
