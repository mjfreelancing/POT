using AllOverIt.Assertion;
using AllOverIt.Async;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Projections.Models;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Extensions;

namespace Pot.App.Features.Projections;

public sealed class ProjectionAccountItem
{
    // TODO: Currently assumes the client / server / database are all in the same time-zone.
    internal TimeProvider TimeProvider { get; } = TimeProvider.System;

    private readonly Dictionary<DateOnly, double> _credits = [];
    private readonly Dictionary<DateOnly, double> _debits = [];

    public Guid AccountId { get; }
    public double StartingBalance { get; }
    public IReadOnlyDictionary<DateOnly, double> Credits => _credits;
    public IReadOnlyDictionary<DateOnly, double> Debits => _debits;

    public ProjectionAccountItem(Guid accountId, double startingBalance)
    {
        AccountId = accountId;
        StartingBalance = startingBalance;
    }

    public void AddIncome(DateOnly date, double amount)
    {
        if (_credits.TryGetValue(date, out var currentAmount))
        {
            _credits[date] = currentAmount + amount;
        }
        else
        {
            _credits[date] = amount;
        }
    }

    public void AddExpense(DateOnly date, double amount)
    {
        if (_debits.TryGetValue(date, out var currentAmount))
        {
            _debits[date] = currentAmount + amount;
        }
        else
        {
            _debits[date] = amount;
        }
    }
}

public sealed class ProjectionOptions
{
    public int DaysForecast { get; init; }
}

internal sealed class ProjectionsService : IProjectionsService
{
    private readonly IExpenseRepositoryFactory _expenseRepositoryFactory;
    private readonly IIncomeRepositoryFactory _incomeRepositoryFactory;
    private readonly ILogger _logger;

    public ProjectionsService(IExpenseRepositoryFactory expenseRepositoryFactory, IIncomeRepositoryFactory incomeRepositoryFactory, ILogger<ProjectionsService> logger)
    {
        _expenseRepositoryFactory = expenseRepositoryFactory.WhenNotNull();
        _incomeRepositoryFactory = incomeRepositoryFactory.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<Output> GetProjectionsAsync(ProjectionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // Can't execute two queries in parallel using the same DbContext.
        var _expenseRepository = _expenseRepositoryFactory.CreateExpenseRepository();
        var _incomeRepository = _incomeRepositoryFactory.CreateIncomeRepository();

        var (expenses, incomes) = await TaskHelper.WhenAll(
            _expenseRepository.GetAllExpensesAsync(cancellationToken),
            _incomeRepository.GetAllIncomesAsync(cancellationToken)
        );

        // Do not perform projections if any expense or income 'NextDue' date has not been advanced.
        var expenseNextDue = expenses.Min(expense => expense.NextDue);
        var incomeNextDue = incomes.Min(income => income.NextDue);

        var today = DateOnly.FromDateTime(DateTime.Today);

        // TODO: Change this to a FailResult
        if (Math.Min(expenseNextDue.DayNumber, incomeNextDue.DayNumber) < today.DayNumber)
        {
            throw new Exception("Cannot perform projections as some expenses or incomes have not been advanced to their next due date.");
        }

        //
        // TODO: Update the response so it also projects the available balance - will have to recalculate accrual etc.
        //

        // Only processing accounts that have either an expense or income associated with them.
        var expenseAccounts = expenses.Select(expense => expense.Account);
        var incomesAccounts = incomes.Select(income => income.Account);

        var uniqueAccounts = expenseAccounts
            .Concat(incomesAccounts)
            .GroupBy(account => account.RowId)
            .Select(grp => grp.First())
            .ToList();

        var projectedAccounts = uniqueAccounts
            .Select(account =>
            {
                var accountId = account.RowId;
                var currentBalance = account.Balance;
                var accountItem = new ProjectionAccountItem(accountId, currentBalance);

                return new KeyValuePair<Guid, ProjectionAccountItem>(accountId, accountItem);
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Forecast the account balances
        for (var day = 0; day < options.DaysForecast; day++)
        {
            var date = today.AddDays(day);

            foreach (var income in incomes)
            {
                var accountItem = projectedAccounts[income.Account.RowId];

                if (income.NextDue == date)
                {
                    accountItem.AddIncome(date, income.Amount);

                    // update the next due date based on the frequency
                    if (income.EndDate > date)
                    {
                        income.NextDue = income.NextDue.AddDays(income.Frequency.GetDays(date, income.FrequencyCount));
                    }
                }
                else
                {
                    accountItem.AddIncome(date, 0);
                }
            }

            foreach (var expense in expenses)
            {
                var accountItem = projectedAccounts[expense.Account.RowId];

                if (expense.NextDue == date)
                {
                    accountItem.AddExpense(date, expense.Amount);

                    // update the next due date based on the frequency
                    if (expense.EndDate.GetValueOrDefault(DateOnly.MaxValue) > date)
                    {
                        expense.NextDue = expense.NextDue.AddDays(expense.Frequency.GetDays(date, expense.FrequencyCount));

                    }
                }
                else
                {
                    accountItem.AddExpense(date, 0);
                }
            }
        }

        // Transform the transactions into daily balances
        var globalBalances = new Dictionary<DateOnly, double>();

        var accountBalances = projectedAccounts
            .ToDictionary(kvp => kvp.Key, kvp =>
            {
                var accountItem = kvp.Value;
                var runningBalance = accountItem.StartingBalance;

                return accountItem.Credits
                    .Zip(accountItem.Debits, (credit, debit) =>
                    {
                        var date = credit.Key;

                        // New balance for the current account
                        runningBalance += credit.Value - debit.Value;

                        // Update the global balance for the date
                        if (globalBalances.TryGetValue(date, out var globalBalance))
                        {
                            globalBalances[date] = globalBalance + runningBalance;
                        }
                        else
                        {
                            globalBalances[date] = runningBalance;
                        }

                        // Return account balance for the specific date
                        return new DateBalance { Date = date, Balance = runningBalance };
                    })
                    .ToList();
            });

        return new Output
        {
            Accounts = new AccountsDailyBalances(accountBalances),
            Global = new GlobalDailyBalances(globalBalances)
        };
    }
}
