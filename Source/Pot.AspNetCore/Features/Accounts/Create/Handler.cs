using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal partial class Handler
{
    public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(Request request,
        IAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var entity = new AccountEntity
        {
            Bsb = request.Bsb,
            Number = request.Number,
            Description = request.Description,
            Balance = request.Balance,
            Reserved = request.Reserved,
            Allocated = request.Allocated,
            DailyAccrual = request.DailyAccrual
        };

        await accountRepository.CreateAsync(entity, cancellationToken);

        var response = new Response
        {
            Id = entity.Id,
            ETag = entity.Etag
        };

        return TypedResults.CreatedAtRoute(response, nameof(Extensions.RouteGroupBuilderExtensions.GetAccount), new { response.Id });
    }
}
