using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Approvals.Pending.Mappings;
using Pot.App.Features.Approvals.Pending.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Approvals.Pending;

/// <summary>
/// Default implementation of <see cref="IGetPendingApprovalsService"/>.
/// </summary>
internal sealed class GetPendingApprovalsService : IGetPendingApprovalsService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public GetPendingApprovalsService(IUserRepository userRepository, ILogger<GetPendingApprovalsService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<List<Output>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // Cross-tenant by design: the platform-administrator approval surface acts on every site.
        var users = await _userRepository.GetPendingApprovalsAsync(cancellationToken);

        return users.SelectToList(user => user.MapToOutput());
    }
}
