using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Accounts.Get.Mappings;
using Pot.App.Features.Accounts.Get.Models;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Shared.Models;

namespace Pot.App.Features.Accounts.Get;

/// <summary>
/// Default implementation of <see cref="IGetAccountService"/>.
/// </summary>
/// <remarks>
/// Accrual totals are calculated against the current local date, so the returned position advances with the
/// clock without requiring a write.
/// </remarks>
internal sealed class GetAccountService : IGetAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IAccrualCalculator _accrualCalculator;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public GetAccountService(IPersistableAccountRepository accountRepository, IAccrualCalculator accrualCalculator,
        ITimeProvider timeProvider, ILogger<GetAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _accrualCalculator = accrualCalculator.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<Output>> GetAccountWithLinkedCountsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = await _accountRepository.GetAccountWithLinkedCountsOrDefaultAsync(accountId, cancellationToken);

        if (account is null)
        {
            var accountNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(accountId, "The account does not exist");

            _logger.LogApiError(accountNotFoundError);

            return EnrichedResult.Fail<Output>(accountNotFoundError);
        }

        var accrualView = CalculateAccrual(account);
        var output = account.MapToOutput(accrualView);

        return EnrichedResult.Success(output);
    }

    private AccountAccrualView CalculateAccrual(AccountWithLinkedCounts account)
    {
        var positions = account.AccrualExpenses.Select(ExpenseAccrualPosition.AtPersistedSchedule);

        return _accrualCalculator.CalculateAccountTotals(positions, _timeProvider.GetLocalDateNow());
    }
}
