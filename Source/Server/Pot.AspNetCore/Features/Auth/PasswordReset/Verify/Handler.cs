using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Errors;
using Pot.App.Features.Auth.PasswordReset.Verify;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Auth.PasswordReset.Verify.Mappings;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Verify;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(Request request,
        HttpContext httpContext, IVerifyPasswordResetService passwordResetService,
        IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null, new { request.Username, request.ReferenceCode, request.VerificationCode });

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            // Don't give any clues to bad actors
            var authProblem = ProblemDetailsErrorFactory.CreateAuthError("Invalid Request");
            return TypedResults.Problem(authProblem.ToProblemDetails());
        }

        var input = request.MapToInput();

        var output = await passwordResetService.VerifyResetAsync(input, cancellationToken);

        return output.IsSuccess
            ? Response.Ok(output.Value!)
            : TypedResults.Problem(output.Error!.ToProblemDetails());
    }
}
