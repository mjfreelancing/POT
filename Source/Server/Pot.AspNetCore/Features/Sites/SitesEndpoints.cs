namespace Pot.AspNetCore.Features.Sites;

internal static class SitesEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/sites";
    public const string Tag = "Sites Api";

    public const string Update = "/{id:guid}";
}
