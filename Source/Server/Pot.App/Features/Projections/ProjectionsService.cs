using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Features.Projections.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Projections;
using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;

namespace Pot.App.Features.Projections;

internal sealed class ProjectionsService : IProjectionsService
{
    private readonly IProjectionsRepository _projectionsRepository;
    private readonly IExpenseRenewalCalculator _expenseRenewalCalculator;
    private readonly IIncomeRenewalCalculator _incomeRenewalCalculator;
    private readonly IAccrueExpenseCalculator _accrueExpenseCalculator;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public ProjectionsService(IProjectionsRepository accountRepository, IExpenseRenewalCalculator expenseRenewalCalculator,
        IIncomeRenewalCalculator incomeRenewalCalculator, IAccrueExpenseCalculator accrueExpenseCalculator,
        ITimeProvider timeProvider, ILogger<ProjectionsService> logger)
    {
        _projectionsRepository = accountRepository.WhenNotNull();
        _expenseRenewalCalculator = expenseRenewalCalculator.WhenNotNull();
        _incomeRenewalCalculator = incomeRenewalCalculator.WhenNotNull();
        _accrueExpenseCalculator = accrueExpenseCalculator.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    // TODO: Move the logic in this service to a projection calculator (keep repository access here)
    public async Task<EnrichedResult<Output>> GetFinancialProjectionsAsync(ProjectionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // The start date may be into the future so we need to aggregate data from today until the start date
        var (localDate, preStartDays) = GetPreStartDays(options.StartDate);

        // Will only contain incomes/expenses that are not excluded from calculations
        var accounts = await _projectionsRepository.GetAllAccountsWithCandidateIncomesAndExpensesAsync(cancellationToken);

        var accountExpenses = accounts.ToDictionary(account => account, account => account.Expenses);
        var accountIncomes = accounts.ToDictionary(account => account, account => account.Incomes);

        var accountDaily = accounts.ToDictionary(account => account.RowId, account => new List<DateProjectionValues>(options.DaysForecast));

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
            var globalExpenseItems = new List<ProjectionExpenseModel>();
            var globalIncomeItems = new List<ProjectionIncomeModel>();

            foreach (var account in accounts)
            {
                var expenses = accountExpenses[account];

                var expensesDue = expenses
                    .Where(expense => expense.Amount > 0.0d && IsDueOnDate(expense, date))
                    .ToArray();

                var expensesPaid = expensesDue.Sum(expense => expense.Amount);

                var incomes = accountIncomes[account];

                var incomesDue = incomes
                    .Where(income => income.Amount > 0 && IsDueOnDate(income, date))
                    .ToArray();

                var incomeReceived = incomesDue.Sum(income => income.Amount);

                _expenseRenewalCalculator.Renew(expenses, RenewalMode.Overdue, date);
                _incomeRenewalCalculator.Renew(incomes, RenewalMode.Overdue, date);
                _accrueExpenseCalculator.AccrueExpenses(account, expenses, date);

                var dateValues = new DateProjectionValues
                {
                    Date = date,
                    StartingBalance = account.Balance,
                    IncomeReceived = incomeReceived,
                    ExpensesPaid = expensesPaid,
                    DailyAccrual = account.DailyExpenseAccrual,
                    Accrued = account.TotalExpenseAccrued,
                    Reserved = account.Reserved,
                    ExpenseItems = expensesDue.SelectToArray(expense => new ProjectionExpenseModel
                    {
                        RowId = expense.RowId,
                        Description = expense.Description,
                        Amount = expense.Amount
                    }),
                    IncomeItems = incomesDue.SelectToArray(income => new ProjectionIncomeModel
                    {
                        RowId = income.RowId,
                        Description = income.Description,
                        Amount = income.Amount
                    })
                };

                account.Balance += dateValues.IncomeReceived - dateValues.ExpensesPaid;

                if (date >= options.StartDate)
                {
                    var dailyList = accountDaily[account.RowId];
                    dailyList.Add(dateValues);
                }

                globalStarting += dateValues.StartingBalance;
                globalIncome += dateValues.IncomeReceived;
                globalExpenses += dateValues.ExpensesPaid;
                globalDailyAccrual += dateValues.DailyAccrual;
                globalAccrued += dateValues.Accrued;
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
                    DailyAccrual = globalDailyAccrual,
                    Accrued = globalAccrued,
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
            Accounts = accountDailyFinancialProjections,
            Global = MapToDateBalanceAvailable(globalDailyProjections)
        };

        return EnrichedResult.Success(output);
    }

    private (DateOnly localDate, int preStartDays) GetPreStartDays(DateOnly startDate)
    {
        var localDate = _timeProvider.GetLocalDateNow();

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

                // Expenses due TODAY are fully accrued. They are not actually considered paid until they are renewed.
                // For the purposes of projections, we consider them paid today so we need to add ExpensesPaid back
                // to get the true available balance because the accrued amount already considers the expense total.
                Available = balance - item.Reserved - item.Accrued + item.ExpensesPaid,

                DailyAccrual = item.DailyAccrual,
                IncomeReceived = item.IncomeReceived,
                ExpensesPaid = item.ExpensesPaid,
                ExpenseItems = item.ExpenseItems,
                IncomeItems = item.IncomeItems
            };
        });
    }
}
