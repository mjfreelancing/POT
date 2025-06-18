using AllOverIt.Assertion;
using AllOverIt.Async;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Projections.Models;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Extensions;

namespace Pot.App.Features.Projections;

using DailyDateBalanceAvailable = Dictionary<DateOnly, DateBalanceAvailable>;

public sealed class ProjectionOptions
{
    public int DaysForecast { get; init; }
}

internal sealed class ProjectionsService : IProjectionsService
{
    internal TimeProvider TimeProvider { get; } = TimeProvider.System;

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

        var todayDateTime = TimeProvider.GetLocalNow().DateTime;
        var today = DateOnly.FromDateTime(todayDateTime);

        if (Math.Min(expenseNextDue.DayNumber, incomeNextDue.DayNumber) < today.DayNumber)
        {
            var problemDetails = new ProblemDetailsError(ProblemType.UnprocessableEntity)
            {
                ErrorCode = ErrorCodes.Invalid,
                ErrorMessage = "Cannot project financial status as some expenses or incomes have not been advanced to their next due date."
            };

            return EnrichedResult.Fail<Output>(problemDetails);
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

        var global = new Dictionary<DateOnly, DateBalanceAvailable>();
        var accountDailyAmounts = new Dictionary<Guid, DailyDateBalanceAvailable>();       // transformed later to new List<AccountDailyBalanceAvailable>();

        // Pre-initialise each account daily balance and available amounts
        foreach (var account in uniqueAccounts)
        {
            var accountId = account.RowId;
            var dailyBalanceAvailable = new DailyDateBalanceAvailable();
            accountDailyAmounts[accountId] = dailyBalanceAvailable;

            for (var day = 0; day < options.DaysForecast; day++)
            {
                var date = today.AddDays(day);

                dailyBalanceAvailable[date] = new DateBalanceAvailable
                {
                    Date = date,
                    Balance = account.Balance,
                    Accrued = account.TotalExpenseAccrued
                };
            }
        }

        // Start processing
        var runningBalance = uniqueAccounts.Sum(account => account.Balance);
        var runningAccrued = uniqueAccounts.Sum(account => account.TotalExpenseAccrued);

        for (var day = 0; day < options.DaysForecast; day++)
        {
            var date = today.AddDays(day);

            var globalBalanceAvailable = new DateBalanceAvailable
            {
                Date = date,
                Balance = runningBalance,
                Accrued = runningBalance - runningAccrued
            };

            global[date] = globalBalanceAvailable;

            if (day != 0)
            {
                foreach (var account in uniqueAccounts)
                {
                    var accountDailyAmount = accountDailyAmounts[account.RowId][date];
                    accountDailyAmount.Accrued += account.DailyExpenseAccrual;
                    runningAccrued += account.DailyExpenseAccrual;
                }
            }

            // Track income received
            foreach (var income in incomes)
            {
                var dailyBalanceAvailable = accountDailyAmounts[income.Account.RowId];
                var dateBalanceAvailable = dailyBalanceAvailable[date];

                if (income.NextDue == date)
                {
                    dateBalanceAvailable.Balance += income.Amount;
                    runningBalance += income.Amount;

                    // update the next due date based on the frequency
                    if (income.EndDate.GetValueOrDefault(DateOnly.MaxValue) >= date)
                    {
                        income.NextDue = income.NextDue.AddDays(income.Frequency.GetDays(date, income.FrequencyCount));
                    }
                }
            }

            // Track expenses paid
            foreach (var expense in expenses)
            {
                var dailyBalanceAvailable = accountDailyAmounts[expense.Account.RowId];
                var dateBalanceAvailable = dailyBalanceAvailable[date];

                if (expense.NextDue == date)
                {
                    dateBalanceAvailable.Balance -= expense.Amount;
                    runningBalance -= expense.Amount;

                    // update the next due date based on the frequency
                    if (expense.EndDate.GetValueOrDefault(DateOnly.MaxValue) >= date)
                    {
                        expense.NextDue = expense.NextDue.AddDays(expense.Frequency.GetDays(date, expense.FrequencyCount));
                    }
                }
            }
        }

        var accountDailyuAmounts = accountDailyAmounts
            .SelectToList(kvp =>
            {
                var accountId = kvp.Key;
                var dailyAmounts = kvp.Value;
                var description = uniqueAccounts.Single(account => account.RowId == accountId).Description;

                return new AccountDailyBalanceAvailable
                {
                    RowId = accountId,
                    Description = description,
                    Dates = dailyAmounts.Values.ToList()
                };
            });

        var output = new Output
        {
            Accounts = accountDailyuAmounts,
            Global = [.. global.Values]
        };

        return EnrichedResult.Success(output);
    }
}
