using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerEnabledDbContext.EFCore.Models;
using TrackerEnabledDbContext.EFCore.Tests.Common;
using TrackerEnabledDbContext.EFCore.Tests.Common.Extensions;
using TrackerEnabledDbContext.EFCore.Tests.Common.Models;

namespace TrackerEnabledDbContext.EFCore.Tests
{
    [TestClass]
    public class LogRetrievalTests : PersistanceTests<TestTrackerContext>
    {
        public LogRetrievalTests()
        {
            this.RollBack = false;
        }

        [TestMethod]
        public async Task Can_get_logs_by_table_name()
        {
            string descr = RandomText;
            NormalModel model = ObjectFactory.Create<NormalModel>();
            model.Description = descr;

            Db.NormalModels.Add(model);
            await Db.SaveChangesAsync(CancellationToken.None);
            model.Id.AssertIsNotZeroOrNegative();

            IEnumerable<AuditLog> logs = Db.GetLogs("TrackerEnabledDbContext.EFCore.Tests.Common.Models.NormalModel", model.Id)
                .AssertCountIsNotZero("logs not found");

            AuditLog lastLog = logs.LastOrDefault().AssertIsNotNull("last log is null");

            lastLog.LogDetails
                .AssertIsNotNull("log details is null")
                .AssertCountIsNotZero("no log details found");
        }

        [TestMethod]
        public async Task Can_get_logs_by_entity_type()
        {
            string descr = RandomText;
            NormalModel model = ObjectFactory.Create<NormalModel>();
            model.Description = descr;

            Db.NormalModels.Add(model);
            await Db.SaveChangesAsync(CancellationToken.None);
            model.Id.AssertIsNotZeroOrNegative();

            IEnumerable<AuditLog> logs = Db.GetLogs<NormalModel>(model.Id)
                .AssertCountIsNotZero("logs not found");

            AuditLog lastLog = logs.LastOrDefault().AssertIsNotNull("last log is null");

            IEnumerable<AuditLogDetail> details = lastLog.LogDetails
                .AssertIsNotNull("log details is null")
                .AssertCountIsNotZero("no log details found");
        }

        [TestMethod]
        public async Task Can_get_all_logs()
        {
            string descr = RandomText;
            NormalModel model = ObjectFactory.Create<NormalModel>();
            model.Description = descr;

            Db.NormalModels.Add(model);
            await Db.SaveChangesAsync(RandomText);
            model.Id.AssertIsNotZeroOrNegative();

            IEnumerable<AuditLog> logs = Db.GetLogs("TrackerEnabledDbContext.EFCore.Tests.Common.Models.NormalModel")
                .AssertCountIsNotZero("logs not found");

            AuditLog lastLog = logs.LastOrDefault().AssertIsNotNull("last log is null");

            IEnumerable<AuditLogDetail> details = lastLog.LogDetails
                .AssertIsNotNull("log details is null")
                .AssertCountIsNotZero("no log details found");
        }
    }
}
