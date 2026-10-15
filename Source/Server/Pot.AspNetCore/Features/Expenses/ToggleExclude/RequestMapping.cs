using Pot.App.Features.Expenses.ToggleExclude.Models;

namespace Pot.AspNetCore.Features.Expenses.ToggleExclude;

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