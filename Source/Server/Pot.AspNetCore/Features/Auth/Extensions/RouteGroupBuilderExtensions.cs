namespace Pot.AspNetCore.Features.Auth.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder LoginUser(this RouteGroupBuilder routeGroupBuilder)
    {
        // Sign-in: the caller has no session yet.
        routeGroupBuilder
            .MapPost(AuthEndpoints.Login, Login.Handler.Invoke)
            .AllowAnonymous()
            .WithName(nameof(LoginUser))
            .WithSummary("Login")
            .WithDescription("Login the user")
            .Produces<Login.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder LogoutUser(this RouteGroupBuilder routeGroupBuilder)
    {
        // Cookie-driven: the refresh-token cookie identifies the session, not a bearer token.
        routeGroupBuilder
            .MapPost(AuthEndpoints.Logout, Logout.Handler.Invoke)
            .AllowAnonymous()
            .WithName(nameof(LogoutUser))
            .WithSummary("Logout")
            .WithDescription("Logout the user")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder RefreshToken(this RouteGroupBuilder routeGroupBuilder)
    {
        // Cookie-driven: the refresh-token cookie identifies the session, not a bearer token.
        routeGroupBuilder
            .MapPost(AuthEndpoints.Refresh, Refresh.Handler.Invoke)
            .AllowAnonymous()
            .WithName(nameof(RefreshToken))
            .WithSummary("Refresh Access Token")
            .WithDescription("Refresh the user access token")
            .Produces<Refresh.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder PasswordResetSend(this RouteGroupBuilder routeGroupBuilder)
    {
        // Password reset happens while the user is signed out.
        routeGroupBuilder
            .MapPost(AuthEndpoints.PasswordResetSend, PasswordReset.Send.Handler.Invoke)
            .AllowAnonymous()
            .WithName(nameof(PasswordResetSend))
            .WithSummary("Request a password reset")
            .WithDescription("Request a password reset")
            .Produces<PasswordReset.Send.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder PasswordResetVerify(this RouteGroupBuilder routeGroupBuilder)
    {
        // Password reset happens while the user is signed out.
        routeGroupBuilder
            .MapPost(AuthEndpoints.PasswordResetVerify, PasswordReset.Verify.Handler.Invoke)
            .AllowAnonymous()
            .WithName(nameof(PasswordResetVerify))
            .WithSummary("Verify a password reset")
            .WithDescription("Verify a password reset")
            .Produces<PasswordReset.Verify.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder SignupSend(this RouteGroupBuilder routeGroupBuilder)
    {
        // Signup happens before the user exists.
        routeGroupBuilder
            .MapPost(AuthEndpoints.SignupSend, Signup.Send.Handler.Invoke)
            .AllowAnonymous()
            .WithName(nameof(SignupSend))
            .WithSummary("Request to signup")
            .WithDescription("Request to signup")
            .Produces<Signup.Send.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder SignupComplete(this RouteGroupBuilder routeGroupBuilder)
    {
        // Signup happens before the user exists.
        routeGroupBuilder
            .MapPost(AuthEndpoints.SignupComplete, Signup.Complete.Handler.Invoke)
            .AllowAnonymous()
            .WithName(nameof(SignupComplete))
            .WithSummary("Complete a signup request")
            .WithDescription("Complete a signup request")
            .Produces<Signup.Complete.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
