using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared;
using Pot.Shared.Enumerations;

namespace Pot.Data.Repositories.Users;

internal sealed class UserRepository : PersistableRepository, IPersistableUserRepository
{
    private readonly ICurrentUserContext _currentUserContext;

    public IQueryable<UserEntity> Users => Set<UserEntity>();

    public UserRepository(PotDbContext dbContext, ICurrentUserContext currentUserContext)
        : base(dbContext)
    {
        _currentUserContext = currentUserContext.WhenNotNull();
    }

    public UserEntity GetCurrentUser(bool includeSite)
    {
        var query = Users;

        if (includeSite)
        {
            query = query.Include(user => user.Site);
        }

        return query.Single(user => user.RowId == _currentUserContext.UserRowId);
    }

    public Task<List<GetAllUserInfo>> GetEnabledUsersAsync(CancellationToken cancellationToken)
    {
        var userQuery = Users.Where(user => user.Status == UserStatus.Enabled);
        return GetUsersAsync(userQuery, cancellationToken);
    }

    public Task<List<GetAllUserInfo>> GetAllForCurrentSiteAsync(CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser(true);

        var userQuery = Users.Where(user => user.Site.Id == currentUser.Site.Id);

        return GetUsersAsync(userQuery, cancellationToken);
    }

    public Task<UserEntity?> GetByUsernameOrDefaultAsync(string username, CancellationToken cancellationToken)
    {
        return Users.SingleOrDefaultAsync(user => user.Username == username, cancellationToken);
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
