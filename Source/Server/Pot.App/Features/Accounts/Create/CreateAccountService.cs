using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Features.Accounts.Create.EntityChecks;
using Pot.App.Features.Accounts.Create.Mappings;
using Pot.App.Features.Accounts.Create.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Sites;

namespace Pot.App.Features.Accounts.Create;

/// <summary>
/// Default implementation of <see cref="ICreateAccountService"/>.
/// </summary>
internal sealed class CreateAccountService : ICreateAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ILogger _logger;

    public CreateAccountService(IPersistableAccountRepository accountRepository, ISiteRepository siteRepository,
        IPreCreateChecker preCreateChecker, ILogger<CreateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _siteRepository = siteRepository.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<Output>> CreateAccountAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_accountRepository.WithTracking())
        {
            var userSite = _siteRepository.GetCurrentSite();

            // Money is stored to two decimals, so an amount arriving with more precision is rounded rather than
            // refused.
            var accountToCreate = new AccountEntity
            {
                Site = userSite,
                Description = input.Description,
                Balance = MonetaryRounding.Round(input.Balance),
                Reserved = MonetaryRounding.Round(input.Reserved)
            };

            // Provided when importing - required so income and expense items can be linked correctly
            if (input.RowId.HasValue)
            {
                accountToCreate.RowId = input.RowId.Value;
            }

            var apiError = await _preCreateChecker.CanSaveAsync(accountToCreate, cancellationToken);

            if (apiError is not null)
            {
                return EnrichedResult.Fail<Output>(apiError);
            }

            await _accountRepository.AddAndSaveAsync(accountToCreate, cancellationToken);

            var createdAccount = accountToCreate.MapToOutput();

            return EnrichedResult.Success(createdAccount);
        }
    }
}
