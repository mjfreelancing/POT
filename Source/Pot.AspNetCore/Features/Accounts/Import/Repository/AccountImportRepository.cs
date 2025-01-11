using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Extensions;

namespace Pot.AspNetCore.Features.Accounts.Import.Repository
{

    internal sealed class AccountImportRepository : IAccountImportRepository
    {
        private readonly IDbContextFactory<PotDbContext> _dbContextFactory;

        public AccountImportRepository(IDbContextFactory<PotDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory.WhenNotNull();
        }

        public async Task<ImportResult> ImportAccountsAsync(AccountImport[] accounts, CancellationToken cancellationToken)
        {
            var existingIds = await GetExistingIds(accounts, cancellationToken).ConfigureAwait(false);

            using var dbContext = _dbContextFactory.CreateDbContext().WithTracking(true);

            var missingAccounts = accounts.Where(account => !existingIds.Contains(account.Id));

            var newAccounts = missingAccounts.Select(account => new AccountEntity
            {
                Id = account.Id,
                Bsb = account.Bsb,
                Number = account.Number,
                Description = account.Description,
                Balance = account.Balance,
                Reserved = account.Reserved,
                Allocated = account.Allocated,
                DailyAccrual = account.DailyAccrual
            });

            await dbContext.Accounts.AddRangeAsync(newAccounts, cancellationToken).ConfigureAwait(false);

            var importCount = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ImportResult
            {
                Skipped = existingIds.Count,
                Imported = importCount,
                Total = accounts.Length
            };
        }

        private async Task<List<int>> GetExistingIds(AccountImport[] accounts, CancellationToken cancellationToken)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var ids = accounts.SelectToArray(account => account.Id);

            var existingQuery = from account in dbContext.Accounts
                                where ids.Contains(account.Id)
                                select account.Id;

            var existingIds = await existingQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

            return existingIds;
        }
    }
}
