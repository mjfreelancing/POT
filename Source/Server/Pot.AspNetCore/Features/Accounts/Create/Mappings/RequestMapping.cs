using Pot.App.Features.Accounts.Create.Models;

namespace Pot.AspNetCore.Features.Accounts.Create.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            Bsb = request.Bsb,
            Number = request.Number,
            Description = request.Description,
            Balance = request.Balance,
            Reserved = request.Reserved
        };
    }
}
