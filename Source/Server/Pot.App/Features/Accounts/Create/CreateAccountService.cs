using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Accounts.Create.EntityChecks;
using Pot.App.Features.Accounts.Create.Mappings;
using Pot.App.Features.Accounts.Create.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Create;

internal sealed class CreateAccountService : ICreateAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ICurrentUserDataContext _currentUserDataContext;
    private readonly ILogger _logger;

    public CreateAccountService(IPersistableAccountRepository accountRepository, IPreCreateChecker preCreateChecker,
        ICurrentUserDataContext currentUserContext, ILogger<CreateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
        _currentUserDataContext = currentUserContext.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_accountRepository.WithTracking())
        {
            var user = await _currentUserDataContext.GetUserAsync();

            var accountToCreate = new AccountEntity
            {
                Site = user.Site,
                Bsb = input.Bsb,
                Number = input.Number,
                Description = input.Description,
                Balance = input.Balance,
                Reserved = input.Reserved
            };

            var problemDetails = await _preCreateChecker.CanSaveAsync(accountToCreate, cancellationToken);

            if (problemDetails is not null)
            {
                return EnrichedResult.Fail<Output>(problemDetails);
            }

            await _accountRepository
                .AddAndSaveAsync(accountToCreate, cancellationToken)
                .ConfigureAwait(false);

            var createdAccount = accountToCreate.MapToOutput();

            return EnrichedResult.Success(createdAccount);
        }
    }
}
