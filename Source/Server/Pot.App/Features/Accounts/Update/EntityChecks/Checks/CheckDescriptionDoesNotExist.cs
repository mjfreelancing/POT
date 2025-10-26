using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Specification.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Specifications;

namespace Pot.App.Features.Accounts.Update.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreUpdateCheckBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CheckDescriptionDoesNotExist(IAccountRepository accountRepository, ILogger<CheckDescriptionDoesNotExist> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var input = state.Input;
        var account = state.AccountToUpdate;

        if (account.Description != input.Description)
        {
            var notSameAccount = AccountSpecifications.IsSameBsbNumber(input.Bsb, input.Number).Not();
            var sameDescription = AccountSpecifications.IsSameDescription(input.Description);
            var predicate = notSameAccount.And(sameDescription).Expression;

            // Account descriptions are globally unique
            var descriptionExists = await _accountRepository.Accounts
                .IgnoreQueryFilters()
                .AnyAsync(predicate, cancellationToken)
                .ConfigureAwait(false);

            if (descriptionExists)
            {
                return ProblemDetailsErrorFactory.CreateEntityExistsError(
                    "Account",
                     nameof(AccountEntity.Description),
                    input.Description);
            }
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
