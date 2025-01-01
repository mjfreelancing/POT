using Microsoft.EntityFrameworkCore;
using Pot.Data;
using Pot.Data.Extensions;

namespace Pot.AspNetCore.Features.Accounts.GetAll;

internal static class Handler
{
    // When potentially returning more than one type
    // Task<Results<Ok<List<AccountResponse>>, NotFound>>
    public static async Task<IResult> GetAllAccounts(IDbContextFactory<PotDbContext> dbContextFactory, CancellationToken cancellationToken)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from account in dbContext.Accounts
                    select account.ToDto();

        var accounts = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(accounts);
    }
}
