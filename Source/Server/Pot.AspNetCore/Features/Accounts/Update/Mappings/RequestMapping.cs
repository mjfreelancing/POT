using Pot.App.Features.Accounts.Update.Models;

namespace Pot.AspNetCore.Features.Accounts.Update.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            RowId = request.RowId,
            Etag = request.Etag,
            Bsb = request.Bsb,
            Number = request.Number,
            Description = request.Description,
            Balance = request.Balance,
            Reserved = request.Reserved
        };
    }
}
