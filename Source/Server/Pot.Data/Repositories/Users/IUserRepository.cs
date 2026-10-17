using Pot.Data.Entities;
using Pot.Data.Repositories.Users.Dtos;

namespace Pot.Data.Repositories.Users;

/// <summary>
/// Reads and writes users.
/// </summary>
/// <remarks>
/// <para>
/// Do not add an <see cref="IQueryable{T}" /> member of <see cref="UserEntity" /> (or of any other entity) to
/// this interface, and do not reach for <c>Set&lt;UserEntity&gt;()</c> from a service. Tenant isolation depends
/// on every user lookup being one of the intent-named methods below: a caller of this repository cannot write a
/// lookup that ignores the site, and that is the point of the surface.
/// </para>
/// <para>
/// The site-scoped methods return <see langword="null" /> for a row belonging to another site, exactly as they
/// do for a row that does not exist, so a refusal cannot be used to discover that a row exists elsewhere.
/// </para>
/// <para>
/// Reads that genuinely cross sites are few, are named for it, and are recorded in the design's cross-tenant
/// inventory. Adding another needs a stated reason and a comment at the call site.
/// </para>
/// </remarks>
public interface IUserRepository : IRepositoryBase
{
    /// <summary>
    /// Resolves the signed-in caller from the current user context.
    /// </summary>
    /// <param name="includeSite">Whether to load the caller's site.</param>
    /// <returns>The signed-in caller.</returns>
    UserEntity GetCurrentUser(bool includeSite);

    /// <summary>
    /// Resolves a user only when the row belongs to the caller's site.
    /// </summary>
    /// <param name="rowId">The user to resolve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user, or <see langword="null" /> when the row is in another site or does not exist.</returns>
    Task<UserEntity?> GetForCurrentSiteAsync(Guid rowId, CancellationToken cancellationToken);

    /// <summary>
    /// As <see cref="GetForCurrentSiteAsync" />, with the user's roles loaded.
    /// </summary>
    /// <param name="rowId">The user to resolve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user with roles, or <see langword="null" /> for a foreign or missing row.</returns>
    Task<UserEntity?> GetForCurrentSiteWithRolesAsync(Guid rowId, CancellationToken cancellationToken);

    /// <summary>
    /// As <see cref="GetForCurrentSiteAsync" />, with the user's site loaded, for the caller's own profile.
    /// </summary>
    /// <param name="rowId">The user to resolve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user with site, or <see langword="null" /> for a foreign or missing row.</returns>
    Task<UserEntity?> GetProfileForCurrentSiteAsync(Guid rowId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists every user in the caller's site.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The users in the caller's site.</returns>
    Task<List<GetAllUserInfo>> GetAllForCurrentSiteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the enabled <c>Admin</c>-role holders in the caller's site.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The enabled administrators of the caller's site.</returns>
    Task<List<GetAllUserInfo>> GetEnabledAdminsForCurrentSiteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Resolves a user by username across every site.
    /// </summary>
    /// <remarks>
    /// Cross-tenant by design: usernames are unique across all sites, and a username is resolved before a site
    /// is known (sign-in), while the caller is signed out (password reset, signup), or for a user in another
    /// site (invitation, approval notification).
    /// </remarks>
    /// <param name="username">The username to resolve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user, or <see langword="null" /> when no user has that username.</returns>
    Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the token-validation state of a user across every site.
    /// </summary>
    /// <remarks>
    /// Cross-tenant by design: an access token is validated before any user context exists, so the token's
    /// subject is the only thing known.
    /// </remarks>
    /// <param name="userRowId">The user the token claims to belong to.</param>
    /// <param name="sessionRowId">The session the token names, read so a revoked session invalidates the token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The token-validation state, or <see langword="null" /> when the user no longer exists.</returns>
    Task<UserSecurityState?> GetSecurityStateAsync(Guid userRowId, Guid sessionRowId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists every site's enabled users.
    /// </summary>
    /// <remarks>
    /// Cross-tenant by design: the notification workers enumerate administrators across all sites, then
    /// re-scope per user.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The enabled users of every site.</returns>
    Task<List<GetAllUserInfo>> GetEnabledUsersForAllSitesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the users awaiting approval across every site.
    /// </summary>
    /// <remarks>
    /// Cross-tenant by design: this is the platform-administrator approval surface, which acts on every site.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The users awaiting approval.</returns>
    Task<List<UserEntity>> GetPendingApprovalsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Resolves a user by id across every site, for the approval surface.
    /// </summary>
    /// <remarks>
    /// Cross-tenant by design: a platform administrator approves a signup in any site.
    /// </remarks>
    /// <param name="rowId">The user to resolve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user, or <see langword="null" /> when the id does not exist.</returns>
    Task<UserEntity?> GetByIdForApprovalAsync(Guid rowId, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the configured platform administrators a notification has to be addressed to.
    /// </summary>
    /// <remarks>
    /// Cross-tenant by design: signup completes anonymously, so the administrators of every site are the
    /// recipients.
    /// </remarks>
    /// <param name="rowIds">The nominated administrator ids.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The users that still exist.</returns>
    Task<List<UserEntity>> GetByIdsForPlatformNotificationAsync(IReadOnlyList<Guid> rowIds, CancellationToken cancellationToken);

    /// <summary>
    /// Counts the configured platform administrators that belong to an enabled user.
    /// </summary>
    /// <remarks>
    /// Cross-tenant by design: platform administration comes from deployment configuration rather than from a
    /// site role, so the population spans every site.
    /// </remarks>
    /// <param name="rowIds">The configured platform administrator identifiers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of configured identifiers that belong to an enabled user.</returns>
    Task<int> CountEnabledPlatformAdminsAsync(IReadOnlyList<Guid> rowIds, CancellationToken cancellationToken);
}
