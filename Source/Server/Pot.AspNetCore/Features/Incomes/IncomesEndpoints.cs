namespace Pot.AspNetCore.Features.Incomes;

internal static class IncomesEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/incomes";
    public const string Tag = "Incomes Api";

    public const string GetAll = "";
    public const string Get = "/{id:guid}";
    public const string Create = "";
    public const string Update = "";
    public const string Delete = "/{id:guid}";
    public const string Renew = "/renew";
    public const string Exclude = "/exclude";
}
