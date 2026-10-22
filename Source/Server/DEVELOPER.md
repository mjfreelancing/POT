# Backend Developer Guide

Comprehensive guide for developers working on the POT ASP.NET Core backend. This guide provides actionable patterns and conventions to help you understand the architecture and implement new features consistently.

## Table of Contents

- [Architecture Overview](#architecture-overview)
  - [Project Structure](#project-structure)
  - [Key Architectural Principles](#key-architectural-principles)
  - [Design Conventions](#design-conventions)
- [Security](#security)
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
- [Accrual Metrics Semantics](#accrual-metrics-semantics)
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
│   │                      # Expenses, Incomes, Maintenance, Me, Notifications, Otp, Projections,
│   │                      # Roles, Settings, Sites, Users)
│   ├── AppContext.cs      # Application context for current user/site
│   ├── IAppContext.cs     # Application context interface
│   └── DependencyRegistrar.cs  # Marker class for auto-registration
├── Pot.AspNetCore/        # API Layer - HTTP endpoints, handlers, requests/responses
│   ├── Concerns/          # Cross-cutting infrastructure (Auth, Converters, Cors, Email,
│   │                      # ExceptionHandlers, Health, Logging, Middleware, ProblemDetails,
│   │                      # RateLimiting, Validation)
│   ├── Extensions/        # Web application builder extensions
│   ├── Features/          # Feature-based endpoints with handlers (Accounts, Accruals, Approvals, Auth,
│   │                      # Expenses, Incomes, Maintenance, Me, Projections, Roles, Settings, Sites,
│   │                      # Users, Workers)
│   ├── Models/            # Shared API models (ResponseBase)
│   ├── Utils/             # API utilities (parameter binding)
│   ├── DependencyRegistrar.cs  # Marker class for auto-registration
│   └── Program.cs         # Application startup and configuration
├── Pot.Data/              # Data Layer - EF Core, entities, repositories
│   ├── Annotations/       # Custom data annotations (CiText, MediumString, OtpCode, SmallString)
│   ├── Configuration/     # Database configuration
│   ├── Entities/          # Database entities (AccountEntity, ExpenseEntity, IncomeEntity, etc.)
│   ├── Extensions/        # EF Core extensions and query helpers
│   ├── Migrations/        # EF Core migrations (generated)
│   ├── Repositories/      # Data access repositories (Accounts, AuthSessions, Expenses,
│   │                      # Incomes, Otp, Projections, Roles, Settings, Sites, Users)
│   ├── Specifications/    # Query specifications - reusable LINQ expressions
│   ├── CurrentUserContext.cs   # Current user context for multi-tenancy
│   ├── DbContextBase.cs   # Base DbContext with common configuration
│   ├── IPotTransactionFactory.cs  # Transaction factory interface
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
    ├── Enumerations/      # Enriched enums (AccrualPolicy, ApprovalStatus, Frequency, OtpReason,
    │                      # OtpStatus, Permission, RenewalMode, Role, SettingCategory, UserStatus)
    ├── Extensions/        # Shared extensions (date/time, frequency, service collection)
    ├── ICurrentUserContext.cs  # Interface for current user context
```

**Test Projects:**

```
Source/Server/
├── Pot.App.Tests/         # App/business layer unit tests
├── Pot.AspNetCore.Tests/  # API layer unit tests (non-hosted)
├── Pot.AspNetCore.Integration.Tests/  # Hosted API integration tests (HTTP boundary)
├── Pot.Data.Tests/        # Data layer unit tests
├── Pot.Shared.Tests/      # Shared layer unit tests
└── Pot.TestUtils/         # Shared test infrastructure (PotFixtureBase, EntityFactory,
                           # CollectionAssertionExtensions)
```

### Key Architectural Principles

**1. Security First**

- **JWT-based authentication** with role-based authorization
- **Endpoint-level permission checks** using `resource:action` pattern (e.g., `account:view`, `expense:manage`)
- **Rate limiting** with differentiated limits (authenticated: 50/30s, anonymous: 15/10s)
- **CORS** configured for trusted origins only
- **Multi-tenancy isolation** - query filters isolate `Account`, `Expense`, `Income` and `Setting`; `User` and `Site` are isolated by the repository surface instead (see [Security](#security))

**2. Layered Clean Architecture**

- **Dependency Flow**: API → App → Data (outer layers depend on inner, never reversed)
- **Separation of Concerns**: HTTP concerns stay in API layer, business logic in App layer, data access in Data layer
- **Feature-Based Organization**: Code organized by feature (Accounts, Expenses) rather than technical layer

**3. Pre-Validation with Problem Details**

- **FluentValidation** executes before business logic via `IProblemDetailsInspector`
- All validation errors returned as **RFC 9457 Problem Details** for consistent error handling
- **400 Bad Request** for all validation errors, with every failure listed in the `errors` array

**4. Result Pattern for Business Logic**

- Services return `EnrichedResult<T>` wrapping success/failure outcomes
- Business rule violations convert to Problem Details maintaining consistent error format
- No exceptions for business rule failures (only for truly exceptional conditions)

**5. Minimal API Pattern**

- **Feature-based endpoint registration** using route groups
- **Authorization via `RequireAuthorization()`** at endpoint level with `resource:action` permissions
- **Fluent endpoint configuration** (WithName, WithSummary, ProducesProblem)

**6. Multi-Tenancy by Default**

- **Site-based query filters** automatically isolate `Account`, `Expense`, `Income` and `Setting`
- **`User` and `Site` are not filtered** - a `User` filter would be self-referential, and permission resolution and token validation run before a site is known. They are reachable only through named, site-scoped repository methods, so a query that omits the site is not expressible
- The two mechanisms sit side by side; neither replaces the other (see [Security](#security))

**7. Health Checks**

- Liveness monitoring at `/_health` (executes no checks); database/schema readiness at `/_health/ready`
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
- HTTP status codes: 200 (OK), 201 (Created), 204 (No Content), 400 (Validation or a refused business rule), 401, 403,
  404 (Not Found), 409 (Conflict), 429 (Rate Limit), 500 (Error) - see [Problem Details](#problem-details)

**Code Organization:**

- Features organized by domain (Accounts, Expenses, Incomes)
- Each feature has consistent structure: Handler, Request, Response, RequestValidator, Mappings
- Services named by operation: `ICreateAccountService`, `IUpdateAccountService`, `IDeleteAccountService`
- Repositories named by entity: `IAccountRepository`, `IExpenseRepository`

**Naming Conventions:**

- Entities: `AccountEntity`, `ExpenseEntity` (must end with `Entity` suffix)
- Services: `CreateAccountService`, `UpdateAccountService` (verb + entity pattern)
- Repositories: `AccountRepository`, `ExpenseRepository` (entity + Repository)
- Handlers: `Handler` (sealed class with a static `Invoke` method per operation)

**Error Handling:**

- Use `ApiDetailErrorFactory` to create standardized errors
- Business rule violations return `EnrichedResult.Fail<T>(error)` from services
- Services log errors before returning failure results
- API handlers convert errors to Problem Details via `ToProblemDetails()` extension

---

## Security

How the product is protected. Every item is a mechanism that is implemented, not a proposal.

- **Isolation has two parts, and they sit side by side.** `Account`, `Expense`, `Income` and `Setting` are isolated by a global query filter that resolves the caller's site once per request. `User` and `Site` deliberately are not filtered: a `User` filter would be self-referential, and permission resolution and token validation both run before a site is known. Those two are isolated by the repository surface instead - the interfaces expose no `IQueryable` for either, so they are reachable only through named, site-scoped methods and a lookup that omits the site is not expressible rather than merely discouraged. Neither mechanism stands in for the other: the filters are the right tool for the four entities they cover, and the repository surface exists because that tool cannot be extended to the other two. The mechanics are in [Multi-Tenancy & Query Filters](#multi-tenancy--query-filters).
- **The target decides tenancy, not the caller.** A caller's own site proves nothing about the row a request names, so every id-taking route resolves its target through a site-scoped lookup. A target in another site and a target that does not exist answer `404` with the same body, never a distinguishable `403`, so a refusal cannot be used to discover that a row exists elsewhere. Cross-tenant refusals are logged at warning, naming the caller and the requested id.
- **Authorization is resolved per request.** Permissions use the `resource:action` pattern and are computed from the caller's roles plus the deployment-configured platform grant, from the database, on every request. A fail-closed `RequireAuthenticatedUser` fallback policy makes an endpoint that declares no requirement of its own authenticated-only, and the routes that must stay reachable before a session exists are marked `AllowAnonymous()` explicitly, so the anonymous set is a short reviewable list rather than a default that grows by accident. A regression test enumerates the host's endpoints and fails if a route is neither.
- **The access token carries no derived state.** It holds `jti`, `sub` and `tokenVersion` and nothing else. Tenancy, permissions and account status are read from the database on every request, because a JWT payload is readable by anyone holding the token and a claim goes stale while the token stays valid.
- **Two identities are protected from lock-out.** A configured platform administrator cannot be disabled, and a site's last enabled `Admin` cannot be disabled or demoted, so neither platform nor tenant administration can be left without an administrator. Both refusals answer `400` with copy the user can act on, and the target is left unchanged.
- **Credentials.** Passwords are stored only as salted one-way hashes. Invitations, sign-ups and password resets use single-use codes with an expiry and an attempt limit. Changing a password or signing out increments the token version, which invalidates every access token already issued, and a disabled account is rejected when its token is validated.
- **Responses are not sniffed or cached.** Every response carries `X-Content-Type-Options: nosniff` and `Cache-Control: no-store`, error responses included, so a browser cannot reinterpret a body as script and neither a browser nor a proxy keeps a copy of financial data, user details or tokens. See [Middleware](#middleware).
- **Abuse throttling.** Rate limiting is partitioned by anonymous client and by signed-in user, and answers `429` with a `Retry-After` header and the wait time in the problem details.
- **The deliberate cross-tenant surfaces.** Approvals and the platform routes run behind the platform grant and read across sites on purpose. Every read that crosses a site boundary is deliberate, is served by a repository method named for it, and records at its call site why it is allowed: a username resolved before a site is known, a caller who is signed out, a token validated before any user context exists, or the platform surface acting on every site. There is no unnamed cross-tenant read.
- **Checklist for a new endpoint that takes an id.** Declare its authorization requirement. Resolve the target through a site-scoped repository method. Refuse as not-found when the caller cannot act on the target. Log the refusal at warning. If the read genuinely has to cross sites, name the method for it and say why at the call site.

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
- Migration applied: `dotnet ef migrations add AddAccountEntity --project Pot.Data --startup-project Pot.Data.Migrations`

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

/// <summary>Carries the values needed to create an account.</summary>
public sealed class Request
{
    /// <summary>A description of the account</summary>
    [Description("A description of the account")]
    public string Description { get; init; } = string.Empty;

    /// <summary>The account balance</summary>
    [Description("The account balance")]
    public double Balance { get; init; }

    /// <summary>The minimum reserved amount</summary>
    [Description("The minimum reserved amount")]
    public double Reserved { get; init; }
}
```

The XML `<summary>` comments reach the OpenAPI document as schema descriptions - `OpenApiSecurityFixture` asserts that
they do - so a request or response type is documented in both places.

Schema components are named after the slice, because the document has one flat namespace and a second `Response` would
silently replace the first: `Pot.AspNetCore.Features.Accounts.Create.Request` is `Accounts_Create_Request`. A feature type
from another project that reaches the document is qualified by that project as well (`App_Projections_Models_DateProjection`),
and collections are described inline. `Concerns/OpenApi/SchemaReferenceId` does the naming, and `SchemaReferenceIdFixture`
fails if two feature types would share a name.

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
        RuleFor(account => account.Description).IsNotEmpty();
        RuleFor(account => account.Balance).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(account => account.Reserved).IsGreaterThanOrEqualTo(0.0d);
    }
}
```

The validator checks only what the request alone can decide. Whether the description is already used needs the database,
so it is an entity check in `Pot.App` (Step 10), not a rule here.

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

Create `Pot.AspNetCore/Features/Accounts/Create/RequestMapping.cs`:

```csharp
using Pot.App.Features.Accounts.Create.Models;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
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

namespace Pot.AspNetCore.Features.Accounts.Create;

internal sealed class Handler
{
    public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(Request request,
        ICreateAccountService accountService, IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
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
            .Produces<Create.Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return routeGroupBuilder;
    }

    // Other endpoint methods (GetAccount, UpdateAccount, etc.)
}
```

`Produces<T>` declares the success body; `ProducesProblem` declares only failures, because it documents the body as
`ProblemDetails`. The `400` and `500` every endpoint can return come from `ProducesStandardProblems()` on the route group
below, so they are not repeated here.

Create `Pot.AspNetCore/Features/Accounts/Extensions/WebApplicationExtensions.cs`:

```csharp
using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Extensions;

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
                .ProducesStandardProblems()
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
app.UsePotMiddleware();

app.UseScalarOpenApi()
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
    // Provided when importing - required so income and expense items can be linked correctly
    public Guid? RowId { get; init; }

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
    Task<ApiDetailError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken);
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

internal sealed class PreCreateChecker : ChainOfResponsibilityAsyncComposer<InputState, ApiDetailError?>, IPreCreateChecker
{
    private readonly ILogger _logger;

    public PreCreateChecker(IEnumerable<IPreCreateCheck> preCheckHandlers, ILogger<PreCreateChecker> logger)
        : base(preCheckHandlers.Cast<PreCreateCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ApiDetailError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken)
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

Create `Pot.App/Features/Accounts/Create/EntityChecks/InputState.cs`, the state passed along the chain:

```csharp
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

internal sealed class InputState
{
    public required AccountEntity AccountToCreate { get; init; }
}
```

Create `Pot.App/Features/Accounts/Create/EntityChecks/Checks/PreCreateCheckBase.cs`. The marker interface is what lets every
check be injected into `PreCreateChecker` as one `IEnumerable<IPreCreateCheck>`:

```csharp
using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

internal interface IPreCreateCheck : IPotScopedDependency;

internal abstract class PreCreateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError?>, IPreCreateCheck
{
}
```

Create each check in `Pot.App/Features/Accounts/Create/EntityChecks/Checks/`:

```csharp
// CheckDescriptionDoesNotExist.cs
using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Specifications;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreCreateCheckBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CheckDescriptionDoesNotExist(IAccountRepository accountRepository, ILogger<CheckDescriptionDoesNotExist> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = state.AccountToCreate;

        var predicate = AccountSpecifications.IsSameDescription(account.Description).Expression;

        // Description uniqueness is per-site; the query filter restricts the check to the current site
        var descriptionExists = await _accountRepository.Accounts.AnyAsync(predicate, cancellationToken);

        if (descriptionExists)
        {
            return ApiDetailErrorFactory.CreateEntityExistsError(
                nameof(AccountEntity.Description),
                "The account description already exists");
        }

        // Call the next handler in the chain
        return await base.HandleAsync(state, cancellationToken);
    }
}
```

The check needs no site filter of its own: the global query filter on `AccountEntity` already restricts `Accounts` to the
caller's site, which is what makes the rule "unique within a site" - see
[Multi-Tenancy & Query Filters](#multi-tenancy--query-filters). The database enforces the same rule with a unique index on
`(SiteId, Description)`, so a request that races past this check gets a `409` from `DatabaseExceptionHandler` instead.

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
                Description = input.Description,
                Balance = input.Balance,
                Reserved = input.Reserved
            };

            // Provided when importing - required so income and expense items can be linked correctly
            if (input.RowId.HasValue)
            {
                accountToCreate.RowId = input.RowId.Value;
            }

            // Run entity checks (business rule validation)
            var apiError = await _preCreateChecker.CanSaveAsync(accountToCreate, cancellationToken);

            if (apiError is not null)
            {
                return EnrichedResult.Fail<Output>(apiError);
            }

            // Save to database
            await _accountRepository.AddAndSaveAsync(accountToCreate, cancellationToken);

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
    .AutoRegisterSingleton<DependencyRegistrar, IPotSingletonDependency>(/* filters */)
    .AddAppDependencies();
```

**Step 14: Test the Endpoint**

**Using Scalar (Development):**

1. Run the application: `dotnet run --project Pot.AspNetCore`
2. Navigate to: `http://localhost:5242/scalar`
3. Find "Accounts Api" → "Create account"
4. Execute test request

**Using Postman:**

```http
POST http://localhost:5242/api/accounts
Content-Type: application/json
Authorization: Bearer {your-jwt-token}

{
  "description": "Savings",
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

**Validation Error Response (400 Bad Request):**

`type`, `title` and `traceId` are supplied by ASP.NET Core; the members below are the ones POT sets.

```json
{
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": [
    {
      "errorCode": "NotEmptyValidator",
      "propertyName": "Description",
      "errorMessage": "'Description' must not be empty."
    }
  ],
  "correlationId": "8e9f7c6d-5b4a-3f2e-1d0c-9a8b7c6d5e4f",
  "instance": "/api/accounts"
}
```

**Business Rule Violation (409 Conflict):**

A failure from a service carries its message in `detail` and repeats it in `errors`. `ToProblemDetails()` sets `detail`
from the error's message, so a client can show one sentence without having to join the `errors` entries. No submitted
value is echoed back: the message names the field, never the value the caller sent.

```json
{
  "status": 409,
  "detail": "The account description already exists",
  "errors": [
    {
      "errorCode": "Conflict",
      "propertyName": "Description",
      "errorMessage": "The account description already exists"
    }
  ],
  "correlationId": "8e9f7c6d-5b4a-3f2e-1d0c-9a8b7c6d5e4f",
  "instance": "/api/accounts"
}
```

**Which endpoints declare 409.** `ProducesProblem(409)` is declared per feature, only on the endpoints whose own checks can reject the request: a stale entity tag, or a description that already exists within the same site or account. It is deliberately not declared on every write. The shared database handler answers 409 for a unique, reference or concurrency failure that reaches the database, such as a race between the check and the save, and documenting that on every write would present a conflict as a normal outcome rather than the rare one it is.

Because the database answers the same status, a test that asserts only 409 cannot tell the two apart. The integration fixtures assert the `propertyName` the rule reported and that `detail` repeats its message (`ShouldBeEntityConflictAsync`); the database fallback names no field, so those assertions fail if a rule stops running. `EndpointResponseMetadataFixture` additionally fails if a declared 409 is not `ProblemDetails` with `application/problem+json`.

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
│   │   │   ├── RequestMapping.cs          # Request → Input mapping (flat - no Mappings/ folder in the API layer)
│   │   │   └── Response.cs                # HTTP response model
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
│   └── ResponseBase.cs                    # Base response with RowId, Etag
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
│   │   │           ├── PreCreateCheckBase.cs   # Base class and IPreCreateCheck marker
│   │   │           └── CheckDescriptionDoesNotExist.cs
│   │   ├── Update/                             # Similar structure
│   │   └── ...
│   ├── Expenses/                               # Similar structure
│   └── ...
├── Concerns/                                   # Cross-cutting application logic
│   ├── Auth/                                   # Authentication/authorization helpers
│   ├── Csv/                                    # CSV parsing helpers
│   ├── Time/                                   # Time provider abstraction
│   ├── Validation/                             # Application-level validation
│   └── Zip/                                    # Archive helpers
├── Errors/                                     # API error definitions
│   ├── ApiErrorBase.cs                         # Error base carrying the ErrorType
│   ├── ApiBasicError.cs                        # Error with code and message
│   ├── ApiDetailError.cs                       # Error with property details
│   ├── ApiDetailErrorCollection.cs             # Error collection
│   ├── ApiDetailErrorFactory.cs                # Error creation factory
│   ├── ErrorCodes.cs                           # Error code constants
│   └── ErrorType.cs                            # Error type enumeration
├── Mappings/                                   # Entity → calculation input projections
│   ├── ExpenseAccrualMapping.cs                # Expense → ExpenseAccrualInput
│   └── IncomeScheduleMapping.cs                # Income → IncomeScheduleInput
├── Calculators/                                # Domain calculators
│   ├── AccrualCalculator.cs                    # Expense accrual calculations
│   ├── ExpenseRenewalCalculator.cs             # Expense renewal calculations
│   ├── IncomeRenewalCalculator.cs              # Income renewal calculations
│   └── ...                                     # Corresponding interfaces
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
        // 1. Load entity, through a site-scoped repository method
        var entity = await _repository.GetEntityOrDefaultAsync(input.RowId, cancellationToken);

        if (entity is null)
        {
            var notFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError("The entity does not exist");

            return EnrichedResult.Fail<Output>(notFoundError);
        }

        // 2. Run entity checks (business rules)
        var apiError = await _entityChecker.CanSaveAsync(input, entity, cancellationToken);

        if (apiError is not null)
        {
            return EnrichedResult.Fail<Output>(apiError);
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
│   │   └── Dtos/                          # Repository-specific DTOs
│   ├── Expenses/                          # Similar structure
│   └── ...
├── Specifications/
│   ├── AccountSpecifications.cs           # Account-specific specifications
│   ├── EntitySpecifications.cs            # Generic entity specifications
│   ├── ExpenseSpecifications.cs           # Expense-specific specifications
│   └── IncomeSpecifications.cs            # Income-specific specifications
├── Configuration/
│   └── DatabaseConfiguration.cs           # Database connection configuration
├── Extensions/
│   ├── DbContextExtensions.cs             # DbContext extension methods
│   └── QueryableExtensions.cs             # IQueryable extension methods
├── Migrations/                            # EF Core migrations
├── DbContextBase.cs                       # Base DbContext with conventions
├── PotDbContext.cs                        # Application DbContext
├── PotDataRegistrar.cs                    # Marker class for data layer registration
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
│   ├── AccrualPolicyExtensions.cs         # Accrual policy utility methods
│   ├── DateOnlyExtensions.cs              # DateOnly utility methods
│   ├── DateTimeExtensions.cs              # DateTime utility methods
│   ├── FrequencyExtensions.cs             # Frequency utility methods
│   ├── ServiceCollectionExtensions.cs     # Service registration helpers
│   └── ...
├── DependencyInjection/
│   ├── IPotScopedDependency.cs            # Marker for scoped services
│   └── IPotSingletonDependency.cs         # Marker for singleton services
└── ICurrentUserContext.cs                 # Current user context interface
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
public static async Task<Ok<Response[]>> Invoke(
    IService service,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    var items = await service.GetAllAsync(cancellationToken);

    return Response.Ok(items);  // Collection response factory
}
```

**3. Get Handler (GET - Route Parameter):**

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(
    [Description("The account Id")] Guid id,  // Route parameter
    IGetAccountService accountService,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    // Route parameters are not validated in the handler; an unknown id surfaces as a
    // not-found error from the service.
    var result = await accountService.GetAccountWithLinkedCountsAsync(id, cancellationToken);

    return result.IsSuccess
        ? Response.Ok(result.Value!)
        : TypedResults.Problem(result.Error!.ToProblemDetails());
}
```

**4. Update Handler (PUT - Route Parameter + Body):**

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(
    [Description("The account Id")] Guid id,  // Route parameter
    Request request,  // Request body
    IUpdateAccountService accountService,
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

    var accountInput = request.MapToInput(id);  // Include route parameter in mapping

    var result = await accountService.UpdateAccountAsync(accountInput, cancellationToken);

    return result.IsSuccess
        ? Response.Ok(result.Value!)
        : TypedResults.Problem(result.Error!.ToProblemDetails());
}
```

**5. Delete Handler (DELETE - Route Parameter Only):**

```csharp
public static async Task<Results<Ok, ProblemHttpResult>> Invoke(
    [Description("The account Id")] Guid id,
    IDeleteAccountService accountService,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    // A delete has no request body or route validation to perform
    var result = await accountService.DeleteAccountAsync(id, cancellationToken);

    return result.IsSuccess
        ? TypedResults.Ok()
        : TypedResults.Problem(result.Error!.ToProblemDetails());
}
```

**6. Action Handler (POST - No Response Body):**

```csharp
public static async Task<Results<Ok, ProblemHttpResult>> Invoke(
    Request request,  // Action payload in the request body
    IService service,
    IProblemDetailsInspector problemDetailsInspector,
    ILogger<Handler> logger,
    CancellationToken cancellationToken)
{
    logger.LogCall(null);

    var problemDetails = problemDetailsInspector.Validate(request);

    if (problemDetails.IsProblem())
    {
        logger.LogErrors(problemDetails);
        return TypedResults.Problem(problemDetails);
    }

    var input = request.MapToInput();

    var result = await service.PerformActionAsync(input, cancellationToken);

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
        RuleFor(account => account.Description).IsNotEmpty();
        RuleFor(account => account.Balance).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(account => account.Reserved).IsGreaterThanOrEqualTo(0.0d);
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
using AllOverIt.Validation.Extensions;
using FluentValidation.Results;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Expenses.Create;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Description).IsNotEmpty();

        // Custom rules receive FluentValidation's ValidationContext; retrieve the request's
        // context data through the AllOverIt helper rather than casting.
        this.CustomRuleFor(request => request.EndDate, (value, context) =>
        {
            if (!value.HasValue)
            {
                return;
            }

            var validationContext = context.GetContextData<Request, RequestValidationContext>();

            if (validationContext.NextDue > value.Value)
            {
                var failure = new ValidationFailure(nameof(Request.EndDate), "Cannot be earlier than the next due date", value)
                {
                    ErrorCode = ErrorCodes.Invalid
                };

                context.AddFailure(failure);
            }
        });
    }
}

// In Handler:
var validationContext = new RequestValidationContext
{
    NextDue = request.NextDue,
    EndDate = request.EndDate,
    AccrualPolicy = request.AccrualPolicy,
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

// Enum validation
RuleFor(x => x.Frequency).IsInEnum();

// Collection validation
RuleFor(x => x.Items).IsNotEmpty();
RuleFor(x => x.Items).Must(x => x.Count > 0);

// Conditional validation
RuleFor(x => x.EndDate).IsNotEmpty().When(x => x.Frequency != Frequency.OneTime);

// Custom validation
RuleFor(x => x.Code).Must(BeSixDigits).WithMessage("Code must be exactly 6 digits");

private static bool BeSixDigits(string code)
{
    return System.Text.RegularExpressions.Regex.IsMatch(code, @"^\d{6}$");
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

**3. Collection Response (200 OK with an array body):**

```csharp
public static Ok<Response[]> Ok(List<Output> items)
{
    var responses = items.SelectToArray(item => new Response(item));

    return TypedResults.Ok(responses);
}
```

**4. No Content Response (200 OK no body):**

Delete handlers return the no-body result directly - there is no zero-argument `Response.Ok()` factory:

```csharp
return TypedResults.Ok();
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
            .Produces<Create.Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);

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
            .Produces<Get.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

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
- **`Produces<T>`**: The success status and its body type. Never `ProducesProblem` for a success: that documents the body as
  `ProblemDetails`, and a generated client then types the successful call as an error object
- **`ProducesProblem`**: The failures this endpoint's own rules can produce - `404`, `409`, and `401` for a protected endpoint.
  The `400` and `500` every endpoint can return are declared once on the route group by `ProducesStandardProblems()`

**Feature Registration:**

**Location:** `Pot.AspNetCore/Features/{Feature}/Extensions/WebApplicationExtensions.cs`

```csharp
using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Extensions;

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
                .ProducesStandardProblems()
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
app.UsePotMiddleware();

app.UseScalarOpenApi()
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
                Description = input.Description,
                Balance = input.Balance,
                Reserved = input.Reserved
            };

            // Provided when importing - required so income and expense items can be linked correctly
            if (input.RowId.HasValue)
            {
                accountToCreate.RowId = input.RowId.Value;
            }

            // Step 4: Run entity checks (business rule validation)
            var apiError = await _preCreateChecker.CanSaveAsync(accountToCreate, cancellationToken);

            if (apiError is not null)
            {
                return EnrichedResult.Fail<Output>(apiError);
            }

            // Step 5: Save to database
            await _accountRepository.AddAndSaveAsync(accountToCreate, cancellationToken);

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
4. **Multi-Tenancy**: Resolve an id-taking target through a site-scoped repository method, so a target outside the caller's site reads as not-found rather than merely being refused
5. **Entity Checks**: Validate business rules before persistence
6. **Error Handling**: Return `EnrichedResult.Fail<T>()` for business rule violations
7. **Success Result**: Return `EnrichedResult.Success<T>()` with mapped output

The ASP.NET Core server does not install a synchronization context, so POT server code should not use `ConfigureAwait(false)`; keep awaits direct.

**Service Operation Patterns:**

The create pattern is generic; the others are the Account feature's own services, lightly trimmed.

**1. Create Service:**

```csharp
public async Task<EnrichedResult<Output>> CreateAsync(Input input, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    using (_repository.WithTracking())
    {
        var entity = new Entity { /* map from input */ };

        var apiError = await _preCreateChecker.CanSaveAsync(entity, cancellationToken);

        if (apiError is not null)
        {
            return EnrichedResult.Fail<Output>(apiError);
        }

        await _repository.AddAndSaveAsync(entity, cancellationToken);

        return EnrichedResult.Success(entity.MapToOutput());
    }
}
```

**2. Update Service:**

```csharp
public async Task<EnrichedResult<Output>> UpdateAccountAsync(Input input, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    using (_accountRepository.WithTracking())
    {
        var accountId = input.RowId;

        // Load entity - tracked, because its properties are changed below
        var accountToUpdate = await _accountRepository.GetAccountOrDefaultAsync(accountId, cancellationToken);

        if (accountToUpdate is null)
        {
            var accountNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError("The account does not exist");

            _logger.LogApiError(accountNotFoundError);

            return EnrichedResult.Fail<Output>(accountNotFoundError);
        }

        // Run entity checks - business rules including etag validation, which is implemented
        // as a check in the chain (CheckHasSameEtag -> CreateEtagConflict("<EntityType>", input.Etag))
        var apiError = await _preUpdateChecker.CanSaveAsync(input, accountToUpdate, cancellationToken);

        if (apiError is not null)
        {
            _logger.LogApiError(apiError);

            return EnrichedResult.Fail<Output>(apiError);
        }

        // Update entity properties
        accountToUpdate.Description = input.Description;
        accountToUpdate.Balance = input.Balance;
        accountToUpdate.Reserved = input.Reserved;

        // Save changes
        _ = await _accountRepository.SaveAsync(cancellationToken);

        return EnrichedResult.Success(accountToUpdate.MapToOutput());
    }
}
```

**3. Delete Service:**

```csharp
public async Task<EnrichedResult<bool>> DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    // Checks first - for example, an account with linked expenses or incomes cannot be deleted (400)
    var apiError = await _preDeleteChecker.CanDeleteAsync(accountId, cancellationToken);

    if (apiError is not null)
    {
        return EnrichedResult.Fail<bool>(apiError);
    }

    var account = await _accountRepository.GetAccountOrDefaultAsync(accountId, cancellationToken);

    if (account is null)
    {
        var accountNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError("The account does not exist");

        _logger.LogApiError(accountNotFoundError);

        return EnrichedResult.Fail<bool>(accountNotFoundError);
    }

    // No tracking scope needed: Delete attaches the entity itself
    _accountRepository.Delete(account);

    await _accountRepository.SaveAsync(cancellationToken);

    return EnrichedResult.Success(true);
}
```

**4. GetAll Service:**

```csharp
public async Task<IReadOnlyList<Output>> GetAllAccountsAsync(CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    var accounts = await _accountRepository.GetAllAccountsWithLinkedCountsAsync(cancellationToken);
    var asOfDate = _timeProvider.GetLocalDateNow();

    // Accrual totals are calculated, not stored, so they are passed to the mapping alongside the entity
    return accounts.SelectToArray(account => account.MapToOutput(CalculateAccrual(account, asOfDate)));
}
```

An empty site returns an empty list, never `null` and never `404`.

**5. Get Service:**

```csharp
public async Task<EnrichedResult<Output>> GetAccountWithLinkedCountsAsync(Guid accountId, CancellationToken cancellationToken)
{
    _logger.LogCall(this);

    var account = await _accountRepository.GetAccountWithLinkedCountsOrDefaultAsync(accountId, cancellationToken);

    if (account is null)
    {
        var accountNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError("The account does not exist");

        _logger.LogApiError(accountNotFoundError);

        return EnrichedResult.Fail<Output>(accountNotFoundError);
    }

    var accrualView = CalculateAccrual(account);
    var output = account.MapToOutput(accrualView);

    return EnrichedResult.Success(output);
}
```

Because the query runs through the site filter, an account in another site is `null` here - so it is reported as `404`,
exactly as a missing one is, and its existence is not disclosed.

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
└── Checks/
    ├── PreCreateCheckBase.cs      # Base class, and the IPreCreateCheck marker interface
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
    Task<ApiDetailError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken);
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

Declared in the same file as the base class below, `Checks/PreCreateCheckBase.cs`:

```csharp
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

// A marker interface so each handler can be dependency injected into the PreCreateChecker
internal interface IPreCreateCheck : IPotScopedDependency;
```

**4. Create Base Class for Checks:**

```csharp
using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.App.Errors;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

internal abstract class PreCreateCheckBase : ChainOfResponsibilityHandlerAsync<InputState, ApiDetailError?>, IPreCreateCheck
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

internal sealed class PreCreateChecker : ChainOfResponsibilityAsyncComposer<InputState, ApiDetailError?>, IPreCreateChecker
{
    private readonly ILogger _logger;

    public PreCreateChecker(IEnumerable<IPreCreateCheck> preCheckHandlers, ILogger<PreCreateChecker> logger)
        : base(preCheckHandlers.Cast<PreCreateCheckBase>())
    {
        _logger = logger.WhenNotNull();
    }

    public Task<ApiDetailError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken)
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
// CheckDescriptionDoesNotExist.cs
using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Specifications;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreCreateCheckBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CheckDescriptionDoesNotExist(IAccountRepository accountRepository, ILogger<CheckDescriptionDoesNotExist> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = state.AccountToCreate;

        var predicate = AccountSpecifications.IsSameDescription(account.Description).Expression;

        // Description uniqueness is per-site; the query filter restricts the check to the current site
        var descriptionExists = await _accountRepository.Accounts.AnyAsync(predicate, cancellationToken);

        if (descriptionExists)
        {
            return ApiDetailErrorFactory.CreateEntityExistsError(
                nameof(AccountEntity.Description),
                "The account description already exists");
        }

        // Call next handler in chain (or return null if no more handlers)
        return await base.HandleAsync(state, cancellationToken);
    }
}
```

The update operation has its own `CheckDescriptionDoesNotExist` in `Update/EntityChecks/Checks/`. It runs the query only when
the description has changed, and excludes the account being updated, by combining the specification with a second predicate:

```csharp
var predicate = AccountSpecifications
    .IsSameDescription(input.Description).Expression
    .And(item => item.Id != account.Id);
