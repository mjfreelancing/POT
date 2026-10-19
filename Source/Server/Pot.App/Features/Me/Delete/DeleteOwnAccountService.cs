using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Me.Delete.EntityChecks;
using Pot.App.Features.Me.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.EmailSender;
using Pot.RazorComponents.Models;

namespace Pot.App.Features.Me.Delete;

/// <summary>
/// Default implementation of <see cref="IDeleteOwnAccountService"/>.
/// </summary>
internal sealed class DeleteOwnAccountService : IDeleteOwnAccountService
{
    private const string SelfDeletionLabel = "themselves";

    private readonly IPersistableUserRepository _userRepository;
    private readonly IAccountDeleteChecker _deleteChecker;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ILogger _logger;

    public DeleteOwnAccountService(IPersistableUserRepository userRepository, IAccountDeleteChecker deleteChecker,
        ISendEmailChannelWriter sendEmailChannelWriter, ILogger<DeleteOwnAccountService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _deleteChecker = deleteChecker.WhenNotNull();
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<bool>> DeleteAccountAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var caller = _userRepository.GetCurrentUser(includeSite: true);

        var apiError = await _deleteChecker.CanDeleteAsync(input, caller, cancellationToken);

        if (apiError is not null)
        {
            _logger.LogApiError(apiError);

            return EnrichedResult.Fail<bool>(apiError);
        }

        // The recipients are captured before the sweep: the caller is one of the site's users, and after the
        // deletion the remaining administrators are the audience.
        var remainingAdministrators = await _userRepository.GetEnabledAdminsForCurrentSiteAsync(cancellationToken);

        var counts = await _userRepository.DeleteUserWithDependentsAsync(caller.RowId, cancellationToken);

        if (counts is null)
        {
            var notFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(caller.RowId, "The user does not exist");

            _logger.LogApiError(notFoundError);

            return EnrichedResult.Fail<bool>(notFoundError);
        }

        _logger.LogInformation(
            "User {CallerRowId} deleted their own account in site {SiteRowId}. Sessions removed: {SessionCount}. One-time codes removed: {OneTimePasswordCount}.",
            caller.RowId,
            caller.Site.RowId,
            counts.Sessions,
            counts.OneTimePasswords);

        // Notifications are best effort and are sent only once the deletion has committed.
        await SendNotificationsAsync(caller, remainingAdministrators, cancellationToken);

        return EnrichedResult.Success(true);
    }

    private async Task SendNotificationsAsync(UserEntity caller, List<GetAllUserInfo> administrators,
        CancellationToken cancellationToken)
    {
        try
        {
            var recipients = new List<(string Username, string Email)>
            {
                (caller.Username, caller.Email)
            };

            recipients.AddRange(administrators
                .Where(administrator => administrator.RowId != caller.RowId)
                .Select(administrator => (administrator.Username, administrator.Email)));

            foreach (var (Username, Email) in recipients)
            {
                var deletedInfo = new EmailUserDeletedInfo
                {
                    Username = Username,
                    Email = Email,
                    DeletedUsername = caller.Username,
                    DeletedByLabel = SelfDeletionLabel,
                    SiteName = caller.Site.Name
                };

                await _sendEmailChannelWriter.SubmitAsync(EmailType.UserDeleted, deletedInfo, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to submit notifications for the deletion of user {CallerRowId}.", caller.RowId);
        }
    }
}
