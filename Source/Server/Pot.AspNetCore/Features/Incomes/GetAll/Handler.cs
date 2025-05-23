using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Repositories.Incomes;

namespace Pot.AspNetCore.Features.Incomes.GetAll;

using OkIncomesResult = Ok<PagedResponse<Response>>;

internal sealed class Handler
{
    public static async Task<Results<OkIncomesResult, ProblemHttpResult>> Invoke(Request request, IIncomeRepository incomeRepository,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var incomes = await incomeRepository.GetAllIncomesAsync(request.Paging, cancellationToken);

        return Response.Ok(incomes);
    }
}
