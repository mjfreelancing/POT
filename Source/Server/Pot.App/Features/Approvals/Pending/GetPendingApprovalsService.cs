using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Approvals.Pending.Mappings;
using Pot.App.Features.Approvals.Pending.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Approvals.Pending;

internal sealed class GetPendingApprovalsService : IGetPendingApprovalsService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public GetPendingApprovalsService(IUserRepository userRepository, ILogger<GetPendingApprovalsService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<List<Output>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var users = await _userRepository.Users
             .IgnoreQueryFilters()
             .Where(user => user.Status == Shared.Enumerations.UserStatus.Approval)
             .ToListAsync(cancellationToken);

        return users.SelectToList(user => user.MapToOutput());
    }
}
