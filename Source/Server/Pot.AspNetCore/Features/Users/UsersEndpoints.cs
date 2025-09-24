namespace Pot.AspNetCore.Features.Users;

internal static class UsersEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/users";
    public const string Tag = "Users Api";

    public const string Me = "/me";
    public const string ChangePassword = "/change-password";
}
