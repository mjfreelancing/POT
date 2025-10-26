using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Roles.GetAll.Mappings;
using Pot.App.Features.Roles.GetAll.Models;
using Pot.Data.Repositories.Roles;

namespace Pot.App.Features.Roles.GetAll;

internal sealed class GetAllRolesService : IGetAllRolesService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger _logger;

    public GetAllRolesService(IRoleRepository roleRepository, ILogger<GetAllRolesService> logger)
    {
        _roleRepository = roleRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<List<Output>> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var roles = await _roleRepository.Roles.ToListAsync(cancellationToken);

        return roles.SelectToList(role => role.MapToOutput());
    }
}
