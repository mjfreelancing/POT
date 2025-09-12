# Pot.Data

This project manages Entity Framework Core database migrations and contains the required design-time tools for the POT application.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Access to the target database (connection string configured in the main application or via environment variables)

## Generating a New Migration

### Using the .NET CLI

1. Open a terminal in the `Pot.Data` project directory.
2. Run the following command, replacing `MigrationName` with a descriptive name for your migration:
   `dotnet ef migrations add MigrationName --project Pot.Data --startup-project Pot.Data.Migrations`

- `--project` specifies the migrations project (Pot.Data).
- `--startup-project` should be set to Pot.Data.Migrations.

### Using Visual Studio (Package Manager Console)

1. Open the solution in Visual Studio.
2. Go to **Tools > NuGet Package Manager > Package Manager Console**.
3. In the Package Manager Console (PMC), ensure the **Default project** is set to `Pot.Data`.
4. Run the following command, replacing `MigrationName` with your migration name:
   `Add-Migration MigrationName -StartupProject Pot.Data.Migrations`

## Applying Migrations to the Database

### Using the .NET CLI

1. Ensure your connection string is set correctly (in `appsettings.json` or via environment variables).
2. Run the following command to apply all pending migrations:
   dotnet ef database update --project Pot.Data --startup-project Pot.Data

### Using Visual Studio (Package Manager Console)

1. Open the solution in Visual Studio.
2. Go to **Tools > NuGet Package Manager > Package Manager Console**.
3. In the Package Manager Console, ensure the **Default project** is set to `Pot.Data`.
4. Run the following command:
   `Update-Database -StartupProject Pot.Data.Migrations`

### Using the Pot.Data.Migrations Application

You can also apply migrations by running the `Pot.Data.Migrations` application directly. This is useful for automated deployments or when you prefer not to use the CLI or Visual Studio.

1. Build the solution.
2. Run the `Pot.Data.Migrations` project using your preferred method (e.g., Visual Studio, `dotnet run`, or as a published executable):
   dotnet run --project Pot.Data.Migrations
   This will apply all pending migrations to the database using the connection string configured for `Pot.Data.Migrations`.

## Adding New Entities

When adding new entities to the database:

> **Important**: All entity class names MUST have the 'Entity' suffix (e.g., `UserEntity`, `AccountEntity`) and inherit from `EntityBase`. This is enforced by `DbContextBase` at runtime.

1. **Create the Entity Class**

   ```csharp
   // In Entities folder
   public class NewTableEntity : EntityBase    // Note: Must inherit from EntityBase and end with 'Entity'
   {
       public string Name { get; set; }

       [Required]
       [MaxLength(256)]
       public string Description { get; set; }

       [Required]
       public DateTimeOffset CreatedUtc { get; set; }

       // Navigation property example (one-to-many)
       public ICollection<RelatedTableEntity> RelatedEntities { get; set; }

       // Navigation property example (many-to-one)
       public ParentTableEntity Parent { get; set; }
       public Guid ParentId { get; set; }
   }
   ```

2. **Add to DbContext**

   ```csharp
   public class PotContext : DbContextBase
   {
       public DbSet<NewEntity> NewEntities { get; set; }

       // Only needed for many-to-many relationships without join entities
       protected override void OnModelCreating(ModelBuilder modelBuilder)
       {
           base.OnModelCreating(modelBuilder);

           // Example many-to-many relationship
           modelBuilder.Entity<FirstEntity>()
               .HasMany(e => e.SecondEntities)
               .WithMany(e => e.FirstEntities)
               .UsingEntity("FirstSecondJoin");
       }
   }
   ```

3. **Generate Migration**
   ```bash
   dotnet ef migrations add Add_NewEntity --project Pot.Data --startup-project Pot.Data.Migrations
   ```

Note:

- Entity configuration is done using attributes on the entity class.
- Navigation properties are used to define relationships between entities.
- Manual configuration in `OnModelCreating` is only needed for many-to-many relationships without explicit join entities.
- Foreign keys are automatically discovered based on navigation property naming conventions.

## Adding Relationships

### One-to-Many Relationship

Define navigation properties and foreign key property:

```csharp
public class ParentEntity : EntityBase
{
    public ICollection<ChildEntity> Children { get; set; }
}

public class ChildEntity : EntityBase
{
    public Guid ParentId { get; set; }
    public ParentEntity Parent { get; set; }
}
```

The relationship will be automatically configured by Entity Framework Core based on:

- Navigation property names (`Parent` and `Children`)
- Foreign key property name (`ParentId`)
- Entity Framework will use `OnDelete.Restrict` by default as configured in `DbContextBase`

### Many-to-Many Relationship

You have two options for many-to-many relationships:

1. **Without Explicit Join Entity** (preferred when no additional data needed):

```csharp
public class RoleEntity : EntityBase
{
    public ICollection<UserEntity> Users { get; set; }
}

public class UserEntity : EntityBase
{
    public ICollection<RoleEntity> Roles { get; set; }
}

// In DbContext:
modelBuilder.Entity<UserEntity>()
    .HasMany(u => u.Roles)
    .WithMany(r => r.Users)
    .UsingEntity("UserRole");
```

2. **With Explicit Join Entity** (when join table needs additional properties):

```csharp
public class UserSiteEntity : EntityBase
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; }

    public Guid SiteId { get; set; }
    public SiteEntity Site { get; set; }

    public DateTimeOffset JoinedUtc { get; set; }  // Additional data
}
```

## Best Practices

1. **Entity Design**

   - Always inherit from `Identity` for consistent IDs
   - Use meaningful property names
   - Add audit fields where appropriate (CreatedUtc, ModifiedUtc)
   - Consider soft delete via IsDeleted flag

2. **Configuration**

   - Always specify max lengths for string properties
   - Configure appropriate indexes for frequent queries
   - Use meaningful foreign key names
   - Consider cascade delete behavior carefully

3. **Migrations**

   - Use descriptive migration names
   - Review migration SQL before applying
   - Test migrations on copy of production data
   - Include data migrations where needed

4. **Performance**
   - Add indexes for frequently queried columns
   - Consider adding covering indexes for common queries
   - Use appropriate fetch strategies (eager vs lazy loading)
   - Monitor query performance during development

## Additional Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [EF Core Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)

---

For troubleshooting or advanced scenarios, refer to the official EF Core documentation or the POT project documentation.
