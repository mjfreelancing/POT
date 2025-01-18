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
    private sealed record AccountKey(string Bsb, string Number);

    private readonly IDbContextFactory<PotDbContext> _dbContextFactory;

    public AccountImportRepository(IDbContextFactory<PotDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory.WhenNotNull();
    }

    public async Task<ImportSummary> ImportAccountsAsync(AccountForImport[] accountsImport, bool overwrite, CancellationToken cancellationToken)
    {
        var importAccountKeys = accountsImport.ToDictionary(account => new AccountKey(account.Bsb, account.Number));

        using var dbContext = _dbContextFactory.CreateDbContext().WithTracking(true);

        var imported = 0;
        var updated = 0;

        foreach (var import in accountsImport)
        {
            var existing = await dbContext.Accounts
                .Where(account => account.Bsb == import.Bsb && account.Number == import.Number)
                .SingleOrDefaultAsync(cancellationToken);

            if (existing is null)
            {
                AddAccountEntity(dbContext, import);
                imported++;
            }
            else if (overwrite)
            {
                UpdateAccountEntity(existing, import);
                updated++;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ImportSummary
        {
            Skipped = accountsImport.Length - imported - updated,
            Imported = imported,
            Updated = updated,
            Total = accountsImport.Length
        };
    }

    private static void AddAccountEntity(PotDbContext dbContext, AccountForImport import)
    {
        var newAccount = new AccountEntity
        {
            Bsb = import.Bsb,
            Number = import.Number,
            Description = import.Description,
            Balance = import.Balance,
            Reserved = import.Reserved,
            Allocated = import.Allocated,
            DailyAccrual = import.DailyAccrual
        };

        dbContext.Accounts.Add(newAccount);
    }

    private static void UpdateAccountEntity(AccountEntity entity, AccountForImport import)
    {
        entity.Bsb = import.Bsb;
        entity.Number = import.Number;
        entity.Description = import.Description;
        entity.Balance = import.Balance;
        entity.Reserved = import.Reserved;
        entity.Allocated = import.Allocated;
        entity.DailyAccrual = import.DailyAccrual;
    }
}
