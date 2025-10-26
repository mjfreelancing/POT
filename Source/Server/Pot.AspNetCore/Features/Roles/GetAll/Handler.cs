using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Roles.GetAll;

namespace Pot.AspNetCore.Features.Roles.GetAll;

internal sealed class Handler
{
    public static async Task<Ok<Response[]>> Invoke(IGetAllRolesService roleService, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var roles = await roleService.GetAllRolesAsync(cancellationToken);

        return Response.Ok(roles);
    }
}
