using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Me.Mappings;
using Pot.App.Features.Me.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Me;

internal sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<Output?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { userId });

        var user = await _userRepository.Users
            .Include(user => user.Site)
            .SingleOrDefaultAsync(user => user.RowId == userId, cancellationToken)
            .ConfigureAwait(false);

        return user?.MapToOutput();
    }
}