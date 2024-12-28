namespace Pot.AspNetCore.Endpoints.Handlers;

internal static class Accounts
{
    // Supports DI
    public static IResult GetAll(/*HttpContext context*/)
    {
        return TypedResults.Ok("Get all accounts");
    }
}