```

The two are separate classes rather than one shared check, because each operation's chain has its own `InputState`.

**Entity Check Guidelines:**

- Each check validates **one business rule**
- Return `ApiDetailError` for violations
- Return `null` and call `base.HandleAsync()` to continue chain
- Log entry point for observability
- Do not use `ConfigureAwait(false)` for async operations
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
var result = EnrichedResult.Fail<Output>(apiDetailError);

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
public bool IsFail { get; }
public T? Value { get; }              // The result value
public EnrichedError? Error { get; }  // Throws InvalidOperationException if the result is not in an error state
```

**Service Layer Pattern:**

```csharp
public async Task<EnrichedResult<Output>> OperationAsync(Input input, CancellationToken cancellationToken)
{
    // Business rule violation - return failure
    if (someBusinessRuleViolated)
    {
        var error = ApiDetailErrorFactory.CreateEntityExistsError("PropertyName", "Error message");
        return EnrichedResult.Fail<Output>(error);
    }

    // Entity not found - return failure
    if (entity is null)
    {
        var error = ApiDetailErrorFactory.CreateEntityNotFoundError("Entity does not exist");
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
        : TypedResults.Problem(result.Error!.ToProblemDetails());  // HTTP 400/401/404/409, from the error's ErrorType
}
```

**ApiDetailError Structure:**

