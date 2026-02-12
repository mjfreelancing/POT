using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Specifications;

namespace Pot.App.Features.Accounts.Create.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreCreateCheckBase
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

        var account = state.AccountToCreate;

        var predicate = AccountSpecifications.IsSameDescription(account.Description).Expression;

        // Account descriptions are globally unique
        var descriptionExists = await _accountRepository.Accounts
            .IgnoreQueryFilters()
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
