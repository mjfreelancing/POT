using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Pot.Data;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal class Handler
{
    public static async Task<Results<Created<CreatedResponse>, ProblemHttpResult /*ValidationProblem*/>> Invoke(IDbContextFactory<PotDbContext> dbContextFactory,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        //using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        //var query = from account in dbContext.Accounts
        //            select account.ToDto();

        //var accounts = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        //return TypedResults.Ok(accounts);

        await Task.Delay(1, cancellationToken);

        var response = new CreatedResponse { Id = 1, ETag = 2 };

        return TypedResults.Created($"{nameof(Extensions.RouteGroupBuilderExtensions.GetAccount)}/{response.Id}", response);
    }

    internal sealed class CreatedResponse
    {
        public int Id { get; init; }
        public long ETag { get; init; }
    }
}

// CREATE AN ABSTRACT FACTORY

// Use 304 for updates not modified