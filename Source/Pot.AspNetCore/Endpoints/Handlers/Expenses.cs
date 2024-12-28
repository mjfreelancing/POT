namespace Pot.AspNetCore.Endpoints.Handlers;

internal static class Expenses
{
    // Supports DI
    public static IResult GetAll(/*HttpContext context*/)
    {
        return TypedResults.Ok("Get all expenses");
    }
}
