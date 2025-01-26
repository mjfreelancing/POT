using AllOverIt.Assertion;
using AllOverIt.Patterns.ChainOfResponsibility;
using Pot.AspNetCore.Features.Accounts.Update.Services.PreCommit.Checks;
using Pot.AspNetCore.Features.Accounts.Update.Services.PreSave.Checks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.PreSave;

internal sealed class PreUpdateChecker : ChainOfResponsibilityAsyncComposer<InputState, OutputState>, IPreUpdateChecker
{
    private static readonly IEnumerable<PreUpdateCheckBase> Handlers =
    [
        new CheckHasSameETag(),
        new CheckAccountNumberDoesNotExist(),
        new CheckDescriptionDoesNotExist()
    ];

    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<PreUpdateChecker> _logger;

    public PreUpdateChecker(IAccountRepository accountRepository, ILogger<PreUpdateChecker> logger)
        : base(Handlers)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull(); ;
    }

    public Task<OutputState?> CanSaveAsync(Request request, AccountEntity accountToUpdate, CancellationToken cancellationToken)
    {
        var state = new InputState
        {
            Request = request,
            AccountToUpdate = accountToUpdate,
            AccountRepository = _accountRepository,
            Logger = _logger
        };

        return HandleAsync(state, cancellationToken);
    }
}

