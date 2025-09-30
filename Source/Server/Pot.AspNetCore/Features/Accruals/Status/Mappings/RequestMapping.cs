using Pot.App.Features.Accruals.Status.Models;

namespace Pot.AspNetCore.Features.Accruals.Status.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, DateOnly asOfDate)
    {
        return new Input
        {
            AccountRowIds = request.AccountRowIds,
            BeforeDate = asOfDate
        };
    }
}