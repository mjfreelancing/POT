using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects a delete when the account still has linked expenses.
/// </summary>
internal sealed class CheckHasNoExpenses : PreDeleteCheckBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CheckHasNoExpenses(IAccountRepository accountRepository, ILogger<CheckHasNoExpenses> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <summary>
    /// Fails when the account has linked expenses, otherwise delegates to the next check in the chain.
    /// </summary>
    /// <param name="state">The state carrying the identifier of the account being validated.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null"/> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var accountId = state.AccountId;

        var hasExpenses = await _accountRepository
            .HasExpensesAsync(accountId, cancellationToken)
            .ConfigureAwait(false);

        if (hasExpenses)
        {
            return ApiDetailErrorFactory.CreateEntityConstraintError(
                nameof(AccountEntity.RowId),
                accountId.ToString(),
                "Cannot delete an Account that has linked Expenses");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
