namespace Pot.AspNetCore.Features.Auth;

internal static class AuthEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/auth";
    public const string Tag = "Auth Api";

    public const string Login = "/login";
    public const string Logout = "/logout";
    public const string Refresh = "/refresh";
    public const string PasswordResetSend = "/password-reset/send";
    public const string PasswordResetVerify = "/password-reset/verify";
}

