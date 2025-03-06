using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Import;

internal sealed class Response
{
    [Description("The number of accounts in the CSV file that were created.")]
    public int Imported { get; init; }

    [Description("The number of accounts in the CSV file that were updated.")]
    public int Updated { get; init; }

    [Description("The total number of accounts read from the CSV file.")]
    public int Total { get; init; }

    public static Ok<Response> Ok(ImportSummary summary)
    {
        return TypedResults.Ok(new Response(summary));
    }

    private Response(ImportSummary summary)
    {
        _ = summary.WhenNotNull();

        Imported = summary.Imported;
        Updated = summary.Updated;
        Total = summary.Total;
    }
}
