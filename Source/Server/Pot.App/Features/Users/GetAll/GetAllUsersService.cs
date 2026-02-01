using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Users.GetAll.Mappings;
using Pot.App.Features.Users.GetAll.Models;
using Pot.Data.Repositories.Users;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Users.GetAll;

internal sealed class GetAllUsersService : IGetAllUsersService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public GetAllUsersService(IUserRepository userRepository, ILogger<GetAllUsersService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<List<Output>> GetAllEnabledAdminsAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var users = await _userRepository
            .GetEnabledUsersAsync(cancellationToken)
            .ConfigureAwait(false);

        return users
            .Where(user => user.Roles.Contains(Role.Admin.Name))
            .SelectToList(user => user.MapToOutput());
    }

    public async Task<List<Output>> GetAllForCurrentSiteAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var userInfos = await _userRepository
            .GetAllForCurrentSiteAsync(cancellationToken)
            .ConfigureAwait(false);

        return userInfos.SelectToList(userInfo => userInfo.MapToOutput());
    }
}
