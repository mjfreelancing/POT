using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Features.Accounts.Create.Services.PreSave;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Create.Services;
internal sealed class CreateAccountService : ICreateAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ILogger _logger;

    public CreateAccountService(IAccountRepository accountRepository, IPreCreateChecker preCreateChecker, ILogger<CreateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull(); ;
        _logger = logger.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
    }

    public async Task<EnrichedResult<AccountEntity>> CreateAccountAsync(Request request, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var accountToCreate = new AccountEntity
        {
            Bsb = request.Bsb,
            Number = request.Number,
            Description = request.Description,
            Balance = request.Balance,
            Reserved = request.Reserved
        };

        var canSaveResult = await _preCreateChecker.CanSaveAsync(accountToCreate, cancellationToken);

        if (canSaveResult is not null)
        {
            return canSaveResult.FailResult;
        }

        await _accountRepository
            .AddAndSaveAsync(accountToCreate, cancellationToken)
            .ConfigureAwait(false);

        return EnrichedResult.Success(accountToCreate);
    }
}
