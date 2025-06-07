using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Update.EntityChecks.Checks;

internal sealed class CheckAccountNumberDoesNotExist : PreUpdateCheckBase
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

        var input = state.Input;
        var account = state.AccountToUpdate;

        var differentAccountNumber = !(account.Bsb == input.Bsb && account.Number == input.Number);

        if (differentAccountNumber)
        {
            var accountExists = await _accountRepository
                .AccountExistsAsync(input.Bsb, input.Number, cancellationToken)
                .ConfigureAwait(false);

            if (accountExists)
            {
                return ProblemDetailsErrorFactory.CreateEntityExistsError(
                    "Account",
                    $"{nameof(AccountEntity.Bsb)}, {nameof(AccountEntity.Number)}",
                    $"{input.Bsb}, {input.Number}");
            }
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