```csharp
public class ApiBasicError : ApiErrorBase
{
    public required string ErrorCode { get; init; }
    public required string ErrorMessage { get; init; }
    public object? CustomState { get; init; }

    public ApiBasicError(ErrorType errorType) : base(errorType) { }
}

public class ApiDetailError : ApiBasicError
{
    public string PropertyName { get; init; } = string.Empty;

    public ApiDetailError(ErrorType errorType) : base(errorType) { }
}
```

**Common Error Factory Methods:**

```csharp
// Entity not found (404)
var error = ApiDetailErrorFactory.CreateEntityNotFoundError(
    errorMessage: "The account does not exist.");

// Entity exists conflict (409)
var error = ApiDetailErrorFactory.CreateEntityExistsError(
    propertyName: nameof(AccountEntity.Description),
    errorMessage: "The account description already exists");

// Entity constraint violation (400) - a rule about related data refused the operation
var error = ApiDetailErrorFactory.CreateEntityConstraintError(
    propertyName: nameof(AccountEntity.RowId),
    errorMessage: "Cannot delete an Account that has linked Expenses");

// ETag conflict (409)
var error = ApiDetailErrorFactory.CreateEtagConflict(
    entityType: "Account");

// Generic bad request (400) - a business rule refused the request
var error = ApiDetailErrorFactory.CreateBadRequestError(
    errorMessage: "The operation cannot be completed.");

// With property name (400)
var error = ApiDetailErrorFactory.CreateBadRequestError(
    propertyName: nameof(InputState.RoleIds),
    errorMessage: "This is the site's only enabled administrator. Promote another administrator first.");
```

