using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Settings.Upsert.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Settings.Upsert;

internal sealed class Response : ResponseBase
{
    public static Ok<Response> Ok(Output setting)
    {
        var response = new Response(setting);

        return TypedResults.Ok(response);
    }

    private Response(Output setting)
    {
        RowId = setting.RowId;
        Etag = setting.Etag;
    }
}
