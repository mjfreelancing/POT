namespace Pot.AspNetCore.Features.Auth.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder LoginUser(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AuthEndpoints.Login, Login.Handler.Invoke)
            .WithName(nameof(LoginUser))
            .WithSummary("Login")
            .WithDescription("Login the user")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder LogoutUser(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AuthEndpoints.Logout, Logout.Handler.Invoke)
            .WithName(nameof(LogoutUser))
            .WithSummary("Logout")
            .WithDescription("Logout the user")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder RefreshToken(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AuthEndpoints.Refresh, Refresh.Handler.Invoke)
            .WithName(nameof(RefreshToken))
            .WithSummary("Refresh Access Token")
            .WithDescription("Refresh the user access token")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder PasswordResetSend(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AuthEndpoints.PasswordResetSend, PasswordReset.Send.Handler.Invoke)
            .WithName(nameof(PasswordResetSend))
            .WithSummary("Request a password reset")
            .WithDescription("Request a password reset")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder PasswordResetVerify(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AuthEndpoints.PasswordResetVerify, PasswordReset.Verify.Handler.Invoke)
            .WithName(nameof(PasswordResetVerify))
            .WithSummary("Verify a password reset")
            .WithDescription("Verify a password reset")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder SignupSend(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AuthEndpoints.SignupSend, Signup.Send.Handler.Invoke)
            .WithName(nameof(SignupSend))
            .WithSummary("Request to signup")
            .WithDescription("Request to signup")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder SignupComplete(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AuthEndpoints.SignupComplete, Signup.Complete.Handler.Invoke)
            .WithName(nameof(SignupComplete))
            .WithSummary("Complete a signup request")
            .WithDescription("Complete a signup request")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
