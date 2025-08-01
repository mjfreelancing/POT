using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Concerns.Time;
using Pot.App.Features.Projections;
using Pot.App.Features.Projections.Models;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.Shared.Extensions;

namespace Pot.AspNetCore.Features.Projections.Get;

using OkGetResult = Ok<Response>;

internal sealed class Handler
{
    public static async Task<Results<OkGetResult, NotFound, ProblemHttpResult>> Invoke([AsParameters] Request request,
         IProblemDetailsInspector problemDetailsInspector, IProjectionsService projectionsService, ITimeProvider timeProvider,
         ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var validationContext = new RequestValidationContext
        {
            Today = DateOnly.FromDateTime(timeProvider.GetLocalNow().Date)
        };

        var problemDetails = problemDetailsInspector.Validate(request, validationContext);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var options = new ProjectionOptions
        {
            StartDate = request.StartDate,
            DaysForecast = request.StartDate.DaysUntil(request.EndDate) + 1
        };

        var output = await projectionsService.GetProjectionsAsync(options, cancellationToken);

        return output.IsSuccess
            ? Response.Ok(output.Value!)
            : TypedResults.Problem(output.Error!.GetProblemDetails());
    }
}
