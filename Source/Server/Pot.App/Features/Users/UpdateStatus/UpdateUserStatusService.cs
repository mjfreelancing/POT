using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Users.UpdateStatus.EntityChecks;
using Pot.App.Features.Users.UpdateStatus.Mappings;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Repositories.Users;

namespace Pot.App.Features.Users.UpdateStatus;

internal sealed class UpdateUserStatusService : IUpdateUserStatusService
{
    private readonly IPersistableUserRepository _userRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly ILogger _logger;

    public UpdateUserStatusService(IPersistableUserRepository userRepository, IPreUpdateChecker preUpdateChecker,
        ILogger<UpdateUserStatusService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> UpdateUserStatusAsync(Input input, CancellationToken cancellationToken)
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
                var userNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(userId, "The user does not exist");

                _logger.LogApiError(userNotFoundError);

                return EnrichedResult.Fail<Output>(userNotFoundError);
            }

            var apiError = await _preUpdateChecker
                .CanSaveAsync(input, userToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (apiError is not null)
            {
                _logger.LogApiError(apiError);

                return EnrichedResult.Fail<Output>(apiError);
            }

            userToUpdate.Status = input.Status;

            _ = await _userRepository.SaveAsync(cancellationToken);

            var output = userToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }
}
