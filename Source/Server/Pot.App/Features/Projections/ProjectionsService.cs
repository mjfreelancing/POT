using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Features.Projections.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Shared.Extensions;

namespace Pot.App.Features.Projections;

internal sealed class ProjectionsService : IProjectionsService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IExpenseRenewalCalculator _expenseRenewalCalculator;
    private readonly IIncomeRenewalCalculator _incomeRenewalCalculator;
    private readonly IAccrueExpenseCalculator _accrueExpenseCalculator;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public ProjectionsService(IAccountRepository accountRepository,
        IExpenseRenewalCalculator expenseRenewalCalculator, IIncomeRenewalCalculator incomeRenewalCalculator,
        IAccrueExpenseCalculator accrueExpenseCalculator, ITimeProvider timeProvider, ILogger<ProjectionsService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _expenseRenewalCalculator = expenseRenewalCalculator.WhenNotNull();
        _incomeRenewalCalculator = incomeRenewalCalculator.WhenNotNull();
        _accrueExpenseCalculator = accrueExpenseCalculator.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> GetProjectionsAsync(ProjectionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // The start date may be into the future so we need to aggregate data from today until the start date
        var (localDate, preStartDays) = GetPreStartDays(options.StartDate);

        var accounts = await _accountRepository.GetAllAccountsWithIncomesAndExpensesAsync(cancellationToken);

        var accountDaily = accounts.ToDictionary(account => account.RowId, account => new List<DateProjectionValues>());
        var globalDailyProjections = new List<DateProjectionValues>(options.DaysForecast);

        for (int day = 0; day < options.DaysForecast + preStartDays; day++)
        {
            var date = localDate.AddDays(day);

            var globalStarting = 0.0d;
            var globalIncome = 0.0d;
            var globalExpenses = 0.0d;
            var globalDailyAccrual = 0.0d;
            var globalAccrued = 0.0d;
            var globalReserved = 0.0d;

            foreach (var account in accounts)
            {
                // Apply debit/credit to accounts otherwise the projections will be based on the current balance
                _expenseRenewalCalculator.Renew(account.Expenses, date, day == 0);
                _incomeRenewalCalculator.Renew(account.Incomes, date, day == 0);

                _accrueExpenseCalculator.AccrueExpenses(account, account.Expenses, date);

                var incomeReceived = account.Incomes
                    .Where(income => IsDueOnDate(income, date))
                    .Sum(income => income.Amount);

                var expensesPaid = account.Expenses
                    .Where(expense => IsDueOnDate(expense, date))
                    .Sum(expense => expense.Amount);

                var dateValues = new DateProjectionValues
                {
                    Date = date,
                    StartingBalance = account.Balance,
                    IncomeReceived = incomeReceived,
                    ExpensesPaid = expensesPaid,
                    DailyAccrual = account.DailyExpenseAccrual,
                    Accrued = account.TotalExpenseAccrued,
                    Reserved = account.Reserved
                };

                account.Balance += dateValues.IncomeReceived - dateValues.ExpensesPaid;

                if (!accountDaily.TryGetValue(account.RowId, out var dailyList))
                {
                    dailyList = new List<DateProjectionValues>(options.DaysForecast);
                    accountDaily[account.RowId] = dailyList;
                }

                dailyList.Add(dateValues);

                globalStarting += dateValues.StartingBalance;
                globalIncome += dateValues.IncomeReceived;
                globalExpenses += dateValues.ExpensesPaid;
                globalDailyAccrual += dateValues.DailyAccrual;
                globalAccrued += dateValues.Accrued;
                globalReserved += dateValues.Reserved;
            }

            if (date >= options.StartDate)
            {
                globalDailyProjections.Add(new DateProjectionValues
                {
                    Date = date,
                    StartingBalance = globalStarting,
                    IncomeReceived = globalIncome,
                    ExpensesPaid = globalExpenses,
                    DailyAccrual = globalDailyAccrual,
                    Accrued = globalAccrued,
                    Reserved = globalReserved
                });
            }
        }

        var accountDailyProjections = accounts
            .SelectToList(account =>
            {
                var projectionValues = accountDaily[account.RowId].Where(item => item.Date >= options.StartDate);

                return new AccountDailyProjection
                {
                    RowId = account.RowId,
                    Description = account.Description,
                    Dates = MapToDateBalanceAvailable(projectionValues)
                };
            });

        var output = new Output
        {
            Accounts = accountDailyProjections,
            Global = MapToDateBalanceAvailable(globalDailyProjections)
        };

        return EnrichedResult.Success(output);
    }

    private (DateOnly localDate, int preStartDays) GetPreStartDays(DateOnly startDate)
    {
        var localDateTimeOffset = _timeProvider.GetLocalNow();
        var localDate = DateOnly.FromDateTime(localDateTimeOffset.Date);

        Throw<InvalidOperationException>.When(localDate > startDate, "Projections cannot start earlier than today");

        return (localDate, localDate.DaysUntil(startDate));
    }

    private static bool IsDueOnDate(IHasNextDue entity, DateOnly date)
    {
        return entity.NextDue == date && (entity.EndDate.GetValueOrDefault(DateOnly.MaxValue) >= date);
    }

    private static List<DateProjection> MapToDateBalanceAvailable(IEnumerable<DateProjectionValues> projectionValues)
    {
        return projectionValues.SelectToList(item =>
        {
            var balance = item.StartingBalance + item.IncomeReceived - item.ExpensesPaid;

            return new DateProjection
            {
                Date = item.Date,
                Balance = balance,
                Available = balance - item.Reserved - item.Accrued,
                DailyAccrual = item.DailyAccrual,
                IncomeReceived = item.IncomeReceived,
                ExpensesPaid = item.ExpensesPaid
            };
        });
    }
}
