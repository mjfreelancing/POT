using Pot.App.Features.Expenses.Exclude.Models;

namespace Pot.AspNetCore.Features.Expenses.ToggleExclude.Mappings;

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