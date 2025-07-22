using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Maintenance.Import;

internal sealed class Response
{
    // TODO: ? break this down to the number of accounts, incomes and expenses ?
    [Description("The number of records in the ZIP file that were imported.")]
    public int Imported { get; init; }

    public static Ok<Response> Ok(int imported)
    {
        return TypedResults.Ok(new Response { Imported = imported });
    }

    //public static Ok<Response> Ok(ImportSummary summary)
    //{
    //    return TypedResults.Ok(new Response(summary));
    //}

    //private Response(ImportSummary summary)
    //{
    //    _ = summary.WhenNotNull();

    //    Imported = summary.Imported;
    //    Updated = summary.Updated;
    //    Total = summary.Total;
    //}
}
