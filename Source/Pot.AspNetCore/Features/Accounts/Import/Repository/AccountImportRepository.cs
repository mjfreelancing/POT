using AllOverIt.Assertion;
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

        public async Task ImportAccountsAsync(AccountImport[] accounts, CancellationToken cancellationToken)
        {
            using var dbContext = _dbContextFactory.CreateDbContext().WithTracking(true);

            var entities = accounts.Select(account => new AccountEntity
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

            await dbContext.Accounts.AddRangeAsync(entities, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
