using AllOverIt.Assertion;
using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.AspNetCore.Features.Accounts.Create.Services.PreSave.Checks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Create.Services.PreSave;

internal sealed class PreCreateChecker : ChainOfResponsibilityAsyncComposer<InputState, OutputState>, IPreCreateChecker
{
    private static readonly IEnumerable<PreCreateCheckBase> _handlers =
    [
        new CheckAccountNumberDoesNotExist(),
        new CheckDescriptionDoesNotExist()
    ];

    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public PreCreateChecker(IAccountRepository accountRepository, ILogger<PreCreateChecker> logger)
        : base(_handlers)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull(); ;
    }

    public Task<OutputState?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken)
    {
        var state = new InputState
        {
            AccountToCreate = accountToCreate,
            AccountRepository = _accountRepository,
            Logger = _logger
        };

        return HandleAsync(state, cancellationToken);
    }
}

