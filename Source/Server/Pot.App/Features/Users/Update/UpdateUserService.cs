using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Users.Update.EntityChecks;
using Pot.App.Features.Users.Update.Mappings;
using Pot.App.Features.Users.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Users.Update;

internal sealed class UpdateUserService : IUpdateUserService
{
    private readonly IPersistableUserRepository _userRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly ILogger _logger;

    public UpdateUserService(IPersistableUserRepository userRepository, IPreUpdateChecker preUpdateChecker,
        ILogger<UpdateUserService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> UpdateUserAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_userRepository.WithTracking())
        {
            var userId = input.RowId;

            var userToUpdate = await _userRepository.Users
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

            UpdateUserEntity(userToUpdate, input);

            // Not calling _userRepository.Update(account) as this will mark the
            // entity as modified even if nothing was changed.
            _ = await _userRepository.SaveAsync(cancellationToken);

            var output = userToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }

    private static void UpdateUserEntity(UserEntity userToUpdate, Input input)
    {
        userToUpdate.DisplayName = input.DisplayName;
        userToUpdate.Email = input.Email;
    }
}
