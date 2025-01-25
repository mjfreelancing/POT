using Pot.AspNetCore.Features.Expenses.Import.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Repository
{
    internal sealed class ExpenseImportRepository : IExpenseImportRepository
    {
        //private readonly IDbContextFactory<PotDbContext> _dbContextFactory;

        // TODO: Not using IDbContextFactory now - not registered
        public ExpenseImportRepository(/*IDbContextFactory<PotDbContext> dbContextFactory*/)
        {
            //_dbContextFactory = dbContextFactory.WhenNotNull();
        }

        public /*async*/ Task ImportExpensesAsync(ExpenseImport[] expenses, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;

            // TO BE REFACTORED TO USE THE NEW REPOSITORY APPROACH

            //using var dbContext = _dbContextFactory.CreateDbContext().WithTracking(true);

            //await expenses
            //    .GroupBy(expense => expense.AccountId)
            //    .ForEachAsync(async (grp, _, token) =>
            //    {
            //        var accountId = grp.Key;

            //        var account = await dbContext.Accounts.SingleAsync(account => account.Id == accountId, token);

            //        var entities = grp.Select(expense => new ExpenseEntity
            //        {
            //            Id = expense.Id,
            //            Description = expense.Description,
            //            NextDue = expense.NextDue,
            //            AccrualStart = expense.AccrualStart,
            //            Frequency = expense.Frequency,
            //            FrequencyCount = expense.FrequencyCount,
            //            Recurring = expense.Recurring,
            //            Amount = expense.Amount,
            //            Allocated = expense.Allocated,
            //            Account = account
            //        });

            //        await dbContext.Expenses.AddRangeAsync(entities, cancellationToken);
            //    }, cancellationToken);

            //await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
