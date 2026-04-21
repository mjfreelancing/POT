using AllOverIt.Assertion;
using Microsoft.Extensions.Logging;

namespace Pot.App.Features.AccountAccruals;

internal sealed class AccountAccrualsService : IAccountAccrualsService
{
    private readonly ILogger<AccountAccrualsService> _logger;

    public AccountAccrualsService(ILogger<AccountAccrualsService> logger)
    {
        _logger = logger.WhenNotNull();
    }
}