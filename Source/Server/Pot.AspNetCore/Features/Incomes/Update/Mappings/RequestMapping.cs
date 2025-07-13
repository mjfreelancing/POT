using Pot.App.Features.Incomes.Update.Models;

namespace Pot.AspNetCore.Features.Incomes.Update.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            RowId = request.RowId,
            Etag = request.Etag,
            Description = request.Description,
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
