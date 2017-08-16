using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerEnabledDbContext.EFCore.Tests.Common;
using TrackerEnabledDbContext.EFCore.Tests.Common.Extensions;
using TrackerEnabledDbContext.EFCore.Tests.Common.Models;

namespace TrackerEnabledDbContext.EFCore.Tests
{
    [TestClass]
    public class EFFunctionalTests : PersistanceTests<TestTrackerContext>
    {
        [TestMethod]
        public void Can_save_model()
        {
            NormalModel model = ObjectFactory.Create<NormalModel>();
            Db.NormalModels.Add(model);
            Db.SaveChanges();
            model.Id.AssertIsNotZeroOrNegative();
        }

        [TestMethod]
        public void Can_save_when_entity_state_changed()
        {
            NormalModel model = ObjectFactory.Create<NormalModel>();
            Db.Entry(model).State = EntityState.Added;
            Db.SaveChanges();
            model.Id.AssertIsNotZeroOrNegative();
        }

        [TestMethod]
        public async Task Can_save_async()
        {
            NormalModel model = ObjectFactory.Create<NormalModel>();
            Db.Entry(model).State = EntityState.Added;
            await Db.SaveChangesAsync();
            model.Id.AssertIsNotZeroOrNegative();
        }

        [TestMethod]
        public void Can_save_child_to_parent()
        {
            ChildModel child = new ChildModel();
            ParentModel parent = new ParentModel();
            child.Parent = parent;

            Db.Children.Add(child);

            Db.SaveChanges();

            child.Id.AssertIsNotZeroOrNegative();
            parent.Id.AssertIsNotZeroOrNegative();
        }

        [TestMethod, Ignore("this won't work in EF Core - By design https://github.com/aspnet/EntityFramework/issues/9413")]
        public void Can_save_child_to_parent_when_entity_state_changed()
        {
            ChildModel child = new ChildModel();
            ParentModel parent = new ParentModel();
            child.Parent = parent;

            Db.Entry(child).State = EntityState.Added;

            Db.SaveChanges();

            child.Id.AssertIsNotZeroOrNegative();
            parent.Id.AssertIsNotZeroOrNegative();
        }

        [TestMethod]
        public void Can_save_child_to_parent_when_added_directly_to_context()
        {
            ChildModel child = new ChildModel();
            ParentModel parent = new ParentModel();
            child.Parent = parent;

            Db.Add(child);

            Db.SaveChanges();

            child.Id.AssertIsNotZeroOrNegative();
            parent.Id.AssertIsNotZeroOrNegative();
        }
    }
}
