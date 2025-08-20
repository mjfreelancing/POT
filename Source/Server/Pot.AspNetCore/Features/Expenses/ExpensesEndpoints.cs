namespace Pot.AspNetCore.Features.Expenses;

internal static class ExpensesEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/expenses";
    public const string Tag = "Expenses Api";

    public const string GetAll = "";
    public const string Get = "/{id:guid}";
    public const string Create = "";
    public const string Update = "";
    public const string Delete = "/{id:guid}";
    public const string Renew = "/renew";
    public const string Exclude = "/exclude";
}
