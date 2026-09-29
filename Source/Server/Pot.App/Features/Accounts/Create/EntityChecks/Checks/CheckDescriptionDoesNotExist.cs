using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Specifications;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

/// <summary>
/// Rejects a create when the account description already exists for the current site.
/// </summary>
internal sealed class CheckDescriptionDoesNotExist : PreCreateCheckBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CheckDescriptionDoesNotExist(IAccountRepository accountRepository, ILogger<CheckDescriptionDoesNotExist> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <summary>
    /// Fails when another account in the current site already uses the same description, otherwise delegates to
    /// the next check in the chain.
    /// </summary>
    /// <param name="state">The state carrying the account being validated.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null"/> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = state.AccountToCreate;

        var predicate = AccountSpecifications.IsSameDescription(account.Description).Expression;

        // Description uniqueness is per-site; the query filter restricts the check to the current site
        var descriptionExists = await _accountRepository.Accounts
            .AnyAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            return ApiDetailErrorFactory.CreateEntityExistsError(
                nameof(AccountEntity.Description),
                account.Description,
                "The account description already exists");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
