using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Auth.Me.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Auth.Me;

internal sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { userId });

        return _userRepository.Current
            .Where(user => user.RowId == userId)
            .Select(userId => new UserInfo(userId.RowId, userId.Username, userId.DisplayName, userId.Email))
            .SingleOrDefaultAsync(cancellationToken);
    }
}