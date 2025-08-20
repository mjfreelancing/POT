using Pot.App.Features.Incomes.Exclude.Models;

namespace Pot.AspNetCore.Features.Incomes.ToggleExclude.Mappings;

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