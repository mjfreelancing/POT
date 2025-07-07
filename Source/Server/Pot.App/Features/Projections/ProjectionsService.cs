using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Features.Projections.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Projections;

internal sealed class ProjectionsService : IProjectionsService
{
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    private readonly IExpenseRepository _expenseRepository;
    private readonly IIncomeRepository _incomeRepository;
    private readonly IExpenseRenewalCalculator _expenseRenewalCalculator;
    private readonly IIncomeRenewalCalculator _incomeRenewalCalculator;
    private readonly IAccrueExpenseCalculator _accrueExpenseCalculator;
    private readonly ILogger _logger;

    public ProjectionsService(IExpenseRepository expenseRepository, IIncomeRepository incomeRepository,
        IExpenseRenewalCalculator expenseRenewalCalculator, IIncomeRenewalCalculator incomeRenewalCalculator,
        IAccrueExpenseCalculator accrueExpenseCalculator, ILogger<ProjectionsService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _incomeRepository = incomeRepository.WhenNotNull();
        _expenseRenewalCalculator = expenseRenewalCalculator.WhenNotNull();
        _incomeRenewalCalculator = incomeRenewalCalculator.WhenNotNull();
        _accrueExpenseCalculator = accrueExpenseCalculator.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> GetProjectionsAsync(ProjectionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // Not worrying about doing these in parallel - not worth the effort of maintaining repository factories for use with DbContextFactory.
        var expenses = await _expenseRepository.GetAllExpensesAsync(cancellationToken);
        var incomes = await _incomeRepository.GetAllIncomesAsync(cancellationToken);

        var uniqueAccounts = expenses.Select(expense => expense.Account)
            .Concat(incomes.Select(income => income.Account))
            .DistinctBy(account => account.RowId)
            .ToList();

        // The expense and income entities sharing the same account by value will not be the same by reference. We need them
        // to be the same when forecasting account balances at the start of the loop, so re-assign to the same instances.
        foreach (var account in uniqueAccounts)
        {
            foreach (var expense in expenses.Where(expense => expense.Account.RowId == account.RowId))
            {
                expense.Account = account;
            }

            foreach (var income in incomes.Where(income => income.Account.RowId == account.RowId))
            {
                income.Account = account;
            }
        }

        var accountDaily = uniqueAccounts.ToDictionary(account => account.RowId, account => new List<DateProjectionValues>());
        var globalDailyProjections = new List<DateProjectionValues>(options.DaysForecast);

        for (int day = 0; day < options.DaysForecast; day++)
        {
            var date = options.StartDate.AddDays(day);

            // Apply debit/credit to accounts otherwise the projections will be based on the current balance
            _expenseRenewalCalculator.Renew(expenses, date, day == 0);
            _incomeRenewalCalculator.Renew(incomes, date, day == 0);

            var globalStarting = 0.0d;
            var globalIncome = 0.0d;
            var globalExpenses = 0.0d;
            var globalAccrued = 0.0d;
            var globalReserved = 0.0d;

            foreach (var account in uniqueAccounts)
            {
                _accrueExpenseCalculator.AccrueExpenses(account, expenses, date);

                var incomeReceived = incomes
                    .Where(income => income.Account.RowId == account.RowId && IsDueOnDate(income, date))
                    .Sum(income => income.Amount);

                var expensesPaid = expenses
                    .Where(expense => expense.Account.RowId == account.RowId && IsDueOnDate(expense, date))
                    .Sum(expense => expense.Amount);

                var dateBalance = new DateProjectionValues
                {
                    Date = date,
                    StartingBalance = account.Balance,
                    IncomeReceived = incomeReceived,
                    ExpensesPaid = expensesPaid,
                    Accrued = account.TotalExpenseAccrued,
                    Reserved = account.Reserved
                };

                account.Balance += dateBalance.IncomeReceived - dateBalance.ExpensesPaid;

                if (!accountDaily.TryGetValue(account.RowId, out var dailyList))
                {
                    dailyList = new List<DateProjectionValues>(options.DaysForecast);
                    accountDaily[account.RowId] = dailyList;
                }

                dailyList.Add(dateBalance);

                globalStarting += dateBalance.StartingBalance;
                globalIncome += dateBalance.IncomeReceived;
                globalExpenses += dateBalance.ExpensesPaid;
                globalAccrued += dateBalance.Accrued;
                globalReserved += dateBalance.Reserved;
            }

            globalDailyProjections.Add(new DateProjectionValues
            {
                Date = date,
                StartingBalance = globalStarting,
                IncomeReceived = globalIncome,
                ExpensesPaid = globalExpenses,
                Accrued = globalAccrued,
                Reserved = globalReserved
            });
        }

        var accountDailyProjections = uniqueAccounts
            .SelectToList(account => new AccountDailyProjection
            {
                RowId = account.RowId,
                Description = account.Description,
                Dates = MapToDateBalanceAvailable(accountDaily[account.RowId])
            });

        var output = new Output
        {
            Accounts = accountDailyProjections,
            Global = MapToDateBalanceAvailable(globalDailyProjections)
        };

        return EnrichedResult.Success(output);
    }

    private static bool IsDueOnDate(IHasNextDue entity, DateOnly date)
    {
        return entity.NextDue == date && (entity.EndDate.GetValueOrDefault(DateOnly.MaxValue) >= date);
    }

    private static List<DateProjection> MapToDateBalanceAvailable(List<DateProjectionValues> dateBalances)
    {
        return dateBalances.SelectToList(item =>
        {
            var balance = item.StartingBalance + item.IncomeReceived - item.ExpensesPaid;

            return new DateProjection
            {
                Date = item.Date,
                Balance = balance,
                Available = balance - item.Reserved - item.Accrued,
                IncomeReceived = item.IncomeReceived,
                ExpensesPaid = item.ExpensesPaid
            };
        });
    }
}
