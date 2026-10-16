using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accruals.Status;

/// <summary>Carries the values needed to query the status of an accrual.</summary>
public sealed class Request
{
    /// <summary>The Account Ids</summary>
    [Description("The Account Ids")]
    public IReadOnlyList<Guid> AccountRowIds { get; init; } = [];

    // Binds to query string parameters such as ?AccountRowIds=val1,val2,val3
    // Whitespace around the commas is stripped
    /// <summary>Binds a request model from the current HTTP context.</summary>
    public static ValueTask<Request?> BindAsync(HttpContext context/*, ParameterInfo _*/)
    {
        var rowids = context.Request.Query
            .Where(item => item.Key.Equals(nameof(AccountRowIds), StringComparison.CurrentCultureIgnoreCase))
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
                AccountRowIds = [.. rowids.Select(rowid => Guid.Parse(rowid.Trim()))]
            };

            return ValueTask.FromResult<Request?>(request);
        }
        catch
        {
            return ValueTask.FromResult<Request?>(null);
        }
    }
}