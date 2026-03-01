using Pot.App.Features.Accruals.AccrueExpenses.Models;

namespace Pot.AspNetCore.Features.Accruals.AccrueExpenses.Mappings;

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