using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Extensions;

namespace Pot.AspNetCore.Features.Accounts.Import.Repository;

internal sealed class AccountImportRepository : IAccountImportRepository
{
    private readonly IDbContextFactory<PotDbContext> _dbContextFactory;

    public AccountImportRepository(IDbContextFactory<PotDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory.WhenNotNull();
    }

    public async Task<ImportSummary> ImportAccountsAsync(AccountForImport[] accounts, bool overwrite, CancellationToken cancellationToken)
    {
        using var dbContext = _dbContextFactory.CreateDbContext().WithTracking(true);

        var existingAccounts = await GetExistingAccountsAsync(dbContext, accounts, cancellationToken).ConfigureAwait(false);

        var existingIds = accounts
            .FindMatches(existingAccounts, account => account.Id, account => account.Id)
            .SelectToArray(account => account.Id);

        var updated = overwrite
            ? await UpdateExistingAccountsAsync(dbContext, accounts, existingIds, cancellationToken).ConfigureAwait(false)
            : 0;

        var imported = await AddNewAccountsAsync(dbContext, accounts, existingIds, cancellationToken).ConfigureAwait(false);

        return new ImportSummary
        {
            Skipped = existingIds.Length - updated,
            Imported = imported,
            Updated = updated,
            Total = accounts.Length
        };
    }

    private static async Task<List<AccountEntity>> GetExistingAccountsAsync(PotDbContext dbContext, AccountForImport[] accounts,
        CancellationToken cancellationToken)
    {
        var accountIds = accounts.SelectToArray(account => account.Id);

        var accountsQuery = from account in dbContext.Accounts
                            where accountIds.Contains(account.Id)
                            select account;

        return await accountsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<int> UpdateExistingAccountsAsync(PotDbContext dbContext, AccountForImport[] accounts,
        int[] existingIds, CancellationToken cancellationToken)
    {
        accounts
            .Where(account => existingIds.Contains(account.Id))
            .ForEach((account, _) =>
            {
                // Will return the account entity already being tracked
                var accountEntity = dbContext.Find<AccountEntity>([account.Id])!;

                accountEntity.Id = account.Id;
                accountEntity.Bsb = account.Bsb;
                accountEntity.Number = account.Number;
                accountEntity.Description = account.Description;
                accountEntity.Balance = account.Balance;
                accountEntity.Reserved = account.Reserved;
                accountEntity.Allocated = account.Allocated;
                accountEntity.DailyAccrual = account.DailyAccrual;
            });

        return await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<int> AddNewAccountsAsync(PotDbContext dbContext, AccountForImport[] accounts,
        int[] existingIds, CancellationToken cancellationToken)
    {
        var missingAccounts = accounts.Where(account => !existingIds.Contains(account.Id));

        var newAccounts = missingAccounts.SelectToArray(account => new AccountEntity
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

        return await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
