using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
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

    public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = state.AccountToCreate;

        var predicate = AccountSpecifications.IsSameDescription(account.Description).Expression;

        var descriptionExists = await _accountRepository
            .AnyAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            return ProblemDetailsErrorFactory.CreateEntityExistsError(
                "Account",
                nameof(AccountEntity.Description),
                account.Description);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
