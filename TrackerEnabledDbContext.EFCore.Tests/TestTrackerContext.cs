using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrackerEnabledDbContext.EFCore.Tests.Common;
using TrackerEnabledDbContext.EFCore.Tests.Common.Models;

namespace TrackerEnabledDbContext.EFCore.Tests
{
    public class TestTrackerContext : TrackerContext, ITestDbContext
    {
        protected static readonly string TestConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=TEDB-Tests;" +
                          "Integrated Security=True;Connect Timeout=30;Encrypt=False;" +
                          "TrustServerCertificate=True;ApplicationIntent=ReadWrite;" +
                          "MultiSubnetFailover=False";
        //            Environment.GetEnvironmentVariable("TestGenericConnectionString")
        //            ?? "DefaultTestConnection";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole();

            optionsBuilder
                .UseSqlServer(TestConnectionString)
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModelWithCompositeKey>()
                .HasKey(c => new {c.Key1, c.Key2});

            modelBuilder.Entity<ModelWithComplexType>()
                .OwnsOne(c => c.ComplexType);
        }

        public DbSet<NormalModel> NormalModels { get; set; }
        public DbSet<ParentModel> ParentModels { get; set; }
        public DbSet<ChildModel> Children { get; set; }
        public DbSet<ModelWithCompositeKey> ModelsWithCompositeKey { get; set; }
        public DbSet<ModelWithConventionalKey> ModelsWithConventionalKey { get; set; }
        public DbSet<ModelWithSkipTracking> ModelsWithSkipTracking { get; set; }
        public DbSet<POCO> POCOs { get; set; }
        public DbSet<TrackedModelWithMultipleProperties> TrackedModelsWithMultipleProperties { get; set; }
        public DbSet<TrackedModelWithCustomTableAndColumnNames> TrackedModelsWithCustomTableAndColumnNames { get; set; }
        public DbSet<SoftDeletableModel> SoftDeletableModels { get; set; }
        public DbSet<ModelWithComplexType> ModelsWithComplexType { get; set; }
    }
}