using AllOverIt.EntityFrameworkCore.Extensions;
using AllOverIt.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pot.Data.Entities;
using System.Diagnostics;

namespace Pot.Data
{
    public sealed class PotDbContext : DbContext
    {
        private const string _entitySuffix = "Entity";
        private static readonly Type _entityBaseType = typeof(EntityBase);

        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }

        public void SetQueryTrackingBehavior(bool enabled)
        {
            ChangeTracker.QueryTrackingBehavior = enabled
                ? QueryTrackingBehavior.TrackAll
                : QueryTrackingBehavior.NoTrackingWithIdentityResolution;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseNpgsql("Host=localhost;Database=Pot;Username=postgres;Password=password", options =>
            {
                options.SetPostgresVersion(new Version(13, 6));
            });

            // Set to no tracking with identity resolution by default
            // This will help with read performance while ensuring duplicate entities with the same key are not created
            // https://docs.microsoft.com/en-us/ef/core/change-tracking/identity-resolution#identity-resolution-and-queries
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureEntities(modelBuilder);
            ConfigureEnrichedEnum(modelBuilder);
        }

        private static void ConfigureEntities(ModelBuilder modelBuilder)
        {
            // Exclude anything that is not an EntityBase, such as EnrichedEnum<>
            var entityTypes = modelBuilder.Model
                .GetEntityTypes()
                .Where(entity => entity.ClrType.IsDerivedFrom(typeof(EntityBase)));

            foreach (var entityType in entityTypes)
            {
                var entityName = entityType.DisplayName();

                ValidateEntity(entityType, entityName);
                SetTableName(entityType, entityName);
                DisableCascadeDelete(entityType);
            }
        }

        [Conditional("DEBUG")]
        private static void ValidateEntity(IMutableEntityType entityType, string entityName)
        {
            if (!entityName.EndsWith(_entitySuffix))
            {
                throw new InvalidOperationException(
                    $"The entity '{entityType.ClrType}' does not have a suffix of '{_entitySuffix}' (i.e. {entityName}{_entitySuffix}).");
            }

            if (!entityType.ClrType.IsDerivedFrom(_entityBaseType))
            {
                throw new InvalidOperationException($"The entity '{entityType.ClrType}' does not inherit '{_entityBaseType}'.");
            }
        }

        private static void SetTableName(IMutableEntityType entityType, string entityName)
        {
            var tableName = entityName[..^_entitySuffix.Length];

            entityType.SetTableName(tableName);
        }

        private static void DisableCascadeDelete(IMutableEntityType entityType)
        {
            var foreignKeys = entityType.GetForeignKeys();

            foreach (var foreignKey in foreignKeys)
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        private static void ConfigureEnrichedEnum(ModelBuilder modelBuilder)
        {
            // All enriched enum's across all entities will be stored as strings
            modelBuilder.UseEnrichedEnum(options => options.AsName(maxLength: 10));
        }
    }
}