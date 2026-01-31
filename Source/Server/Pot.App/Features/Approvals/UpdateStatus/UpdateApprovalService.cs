using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Approvals.UpdateStatus.EntityChecks;
using Pot.App.Features.Approvals.UpdateStatus.Mappings;
using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Approvals.UpdateStatus;

internal sealed class UpdateApprovalService : IUpdateApprovalService
{
    private readonly IPersistableUserRepository _userRepository;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly ILogger _logger;

    public UpdateApprovalService(IPersistableUserRepository userRepository, ISendEmailChannelWriter sendEmailChannelWriter,
        IPreUpdateChecker preUpdateChecker, ILogger<UpdateApprovalService> logger)
    {
        _userRepository = userRepository.WhenNotNull(); ;
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> UpdateUserApprovalAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_userRepository.WithTracking())
        {
            var userId = input.RowId;

            var userToUpdate = await _userRepository.Users
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(user => user.RowId == input.RowId, cancellationToken)
                .ConfigureAwait(false);

            if (userToUpdate is null)
            {
                var userNotFoundDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(userId, "The user does not exist");

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

            userToUpdate.Status = input.Status == ApprovalStatus.Approved
                ? UserStatus.Enabled
                : UserStatus.Disabled;

            _ = await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            await SendApprovalStatusEmail(userToUpdate, input.Status, cancellationToken).ConfigureAwait(false);

            var output = userToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }

    private ValueTask SendApprovalStatusEmail(UserEntity user, ApprovalStatus status, CancellationToken cancellationToken)
    {
        var emailConfig = new EmailApprovalStatusInfo
        {
            Username = user.Username,
            Email = user.Email
        };

        return status == ApprovalStatus.Approved
            ? _sendEmailChannelWriter.SubmitAsync(EmailType.ApprovalAccepted, emailConfig, cancellationToken)
            : _sendEmailChannelWriter.SubmitAsync(EmailType.ApprovalRejected, emailConfig, cancellationToken);
    }
}
