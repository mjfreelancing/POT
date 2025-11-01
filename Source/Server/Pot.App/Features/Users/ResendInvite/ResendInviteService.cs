using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Auth;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Auth;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;

namespace Pot.App.Features.Users.ResendInvite;

internal sealed class ResendInviteService : IResendInviteService
{
    private const int InvitationPasswordLength = PasswordGenerator.DefaultLength;

    private readonly IPersistableUserRepository _userRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ILogger _logger;

    public ResendInviteService(IPersistableUserRepository userRepository, IUserPasswordHasher passwordHasher,
        ISendEmailChannelWriter sendEmailChannelWriter, ILogger<ResendInviteService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _passwordHasher = passwordHasher.WhenNotNull();
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> ResendInviteAsync(Guid userRowId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { userRowId });

        using (_userRepository.WithTracking())
        {
            // check if the username is already taken
            var user = await _userRepository.Users
                .SingleOrDefaultAsync(user => user.RowId == userRowId, cancellationToken)
                .ConfigureAwait(false);

            if (user is null)
            {
                var userProblemDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(userRowId, "User not found");

                _logger.LogError(userProblemDetails);

                return EnrichedResult.Fail<bool>(userProblemDetails);
            }

            // Need to reset the password because we don't know the previous one (as it was sent via email)
            var tempPassword = PasswordGenerator.Create(InvitationPasswordLength);
            var tempPasswordHash = _passwordHasher.GetHash(user, tempPassword);

            user.PasswordHash = tempPasswordHash;

            await SendInvitationEmail(user, tempPassword, cancellationToken).ConfigureAwait(false);

            await _userRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            return EnrichedResult.Success(true);
        }
    }

    private ValueTask SendInvitationEmail(UserEntity user, string tempPassword, CancellationToken cancellationToken)
    {
        var emailConfig = new EmailInvitationInfo
        {
            Username = user.Username,
            Email = user.Email,
            TempPassword = tempPassword!,
        };

        return _sendEmailChannelWriter.SubmitAsync(EmailType.Invitation, emailConfig, cancellationToken);
    }
}