There is no `422` in POT: request validation and refused business rules both answer `400`, and a client tells them apart by
the `errorCode` of each entry in `errors`. `EnrichedErrorExtensionsFixture` pins the mapping from `ErrorType` to status.

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

    // The user must be tracked to update its etag and roles
    using (_userRepository.WithTracking())
    {
        // Begin transaction
        using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);

        var userId = input.RowId;

        // Step 1: Load user with roles, through a site-scoped repository method - the user repository exposes no
        // queryable, because UserEntity has no site query filter
        var userToUpdate = await _userRepository.GetForCurrentSiteWithRolesAsync(userId, cancellationToken);

        if (userToUpdate is null)
        {
            var userNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError("The user does not exist");

            _logger.LogApiError(userNotFoundError);

            return EnrichedResult.Fail<Output>(userNotFoundError);
        }

        // Step 2: Run entity checks
        var apiError = await _preUpdateChecker.CanSaveAsync(input, userToUpdate, cancellationToken);

        if (apiError is not null)
        {
            _logger.LogApiError(apiError);

            return EnrichedResult.Fail<Output>(apiError);
        }

        // Step 3: Load new roles
        var roles = await _roleRepository.GetRolesAsync(input.RoleIds, cancellationToken);

        // Step 4: Update user roles
        userToUpdate.Roles = roles;

        // Step 5: Force the etag update. The user is already tracked, so this does not hit the database
        var userEntry = _userRepository.GetEntry(userToUpdate);
        userEntry.State = EntityState.Modified;

        // Step 6: Save changes (within transaction)
        _ = await _userRepository.SaveAsync(cancellationToken);

        // Step 7: Commit transaction
        await transaction.CommitAsync(cancellationToken);

        return EnrichedResult.Success(userToUpdate.MapToOutput());
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

        var apiError = await _preCreateChecker.CanSaveAsync(account, cancellationToken);

        if (apiError is not null)
        {
            return EnrichedResult.Fail<Output>(apiError);
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
public interface IRepositoryBase : IPotScopedDependency
{
    // On IRepositoryBase just in case a read entity needs to be attached to another processed by IPersistableRepository
    IDisposable WithTracking();

    EntityEntry GetEntry<TEntity>(TEntity entity) where TEntity : EntityBase;

    // Deliberately no Set<TEntity>() here: a queryable on the interface would let any caller write a lookup with
    // no site predicate, which is what the site-scoped repository methods exist to prevent. It stays on
    // RepositoryBase for repository implementations to use.
}

// Pot.Data/Repositories/IPersistableRepository.cs
public interface IPersistableRepository
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
    Task<IReadOnlyList<AccountWithLinkedCounts>> GetAllAccountsWithLinkedCountsAsync(CancellationToken cancellationToken);
}

