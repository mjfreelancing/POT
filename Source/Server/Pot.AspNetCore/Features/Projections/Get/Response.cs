using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Projections.Get;

internal sealed class Response : ResponseBase
{
    public static Ok<Response> Ok()
    {
        return TypedResults.Ok(new Response());
    }
}
