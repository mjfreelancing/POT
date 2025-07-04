using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Accounts.Get.Mappings;
using Pot.App.Features.Accounts.Get.Models;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Get;

internal sealed class GetAccountService : IGetAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public GetAccountService(IPersistableAccountRepository accountRepository, ILogger<GetAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> GetAccountWithLinkedCountsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = await _accountRepository.GetAccountWithLinkedCountsAsync(accountId, cancellationToken);

        if (account is null)
        {
            var accountNotFoundDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(accountId, "The account does not exist.");

            _logger.LogError(accountNotFoundDetails);

            return EnrichedResult.Fail<Output>(accountNotFoundDetails);
        }

        var output = account.MapToOutput();

        return EnrichedResult.Success(output);
    }
}
