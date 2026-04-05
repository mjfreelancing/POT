using Pot.App.Features.Expenses.Create.Models;

namespace Pot.AspNetCore.Features.Expenses.Create.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            Description = request.Description,
            NextDue = request.NextDue,
            AccrualStart = request.AccrualStart,
            EndDate = request.EndDate,
            AccrualPolicy = request.AccrualPolicy,
            Frequency = request.Frequency,
            FrequencyCount = request.FrequencyCount,
            Amount = request.Amount,
            Note = request.Note,
            AccountRowId = request.AccountRowId
        };
    }
}
