# Backend Developer Guide

Comprehensive guide for developers working on the POT ASP.NET Core backend.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Database & Entity Framework Core](#database--entity-framework-core)
- [Entity Design Patterns](#entity-design-patterns)
- [Relationships](#relationships)
- [Migrations](#migrations)
- [Database Sequences](#database-sequences)
- [CORS Configuration](#cors-configuration)
- [Best Practices](#best-practices)

---

## Architecture Overview

**Tech Stack:**

- ASP.NET Core (C#)
- Entity Framework Core (EF Core)
- PostgreSQL database
- JWT authentication
- Role-based permissions

**Project Structure:**

```
Source/Server/
├── Pot.AspNetCore/        # API controllers, middleware
├── Pot.App/               # Business logic, services
├── Pot.Data/              # EF Core, DbContext, entities
├── Pot.Data.Migrations/   # Database migrations
├── Pot.Shared/            # DTOs, shared models
└── Pot.EmailSender/       # Email services
```

**Key Principles:**

- Clean separation: API → App (business logic) → Data
- Entity Framework Core for data access
- All database entities inherit from `EntityBase`
- Entity names must end with `Entity` suffix
- Code-first migrations

---

## Database & Entity Framework Core

### DbContext

**Location:** `Pot.Data/PotDbContext.cs`

The main database context containing all `DbSet<>` properties for entities.

### Connection String

**Development:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pot;Username=postgres;Password=yourpassword"
  }
}
```

**Production (Docker):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=pot;Username=postgres;Password=password123"
  }
}
```

---

## Entity Design Patterns

### Base Entity

**All entities must inherit from `EntityBase`:**

```csharp
public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Benefits:**

- Consistent ID pattern (GUID)
- Automatic audit timestamps
- Shared behavior across all entities

### Entity Naming Convention

**All entity class names must end with `Entity`:**

```csharp
// ✅ GOOD
public class AccountEntity : EntityBase { }
public class ExpenseEntity : EntityBase { }
public class IncomeEntity : EntityBase { }

// ❌ BAD
public class Account : EntityBase { }
public class Expense : EntityBase { }
```

**Why?**

- Clear distinction between entities and DTOs
- Prevents naming conflicts
- Consistency across codebase

### Example Entity

```csharp
public class AccountEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public UserEntity User { get; set; } = null!;
    public ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();
    public ICollection<IncomeEntity> Incomes { get; set; } = new List<IncomeEntity>();
}
```

---

## Relationships

### One-to-Many Relationships

**Pattern:**

1. Foreign key property in dependent entity
2. Navigation property in dependent entity (reference)
3. Navigation property in principal entity (collection)

**Example:** User has many Accounts

```csharp
// Principal: UserEntity
public class UserEntity : EntityBase
{
    public string Email { get; set; } = string.Empty;

    // Collection navigation property
    public ICollection<AccountEntity> Accounts { get; set; } = new List<AccountEntity>();
}

// Dependent: AccountEntity
public class AccountEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;

    // Foreign key
    public Guid UserId { get; set; }

    // Reference navigation property
    public UserEntity User { get; set; } = null!;
}
```

**Configuration (Fluent API):**

```csharp
modelBuilder.Entity<AccountEntity>()
    .HasOne(a => a.User)
    .WithMany(u => u.Accounts)
    .HasForeignKey(a => a.UserId)
    .OnDelete(DeleteBehavior.Cascade);
```

### Many-to-Many Relationships

**Two approaches:**

#### 1. With Explicit Join Entity (Recommended for complex joins)

**Example:** User and Role relationship with additional metadata

```csharp
// Join Entity
public class UserRoleEntity : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; }

    // Navigation properties
    public UserEntity User { get; set; } = null!;
    public RoleEntity Role { get; set; } = null!;
}

// UserEntity
public class UserEntity : EntityBase
{
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}

// RoleEntity
public class RoleEntity : EntityBase
{
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}
```

**Configuration:**

```csharp
modelBuilder.Entity<UserRoleEntity>()
    .HasKey(ur => new { ur.UserId, ur.RoleId });

modelBuilder.Entity<UserRoleEntity>()
    .HasOne(ur => ur.User)
    .WithMany(u => u.UserRoles)
    .HasForeignKey(ur => ur.UserId);

modelBuilder.Entity<UserRoleEntity>()
    .HasOne(ur => ur.Role)
    .WithMany(r => r.UserRoles)
    .HasForeignKey(ur => ur.RoleId);
```

#### 2. Without Join Entity (Simple relationships)

**Example:** Simple tags relationship

```csharp
// UserEntity
public class UserEntity : EntityBase
{
    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
}

// TagEntity
public class TagEntity : EntityBase
{
    public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
}
```

**Configuration:**

```csharp
modelBuilder.Entity<UserEntity>()
    .HasMany(u => u.Tags)
    .WithMany(t => t.Users)
    .UsingEntity(j => j.ToTable("UserTags"));
```

---

## Migrations

### Creating Migrations

#### Visual Studio Package Manager Console

```powershell
# Set Pot.Data.Migrations as startup project
Add-Migration AddNewFeature -Project Pot.Data.Migrations -StartupProject Pot.Data.Migrations

# Or from Pot.AspNetCore
Add-Migration AddNewFeature -Project Pot.Data.Migrations -StartupProject Pot.AspNetCore
```

#### .NET CLI

```bash
# From solution root
cd Source/Server
dotnet ef migrations add AddNewFeature --project Pot.Data.Migrations
```

**Migration Naming:**

- Use PascalCase
- Be descriptive: `AddExpenseCategory`, `UpdateUserPermissions`
- Prefix with action: `Add`, `Update`, `Remove`, `Create`

### Applying Migrations

#### Visual Studio Package Manager Console

```powershell
Update-Database -Project Pot.Data.Migrations
```

#### .NET CLI

```bash
cd Source/Server
dotnet ef database update --project Pot.Data.Migrations
```

#### Using Pot.Data.Migrations Console App

The `Pot.Data.Migrations` project includes a console application that can apply migrations at startup.

**When to use:**

- Docker containers (automatic migration on startup)
- CI/CD pipelines
- First-time database setup

**Configuration:**

```csharp
// Program.cs in Pot.Data.Migrations
var connectionString = configuration.GetConnectionString("DefaultConnection");
using var scope = services.BuildServiceProvider().CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
await dbContext.Database.MigrateAsync(); // Applies all pending migrations
```

### Rollback Migrations

```bash
# Visual Studio
Update-Database -Migration PreviousMigrationName -Project Pot.Data.Migrations

# .NET CLI
dotnet ef database update PreviousMigrationName --project Pot.Data.Migrations
```

### Remove Last Migration (not yet applied)

```bash
# Visual Studio
Remove-Migration -Project Pot.Data.Migrations

# .NET CLI
dotnet ef migrations remove --project Pot.Data.Migrations
```

---

## Database Sequences

### What are Sequences?

PostgreSQL sequences generate unique numeric identifiers. In POT, they're used for human-readable IDs (e.g., expense numbers, invoice numbers).

**Difference from GUIDs:**

- Sequences: Sequential integers (1, 2, 3, ...)
- GUIDs: Random unique identifiers (EntityBase.Id)

### When to Use Sequences

- User-facing identifiers: Invoice numbers, expense IDs
- Sequential numbering: Order numbers, ticket IDs
- NOT for primary keys (use GUID from EntityBase)

### Defining Sequences

```csharp
// In entity configuration
modelBuilder.HasSequence<int>("expense_number_seq")
    .StartsAt(1)
    .IncrementsBy(1);

modelBuilder.Entity<ExpenseEntity>()
    .Property(e => e.ExpenseNumber)
    .HasDefaultValueSql("nextval('expense_number_seq')");
```

### Common Issue: Out-of-Sync Sequences

**Symptoms:**

- Duplicate key violations on insert
- Sequence generates numbers already in use

**Cause:**

- Manual data imports with explicit sequence values
- Database restore from backup
- Manually inserted rows

**Fix:**

```sql
-- Find the maximum value currently in use
SELECT MAX(expense_number) FROM expenses;

-- Reset sequence to max + 1
SELECT setval('expense_number_seq', (SELECT MAX(expense_number) FROM expenses));
```

**Automated Fix Script:**

```sql
-- For all sequences in database
DO $$
DECLARE
    seq_name text;
    max_val bigint;
BEGIN
    FOR seq_name IN
        SELECT sequence_name
        FROM information_schema.sequences
        WHERE sequence_schema = 'public'
    LOOP
        EXECUTE format('SELECT setval(''%I'', COALESCE((SELECT MAX(id) FROM %I), 1))',
                      seq_name,
                      REPLACE(seq_name, '_seq', ''));
    END LOOP;
END $$;
```

### Preventing Sequence Issues

1. **Never manually set sequence values** unless resetting
2. **After bulk imports**, reset sequences:
   ```sql
   SELECT setval('expense_number_seq', (SELECT MAX(expense_number) FROM expenses));
   ```
3. **In migrations**, use sequence configuration:
   ```csharp
   migrationBuilder.CreateSequence<int>("expense_number_seq", startValue: 1);
   ```

---

## CORS Configuration

**Location:** `Pot.AspNetCore/Program.cs` or startup configuration

### Export Feature Support

For the frontend export feature to extract filenames from the `Content-Disposition` header, the backend must expose this header in CORS policy:

```csharp
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5175") // Frontend dev server
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("content-disposition")); // Required for export filename extraction
```

**Why This Is Needed:**

- By default, browsers only expose safe CORS headers (e.g., `Content-Type`)
- `Content-Disposition` is not a safe header and must be explicitly exposed
- Frontend JavaScript cannot read it without this configuration
- Used by export functionality to determine downloaded filename

**Development vs Production:**

```csharp
// Development
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5175")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("content-disposition"));

// Production (configure allowed origins from environment)
var allowedOrigins = configuration["Cors:AllowedOrigins"]?.Split(',') ?? Array.Empty<string>();
app.UseCors(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("content-disposition"));
```

---

## Best Practices

### Entity Design

1. **Always inherit from EntityBase**

   ```csharp
   public class MyEntity : EntityBase { }
   ```

2. **Use Entity suffix**

   ```csharp
   public class AccountEntity : EntityBase { }
   ```

3. **Initialize collections**

   ```csharp
   public ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();
   ```

4. **Use non-nullable reference types**
   ```csharp
   public string Name { get; set; } = string.Empty;
   public UserEntity User { get; set; } = null!; // Will be set by EF Core
   ```

### Entity Configuration

1. **Use Fluent API over Data Annotations**

   ```csharp
   // ✅ GOOD - Fluent API
   modelBuilder.Entity<AccountEntity>()
       .Property(a => a.Name)
       .HasMaxLength(100)
       .IsRequired();

   // ❌ AVOID - Data Annotations
   [MaxLength(100), Required]
   public string Name { get; set; }
   ```

2. **Configure relationships explicitly**

   ```csharp
   modelBuilder.Entity<ExpenseEntity>()
       .HasOne(e => e.Account)
       .WithMany(a => a.Expenses)
       .HasForeignKey(e => e.AccountId)
       .OnDelete(DeleteBehavior.Cascade);
   ```

3. **Use separate configuration classes for complex entities**

   ```csharp
   public class AccountEntityConfiguration : IEntityTypeConfiguration<AccountEntity>
   {
       public void Configure(EntityTypeBuilder<AccountEntity> builder)
       {
           builder.HasKey(a => a.Id);
           builder.Property(a => a.Name).HasMaxLength(100).IsRequired();
           // ...
       }
   }

   // In DbContext.OnModelCreating
   modelBuilder.ApplyConfiguration(new AccountEntityConfiguration());
   ```

### Migrations

1. **Review generated migrations** before applying

   ```bash
   # Check what the migration will do
   dotnet ef migrations script --project Pot.Data.Migrations
   ```

2. **Keep migrations small and focused**

   - One feature per migration
   - Easier to rollback
   - Clearer history

3. **Test migrations** on development database first

4. **Never modify applied migrations**
   - Create new migration to fix issues
   - Or rollback and remove, then recreate

### Performance

1. **Use indexes for frequently queried columns**

   ```csharp
   modelBuilder.Entity<ExpenseEntity>()
       .HasIndex(e => e.Date);
   ```

2. **Use composite indexes for multi-column queries**

   ```csharp
   modelBuilder.Entity<ExpenseEntity>()
       .HasIndex(e => new { e.UserId, e.Date });
   ```

3. **Avoid N+1 queries with Include/ThenInclude**

   ```csharp
   // ❌ BAD - N+1 queries
   var users = await context.Users.ToListAsync();
   foreach (var user in users)
   {
       var accounts = user.Accounts; // Separate query per user
   }

   // ✅ GOOD - Single query with join
   var users = await context.Users
       .Include(u => u.Accounts)
       .ToListAsync();
   ```

4. **Use AsNoTracking for read-only queries**

   ```csharp
   var accounts = await context.Accounts
       .AsNoTracking()
       .ToListAsync();
   ```

5. **Project to DTOs to reduce data transfer**
   ```csharp
   var accountDtos = await context.Accounts
       .Select(a => new AccountDto
       {
           Id = a.Id,
           Name = a.Name,
           Balance = a.Balance
       })
       .ToListAsync();
   ```

### Data Validation

1. **Validate at multiple layers**

   - Client-side (React forms)
   - API layer (model validation)
   - Business logic layer
   - Database constraints

2. **Use database constraints for critical rules**

   ```csharp
   modelBuilder.Entity<AccountEntity>()
       .HasCheckConstraint("CK_Account_Balance", "balance >= 0");
   ```

3. **Don't rely solely on application validation**
   - Database constraints prevent bad data
   - Protects against direct DB access

---

## Common Patterns

### Soft Delete

```csharp
public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

// Global query filter
modelBuilder.Entity<AccountEntity>()
    .HasQueryFilter(a => !a.IsDeleted);
```

### Audit Fields

```csharp
public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}

