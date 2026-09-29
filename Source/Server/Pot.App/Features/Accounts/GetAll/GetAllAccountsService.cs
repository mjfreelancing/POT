using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Features.Accounts.GetAll.Mappings;
using Pot.App.Features.Accounts.GetAll.Models;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Shared.Models;

namespace Pot.App.Features.Accounts.GetAll;

/// <summary>
/// Default implementation of <see cref="IGetAllAccountsService"/>.
/// </summary>
/// <remarks>
/// A single as-of date is captured for the whole call and reused for every account, so all returned positions
/// are measured against the same current local date.
/// </remarks>
internal sealed class GetAllAccountsService : IGetAllAccountsService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccrualCalculator _accrualCalculator;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public GetAllAccountsService(IAccountRepository accountRepository, IAccrualCalculator accrualCalculator,
        ITimeProvider timeProvider, ILogger<GetAllAccountsService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _accrualCalculator = accrualCalculator.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<Output[]> GetAllAccountsAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var accounts = await _accountRepository.GetAllAccountsWithLinkedCountsAsync(cancellationToken);
        var asOfDate = _timeProvider.GetLocalDateNow();

        return accounts.SelectToArray(account => account.MapToOutput(CalculateAccrual(account, asOfDate)));
    }

    private AccountAccrualView CalculateAccrual(AccountWithLinkedCounts account, DateOnly asOfDate)
    {
        var positions = account.AccrualExpenses.Select(ExpenseAccrualPosition.AtPersistedSchedule);

        return _accrualCalculator.CalculateAccountTotals(positions, asOfDate);
    }
}