// Pot.Data/Repositories/Accounts/IPersistableAccountRepository.cs - read operations from IAccountRepository,
// write operations from IPersistableRepository
public interface IPersistableAccountRepository : IAccountRepository, IPersistableRepository;
```

**Exposing `Accounts` as a queryable is safe because `AccountEntity` has a site query filter**, so any query a caller
writes against it is already scoped to the caller's site - which is how `CheckDescriptionDoesNotExist` composes its own query.
`UserEntity` and `SiteEntity` have no filter, so their repositories expose no queryable at all - see
[Multi-Tenancy & Query Filters](#multi-tenancy--query-filters).

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

Provides core capabilities without write operations:

- `WithTracking()` - Enable change tracking for a scope
- `GetEntry<TEntity>()` - Get entity entry metadata

`Set<TEntity>()` is on the `RepositoryBase` class only, for repository implementations - see the comment in the interface
above.

**`IPersistableRepository` (Write Operations):**

Declares data modification methods. It does **not** inherit `IRepositoryBase` - feature repositories compose both, for example `IPersistableAccountRepository : IAccountRepository, IPersistableRepository`:

- `Add<TEntity>()` - Add entity to context
- `Update<TEntity>()` - Update entity in context
- `Delete<TEntity>()` - Delete entity from context
- `Save()` / `SaveAsync()` - Persist changes to database
- `AddAndSaveAsync<TEntity>()` - Add and immediately save
- `UpdateAndSaveAsync<TEntity>()` - Update and immediately save

**⚠️ Critical: WithTracking() Requirement for Loaded Entities**

`Add`/`Update`/`Delete` attach the entity to the context themselves, so they do not need a tracking scope. A tracking scope **is** required when you load an entity and then mutate its properties, because queries return untracked entities by default:

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
using Pot.Shared.Models;
using System.Linq.Expressions;

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

    public async Task<AccountWithLinkedCounts?> GetAccountWithLinkedCountsOrDefaultAsync(Guid rowId, CancellationToken cancellationToken)
    {
        // Projection query - returns DTO instead of entity
        return await Accounts
            .Where(account => account.RowId == rowId)
            .Select(item => new AccountWithLinkedCounts
            {
                Account = item,
                LinkedIncomes = item.Incomes.Count,
                LinkedExpenses = item.Expenses.Count,
                AccrualExpenses = item.Expenses
                    .AsQueryable()
                    .Where(expense => !expense.ExcludeFromCalcs)
                    .Select(AccrualExpenseProjection)
                    .ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AccountWithLinkedCounts>> GetAllAccountsWithLinkedCountsAsync(CancellationToken cancellationToken)
    {
        // The same projection over every account in the site
        return await Accounts
            .Select(item => new AccountWithLinkedCounts { /* as above */ })
            .ToArrayAsync(cancellationToken);
    }

    // Part of a query expression: this must stay an expression tree so EF Core can translate it into SQL, and the
    // nested collections call AsQueryable() so this overload binds. Declared once because both account queries
    // project the same accrual facts.
    private static readonly Expression<Func<ExpenseEntity, ExpenseAccrualInput>> AccrualExpenseProjection =
        expense => new ExpenseAccrualInput
        {
            RowId = expense.RowId,
            ExcludeFromCalcs = expense.ExcludeFromCalcs,
            AccrualStart = expense.AccrualStart,
            NextDue = expense.NextDue,
            EndDate = expense.EndDate,
            AccrualPolicy = expense.AccrualPolicy,
            Frequency = expense.Frequency,
            FrequencyCount = expense.FrequencyCount,
            Amount = expense.Amount
        };
}
```

