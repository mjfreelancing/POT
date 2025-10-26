using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Errors;
using Pot.App.Features.Auth.Signup.Request;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Auth.Signup.Send.Mappings;

namespace Pot.AspNetCore.Features.Auth.Signup.Send;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(Request request,
        HttpContext httpContext, IRequestSignupService requestSignupService,
        IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        cancellationToken = CancellationToken.None;

        logger.LogCall(null, new { request.Username });

        if (!httpContext.Request.TryGetCorrelationId(out var correlationId))
        {
            // Bad actors are unlikely to set a correlation id so we capture the trace identifier
            correlationId = httpContext.TraceIdentifier;
        }

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            // Don't give any clues to bad actors
            var authProblem = ProblemDetailsErrorFactory.CreateAuthError("Invalid Request");
            return TypedResults.Problem(authProblem.ToProblemDetails());
        }

        var input = request.MapToInput(correlationId);

        var output = await requestSignupService.RequestSignupAsync(input, cancellationToken);

        return output.IsSuccess
            ? Response.Ok(output.Value!)
            : TypedResults.Problem(output.Error!.ToProblemDetails());
    }
}
