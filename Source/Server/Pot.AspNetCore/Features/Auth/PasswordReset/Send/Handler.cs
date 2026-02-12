using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Errors;
using Pot.App.Features.Auth.PasswordReset.Request;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Auth.PasswordReset.Send.Mappings;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Send;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(Request request,
        HttpContext httpContext, IRequestPasswordResetService passwordResetService,
        IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
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
            var authProblem = ApiDetailErrorFactory.CreateAuthError("Invalid Request");
            return TypedResults.Problem(authProblem.ToProblemDetails());
        }

        var input = request.MapToInput(correlationId);

        var referenceCode = await passwordResetService.RequestResetAsync(input, cancellationToken);

        // Since this is a password reset request, we do not want to expose info such as whether the username
        // is valid or not. The response therefore will always be a 200. All errors are logged for investigation.
        return Response.Ok(referenceCode);
    }
}