A uniqueness rule is not a repository method here. The description check composes its query from
`AccountSpecifications.IsSameDescription` against `Accounts` - see [Query Specifications](#query-specifications).

**Repository Method Naming Conventions:**

Entity-specific methods are prefixed with the entity name, for example:

- `GetAccountAsync()` - Returns entity, throws if not found
- `GetAccountOrDefaultAsync()` - Returns entity or null
- `AccountExistsAsync()` - Returns bool
- `HasExpensesAsync()` / `HasIncomesAsync()` - Returns bool
- `GetAllAccountsWithLinkedCountsAsync()` - Returns collection
- `Add()` - Attaches entity to the context (changes persisted by `Save()` / `SaveAsync()`)
- `Update()` - Attaches entity to the context (changes persisted by `Save()` / `SaveAsync()`)
- `Delete()` - Attaches entity to the context (changes persisted by `Save()` / `SaveAsync()`)
- `Save()` / `SaveAsync()` - Persists changes
- `AddAndSaveAsync()` - Adds and persists immediately
- `UpdateAndSaveAsync()` - Updates and persists immediately

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
// Reference count tracking scopes per DbContext instance
private static readonly ConditionalWeakTable<DbContext, TrackingCounter> TrackingCounters = [];

public static IDisposable WithAutoTracking(this DbContext dbContext)
{
    var counter = TrackingCounters.GetOrCreateValue(dbContext);

    return new Raii(
        () => { counter.Increment(dbContext); },
        () => { counter.Decrement(dbContext); });
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
using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class EntitySpecifications
{
    public static ILinqSpecification<TEntity> IsSameId<TEntity>(Guid rowId) where TEntity : EntityBase
    {
        return LinqSpecification<TEntity>.Create(entity => entity.RowId == rowId);
    }
}

// Pot.Data/Specifications/AccountSpecifications.cs
using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class AccountSpecifications
{
    public static ILinqSpecification<AccountEntity> IsSameDescription(string description)
    {
        // This appears case-insensitive but the database schema uses CITEXT for the text fields
        return LinqSpecification<AccountEntity>.Create(account => account.Description == description);
    }
}
```

**Using Specifications:**

```csharp
// In repository
public Task<AccountEntity?> GetAccountOrDefaultAsync(Guid rowId, CancellationToken cancellationToken)
{
    return Accounts.SingleOrDefaultAsync(EntitySpecifications.IsSameId<AccountEntity>(rowId).Expression, cancellationToken);
}

// In entity check
public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
{
    var account = state.AccountToCreate;

    var predicate = AccountSpecifications.IsSameDescription(account.Description).Expression;

    // Description uniqueness is per-site; the query filter restricts the check to the current site
    var descriptionExists = await _accountRepository.Accounts.AnyAsync(predicate, cancellationToken);

    if (descriptionExists)
    {
        return ApiDetailErrorFactory.CreateEntityExistsError(/* ... */);
    }

    return await base.HandleAsync(state, cancellationToken);
}
```

**Combining Specifications:**

```csharp
// AND combination
var spec = AccountSpecifications.IsSameDescription("Savings")
    .And(EntitySpecifications.IsSameId<AccountEntity>(rowId));

var accounts = await _repository.Accounts
    .Where(spec.Expression)
    .ToListAsync(cancellationToken);

// OR combination
var spec = AccountSpecifications.IsSameDescription("Savings")
    .Or(AccountSpecifications.IsSameDescription("Everyday"));

var accounts = await _repository.Accounts
    .Where(spec.Expression)
    .ToListAsync(cancellationToken);

// NOT combination
var spec = AccountSpecifications.IsSameDescription("Savings")
    .And(EntitySpecifications.IsSameId<AccountEntity>(rowId).Not());

var accounts = await _repository.Accounts
    .Where(spec.Expression)
    .ToListAsync(cancellationToken);
```

A specification's `Expression` can also be combined with a plain lambda, through `AllOverIt.Expressions`. The update
check does that to exclude the account being updated:

```csharp
var predicate = AccountSpecifications
    .IsSameDescription(input.Description).Expression
    .And(item => item.Id != account.Id);
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

**Location:** `Pot.AspNetCore/Features/{Feature}/{Operation}/RequestMapping.cs`

The API operation folder is flat - there is no `Mappings/` folder in `Pot.AspNetCore` - so the mapping shares the handler's
namespace and the handler needs no `using` to call it. `Pot.App` operations do use a `Mappings/` folder (see below).

**Purpose:** Transform HTTP request models (API layer) to service input models (application layer).

**Pattern:**

```csharp
using Pot.App.Features.Accounts.Create.Models;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
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

namespace Pot.AspNetCore.Features.Accounts.Update;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, Guid accountId)
    {
        return new Input
        {
            RowId = accountId,  // From route
            Etag = request.Etag,
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
using Pot.Shared.Models;

namespace Pot.App.Features.Expenses.GetAll.Mappings;

internal static class EntityMapping
{
    // Accrued and Arrears are calculated by the service, not stored on the entity, so they arrive as a second argument
    public static Output MapToOutput(this ExpenseEntity expense, ExpenseAccrualDetail accrual)
    {
        return new Output
        {
            RowId = expense.RowId,
            Etag = expense.Etag,
            ExcludeFromCalcs = expense.ExcludeFromCalcs,
            Description = expense.Description,
            NextDue = expense.NextDue,
            EndDate = expense.EndDate,
            AccrualStart = expense.AccrualStart,
            AccrualPolicy = expense.AccrualPolicy,
            Frequency = expense.Frequency,
            FrequencyCount = expense.FrequencyCount,
            Amount = expense.Amount,
            Accrued = accrual.Accrued,
            Arrears = accrual.Arrears,
            Note = expense.Note,
            Account = new Output.AccountModel
            {
                RowId = expense.Account.RowId,
                Description = expense.Account.Description
            }
        };
    }
}
```

**Collection Mapping:**

Collections are mapped with `SelectToList(...)` over the single-item extension; no collection overload of `MapToOutput` exists:

```csharp
var outputs = entities.SelectToList(entity => entity.MapToOutput());
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
├── Middleware/              # Custom middleware (correlation ID, security headers, user context)
├── ProblemDetails/          # RFC 9457 problem details customization
├── ExceptionHandlers/       # Global exception handlers
├── Email/                   # SMTP configuration setup
├── Health/                  # Service health polling
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

            // Fail closed: an endpoint that declares no requirement of its own is authenticated-only rather
            // than reachable anonymously.
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
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

Routes that must stay reachable while signed out - sign-in, sign-out, token refresh, signup, password reset and the two health checks - opt out with `.AllowAnonymous()`, which keeps the anonymous set an explicit list. See [Security](#security).

**JWT Configuration (`appsettings.json`):**

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "POT",
    "Audience": "POT"
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

Resolves the user's permissions from the database (they are not carried as claims):

```csharp
protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    PermissionRequirement requirement)
{
    var userRowId = context.User.Claims
        .SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

    if (!Guid.TryParse(userRowId, out Guid parsedUserRowId))
    {
        return;
    }

    using var scope = _serviceScopeFactory.CreateScope();

    var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

    var permissions = await permissionService.GetPermissionsAsync(parsedUserRowId, CancellationToken.None);

    if (permissions.Contains(requirement.Permission))
    {
        context.Succeed(requirement);
    }
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

- **View permissions**: `site:view`, `user:view`, `account:view`, `expense:view`, `income:view`
- **Manage permissions**: `site:manage`, `user:manage`, `account:manage`, `expense:manage`, `income:manage`
- **Maintenance permissions**: `maintenance:export`, `maintenance:import`
- **Platform admin**: `platform:manage` (granted in addition to the user's site permissions)

**JWT Token Claims:**

```json
{
  "jti": "token-id", // Unique token identifier
  "sub": "user-guid", // User identifier (RowId)
  "token_version": 3, // Incremented to invalidate existing tokens
  "iat": 1732800000, // Issued at
  "exp": 1732800900 // Expiration
}
```

**Refresh Token Pattern:**

- Access token: Short-lived (15 minutes)
- Refresh token: Long-lived (30 days), stored in HTTP-only cookie
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

    public async Task<Microsoft.AspNetCore.Mvc.ProblemDetails> ValidateAsync<TType>(TType instance, CancellationToken cancellationToken)
    {
        var validationResult = await _validationInvoker.ValidateAsync(instance, cancellationToken);

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

internal abstract class PotValidatorBase<TType> : ValidatorBase<TType>
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
internal sealed class NoProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public static readonly NoProblemDetails Single = new();
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
public sealed class CorsOptionsSetup : IConfigureOptions<CorsOptions>
{
    private static readonly TimeSpan PreflightMaxAge = TimeSpan.FromMinutes(10);

    private readonly CorsConfiguration _corsConfiguration;

    public CorsOptionsSetup(CorsConfiguration corsConfiguration)
    {
        _corsConfiguration = corsConfiguration;
    }

    public void Configure(CorsOptions options)
    {
        var allowedOrigins = _corsConfiguration.GetAllowedOrigins().ToArray();

        options.AddDefaultPolicy(policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("content-disposition", "retry-after", CorrelationIdMiddleware.HeaderName)
                .SetPreflightMaxAge(PreflightMaxAge);
        });
    }
}
```

**Why expose these headers?**

The frontend export feature needs to extract filenames from the `Content-Disposition` header. By default, browsers only expose "safe" CORS headers (`Content-Type`, `Cache-Control`, etc.). `retry-after` is exposed so the client can read the 429 cooldown, and `X-Correlation-Id` so it can read the id to quote to support. The policy caches preflights for ten minutes; without `Access-Control-Max-Age`, Chromium caches them for only five seconds and nearly every JSON or `Authorization` request pays an extra `OPTIONS` round trip.

**Configuration (`appsettings.json`):**

```json
{
  "Cors": {
    "AllowedOrigins": "http://localhost:5175"
  }
}
```

Note: `AllowedOrigins` is a delimited **string** (not a JSON array). Empty means no cross-origin access; the development settings allow the Vite dev server on port 5175. Each entry must be an absolute `http`/`https` origin with no path, query, fragment or trailing slash (and `*` is rejected); an invalid entry fails startup via `ValidateOnStart()`.

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
    // WARNING: The RateLimiting configuration keys (RateLimiting:Anonymous:PermitLimit,
    // RateLimiting:Anonymous:WindowSeconds, RateLimiting:Authenticated:PermitLimit,
    // RateLimiting:Authenticated:WindowSeconds) must NOT be added to any configuration file.
    // They are injected as command-line arguments by the Playwright webServer configuration for
    // E2E runs only; the hardcoded RateLimiterDefaults values are the production source of truth.
    builder.Services
        // Binds the "RateLimiting" section (present only during E2E runs, for more aggressive limits)
        .ConfigureOptions<RateLimitingConfigurationSetup>()

        // Allow for injection of RateLimitingConfiguration instead of IOptions<RateLimitingConfiguration>
        .AddSingletonFromOptions<RateLimitingConfiguration>()

        // Sets up rate limiter options - configures the rejection handler and chained policy
        .ConfigureOptions<RateLimiterOptionsSetup>()

        .AddRateLimiter();

    return builder;
}
```

The 429 rejection status, `Retry-After` header, error body and `.AddPolicy(RateLimiterPolicy.Chained, RateLimiterPolicy.CreateChainedPolicy)` wiring live in `Concerns/RateLimiting/Configuration/RateLimiterOptionsSetup.cs`.

**Chained Policy:** `Pot.AspNetCore/Concerns/RateLimiting/RateLimiterPolicy.cs`

```csharp
internal static class RateLimiterPolicy
{
    public const string Chained = "ChainedPolicy";

    public static RateLimitPartition<string> CreateChainedPolicy(HttpContext httpContext, int anonymousPermitLimit,
        TimeSpan anonymousWindow, int authenticatedPermitLimit, TimeSpan authenticatedWindow)
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
  "status": 429,
  "detail": "Too many requests. Please wait and try again after 43 seconds.",
  "errors": [
    {
      "errorCode": "TooManyRequests",
      "errorMessage": "Too many requests. Please wait and try again after 43 seconds."
    }
  ]
}
```

The rate limiter writes this body through `IProblemDetailsService`, so it carries the same `type`,
`title`, `correlationId` and `instance` as every other error response. `Retry-After` is an integer
delta-seconds value (RFC 9110), rounded up to the next whole second. When the lease carries no
`RetryAfter` metadata the header is omitted and the body omits the wait instead of naming one.

**Response Headers:**

```
HTTP/1.1 429 Too Many Requests
Retry-After: 43
Content-Type: application/problem+json
```

### Middleware

POT uses custom middleware for cross-cutting request/response processing.

**Middleware Registration:** `Pot.AspNetCore/Extensions/WebApplicationExtensions.cs`

```csharp
public static WebApplication UseSecurityHeaders(this WebApplication app)
{
    app.UseMiddleware<SecurityHeadersMiddleware>();

    return app;
}

public static WebApplication UseCorrelationId(this WebApplication app)
{
    app.UseMiddleware<CorrelationIdMiddleware>();

    return app;
}

public static WebApplication UsePotMiddleware(this WebApplication app)
{
    app.UseMiddleware<UserContextMiddleware>();

    return app;
}
```

**Middleware Order in Pipeline:**

```csharp
app.UseSecurityHeaders();                // 1. nosniff and no-store on every response (first, so error responses get them)
app.UseHttpLogging();                    // 2. One request-log entry per request; no body or headers
app.UseExceptionHandler();               // 3. Global exception handling
app.UseStatusCodePages();                // 4. Problem Details for error responses that have no body
// app.UseHttpsRedirection();            // (Not used - behind Azure reverse proxy)
app.MapHealthChecks("/_health");         // 5. Liveness endpoint (short-circuited: no middleware, no headers)
app.MapHealthChecks("/_health/ready");   // 6. Readiness endpoint (short-circuited)
app.UseCors();                           // 7. CORS (must be before auth)
app.UseCorrelationId();                  // 8. Correlation ID (after CORS, before auth and rate limiting)
app.UseAuthentication();                 // 9. JWT authentication
app.UseAuthorization();                  // 10. Permission authorization
app.UseRateLimiter();                    // 11. Rate limiting
app.UsePotMiddleware();                  // 12. User context
// ... endpoint routing ...
```

**Request logging (framework HTTP logging):**

`AddLogging` registers `AddHttpLogging` with `RequestMethod | RequestPath | RequestQuery | ResponseStatusCode | Duration`
and `CombineLogs = true`, so each request produces one entry when its response completes. No request body and no request
or response headers are logged in any environment. `appsettings.json` sets the
`Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware` category to `Information`; `Microsoft.AspNetCore` is `Warning`,
which would otherwise swallow the entry. The health probes are short-circuited at routing, so they produce no entry and
are outside the security-header and correlation middleware (the health check middleware's own no-cache headers remain).

**SecurityHeadersMiddleware:**

Adds `X-Content-Type-Options: nosniff` and `Cache-Control: no-store` to every response
(`Pot.AspNetCore/Concerns/Middleware/SecurityHeadersMiddleware.cs`).

- `nosniff` stops a browser second-guessing the declared content type, so a body is never treated as script or HTML because
  of what it contains. `no-store` keeps responses out of browser and proxy caches: almost everything the API returns is a
  site's financial data or a user's details, login and refresh responses carry tokens, and error bodies carry the correlation id.
- Every response, not only `/api`, because nothing the API serves is meant to be cached and a path test is one more thing a
  new route can fall outside of. An endpoint that genuinely wants caching sets its own `Cache-Control`, which is left alone.
  The health probes are the exception: they are short-circuited at routing and never reach this middleware.
- The headers are written as the response starts, because the exception handler clears the response headers before writing
  a 500. It runs first in the pipeline so the exception handler's 500s, status code pages, and the 401/403/429 responses from
  authentication, authorization and rate limiting all carry them. A 500 carries the exception handler's own
  `no-cache,no-store`, which satisfies the same rule.

**CorrelationIdMiddleware:**

Applies the caller's correlation ID to the request for tracing and debugging
(`Pot.AspNetCore/Concerns/Middleware/CorrelationIdMiddleware.cs`).

- The header is `X-Correlation-Id` (via `Request.TryGetCorrelationId()`). An empty or whitespace value is treated as absent.
- A supplied value must be at most 128 characters and contain only letters, digits, `-`, `_`, `.` and `:`. Anything else is
  rejected with a 400 ProblemDetails response, because the value is written into every log entry for the request. The reason
  is in `detail` and also in `errors`, as one entry with `propertyName` set to `X-Correlation-Id` and `errorCode` `Invalid`;
  the rejected value is not echoed back.
- A valid value replaces `HttpContext.TraceIdentifier`; when the header is absent the framework default is kept. Every
  ProblemDetails body carries `TraceIdentifier` as `correlationId`, and `CorrelationIdLogEnricher` adds the same value to every
  log entry written during the request - including the exception handlers' and a rejected id's, which carry the generated id.
- The id is also tagged on the request's `Activity` as `correlationId`, so a trace can be found by it. The hosting layer only
  creates that `Activity` when something observes it, so the tag is skipped when there is none and nothing else relies on it.
- The id is echoed in `X-Correlation-Id` on every response that passes through the middleware - the caller's own id, or the
  generated one when none was sent or the one sent was rejected. On a successful response the header is the only place the id
  travels. It is written as the response starts, because the exception handler clears the response headers before writing
  a 500. CORS exposes it so browser script can read it.
- The header is documented on every operation in the OpenAPI document: optional on the request, with the same length and
  character limits, and present on every response. `correlationId`, `traceId` and `instance` are documented as required,
  non-nullable members of every Problem Details schema (`Concerns/OpenApi/ProblemDetailsSchemaTransformer`): the first two travel in
  the extensions dictionary the generator cannot see, and `instance` is always set although the type allows null.
- It runs straight after `UseCors()`: after it so its own 400 carries CORS headers, and before authentication, authorization
  and rate limiting so their 401/403/429 responses and log entries carry the caller's id.

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
// Pot.Data/Repositories/Sites/SiteRepository.cs
// Repositories use current user context for multi-tenancy
public SiteEntity GetCurrentSite()
{
    return _userRepository.GetCurrentUser(true).Site;
}
```

### Problem Details

POT follows the **RFC 9457 Problem Details** standard (which replaced RFC 7807) for all error responses.

**Configuration:** `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs`

```csharp
public static WebApplicationBuilder AddProblemDetails(this WebApplicationBuilder builder)
{
    // IProblemDetailsService is registered as a singleton.
    builder.Services.AddProblemDetails(options =>
        options.CustomizeProblemDetails = ctx =>
        {
            ctx.ProblemDetails.Extensions["correlationId"] = ctx.HttpContext.TraceIdentifier;
            ctx.ProblemDetails.Instance = ctx.HttpContext.Request.Path.ToString();
        });

    // ...then the framework's IProblemDetailsWriter is wrapped in UnconditionalProblemDetailsWriter - see below.

    return builder;
}
```

- **The indexer, not `Extensions.Add()`.** `Add()` throws on a key that is already present, and this runs while an error
  response is being written - the one place a throw turns a handled failure into an unhandled one.
- **`instance` is the `ProblemDetails.Instance` property, never an extension**, and holds the request path only (`/api/accounts`,
  not `POST /api/accounts`). An `instance` extension alongside the property would give the body two `instance` keys.

**Problem Details Structure:**

The `errors` extension is always an **array** of error detail objects. `type` and `title` are supplied by ASP.NET Core.

```json
{
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": [
    {
      "errorCode": "NotEmptyValidator",
      "propertyName": "Description",
      "errorMessage": "'Description' must not be empty."
    }
  ],
  "correlationId": "8e9f7c6d-5b4a-3f2e-1d0c-9a8b7c6d5e4f",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
  "instance": "/api/accounts"
}
```

`detail` is set from the failure's message on every status, so a client can show it without inspecting `errors`. No
submitted value appears in `detail`, in `errors` or in the request log.

Every problem response carries two identifiers:

- `correlationId` is `HttpContext.TraceIdentifier`, which is the caller's `X-Correlation-Id` when
  supplied. It appears on every log entry for the request and is the value to quote to support.
- `traceId` is the framework's identifier: `Activity.Current.Id` (the W3C traceparent) when tracing is
  active, otherwise `TraceIdentifier`, so it matches `correlationId`.

Use `correlationId` for support; `traceId` identifies the distributed trace.

**Every error is Problem Details, whatever the `Accept` header says.** The framework's writer declines a request whose `Accept`
excludes JSON (a browser navigation sends `text/html`), and each route into an error body then fails differently: a rejected
correlation id became a 500, the exception handler and `UseStatusCodePages` wrote plain text, and `TypedResults.Problem` wrote
JSON without `correlationId`. `AddProblemDetails()` wraps the framework's writer in `UnconditionalProblemDetailsWriter`
(`Pot.AspNetCore/Concerns/ProblemDetails/`), which always writes, so the body stays the framework's but the decision to write is
ours. Startup fails if anything other than exactly one framework writer is registered, since a second writer would still decline.

Validation failures are logged by `LogErrors` from the `errors` extension, one entry per error.
Attempted field values are never logged: for login, signup and password reset they can be a password
or an OTP.

**Problem Types:**

- **400 Bad Request**: Request validation errors; a business rule that refused the request (`ErrorType.BadRequest` or
  `ErrorType.Constraint`, such as deleting an account that has linked expenses); a malformed request; or a correlation ID
  longer than 128 characters or containing characters other than letters, digits, `-`, `_`, `.` and `:` (`CorrelationIdMiddleware`)
- **401 Unauthorized**: No valid authentication token, or a failed sign-in (`ErrorType.Auth`)
- **403 Forbidden**: Authenticated but lacks required permission
- **404 Not Found**: Entity not found
- **409 Conflict**: ETag mismatch (optimistic concurrency), an entity that already exists, or a unique, reference or
  concurrency failure reported by the database (`DatabaseExceptionHandler`)
- **429 Too Many Requests**: Rate limit exceeded
- **500 Internal Server Error**: Unexpected server error

POT does not use `422`; see the note under the error factory methods.

**Converting Service Errors to Problem Details:**

```csharp
// Pot.AspNetCore/Extensions/EnrichedErrorExtensions.cs
internal static class EnrichedErrorExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this EnrichedError enrichedError)
    {
        var error = enrichedError as ApiErrorBase;

        var statusCode = error!.ErrorType switch
        {
            ErrorType.Auth => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Constraint => StatusCodes.Status400BadRequest,
            ErrorType.BadRequest => StatusCodes.Status400BadRequest,
            ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
            ErrorType.Server => StatusCodes.Status500InternalServerError,
            _ => throw new UnreachableException($"Unknown problem type: {error.ErrorType}")
        };

        // Note: 'errors' must be an array of objects
        var errors = enrichedError switch
        {
            ApiDetailError problemDetailsError => [problemDetailsError.GetErrorDetails()],
            ApiBasicError problemDetailsBasicError => [problemDetailsBasicError.GetErrorDetails()],
            ApiDetailErrorCollection problemDetailsErrorCollection => problemDetailsErrorCollection.Errors.SelectToArray(error => error.GetErrorDetails()),
            _ => throw new UnreachableException($"Unknown enriched error type: {enrichedError.GetType()}")
        };

        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = error.Description,
            Status = statusCode,
            Extensions = new Dictionary<string, object?>
            {
                { "errors", errors }
            }
        };
    }
}
```

**Exception Handler:**

`BadRequestExceptionHandler` answers a `BadHttpRequestException` with the status the framework chose -
400 for a binding failure, 413 for a body past the size limit - and logs it at Warning, below Error.
It is registered before `DatabaseExceptionHandler`.

```csharp
// Pot.AspNetCore/Concerns/ExceptionHandlers/DatabaseExceptionHandler.cs
// Note: Exception handlers are registered as a Singleton.
internal sealed class DatabaseExceptionHandler : IExceptionHandler
{
    // Will also catch UniqueConstraintException and ReferenceConstraintException
    private static readonly Type DbUpdateExceptionType = typeof(DbUpdateException);

