using AllOverIt.EntityFrameworkCore.Extensions;
using AllOverIt.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Pot.Data.Entities;
using Pot.Shared.Extensions;
using System.Diagnostics;

namespace Pot.Data
{
    public abstract class DbContextBase : DbContext
    {
        private const string _entitySuffix = "Entity";
        private static readonly Type _entityBaseType = typeof(EntityBase);

        public override int SaveChanges()
        {
            OnBeforeSave();

            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSave();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSave();

            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnBeforeSave();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

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

        private void OnBeforeSave()
        {
            // For all create or update operations auto set the entity etag
            var entries = ChangeTracker
                .Entries()
                .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

            foreach (var entry in entries)
            {
                var entity = entry.Entity as EntityBase;

                if (entity is not null)
                {
                    entity.Etag = DateTime.UtcNow.GetEtag();
                }
            }
        }
    }
}