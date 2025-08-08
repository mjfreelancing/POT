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
            ExcludeFromCalcs = request.ExcludeFromCalcs!.Value, // Will have been validated
            Description = request.Description,
            AccrualStart = request.AccrualStart,
            NextDue = request.NextDue,
            EndDate = request.EndDate,
            Frequency = request.Frequency,
            FrequencyCount = request.FrequencyCount,
            Amount = request.Amount,
            Note = request.Note,
            AccountRowId = request.AccountRowId
        };
    }
}
