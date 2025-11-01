namespace Pot.AspNetCore.Features.Users;

internal static class UsersEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/users";
    public const string Tag = "Users Api";

    public const string GetAll = "";
    public const string Update = "/{id:guid}";
    public const string UpdateRoles = "/{id:guid}/roles";
    public const string UpdateStatus = "/{id:guid}/status";
    public const string Invite = "/invite";
}
