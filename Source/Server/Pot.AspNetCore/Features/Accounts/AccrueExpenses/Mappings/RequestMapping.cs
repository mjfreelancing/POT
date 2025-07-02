using Pot.App.Features.Accounts.AccrueExpenses.Models;

namespace Pot.AspNetCore.Features.Accounts.AccrueExpenses.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            RowIds = request.RowIds
        };
    }
}