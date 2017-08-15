using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerEnabledDbContext.EFCore.Configuration;
using TrackerEnabledDbContext.EFCore.Tests.Common.Code;

namespace TrackerEnabledDbContext.EFCore.Tests.Common
{
    [TestClass]
    public class PersistanceTests<TContext> where TContext : ITestDbContext, new()
    {
        private const string TestConnectionString = "DefaultTestConnection";
        private readonly RandomDataGenerator _randomDataGenerator = new RandomDataGenerator();

        protected TContext Db = new TContext();

        private IDbContextTransaction _transaction;

        protected bool RollBack = true;

        protected string RandomText => _randomDataGenerator.Get<string>();

        protected int RandomNumber => _randomDataGenerator.Get<int>();

        protected DateTime RandomDate => _randomDataGenerator.Get<DateTime>();

        protected char RandomChar => _randomDataGenerator.Get<char>();

        protected ObjectFactory<TContext> ObjectFactory = new ObjectFactory<TContext>();

        [TestInitialize]
        public virtual void Initialize()
        {
            Db.Database.EnsureCreated();

            _transaction = Db.Database.BeginTransaction();
            GlobalTrackingConfig.Enabled = true;
            GlobalTrackingConfig.TrackEmptyPropertiesOnAdditionAndDeletion = false;
            GlobalTrackingConfig.DisconnectedContext = false;
            GlobalTrackingConfig.ClearFluentConfiguration();
        }

        [TestCleanup]
        public virtual void CleanUp()
        {
            GlobalTrackingConfig.DisconnectedContext = false;

            if (RollBack)
            {
                _transaction?.Rollback();
            }
            else
            {
                _transaction?.Commit();
            }
        }
    }
}