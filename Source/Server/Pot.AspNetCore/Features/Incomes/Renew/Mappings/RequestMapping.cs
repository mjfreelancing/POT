using Pot.App.Features.Incomes.Renew.Models;

namespace Pot.AspNetCore.Features.Incomes.Renew.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            RowIds = request.RowIds,
            AsOfDate = request.AsOfDate,
            Mode = request.Mode
        };
    }
}