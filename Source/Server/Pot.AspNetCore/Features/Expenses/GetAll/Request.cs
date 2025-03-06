using Pot.AspNetCore.Models;
using Pot.AspNetCore.Utils;
using System.ComponentModel;
using System.Reflection;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

internal sealed class Request : PagedRequest
{
    [Description("The account Id.")]
    public Guid AccountId { get; init; }

    public static ValueTask<Request?> BindAsync(HttpContext context, ParameterInfo _)
    {
        try
        {
            var request = context.CreateFromQueryString<Request>();
            return ValueTask.FromResult<Request?>(request);
        }
        catch
        {
            return ValueTask.FromResult<Request?>(null);
        }
    }
}
