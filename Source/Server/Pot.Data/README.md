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

## Database Sequences

This section explains how to manage and correct database sequences if they become out of sync during development. Using the `Account`, `Expense`, and `Income` tables as examples:

### Determining the Sequence Names

To find the sequence names for your tables, run the following query:

```sql
SELECT c.relname AS sequence_name
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE c.relkind = 'S'; -- 'S' stands for sequence
```

This will return the names of all sequences in the database. For example:

- `Account_Id_seq`
- `Expense_Id_seq`
- `Income_Id_seq`

### Updating the Sequences

If a sequence becomes out of sync (e.g., after manual imports), you can update it to match the highest `Id` in the corresponding table. Use the following commands:

```sql
-- Update the Account sequence
SELECT MAX("Id") AS max_id FROM "Account";
SELECT setval('public."Account_Id_seq"', (SELECT MAX("Id") FROM "Account"));

-- Update the Expense sequence
SELECT MAX("Id") AS max_id FROM "Expense";
SELECT setval('public."Expense_Id_seq"', (SELECT MAX("Id") FROM "Expense"));

-- Update the Income sequence
SELECT MAX("Id") AS max_id FROM "Income";
SELECT setval('public."Income_Id_seq"', (SELECT MAX("Id") FROM "Income"));
```

### Verifying the Sequence Values

After updating the sequences, you can verify their current values using the following queries:

```sql
-- Check the current value of the Account sequence
SELECT last_value, is_called
FROM public."Account_Id_seq";

-- Check the current value of the Expense sequence
SELECT last_value, is_called
FROM public."Expense_Id_seq";

-- Check the current value of the Income sequence
SELECT last_value, is_called
FROM public."Income_Id_seq";
```

### Explanation of Commands

1. **Determining the Maximum ID**:

   - `SELECT MAX("Id") AS max_id FROM "Account";`
     - Finds the highest `Id` currently in the `Account` table.

2. **Updating the Sequence**:

   - `SELECT setval('public."Account_Id_seq"', (SELECT MAX("Id") FROM "Account"));`
     - Sets the sequence to start at the highest `Id` in the `Account` table.

3. **Verifying the Sequence**:
   - `SELECT last_value, is_called FROM public."Account_Id_seq";`
     - Confirms the current value of the sequence (`last_value`) and whether it has been used (`is_called`).

### Reference for all tables

```sql
SELECT c.relname AS sequence_name
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE c.relkind = 'S' -- 'S' stands for sequence

SELECT setval('public."Permission_Id_seq"', (SELECT MAX("Id") FROM "Permission"));
SELECT setval('public."Role_Id_seq"', (SELECT MAX("Id") FROM "Role"));
SELECT setval('public."Site_Id_seq"', (SELECT MAX("Id") FROM "Site"));
SELECT setval('public."Account_Id_seq"', (SELECT MAX("Id") FROM "Account"));
SELECT setval('public."User_Id_seq"', (SELECT MAX("Id") FROM "User"));
SELECT setval('public."Expense_Id_seq"', (SELECT MAX("Id") FROM "Expense"));
SELECT setval('public."Income_Id_seq"', (SELECT MAX("Id") FROM "Income"));
```

## Additional Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [EF Core Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)

---

For troubleshooting or advanced scenarios, refer to the official EF Core documentation or the POT project documentation.
