using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Delete.EntityChecks.Checks;

internal sealed class CheckHasNoIncomes : PreDeleteCheckBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CheckHasNoIncomes(IAccountRepository accountRepository, ILogger<CheckHasNoIncomes> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var accountId = state.AccountId;

        var hasIncomes = await _accountRepository
            .HasIncomesAsync(accountId, cancellationToken)
            .ConfigureAwait(false);

        if (hasIncomes)
        {
            return ProblemDetailsErrorFactory.CreateEntityConstraintError(
                nameof(AccountEntity.RowId),
                accountId.ToString(),
                "Cannot delete an Account that has linked Incomes");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
