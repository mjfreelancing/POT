using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Users;

internal sealed class UserRepository : PersistableRepository, IPersistableUserRepository
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(PotDbContext dbContext, ICurrentUserContext currentUserContext, ILogger<UserRepository> logger)
        : base(dbContext)
    {
        _currentUserContext = currentUserContext.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public UserEntity GetCurrentUser(bool includeSite)
    {
        var query = Set<UserEntity>();

        if (includeSite)
        {
            query = query.Include(user => user.Site);
        }

        return query.Single(user => user.RowId == _currentUserContext.UserRowId);
    }

    public async Task<UserEntity?> GetForCurrentSiteAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return await FindForCurrentSiteAsync(rowId, includeRoles: false, includeSite: false, cancellationToken);
    }

    public async Task<UserEntity?> GetForCurrentSiteWithRolesAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return await FindForCurrentSiteAsync(rowId, includeRoles: true, includeSite: false, cancellationToken);
    }

    public async Task<UserEntity?> GetProfileForCurrentSiteAsync(Guid rowId, CancellationToken cancellationToken)
    {
        return await FindForCurrentSiteAsync(rowId, includeRoles: false, includeSite: true, cancellationToken);
    }

    /// <summary>
    /// Resolves a user only when the row belongs to the caller's site, and records a cross-tenant attempt.
    /// </summary>
    /// <remarks>
    /// A refusal is reported exactly as a missing row is, so the caller learns nothing. The refusal is recorded
    /// here, where the two cases can still be told apart, so that every route resolving a target is covered
    /// rather than each route having to remember to log it.
    /// </remarks>
    /// <param name="rowId">The user to resolve.</param>
    /// <param name="includeRoles">Whether to load the user's roles.</param>
    /// <param name="includeSite">Whether to load the user's site.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user, or <see langword="null" /> for a foreign or missing row.</returns>
    private async Task<UserEntity?> FindForCurrentSiteAsync(Guid rowId, bool includeRoles, bool includeSite, CancellationToken cancellationToken)
    {
        var currentSiteId = GetCurrentSiteId();

        if (currentSiteId is null)
        {
            return null;
        }

        var siteId = currentSiteId.Value;
        var query = Set<UserEntity>();

        if (includeRoles)
        {
            query = query.Include(user => user.Roles);
        }

        if (includeSite)
        {
            query = query.Include(user => user.Site);
        }

        var user = await query.SingleOrDefaultAsync(user => user.RowId == rowId && user.Site.Id == siteId, cancellationToken);

        if (user is not null)
        {
            return user;
        }

        await LogCrossTenantRefusalAsync(rowId, siteId, cancellationToken);

        return null;
    }

    /// <summary>
    /// Records a refused attempt to reach a row that exists in another site.
    /// </summary>
    /// <remarks>
    /// A row that does not exist at all is not recorded: only an id that exists elsewhere is evidence of a
    /// caller reaching outside its own site.
    /// </remarks>
    /// <param name="rowId">The user the caller asked for.</param>
    /// <param name="callerSiteId">The site the caller belongs to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task LogCrossTenantRefusalAsync(Guid rowId, int callerSiteId, CancellationToken cancellationToken)
    {
        var existsInAnotherSite = await Set<UserEntity>()
            .AnyAsync(user => user.RowId == rowId && user.Site.Id != callerSiteId, cancellationToken);

        if (!existsInAnotherSite)
        {
            return;
        }

        _logger.LogWarning(
            "Cross-tenant refusal: caller {CallerRowId} of site {CallerSiteId} requested user {RequestedRowId}",
            _currentUserContext.UserRowId,
            callerSiteId,
            rowId);
    }

    public async Task<List<GetAllUserInfo>> GetAllForCurrentSiteAsync(CancellationToken cancellationToken)
    {
        var currentSiteId = GetCurrentSiteId();

        if (currentSiteId is null)
        {
            return [];
        }

        var siteId = currentSiteId.Value;
        var userQuery = Set<UserEntity>().Where(user => user.Site.Id == siteId);

        return await GetUsersAsync(userQuery, cancellationToken);
    }

    public async Task<List<GetAllUserInfo>> GetEnabledAdminsForCurrentSiteAsync(CancellationToken cancellationToken)
    {
        var currentSiteId = GetCurrentSiteId();

        if (currentSiteId is null)
        {
            return [];
        }

        var siteId = currentSiteId.Value;

        var userQuery = Set<UserEntity>()
            .Where(user => user.Site.Id == siteId)
            .Where(user => user.Status == UserStatus.Enabled)
            .Where(user => user.Roles.Any(role => role.Name == Role.Admin));

        return await GetUsersAsync(userQuery, cancellationToken);
    }

    public Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        // Cross-tenant (not restricted to the current site) since this is used for authentication
        return Set<UserEntity>().SingleOrDefaultAsync(user => user.Username == username, cancellationToken);
    }

    public Task<UserSecurityState?> GetSecurityStateAsync(Guid userRowId, Guid sessionRowId, CancellationToken cancellationToken)
    {
        // Cross-tenant: the token is validated before any user context exists.
        return Set<UserEntity>()
            .Where(user => user.RowId == userRowId)
            .Select(user => new UserSecurityState
            {
                TokenVersion = user.TokenVersion,
                Status = user.Status,

                // Both reads stay inside the one projection so validation costs a single round trip. The
                // expiry is cast to a nullable so a missing session reports null instead of default(DateTime);
                // an active session always has an expiry.
                SessionRevokedUtc = user.AuthSessions
                    .Where(session => session.RowId == sessionRowId)
                    .Select(session => session.RevokedUtc)
                    .FirstOrDefault(),

                SessionExpiresUtc = user.AuthSessions
                    .Where(session => session.RowId == sessionRowId)
                    .Select(session => (DateTime?)session.ExpiresUtc)
                    .FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<List<GetAllUserInfo>> GetEnabledUsersForAllSitesAsync(CancellationToken cancellationToken)
    {
        // Cross-tenant: the notification workers enumerate every site's administrators.
        var userQuery = Set<UserEntity>().Where(user => user.Status == UserStatus.Enabled);
        return GetUsersAsync(userQuery, cancellationToken);
    }

    public Task<List<UserEntity>> GetPendingApprovalsAsync(CancellationToken cancellationToken)
    {
        // Cross-tenant: the approval surface acts on every site.
        return Set<UserEntity>()
            .IgnoreQueryFilters()
            .Where(user => user.Status == UserStatus.Approval)
            .OrderBy(user => user.Username)
            .ToListAsync(cancellationToken);
    }

    public Task<UserEntity?> GetByIdForApprovalAsync(Guid rowId, CancellationToken cancellationToken)
    {
        // Cross-tenant: a platform administrator approves a signup in any site.
        return Set<UserEntity>()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(user => user.RowId == rowId, cancellationToken);
    }

    public Task<List<UserEntity>> GetByIdsForPlatformNotificationAsync(
        IReadOnlyList<Guid> rowIds, CancellationToken cancellationToken)
    {
        // Cross-tenant: signup completes anonymously, so the recipients are the configured administrators.
        return Set<UserEntity>()
            .Where(user => rowIds.Contains(user.RowId))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// The caller's site id, or <see langword="null" /> when the context reports no user.
    /// </summary>
    /// <remarks>
    /// The migration worker reports an empty identifier, which is treated here as a refusal rather than an
    /// error. The web context instead throws when no user has been set, so these reads rely on every route that
    /// resolves a target being authorized: an unauthenticated request is refused by the pipeline before it
    /// reaches here. The site comes from the same cached value the query filters use, so a repository read and
    /// the filters cannot scope to different sites.
    /// </remarks>
    private int? GetCurrentSiteId()
    {
        if (_currentUserContext.UserRowId == Guid.Empty)
        {
            return null;
        }

        return _dbContext.GetCurrentUserSiteId();
    }

    private static async Task<List<GetAllUserInfo>> GetUsersAsync(IQueryable<UserEntity> userQuery, CancellationToken cancellationToken)
    {
        var users = await userQuery
            .Select(user => new
            {
                user.RowId,
                user.Etag,
                user.Username,
                user.DisplayName,
                user.Email,
                user.Status,
                Roles = user.Roles.Select(role => role.Name),   // 'Name' is the Role enum, hence using an anonymous type first (can't project to the string Name)
                user.LastLoggedInUtc
            })
            .ToListAsync(cancellationToken);

        return [.. users.Select(user => new GetAllUserInfo
            {
                RowId = user.RowId,
                Etag = user.Etag,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Status = user.Status.Name,
                Roles = [.. user.Roles.Select(role => role.Name)],
                LastLoggedInUtc = user.LastLoggedInUtc
            })];
    }
}
