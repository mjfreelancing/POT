using AllOverIt.Assertion;
using AllOverIt.Async;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Projections.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Extensions;
using System.Diagnostics;

namespace Pot.App.Features.Projections;
public sealed class ProjectionOptions
{
    public int DaysForecast { get; init; }
}

internal sealed class ProjectionsService : IProjectionsService
{
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    private readonly IExpenseRepositoryFactory _expenseRepositoryFactory;
    private readonly IIncomeRepositoryFactory _incomeRepositoryFactory;
    private readonly ILogger _logger;

    public ProjectionsService(IExpenseRepositoryFactory expenseRepositoryFactory, IIncomeRepositoryFactory incomeRepositoryFactory,
        ILogger<ProjectionsService> logger)
    {
        _expenseRepositoryFactory = expenseRepositoryFactory.WhenNotNull();
        _incomeRepositoryFactory = incomeRepositoryFactory.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> GetProjectionsAsync(ProjectionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var _expenseRepository = _expenseRepositoryFactory.CreateExpenseRepository();
        var _incomeRepository = _incomeRepositoryFactory.CreateIncomeRepository();

        var (expenses, incomes) = await TaskHelper.WhenAll(
            _expenseRepository.GetAllExpensesAsync(cancellationToken),
            _incomeRepository.GetAllIncomesAsync(cancellationToken)
        );

        var todayDateTime = TimeProvider.GetLocalNow().DateTime;
        var today = DateOnly.FromDateTime(todayDateTime);

        if (NextDueIsBehindSchedule(today, expenses, out var problemDetails) || NextDueIsBehindSchedule(today, incomes, out problemDetails))
        {
            return EnrichedResult.Fail<Output>(problemDetails);
        }

        // Build unique accounts list
        var uniqueAccounts = expenses.Select(e => e.Account)
            .Concat(incomes.Select(i => i.Account))
            .GroupBy(a => a.RowId)
            .Select(g => g.First())
            .ToList();

        // Prepare per-account and global daily projections
        var accountDaily = new Dictionary<Guid, List<DateBalanceAvailable>>(uniqueAccounts.Count);
        var globalDaily = new List<DateBalanceAvailable>(options.DaysForecast);

        // Precompute per-account expense/income lists for fast lookup
        var expensesByAccount = expenses.GroupBy(expense => expense.Account.RowId).ToDictionary(grp => grp.Key, grp => grp.ToList());
        var incomesByAccount = incomes.GroupBy(income => income.Account.RowId).ToDictionary(grp => grp.Key, grp => grp.ToList());

        // Track running next due dates for each expense/income (so we don't mutate the originals)
        var expenseNextDue = expenses.ToDictionary(expense => expense, expense => expense.NextDue);
        var incomeNextDue = incomes.ToDictionary(income => income, income => income.NextDue);

        // Track running accrued for each account
        var accountAccrued = uniqueAccounts.ToDictionary(account => account.RowId, account => account.TotalExpenseAccrued);

        // Track running balance for each account
        var accountBalances = uniqueAccounts.ToDictionary(account => account.RowId, account => account.Balance);
        var accountReserved = uniqueAccounts.ToDictionary(account => account.RowId, account => account.Reserved);

        for (int day = 0; day < options.DaysForecast; day++)
        {
            var date = today.AddDays(day);

            var globalStarting = 0.0d;
            var globalIncome = 0.0d;
            var globalExpenses = 0.0d;
            var globalAccrued = 0.0d;
            var globalReserved = 0.0d;

            foreach (var account in uniqueAccounts)
            {
                var accountId = account.RowId;
                var prevBalance = accountBalances[accountId];
                var prevAccrued = accountAccrued[accountId];
                var reserved = accountReserved[accountId];

                var incomeReceived = 0.0d;
                var expensesPaid = 0.0d;
                var accrued = prevAccrued;

                // Income for this account on this day
                if (incomesByAccount.TryGetValue(accountId, out var accountIncomes))
                {
                    foreach (var income in accountIncomes)
                    {
                        var nextDue = incomeNextDue[income];

                        if (nextDue == date && (income.EndDate.GetValueOrDefault(DateOnly.MaxValue) >= date))
                        {
                            incomeReceived += income.Amount;

                            var daysToIncrement = income.Frequency.GetDaysToNext(date, income.FrequencyCount);
                            incomeNextDue[income] = nextDue.AddDays(daysToIncrement);
                        }
                    }
                }

                // Expenses for this account on this day
                if (expensesByAccount.TryGetValue(accountId, out var accountExpenses))
                {
                    foreach (var expense in accountExpenses)
                    {
                        // Accrual is daily, use account's DailyExpenseAccrual
                        accrued += account.DailyExpenseAccrual;

                        var nextDue = expenseNextDue[expense];
                        if (nextDue == date && (expense.EndDate.GetValueOrDefault(DateOnly.MaxValue) >= date))
                        {
                            expensesPaid += expense.Amount;
                            // Advance next due only if recurring
                            if (expense.Recurring)
                            {
                                var daysToIncrement = expense.Frequency.GetDaysToNext(date, expense.FrequencyCount);
                                expenseNextDue[expense] = nextDue.AddDays(daysToIncrement);
                            }
                        }
                    }
                }

                var dateBalance = new DateBalanceAvailable
                {
                    Date = date,
                    StartingBalance = prevBalance,
                    IncomeReceived = incomeReceived,
                    ExpensesPaid = expensesPaid,
                    Accrued = accrued,
                    Reserved = reserved
                };

                // Update running values
                accountBalances[accountId] = dateBalance.Balance;
                accountAccrued[accountId] = accrued;

                // Store per-account
                if (!accountDaily.TryGetValue(accountId, out var dailyList))
                {
                    dailyList = new List<DateBalanceAvailable>(options.DaysForecast);
                    accountDaily[accountId] = dailyList;
                }

                dailyList.Add(dateBalance);

                // Aggregate for global
                globalStarting += dateBalance.StartingBalance;
                globalIncome += dateBalance.IncomeReceived;
                globalExpenses += dateBalance.ExpensesPaid;
                globalAccrued += dateBalance.Accrued;
                globalReserved += dateBalance.Reserved;
            }

            globalDaily.Add(new DateBalanceAvailable
            {
                Date = date,
                StartingBalance = globalStarting,
                IncomeReceived = globalIncome,
                ExpensesPaid = globalExpenses,
                Accrued = globalAccrued,
                Reserved = globalReserved
            });
        }

        var accountResults = uniqueAccounts
            .SelectToList(account => new AccountDailyBalanceAvailable
            {
                RowId = account.RowId,
                Description = account.Description,
                Dates = accountDaily[account.RowId]
            });

        var output = new Output
        {
            Accounts = accountResults,
            Global = globalDaily
        };

        return EnrichedResult.Success(output);
    }

    private static bool NextDueIsBehindSchedule<TEntity>(DateOnly today, List<TEntity> entities, out ProblemDetailsError? error)
        where TEntity : IHasNextDue
    {
        error = null;

        if (entities.Count == 0)
        {
            return false;
        }

        var nextDue = entities.Min(entity => entity.NextDue);

        if (nextDue.DayNumber >= today.DayNumber)
        {
            return false;
        }

        var entityType = typeof(TEntity) switch
        {
            _ when typeof(TEntity) == typeof(ExpenseEntity) => "expense",
            _ when typeof(TEntity) == typeof(IncomeEntity) => "income",
            _ => throw new UnreachableException($"Unexpected entity type: {typeof(TEntity).GetFriendlyName()}")
        };

        error = new ProblemDetailsError(ProblemType.UnprocessableEntity)
        {
            ErrorCode = ErrorCodes.Invalid,
            ErrorMessage = $"Cannot project financial status. At least one {entityType} has not been advanced to a current next due date."
        };

        return true;
    }
}
