using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Errors;
using Pot.App.Features.Projections.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;
using System.Diagnostics;

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

        if (NextDueIsBehindSchedule(options.StartDate, expenses, out var problemDetails) ||
            NextDueIsBehindSchedule(options.StartDate, incomes, out problemDetails))
        {
            return EnrichedResult.Fail<Output>(problemDetails);
        }

        var uniqueAccounts = expenses.Select(e => e.Account)
            .Concat(incomes.Select(i => i.Account))
            .GroupBy(a => a.RowId)
            .Select(g => g.First())
            .ToList();

        var accountDaily = uniqueAccounts.ToDictionary(account => account.RowId, account => new List<DateProjectionValues>());
        var globalDailyProjections = new List<DateProjectionValues>(options.DaysForecast);

        for (int day = 0; day < options.DaysForecast; day++)
        {
            var date = options.StartDate.AddDays(day);

            _expenseRenewalCalculator.Renew(expenses, date);
            _incomeRenewalCalculator.Renew(incomes, date);

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
