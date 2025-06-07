using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Accounts.Get.Mappings;
using Pot.App.Features.Accounts.Get.Models;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Get;

internal sealed class GetAccountService : IGetAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public GetAccountService(IPersistableAccountRepository accountRepository, ILogger<GetAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<Output?> GetAccountAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = await _accountRepository.GetAccountOrDefaultAsync(id, cancellationToken);

        return account?.MapToOutput();
    }
}
