using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Errors;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Auth;

internal static class AuthUtils
{
    public static ProblemHttpResult CreateAuthErrorResult()
    {
        var authError = ProblemDetailsErrorFactory.CreateAuthError("The username or password is invalid.");

        return TypedResults.Problem(authError.ToProblemDetails());
    }
}