    private readonly IProblemDetailsService _problemDetailsService;

    public DatabaseExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (exceptionType.IsDerivedFrom(DbUpdateExceptionType))
        {
            ProblemDetailsContext problemContext;

            if (exception.InnerException is PostgresException postgresException)
            {
                var errorDetail = new ApiDetailError(ErrorType.Server)
                {
                    ErrorCode = ErrorCodes.Database,
                    ErrorMessage = postgresException.MessageText
                };

                problemContext = ProblemDetailsContextFactory.Create(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    postgresException.MessageText,
                    exception,
                    [errorDetail]);
            }
            else
            {
                var errorDetail = new ApiDetailError(ErrorType.Server)
                {
                    ErrorCode = ErrorCodes.Database,
                    ErrorMessage = exception.Message
                };

                problemContext = ProblemDetailsContextFactory.Create(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    exception.Message,
                    exception,
                    [errorDetail]);
            }

            return await _problemDetailsService.TryWriteAsync(problemContext);
        }

        return false;
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
[Index("SiteId", nameof(Description), IsUnique = true)]  // Description unique per site
public sealed class AccountEntity : EntityBase
{
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
   // OneTimePasswordEntity.cs - see Pot.Data/Annotations/
   [Required]
   [MaxLength(6)]
   [OtpCode]
   public required string OtpCode { get; set; }
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
   modelBuilder.Entity<OneTimePasswordEntity>()
       .Property(otp => otp.AttemptCount)
       .HasDefaultValue(0);
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
   var accounts = await _accountRepository.GetAllAccountsWithLinkedCountsAsync(cancellationToken);

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
   // See Pot.Data/Specifications/ for examples
   return Accounts.Where(AccountSpecifications.IsSameDescription(description).Expression);
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
// Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs - AddHttpJsonOptions()
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

- `Frequency` - Days, Weeks, Months, Years, OneTime, EndOfMonth
- `UserStatus` - Enabled, Disabled, Pending, Approval
- `ApprovalStatus` - Approved, Rejected
- `Role` - Admin, Viewer
- `Permission` - site:manage/view, user:manage/view, account:manage/view, expense:manage/view, income:manage/view, maintenance:export/import
- `OtpStatus` - Active, Used, Invalidated, Expired, Failed
- `OtpReason` - Signup, PasswordReset
- `SettingCategory` - EmailBudgetReminder
- `RenewalMode` - Overdue, Future
- `AccrualPolicy` - Automatic, None

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

**Default:** Cascade delete is **disabled (Restrict)** for all foreign keys on `EntityBase`-derived entities. The EF-managed join tables (`UserRole`, `RolePermission`) keep the default `Cascade` behaviour.

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
[Index("SiteId", nameof(Description), IsUnique = true)]  // Description unique per site
public sealed class AccountEntity : EntityBase
{
    public required string Description { get; set; }
    public required SiteEntity Site { get; set; }
}
```

The unique index is the database's half of a rule `Pot.App` also checks. `CheckDescriptionDoesNotExist` rejects a duplicate
with a `409` naming the field; the index catches the request that races past that check, which `DatabaseExceptionHandler`
answers with a generic `409`. The description is `citext`, so the index - and the check - are case-insensitive.

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
[Index(nameof(ExpiryUtc))]
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

POT isolates tenants with two mechanisms that sit side by side. Four entities are filtered automatically by the caller's Site; `User` and `Site` deliberately are not, and are isolated by the repository surface instead. See [Security](#security) for the model as a whole.

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

    // Site-specific filter for Settings (a setting is always owned by a site)
    modelBuilder
        .Entity<SettingEntity>()
        .HasQueryFilter(setting => setting.Site.Id == GetCurrentUserSiteId());
}

internal int GetCurrentUserSiteId()
{
    _currentUserSiteId ??= Set<UserEntity>()
        .Include(user => user.Site)
        .Single(user => user.RowId == _currentUserContext.UserRowId)
        .Site.Id;

    return _currentUserSiteId.Value;
}
```

**How It Works:**

- Every query over one of those four entities is filtered to the current user's Site
- The site is resolved once per context and shared with the site-scoped repository reads, so the two cannot disagree and the lookup costs one query per request rather than one per call
- `UserEntity` and `SiteEntity` have no filter. Isolating them is the repository surface's job: the interfaces expose no `IQueryable` for either, so a lookup that omits the site cannot be written
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

**Use Case:** a read that genuinely has to cross sites. `IgnoreQueryFilters()` applies to the four filtered entities; `UserEntity` has no site filter, so calling it there is defensive rather than a bypass.

The only current use is the approval surface, which acts on every site behind the platform grant:

```csharp
// Pot.Data/Repositories/Users/UserRepository.cs
public Task<List<UserEntity>> GetPendingApprovalsAsync(CancellationToken cancellationToken)
{
    // Cross-tenant: the approval surface acts on every site.
    return Set<UserEntity>()
        .IgnoreQueryFilters()
        .Where(user => user.Status == UserStatus.Approval)
        .OrderBy(user => user.Username)
        .ToListAsync(cancellationToken);
}
```

Every such read is served by a repository method whose name says it crosses sites, and the call site records why.

### Using Query Filters

1. **Use `IgnoreQueryFilters()`** only when needed

