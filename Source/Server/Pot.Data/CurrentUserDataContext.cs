using AllOverIt.Assertion;
using AllOverIt.Async;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;

namespace Pot.Data;

internal sealed class CurrentUserDataContext : ICurrentUserDataContext
{
    private readonly IUserRepository _userRepository;

    private Guid? _userRowId;
    private readonly AsyncLazy<UserEntity> _lazyUser;

    public CurrentUserDataContext(IUserRepository userRepository)
    {
        _userRepository = userRepository.WhenNotNull();

        _lazyUser = new AsyncLazy<UserEntity>(LoadUserAsync);
    }

    public void SetUser(Guid userRowId)
    {
        _userRowId = userRowId;
    }

    public async Task<UserEntity> GetUserAsync()
    {
        // If the JWT does not contain a valid user RowId, then we wouldn't get this far.
        return await _lazyUser;
    }

    private Task<UserEntity> LoadUserAsync()
    {
        Throw<InvalidOperationException>.WhenNot(_userRowId.HasValue, "The user identifier has not been set");

        return _userRepository.Current
            .Include(user => user.Site)
            .SingleAsync(user => user.RowId == _userRowId.Value, CancellationToken.None);
    }
}
