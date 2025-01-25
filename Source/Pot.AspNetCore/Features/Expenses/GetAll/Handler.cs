﻿using Microsoft.EntityFrameworkCore;
using Pot.Data;
using Pot.Data.Extensions;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

internal static class Handler
{
    public static async Task<IResult> Invoke(/*IDbContextFactory<PotDbContext> dbContextFactory,*/ PotDbContext dbContext, CancellationToken cancellationToken)
    {
        //using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from expense in dbContext.Expenses.Include(expense => expense.Account)
                    select expense.ToDto();

        var expenses = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(expenses);
    }
}
