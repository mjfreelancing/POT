namespace Pot.AspNetCore.Features.Accounts;

internal static class AccountsEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/accounts";
    public const string Tag = "Accounts Api";

    public const string GetAll = "";
    public const string Get = "/{id:guid}";
    public const string Create = "";
    public const string Update = "";
    public const string Delete = "/{id:guid}";
    public const string AccrueExpenses = "/accrue-expenses";
}
