using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Accounts.Create;
using Pot.App.Features.Accounts.GetAll.Mappings;
using Pot.App.Features.Accounts.GetAll.Models;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.GetAll;

internal sealed class GetAllAccountsService : IGetAllAccountsService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public GetAllAccountsService(IPersistableAccountRepository accountRepository, ILogger<CreateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<List<Output>> GetAllAccountsAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var accounts = await _accountRepository.GetAllAsync(cancellationToken);

        return accounts.SelectToList(account => account.MapToOutput());
    }
}
