using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Users.GetAll;

namespace Pot.AspNetCore.Features.Users.GetAll;

internal sealed class Handler
{
    // Note: Gets all users for the same site as the caller
    public static async Task<Ok<Response[]>> Invoke(IGetAllUsersService roleService, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var roles = await roleService.GetAllUsersAsync(cancellationToken);

        return Response.Ok(roles);
    }
}
