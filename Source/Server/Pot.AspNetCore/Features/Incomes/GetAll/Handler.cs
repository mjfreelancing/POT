using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.GetAll;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Incomes.GetAll;

using OkGetAllResult = Ok<PagedResponse<Response>>;

internal sealed class Handler
{
    public static async Task<Results<OkGetAllResult, ProblemHttpResult>> Invoke(Request request,
        IGetAllIncomesService incomeService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var incomes = await incomeService.GetAllIncomesAsync(request.Paging, cancellationToken);

        return Response.Ok(incomes);
    }
}
