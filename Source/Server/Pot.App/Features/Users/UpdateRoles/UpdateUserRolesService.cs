using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Users.UpdateRoles.EntityChecks;
using Pot.App.Features.Users.UpdateRoles.Mappings;
using Pot.App.Features.Users.UpdateRoles.Models;
using Pot.Data;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Users.UpdateRoles;

internal sealed class UpdateUserRolesService : IUpdateUserRolesService
{
    private readonly IPersistableUserRepository _userRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly IPotTransactionFactory _transactionFactory;
    private readonly ILogger _logger;

    public UpdateUserRolesService(IPersistableUserRepository userRepository, IPreUpdateChecker preUpdateChecker,
        IPotTransactionFactory transactionFactory, ILogger<UpdateUserRolesService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _transactionFactory = transactionFactory.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> UpdateUserRolesAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // The user must be tracked to update its etag and roles
        using (_userRepository.WithTracking())
        {
            using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);

            var userId = input.RowId;

            var userToUpdate = await _userRepository.Users
                .Include(user => user.Roles)
                .SingleOrDefaultAsync(user => user.RowId == input.RowId, cancellationToken)
                .ConfigureAwait(false);

            if (userToUpdate is null)
            {
                var userNotFoundDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(userId, "The user does not exist.");

                _logger.LogError(userNotFoundDetails);

                return EnrichedResult.Fail<Output>(userNotFoundDetails);
            }

            var problemDetails = await _preUpdateChecker
                .CanSaveAsync(input, userToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (problemDetails is not null)
            {
                _logger.LogError(problemDetails);

                return EnrichedResult.Fail<Output>(problemDetails);
            }

            // The user is already tracked, so this will not hit the database
            var userEntry = _userRepository.GetEntry(userToUpdate);
            userEntry.State = EntityState.Modified;    // force the user etag to be updated

            // This performs the role updates (save)
            await _userRepository
                .UpdateUserRolesAsync(userToUpdate, input.RoleIds, cancellationToken)
                .ConfigureAwait(false);

            _ = await _userRepository.SaveAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            var output = userToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }
}
