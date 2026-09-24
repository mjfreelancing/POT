using AllOverIt.Assertion;
using AllOverIt.Expressions;
using AllOverIt.Logging.Extensions;
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

    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var input = state.Input;
        var account = state.AccountToUpdate;

        if (account.Description != input.Description)
        {
            var predicate = AccountSpecifications
                .IsSameDescription(input.Description).Expression
                .And(item => item.Id != account.Id);

            // Description uniqueness is per-site; the query filter restricts the check to the current site
            var descriptionExists = await _accountRepository.Accounts
                .AnyAsync(predicate, cancellationToken)
                .ConfigureAwait(false);

            if (descriptionExists)
            {
                return ApiDetailErrorFactory.CreateEntityExistsError(
                    nameof(AccountEntity.Description),
                    input.Description,
                    "The account description already exists");
            }
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
