using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.App.Extensions;
using Pot.App.Features.Sites.Delete.EntityChecks;
using Pot.App.Features.Sites.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Sites;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.EmailSender;
using Pot.RazorComponents.Models;

namespace Pot.App.Features.Sites.Delete;

/// <summary>
/// Default implementation of <see cref="IDeleteSiteService"/>.
/// </summary>
internal sealed class DeleteSiteService : IDeleteSiteService
{
    private readonly IPersistableSiteRepository _siteRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISiteDeleteChecker _deleteChecker;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public DeleteSiteService(IPersistableSiteRepository siteRepository, IUserRepository userRepository,
        ISiteDeleteChecker deleteChecker, ISendEmailChannelWriter sendEmailChannelWriter, ITimeProvider timeProvider,
        ILogger<DeleteSiteService> logger)
    {
        _siteRepository = siteRepository.WhenNotNull();
        _userRepository = userRepository.WhenNotNull();
        _deleteChecker = deleteChecker.WhenNotNull();
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<bool>> DeleteSiteAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var caller = _userRepository.GetCurrentUser(includeSite: true);
        var site = _siteRepository.GetCurrentSite();

        var apiError = await _deleteChecker.CanDeleteAsync(input, caller, site, cancellationToken);

        if (apiError is not null)
        {
            _logger.LogApiError(apiError);

            return EnrichedResult.Fail<bool>(apiError);
        }

        // The recipients are captured before the sweep: after it, none of the site's rows exist.
        var administrators = await _userRepository.GetEnabledAdminsForCurrentSiteAsync(cancellationToken);

        var counts = await _siteRepository.DeleteCurrentSiteWithDependentsAsync(cancellationToken);

        _logger.LogInformation(
            "Site {SiteRowId} deleted by {CallerRowId}. Users removed: {UserCount}. Accounts removed: {AccountCount}. Expenses removed: {ExpenseCount}. Incomes removed: {IncomeCount}. Settings removed: {SettingCount}. Sessions removed: {SessionCount}. One-time codes removed: {OneTimePasswordCount}.",
            site.RowId,
            caller.RowId,
            counts.Users,
            counts.Accounts,
            counts.Expenses,
            counts.Incomes,
            counts.Settings,
            counts.Sessions,
            counts.OneTimePasswords);

        // Notifications are best effort and are sent only once the deletion has committed.
        await SendNotificationsAsync(caller, site, administrators, cancellationToken);

        return EnrichedResult.Success(true);
    }

    private async Task SendNotificationsAsync(UserEntity caller, SiteEntity site, List<GetAllUserInfo> administrators,
        CancellationToken cancellationToken)
    {
        try
        {
            var deletedAt = _timeProvider.GetLocalDateTimeNow();

            foreach (var administrator in administrators)
            {
                var siteDeletedInfo = new EmailSiteDeletedInfo
                {
                    Username = administrator.Username,
                    DisplayName = administrator.DisplayName,
                    Email = administrator.Email,
                    DeletedSiteName = site.Name,
                    DeletedByDisplayName = caller.DisplayName,
                    DeletedByUsername = caller.Username,
                    DeletedAt = deletedAt
                };

                await _sendEmailChannelWriter.SubmitAsync(EmailType.SiteDeleted, siteDeletedInfo, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            // A notification failure must not turn a committed deletion into an error response.
            _logger.LogError(exception, "Failed to submit notifications for the deletion of site {SiteRowId}.", site.RowId);
        }
    }
}
