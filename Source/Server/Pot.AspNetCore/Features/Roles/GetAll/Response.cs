using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Roles.GetAll.Models;
using Pot.AspNetCore.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Roles.GetAll;

internal sealed class Response : ResponseBase
{
    [Description("The role Name")]
    public string Name { get; init; }

    public static Ok<Response[]> Ok(List<Output> roles)
    {
        var responses = roles.SelectToArray(role => new Response(role));

        return TypedResults.Ok(responses);
    }

    private Response(Output role)
    {
        _ = role.WhenNotNull();

        RowId = role.RowId;
        Etag = role.Etag;
        Name = role.Name;
    }
}