   ```csharp
   // The approval surface reads across sites on purpose, behind the platform grant
   var approvals = Set<UserEntity>()
       .IgnoreQueryFilters()
       .Where(user => user.Status == UserStatus.Approval);
   ```

2. **Document why filters are bypassed**

   ```csharp
   // Cross-tenant: the approval surface acts on every site.
   return Set<UserEntity>()
       .IgnoreQueryFilters()
       .AnyAsync(user => user.Status == UserStatus.Approval);
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
            entity.Etag = DateTime.UtcNow.GetEtag();  // Unix millisecond timestamp
        }
    }
}
```

**ETag Values:**

- Generated from `DateTime.UtcNow.GetEtag()`, which returns a Unix millisecond timestamp (long)
- Updated automatically on insert and update operations
- Millisecond precision - entities saved in the same operation share the same value

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
  "etag": 1758237600000
}

// PUT request includes ETag
{
  "rowId": "550e8400-e29b-41d4-a716-446655440000",
  "description": "Monthly Rent",
  "etag": 1758237600000  // Must match database value
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

### EF CLI Tool Prerequisite

If `dotnet ef` is not recognized, install (or update) the EF CLI tool:

```bash
dotnet tool install --global dotnet-ef
```

```bash
dotnet tool update --global dotnet-ef
```

Verify installation:

```bash
dotnet ef --version
```

### Creating Migrations

**Using .NET CLI** (Recommended):

```bash
# From Source/Server directory
dotnet ef migrations add MigrationName --project Pot.Data --startup-project Pot.Data.Migrations
```

**Using Visual Studio Package Manager Console:**

```powershell
# Set Pot.Data.Migrations as startup project and Pot.Data as Default project
Add-Migration MigrationName -Project Pot.Data -StartupProject Pot.Data.Migrations
```

**Migration Naming Conventions:**

- Use PascalCase
- Be descriptive: `AddUserApprovalStatus`, `UpdateAccountIndexes`
- Prefix with action verb: `Add`, `Update`, `Remove`, `Create`

### Applying Migrations

**Using .NET CLI:**

```bash
cd Source/Server
dotnet ef database update --project Pot.Data --startup-project Pot.Data.Migrations
```

**Using Visual Studio:**

```powershell
Update-Database -Project Pot.Data -StartupProject Pot.Data.Migrations
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
    .RunConsoleAsync(options => options.SuppressStatusMessages = true);

// App.cs applies migrations (after waiting for the database to be ready)
await _databaseMigrator.MigrateAsync();  // IDatabaseMigrator -> PotDbMigrator
```

### Rollback Migrations

```bash
# .NET CLI
dotnet ef database update PreviousMigrationName --project Pot.Data --startup-project Pot.Data.Migrations

# Visual Studio
Update-Database -Migration PreviousMigrationName -Project Pot.Data -StartupProject Pot.Data.Migrations
```

### Remove Last Migration (not yet applied)

```bash
# .NET CLI
dotnet ef migrations remove --project Pot.Data --startup-project Pot.Data.Migrations

# Visual Studio
Remove-Migration -Project Pot.Data -StartupProject Pot.Data.Migrations
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
            .WithOrigins(_corsConfiguration.GetAllowedOrigins().ToArray())

            .AllowAnyMethod()
            .AllowAnyHeader()

            // Required for authentication (cookies/tokens)
            .AllowCredentials()

            // Expose the download filename and the 429 cooldown
            .WithExposedHeaders("content-disposition", "retry-after", CorrelationIdMiddleware.HeaderName)

            // Cache preflights for ten minutes instead of Chromium's five-second default
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
}
```

### Why Expose `content-disposition`?

The frontend export feature needs to extract filenames from the `Content-Disposition` header. By default, browsers only expose "safe" CORS headers (`Content-Type`, `Cache-Control`, etc.). `retry-after` is exposed for the 429 cooldown, and `X-Correlation-Id` for the id to quote to support. The ten-minute preflight cache keeps most API calls from paying an extra `OPTIONS` round trip.

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
    "AllowedOrigins": "http://localhost:5175" // Vite dev server (delimited string, not an array)
  }
}
```

**Important:** The ASP.NET Core CORS policy cannot use `AllowAnyOrigin()` with `AllowCredentials()` - must specify explicit origins.

**Startup validation:** The API validates its JWT, authentication-cookie, platform-admin, CORS, SMTP, and database options during startup in every environment. Each environment must provide values that pass its registered validators; invalid configuration prevents the API from starting. Platform-admin user IDs may be omitted, and authentication-cookie settings have defaults.

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

    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
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

    builder.Services.AddSingleton<IProblemDetailsInspector, ProblemDetailsInspector>();

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

Accrual is derived on read, never stored. `Pot.App/Calculators/AccrualCalculator.cs` computes it for an as-of date from the accrual-relevant facts plus each expense's schedule cursor, and every accrual-bearing read obtains its values from that one engine, so the values read are always the current ones.

The server tracks two different daily accrual concepts for expenses.

### Dynamic Accrual (`DailyExpenseAccrual`)

- Purpose: operational projection behavior and event-date simulation.
- Behavior: changes through each cycle as due dates approach and renew.
- Primary use: projections and date-by-date cashflow mechanics.

### Stable Accrual (`StableExpenseAccrual`)

- Purpose: long-run daily funding guidance.
- Behavior: designed to be more stable than dynamic accrual.
- Recurring rule: `Amount / Frequency.GetAverageDaysToNext(FrequencyCount)`.
- One-time rule: fixed bounded period contribution before due date only.

### Accrual Policy Gate

`AccrualPolicy.None` explicitly disables accrual contribution.

- No contribution to `TotalExpenseAccrued`
- No contribution to `DailyExpenseAccrual`
- No contribution to `StableExpenseAccrual`

This preserves due-date debit behavior while disabling pre-funding accrual behavior. It does **not** disable arrears: `TotalArrears` follows the schedule, so a `None`-policy row whose due date has passed still carries it.

### Account-Level Obligation Terms

- `TotalExpenseAccrued`: the accrual of the cycles in progress.
- `TotalArrears`: one `Amount` per past-due, un-settled occurrence.
- `TotalCommitted`: `TotalExpenseAccrued + TotalArrears`, rendered as the `Committed` column on the accounts table.
- `Available = Balance - Reserved - TotalCommitted` on the accounts read. The projections read publishes no derived `Available`: it returns the running forecast `balance` plus `reserved`, `arrears` and `unpaidAccrual`, and the chart composes the value it plots as `balance - reserved - unpaidAccrual - arrears`.

A bill due today is the current bill: it accrues in full and is **not** counted as arrears.

---

## Available Commands

### Development

| Command                                 | Description                                        |
| --------------------------------------- | -------------------------------------------------- |
| `dotnet run --project Pot.AspNetCore`   | Start API server (typically http://localhost:5242) |
| `dotnet build`                          | Build solution                                     |
| `dotnet watch --project Pot.AspNetCore` | Run API with hot reload                            |

### Migrations

| Command                                                                                              | Description                     |
| ---------------------------------------------------------------------------------------------------- | ------------------------------- |
| `dotnet ef migrations add <Name> --project Pot.Data --startup-project Pot.Data.Migrations`           | Create new migration            |
| `dotnet ef database update --project Pot.Data --startup-project Pot.Data.Migrations`                 | Apply pending migrations        |
| `dotnet ef migrations remove --project Pot.Data --startup-project Pot.Data.Migrations`               | Remove last unapplied migration |
| `dotnet ef database update <MigrationName> --project Pot.Data --startup-project Pot.Data.Migrations` | Rollback to specific migration  |
| `dotnet run --project Pot.Data.Migrations`                                                           | Run migrations console app      |

### Testing & Quality

| Command                                       | Description                                                             |
| --------------------------------------------- | ----------------------------------------------------------------------- |
| `dotnet test`                                 | Run all unit tests                                                      |
| `dotnet test --collect:"XPlat Code Coverage"` | Run tests with code coverage                                            |
| `pwsh -NoProfile -File ./code_coverage.ps1`   | Tests + HTML coverage report (VS Code task: `server-run-test-coverage`) |
| `dotnet format`                               | Format code using .editorconfig                                         |

---

## Additional Resources

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)

---

**For frontend integration, see:** `Source/Client/pot-react/DEVELOPER.md`

**For Docker setup, see:** `Source/Docker/DEVELOPER.md`
