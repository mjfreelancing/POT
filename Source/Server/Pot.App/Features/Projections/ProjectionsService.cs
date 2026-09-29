using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Features.Projections.Models;
using Pot.Data.Repositories.Projections;
using Pot.Shared.Extensions;
using Pot.Shared.Models;

namespace Pot.App.Features.Projections;

/// <summary>
/// Default implementation of <see cref="IProjectionsService"/>.
/// </summary>
internal sealed class ProjectionsService : IProjectionsService
{
    private readonly IProjectionsRepository _projectionsRepository;
    private readonly IExpenseRenewalFold _expenseRenewalFold;
    private readonly IIncomeRenewalFold _incomeRenewalFold;
    private readonly IAccrualCalculator _accrualCalculator;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public ProjectionsService(IProjectionsRepository accountRepository, IExpenseRenewalFold expenseRenewalFold,
        IIncomeRenewalFold incomeRenewalFold, IAccrualCalculator accrualCalculator, ITimeProvider timeProvider,
        ILogger<ProjectionsService> logger)
    {
        _projectionsRepository = accountRepository.WhenNotNull();
        _expenseRenewalFold = expenseRenewalFold.WhenNotNull();
        _incomeRenewalFold = incomeRenewalFold.WhenNotNull();
        _accrualCalculator = accrualCalculator.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    // TODO: Move the logic in this service to a projection calculator (keep repository access here)
    public async Task<EnrichedResult<Output>> GetFinancialProjectionsAsync(ProjectionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // The start date may be into the future so we need to aggregate data from today until the start date
        var (localDate, preStartDays) = GetPreStartDays(options.StartDate);

        // Will only contain incomes/expenses that are not excluded from calculations
        var accounts = await _projectionsRepository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(cancellationToken);

        // One cursor per row, seeded from the persisted schedule and folded as the window walks. The loop always
        // starts on site-local today, so the pre-start days are simulated only to carry the balance forward.
        var accountStates = accounts.ToDictionary(account => account, account => new AccountProjectionState(account));

        var accountDaily = accounts.ToDictionary(account => account.RowId, account => new List<DateProjectionValues>(options.DaysForecast));

        var globalDailyProjections = new List<DateProjectionValues>(options.DaysForecast);

        var totalDays = options.DaysForecast + preStartDays;

        for (int day = 0; day < totalDays; day++)
        {
            var date = localDate.AddDays(day);
            var isToday = day == 0;

            var globalStarting = 0.0d;
            var globalIncome = 0.0d;
            var globalExpenses = 0.0d;
            var globalAccrualSettledByPayments = 0.0d;
            var globalDailyAccrual = 0.0d;
            var globalAccrued = 0.0d;
            var globalArrears = 0.0d;
            var globalReserved = 0.0d;
            var globalExpenseItems = new List<ProjectionExpenseModel>();
            var globalIncomeItems = new List<ProjectionIncomeModel>();

            foreach (var account in accounts)
            {
                var dateValues = ResolveDay(accountStates[account], date, isToday);

                if (date >= options.StartDate)
                {
                    var dailyList = accountDaily[account.RowId];
                    dailyList.Add(dateValues);
                }

                globalStarting += dateValues.StartingBalance;
                globalIncome += dateValues.IncomeReceived;
                globalExpenses += dateValues.ExpensesPaid;
                globalAccrualSettledByPayments += dateValues.AccrualSettledByPayments;
                globalDailyAccrual += dateValues.DailyAccrual;
                globalAccrued += dateValues.Accrued;
                globalArrears += dateValues.Arrears;
                globalReserved += dateValues.Reserved;
                globalExpenseItems.AddRange(dateValues.ExpenseItems);
                globalIncomeItems.AddRange(dateValues.IncomeItems);
            }

            if (date >= options.StartDate)
            {
                globalDailyProjections.Add(new DateProjectionValues
                {
                    Date = date,
                    StartingBalance = globalStarting,
                    IncomeReceived = globalIncome,
                    ExpensesPaid = globalExpenses,
                    AccrualSettledByPayments = globalAccrualSettledByPayments,
                    DailyAccrual = globalDailyAccrual,
                    Accrued = globalAccrued,
                    Arrears = globalArrears,
                    Reserved = globalReserved,
                    ExpenseItems = [.. globalExpenseItems],
                    IncomeItems = [.. globalIncomeItems]
                });
            }
        }

        // Financial projections for each account
        var accountDailyFinancialProjections = accounts
            .SelectToList(account =>
            {
                var projectionValues = accountDaily[account.RowId];

                return new AccountDailyFinancialProjection
                {
                    RowId = account.RowId,
                    Description = account.Description,
                    Dates = MapToDateBalanceAvailable(projectionValues)
                };
            });

        var output = new Output
        {
            Accounts = [.. accountDailyFinancialProjections],
            Global = MapToDateBalanceAvailable(globalDailyProjections)
        };

        return EnrichedResult.Success(output);
    }

    /// <summary>
    /// Resolves one account's day: the due check, the paid set and the accrual are all measured against the
    /// cursor as it stands at the start of the day, and only then is that day's renewal folded in.
    /// </summary>
    private DateProjectionValues ResolveDay(AccountProjectionState state, DateOnly date, bool isToday)
    {
        var account = state.Account;

        var expensesDue = state.ExpenseRows
            .Where(row => row.Facts.Amount > 0.0d && IsDueOnDate(row.Facts, row.Cursor, date))
            .ToArray();

        var incomesDue = state.IncomeRows
            .Where(row => row.Income.Amount > 0.0d && IsDueOnDate(row.Schedule, date))
            .ToArray();

        // A day with no payment reads only the aggregates. A payment day needs the per-row accruals, because the
        // add-back settles only what the paid rows themselves accrued, so it uses the detail shape and takes the
        // aggregates from the same call.
        var accrual = expensesDue.Length == 0
            ? _accrualCalculator.CalculateAccountTotals(state.CreatePositions(), date)
            : _accrualCalculator.CalculateAccountWithExpenseDetail(state.CreatePositions(), date);

        if (isToday)
        {
            state.HoldArrears(accrual.TotalArrears);
        }

        var expensesPaid = expensesDue.Sum(row => row.Facts.Amount);
        var incomeReceived = incomesDue.Sum(row => row.Income.Amount);

        var dateValues = new DateProjectionValues
        {
            Date = date,
            StartingBalance = state.RunningBalance,
            IncomeReceived = incomeReceived,
            ExpensesPaid = expensesPaid,
            AccrualSettledByPayments = CalculateAccrualSettledByPayments(accrual, expensesDue),
            DailyAccrual = accrual.DailyExpenseAccrual,
            Accrued = accrual.TotalExpenseAccrued,
            Arrears = state.Arrears,
            Reserved = account.Reserved,
            ExpenseItems = expensesDue.SelectToArray(row => new ProjectionExpenseModel
            {
                RowId = row.Facts.RowId,
                Description = row.Expense.Description,
                Amount = row.Facts.Amount
            }),
            IncomeItems = incomesDue.SelectToArray(row => new ProjectionIncomeModel
            {
                RowId = row.Income.RowId,
                Description = row.Income.Description,
                Amount = row.Income.Amount
            })
        };

        // The window assumes income is received and expenses are paid on time, so the balance is loop-local and
        // the account row is never written.
        state.RecordCashMovement(incomeReceived, expensesPaid);

        // Folding after the measurement is what lets a payment day mid-window behave exactly as today does.
        state.FoldRenewals(_expenseRenewalFold, _incomeRenewalFold, date);

        return dateValues;
    }

    /// <summary>
    /// Returns the part of the day's accrual that the day's payments settle.
    /// </summary>
    /// <remarks>
    /// <see cref="AccountAccrualView.TotalExpenseAccrued" /> is measured before the day's payments are applied, so
    /// the occurrence falling due today is still counted in full. This sums the accrual of each expense row due on
    /// the day, matched by <see cref="ExpenseAccrualDetail.RowId" />, so the response mapping re-adds exactly the
    /// accrual those payments discharge and no more. Reading it from the detail shape means the amount never
    /// re-derives the accrual gate, so a row that accrues nothing settles nothing.
    /// </remarks>
    private static double CalculateAccrualSettledByPayments(AccountAccrualView accrual, AccountProjectionState.ExpenseProjectionRow[] expensesDue)
    {
        if (expensesDue.Length == 0)
        {
            return 0.0d;
        }

        var paidRowIds = expensesDue.Select(row => row.Facts.RowId).ToHashSet();

        return accrual.Expenses
            .Where(detail => paidRowIds.Contains(detail.RowId))
            .Sum(detail => detail.Accrued);
    }

    private (DateOnly localDate, int preStartDays) GetPreStartDays(DateOnly startDate)
    {
        var localDate = _timeProvider.GetLocalDateNow();

        Throw<InvalidOperationException>.When(localDate > startDate, "Projections cannot start earlier than today");

        return (localDate, localDate.DaysUntil(startDate));
    }

    private static bool IsDueOnDate(ExpenseAccrualInput facts, AccrualCursor cursor, DateOnly date)
    {
        return cursor.NextDue == date && (facts.EndDate.GetValueOrDefault(DateOnly.MaxValue) >= date);
    }

    private static bool IsDueOnDate(IncomeScheduleInput schedule, DateOnly date)
    {
        return schedule.NextDue == date && (schedule.EndDate.GetValueOrDefault(DateOnly.MaxValue) >= date);
    }

    private static DateProjection[] MapToDateBalanceAvailable(IEnumerable<DateProjectionValues> projectionValues)
    {
        return projectionValues.SelectToArray(item =>
        {
            var balance = item.StartingBalance + item.IncomeReceived - item.ExpensesPaid;

            return new DateProjection
            {
                Date = item.Date,
                Balance = balance,

                // Expenses due TODAY are fully accrued. They are not actually considered paid until they are renewed.
                // For the purposes of projections, we consider them paid today, so only the part of that payment the
                // accrual already counted is added back to get the true available balance: the accrued amount already
                // considers the expense total, and a row that accrues nothing (AccrualPolicy.None, or no accrual start)
                // is not credited. Arrears is a standing obligation for past-due cycles, so it is subtracted and never
                // released.
                Available = balance - item.Reserved - item.Accrued - item.Arrears + item.AccrualSettledByPayments,

                DailyAccrual = item.DailyAccrual,
                IncomeReceived = item.IncomeReceived,
                ExpensesPaid = item.ExpensesPaid,
                ExpenseItems = item.ExpenseItems,
                IncomeItems = item.IncomeItems
            };
        });
    }
}
