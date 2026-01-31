using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

internal sealed class CheckAccountNumberDoesNotExist : PreCreateCheckBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CheckAccountNumberDoesNotExist(IAccountRepository accountRepository, ILogger<CheckAccountNumberDoesNotExist> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = state.AccountToCreate;

        var accountExists = await _accountRepository
            .AccountExistsAsync(account.Bsb, account.Number, cancellationToken)
            .ConfigureAwait(false);

        if (accountExists)
        {
            return ProblemDetailsErrorFactory.CreateEntityExistsError(
                $"{nameof(AccountEntity.Bsb)}, {nameof(AccountEntity.Number)}",
                $"{account.Bsb}, {account.Number}",
                "The account number already exists");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

