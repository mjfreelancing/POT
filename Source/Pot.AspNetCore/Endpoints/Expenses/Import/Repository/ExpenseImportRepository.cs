﻿using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.AspNetCore.Endpoints.Expenses.Import.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Extensions;

namespace Pot.AspNetCore.Endpoints.Expenses.Import.Repository
{
    internal sealed class ExpenseImportRepository : IExpenseImportRepository
    {
        private readonly IDbContextFactory<PotDbContext> _dbContextFactory;

        public ExpenseImportRepository(IDbContextFactory<PotDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory.WhenNotNull();
        }

        public async Task ImportExpensesAsync(ExpenseImport[] expenses, CancellationToken cancellationToken)
        {
            using var dbContext = _dbContextFactory.CreateDbContext().WithTracking(true);

            await expenses
                .GroupBy(expense => expense.AccountId)
                .ForEachAsync(async (grp, _, token) =>
                {
                    var accountId = grp.Key;

                    var account = await dbContext.Accounts.SingleAsync(account => account.Id == accountId, token);

                    // TODO: Handle dates local vs UTC
                    // TODO: Try DateOnly since we don't need timestamps ?? except UTC will

                    var entities = grp.Select(expense => new ExpenseEntity
                    {
                        Id = expense.Id,
                        Description = expense.Description,
                        NextDue = expense.NextDue.ToUniversalTime(),
                        AccrualStart = expense.AccrualStart.ToUniversalTime(),
                        Frequency = expense.Frequency,
                        FrequencyCount = expense.FrequencyCount,
                        Recurring = expense.Recurring,
                        Amount = expense.Amount,
                        Allocated = expense.Allocated,
                        Account = account
                    });

                    await dbContext.Expenses.AddRangeAsync(entities, cancellationToken);
                }, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
