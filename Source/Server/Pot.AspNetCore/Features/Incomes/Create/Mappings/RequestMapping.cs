using Pot.App.Features.Incomes.Create.Models;

namespace Pot.AspNetCore.Features.Incomes.Create.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            Description = request.Description,
            NextDue = request.NextDue,
            EndDate = request.EndDate,
            Frequency = request.Frequency,
            FrequencyCount = request.FrequencyCount,
            Amount = request.Amount,
            AccountRowId = request.AccountRowId
        };
    }
}