// Automatic timestamp update
public override int SaveChanges()
{
    UpdateTimestamps();
    return base.SaveChanges();
}

private void UpdateTimestamps()
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.Entity is EntityBase &&
                   (e.State == EntityState.Added || e.State == EntityState.Modified));

    foreach (var entry in entries)
    {
        var entity = (EntityBase)entry.Entity;

        if (entry.State == EntityState.Added)
        {
            entity.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
```

### Owned Types

For value objects that don't have their own identity:

```csharp
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostCode { get; set; } = string.Empty;
}

public class UserEntity : EntityBase
{
    public Address Address { get; set; } = new();
}

// Configuration
modelBuilder.Entity<UserEntity>()
    .OwnsOne(u => u.Address);
```

---

## Troubleshooting

### Migration Not Detected

**Problem:** EF Core doesn't detect entity changes

**Solutions:**

1. Rebuild solution
2. Check entity is included in DbContext as DbSet
3. Verify entity inherits from EntityBase
4. Check entity naming (ends with Entity)

### Connection String Issues

**Problem:** Can't connect to PostgreSQL

**Solutions:**

1. Check PostgreSQL is running: `docker ps`
2. Verify connection string format: `Host=localhost;Database=pot;Username=postgres;Password=password`
3. Check port not in use: `netstat -an | findstr 5432`
4. Review PostgreSQL logs: `docker logs pot-postgres`

### Sequence Out of Sync

**Problem:** Duplicate key violation on insert

**Solution:**

```sql
SELECT setval('sequence_name', (SELECT MAX(column_name) FROM table_name));
```

### N+1 Query Performance

**Problem:** Slow queries with many round trips

**Solution:**

```csharp
// Use Include to eager load relationships
var users = await context.Users
    .Include(u => u.Accounts)
    .ThenInclude(a => a.Expenses)
    .ToListAsync();
```

---

## Additional Resources

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)

---

**For frontend integration, see:** `Source/Client/pot-react/DEVELOPER.md`

**For Docker setup, see:** `Source/Docker/DEVELOPER.md`
