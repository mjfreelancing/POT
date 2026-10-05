# Cross-Tenant Authorization Hardening PRD

**Status:** Planning (design complete; awaiting review before Planned)
**Priority:** High
**Last Updated:** 2026-10-05
**Feature ID:** 026
**Scope:** Make tenant isolation enforceable rather than conventional. POT is multi-tenant, but isolation is enforced in only two places — site-scoped global query filters on four entity types, and `resource:action` permissions — and neither binds a _target row_ to the caller's tenant. Any endpoint that resolves a user or a site by the id supplied by the caller therefore reaches every tenant. This document specifies: a repository surface on which a tenant-unbound user or site lookup cannot be written; a single refusal contract (a target outside the caller's site is reported exactly as a non-existent one); fail-closed authorization so a route cannot be anonymous by accident; the two protected identities the user-mutation routes must not be able to destroy (a configured platform administrator, and a site's last enabled `Admin`); and the cross-tenant operations that are deliberately allowed, named and documented. It is the prerequisite for the user and site deletion feature (PRD-022) and for platform administration (PRD-027), and it defines the shared primitives both reuse.

## 1. Purpose

Guarantee that a signed-in user of one site cannot read or write another site's users or settings by supplying a `Guid` `RowId`, and that no user-mutation route can leave a tenant without an administrator — without relying on every future endpoint remembering to add a predicate.

The hardening must be structural, not per-route. Adding a site predicate to the five routes that are wrong today closes today's defects and leaves the next id-taking endpoint exposed; the surface that made them wrong must change.

## 2. Problem

### 2.1 How isolation works today

| Mechanism                        | Where                                                                             | Binds a caller to a target row?                                                                                                       |
| -------------------------------- | --------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Site-scoped global query filters | `PotDbContext.SetupQueryFilters` — `Account`, `Expense`, `Income`, `Setting` only | Partly: filters those four entity types; `User` and `Site` have no filter at all                                                      |
| Permission policies              | `RequireAuthorization("<resource>:<action>")` per route                           | **No**: a permission says what a caller may do, never which tenant's row they may do it to                                            |
| Current-site resolution          | `SiteRepository.GetCurrentSite()`, `UserRepository.GetCurrentUser(true)`          | Yes, but only used to _attach_ data to the caller's site (for example when inviting a user); never to bind an id supplied by a caller |
| Current-caller identity          | `UserContextMiddleware` sets `ICurrentUserContext.UserRowId` from the JWT `sub`   | Available, but not consulted by any user- or site-targeting service                                                                   |

There is no global fallback authorization policy. `AddPotAuth` registers the `AuthenticatedUser` policy only, and the pipeline calls `UseAuthentication()` / `UseAuthorization()` with no default requirement, so an endpoint that omits `RequireAuthorization(...)` has no authorization requirement whatsoever.

### 2.2 Why it fails

Two independent properties combine:

1. **`UserEntity` and `SiteEntity` are unfiltered.** `UserRepository.Users` is `Set<UserEntity>()` and `SiteRepository.Sites` is `Set<SiteEntity>()`, so both `DbSet`s return rows from every site.
2. **The repositories expose those sets as raw `IQueryable`s.** Any service can write `_userRepository.Users.SingleOrDefaultAsync(user => user.RowId == input.RowId)` — a lookup with no site predicate — and it compiles.

`PermissionService` resolves permissions per request from the database plus the configured platform-administrator list, and never consults the caller's site, so `user:manage` in site A satisfies `RequireAuthorization("user:manage")` for a target in site B.

The result is a class of defect, not a set of five bugs: **any request-reachable endpoint that resolves a `UserEntity` or `SiteEntity` by an id from the request is cross-tenant unless it adds a site predicate by hand.**

### 2.3 Evidence

`Pot.AspNetCore.Integration.Tests/Security/CrossTenantAuthorizationFixture.cs` exercises the routes over HTTP against the real host. Each seeded user is placed in its own site, so a seeded pair is a caller in one site and a target in another. The suite reads the target's etag from the test database (sign-in records itself on the user row and changes its etag), so each case exercises authorization rather than the concurrency check.

Current results — 3 pass, 10 fail:

| Case                                                                     | Observed                | Required                                         |
| ------------------------------------------------------------------------ | ----------------------- | ------------------------------------------------ |
| Anonymous update of a user's details                                     | `200` — details changed | `401`                                            |
| Anonymous update of an unknown user's details                            | `404`                   | `401` (authorization precedes target resolution) |
| Update of another site's user details, as an admin                       | `200` — details changed | `404`                                            |
| Update of another site's user details, as a caller without `user:manage` | `200` — details changed | `404`                                            |
| Change of another site's user status                                     | `200` — user disabled   | `404`                                            |
| Grant of a role to another site's user                                   | `200` — `Admin` granted | `404`                                            |
| Resend invite for another site's user                                    | `200` — password reset  | `404`                                            |
| Update of another site                                                   | `200` — site renamed    | `404`                                            |
| Disable the site's last enabled `Admin`                                  | `200` — admin disabled  | `422`                                            |
| Remove `Admin` from the site's last enabled `Admin`                      | `200` — role removed    | `422`                                            |
| A user updates their own details                                         | `200`                   | `200` (control)                                  |
| An admin updates their own site                                          | `200`                   | `200` (control)                                  |
| Change the status of a user that does not exist                          | `404`                   | `404` (control)                                  |

The three controls are the behaviour that must survive the hardening.

## 3. Issues

Each issue states the use case, the code today, and the code after the hardening.

### I1 — `PUT /api/users/{id}` requires no authorization and binding

**Use case.** The route exists so a user can change their own display name and email, which is why it was left open to non-administrators. In practice an anonymous caller who knows a `RowId` (or guesses one) and can supply the matching etag changes any user's details in any site.

**Current**

```csharp
// Pot.AspNetCore/Features/Users/Extensions/RouteGroupBuilderExtensions.cs
public static RouteGroupBuilder UpdateUser(this RouteGroupBuilder routeGroupBuilder)
{
    // There's no RequireAuthorization here since the user needs to be able to change their own display / email details
    routeGroupBuilder
        .MapPut(UsersEndpoints.Update, Update.Handler.Invoke)
        //.RequireAuthorization("user:manage")
        .WithName(nameof(UpdateUser));
    ...
}
```

```csharp
// Pot.App/Features/Users/Update/UpdateUserService.cs
var userToUpdate = await _userRepository.Users
    .SingleOrDefaultAsync(user => user.RowId == input.RowId, cancellationToken)
    .ConfigureAwait(false);
```

**New**

```csharp
// Pot.AspNetCore/Features/Users/Extensions/RouteGroupBuilderExtensions.cs
public static RouteGroupBuilder UpdateUser(this RouteGroupBuilder routeGroupBuilder)
{
    // Any signed-in user may edit their own details; editing anyone else needs user:manage. The route is
    // never anonymous, and a target outside the caller's site is refused as not-found.
    routeGroupBuilder
        .MapPut(UsersEndpoints.Update, Update.Handler.Invoke)
        .RequireAuthorization("AuthenticatedUser")
        .WithName(nameof(UpdateUser))
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    ...
}
```

```csharp
// Pot.AspNetCore/Features/Users/Update/Handler.cs
logger.LogCall(null);

// Without user:manage the caller may only target themselves. Anything else is refused as not-found so
// the response cannot be used to probe for the existence of other users.
if (!IsSelf(user, id) && !await IsAuthorizedAsync(authorizationService, user, "user:manage"))
{
    return TypedResults.NotFound();
}
```

```csharp
// Pot.App/Features/Users/Update/UpdateUserService.cs
// Resolves only a row that belongs to the caller's site; a foreign id and a missing id are indistinguishable.
var userToUpdate = await _userRepository
    .GetForCurrentSiteAsync(input.RowId, cancellationToken)
    .ConfigureAwait(false);
```

**After:** anonymous `401`; self `200`; another user without `user:manage` → `404`; another user in the caller's own site with `user:manage` → `200`; any other site → `404`.

### I2 — `PUT /api/users/{id}/roles` is cross-tenant

**Use case.** A `user:manage` holder in site A replaces the roles of a user in site B, including granting or removing `Admin` — privilege escalation inside another tenant.

**Current**

```csharp
// Pot.App/Features/Users/UpdateRoles/UpdateUserRolesService.cs
var userToUpdate = await _userRepository.Users
    .Include(user => user.Roles)
    .SingleOrDefaultAsync(user => user.RowId == input.RowId, cancellationToken)
    .ConfigureAwait(false);
```

**New**

```csharp
var userToUpdate = await _userRepository
    .GetForCurrentSiteAsync(input.RowId, cancellationToken)
    .ConfigureAwait(false);

if (userToUpdate is null)
{
    return EnrichedResult.Fail<Output>(ApiDetailErrorFactory.CreateEntityNotFoundError(userId, "The user does not exist"));
}
```

**After:** a foreign id is refused as not-found and no role is written. The last-enabled-`Admin` rule (I6) is evaluated only for a target the caller can already act on.

### I3 — `PUT /api/users/{id}/status` is cross-tenant

**Use case.** A `user:manage` holder in site A disables a user in site B — a denial of service against that tenant, and (see I6) a way to lock a tenant out of its own administration.

**Current**

```csharp
// Pot.App/Features/Users/UpdateStatus/UpdateUserStatusService.cs
var userToUpdate = await _userRepository.Users
    .SingleOrDefaultAsync(user => user.RowId == input.RowId, cancellationToken)
    .ConfigureAwait(false);
```

**New**

```csharp
var userToUpdate = await _userRepository
    .GetForCurrentSiteAsync(input.RowId, cancellationToken)
    .ConfigureAwait(false);
```

**After:** a foreign id is refused as not-found and no status is written.

### I4 — `POST /api/users/{id}/resend-invite` is cross-tenant, and has no concurrency token

**Use case.** A `user:manage` holder in site A forces a credential reset on a user in site B, overwriting that user's `PasswordHash` and emailing them a new temporary password. Unlike the other mutations, this route takes **no request body and no etag** — the handler accepts only the id, and the service reads nothing else — so there is no concurrency guard standing between the caller and the write.

**Current**

```csharp
// Pot.AspNetCore/Features/Users/ResendInvite/Handler.cs
public static async Task<Results<Ok, ProblemHttpResult>> Invoke(Guid id, IResendInviteService resendInviteService, ...)
{
    var output = await resendInviteService.ResendInviteAsync(id, cancellationToken);
    ...
}
```

```csharp
// Pot.App/Features/Users/ResendInvite/ResendInviteService.cs
var user = await _userRepository.Users
    .SingleOrDefaultAsync(user => user.RowId == userRowId, cancellationToken)
    .ConfigureAwait(false);
```

**New**

```csharp
var user = await _userRepository
    .GetForCurrentSiteAsync(userRowId, cancellationToken)
    .ConfigureAwait(false);

if (user is null)
{
    return EnrichedResult.Fail<bool>(ApiDetailErrorFactory.CreateEntityNotFoundError(userRowId, "User not found"));
}
```

**After:** a foreign id is refused as not-found and the stored password hash is unchanged. Whether this route should also require a concurrency token, or accept only users in a `Pending` status, is an open question (§10).

### I5 — `PUT /api/sites/{id}` is cross-tenant

**Use case.** The route reads as "update the current site", and the client sends the current site's id — but the server resolves whatever id it is given. A `site:manage` holder in site A renames or re-describes any site in the deployment. The lambda parameter is also named `user` in a query against `Sites`.

**Current**

```csharp
// Pot.App/Features/Sites/Update/UpdateSiteService.cs
var siteToUpdate = await _siteRepository.Sites
    .SingleOrDefaultAsync(user => user.RowId == input.RowId, cancellationToken)
    .ConfigureAwait(false);
```

**New**

```csharp
// The route updates the caller's own site only; any other id is indistinguishable from a missing one.
var currentSite = _siteRepository.GetCurrentSite();

if (currentSite.RowId != input.RowId)
{
    return EnrichedResult.Fail<Output>(
        ApiDetailErrorFactory.CreateEntityNotFoundError(siteId, "The site does not exist"));
}
```

**After:** the caller's own site updates as before; any other id is refused as not-found. `ISiteRepository` no longer exposes a queryable set at all.

### I6 — The protected identities are unreachable through these routes

**Use case.** The deletion feature (PRD-022) refuses to delete a configured platform administrator and refuses to delete a site's last enabled `Admin`, because either would leave the platform or the tenant unadministrable. Neither refusal is worth anything while the same outcome is one status or role change away: a sole `Admin` disables themselves, or strips their own `Admin` role, and the tenant has no administrator. A configured platform administrator can be disabled the same way, and because `platform:manage` is granted from deployment configuration and is not revocable in-app, there is no in-app remedy.

**Current**

```csharp
// Pot.App/Features/Users/UpdateStatus/EntityChecks/Checks — the only pre-write check is the etag
public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
{
    if (state.UserEtag != state.Input.Etag)
    {
        return ApiDetailErrorFactory.CreateEtagConflict("User", state.Input.Etag);
    }

    return await base.HandleAsync(state, cancellationToken);
}
```

```csharp
// Pot.App/Features/Users/UpdateRoles/EntityChecks/Checks — etag plus "do these roles exist", nothing else
```

**New**

```csharp
// Pot.App/Features/Users/UpdateStatus/EntityChecks/Checks/CheckTargetIsNotPlatformAdmin.cs
internal sealed class CheckTargetIsNotPlatformAdmin : PreUpdateCheckBase
{
    public override Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        if (state.Input.Status != UserStatus.Enabled && state.PlatformAdminRowIds.Contains(state.Target.RowId))
        {
            return Task.FromResult<ApiDetailError?>(ApiDetailErrorFactory.CreateUnprocessableEntityError(
                nameof(Input.Status),
                state.Input.Status.Name,
                "A platform administrator cannot be disabled."));
        }

        return base.HandleAsync(state, cancellationToken);
    }
}
```

```csharp
// Pot.App/Features/Users/UpdateStatus/EntityChecks/Checks/CheckTargetIsNotLastEnabledSiteAdmin.cs
if (state.Input.Status != UserStatus.Enabled && await IsOnlyEnabledSiteAdminAsync(state, cancellationToken))
{
    return ApiDetailErrorFactory.CreateUnprocessableEntityError(
        nameof(Input.Status),
        state.Input.Status.Name,
        "This is the site's only enabled administrator. Promote or enable another administrator first.");
}
```

```csharp
// Pot.App/Features/Users/UpdateRoles/EntityChecks/Checks/CheckRemovingAdminLeavesAnAdministrator.cs
if (RemovesAdmin(state.Input.RoleIds) && await IsOnlyEnabledSiteAdminAsync(state, cancellationToken))
{
    return ApiDetailErrorFactory.CreateUnprocessableEntityError(
        nameof(Input.RoleIds),
        string.Join(", ", state.Input.RoleIds),
        "This is the site's only enabled administrator. Promote another administrator first.");
}
```

The configured platform-administrator ids are supplied by the status handler through the service `Input`, exactly as the signup flow supplies them for approval emails, because they come from deployment configuration rather than the database.

**After:** both refusals return `422` with actionable copy, and the target row is unchanged. Evaluation order: platform administrator first, then last enabled `Admin`; the first failure wins.

### I7 — The defect class, not just the five routes

**Use case.** `UserRepository.Users` and `SiteRepository.Sites` are public `IQueryable`s. That is the mechanism every affected service used, and it is available to every future service. The same applies to the authorization default: because no fallback policy exists, a route that simply forgets `.RequireAuthorization(...)` ships as anonymous.

**Current**

```csharp
// Pot.Data/Repositories/Users/IUserRepository.cs
public interface IUserRepository : IRepositoryBase
{
    IQueryable<UserEntity> Users { get; }        // any service can query across all sites
    UserEntity GetCurrentUser(bool includeSite);
    Task<List<GetAllUserInfo>> GetAllForCurrentSiteAsync(CancellationToken cancellationToken);
}
```

```csharp
// Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs — no fallback, so an unannotated route is anonymous
.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
})
```

**New**

```csharp
// Pot.Data/Repositories/Users/IUserRepository.cs
public interface IUserRepository : IRepositoryBase
{
    // Site-scoped: resolves a row only when it belongs to the caller's site. A foreign id and a missing
    // id both return null, so callers cannot distinguish them.
    Task<UserEntity?> GetForCurrentSiteAsync(Guid rowId, CancellationToken cancellationToken);
    UserEntity GetCurrentUser(bool includeSite);
    Task<List<GetAllUserInfo>> GetAllForCurrentSiteAsync(CancellationToken cancellationToken);
    Task<List<GetAllUserInfo>> GetEnabledAdminsForCurrentSiteAsync(CancellationToken cancellationToken);

    // Deliberately cross-tenant: sign-in resolves a username before a site is known. Named to read as such.
    Task<UserEntity?> GetByUsernameForAuthenticationAsync(string username, CancellationToken cancellationToken);
    Task<UserSecurityState?> GetSecurityStateAsync(Guid userRowId, CancellationToken cancellationToken);
    Task<List<GetAllUserInfo>> GetEnabledUsersForAllSitesAsync(CancellationToken cancellationToken);
}
```

```csharp
// Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs
.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());

    // Fail closed: an endpoint that declares no requirement is authenticated-only rather than anonymous.
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
})
```

Every intentionally anonymous route is then marked `AllowAnonymous()` explicitly, which turns the set of anonymous routes into a reviewable list.

**After:** a tenant-unbound user or site lookup no longer compiles, and a forgotten annotation produces a `401` instead of an open endpoint.

## 4. Design

### 4.1 Repository surface

`IUserRepository`/`IPersistableUserRepository` and `ISiteRepository`/`IPersistableSiteRepository` stop exposing `IQueryable`. Callers choose from intent-named methods:

- **Site-scoped** (the default): `GetForCurrentSiteAsync`, `GetCurrentUser`, `GetAllForCurrentSiteAsync`, `GetEnabledAdminsForCurrentSiteAsync`, `GetCurrentSite`.
- **Cross-tenant** (explicit, documented, and few): the named methods in §4.5.

`ISiteRepository.Sites` is removed entirely; the site update route compares the supplied id with `GetCurrentSite()`.

This is the load-bearing part of the design: it makes the hazardous lookup unrepresentable rather than merely discouraged, and it is what a global query filter would otherwise have provided, without touching the authentication pipeline.

### 4.2 Refusal contract

A target the caller cannot act on is reported as **`404`, identical to an id that does not exist** — never `403`. A `403` confirms the row exists in another tenant and turns the endpoint into a membership oracle. Because the site-scoped lookup returns `null` for both cases, the contract is a property of the data access rather than a rule each handler must remember. Cross-tenant refusals are logged at warning level with the caller's `RowId`, the caller's site and the requested id, so probing is visible.

### 4.3 Fail-closed authorization

A `RequireAuthenticatedUser` fallback policy makes an unannotated endpoint authenticated-only, and intentionally anonymous routes are marked `AllowAnonymous()`. An integration test enumerates the host's `EndpointDataSource` and asserts that every mapped endpoint either carries an authorization requirement or appears on an explicit anonymous allow-list, and that every user- and site-targeting route carries its expected requirement. A forgotten annotation becomes a failing test rather than a shipped vulnerability.

### 4.4 Protected identities

`PUT /api/users/{id}/status` refuses (`422`) any change that would set a configured platform administrator's status to anything other than `Enabled`, and any change that would leave a site with no enabled `Admin`. `PUT /api/users/{id}/roles` refuses any change that removes `Admin` from that site's last enabled `Admin`. The population counted is enabled `Admin`-role holders in the target's site, shared with PRD-022 so the two cannot disagree. These checks run only after the target has passed the site-scoped lookup, so a foreign id is still a not-found rather than an unprocessable entity.

### 4.5 Cross-tenant inventory

Every read across sites is deliberate, named as such, and listed here. This is the complete set; anything not on the list must not exist.

| Call site                                                            | Why it is cross-tenant                                                               | Method                                                                                   |
| -------------------------------------------------------------------- | ------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------- |
| `AuthService.LoginAsync`                                             | Sign-in resolves a username before a site is known                                   | `GetByUsernameForAuthenticationAsync`                                                    |
| `AuthService.ChangePasswordAsync`, `Me/UserService.GetUserInfoAsync` | Resolve the authenticated caller from the token's `sub`                              | `GetForCurrentSiteAsync` via `ICurrentUserContext` where a site exists; `GetCurrentUser` |
| `JwtBearerEventsSetup.OnTokenValidated`                              | Token-version and status check runs before any user context exists                   | `GetSecurityStateAsync`                                                                  |
| `RoleRepository.GetRolesForUserAsync`                                | Permission resolution runs during `UseAuthorization`, before `UserContextMiddleware` | named method with an XML `<remarks>` recording the invariant                             |
| `BudgetReminderEmailWorker` → `GetAllEnabledAdminsAsync`             | Enumerates every site's enabled administrators, then re-scopes per user              | `GetEnabledUsersForAllSitesAsync`                                                        |
| `VerifySignupService` approval notification                          | Signup completes anonymously; looks up the configured platform administrators by id  | `GetByIdsForPlatformNotificationAsync`                                                   |
| `Approvals/{Pending,UpdateStatus}`                                   | The deliberate platform-administrator surface behind `platform:manage`               | named methods documented as cross-tenant                                                 |

`Source/Server/DEVELOPER.md` is corrected to match: query filters cover `Account`, `Expense`, `Income` and `Setting` only; `User` and `Site` targets must be bound explicitly; cross-tenant reads are the named exceptions above.

### 4.6 Token and claim policy

The access token carries `jti`, `sub` and `tokenVersion` and nothing else. Permissions, account status and tenancy are resolved from the database on every request. Site and permission claims are deliberately **not** added:

- A JWT payload is base64url-encoded, not encrypted, so a claim is readable by anyone holding the token.
- Claims go stale. Roles, status and site membership change; a token stays valid until it expires. Per-request resolution is why `TokenVersion` exists, and embedding derived state would reopen the revocation window it closes.
- Tenancy is a property of the **target**, not the caller. A caller-site claim does not tell a handler whether the target belongs to that site, so the target must be loaded regardless.

### 4.7 Why not a query filter on `User` and `Site`

A site-scoped filter on `User` and `Site` was considered and rejected as the primary mechanism, because it conflicts with code that legitimately runs before a user context exists:

- Permission resolution happens during `UseAuthorization`, which runs **before** `UserContextMiddleware`, so `RoleRepository.GetRolesForUserAsync` would be filtered by a site that has not been set.
- `JwtBearerEventsSetup.OnTokenValidated` reads the user with no context at all.
- `PotDbContext.GetCurrentUserSiteId()` itself reads `Set<UserEntity>()`, so a `User` filter is self-referential.
- Authentication by username, the reminder worker and Approvals are cross-tenant by design and would each need an `IgnoreQueryFilters()` opt-out.

The repository surface in §4.1 delivers the same guarantee — the unsafe query cannot be written — without those interactions. Adding filters on top remains possible later; it is not required for this feature and is out of scope.

## 5. Route contract

| Route                                | Authorization                                                            | Target resolution                                                                 | Refusal                                                        |
| ------------------------------------ | ------------------------------------------------------------------------ | --------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| `PUT /api/users/{id}`                | `AuthenticatedUser`; caller must be the target **or** hold `user:manage` | `GetForCurrentSiteAsync`                                                          | `401` anonymous; `404` for any target the caller cannot act on |
| `PUT /api/users/{id}/roles`          | `user:manage`                                                            | `GetForCurrentSiteAsync` + last-enabled-`Admin` check                             | `401` / `404` / `422`                                          |
| `PUT /api/users/{id}/status`         | `user:manage`                                                            | `GetForCurrentSiteAsync` + platform-administrator and last-enabled-`Admin` checks | `401` / `404` / `422`                                          |
| `POST /api/users/{id}/resend-invite` | `user:manage`                                                            | `GetForCurrentSiteAsync`                                                          | `401` / `404`                                                  |
| `PUT /api/sites/{id}`                | `site:manage`                                                            | supplied id must equal `GetCurrentSite()`                                         | `401` / `404`                                                  |

`platform:manage` carries no authority on these routes: a platform administrator acting on another tenant uses the dedicated routes in PRD-027. Within the caller's own site, `platform:manage` is not required and confers nothing extra.

## 6. Shared primitives for PRD-022 and PRD-027

| Primitive                                                                         | Consumer                                                                                     |
| --------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `GetForCurrentSiteAsync` (user) — site-scoped, null for foreign and missing alike | PRD-022's `DELETE /api/users/{id}` and `DELETE /api/me`; PRD-027's foreign-target resolution |
| `GetEnabledAdminsForCurrentSiteAsync` — enabled `Admin`-role holders in a site    | PRD-022's `CheckTargetIsNotLastEnabledSiteAdmin`; PRD-026 I6                                 |
| Configured platform-administrator check supplied through the service `Input`      | PRD-022's `CheckTargetIsNotPlatformAdmin`; PRD-026 I6                                        |
| Cross-tenant reads as named, documented repository methods                        | PRD-027's platform-administrator lookups                                                     |
| The `404`-not-`403` refusal contract                                              | PRD-022 and PRD-027 refusal handling                                                         |
| Fail-closed authorization and the endpoint-metadata regression test               | Every route added by PRD-022 and PRD-027                                                     |

PRD-022 owns the delete routes and the deletion sweeps; PRD-027 owns the cross-tenant platform routes. This document owns the primitives and the isolation guarantees they depend on.

## 7. Tests

The acceptance suite is `Pot.AspNetCore.Integration.Tests/Security/CrossTenantAuthorizationFixture.cs`, rewritten to the §5 contract. It covers every issue above, including the anonymous cases expecting `401` rather than `404`, the self-edit and own-site controls, a status change for an unknown user expecting the same `404` as a foreign target, and the two protected-identity refusals. It fails until the implementation lands — 10 of its 13 cases are red today and 3 pass (the controls).

Supporting coverage:

- **Repository** (`Pot.Data.Tests`): a same-site id returns the user; a foreign-site id returns null; `GetEnabledAdminsForCurrentSiteAsync` counts enabled `Admin`-role holders only, ignores another site's `Admin`, and ignores a `Viewer`.
- **Application** (`Pot.App.Tests`): each of the five services refuses a foreign target with the not-found error and performs no write; a same-site target succeeds; the new entity checks refuse the protected-identity cases and allow a second enabled `Admin`.
- **Integration** — authorization regression: every mapped endpoint carries a requirement or is on the anonymous allow-list; user- and site-targeting routes carry their expected requirement.
- **Integration** — permission regression: `platform:manage` still reaches the Approvals endpoints, and a `user:manage` caller without it does not.
- **Integration** — refusal logging: a cross-tenant rejection is logged at warning with the caller's `RowId`, site and the requested id.

Two cases need test-host support before they can be written, and are added with the implementation:

- **A configured platform administrator cannot be disabled.** The host currently configures an empty platform-administrator list, so the fixture needs a way to nominate a seeded user — a configuration seam on the host factory.
- **A second enabled `Admin` allows the change.** Seeding places each user in its own site, so the fixture needs a way to add a user to an existing site.

## 8. Manual verification (UI)

Automated coverage cannot reach every affected journey: the E2E seed is a single site, so no browser test can observe a cross-tenant refusal, and the client has no surface for the protected-identity refusals. The journeys below are exercised by hand in the running app before this feature is considered done. Each is a path whose **server contract changes**, so the point is to confirm the client still behaves — and, where a request is now expected to be refused, that it fails visibly rather than silently.

| #   | Area                   | Journey                                                                                         | Confirm                                                                                                                 |
| --- | ---------------------- | ----------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| M1  | Authentication         | Sign in, sign out, and refresh (reload after the access token expires)                          | All succeed; sign-out clears the session and returns to `/login`                                                        |
| M2  | Authentication         | Request a password reset and verify the code                                                    | Still reachable while signed out (it must remain anonymous)                                                             |
| M3  | Authentication         | Complete a signup                                                                               | Still reachable while signed out; the user lands in `Approval` and appears for a platform administrator                 |
| M4  | Health                 | Request `/_health` with no credentials                                                          | Still returns healthy                                                                                                   |
| M5  | Profile (self-service) | As a **Viewer**, user menu -> Settings -> User Details -> change display name and email -> save | Succeeds, the success toast appears, and the header name updates — this is the journey `PUT /api/users/{id}` exists for |
| M6  | Site details           | As a **site admin**, Settings -> Site Details -> change the name and description -> save        | Succeeds and the toast appears                                                                                          |
| M7  | Site details           | As a **Viewer**, open Site Details                                                              | Shown read-only (`View only`); the submit button is absent or disabled                                                  |
| M8  | Users (site admin)     | Invite a user                                                                                   | The invited user appears with a `Pending` status                                                                        |
| M9  | Users (site admin)     | Open a `Pending` row's menu -> Resend Invitation                                                | Success toast; the invitation is queued (delivery needs SMTP configured in the environment)                             |
| M10 | Users (site admin)     | Open an `Enabled` row's menu -> Disable User, then Enable User                                  | Status badge and toast reflect each change                                                                              |
| M11 | Users (site admin)     | Open a row's menu -> Change Role -> save                                                        | The role badge updates; the saved roles match the selection                                                             |
| M12 | Users (Viewer)         | Open the Users page                                                                             | Read-only: no row action menus, no Invite User button                                                                   |
| M13 | Refusal presentation   | As the site's only enabled `Admin`, try to disable yourself from the Users list                 | Refused with the protected-identity message (not silently ignored, not a success toast)                                 |
| M14 | Refusal presentation   | As the site's only enabled `Admin`, try to remove your own `Admin` role                         | Refused with the protected-identity message                                                                             |
| M15 | Refusal presentation   | Force a stale save (open a form, change the row elsewhere, then save)                           | The existing conflict presentation still appears                                                                        |
| M16 | Approvals              | As a platform administrator, approve and reject a pending signup                                | Both still work (`platform:manage` is unaffected)                                                                       |
| M17 | Session                | After signing out, navigate directly to `/users` and `/dashboard`                               | Redirected to `/login`                                                                                                  |

M13 and M14 cannot be driven from the client for both halves of the rule — the UI hides self-actions — so they are expected to be exercised with the action menu on a second administrator's row, or via the API, and the _presentation_ verified in the UI. A cross-tenant refusal has no UI path at all: it is covered by the server acceptance suite.

## 9. E2E regression coverage

Playwright is the regression net for the client journeys that the server contract changes sit under. The suite is run when this feature lands and re-run while PRD-022 and PRD-027 are implemented, because both extend the surfaces this feature touches (PRD-022 adds a Delete action to the Users list and a Danger Zone to the settings sheet; PRD-027 adds a platform page).

Existing coverage that must stay green:

| Spec                                                                            | Guards                                                                                                                        |
| ------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| `auth/loginFlow.test.ts`, `auth/logout.test.ts`, `auth/protectedRoutes.test.ts` | Sign-in, sign-out and protected-route redirects — the journeys a fallback authorization policy can break                      |
| `dialogs/modalDialogs.test.ts`                                                  | The signup and password-reset dialogs (anonymous routes), the settings sheet, and the Change Role dialog open/close           |
| `settings/userSettings.test.ts`                                                 | Self-service profile update (`PUT /api/users/{id}`) and password change                                                       |
| `settings/siteDetails.test.ts` **(new)**                                        | Site Details save (`PUT /api/sites/{id}`)                                                                                     |
| `users/userManagement.test.ts` **(new)**                                        | Resend Invitation and enable/disable from the Users list (`POST /api/users/{id}/resend-invite`, `PUT /api/users/{id}/status`) |
| `approvals/approvals.test.ts`                                                   | The `platform:manage` approvals surface                                                                                       |
| `auth/permissionGating.test.ts`                                                 | Permission-gated affordances for a Viewer                                                                                     |

The two new specs are desktop-only (the mobile projects use a card grid with a different action surface, covered by `mobile/mobileCardGrids.test.ts`) and are `serial` because they mutate state. Neither can assert a cross-tenant refusal: the E2E environment seeds one site, so every id the client can send belongs to the caller. That case lives in the server acceptance suite, and this split is deliberate — HTTP-level contract tests for the server, user-journey tests for the client.

**Email delivery is not asserted.** The invitation and approval endpoints reachable from these journeys only _queue_ an email: submission writes to an unbounded channel and returns, and the dispatch loop catches send failures, so the endpoint succeeds with no SMTP server present. The E2E harness launches the API with no SMTP settings (the config section binds an empty host and port 0), and the existing invite and signup specs already rely on this — the requests succeed and the send is logged and discarded. The specs therefore assert the HTTP contract and the client outcome; actual delivery belongs to manual verification (M3, M9).

Run commands (from `Source/Client/pot-react`): `npm run e2e:preflight` before a run, `npx playwright test <path> --project=chromium` for a targeted loop, and `npm run e2e:all:dev` for the full four-project matrix that gates the feature.

## 10. Open questions

1. **Should `resend-invite` gain a concurrency token, and should it accept only `Pending` users?** Today it takes no body, requires no etag, and accepts any user. Site binding fixes the cross-tenant defect; whether a same-site administrator should be able to force-reset an enabled user's credentials is a separate behaviour decision. Recommendation: restrict to `Pending`, and do not add an etag (the operation is idempotent in intent).
2. **Is a configured platform administrator's site role frozen, or only their status?** Recommendation: status only. `platform:manage` comes from configuration and is unaffected by roles, and the last-enabled-`Admin` rule already protects a tenant's administrability.
3. **Which population counts toward "last enabled `Admin`"?** Recommendation: enabled `Admin`-role holders only — a disabled or pending `Admin` cannot administer the site or re-enable anyone. This must match PRD-022 and PRD-027.

## 11. Scope

**In scope:** the repository surface for user and site access; the five routes in §5; the protected-identity checks; the fallback authorization policy and its regression test; the cross-tenant inventory, naming and `DEVELOPER.md` correction; the acceptance suite.

**Out of scope:** the delete routes and deletion sweeps (PRD-022); the cross-tenant platform routes (PRD-027); authentication, session and refresh-token architecture; the intentional Approvals surface; the reminder worker's platform-wide enumeration; client changes (`SiteDetailsForm` already sends the current site's id, and no client flow targets another site); rate limiting; global query filters on `User` and `Site` (§4.7); an audit store.

## 12. Acceptance criteria

1. All 13 acceptance cases pass, and the suite is unmodified apart from additions.
2. `IUserRepository` and `ISiteRepository` expose no `IQueryable` for `UserEntity` or `SiteEntity`.
3. The endpoint-metadata regression test fails if a route is added without an authorization requirement.
4. The five routes return the statuses in §5, and no refusal path writes a row.
5. The protected-identity refusals return `422` and leave the target unchanged.
6. The cross-tenant inventory in §4.5 is complete and each entry is named as cross-tenant at the call site.
7. `dotnet test pot.sln` is green, and `DEVELOPER.md` matches the implemented isolation model.

## 13. Related documents

- Prerequisite for: [User and Site Deletion PRD](PRD-022-user-and-site-deletion.md) — blocked pending this document.
- Prerequisite for: [Platform Administration PRD](PRD-027-platform-administration.md) — blocked pending this document.
- Authentication reference (platform administrators and permissions): `Docs/AUTHENTICATION.md`
- Server conventions (multi-tenancy, query filters): `Source/Server/DEVELOPER.md`
- Future index: [Docs/Future/README.md](README.md)
