# Backend Developer Guide

Comprehensive guide for developers working on the POT ASP.NET Core backend. This guide provides actionable patterns and conventions to help you understand the architecture and implement new features consistently.

## Table of Contents

- [Architecture Overview](#architecture-overview)
  - [Project Structure](#project-structure)
  - [Key Architectural Principles](#key-architectural-principles)
  - [Design Conventions](#design-conventions)
- [Feature Implementation Recipe](#feature-implementation-recipe)
  - [Step-by-Step Guide](#step-by-step-guide)
  - [Example Walkthrough](#example-walkthrough)
- [Feature Organization](#feature-organization)
  - [API Layer (Pot.AspNetCore)](#api-layer-potaspnetcore)
  - [Application Layer (Pot.App)](#application-layer-potapp)
  - [Data Layer (Pot.Data)](#data-layer-potdata)
  - [Shared Layer (Pot.Shared)](#shared-layer-potshared)
- [Endpoint Patterns](#endpoint-patterns)
  - [Handler Pattern](#handler-pattern)
  - [Request Validation](#request-validation)
  - [Response Construction](#response-construction)
  - [Endpoint Registration](#endpoint-registration)
- [Service Layer](#service-layer)
  - [Service Pattern](#service-pattern)
  - [Entity Checks](#entity-checks)
  - [Result Pattern](#result-pattern)
  - [Transaction Management](#transaction-management)
- [Repository Layer](#repository-layer)
  - [Repository Pattern](#repository-pattern)
  - [Tracking vs No-Tracking](#tracking-vs-no-tracking)
  - [Query Specifications](#query-specifications)
- [Mapping Patterns](#mapping-patterns)
  - [Request to Input Mapping](#request-to-input-mapping)
  - [Entity to Output Mapping](#entity-to-output-mapping)
- [Concerns (Cross-Cutting)](#concerns-cross-cutting)
  - [Authentication & Authorization](#authentication--authorization)
  - [Validation](#validation)
  - [CORS Configuration](#cors-configuration)
  - [Rate Limiting](#rate-limiting)
  - [Middleware](#middleware)
  - [Problem Details](#problem-details)
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
- [Dependency Injection](#dependency-injection)
  - [Auto-Registration Pattern](#auto-registration-pattern)
  - [Service Lifetimes](#service-lifetimes)
- [Testing Patterns](#testing-patterns)
- [Available Commands](#available-commands)

---

## Architecture Overview

POT follows a **clean architecture pattern** with strict layer separation and dependency flow. The backend is designed for security, maintainability, and testability.

### Project Structure

```
Source/Server/
├── Pot.App/               # Application Layer - Business logic, services
│   ├── Calculators/       # Domain calculators (expense/income renewals, accruals)
│   ├── Concerns/          # Cross-cutting application logic (auth, CSV, time, validation, zip)
│   ├── Errors/            # Problem details error definitions
│   ├── Extensions/        # Service extensions and helpers
│   ├── Features/          # Feature-based services (Accounts, Accruals, Approvals, Auth,
│   │                      # Expenses, Incomes, Maintenance, Me, Otp, Projections, Roles, Sites, Users)
│   ├── AppContext.cs      # Application context for current user/site
│   └── DependencyRegistrar.cs  # Marker class for auto-registration
├── Pot.AspNetCore/        # API Layer - HTTP endpoints, handlers, requests/responses
│   ├── Concerns/          # Cross-cutting infrastructure (Auth, Converters, Cors, Email,
│   │                      # ExceptionHandlers, Logging, Middleware, ProblemDetails, RateLimiting, Validation)
│   ├── Extensions/        # Web application builder extensions
│   ├── Features/          # Feature-based endpoints with handlers (Accounts, Accruals, Approvals, Auth,
│   │                      # Expenses, Incomes, Maintenance, Me, Projections, Roles, Sites, Users, Workers)
│   ├── Models/            # Shared API models (PagedRequest, PagedResponse, ResponseBase)
│   ├── Utils/             # API utilities (parameter binding)
│   ├── DependencyRegistrar.cs  # Marker class for auto-registration
│   └── Program.cs         # Application startup and configuration
├── Pot.Data/              # Data Layer - EF Core, entities, repositories
│   ├── Annotations/       # Custom data annotations (AccountBsb, CiText, MediumString, OtpCode, SmallString)
│   ├── Configuration/     # Database configuration
│   ├── Entities/          # Database entities (AccountEntity, ExpenseEntity, IncomeEntity, etc.)
│   ├── Extensions/        # EF Core extensions and query helpers
│   ├── Migrations/        # EF Core migrations (generated)
│   ├── Repositories/      # Data access repositories (Accounts, Expenses, Incomes, Otp,
│   │                      # Projections, Roles, Settings, Sites, Users)
│   ├── Specifications/    # Query specifications - reusable LINQ expressions
│   ├── UnitOfWork/        # Unit of work pattern implementation
│   ├── CurrentUserContext.cs   # Current user context for multi-tenancy
│   ├── DbContextBase.cs   # Base DbContext with common configuration
│   ├── PotDataRegistrar.cs     # Marker class for data layer registration
│   ├── PotDbContext.cs    # Main database context
│   └── PotTransactionFactory.cs  # Database transaction factory
├── Pot.Data.Migrations/   # Migrations Console App - Applies migrations on startup
│   ├── App.cs             # Application logic for migration runner
│   ├── ErdExporter.cs     # ERD export utility
│   ├── NullCurrentUserContext.cs   # No-op user context for migrations
│   ├── PotDbContextFactory.cs      # Factory for design-time operations
│   ├── PotDbMigrator.cs   # Migration execution logic
│   └── Program.cs         # Entry point
├── Pot.EmailSender/       # Email infrastructure
│   ├── Configuration/     # Email configuration
│   ├── Extensions/        # Email sender extensions
│   ├── DependencyRegistrar.cs  # Marker class for auto-registration
│   ├── EmailSender.cs     # Email sender implementation
│   └── SendEmailChannel.cs # Channel-based email queue
├── Pot.RazorComponents/   # Email templates (Razor components)
│   ├── Emails/            # Email template components
│   ├── Extensions/        # Razor rendering extensions
│   ├── Models/            # Email template models
│   ├── wwwroot/           # Static assets for emails
│   ├── PlainTextEmailTemplateLoader.cs  # Plain text email loader
│   └── RazorComponentRenderer.cs        # Razor component to HTML renderer
└── Pot.Shared/            # Shared Layer - Common types, interfaces, extensions
    ├── DependencyInjection/  # Marker interfaces (IPotScopedDependency, IPotSingletonDependency)
    ├── Enumerations/      # Enriched enums (ApprovalStatus, Frequency, OtpReason, OtpStatus,
    │                      # Permission, Role, SettingCategory, UserStatus)
    ├── Extensions/        # Shared extensions (date/time, frequency, service collection)
    ├── ICurrentUserContext.cs  # Interface for current user context
    └── Paging.cs          # Pagination helpers
```

### Key Architectural Principles

**1. Security First**

- **JWT-based authentication** with role-based authorization
- **Endpoint-level permission checks** using `resource:action` pattern (e.g., `account:view`, `expense:manage`)
- **Rate limiting** with differentiated limits (authenticated: 50/30s, anonymous: 15/10s)
- **CORS** configured for trusted origins only
- **Multi-tenancy isolation** via query filters - users can only see their Site's data

**2. Layered Clean Architecture**

- **Dependency Flow**: API → App → Data (outer layers depend on inner, never reversed)
- **Separation of Concerns**: HTTP concerns stay in API layer, business logic in App layer, data access in Data layer
- **Feature-Based Organization**: Code organized by feature (Accounts, Expenses) rather than technical layer

**3. Pre-Validation with Problem Details**

- **FluentValidation** executes before business logic via `IProblemDetailsInspector`
- All validation errors returned as **RFC 7807 Problem Details** for consistent error handling
- **422 Unprocessable Entity** for all validation errors (never 400)

**4. Result Pattern for Business Logic**

- Services return `EnrichedResult<T>` wrapping success/failure outcomes
- Business rule violations convert to Problem Details maintaining consistent error format
- No exceptions for business rule failures (only for truly exceptional conditions)

**5. Minimal API Pattern**

- **Feature-based endpoint registration** using route groups
- **Authorization via `RequireAuthorization()`** at endpoint level with `resource:action` permissions
- **Fluent endpoint configuration** (WithName, WithSummary, ProducesProblem)

**6. Multi-Tenancy by Default**

- **Site-based query filters** automatically isolate tenant data
- Requires explicit `IgnoreQueryFilters()` for cross-site operations
- Prevents accidental data leakage between tenants

**7. Health Checks**

- API and database health monitoring at `/_health` endpoint
- Used by infrastructure monitoring and container orchestration

**8. Explicit Transactions**

- Manual transaction management via `IPotTransactionFactory` when needed
- `SaveChanges` for single operations
- Transactions for multi-step business logic (e.g., updating user roles)

### Design Conventions

**API Design:**

- All public APIs use `RowId` (Guid), never `Id` (int) to prevent leaking internal database identifiers
- Endpoints return typed results: `Results<Ok<Response>, ProblemHttpResult>`
- Include `Etag` in responses for optimistic concurrency control
- HTTP status codes: 200 (OK), 201 (Created), 404 (Not Found), 422 (Validation), 429 (Rate Limit), 500 (Error)

**Code Organization:**

- Features organized by domain (Accounts, Expenses, Incomes)
- Each feature has consistent structure: Handler, Request, Response, RequestValidator, Mappings
- Services named by operation: `ICreateAccountService`, `IUpdateAccountService`, `IDeleteAccountService`
- Repositories named by entity: `IAccountRepository`, `IExpenseRepository`

**Naming Conventions:**

- Entities: `AccountEntity`, `ExpenseEntity` (must end with `Entity` suffix)
- Services: `CreateAccountService`, `UpdateAccountService` (verb + entity pattern)
- Repositories: `AccountRepository`, `ExpenseRepository` (entity + Repository)
- Handlers: `Handler` (static class with `Invoke` method per operation)

**Error Handling:**

- Use `ProblemDetailsErrorFactory` to create standardized errors
- Business rule violations return `EnrichedResult.Fail<T>(error)` from services
- Services log errors before returning failure results
- API handlers convert errors to Problem Details via `ToProblemDetails()` extension

---

## Feature Implementation Recipe

This section provides a step-by-step guide to implementing a new feature from scratch. Follow this recipe to maintain consistency with the existing codebase.

### Step-by-Step Guide

**When to Use This Recipe:**

- Adding a new domain entity (e.g., Budget, Category, Transaction)
- Implementing CRUD operations for existing entities
- Adding new operations to existing features

**What You'll Create:**

1. **API Layer** (Pot.AspNetCore): Endpoints, handlers, requests, responses, validators
2. **Application Layer** (Pot.App): Services, entity checks, models (Input/Output)
3. **Data Layer** (Pot.Data): Entity, repository, specifications
4. **Integration**: Endpoint registration, dependency injection (auto-registered)

### Example Walkthrough

Let's implement a **Create Account** feature step-by-step.

**Prerequisites:**

- Entity created: `AccountEntity` in `Pot.Data/Entities/`
- Repository created: `IAccountRepository` and `AccountRepository` in `Pot.Data/Repositories/Accounts/`
- Migration applied: `dotnet ef migrations add AddAccountEntity --project Pot.Data.Migrations`

**Step 1: Define API Endpoints Constants**

Create endpoint paths in `Pot.AspNetCore/Features/Accounts/AccountsEndpoints.cs`:

```csharp
namespace Pot.AspNetCore.Features.Accounts;

internal static class AccountsEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/accounts";
    public const string Tag = "Accounts Api";

    public const string GetAll = "";
    public const string Get = "/{id:guid}";
    public const string Create = "";
    public const string Update = "/{id:guid}";
    public const string Delete = "/{id:guid}";
}
```

**Step 2: Create API Request Model**

Create `Pot.AspNetCore/Features/Accounts/Create/Request.cs`:

```csharp
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Create;

public sealed class Request
{
    [Description("The account BSB")]
    public string Bsb { get; init; } = string.Empty;

    [Description("The account number")]
    public string Number { get; init; } = string.Empty;

    [Description("A description of the account")]
    public string Description { get; init; } = string.Empty;

    [Description("The account balance")]
    public double Balance { get; init; }

    [Description("The minimum reserved amount")]
    public double Reserved { get; init; }
}
```

**Why Init-Only Properties?**

- Ensures immutability after object construction
- Better for thread safety and reasoning about data flow
- Standard pattern for DTOs in modern C#

**Step 3: Create Request Validator**

Create `Pot.AspNetCore/Features/Accounts/Create/RequestValidator.cs`:

```csharp
using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(account => account.Bsb).IsNotEmpty();
        RuleFor(account => account.Number).IsNotEmpty();
        RuleFor(account => account.Description).IsNotEmpty();
        RuleFor(account => account.Balance).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(account => account.Reserved).IsGreaterThanOrEqualTo(0.0d);
    }
}
```

**Why Inherit from PotValidatorBase?**

- Provides base FluentValidation setup
- Auto-registered via `AddAspNetValidation()` in `Program.cs`
- No need to manually register validators

**Step 4: Create API Response Model**

Create `Pot.AspNetCore/Features/Accounts/Create/Response.cs`:

```csharp
using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Create.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Accounts.Create;

public sealed class Response : ResponseBase
{
    // Static factory method for 201 Created response
    public static CreatedAtRoute<Response> Created(Output account)
    {
        var response = new Response(account);

        return TypedResults.CreatedAtRoute(
            response,
            nameof(Extensions.RouteGroupBuilderExtensions.GetAccount),
            new { Id = response.RowId });
    }

    private Response(Output account)
    {
        _ = account.WhenNotNull();

        RowId = account.RowId;
        Etag = account.Etag;
    }
}
```

**Why CreatedAtRoute?**

- Returns 201 Created with Location header pointing to the new resource
- Follows REST conventions for resource creation
- Location header: `api/accounts/{newAccountRowId}`

**Step 5: Create Request to Input Mapping**

Create `Pot.AspNetCore/Features/Accounts/Create/Mappings/RequestMapping.cs`:

```csharp
using Pot.App.Features.Accounts.Create.Models;

namespace Pot.AspNetCore.Features.Accounts.Create.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            Bsb = request.Bsb,
            Number = request.Number,
            Description = request.Description,
            Balance = request.Balance,
            Reserved = request.Reserved
        };
    }
}
```

**Why Separate Request and Input Models?**

- **Request**: API-specific (HTTP layer concerns, attributes for OpenAPI)
- **Input**: Application-specific (business logic layer, no HTTP dependencies)
- Separation allows API and business logic to evolve independently

**Step 6: Create Handler**

Create `Pot.AspNetCore/Features/Accounts/Create/Handler.cs`:

```csharp
using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Create;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Create.Mappings;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal sealed class Handler
{
    public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(
        Request request,
        ICreateAccountService accountService,
        IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        // Step 1: Validate request
        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);
            return TypedResults.Problem(problemDetails);
        }

        // Step 2: Map request to input
        var accountInput = request.MapToInput();

        // Step 3: Call service
        var accountOutput = await accountService.CreateAccountAsync(accountInput, cancellationToken);

        // Step 4: Return result
        return accountOutput.IsSuccess
            ? Response.Created(accountOutput.Value!)
            : TypedResults.Problem(accountOutput.Error!.ToProblemDetails());
    }
}
```

**Handler Pattern Benefits:**

- Static methods enable minimal API pattern (no controller overhead)
- Dependency injection via method parameters (cleaner than constructor injection)
- Typed results provide compile-time safety for status codes
- Logging at entry and error points for observability

**Step 7: Register Endpoint**

Create `Pot.AspNetCore/Features/Accounts/Extensions/RouteGroupBuilderExtensions.cs`:

```csharp
using System.Net;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder CreateAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AccountsEndpoints.Create, Create.Handler.Invoke)
            .RequireAuthorization("account:manage")
            .WithName(nameof(CreateAccount))
            .WithSummary("Create account")
            .WithDescription("Create new account details")
            .ProducesProblem(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    // Other endpoint methods (GetAccount, UpdateAccount, etc.)
}
```

Create `Pot.AspNetCore/Features/Accounts/Extensions/WebApplicationExtensions.cs`:

```csharp
using Pot.AspNetCore.Concerns.RateLimiting;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddAccountEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Account Routes]"))
        {
            app.Logger.LogInformation("Adding account endpoints");

            app.MapGroup(AccountsEndpoints.Group)
                .WithTags(AccountsEndpoints.Tag)
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .GetAllAccounts()
                .GetAccount()
                .CreateAccount()
                .UpdateAccount()
                .DeleteAccount();
        }

        return app;
    }
}
```

Update `Pot.AspNetCore/Program.cs` to register endpoints:

```csharp
app.UsePotMiddleware()
   .UseScalarOpenApi()
   .AddAuthEndpoints()
   .AddApprovalEndpoints()
   .AddMeEndpoints()
   .AddUserEndpoints()
   .AddRoleEndpoints()
   .AddSiteEndpoints()
   .AddAccountEndpoints()  // <-- Add this line
   // ... other endpoints
```

**Step 8: Create Application Input/Output Models**

Create `Pot.App/Features/Accounts/Create/Models/Input.cs`:

```csharp
namespace Pot.App.Features.Accounts.Create.Models;

public sealed class Input
{
    public Guid? RowId { get; init; }  // Optional for import scenarios
    public string Bsb { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double Balance { get; init; }
    public double Reserved { get; init; }
}
```

Create `Pot.App/Features/Accounts/Create/Models/Output.cs`:

```csharp
namespace Pot.App.Features.Accounts.Create.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
}
```

**Step 9: Create Entity to Output Mapping**

Create `Pot.App/Features/Accounts/Create/Mappings/EntityMapping.cs`:

```csharp
using Pot.App.Features.Accounts.Create.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Create.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this AccountEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag
        };
    }
}
```

**Step 10: Create Entity Checks**

Create `Pot.App/Features/Accounts/Create/EntityChecks/IPreCreateChecker.cs`:

```csharp
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

internal interface IPreCreateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken);
}
```

Create `Pot.App/Features/Accounts/Create/EntityChecks/PreCreateChecker.cs` (Chain of Responsibility coordinator):

```csharp
using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Create.EntityChecks.Checks;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

internal sealed class PreCreateChecker : ChainOfResponsibilityAsyncComposer<InputState, ProblemDetailsError?>, IPreCreateChecker
{
    private readonly ILogger _logger;

    public PreCreateChecker(IEnumerable<IPreCreateCheck> preCheckHandlers, ILogger<PreCreateChecker> logger)
        : base(preCheckHandlers.Cast<PreCreateCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ProblemDetailsError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            AccountToCreate = accountToCreate
        };

        return HandleAsync(state, cancellationToken);
    }
}
```

Create individual checks in `Pot.App/Features/Accounts/Create/EntityChecks/Checks/`:

```csharp
// CheckAccountNumberDoesNotExist.cs
using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

internal sealed class CheckAccountNumberDoesNotExist : PreCreateCheckBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CheckAccountNumberDoesNotExist(IAccountRepository accountRepository, ILogger<CheckAccountNumberDoesNotExist> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = state.AccountToCreate;

        var accountExists = await _accountRepository
            .AccountExistsAsync(account.Bsb, account.Number, cancellationToken)
            .ConfigureAwait(false);

        if (accountExists)
        {
            return ProblemDetailsErrorFactory.CreateEntityExistsError(
                "Account",
                $"{nameof(AccountEntity.Bsb)}, {nameof(AccountEntity.Number)}",
                $"{account.Bsb}, {account.Number}");
        }

        // Call next handler in chain
        return await base.HandleAsync(state, cancellationToken);
    }
}

// CheckDescriptionDoesNotExist.cs (similar pattern)
```

**Why Chain of Responsibility for Entity Checks?**

- Each check is a separate, testable unit
- Easy to add/remove checks without modifying core logic
- Clear separation of business rules
- Stops at first failure (short-circuit evaluation)

**Step 11: Create Service Interface**

Create `Pot.App/Features/Accounts/Create/ICreateAccountService.cs`:

```csharp
using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.Create.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create;

public interface ICreateAccountService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken);
}
```

**Step 12: Create Service Implementation**

Create `Pot.App/Features/Accounts/Create/CreateAccountService.cs`:

```csharp
using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Accounts.Create.EntityChecks;
using Pot.App.Features.Accounts.Create.Mappings;
using Pot.App.Features.Accounts.Create.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Sites;

namespace Pot.App.Features.Accounts.Create;

internal sealed class CreateAccountService : ICreateAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ILogger _logger;

    public CreateAccountService(
        IPersistableAccountRepository accountRepository,
        ISiteRepository siteRepository,
        IPreCreateChecker preCreateChecker,
        ILogger<CreateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _siteRepository = siteRepository.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // Enable tracking for write operations
        using (_accountRepository.WithTracking())
        {
            var userSite = _siteRepository.GetCurrentSite();

            var accountToCreate = new AccountEntity
            {
                Site = userSite,
                Bsb = input.Bsb,
                Number = input.Number,
                Description = input.Description,
                Balance = input.Balance,
                Reserved = input.Reserved
            };

            // Optional RowId for import scenarios
            if (input.RowId.HasValue)
            {
                accountToCreate.RowId = input.RowId.Value;
            }

            // Run entity checks (business rule validation)
            var problemDetails = await _preCreateChecker.CanSaveAsync(accountToCreate, cancellationToken);

            if (problemDetails is not null)
            {
                return EnrichedResult.Fail<Output>(problemDetails);
            }

            // Save to database
            await _accountRepository
                .AddAndSaveAsync(accountToCreate, cancellationToken)
                .ConfigureAwait(false);

            // Map entity to output
            var createdAccount = accountToCreate.MapToOutput();

            return EnrichedResult.Success(createdAccount);
        }
    }
}
```

**Service Pattern Benefits:**

- Clear transaction boundaries (`WithTracking()`)
- Separation of validation (entity checks) from persistence
- Testable business logic (can mock repositories)
- Consistent error handling via `EnrichedResult`

**Step 13: Verify Auto-Registration**

No manual registration needed! The service and validator are automatically registered via:

- **Service**: Implements `IPotScopedDependency` → Auto-registered as scoped
- **Validator**: Inherits from `PotValidatorBase<Request>` → Auto-registered
- **Entity Checks**: Implement `IPreCreateCheck` and `IPotScopedDependency` → Auto-registered

Verify in `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs`:

```csharp
builder.Services
    .AutoRegisterScoped<DependencyRegistrar, IPotScopedDependency>(/* filters */)
    .AddAppDependencies();
```

**Step 14: Test the Endpoint**

**Using Scalar (Development):**

1. Run the application: `dotnet run --project Pot.AspNetCore`
2. Navigate to: `https://localhost:7241/scalar/v1`
3. Find "Accounts Api" → "Create account"
4. Execute test request

**Using Postman:**

```http
POST https://localhost:7241/api/accounts
Content-Type: application/json
Authorization: Bearer {your-jwt-token}

{
  "bsb": "123-456",
  "number": "12345678",
  "description": "Savings Account",
  "balance": 5000.00,
  "reserved": 500.00
}
```

**Expected Response (201 Created):**

```json
{
  "rowId": "550e8400-e29b-41d4-a716-446655440000",
  "etag": 1732800000
}
```

**Validation Error Response (422 Unprocessable Entity):**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.23",
  "title": "Unprocessable Entity",
  "status": 422,
  "errors": {
    "Bsb": ["'Bsb' must not be empty."],
    "Number": ["'Number' must not be empty."]
  },
  "correlationId": "8e9f7c6d-5b4a-3f2e-1d0c-9a8b7c6d5e4f"
}
```

**Business Rule Violation (422 Unprocessable Entity):**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.23",
  "title": "Conflict",
  "status": 422,
  "detail": "The operation would conflict with another Account entity.",
  "errors": {
    "Bsb, Number": ["123-456, 12345678"]
  }
}
```

## Feature Organization

POT uses a **feature-based architecture** where code is organized by domain feature rather than technical layer. This improves discoverability, maintainability, and enables feature teams to work independently.

### API Layer (Pot.AspNetCore)

**Location:** `Source/Server/Pot.AspNetCore/`

**Purpose:** HTTP endpoint definitions, request/response models, validation, and routing.

**Structure:**

```
Pot.AspNetCore/
├── Features/
│   ├── Accounts/
│   │   ├── AccountsEndpoints.cs           # Endpoint path constants
│   │   ├── Create/
│   │   │   ├── Handler.cs                 # Static handler with Invoke method
│   │   │   ├── Request.cs                 # HTTP request model
│   │   │   ├── RequestValidator.cs        # FluentValidation validator
│   │   │   ├── Response.cs                # HTTP response model
│   │   │   └── Mappings/
│   │   │       └── RequestMapping.cs      # Request → Input mapping
│   │   ├── Update/                        # Similar structure
│   │   ├── Delete/                        # Similar structure
│   │   ├── Get/                           # Similar structure
│   │   ├── GetAll/                        # Similar structure
│   │   └── Extensions/
│   │       ├── RouteGroupBuilderExtensions.cs    # Endpoint registration
│   │       └── WebApplicationExtensions.cs       # Feature registration
│   ├── Expenses/                          # Similar structure to Accounts
│   ├── Incomes/                           # Similar structure
│   └── ...
├── Concerns/                              # Cross-cutting infrastructure
│   ├── Auth/                              # JWT, permissions, policies
│   ├── Validation/                        # FluentValidation setup
│   ├── Cors/                              # CORS configuration
│   ├── RateLimiting/                      # Rate limiting policies
│   ├── Middleware/                        # Custom middleware
│   └── ProblemDetails/                    # Problem details customization
├── Models/                                # Shared API models
│   ├── ResponseBase.cs                    # Base response with RowId, Etag
│   └── PagedResponse.cs                   # Paged collection response
└── Program.cs                             # Application startup
```

**Key Components:**

- **Handlers**: Static classes with `Invoke` method for each endpoint operation
- **Requests/Responses**: DTOs for HTTP communication (immutable with init-only properties)
- **Validators**: FluentValidation validators inheriting from `PotValidatorBase<T>`
- **Mappings**: Extension methods for Request → Input transformation
- **Extensions**: Fluent endpoint registration methods

### Application Layer (Pot.App)

**Location:** `Source/Server/Pot.App/`

**Purpose:** Business logic, domain services, entity validation, and application workflows.

**Structure:**

```
Pot.App/
├── Features/
│   ├── Accounts/
│   │   ├── Create/
│   │   │   ├── ICreateAccountService.cs        # Service interface
│   │   │   ├── CreateAccountService.cs         # Service implementation
│   │   │   ├── Models/
│   │   │   │   ├── Input.cs                    # Service input model
│   │   │   │   └── Output.cs                   # Service output model
│   │   │   ├── Mappings/
│   │   │   │   └── EntityMapping.cs            # Entity → Output mapping
│   │   │   └── EntityChecks/
│   │   │       ├── IPreCreateChecker.cs        # Checker interface
│   │   │       ├── PreCreateChecker.cs         # Chain of responsibility coordinator
│   │   │       ├── InputState.cs               # State object for chain
│   │   │       └── Checks/
│   │   │           ├── PreCreateCheckBase.cs   # Base class for checks
│   │   │           ├── CheckAccountNumberDoesNotExist.cs
│   │   │           └── CheckDescriptionDoesNotExist.cs
│   │   ├── Update/                             # Similar structure
│   │   └── ...
│   ├── Expenses/                               # Similar structure
│   └── ...
├── Concerns/                                   # Cross-cutting application logic
│   ├── Validation/                             # Application-level validation
│   ├── Time/                                   # Time provider abstraction
│   └── ...
├── Errors/                                     # Problem details error definitions
│   ├── ProblemDetailsError.cs                  # Error model
│   ├── ProblemDetailsErrorFactory.cs           # Error creation factory
│   └── ErrorCodes.cs                           # Error code constants
├── Calculators/                                # Domain calculators
│   ├── Accruals/                               # Expense accrual calculations
│   └── Projections/                            # Financial projection calculations
└── DependencyRegistrar.cs                      # Auto-registration marker class
```

**Key Components:**

- **Services**: Business logic implementation returning `EnrichedResult<T>`
- **Entity Checks**: Chain of responsibility pattern for business rule validation
- **Input/Output Models**: Application-specific DTOs independent of HTTP layer
- **Mappings**: Extension methods for Entity → Output transformation
- **Calculators**: Complex domain calculations (projections, accruals)

**Service Pattern:**

```csharp
public async Task<EnrichedResult<Output>> OperationAsync(Input input, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    using (_repository.WithTracking())
    {
        // 1. Load entity
        var entity = await _repository.GetAsync(input.Id, cancellationToken);

        // 2. Run entity checks (business rules)
        var problemDetails = await _entityChecker.CanSaveAsync(entity, cancellationToken);
        if (problemDetails is not null)
        {
            return EnrichedResult.Fail<Output>(problemDetails);
        }

        // 3. Perform operation
        // ... business logic ...

        // 4. Save changes
        await _repository.SaveAsync(cancellationToken);

        // 5. Map and return result
        var output = entity.MapToOutput();
        return EnrichedResult.Success(output);
    }
}
```

### Data Layer (Pot.Data)

**Location:** `Source/Server/Pot.Data/`

**Purpose:** Data access, entity definitions, repositories, and database context.

**Structure:**

```
Pot.Data/
├── Entities/
│   ├── EntityBase.cs                      # Base entity (Id, RowId, Etag)
│   ├── AccountEntity.cs                   # Account entity
│   ├── ExpenseEntity.cs                   # Expense entity
│   ├── IncomeEntity.cs                    # Income entity
│   ├── UserEntity.cs                      # User entity
│   └── ...
├── Repositories/
│   ├── IRepositoryBase.cs                 # Base repository interface
│   ├── RepositoryBase.cs                  # Base repository implementation
│   ├── IPersistableRepository.cs          # Persistable repository interface
│   ├── PersistableRepository.cs           # Persistable repository implementation
│   ├── Accounts/
│   │   ├── IAccountRepository.cs          # Account repository interface
│   │   ├── IPersistableAccountRepository.cs   # Persistable account repository interface
│   │   ├── AccountRepository.cs           # Account repository implementation
│   │   ├── Dtos/                          # Repository-specific DTOs
│   │   └── Specifications/                # Account query specifications
│   ├── Expenses/                          # Similar structure
│   └── ...
├── Specifications/
│   ├── EntitySpecifications.cs            # Generic entity specifications
│   └── AccountSpecifications.cs           # Account-specific specifications
├── Configuration/
│   └── DatabaseConfiguration.cs           # Database connection configuration
├── Extensions/
│   ├── DbContextExtensions.cs             # DbContext extension methods
│   └── QueryableExtensions.cs             # IQueryable extension methods
├── Migrations/                            # EF Core migrations
├── UnitOfWork/
│   ├── IUnitOfWork.cs                     # Unit of work interface
│   └── UnitOfWork.cs                      # Unit of work implementation
├── DbContextBase.cs                       # Base DbContext with conventions
├── PotDbContext.cs                        # Application DbContext
├── CurrentUserContext.cs                  # Current user context implementation
├── IPotTransactionFactory.cs              # Transaction factory interface
└── PotTransactionFactory.cs               # Transaction factory implementation
```

**Key Components:**

- **Entities**: Database entities inheriting from `EntityBase` (must end with `Entity` suffix)
- **Repositories**: Data access abstractions with distinct read-only and read-write interfaces
  - `IRepositoryBase` - Read-only base interface for queries
  - `IPersistableRepository` - Extends `IRepositoryBase` with write operations (Add, Update, Delete, Save)
  - Feature repositories follow same pattern: `IAccountRepository` (read-only) → `IPersistableAccountRepository` (read-write)
- **Specifications**: Reusable LINQ expressions for common queries
- **DbContext**: Entity configuration, query filters, and change tracking setup

**Repository Pattern:**

```csharp
// Read-only interface (queries only)
public interface IAccountRepository : IRepositoryBase
{
    IQueryable<AccountEntity> Accounts { get; }

    Task<AccountEntity> GetAccountAsync(Guid rowId, CancellationToken cancellationToken);
    Task<bool> AccountExistsAsync(Guid rowId, CancellationToken cancellationToken);
    // ... other query methods
}

// Persistable interface (queries + write operations)
public interface IPersistableAccountRepository : IAccountRepository, IPersistableRepository
{
    // Inherits read operations from IAccountRepository
    // Inherits write operations (Add, Update, Delete, Save) from IPersistableRepository
}
```

**⚠️ Critical:** When using persistable repositories, always wrap write operations in `WithTracking()` or changes will not be saved to the database.

### Shared Layer (Pot.Shared)

**Location:** `Source/Server/Pot.Shared/`

**Purpose:** Shared types, enumerations, and interfaces used across all layers.

**Structure:**

```
Pot.Shared/
├── Enumerations/
│   ├── Frequency.cs                       # Enriched enum for frequency
│   ├── UserStatus.cs                      # Enriched enum for user status
│   ├── ApprovalStatus.cs                  # Enriched enum for approval status
│   └── ...
├── Extensions/
│   ├── FrequencyExtensions.cs             # Frequency utility methods
│   ├── DateTimeExtensions.cs              # DateTime utility methods
│   └── ...
├── DependencyInjection/
│   ├── IPotScopedDependency.cs            # Marker for scoped services
│   └── IPotSingletonDependency.cs         # Marker for singleton services
├── ICurrentUserContext.cs                 # Current user context interface
└── Paging.cs                              # Pagination models
```

**Key Components:**

- **Enriched Enums**: Type-safe enum pattern with additional functionality
- **Marker Interfaces**: Auto-registration markers (`IPotScopedDependency`, `IPotSingletonDependency`)
- **Shared Extensions**: Utility methods used across layers

---

## Endpoint Patterns

POT uses a **minimal API pattern** with static handlers and feature-based organization. This section explains the consistent patterns for implementing endpoints.

### Handler Pattern

**Location:** `Pot.AspNetCore/Features/{Feature}/{Operation}/Handler.cs`

**Pattern:**

```csharp
internal sealed class Handler
{
    public static async Task<Results<TSuccess, ProblemHttpResult>> Invoke(
        Request request,
        IService service,
        IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        // Step 1: Validate request (if applicable)
        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);
            return TypedResults.Problem(problemDetails);
        }

        // Step 2: Map request to input (if applicable)
        var input = request.MapToInput();

        // Step 3: Call service
        var result = await service.OperationAsync(input, cancellationToken);

        // Step 4: Return result
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)  // or Response.Created(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}
```

**Key Characteristics:**

- **Static Method**: Enables minimal API registration without controller overhead
- **Dependency Injection via Parameters**: Framework injects dependencies automatically
- **Typed Results**: `Results<TSuccess, ProblemHttpResult>` provides compile-time safety
- **Logging**: Entry point logging via `logger.LogCall(null)` and error logging
- **Validation First**: FluentValidation runs before business logic
- **Result Pattern**: Services return `EnrichedResult<T>` for success/failure

**Handler Variations:**

**1. Create Handler (POST - Returns 201 Created):**

```csharp
public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(...)
{
    // ... validation and mapping ...

    var result = await service.CreateAsync(input, cancellationToken);

    return result.IsSuccess
        ? Response.Created(result.Value!)  // Static factory method
        : TypedResults.Problem(result.Error!.ToProblemDetails());
}
```

**2. GetAll Handler (GET - No Request Body):**

```csharp
public static async Task<Results<Ok<PagedResponse<Response>>, ProblemHttpResult>> Invoke(
    Request request,  // Query parameters from route
    IService service,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    var result = await service.GetAllAsync(request.Paging, cancellationToken);

    return Response.Ok(result);  // PagedResponse factory
}
```

**3. Get Handler (GET - Route Parameter):**

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(
    Guid id,  // Route parameter
    IService service,
    IProblemDetailsInspector problemDetailsInspector,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    // Validate route parameter
    var request = new Request { Id = id };
    var problemDetails = problemDetailsInspector.Validate(request);

    if (problemDetails.IsProblem())
    {
        logger.LogErrors(problemDetails);
        return TypedResults.Problem(problemDetails);
    }

    var result = await service.GetAsync(id, cancellationToken);

    return result.IsSuccess
        ? TypedResults.Ok(result.Value)
        : TypedResults.Problem(result.Error!.ToProblemDetails());
}
```

**4. Update Handler (PUT - Route Parameter + Body):**

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(
    Guid id,  // Route parameter
    Request request,  // Request body
    IService service,
    IProblemDetailsInspector problemDetailsInspector,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    // Validate request
    var problemDetails = problemDetailsInspector.Validate(request);

    if (problemDetails.IsProblem())
    {
        logger.LogErrors(problemDetails);
        return TypedResults.Problem(problemDetails);
    }

    var input = request.MapToInput(id);  // Include route parameter in mapping

    var result = await service.UpdateAsync(input, cancellationToken);

    return result.IsSuccess
        ? TypedResults.Ok(result.Value)
        : TypedResults.Problem(result.Error!.ToProblemDetails());
}
```

**5. Delete Handler (DELETE - Route Parameter Only):**

```csharp
public static async Task<Results<Ok, ProblemHttpResult>> Invoke(
    Guid id,
    IService service,
    IProblemDetailsInspector problemDetailsInspector,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    var request = new Request { Id = id };
    var problemDetails = problemDetailsInspector.Validate(request);

    if (problemDetails.IsProblem())
    {
        logger.LogErrors(problemDetails);
        return TypedResults.Problem(problemDetails);
    }

    var result = await service.DeleteAsync(id, cancellationToken);

    return result.IsSuccess
        ? TypedResults.Ok()
        : TypedResults.Problem(result.Error!.ToProblemDetails());
}
```

**6. Action Handler (POST - No Response Body):**

```csharp
public static async Task<Results<Ok, ProblemHttpResult>> Invoke(
    Guid id,
    IService service,
    IProblemDetailsInspector problemDetailsInspector,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    var request = new Request { Id = id };
    var problemDetails = problemDetailsInspector.Validate(request);

    if (problemDetails.IsProblem())
    {
        logger.LogErrors(problemDetails);
        return TypedResults.Problem(problemDetails);
    }

    var result = await service.PerformActionAsync(id, cancellationToken);

    return result.IsSuccess
        ? TypedResults.Ok()
        : TypedResults.Problem(result.Error!.ToProblemDetails());
}
```

### Request Validation

**Location:** `Pot.AspNetCore/Features/{Feature}/{Operation}/RequestValidator.cs`

**Pattern:**

```csharp
using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(x => x.Bsb).IsNotEmpty().MaximumLength(7);
        RuleFor(x => x.Number).IsNotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).IsNotEmpty().MaximumLength(255);
        RuleFor(x => x.Balance).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(x => x.Reserved).IsGreaterThanOrEqualTo(0.0d);
    }
}
```

**Why Inherit from PotValidatorBase?**

- Provides base FluentValidation setup
- Auto-registered via `AddAspNetValidation()` in `Program.cs`
- Consistent validation behavior across all validators

**Validation with Context:**

Some validations require additional context (e.g., comparing two dates):

```csharp
internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(x => x.Description).IsNotEmpty();
        RuleFor(x => x.NextDue).IsNotEmpty();
        RuleFor(x => x.EndDate).IsNotEmpty();

        // Validation requiring context
        RuleFor(x => x.EndDate)
            .Must((request, endDate, context) =>
            {
                var validationContext = (RequestValidationContext)context;
                return validationContext.EndDate is null || validationContext.EndDate >= validationContext.NextDue;
            })
            .WithMessage("'End Date' must be greater than or equal to 'Next Due'.")
            .When(x => x.EndDate.HasValue);
    }
}

// In Handler:
var validationContext = new RequestValidationContext
{
    NextDue = request.NextDue,
    EndDate = request.EndDate,
    Frequency = request.Frequency
};

var problemDetails = problemDetailsInspector.Validate(request, validationContext);
```

**Common Validation Rules:**

```csharp
// Required fields
RuleFor(x => x.Name).IsNotEmpty();

// String length
RuleFor(x => x.Description).MaximumLength(255);

// Numeric range
RuleFor(x => x.Amount).IsGreaterThanOrEqualTo(0.0d);
RuleFor(x => x.Quantity).IsGreaterThan(0);

// Guid validation
RuleFor(x => x.Id).IsNotEmpty();

// Email validation
RuleFor(x => x.Email).IsEmailAddress();

// Enum validation
RuleFor(x => x.Frequency).IsInEnum();

// Collection validation
RuleFor(x => x.Items).IsNotEmpty();
RuleFor(x => x.Items).Must(x => x.Count > 0);

// Conditional validation
RuleFor(x => x.EndDate).IsNotEmpty().When(x => x.Frequency != Frequency.OneTime);

// Custom validation
RuleFor(x => x.Bsb).Must(BeValidBsb).WithMessage("BSB must be in format XXX-XXX");

private bool BeValidBsb(string bsb)
{
    return System.Text.RegularExpressions.Regex.IsMatch(bsb, @"^\d{3}-\d{3}$");
}
```

### Response Construction

**Location:** `Pot.AspNetCore/Features/{Feature}/{Operation}/Response.cs`

**Pattern:**

```csharp
using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Create.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Accounts.Create;

public sealed class Response : ResponseBase
{
    // Static factory method for typed result
    public static CreatedAtRoute<Response> Created(Output account)
    {
        var response = new Response(account);

        return TypedResults.CreatedAtRoute(
            response,
            nameof(Extensions.RouteGroupBuilderExtensions.GetAccount),
            new { Id = response.RowId });
    }

    // Private constructor - only creatable via factory
    private Response(Output account)
    {
        _ = account.WhenNotNull();

        RowId = account.RowId;
        Etag = account.Etag;
    }
}
```

**Response Patterns:**

**1. Create Response (201 Created with Location header):**

```csharp
public static CreatedAtRoute<Response> Created(Output output)
{
    var response = new Response(output);

    return TypedResults.CreatedAtRoute(
        response,
        nameof(Extensions.RouteGroupBuilderExtensions.GetAccount),
        new { Id = response.RowId });
}
```

**2. OK Response (200 OK with body):**

```csharp
public static Ok<Response> Ok(Output output)
{
    var response = new Response(output);
    return TypedResults.Ok(response);
}
```

**3. Paged Response (200 OK with pagination):**

```csharp
public static Ok<PagedResponse<Response>> Ok(PagedResult<Output> pagedResult)
{
    var response = PagedResponse<Response>.FromPagedResult(
        pagedResult,
        output => new Response(output));

    return TypedResults.Ok(response);
}
```

**4. No Content Response (200 OK no body):**

```csharp
public static Ok Ok()
{
    return TypedResults.Ok();
}
```

**Why ResponseBase?**

All responses inherit from `ResponseBase` to ensure consistent `RowId` and `Etag` properties:

```csharp
public abstract class ResponseBase
{
    public Guid RowId { get; init; }
    public long Etag { get; init; }
}
```

### Endpoint Registration

**Location:** `Pot.AspNetCore/Features/{Feature}/Extensions/RouteGroupBuilderExtensions.cs`

**Pattern:**

```csharp
using System.Net;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder CreateAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AccountsEndpoints.Create, Create.Handler.Invoke)
            .RequireAuthorization("account:manage")
            .WithName(nameof(CreateAccount))
            .WithSummary("Create account")
            .WithDescription("Create new account details")
            .ProducesProblem(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder GetAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(AccountsEndpoints.Get, Get.Handler.Invoke)
            .RequireAuthorization("account:view")
            .WithName(nameof(GetAccount))
            .WithSummary("Get account")
            .WithDescription("Get details for an existing account")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    // Other endpoint methods...
}
```

**Endpoint Configuration Explanation:**

- **`MapPost/MapGet/MapPut/MapDelete`**: HTTP verb and path
- **`RequireAuthorization("resource:action")`**: Permission required (`account:view`, `account:manage`, etc.)
- **`WithName`**: Named route for `CreatedAtRoute` references
- **`WithSummary`**: Short description for OpenAPI documentation
- **`WithDescription`**: Detailed description for OpenAPI documentation
- **`ProducesProblem`**: Expected status codes for OpenAPI documentation

**Feature Registration:**

**Location:** `Pot.AspNetCore/Features/{Feature}/Extensions/WebApplicationExtensions.cs`

```csharp
using Pot.AspNetCore.Concerns.RateLimiting;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddAccountEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Account Routes]"))
        {
            app.Logger.LogInformation("Adding account endpoints");

            app.MapGroup(AccountsEndpoints.Group)
                .WithTags(AccountsEndpoints.Tag)
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .GetAllAccounts()
                .GetAccount()
                .CreateAccount()
                .UpdateAccount()
                .DeleteAccount();
        }

        return app;
    }
}
```

**Route Group Configuration:**

- **`MapGroup`**: Groups endpoints under common base path (`api/accounts`)
- **`WithTags`**: Groups endpoints in OpenAPI documentation
- **`RequireRateLimiting`**: Applies rate limiting policy to all endpoints in group
- **Fluent Methods**: Chained extension methods for each endpoint

**Program.cs Integration:**

```csharp
app.UsePotMiddleware()
   .UseScalarOpenApi()
   .AddAuthEndpoints()
   .AddAccountEndpoints()  // <-- Register feature endpoints
   .AddExpenseEndpoints()
   // ... other features
```

---

## Service Layer

The service layer contains all business logic and orchestrates operations between the API and data layers. Services return `EnrichedResult<T>` to indicate success or failure without throwing exceptions for business rule violations.

### Service Pattern

**Location:** `Pot.App/Features/{Feature}/{Operation}/{Operation}{Entity}Service.cs`

**Interface Pattern:**

```csharp
using AllOverIt.Patterns.Result;
using Pot.App.Features.Accounts.Create.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create;

public interface ICreateAccountService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken);
}
```

**Why `IPotScopedDependency`?**

- Marker interface for auto-registration
- Services are scoped by default (new instance per HTTP request)
- No manual registration required in `Program.cs`

**Implementation Pattern:**

```csharp
using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Accounts.Create.EntityChecks;
using Pot.App.Features.Accounts.Create.Mappings;
using Pot.App.Features.Accounts.Create.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Sites;

namespace Pot.App.Features.Accounts.Create;

internal sealed class CreateAccountService : ICreateAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ILogger _logger;

    public CreateAccountService(
        IPersistableAccountRepository accountRepository,
        ISiteRepository siteRepository,
        IPreCreateChecker preCreateChecker,
        ILogger<CreateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _siteRepository = siteRepository.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // Step 1: Enable tracking for write operations
        using (_accountRepository.WithTracking())
        {
            // Step 2: Get current user's site (for multi-tenancy)
            var userSite = _siteRepository.GetCurrentSite();

            // Step 3: Create entity from input
            var accountToCreate = new AccountEntity
            {
                Site = userSite,
                Bsb = input.Bsb,
                Number = input.Number,
                Description = input.Description,
                Balance = input.Balance,
                Reserved = input.Reserved
            };

            // Optional RowId for import scenarios
            if (input.RowId.HasValue)
            {
                accountToCreate.RowId = input.RowId.Value;
            }

            // Step 4: Run entity checks (business rule validation)
            var problemDetails = await _preCreateChecker.CanSaveAsync(accountToCreate, cancellationToken);

            if (problemDetails is not null)
            {
                return EnrichedResult.Fail<Output>(problemDetails);
            }

            // Step 5: Save to database
            await _accountRepository
                .AddAndSaveAsync(accountToCreate, cancellationToken)
                .ConfigureAwait(false);

            // Step 6: Map entity to output and return success
            var createdAccount = accountToCreate.MapToOutput();

            return EnrichedResult.Success(createdAccount);
        }
    }
}
```

**Service Pattern Key Elements:**

1. **Constructor Dependency Injection**: All dependencies validated with `.WhenNotNull()`
2. **Entry Logging**: `_logger.LogCall(this)` logs method entry with class name
3. **Tracking Scope**: `using (_repository.WithTracking())` enables change tracking for writes
4. **Multi-Tenancy**: Always get current user's site for data isolation
5. **Entity Checks**: Validate business rules before persistence
6. **Error Handling**: Return `EnrichedResult.Fail<T>()` for business rule violations
7. **ConfigureAwait(false)**: Prevents deadlocks in async operations
8. **Success Result**: Return `EnrichedResult.Success<T>()` with mapped output

**Service Operation Patterns:**

**1. Create Service:**

```csharp
public async Task<EnrichedResult<Output>> CreateAsync(Input input, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    using (_repository.WithTracking())
    {
        var entity = new Entity { /* map from input */ };

        var problemDetails = await _preCreateChecker.CanSaveAsync(entity, cancellationToken);
        if (problemDetails is not null)
        {
            return EnrichedResult.Fail<Output>(problemDetails);
        }

        await _repository.AddAndSaveAsync(entity, cancellationToken);

        return EnrichedResult.Success(entity.MapToOutput());
    }
}
```

**2. Update Service:**

```csharp
public async Task<EnrichedResult<Output>> UpdateAsync(Input input, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    using (_repository.WithTracking())
    {
        // Load entity
        var entity = await _repository.GetAsync(input.RowId, cancellationToken);

        if (entity is null)
        {
            var error = ProblemDetailsErrorFactory.CreateEntityNotFoundError(
                input.RowId, "The entity does not exist.");
            return EnrichedResult.Fail<Output>(error);
        }

        // Check Etag for optimistic concurrency
        if (entity.Etag != input.Etag)
        {
            var error = ProblemDetailsErrorFactory.CreateEtagConflict("Entity", input.Etag);
            return EnrichedResult.Fail<Output>(error);
        }

        // Run entity checks
        var problemDetails = await _preUpdateChecker.CanSaveAsync(input, entity, cancellationToken);
        if (problemDetails is not null)
        {
            return EnrichedResult.Fail<Output>(problemDetails);
        }

        // Update entity properties
        entity.Property1 = input.Property1;
        entity.Property2 = input.Property2;

        // Save changes
        await _repository.SaveAsync(cancellationToken);

        return EnrichedResult.Success(entity.MapToOutput());
    }
}
```

**3. Delete Service:**

```csharp
public async Task<EnrichedResult> DeleteAsync(Guid rowId, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    using (_repository.WithTracking())
    {
        var entity = await _repository.GetAsync(rowId, cancellationToken);

        if (entity is null)
        {
            var error = ProblemDetailsErrorFactory.CreateEntityNotFoundError(
                rowId, "The entity does not exist.");
            return EnrichedResult.Fail(error);
        }

        // Check if entity can be deleted
        var problemDetails = await _preDeleteChecker.CanDeleteAsync(entity, cancellationToken);
        if (problemDetails is not null)
        {
            return EnrichedResult.Fail(problemDetails);
        }

        _repository.Delete(entity);
        await _repository.SaveAsync(cancellationToken);

        return EnrichedResult.Success();
    }
}
```

**4. GetAll Service:**

```csharp
public async Task<PagedResult<Output>> GetAllAsync(Paging paging, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    // No tracking needed for read-only operations (default behavior)
    var query = _repository.Entities
        .OrderBy(e => e.Description);

    var pagedResult = await query.ToPagedResultAsync(
        paging,
        entity => entity.MapToOutput(),
        cancellationToken);

    return pagedResult;
}
```

**5. Get Service:**

```csharp
public async Task<EnrichedResult<Output>> GetAsync(Guid rowId, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    var entity = await _repository.GetOrDefaultAsync(rowId, cancellationToken);

    if (entity is null)
    {
        var error = ProblemDetailsErrorFactory.CreateEntityNotFoundError(
            rowId, "The entity does not exist.");
        return EnrichedResult.Fail<Output>(error);
    }

    return EnrichedResult.Success(entity.MapToOutput());
}
```

### Entity Checks

Entity checks validate business rules before persisting data. They use the **Chain of Responsibility pattern** to organize multiple validation rules into separate, testable units.

**Location:** `Pot.App/Features/{Feature}/{Operation}/EntityChecks/`

**Why Chain of Responsibility?**

- Each check is a separate, testable unit
- Easy to add/remove checks without modifying core logic
- Clear separation of business rules
- Stops at first failure (short-circuit evaluation)
- Reusable checks across operations

**Pattern Structure:**

```
EntityChecks/
├── IPreCreateChecker.cs           # Checker interface
├── PreCreateChecker.cs            # Chain coordinator
├── InputState.cs                  # State object passed through chain
├── IPreCreateCheck.cs             # Individual check interface
└── Checks/
    ├── PreCreateCheckBase.cs      # Base class for checks
    ├── CheckAccountNumberDoesNotExist.cs
    └── CheckDescriptionDoesNotExist.cs
```

**1. Define Checker Interface:**

```csharp
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

internal interface IPreCreateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken);
}
```

**2. Create InputState (Context Object):**

```csharp
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

internal sealed class InputState
{
    public required AccountEntity AccountToCreate { get; init; }
}
```

**3. Define Individual Check Interface:**

```csharp
using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

internal interface IPreCreateCheck : IAsyncChainOfResponsibilityHandler<InputState, ProblemDetailsError?>, IPotScopedDependency
{
}
```

**4. Create Base Class for Checks:**

```csharp
using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

internal abstract class PreCreateCheckBase : AsyncChainOfResponsibilityHandlerBase<InputState, ProblemDetailsError?>, IPreCreateCheck
{
}
```

**5. Implement Chain Coordinator:**

```csharp
using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.ChainOfResponsibility;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Create.EntityChecks.Checks;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

internal sealed class PreCreateChecker : ChainOfResponsibilityAsyncComposer<InputState, ProblemDetailsError?>, IPreCreateChecker
{
    private readonly ILogger _logger;

    public PreCreateChecker(IEnumerable<IPreCreateCheck> preCheckHandlers, ILogger<PreCreateChecker> logger)
        : base(preCheckHandlers.Cast<PreCreateCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ProblemDetailsError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var state = new InputState
        {
            AccountToCreate = accountToCreate
        };

        return HandleAsync(state, cancellationToken);
    }
}
```

**6. Implement Individual Checks:**

```csharp
// CheckAccountNumberDoesNotExist.cs
using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

internal sealed class CheckAccountNumberDoesNotExist : PreCreateCheckBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CheckAccountNumberDoesNotExist(
        IAccountRepository accountRepository,
        ILogger<CheckAccountNumberDoesNotExist> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public override async Task<ProblemDetailsError?> HandleAsync(
        InputState state,
        CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = state.AccountToCreate;

        var accountExists = await _accountRepository
            .AccountExistsAsync(account.Bsb, account.Number, cancellationToken)
            .ConfigureAwait(false);

        if (accountExists)
        {
            return ProblemDetailsErrorFactory.CreateEntityExistsError(
                "Account",
                $"{nameof(AccountEntity.Bsb)}, {nameof(AccountEntity.Number)}",
                $"{account.Bsb}, {account.Number}");
        }

        // Call next handler in chain (or return null if no more handlers)
        return await base.HandleAsync(state, cancellationToken);
    }
}
```

**Entity Check Guidelines:**

- Each check validates **one business rule**
- Return `ProblemDetailsError` for violations
- Return `null` and call `base.HandleAsync()` to continue chain
- Log entry point for observability
- Use `ConfigureAwait(false)` for async operations
- Keep checks focused and testable

### Result Pattern

POT uses `EnrichedResult<T>` from AllOverIt library to represent operation outcomes without throwing exceptions for business rule violations.

**Why EnrichedResult?**

- Explicit success/failure handling
- Type-safe error information
- No exceptions for expected business rule violations
- Better performance (no stack unwinding)
- Clearer API contracts

**Basic Usage:**

```csharp
// Success result
var result = EnrichedResult.Success(output);

// Failure result
var result = EnrichedResult.Fail<Output>(problemDetailsError);

// Checking result
if (result.IsSuccess)
{
    var value = result.Value;  // Safe to access
}
else
{
    var error = result.Error;  // Safe to access
}
```

**EnrichedResult Properties:**

```csharp
public bool IsSuccess { get; }
public bool IsFailure { get; }
public T? Value { get; }           // Null if IsFailure
public TError? Error { get; }      // Null if IsSuccess
```

**Service Layer Pattern:**

```csharp
public async Task<EnrichedResult<Output>> OperationAsync(Input input, CancellationToken cancellationToken)
{
    // Business rule violation - return failure
    if (someBusinessRuleViolated)
    {
        var error = ProblemDetailsErrorFactory.CreateConflict("PropertyName", attemptedValue, "Error message");
        return EnrichedResult.Fail<Output>(error);
    }

    // Entity not found - return failure
    if (entity is null)
    {
        var error = ProblemDetailsErrorFactory.CreateEntityNotFoundError(id, "Entity does not exist");
        return EnrichedResult.Fail<Output>(error);
    }

    // Operation succeeded - return success
    var output = entity.MapToOutput();
    return EnrichedResult.Success(output);
}
```

**Handler Layer Pattern:**

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(...)
{
    var result = await service.OperationAsync(input, cancellationToken);

    return result.IsSuccess
        ? TypedResults.Ok(result.Value)  // HTTP 200
        : TypedResults.Problem(result.Error!.ToProblemDetails());  // HTTP 422/404/409
}
```

**ProblemDetailsError Structure:**

```csharp
public class ProblemDetailsError : ProblemDetailsBasicError
{
    public string PropertyName { get; init; } = string.Empty;
    public object? AttemptedValue { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public ProblemType ProblemType { get; init; }  // NotFound, Conflict, Constraint, etc.
}
```

**Common Error Factory Methods:**

```csharp
// Entity not found (404)
var error = ProblemDetailsErrorFactory.CreateEntityNotFoundError(
    attemptedValue: rowId,
    errorMessage: "The account does not exist.");

// Entity exists conflict (422)
var error = ProblemDetailsErrorFactory.CreateEntityExistsError(
    entityType: "Account",
    propertyName: "Bsb, Number",
    attemptedValue: $"{bsb}, {number}");

// Entity constraint violation (422)
var error = ProblemDetailsErrorFactory.CreateEntityConstraintError(
    propertyName: "AccountId",
    attemptedValue: accountId,
    errorMessage: "Cannot delete account with linked expenses.");

// ETag conflict (409)
var error = ProblemDetailsErrorFactory.CreateEtagConflict(
    entityType: "Account",
    attemptedValue: attemptedEtag);

// Generic unprocessable entity (422)
var error = ProblemDetailsErrorFactory.CreateUnprocessableEntityError(
    errorMessage: "The operation cannot be completed.");

// With property name (422)
var error = ProblemDetailsErrorFactory.CreateUnprocessableEntityError(
    propertyName: "Balance",
    attemptedValue: -100,
    errorMessage: "Balance cannot be negative.");
```

### Transaction Management

Most operations use a single `SaveChanges` call within a tracking scope. For operations that require multiple coordinated database changes, use explicit transactions via `IPotTransactionFactory`.

**When to Use Transactions:**

- Multiple entity updates that must succeed or fail together
- Updates spanning multiple repositories
- Complex business operations with multiple database writes
- When rollback is needed if any step fails

**Pattern:**

```csharp
public async Task<EnrichedResult<Output>> UpdateUserRolesAsync(Input input, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    using (_userRepository.WithTracking())
    {
        // Begin transaction
        using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);

        // Step 1: Load user with roles
        var user = await _userRepository.Users
            .Include(u => u.Roles)
            .SingleOrDefaultAsync(u => u.RowId == input.RowId, cancellationToken);

        if (user is null)
        {
            var error = ProblemDetailsErrorFactory.CreateEntityNotFoundError(
                input.RowId, "The user does not exist.");
            return EnrichedResult.Fail<Output>(error);
        }

        // Step 2: Run entity checks
        var problemDetails = await _preUpdateChecker.CanSaveAsync(input, user, cancellationToken);
        if (problemDetails is not null)
        {
            return EnrichedResult.Fail<Output>(problemDetails);
        }

        // Step 3: Load new roles
        var roles = await _roleRepository.GetRolesAsync(input.RoleIds, cancellationToken);

        // Step 4: Update user roles
        user.Roles = roles;

        // Step 5: Force etag update
        var userEntry = _userRepository.GetEntry(user);
        userEntry.State = EntityState.Modified;

        // Step 6: Save changes (within transaction)
        await _userRepository.SaveAsync(cancellationToken);

        // Step 7: Commit transaction
        await transaction.CommitAsync(cancellationToken);

        return EnrichedResult.Success(user.MapToOutput());
    }
}
```

**Transaction Guidelines:**

- Use `using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken)`
- Always commit explicitly: `await transaction.CommitAsync(cancellationToken)`
- Transaction auto-rolls back if not committed (on dispose)
- Keep transaction scope as small as possible
- Perform validation **before** starting transaction when possible

**Simple Operations (No Transaction Needed):**

```csharp
public async Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    using (_accountRepository.WithTracking())
    {
        var account = new AccountEntity { /* ... */ };

        var problemDetails = await _preCreateChecker.CanSaveAsync(account, cancellationToken);
        if (problemDetails is not null)
        {
            return EnrichedResult.Fail<Output>(problemDetails);
        }

        // Single operation - no transaction needed
        await _accountRepository.AddAndSaveAsync(account, cancellationToken);

        return EnrichedResult.Success(account.MapToOutput());
    }
}
```

---

## Repository Layer

Repositories provide data access abstraction and encapsulate all database queries. POT uses a split repository pattern with read-only and persistable interfaces.

### Repository Pattern

**Location:** `Pot.Data/Repositories/{Entity}/`

**Interface Hierarchy:**

```
IRepositoryBase (read-only base)
    ↓
IAccountRepository (read-only account queries)
    ↓
IPersistableAccountRepository (adds write operations)
```

**Base Interfaces:**

```csharp
// Pot.Data/Repositories/IRepositoryBase.cs
public interface IRepositoryBase
{
    IDisposable WithTracking();
    IQueryable<TEntity> Set<TEntity>() where TEntity : EntityBase;
    EntityEntry GetEntry<TEntity>(TEntity entity) where TEntity : EntityBase;
    ValueTask<TEntity?> GetByPrimaryKeyAsync<TEntity, TKey>(TKey id, CancellationToken cancellationToken) where TEntity : EntityBase;
}

// Pot.Data/Repositories/IPersistableRepository.cs
public interface IPersistableRepository : IRepositoryBase
{
    EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : EntityBase;
    EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : EntityBase;
    EntityEntry<TEntity> Delete<TEntity>(TEntity entity) where TEntity : EntityBase;
    int Save();
    Task<int> SaveAsync(CancellationToken cancellationToken);
    Task<int> AddAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : EntityBase;
    Task<int> UpdateAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : EntityBase;
}
```

**Repository Interface Pattern:**

```csharp
// Pot.Data/Repositories/Accounts/IAccountRepository.cs
public interface IAccountRepository : IRepositoryBase
{
    IQueryable<AccountEntity> Accounts { get; }

    Task<bool> AccountExistsAsync(Guid rowId, CancellationToken cancellationToken);
    Task<bool> HasExpensesAsync(Guid rowId, CancellationToken cancellationToken);
    Task<bool> HasIncomesAsync(Guid rowId, CancellationToken cancellationToken);
    Task<AccountEntity> GetAccountAsync(Guid rowId, CancellationToken cancellationToken);
    Task<AccountEntity?> GetAccountOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);
    Task<AccountWithLinkedCounts?> GetAccountWithLinkedCountsOrDefaultAsync(Guid rowId, CancellationToken cancellationToken);
    Task<AccountWithLinkedCounts[]> GetAllAccountsWithLinkedCountsAsync(CancellationToken cancellationToken);
    Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken);
    Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken);
}

// Pot.Data/Repositories/Accounts/IPersistableAccountRepository.cs
public interface IPersistableAccountRepository : IAccountRepository, IPersistableRepository
{
    // Inherits all read operations from IAccountRepository
    // Inherits all write operations from IPersistableRepository
}
```

**Why Split Interfaces?**

- **Interface Segregation Principle (ISP)**: Repositories have distinct read-only and read-write responsibilities
- **Read-only repositories** (`IRepositoryBase`, `IAccountRepository`) prevent accidental data modifications
- **Persistable repositories** (`IPersistableRepository`, `IPersistableAccountRepository`) explicitly support write operations
- **Clear intent**: Services declare their data access requirements through interface dependencies
  - Read-only services depend on `IAccountRepository` (queries only)
  - Write services depend on `IPersistableAccountRepository` (queries + commands)
- **Better testability**: Mock only the operations your service actually needs

**Repository Interface Responsibilities:**

**`IRepositoryBase` (Read-Only Base):**

Provides core query capabilities without write operations:

- `WithTracking()` - Enable change tracking for a scope
- `Set<TEntity>()` - Access DbSet for entity queries
- `GetEntry<TEntity>()` - Get entity entry metadata
- `GetByPrimaryKeyAsync<TEntity, TKey>()` - Retrieve by primary key

**`IPersistableRepository` (Write Operations):**

Extends `IRepositoryBase` with data modification methods:

- `Add<TEntity>()` - Add entity to context
- `Update<TEntity>()` - Update entity in context
- `Delete<TEntity>()` - Delete entity from context
- `Save()` / `SaveAsync()` - Persist changes to database
- `AddAndSaveAsync<TEntity>()` - Add and immediately save
- `UpdateAndSaveAsync<TEntity>()` - Update and immediately save

**⚠️ Critical: WithTracking() Requirement for Persistable Operations**

When using `IPersistableRepository` methods (Add, Update, Delete), **you must wrap operations in `WithTracking()`** or changes will not be saved:

```csharp
// ❌ INCORRECT - Changes will not be saved (no tracking enabled)
var account = await _accountRepository.GetAccountAsync(id, cancellationToken);
account.Description = "Updated";
await _accountRepository.SaveAsync(cancellationToken);

// ✅ CORRECT - Tracking enabled, changes will be saved
using (_accountRepository.WithTracking())
{
    var account = await _accountRepository.GetAccountAsync(id, cancellationToken);
    account.Description = "Updated";
    await _accountRepository.SaveAsync(cancellationToken);
}
```

**Why This Requirement?**

- Default query tracking behavior is `NoTrackingWithIdentityResolution` for performance
- EF Core cannot detect property changes on untracked entities
- `WithTracking()` temporarily enables `TrackAll` behavior for the scope
- Tracking is automatically restored to default when scope is disposed

**Repository Implementation Pattern:**

```csharp
// Pot.Data/Repositories/Accounts/AccountRepository.cs
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Data.Specifications;

namespace Pot.Data.Repositories.Accounts;

internal sealed class AccountRepository : PersistableRepository, IPersistableAccountRepository
{
    public IQueryable<AccountEntity> Accounts => _dbContext.Accounts;

    public AccountRepository(PotDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<bool> AccountExistsAsync(Guid rowId, CancellationToken cancellationToken)
    {
        // Extension method from QueryableExtensions
        return Accounts.AnyAsync(rowId, cancellationToken);
    }

    public Task<bool> HasExpensesAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return Accounts.AnyAsync(
            account => account.RowId == rowId && account.Expenses.Any(),
            cancellationToken);
    }

    public Task<AccountEntity> GetAccountAsync(Guid rowId, CancellationToken cancellationToken)
    {
        // Extension method - throws if not found
        return Accounts.SingleAsync(rowId, cancellationToken);
    }

    public Task<AccountEntity?> GetAccountOrDefaultAsync(Guid rowId, CancellationToken cancellationToken)
    {
        // Using specification for reusable query logic
        return Accounts.SingleOrDefaultAsync(
            EntitySpecifications.IsSameId<AccountEntity>(rowId).Expression,
            cancellationToken);
    }

    public async Task<AccountWithLinkedCounts?> GetAccountWithLinkedCountsOrDefaultAsync(
        Guid rowId,
        CancellationToken cancellationToken)
    {
        // Projection query - returns DTO instead of entity
        return await Accounts
            .Where(account => account.RowId == rowId)
            .Select(item => new AccountWithLinkedCounts
            {
                Account = item,
                LinkedIncomes = item.Incomes.Count,
                LinkedExpenses = item.Expenses.Count
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<AccountWithLinkedCounts[]> GetAllAccountsWithLinkedCountsAsync(
        CancellationToken cancellationToken)
    {
        return Accounts
            .Select(item => new AccountWithLinkedCounts
            {
                Account = item,
                LinkedIncomes = item.Incomes.Count,
                LinkedExpenses = item.Expenses.Count
            })
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> AccountExistsAsync(string bsb, string number, CancellationToken cancellationToken)
    {
        // Global uniqueness check - ignores multi-tenancy filter
        return Accounts
            .IgnoreQueryFilters()
            .AnyAsync(AccountSpecifications.IsSameBsbNumber(bsb, number).Expression, cancellationToken);
    }

    public Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken)
    {
        return Accounts.SingleOrDefaultAsync(
            AccountSpecifications.IsSameBsbNumber(bsb, number).Expression,
            cancellationToken);
    }
}
```

**Repository Method Naming Conventions:**

- `GetAsync()` - Returns entity, throws if not found
- `GetOrDefaultAsync()` - Returns entity or null
- `ExistsAsync()` - Returns bool
- `GetAllAsync()` - Returns collection
- `AddAsync()` - Adds entity (deferred)
- `UpdateAsync()` - Updates entity (deferred)
- `DeleteAsync()` - Deletes entity (deferred)
- `SaveAsync()` - Persists changes
- `AddAndSaveAsync()` - Adds and persists immediately

### Tracking vs No-Tracking

EF Core change tracking determines whether entities are monitored for modifications.

**Default Behavior:** `QueryTrackingBehavior.NoTrackingWithIdentityResolution`

- Queries return untracked entities by default (better performance)
- Identity resolution prevents duplicate instances
- Write operations require explicit tracking

**Why No-Tracking by Default?**

- Better performance for read-only queries
- Reduced memory usage
- Prevents accidental updates
- Explicit tracking for writes makes intent clear

**Enabling Tracking:**

```csharp
public async Task<EnrichedResult<Output>> UpdateAccountAsync(Input input, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    // Enable tracking for this scope
    using (_accountRepository.WithTracking())
    {
        var account = await _accountRepository.GetAccountAsync(input.RowId, cancellationToken);

        // Entity is now tracked - modifications will be detected
        account.Description = input.Description;
        account.Balance = input.Balance;

        await _accountRepository.SaveAsync(cancellationToken);

        return EnrichedResult.Success(account.MapToOutput());
    }
    // Tracking automatically disabled when scope exits
}
```

**How WithTracking() Works:**

```csharp
// Pot.Data/Extensions/DbContextExtensions.cs
public static IDisposable WithAutoTracking(this DbContext context)
{
    return new AutoTrackingScope(context);
}

private sealed class AutoTrackingScope : IDisposable
{
    private readonly DbContext _context;
    private readonly bool _wasTracking;

    public AutoTrackingScope(DbContext context)
    {
        _context = context;
        _wasTracking = _context.ChangeTracker.QueryTrackingBehavior != QueryTrackingBehavior.NoTrackingWithIdentityResolution;

        if (!_wasTracking)
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }
    }

    public void Dispose()
    {
        if (!_wasTracking)
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution;
        }
    }
}
```

**Reference Counting for Nested Scopes:**

```csharp
// Supports nested WithTracking() calls safely
using (_repository.WithTracking())
{
    using (_repository.WithTracking())  // Nested - ref count incremented
    {
        // Both scopes track
    }
    // Inner scope disposed - ref count decremented, still tracking
}
// Outer scope disposed - ref count zero, tracking disabled
```

**When to Use Tracking:**

- ✅ **Use Tracking** for write operations (Create, Update, Delete)
- ✅ **Use Tracking** when loading entities to modify
- ✅ **Use Tracking** when working with navigation properties
- ❌ **Don't Use Tracking** for read-only queries
- ❌ **Don't Use Tracking** for projections (Select to DTOs)
- ❌ **Don't Use Tracking** for GetAll queries

### Query Specifications

Specifications are reusable LINQ expressions that encapsulate query logic. They prevent duplication and improve testability.

**Location:** `Pot.Data/Specifications/`

**Pattern:**

```csharp
// Pot.Data/Specifications/EntitySpecifications.cs
using AllOverIt.Filtering.Specifications;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class EntitySpecifications
{
    public static Specification<TEntity> IsSameId<TEntity>(Guid rowId) where TEntity : EntityBase
    {
        return new Specification<TEntity>(entity => entity.RowId == rowId);
    }

    public static Specification<TEntity> IsSameEtag<TEntity>(long etag) where TEntity : EntityBase
    {
        return new Specification<TEntity>(entity => entity.Etag == etag);
    }
}

// Pot.Data/Repositories/Accounts/Specifications/AccountSpecifications.cs
using AllOverIt.Filtering.Specifications;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class AccountSpecifications
{
    public static Specification<AccountEntity> IsSameBsbNumber(string bsb, string number)
    {
        return new Specification<AccountEntity>(account =>
            account.Bsb == bsb && account.Number == number);
    }

    public static Specification<AccountEntity> HasDescription(string description)
    {
        return new Specification<AccountEntity>(account =>
            account.Description == description);
    }

    public static Specification<AccountEntity> HasMinimumBalance(double minimumBalance)
    {
        return new Specification<AccountEntity>(account =>
            account.Balance >= minimumBalance);
    }
}
```

**Using Specifications:**

```csharp
// In repository
public Task<AccountEntity?> GetAccountOrDefaultAsync(string bsb, string number, CancellationToken cancellationToken)
{
    return Accounts.SingleOrDefaultAsync(
        AccountSpecifications.IsSameBsbNumber(bsb, number).Expression,
        cancellationToken);
}

// In entity check
public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
{
    var account = state.AccountToCreate;

    var exists = await _accountRepository.Accounts
        .AnyAsync(AccountSpecifications.IsSameBsbNumber(account.Bsb, account.Number).Expression, cancellationToken);

    if (exists)
    {
        return ProblemDetailsErrorFactory.CreateEntityExistsError(/* ... */);
    }

    return await base.HandleAsync(state, cancellationToken);
}
```

**Combining Specifications:**

```csharp
// AND combination
var spec = AccountSpecifications.HasMinimumBalance(1000)
    .And(AccountSpecifications.HasDescription("Savings"));

var accounts = await _repository.Accounts
    .Where(spec.Expression)
    .ToListAsync(cancellationToken);

// OR combination
var spec = AccountSpecifications.HasDescription("Savings")
    .Or(AccountSpecifications.HasDescription("Checking"));

var accounts = await _repository.Accounts
    .Where(spec.Expression)
    .ToListAsync(cancellationToken);

// NOT combination
var spec = AccountSpecifications.HasMinimumBalance(0)
    .And(AccountSpecifications.HasDescription("Closed").Not());

var accounts = await _repository.Accounts
    .Where(spec.Expression)
    .ToListAsync(cancellationToken);
```

**Specification Benefits:**

- Reusable query logic
- Testable in isolation
- Type-safe LINQ expressions
- Composable (AND, OR, NOT)
- Self-documenting business rules
- Easier to refactor than inline lambdas

---

## Mapping Patterns

POT uses **explicit mapping** with extension methods to transform between layers. No auto-mapping libraries (like AutoMapper) are used to keep transformations explicit and maintainable.

### Request to Input Mapping

**Location:** `Pot.AspNetCore/Features/{Feature}/{Operation}/Mappings/RequestMapping.cs`

**Purpose:** Transform HTTP request models (API layer) to service input models (application layer).

**Pattern:**

```csharp
using Pot.App.Features.Accounts.Create.Models;

namespace Pot.AspNetCore.Features.Accounts.Create.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            Bsb = request.Bsb,
            Number = request.Number,
            Description = request.Description,
            Balance = request.Balance,
            Reserved = request.Reserved
        };
    }
}
```

**With Route Parameters:**

```csharp
using Pot.App.Features.Accounts.Update.Models;

namespace Pot.AspNetCore.Features.Accounts.Update.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, Guid id)
    {
        return new Input
        {
            RowId = id,  // From route
            Etag = request.Etag,
            Bsb = request.Bsb,
            Number = request.Number,
            Description = request.Description,
            Balance = request.Balance,
            Reserved = request.Reserved
        };
    }
}
```

**Usage in Handler:**

```csharp
public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(
    Request request,
    ICreateAccountService accountService,
    /* ... */)
{
    // ...validation...

    var accountInput = request.MapToInput();  // Extension method

    var accountOutput = await accountService.CreateAccountAsync(accountInput, cancellationToken);

    // ...
}
```

### Entity to Output Mapping

**Location:** `Pot.App/Features/{Feature}/{Operation}/Mappings/EntityMapping.cs`

**Purpose:** Transform database entities (data layer) to service output models (application layer).

**Pattern:**

```csharp
using Pot.App.Features.Accounts.Create.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Create.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this AccountEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag
        };
    }
}
```

**More Complex Mapping:**

```csharp
using Pot.App.Features.Expenses.GetAll.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.GetAll.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this ExpenseEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag,
            AccountRowId = entity.Account.RowId,
            Description = entity.Description,
            Amount = entity.Amount,
            Frequency = entity.Frequency,
            FrequencyCount = entity.FrequencyCount,
            NextDue = entity.NextDue,
            EndDate = entity.EndDate,
            IsExcluded = entity.IsExcluded,
            AccruedAmount = entity.AccruedAmount,
            DailyAccrual = entity.DailyAccrual
        };
    }
}
```

**Collection Mapping:**

```csharp
public static IEnumerable<Output> MapToOutput(this IEnumerable<AccountEntity> entities)
{
    return entities.Select(entity => entity.MapToOutput());
}

// Usage
var outputs = entities.MapToOutput();
```

**Mapping Guidelines:**

- Use extension methods on source type (`.MapToInput()`, `.MapToOutput()`)
- Keep mappings simple and explicit (no complex logic)
- One mapping file per operation
- Include only necessary properties in output (don't expose everything)
- Always map `RowId` and `Etag` for entities
- Use enriched enums directly (no conversion needed)

**Why Extension Methods?**

- Fluent syntax: `request.MapToInput()` vs `RequestMapper.Map(request)`
- Easy to discover (IntelliSense on source object)
- No need to instantiate mapper classes
- Clear ownership (mapping lives with the operation)

---

## Concerns (Cross-Cutting)

Concerns are infrastructure components that address cross-cutting aspects of the application. They are configured in `Program.cs` and apply globally across features.

**Location:** `Pot.AspNetCore/Concerns/`

**Structure:**

```
Concerns/
├── Auth/                    # JWT authentication, authorization, permissions
├── Validation/              # FluentValidation setup, problem details inspection
├── Cors/                    # CORS configuration
├── RateLimiting/            # Rate limiting policies
├── Middleware/              # Custom middleware (correlation ID, user context)
├── ProblemDetails/          # RFC 7807 problem details customization
├── ExceptionHandlers/       # Global exception handlers
├── Logging/                 # Logging configuration and formatters
└── Converters/              # JSON converters (enriched enums, GUIDs)
```

### Authentication & Authorization

POT uses **JWT-based authentication** with **permission-based authorization**. Permissions use the `resource:action` pattern (e.g., `account:view`, `expense:manage`).

**Configuration:** `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs`

```csharp
public static WebApplicationBuilder AddPotAuth(this WebApplicationBuilder builder)
{
    var services = builder.Services;

    services
        // JWT options from appsettings.json "Jwt" section
        .ConfigureOptions<JwtOptionsSetup>()
        .ConfigureOptions<JwtBearerOptionsSetup>()
        .AddSingletonFromOptions<JwtOptions>()

        // JWT Bearer event handlers (validate tokens against database)
        .ConfigureOptions<JwtBearerEventsSetup>()

        // Authentication options (refresh token cookies, etc.)
        .ConfigureOptions<AuthenticationOptionsSetup>()
        .AddSingletonFromOptions<AuthenticationOptions>()

        // Platform admin configuration
        .ConfigureOptions<PlatformAdminOptionsSetup>()
        .AddSingletonFromOptions<PlatformAdminOptions>()

        .AddAuthorization(options =>
        {
            // Required for /me endpoint (authenticated, no specific permissions)
            options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
        })
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Prevent default claim mapping (preserve original claim names)
            options.MapInboundClaims = false;
        });

    services
        .AddOtpCleanup()  // Background service to clean expired OTP codes
        .AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>()
        .AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

    return builder;
}
```

**JWT Configuration (`appsettings.json`):**

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "POT",
    "Audience": "POT",
    "ExpiryMinutes": 60
  }
}
```

**Permission-Based Authorization:**

**Custom Policy Provider:** `Pot.AspNetCore/Concerns/Auth/PermissionAuthorizationPolicyProvider.cs`

Dynamically creates authorization policies from permission strings:

```csharp
public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
{
    // Check if policy already exists
    var policy = await base.GetPolicyAsync(policyName);

    if (policy is not null)
    {
        return policy;
    }

    // Treat policy name as permission (e.g., "account:view")
    return new AuthorizationPolicyBuilder()
        .AddRequirements(new PermissionRequirement(policyName))
        .Build();
}
```

**Permission Handler:** `Pot.AspNetCore/Concerns/Auth/PermissionAuthorizationHandler.cs`

Validates user has required permission:

```csharp
protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    PermissionRequirement requirement)
{
    // Get permissions from user claims
    var permissions = context.User.Claims
        .Where(c => c.Type == "permission")
        .Select(c => c.Value)
        .ToList();

    // Check if user has required permission
    if (permissions.Contains(requirement.Permission))
    {
        context.Succeed(requirement);
    }

    return Task.CompletedTask;
}
```

**Endpoint Authorization:**

```csharp
routeGroupBuilder
    .MapPost(AccountsEndpoints.Create, Create.Handler.Invoke)
    .RequireAuthorization("account:manage")  // Permission string
    .WithName(nameof(CreateAccount))
    // ...
```

**Permission Patterns:**

- **View permissions**: `account:view`, `expense:view`, `income:view`
- **Manage permissions**: `account:manage`, `expense:manage`, `income:manage`
- **Admin permissions**: `user:manage`, `role:manage`, `approval:manage`
- **Platform admin**: `platform:admin` (super user, bypasses most checks)

**JWT Token Claims:**

```json
{
  "sub": "user-guid", // User identifier (RowId)
  "email": "user@example.com", // User email
  "permission": [
    // Permissions array
    "account:view",
    "account:manage",
    "expense:view",
    "expense:manage"
  ],
  "iat": 1732800000, // Issued at
  "exp": 1732803600 // Expiration
}
```

**Refresh Token Pattern:**

- Access token: Short-lived (60 minutes by default)
- Refresh token: Long-lived (7 days), stored in HTTP-only cookie
- `/api/auth/refresh` endpoint exchanges refresh token for new access token
- Refresh tokens tracked in database, can be revoked

### Validation

POT uses **FluentValidation** for request validation, executed before business logic via `IProblemDetailsInspector`.

**Configuration:** `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs`

```csharp
public static WebApplicationBuilder AddAspNetValidation(this WebApplicationBuilder builder)
{
    builder.Services.AddLifetimeValidationInvoker(validationRegistry =>
    {
        // Auto-register scoped validators (IScopedLifetimeValidator)
        validationRegistry.AutoRegisterScopedValidators<ValidationRegistrar>((modelType, validatorType) =>
        {
            return validatorType.IsAssignableTo(ScopedLifetimeValidatorType);
        });

        // Auto-register singleton validators (default)
        validationRegistry.AutoRegisterSingletonValidators<ValidationRegistrar>((modelType, validatorType) =>
        {
            return !validatorType.IsAssignableTo(ScopedLifetimeValidatorType);
        });

        validationRegistry.AddAppValidators();  // Register app layer validators
    });

    builder.Services.AddSingleton<IProblemDetailsInspector, ProblemDetailsInspector>();

    return builder;
}
```

**ProblemDetailsInspector:**

```csharp
// Pot.AspNetCore/Concerns/Validation/IProblemDetailsInspector.cs
public interface IProblemDetailsInspector
{
    Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType>(TType instance);
    Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType, TContext>(TType instance, TContext context);
    Task<Microsoft.AspNetCore.Mvc.ProblemDetails> ValidateAsync<TType>(TType instance, CancellationToken cancellationToken);
}

// Pot.AspNetCore/Concerns/Validation/ProblemDetailsInspector.cs
internal sealed class ProblemDetailsInspector : IProblemDetailsInspector
{
    private readonly ILifetimeValidationInvoker _validationInvoker;

    public ProblemDetailsInspector(ILifetimeValidationInvoker validationInvoker)
    {
        _validationInvoker = validationInvoker.WhenNotNull();
    }

    public Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType>(TType instance)
    {
        var validationResult = _validationInvoker.Validate(instance);

        return AsProblemDetails(validationResult);
    }

    public Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType, TContext>(TType instance, TContext context)
    {
        var validationResult = _validationInvoker.Validate(instance, context);

        return AsProblemDetails(validationResult);
    }

    private static Microsoft.AspNetCore.Mvc.ProblemDetails AsProblemDetails(ValidationResult validationResult)
    {
        return validationResult.IsValid
            ? NoProblemDetails.Single  // Sentinel object (no errors)
            : validationResult.ToProblemDetails();
    }
}
```

**Validator Base Class:**

```csharp
// Pot.AspNetCore/Concerns/Validation/PotValidatorBase.cs
using AllOverIt.Validation;

namespace Pot.AspNetCore.Concerns.Validation;

public abstract class PotValidatorBase<TType> : ValidatorBase<TType>
{
    // Provides base FluentValidation setup
    // Auto-registered as singleton (unless marked with IScopedLifetimeValidator)
}
```

**Usage in Handler:**

```csharp
public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(
    Request request,
    ICreateAccountService accountService,
    IProblemDetailsInspector problemDetailsInspector,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    // Validate request
    var problemDetails = problemDetailsInspector.Validate(request);

    // Check if validation failed
    if (problemDetails.IsProblem())
    {
        logger.LogErrors(problemDetails);
        return TypedResults.Problem(problemDetails);
    }

    // Continue with business logic...
}
```

**NoProblemDetails Pattern:**

```csharp
// Pot.AspNetCore/Concerns/ProblemDetails/NoProblemDetails.cs
public static class NoProblemDetails
{
    public static readonly Microsoft.AspNetCore.Mvc.ProblemDetails Single = new();
}

// Extension method
public static bool IsProblem(this Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
{
    return problemDetails != NoProblemDetails.Single;
}
```

**Why Sentinel Object?**

- Avoids nullable ProblemDetails
- Type-safe check with extension method
- Clear intent: "no problems" vs null

### CORS Configuration

CORS (Cross-Origin Resource Sharing) allows the frontend to call the API from a different origin.

**Configuration:** `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs`

```csharp
public static WebApplicationBuilder AddPotCors(this WebApplicationBuilder builder)
{
    builder.Services
        // Bind CORS configuration from appsettings.json
        .ConfigureOptions<CorsConfigurationSetup>()
        .AddSingletonFromOptions<CorsConfiguration>()

        // Setup CORS policy options
        .ConfigureOptions<CorsOptionsSetup>()
        .AddCors();

    return builder;
}
```

**CORS Options Setup:** `Pot.AspNetCore/Concerns/Cors/Configuration/CorsOptionsSetup.cs`

```csharp
internal sealed class CorsOptionsSetup : IConfigureOptions<CorsOptions>
{
    private readonly CorsConfiguration _corsConfiguration;

    public CorsOptionsSetup(CorsConfiguration corsConfiguration)
    {
        _corsConfiguration = corsConfiguration.WhenNotNull();
    }

    public void Configure(CorsOptions options)
    {
        options.AddDefaultPolicy(policy =>
        {
            policy
                .WithOrigins(_corsConfiguration.AllowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("content-disposition");  // For export downloads
        });
    }
}
```

**Why Expose `content-disposition`?**

The frontend export feature needs to extract filenames from the `Content-Disposition` header. By default, browsers only expose "safe" CORS headers (`Content-Type`, `Cache-Control`, etc.).

**Configuration (`appsettings.json`):**

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "https://app.example.com"]
  }
}
```

**Program.cs Usage:**

```csharp
var app = builder.Build();

// UseCors MUST be called BEFORE UseAuthentication() and UseAuthorization()
// to ensure CORS headers are on all responses (including 401/403 errors)
app.UseCors();

app.UseAuthentication()
   .UseAuthorization();
```

**CORS Policy Restrictions:**

- Cannot use `AllowAnyOrigin()` with `AllowCredentials()` - must specify explicit origins
- `AllowCredentials()` required for JWT cookies (refresh tokens)
- `WithExposedHeaders()` allows frontend to read additional response headers

### Rate Limiting

Rate limiting prevents API abuse by limiting the number of requests per time window. POT uses a **chained policy** with separate limits for authenticated and anonymous users.

**Configuration:** `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs`

```csharp
public static WebApplicationBuilder AddPotRateLimiting(this WebApplicationBuilder builder)
{
    builder.Services.AddRateLimiter(limiterOptions =>
    {
        limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        limiterOptions.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";
            }

            var errorDetail = ProblemDetailsErrorFactory.CreateTooManyRequests(retryAfter.TotalSeconds);

            await context.HttpContext.Response.WriteAsJsonAsync(
                errorDetail.ToProblemDetails(),
                cancellationToken);
        };

        limiterOptions.AddPolicy(RateLimiterPolicy.Chained, RateLimiterPolicy.CreateChainedPolicy);
    });

    return builder;
}
```

**Chained Policy:** `Pot.AspNetCore/Concerns/RateLimiting/RateLimiterPolicy.cs`

```csharp
public static class RateLimiterPolicy
{
    public const string Chained = "ChainedPolicy";

    public static RateLimitPartition<string> CreateChainedPolicy(HttpContext httpContext)
    {
        var subject = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAuthenticated = subject.IsNotNullOrEmpty();

        // Partition key for rate limiting: use userId for authenticated, IP for anonymous
        var partitionKey = isAuthenticated
            ? $"user:{subject}"
            : $"ip:{httpContext.Connection.RemoteIpAddress}";

        if (isAuthenticated)
        {
            // AUTHENTICATED USERS: Sliding window for smooth, fair rate limiting
            // 50 requests per 30 seconds (~1.67 requests/second sustained)
            // 10 segments = 3 seconds each for gradual expiration
            // NO queue - fail fast for better UX
            return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 50,                       // 50 requests per window
                Window = TimeSpan.FromSeconds(30),      // 30 second window
                SegmentsPerWindow = 10,                 // 10 segments = 3 sec each
                QueueLimit = 0,                         // No queueing - fail fast
            });
        }
        else
        {
            // ANONYMOUS USERS: Fixed window with strict limits
            // 15 requests per 10 seconds (1.5 requests/second sustained)
            // Prevents brute force, DDoS, and API scraping
            // NO queue - fail fast to conserve resources
            return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,                       // 15 requests per window
                Window = TimeSpan.FromSeconds(10),      // 10 second window
                QueueLimit = 0,                         // No queueing - fail fast
            });
        }
    }
}
```

**Policy Characteristics:**

- **Authenticated users**:
  - **50 requests per 30 seconds** (sliding window)
  - **No queue** - immediate failure if limit exceeded
  - **Partition key**: `user:{userId}` (per-user limits)
  - **Sliding window**: Smooth rate limiting with gradual expiration
- **Anonymous users**:
  - **15 requests per 10 seconds** (fixed window)
  - **No queue** - immediate failure if limit exceeded
  - **Partition key**: `ip:{remoteIp}` (per-IP limits)
  - **Fixed window**: Simple, aggressive limiting for untrusted traffic
- **Response**: 429 Too Many Requests with `Retry-After` header

**Applying to Endpoints:**

```csharp
// Applied to entire feature group
app.MapGroup(AccountsEndpoints.Group)
    .WithTags(AccountsEndpoints.Tag)
    .RequireRateLimiting(RateLimiterPolicy.Chained)  // All endpoints rate-limited
    .GetAllAccounts()
    .GetAccount()
    // ...
```

**Rate Limit Error Response (429):**

```json
{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Too many requests. Please wait and try again after 42.5 seconds.",
  "correlationId": "8e9f7c6d-5b4a-3f2e-1d0c-9a8b7c6d5e4f"
}
```

**Response Headers:**

```
HTTP/1.1 429 Too Many Requests
Retry-After: 42.5
Content-Type: application/problem+json
```

### Middleware

POT uses custom middleware for cross-cutting request/response processing.

**Middleware Registration:** `Pot.AspNetCore/Extensions/WebApplicationExtensions.cs`

```csharp
public static WebApplication UsePotMiddleware(this WebApplication app)
{
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<UserContextMiddleware>();

    return app;
}
```

**Middleware Order in Pipeline:**

```csharp
app.UseExceptionHandler();          // 1. Global exception handling
// app.UseHttpsRedirection();       // (Not used - behind Azure reverse proxy)
app.MapHealthChecks("/_health");    // 2. Health check endpoint
app.UseCors();                      // 3. CORS (must be before auth)
app.UseAuthentication();            // 4. JWT authentication
app.UseAuthorization();             // 5. Permission authorization
app.UseRateLimiter();               // 6. Rate limiting
app.UseMiddleware<CorrelationIdMiddleware>();  // 7. Correlation ID
app.UseMiddleware<UserContextMiddleware>();    // 8. User context
// ... endpoint routing ...
```

**CorrelationIdMiddleware:**

Generates unique correlation ID for each request for tracing and debugging.

```csharp
// Pot.AspNetCore/Concerns/Middleware/CorrelationIdMiddleware.cs
internal sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation ID if not provided
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Store in HttpContext for logging
        context.TraceIdentifier = correlationId;

        // Add to response headers
        context.Response.Headers.Append("X-Correlation-ID", correlationId);

        await _next(context);
    }
}
```

**UserContextMiddleware:**

Extracts user identifier from JWT token and stores in scoped `ICurrentUserContext`.

```csharp
// Pot.AspNetCore/Concerns/Middleware/UserContextMiddleware.cs
using AllOverIt.Extensions;
using Pot.Shared;
using System.IdentityModel.Tokens.Jwt;

namespace Pot.AspNetCore.Concerns.Middleware;

internal sealed class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserContext userContext)
    {
        // Extract user ID from JWT "sub" claim
        var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userId.IsNotNullOrEmpty() && Guid.TryParse(userId, out var userRowId))
        {
            userContext.SetUserRowId(userRowId);
        }

        await _next(context);
    }
}
```

**ICurrentUserContext:**

```csharp
// Pot.Shared/ICurrentUserContext.cs
public interface ICurrentUserContext : IPotScopedDependency
{
    Guid UserRowId { get; }
    void SetUserRowId(Guid userRowId);
}

// Pot.Data/CurrentUserContext.cs
internal sealed class CurrentUserContext : ICurrentUserContext
{
    private Guid? _userRowId = null;

    public Guid UserRowId => GetUserRowId();

    public void SetUserRowId(Guid userRowId)
    {
        _userRowId = userRowId;
    }

    private Guid GetUserRowId()
    {
        Throw<UnreachableException>.WhenNot(
            _userRowId.HasValue,
            "The user identifier has not been set");

        return _userRowId.Value;
    }
}
```

**Usage in Services:**

```csharp
// Services can inject ICurrentUserContext to get current user
public async Task<EnrichedResult<Output>> CreateAsync(Input input, CancellationToken cancellationToken)
{
    var currentUserId = _currentUserContext.UserRowId;  // From middleware

    // Use current user ID for logging, auditing, etc.
}
```

**Usage in Repositories:**

```csharp
// Repositories use current user context for multi-tenancy
public SiteEntity GetCurrentSite()
{
    var userRowId = _currentUserContext.UserRowId;

    return Users
        .Include(user => user.Site)
        .Single(user => user.RowId == userRowId)
        .Site;
}
```

### Problem Details

POT follows **RFC 7807 Problem Details** standard for all error responses.

**Configuration:** `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs`

```csharp
public static WebApplicationBuilder AddCustomProblemDetails(this WebApplicationBuilder builder)
{
    builder.Services.AddProblemDetails(options =>
        options.CustomizeProblemDetails = ctx =>
        {
            // Add correlation ID to all problem details
            ctx.ProblemDetails.Extensions.Add("correlationId", ctx.HttpContext.TraceIdentifier);

            // Add HTTP method and path
            ctx.ProblemDetails.Extensions.Add(
                "instance",
                $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}");
        });

    return builder;
}
```

**Problem Details Structure:**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.23",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "Bsb": ["'Bsb' must not be empty."],
    "Number": ["'Number' must not be empty."]
  },
  "correlationId": "8e9f7c6d-5b4a-3f2e-1d0c-9a8b7c6d5e4f",
  "instance": "POST /api/accounts"
}
```

**Problem Types:**

- **400 Bad Request**: Malformed request (rare - usually caught by model binding)
- **401 Unauthorized**: No authentication token provided
- **403 Forbidden**: Authenticated but lacks required permission
- **404 Not Found**: Entity not found
- **409 Conflict**: ETag mismatch (optimistic concurrency)
- **422 Unprocessable Entity**: Validation errors (request or business rules)
- **429 Too Many Requests**: Rate limit exceeded
- **500 Internal Server Error**: Unexpected server error

**Converting Service Errors to Problem Details:**

```csharp
// Extension method
public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this ProblemDetailsError error)
{
    var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        Type = error.ProblemType.GetTypeUri(),
        Title = error.ProblemType.GetTitle(),
        Status = error.ProblemType.GetStatusCode(),
        Detail = error.ErrorMessage
    };

    if (error.PropertyName.IsNotNullOrEmpty())
    {
        problemDetails.Extensions.Add("errors", new Dictionary<string, string[]>
        {
            [error.PropertyName] = [error.AttemptedValue?.ToString() ?? string.Empty]
        });
    }

    return problemDetails;
}
```

**Exception Handler:**

```csharp
// Pot.AspNetCore/Concerns/ExceptionHandlers/DatabaseExceptionHandler.cs
internal sealed class DatabaseExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DatabaseExceptionHandler> _logger;

    public DatabaseExceptionHandler(ILogger<DatabaseExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException dbUpdateException)
        {
            return false;  // Not handled, pass to next handler
        }

        _logger.LogError(dbUpdateException, "Database update error");

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Title = "Database Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An error occurred while updating the database."
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;  // Handled
    }
}
```

---

## Entity Framework Core

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

## Dependency Injection

POT uses **auto-registration** via marker interfaces to automatically discover and register services. This eliminates manual registration in `Program.cs` and ensures consistent service lifetimes.

### Auto-Registration Pattern

**Configuration:** `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs`

```csharp
public static WebApplicationBuilder AddAspNetDependencies(this WebApplicationBuilder builder)
{
    builder.Services
        // Auto-register scoped services
        .AutoRegisterScoped<DependencyRegistrar, IPotScopedDependency>(config =>
        {
            // Exclude interfaces we don't want to register
            config.Filter((serviceType, implementationType) =>
            {
                if (serviceType.IsGenericType)
                {
                    var genericTypeDefinition = serviceType.GetGenericTypeDefinition();

                    // Filter out FluentValidation types
                    return !(genericTypeDefinition == typeof(IValidator<>) ||
                             genericTypeDefinition == typeof(ValidatorBase<>));
                }

                return serviceType != typeof(IPotScopedDependency);
            });
        })
        // Auto-register singleton services
        .AutoRegisterSingleton<DependencyRegistrar, IPotSingletonDependency>(config =>
        {
            config.Filter((serviceType, implementationType) =>
            {
                return serviceType != typeof(IPotSingletonDependency);
            });
        })
        .AddAppDependencies();  // Register app layer dependencies

    return builder;
}
```

**How It Works:**

1. **Marker Interfaces**: Services implement `IPotScopedDependency` or `IPotSingletonDependency`
2. **Assembly Scanning**: `DependencyRegistrar` marker class identifies assembly to scan
3. **Auto-Registration**: All interfaces implementing marker are registered with appropriate lifetime
4. **Filtering**: Generic types like validators are excluded (registered separately)

**Dependency Registrar Marker:**

```csharp
// Pot.AspNetCore/DependencyRegistrar.cs
using AllOverIt.DependencyInjection;

namespace Pot.AspNetCore;

// Used to identify the assembly containing the types to auto-register
internal sealed class DependencyRegistrar : ServiceRegistrarBase
{
}

// Pot.App/DependencyRegistrar.cs
using AllOverIt.DependencyInjection;

namespace Pot.App;

// Used to identify the assembly containing the types to auto-register
internal sealed class DependencyRegistrar : ServiceRegistrarBase
{
}
```

### Service Lifetimes

**Scoped (IPotScopedDependency):**

- New instance per HTTP request
- Disposed at end of request
- Most services use scoped lifetime

```csharp
// Pot.Shared/DependencyInjection/IPotScopedDependency.cs
public interface IPotScopedDependency
{
}

// Usage example
public interface ICreateAccountService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken);
}
```

**Services That Use Scoped Lifetime:**

- Business logic services (`ICreateAccountService`, `IUpdateExpenseService`, etc.)
- Repositories (`IAccountRepository`, `IExpenseRepository`, etc.)
- Entity checkers (`IPreCreateChecker`, `IPreUpdateChecker`, etc.)
- Current user context (`ICurrentUserContext`)
- Database context (`PotDbContext`)
- Transaction factory (`IPotTransactionFactory`)

**Singleton (IPotSingletonDependency):**

- Single instance for application lifetime
- Shared across all requests
- Must be thread-safe

```csharp
// Pot.Shared/DependencyInjection/IPotSingletonDependency.cs
public interface IPotSingletonDependency
{
}

// Usage example
public interface ITimeProvider : IPotSingletonDependency
{
    DateOnly GetUtcDateNow();
    DateTime GetUtcDateTimeNow();
    DateOnly GetLocalDateNow();
    DateTime GetLocalDateTimeNow();
    TimeSpan GetLocalTimeZoneOffset();
}
```

**Services That Use Singleton Lifetime:**

- Time providers (`ITimeProvider`)
- Configuration options (`JwtOptions`, `SmtpConfiguration`, etc.)
- Logging formatters
- Authorization handlers and policy providers
- Exception handlers

**Transient (Rare):**

- New instance every time requested
- Not used in POT (scoped is preferred)

**Validation Registration:**

Validators are registered separately because they require special lifetime handling:

```csharp
public static WebApplicationBuilder AddAspNetValidation(this WebApplicationBuilder builder)
{
    builder.Services.AddLifetimeValidationInvoker(validationRegistry =>
    {
        // Auto-register scoped validators (marked with IScopedLifetimeValidator)
        validationRegistry.AutoRegisterScopedValidators<ValidationRegistrar>((modelType, validatorType) =>
        {
            return validatorType.IsAssignableTo(typeof(IScopedLifetimeValidator));
        });

        // Auto-register singleton validators (default - no IScopedLifetimeValidator marker)
        validationRegistry.AutoRegisterSingletonValidators<ValidationRegistrar>((modelType, validatorType) =>
        {
            return !validatorType.IsAssignableTo(typeof(IScopedLifetimeValidator));
        });

        validationRegistry.AddAppValidators();
    });

    return builder;
}
```

**When to Use Each Lifetime:**

| Lifetime  | Use When                                                     | Examples                                           |
| --------- | ------------------------------------------------------------ | -------------------------------------------------- |
| Scoped    | Service depends on request-specific data (user, transaction) | Services, repositories, entity checkers, DbContext |
| Singleton | Service has no state or state is shared across all requests  | Configuration, time providers, authorization       |
| Transient | Service is lightweight and needs fresh instance each use     | (Not used in POT)                                  |

**Manual Registration (When Needed):**

Some services require manual registration for special configuration:

```csharp
// In Program.cs or extension method
builder.Services.AddDbContext<PotDbContext>((provider, options) =>
{
    var databaseConfiguration = provider.GetRequiredService<DatabaseConfiguration>();
    var connectionString = databaseConfiguration.GetConnectionString();

    options.ConfigurePostgres(connectionString);
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
```

---

## Accrual Metrics Semantics

The server tracks two different daily accrual concepts for expenses.

### Dynamic DailyExpenseAccrual

- Purpose: operational projection behavior and event-date simulation.
- Behavior: changes through each cycle as due dates approach and renew.
- Primary use: projections and date-by-date cashflow mechanics.

### Stable StableExpenseAccrual

- Purpose: long-run daily funding guidance.
- Behavior: designed to be more stable than dynamic accrual.
- Recurring rule: `Amount / Frequency.GetAverageDaysToNext(FrequencyCount)`.
- One-time rule: fixed bounded period contribution before due date only.

### Accrual Policy Gate

`AccrualPolicy.None` explicitly disables accrual contribution.

- No contribution to `TotalExpenseAccrued`
- No contribution to `DailyExpenseAccrual`
- No contribution to `StableExpenseAccrual`

This preserves due-date debit behavior while disabling pre-funding accrual behavior.

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
