using Pot.AspNetCore.Models;
using Pot.AspNetCore.Utils;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

internal sealed class Request : PagedRequest
{
    public static ValueTask<Request?> BindAsync(HttpContext context/*, ParameterInfo _*/)
    {
        try
        {
            var request = context.CreateFromQueryString<Request>();
            return ValueTask.FromResult<Request?>(request);
        }
        catch
        {
            // Returning null indicates binding failure
            return ValueTask.FromResult<Request?>(null);
        }
    }
}
