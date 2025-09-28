using System.ComponentModel;
using System.Reflection;

namespace Pot.AspNetCore.Features.Accruals.Status;

public sealed class Request
{
    [Description("The Account Ids")]
    public Guid[] RowIds { get; init; } = [];

    // Binds to query string parameters such as ?RowIds=val1,val2,val3
    // Whitespace around the commas is stripped
    public static ValueTask<Request?> BindAsync(HttpContext context, ParameterInfo _)
    {
        var rowids = context.Request.Query
            .Where(item => item.Key.Equals(nameof(RowIds), StringComparison.CurrentCultureIgnoreCase))
            .Select(item => item.Value[0])
            .SingleOrDefault()
            ?.Split(",");

        if (rowids is null)
        {
            // Returning null indicates binding failure
            return ValueTask.FromResult<Request?>(null);
        }

        try
        {
            var request = new Request
            {
                RowIds = [.. rowids.Select(rowid => Guid.Parse(rowid.Trim()))]
            };

            return ValueTask.FromResult<Request?>(request);
        }
        catch
        {
            return ValueTask.FromResult<Request?>(null);
        }
    }
}