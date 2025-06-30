using Pot.App.Features.Expenses.Update.Models;

namespace Pot.AspNetCore.Features.Expenses.Update.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            RowId = request.RowId,
            Etag = request.Etag,
            Description = request.Description,
            AccrualStart = request.AccrualStart,
            NextDue = request.NextDue,
            EndDate = request.EndDate,
            Frequency = request.Frequency,
            FrequencyCount = request.FrequencyCount,
            Amount = request.Amount,
            Recurring = request.Recurring,
            AccountRowId = request.AccountRowId
        };
    }
}
