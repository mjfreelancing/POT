using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Repositories.Roles;

namespace Pot.App.Features.Users.Invite.EntityChecks.Checks;

internal sealed class CheckValidRoles : PreUpdateCheckBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger _logger;

    public CheckValidRoles(IRoleRepository roleRepository, ILogger<CheckValidRoles> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger.WhenNotNull();
    }

    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        if (state.RoleIds.Length > 0)
        {
            var foundRoles = await _roleRepository.Roles
                .Where(role => state.RoleIds.Contains(role.RowId))
                .Select(role => role.RowId)
                .ToListAsync(cancellationToken);

            var missingRoles = state.RoleIds.Except(foundRoles).ToList();

            if (missingRoles.Count > 0)
            {
                var roleIdsText = string.Join(", ", missingRoles);
                return ApiDetailErrorFactory.CreateUnprocessableEntityError(nameof(InputState.RoleIds), roleIdsText, "One or more roles were not found");
            }
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

