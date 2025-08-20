using Pot.App.Features.Expenses.Exclude.Models;

namespace Pot.AspNetCore.Features.Expenses.Exclude.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            RowIds = request.RowIds,
            ExcludeFromCalcs = request.ExcludeFromCalcs!.Value  // Request validation ensures this is not null
        };
    }
}