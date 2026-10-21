using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Users.Delete.EntityChecks;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;

namespace Pot.App.Features.Users.Delete;

/// <summary>
/// Default implementation of <see cref="IDeleteUserService"/>.
/// </summary>
internal sealed class DeleteUserService : IDeleteUserService
{
    private readonly IPersistableUserRepository _userRepository;
    private readonly IUserDeleteChecker _deleteChecker;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public DeleteUserService(IPersistableUserRepository userRepository, IUserDeleteChecker deleteChecker,
        ISendEmailChannelWriter sendEmailChannelWriter, ITimeProvider timeProvider, ILogger<DeleteUserService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _deleteChecker = deleteChecker.WhenNotNull();
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<bool>> DeleteUserAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var caller = _userRepository.GetCurrentUser(includeSite: true);

        // The site-scoped lookup reports a foreign row exactly as a missing one, so the caller cannot tell the
        // two apart.
        var target = await _userRepository.GetForCurrentSiteAsync(input.RowId, cancellationToken);

        if (target is null)
        {
            var notFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(input.RowId, "The user does not exist");

            _logger.LogApiError(notFoundError);

            return EnrichedResult.Fail<bool>(notFoundError);
        }

        var apiError = await _deleteChecker.CanDeleteAsync(input, caller, target, cancellationToken);

        if (apiError is not null)
        {
            _logger.LogApiError(apiError);

            return EnrichedResult.Fail<bool>(apiError);
        }

        var counts = await _userRepository.DeleteUserWithDependentsAsync(input.RowId, cancellationToken);

        if (counts is null)
        {
            // Another caller removed the same user between the lookup and the sweep.
            var notFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(input.RowId, "The user does not exist");

            _logger.LogApiError(notFoundError);

            return EnrichedResult.Fail<bool>(notFoundError);
        }

        _logger.LogInformation(
            "User {TargetRowId} of site {SiteRowId} deleted by {CallerRowId}. Sessions removed: {SessionCount}. One-time codes removed: {OneTimePasswordCount}.",
            target.RowId,
            caller.Site.RowId,
            caller.RowId,
            counts.Sessions,
            counts.OneTimePasswords);

        // Notifications are best effort and are sent only once the deletion has committed, so the recipients are
        // never told about a removal that did not happen.
        await SendNotificationsAsync(caller, target, cancellationToken);

        return EnrichedResult.Success(true);
    }

    private async Task SendNotificationsAsync(UserEntity caller, UserEntity target, CancellationToken cancellationToken)
    {
        try
        {
            var deletedAt = _timeProvider.GetLocalDateTimeNow();

            var administrators = await _userRepository.GetEnabledAdminsForCurrentSiteAsync(cancellationToken);

            // One confirmation per recipient: the deleted user, and every enabled administrator of the site
            // (including the caller, who is one of them).
            var recipients = new List<(string Username, string DisplayName, string Email)>
            {
                (target.Username, target.DisplayName, target.Email)
            };

            recipients.AddRange(administrators.Select(administrator =>
                (administrator.Username, administrator.DisplayName, administrator.Email)));

            foreach (var (Username, DisplayName, Email) in recipients)
            {
                var deletedInfo = new EmailUserDeletedInfo
                {
                    Username = Username,
                    DisplayName = DisplayName,
                    Email = Email,
                    DeletedUserDisplayName = target.DisplayName,
                    DeletedUsername = target.Username,
                    DeletedByDisplayName = caller.DisplayName,
                    DeletedByUsername = caller.Username,
                    SiteName = caller.Site.Name,
                    DeletedAt = deletedAt
                };

                await _sendEmailChannelWriter.SubmitAsync(EmailType.UserDeleted, deletedInfo, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            // A notification failure must not turn a committed deletion into an error response.
            _logger.LogError(exception, "Failed to submit notifications for the deletion of user {TargetRowId}.", target.RowId);
        }
    }
}
