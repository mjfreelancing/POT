# Backend Developer Guide

Comprehensive guide for developers working on the POT ASP.NET Core backend.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Entity Framework Core](#entity-framework-core)
  - [EntityBase](#entitybase)
  - [Entity Naming Convention](#entity-naming-convention)
  - [Table Naming Convention](#table-naming-convention)
- [Enriched Enums](#enriched-enums)
- [Entity Relationships](#entity-relationships)
- [Indexes](#indexes)
- [Multi-Tenancy & Query Filters](#multi-tenancy--query-filters)
- [Optimistic Concurrency (ETags)](#optimistic-concurrency-etags)
- [Migrations](#migrations)
- [CORS Configuration](#cors-configuration)
- [Available Commands](#available-commands)

---

## Architecture Overview

**Tech Stack:**

- ASP.NET Core 9 (C#)
- Entity Framework Core 9
- PostgreSQL 17 database
- JWT authentication
- Role-based permissions

**Project Structure:**

```
Source/Server/
├── Pot.AspNetCore/        # API controllers, middleware, endpoints
├── Pot.App/               # Business logic, services
├── Pot.Data/              # EF Core, DbContext, entities, repositories
├── Pot.Data.Migrations/   # Database migrations console app
├── Pot.Shared/            # DTOs, enumerations, shared models
└── Pot.EmailSender/       # Email services
```

**Key Architectural Principles:**

- **Security First**: JWT-based authentication with role-based authorization; endpoint-level permission checks (`resource:action`); rate limiting protecting against abuse (differentiated limits for authenticated vs anonymous users); CORS configured for trusted origins only; multi-tenancy isolation via query filters
- **Layered Clean Architecture**: Strict separation (API → App → Data) with dependency flow inward; outer layers depend on inner, never reversed
- **Pre-Validation with Problem Details**: FluentValidation executes before business logic; all errors returned as RFC 7807 Problem Details for consistent error handling across the API surface
- **Result Pattern for Business Logic**: Services return `EnrichedResult<T>` wrapping success/failure outcomes; business rule violations convert to Problem Details maintaining consistent error format between input validation and domain logic failures
- **Unified 422 Validation Responses**: All validation errors (FluentValidation request validation AND service-level business rule violations) return 422 Unprocessable Entity, never 400; simplifies client-side error handling with single status code for all validation scenarios
- **Minimal API Pattern**: Feature-based endpoint registration using route groups; authorization via `RequireAuthorization()` at endpoint level with `resource:action` permissions
- **Multi-Tenancy by Default**: Site-based query filters automatically isolate tenant data; requires explicit `IgnoreQueryFilters()` for cross-site operations
- **Health Checks**: API and database health monitoring at `/_health` endpoint for infrastructure monitoring and container orchestration
- **Explicit Transactions**: Manual transaction management via `IPotTransactionFactory` when needed; SaveChanges for single operations, transactions for multi-step business logic

**Design Conventions:**

- All entities inherit from `EntityBase` (provides `Id`, `RowId`, `Etag`)
- Entity names must end with `Entity` suffix (enforced in DEBUG builds)
- Table names automatically strip the `Entity` suffix (e.g., `AccountEntity` → `Account` table)
- Enriched enums stored as strings for database readability
- Code-first migrations for all schema changes
- Public APIs use `RowId` (Guid), never `Id` (int) to prevent leaking internal database identifiers

---

## Entity Framework Core

### EntityBase

**Location:** `Pot.Data/Entities/EntityBase.cs`

All entities **must** inherit from `EntityBase`:

```csharp
[Index(nameof(RowId), IsUnique = true)]
[Index(nameof(Etag), IsUnique = false)]
public abstract class EntityBase
{
    public int Id { get; set; }          // Primary key (auto-increment)
    public Guid RowId { get; set; }      // Public identifier (auto-generated)
    public long Etag { get; set; }       // Optimistic concurrency token (auto-updated)
}
```

**Why Three Identifiers?**

- **`Id` (int)**: Database primary key - efficient joins and foreign keys
- **`RowId` (Guid)**: Public API identifier - prevents leaking internal database IDs to consumers
- **`Etag` (long)**: Optimistic concurrency token - detects concurrent updates

**All API requests/responses use `RowId`, never `Id`.**

**Auto-Generation:**

- `RowId`: Auto-generated on insert via `GuidValueGenerator`
- `Etag`: Auto-updated on insert/update via `DbContextBase.OnBeforeSave()`

### Entity Naming Convention

**All entity class names must end with `Entity`:**

```csharp
// ✅ GOOD
public class AccountEntity : EntityBase { }
public class ExpenseEntity : EntityBase { }
public class UserEntity : EntityBase { }

// ❌ BAD - will throw InvalidOperationException in DEBUG builds
public class Account : EntityBase { }
public class Expense : EntityBase { }
```

**Why?**

- Clear distinction between entities (database models) and DTOs (API models)
- Prevents naming conflicts
- Enforced consistency across codebase

**Enforcement:**

```csharp
// DbContextBase.cs - ValidateEntity() runs in DEBUG builds only
if (!entityName.EndsWith(EntitySuffix))
{
    throw new InvalidOperationException(
        $"The entity '{entityType.ClrType}' does not have a suffix of '{EntitySuffix}'."
    );
}
```

### Table Naming Convention

**Table names automatically strip the `Entity` suffix:**

```csharp
// Entity class
public class AccountEntity : EntityBase { }

// Database table name
→ "Account"
```

**Implementation:**

```csharp
// DbContextBase.cs - SetTableName()
private static void SetTableName(IMutableEntityType entityType, string entityName)
{
    var tableName = entityName[..^EntitySuffix.Length];
    entityType.SetTableName(tableName);
}
```

**Example Entity:**

```csharp
// Pot.Data/Entities/AccountEntity.cs
[Index(nameof(Description), IsUnique = true)]
[Index(nameof(Bsb), nameof(Number), IsUnique = true)]
public sealed class AccountEntity : EntityBase
{
    [Required]
    [AccountBsb]
    [MaxLength(7)]
    public required string Bsb { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Number { get; set; }

    [Required]
    [MediumString]
    [Citext]
    public required string Description { get; set; }

    public double Balance { get; set; }
    public double Reserved { get; set; }

    // Navigation properties
    public required SiteEntity Site { get; set; }
    public ICollection<IncomeEntity> Incomes { get; set; } = [];
    public ICollection<ExpenseEntity> Expenses { get; set; } = [];
}
```

### Entity Conventions

1. **Always inherit from EntityBase**

   ```csharp
   public sealed class MyEntity : EntityBase { }
   ```

2. **Use `Entity` suffix** (enforced in DEBUG builds)

   ```csharp
   public sealed class AccountEntity : EntityBase { }
   ```

3. **Initialize collections with `[]`**

   ```csharp
   public ICollection<ExpenseEntity> Expenses { get; set; } = [];
   ```

4. **Use `required` for non-nullable reference properties**

   ```csharp
   public required string Description { get; set; }
   public required AccountEntity Account { get; set; }
   ```

5. **Seal entity classes** (prevents inheritance unless explicitly designed for it)

   ```csharp
   public sealed class AccountEntity : EntityBase { }
   ```

6. **Use Data Annotations** for simple validation

   ```csharp
   [Required]
   [MaxLength(100)]
   public required string Description { get; set; }
   ```

7. **Create custom validation attributes** for domain-specific rules

   ```csharp
   [AccountBsb]  // See Pot.Data/Annotations/
   public required string Bsb { get; set; }
   ```

### Entity Configuration

1. **Use Fluent API for complex relationships**

   ```csharp
   // PotDbContext.cs - OnModelCreating()
   modelBuilder.Entity<UserEntity>()
       .HasMany(u => u.Roles)
       .WithMany(r => r.Users)
       .UsingEntity("UserRole");
   ```

2. **Set default values explicitly when needed**

   ```csharp
   modelBuilder.Entity<ExpenseEntity>()
       .Property(e => e.AccruedIsDirty)
       .HasDefaultValue(true);
   ```

3. **Use global query filters** for multi-tenancy

   ```csharp
   modelBuilder.Entity<AccountEntity>()
       .HasQueryFilter(a => a.Site.Id == GetCurrentUserSiteId());
   ```

### Performance Considerations

1. **Use `WithTracking()` only when persisting changes**

   ```csharp
   // ✅ GOOD - No tracking needed for read-only queries (default behavior)
   var accounts = await _accountRepository.GetAllAccountsAsync(cancellationToken);

   // ✅ GOOD - Enable tracking when updating entities
   using (_accountRepository.WithTracking())
   {
       var account = await _accountRepository.GetAccountAsync(id, cancellationToken);
       account.Balance = newBalance;
       await _accountRepository.SaveAsync(cancellationToken);
   }

   // ❌ BAD - Don't use AsNoTracking() (already default behavior)
   var accounts = await context.Accounts.AsNoTracking().ToListAsync();
   ```

   **Why:** DbContext is configured with `QueryTrackingBehavior.NoTrackingWithIdentityResolution` by default for performance. Repositories expose `WithTracking()` which calls `WithAutoTracking()` on the `DbContext` internally - this uses reference counting via `ConditionalWeakTable` to enable tracking only within the using scope, then automatically restores no-tracking behavior. This supports nested scopes safely (see `Pot.Data/Extensions/DbContextExtensions.cs`).

2. **Use `Include()` to avoid N+1 queries**

   ```csharp
   var users = await context.Users
       .Include(u => u.Roles)
       .ToListAsync();
   ```

3. **Project to DTOs** to reduce data transfer

   ```csharp
   var accountDtos = await context.Accounts
       .Select(a => new AccountDto
       {
           RowId = a.RowId,
           Description = a.Description,
           Balance = a.Balance
       })
       .ToListAsync();
   ```

4. **Use specifications pattern** for reusable query logic

   ```csharp
   // See Pot.Data/Repositories/*/Specifications/ for examples
   return Accounts.Where(AccountSpecifications.IsSameBsbNumber(bsb, number).Expression);
   ```

### API Design Guidelines

1. **Always use `RowId` in requests/responses**, never `Id`

   ```csharp
   // ✅ GOOD
   public record AccountDto(Guid RowId, string Description);

   // ❌ BAD - exposes internal database ID
   public record AccountDto(int Id, string Description);
   ```

2. **Include `Etag` in responses** for optimistic concurrency

   ```csharp
   public record AccountDto(Guid RowId, string Description, long Etag);
   ```

3. **Use enriched enums** for type-safe, human-readable values

   ```csharp
   public required Frequency Frequency { get; set; }  // Not int or string
   ```

---

## Enriched Enums

**Location:** `Pot.Shared/Enumerations/`

POT uses **enriched enums** (type-safe enum pattern) instead of standard C# enums.

### What Are Enriched Enums?

Enriched enums are classes that provide type-safe, named constants with additional functionality:

```csharp
// Pot.Shared/Enumerations/Frequency.cs
public sealed class Frequency : EnrichedEnum<Frequency>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly Frequency Days = new(1);
    public static readonly Frequency Weeks = new(2);
    public static readonly Frequency Months = new(3);
    public static readonly Frequency Years = new(4);
    public static readonly Frequency OneTime = new(5);

    private Frequency(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
```

### Database Storage

**Enriched enums are stored as strings** in the database:

```csharp
// DbContextBase.cs - ConfigureEnrichedEnum()
private static void ConfigureEnrichedEnum(ModelBuilder modelBuilder)
{
    // All enriched enum's across all entities will be stored as strings
    modelBuilder.UseEnrichedEnum(options => options.AsName(maxLength: 50));
}
```

**Why Strings Instead of Integers?**

- **Clarity**: Database queries and exports are human-readable (`"Months"` vs `3`)
- **Debugging**: Easier to understand data without lookup tables
- **Refactoring**: Adding/removing enum values doesn't break existing data
- **Trade-off**: Acknowledged that integers are more space/time efficient, but readability wins

**Example in Database:**

```sql
-- ExpenseEntity with Frequency
SELECT description, frequency FROM "Expense";

-- Results show human-readable strings:
-- "Rent"       | "Months"
-- "Electricity"| "Months"
-- "Insurance"  | "Years"
```

### Serialization Configuration

**Required for API requests/responses:**

```csharp
// Pot.AspNetCore/Program.cs - AddHttpJsonOptions()
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(EnrichedEnumJsonConverter<Frequency>.Create());
    options.SerializerOptions.Converters.Add(EnrichedEnumJsonConverter<UserStatus>.Create());
    options.SerializerOptions.Converters.Add(EnrichedEnumJsonConverter<ApprovalStatus>.Create());
    // ... add converter for each enriched enum
});
```

**Why Converters Are Required:**

Without these converters, JSON serialization/deserialization fails with non-obvious errors. The converters translate between:

- **Requests**: JSON string → Enriched enum instance
- **Responses**: Enriched enum instance → JSON string

**Example API Usage:**

```json
// Request body
{
  "description": "Rent",
  "frequency": "Months",
  "frequencyCount": 1
}

// Response body
{
  "rowId": "550e8400-e29b-41d4-a716-446655440000",
  "description": "Rent",
  "frequency": "Months",
  "frequencyCount": 1
}
```

### Available Enriched Enums

- `Frequency` - Days, Weeks, Months, Years, OneTime
- `UserStatus` - Active, Inactive, Approval, Suspended
- `ApprovalStatus` - Pending, Approved, Rejected
- `Role` - Admin, User, etc.
- `Permission` - account:view, expense:manage, etc.
- `OtpStatus` - Pending, Used, Expired
- `OtpReason` - Login, Registration, PasswordReset
- `SettingCategory` - Application settings categories

---

## Entity Relationships

### One-to-Many Relationships

**Pattern:** Foreign key in dependent entity + navigation properties

**Example:** Account has many Expenses

```csharp
// AccountEntity.cs
public sealed class AccountEntity : EntityBase
{
    public required string Description { get; set; }

    // Collection navigation property
    public ICollection<ExpenseEntity> Expenses { get; set; } = [];
}

// ExpenseEntity.cs
public sealed class ExpenseEntity : EntityBase
{
    public required string Description { get; set; }

    // Reference navigation property
    public required AccountEntity Account { get; set; }
}
```

**EF Core Configuration:**

Relationships are configured using **Data Annotations** (Index attributes) and conventions. Explicit Fluent API configuration is used only when needed.

**Collection Initialization:**

Use `[]` (collection expression) for new collections:

```csharp
// ✅ GOOD - Modern C# collection expression
public ICollection<ExpenseEntity> Expenses { get; set; } = [];

// ❌ OUTDATED - Don't use
public ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();
```

### Many-to-Many Relationships

**POT uses skip navigation properties** (EF Core automatically creates join tables):

**Example:** User and Role relationship

```csharp
// UserEntity.cs
public sealed class UserEntity : EntityBase
{
    public required string Username { get; set; }

    // Skip navigation property (skips join table)
    public ICollection<RoleEntity> Roles { get; set; } = [];
}

// RoleEntity.cs
public sealed class RoleEntity : EntityBase
{
    public required Role Name { get; set; }

    // Skip navigation property (skips join table)
    public ICollection<UserEntity> Users { get; set; } = [];
}
```

**Configuration:**

```csharp
// PotDbContext.cs - OnModelCreating()
modelBuilder
    .Entity<UserEntity>()
    .HasMany(user => user.Roles)
    .WithMany(role => role.Users)
    .UsingEntity("UserRole");  // Join table name
```

**Why Skip Navigation?**

- Simpler entity models (no explicit join entity)
- EF Core manages join table automatically
- Cleaner queries: `user.Roles` instead of `user.UserRoles.Select(ur => ur.Role)`

**Additional Example:** Role and Permission

```csharp
// RoleEntity.cs
public ICollection<PermissionEntity> Permissions { get; set; } = [];

// PermissionEntity.cs
public ICollection<RoleEntity> Roles { get; set; } = [];

// Configuration creates "RolePermission" join table
modelBuilder
    .Entity<RoleEntity>()
    .HasMany(role => role.Permissions)
    .WithMany(permission => permission.Roles)
    .UsingEntity("RolePermission");
```

### Cascade Delete Behavior

**Default:** Cascade delete is **disabled globally** for all foreign keys.

```csharp
// DbContextBase.cs - DisableCascadeDelete()
private static void DisableCascadeDelete(IMutableEntityType entityType)
{
    var foreignKeys = entityType.GetForeignKeys();

    foreach (var foreignKey in foreignKeys)
    {
        foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
```

**Why Restrict by Default?**

**Opinionated approach:** It's better to know you've coded something incorrectly (foreign key constraint violation) rather than allow the database to silently delete data you're not immediately aware of.

---

## Indexes

### Index Definition

Indexes are defined using **Data Annotations** directly on entity classes:

```csharp
// AccountEntity.cs
[Index(nameof(Description), IsUnique = true)]
[Index(nameof(Bsb), nameof(Number), IsUnique = true)]
public sealed class AccountEntity : EntityBase
{
    public required string Description { get; set; }
    public required string Bsb { get; set; }
    public required string Number { get; set; }
}
```

### Single Column Index

```csharp
[Index(nameof(Username), IsUnique = true)]
public sealed class UserEntity : EntityBase
{
    public required string Username { get; set; }
}
```

### Composite Index

```csharp
[Index("AccountId", nameof(Description), IsUnique = true)]
public sealed class ExpenseEntity : EntityBase
{
    public required string Description { get; set; }
    public required AccountEntity Account { get; set; }
}
```

**Note:** Use `"AccountId"` (string) for foreign key properties that aren't explicitly declared, or `nameof()` for declared properties.

### Multiple Indexes

```csharp
[Index(nameof(Status), nameof(ExpiryUtc))]
[Index(nameof(Username), nameof(Status), nameof(CreatedUtc))]
[Index(nameof(Reason), nameof(Username), nameof(RefCode))]
public sealed class OneTimePasswordEntity : EntityBase
{
    // Properties...
}
```

### EntityBase Indexes

All entities automatically inherit these indexes from `EntityBase`:

```csharp
[Index(nameof(RowId), IsUnique = true)]  // Unique index on public identifier
[Index(nameof(Etag), IsUnique = false)]  // Index for optimistic concurrency queries
public abstract class EntityBase
```

### Index Guidelines

1. **Use Data Annotations** for index definitions

   ```csharp
   [Index(nameof(Username), IsUnique = true)]
   public sealed class UserEntity : EntityBase { }
   ```

2. **Create composite indexes** to enforce uniqueness constraints

   ```csharp
   [Index("AccountId", nameof(Description), IsUnique = true)]
   ```

3. **Index foreign keys and frequently queried columns**

---

## Multi-Tenancy & Query Filters

### Site-Based Filtering

POT implements **multi-tenancy** using global query filters that automatically filter entities by the current user's Site.

**Location:** `Pot.Data/PotDbContext.cs`

```csharp
private void SetupQueryFilters(ModelBuilder modelBuilder)
{
    // Site-specific filter for Accounts
    modelBuilder
        .Entity<AccountEntity>()
        .HasQueryFilter(account => account.Site.Id == GetCurrentUserSiteId());

    // Site-specific filter for Expenses (via Account relationship)
    modelBuilder
        .Entity<ExpenseEntity>()
        .HasQueryFilter(expense => expense.Account.Site.Id == GetCurrentUserSiteId());

    // Site-specific filter for Incomes (via Account relationship)
    modelBuilder
        .Entity<IncomeEntity>()
        .HasQueryFilter(income => income.Account.Site.Id == GetCurrentUserSiteId());
}

private int GetCurrentUserSiteId()
{
    _currentUserSiteId ??= Set<UserEntity>()
        .Include(user => user.Site)
        .Single(user => user.RowId == _currentUserContext.UserRowId)
        .Site.Id;

    return _currentUserSiteId.Value;
}
```

**How It Works:**

- Every query automatically filters results to the current user's Site
- Users can only see data belonging to their Site
- Prevents accidental cross-site data access
- Applied automatically by EF Core

**Example:**

```csharp
// This query automatically filters by current user's Site
var accounts = await _dbContext.Accounts.ToListAsync();

// Equivalent to:
var accounts = await _dbContext.Accounts
    .Where(a => a.Site.Id == currentUserSiteId)
    .ToListAsync();
```

### Bypassing Query Filters

**Use Case:** When global uniqueness checks or cross-site operations are needed.

```csharp
// Check if account number exists globally (across all sites)
var exists = await Accounts
    .IgnoreQueryFilters()
    .AnyAsync(a => a.Bsb == bsb && a.Number == number);
```

**Example Use Cases:**

1. **Global Uniqueness:** Account BSB/Number must be unique across all sites

```csharp
// Pot.Data/Repositories/Accounts/AccountRepository.cs
public Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken)
{
    // Account numbers are globally unique
    return Accounts
        .IgnoreQueryFilters()
        .AnyAsync(AccountSpecifications.IsSameBsbNumber(bsb, number).Expression, cancellationToken);
}
```

2. **Cross-Site Admin Operations:** Platform admins viewing pending user approvals

```csharp
// Pot.App/Features/Approvals/Pending/GetPendingApprovalsService.cs
var users = await _userRepository.Users
    .IgnoreQueryFilters()
    .Where(user => user.Status == UserStatus.Approval)
    .ToListAsync(cancellationToken);
```

### Using Query Filters

1. **Use `IgnoreQueryFilters()`** only when needed

   ```csharp
   // For global uniqueness checks
   var exists = await Accounts
       .IgnoreQueryFilters()
       .AnyAsync(a => a.Bsb == bsb && a.Number == number);
   ```

2. **Document why filters are bypassed**

   ```csharp
   // Account numbers are globally unique across all sites
   return Accounts.IgnoreQueryFilters().AnyAsync(/*...*/);
   ```

---

## Optimistic Concurrency (ETags)

### What is an ETag?

**ETag** (Entity Tag) is a timestamp-based token used for optimistic concurrency control. It prevents lost updates when multiple users edit the same entity simultaneously.

**Location:** `EntityBase.Etag` property

```csharp
public abstract class EntityBase
{
    public int Id { get; set; }
    public Guid RowId { get; set; }
    public long Etag { get; set; }  // Auto-updated on every save
}
```

### How It Works

**Automatic ETag Generation:**

```csharp
// DbContextBase.cs - OnBeforeSave()
private void OnBeforeSave()
{
    var entries = ChangeTracker
        .Entries()
        .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

    foreach (var entry in entries)
    {
        var entity = entry.Entity as EntityBase;

        if (entity is not null)
        {
            entity.Etag = DateTime.UtcNow.GetEtag();  // Unix timestamp
        }
    }
}
```

**ETag Values:**

- Generated from `DateTime.UtcNow` converted to Unix timestamp (long)
- Updated automatically on insert and update operations
- Unique per save operation (timestamp precision)

### Usage Pattern

1. **Client retrieves entity** with current ETag
2. **User modifies data**
3. **Client sends update** with original ETag
4. **Server compares** ETag in request vs database
5. **If ETags match** → Update succeeds, new ETag generated
6. **If ETags differ** → Concurrent modification detected, update rejected

**Example API Flow:**

```csharp
// GET response includes ETag
{
  "rowId": "550e8400-e29b-41d4-a716-446655440000",
  "description": "Rent",
  "etag": 1700000000
}

// PUT request includes ETag
{
  "rowId": "550e8400-e29b-41d4-a716-446655440000",
  "description": "Monthly Rent",
  "etag": 1700000000  // Must match database value
}
```

### Why Optimistic Concurrency?

- Prevents "last write wins" conflicts
- No locking required (better performance)
- Client is notified of conflicts and can handle appropriately
- Standard pattern for distributed systems and REST APIs

---

## Migrations

**Location:** `Pot.Data.Migrations/` - Console application for database migrations

### Creating Migrations

**Using .NET CLI** (Recommended):

```bash
# From Source/Server directory
dotnet ef migrations add MigrationName --project Pot.Data.Migrations
```

**Using Visual Studio Package Manager Console:**

```powershell
# Set Pot.Data.Migrations as startup project
Add-Migration MigrationName -Project Pot.Data.Migrations
```

**Migration Naming Conventions:**

- Use PascalCase
- Be descriptive: `AddUserApprovalStatus`, `UpdateAccountIndexes`
- Prefix with action verb: `Add`, `Update`, `Remove`, `Create`

### Applying Migrations

**Using .NET CLI:**

```bash
cd Source/Server
dotnet ef database update --project Pot.Data.Migrations
```

**Using Visual Studio:**

```powershell
Update-Database -Project Pot.Data.Migrations
```

**Using Migrations Console App:**

The `Pot.Data.Migrations` project is a console application that automatically applies pending migrations on startup.

```bash
cd Source/Server/Pot.Data.Migrations
dotnet run
```

**When Used:**

- Docker container startup (automatic migrations)
- CI/CD pipelines
- First-time database setup
- Production deployments

**How It Works:**

```csharp
// Pot.Data.Migrations/Program.cs
await GenericHost
    .CreateConsoleHostBuilder<App>(args)
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddDbContextFactory<PotDbContext>(/*...*/)
            .AddSingleton<IDatabaseMigrator, PotDbMigrator>();
    })
    .RunConsoleAsync();

// App.cs applies migrations
await dbContext.Database.MigrateAsync();  // Applies all pending migrations
```

### Rollback Migrations

```bash
# .NET CLI
dotnet ef database update PreviousMigrationName --project Pot.Data.Migrations

# Visual Studio
Update-Database -Migration PreviousMigrationName -Project Pot.Data.Migrations
```

### Remove Last Migration (not yet applied)

```bash
# .NET CLI
dotnet ef migrations remove --project Pot.Data.Migrations

# Visual Studio
Remove-Migration -Project Pot.Data.Migrations
```

### Migration Guidelines

1. **Review generated migrations** before applying
2. **Keep migrations small and focused** - one feature per migration
3. **Test on development database** before production
4. **Never modify applied migrations** - create new migration to fix issues
5. **Use descriptive names** for easy history tracking

---

## CORS Configuration

**Location:** `Pot.AspNetCore/Concerns/Cors/`

POT uses a configuration-based CORS system that loads allowed origins from `appsettings.json`.

### Configuration Setup

```csharp
// Pot.AspNetCore/Program.cs
builder
    .AddPotCors()  // Configures CORS from configuration
    // ...

app.UseCors();  // Must be before UseAuthentication/UseAuthorization
```

### CORS Policy

```csharp
// Pot.AspNetCore/Concerns/Cors/Configuration/CorsOptionsSetup.cs
public void Configure(CorsOptions options)
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            // Allow frontend URLs from configuration
            .WithOrigins(_corsConfiguration.AllowedOrigins)

            .AllowAnyMethod()
            .AllowAnyHeader()

            // Required for authentication (cookies/tokens)
            .AllowCredentials()

            // Expose content-disposition for file downloads
            .WithExposedHeaders("content-disposition");
    });
}
```

### Why Expose `content-disposition`?

The frontend export feature needs to extract filenames from the `Content-Disposition` header. By default, browsers only expose "safe" CORS headers (`Content-Type`, `Cache-Control`, etc.).

**Without this configuration:**

- Browser blocks access to `Content-Disposition` header
- Frontend cannot determine downloaded filename
- Export feature fails silently

**With this configuration:**

- Frontend can read `Content-Disposition: attachment; filename="export-2025-11-18.pot"`
- Export downloads work correctly

### Configuration File

```json
// appsettings.Development.json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5175" // Vite dev server
    ]
  }
}
```

**Important:** The ASP.NET Core CORS policy cannot use `AllowAnyOrigin()` with `AllowCredentials()` - must specify explicit origins.

---

## Available Commands

### Development

| Command                                 | Description                                         |
| --------------------------------------- | --------------------------------------------------- |
| `dotnet run --project Pot.AspNetCore`   | Start API server (typically https://localhost:7241) |
| `dotnet build`                          | Build solution                                      |
| `dotnet watch --project Pot.AspNetCore` | Run API with hot reload                             |

### Migrations

| Command                                                                   | Description                     |
| ------------------------------------------------------------------------- | ------------------------------- |
| `dotnet ef migrations add <Name> --project Pot.Data.Migrations`           | Create new migration            |
| `dotnet ef database update --project Pot.Data.Migrations`                 | Apply pending migrations        |
| `dotnet ef migrations remove --project Pot.Data.Migrations`               | Remove last unapplied migration |
| `dotnet ef database update <MigrationName> --project Pot.Data.Migrations` | Rollback to specific migration  |
| `dotnet run --project Pot.Data.Migrations`                                | Run migrations console app      |

### Testing & Quality

| Command                                       | Description                     |
| --------------------------------------------- | ------------------------------- |
| `dotnet test`                                 | Run all unit tests              |
| `dotnet test --collect:"XPlat Code Coverage"` | Run tests with code coverage    |
| `dotnet format`                               | Format code using .editorconfig |

---

## Additional Resources

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)

---

**For frontend integration, see:** `Source/Client/pot-react/DEVELOPER.md`

**For Docker setup, see:** `Source/Docker/DEVELOPER.md`
