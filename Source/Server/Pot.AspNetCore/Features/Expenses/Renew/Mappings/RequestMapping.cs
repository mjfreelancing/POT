using Pot.App.Features.Expenses.Renew.Models;

namespace Pot.AspNetCore.Features.Expenses.Renew.Mappings;

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