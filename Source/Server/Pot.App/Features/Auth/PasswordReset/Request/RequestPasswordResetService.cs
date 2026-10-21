using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.App.Features.Auth.PasswordReset.Request.Models;
using Pot.App.Features.Otp;
using Pot.App.Features.Otp.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Auth.PasswordReset.Request;

/// <summary>
/// Default implementation of <see cref="IRequestPasswordResetService"/>.
/// </summary>
internal sealed class RequestPasswordResetService : IRequestPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public RequestPasswordResetService(IUserRepository userRepository, IOtpService otpService,
        ISendEmailChannelWriter sendEmailChannelWriter, ITimeProvider timeProvider, ILogger<RequestPasswordResetService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _otpService = otpService.WhenNotNull();
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<string> RequestResetAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { input.Username, input.CorrelationId });

        using (_userRepository.WithTracking())
        {
            // Tracking is required otherwise the user cannot be attached to the OneTimePasswordEntity when
            // it is created since EF will assume it is a new entity rather than an existing one.
            // Cross-tenant by design: a signed-out caller resolves a username in order to reset a password.
            // The site is loaded so the message can name it: one email address can hold accounts in several sites.
            var user = await _userRepository.GetByUsernameWithSiteAsync(input.Username, cancellationToken);

            if (user?.Email is null)
            {
                _logger.LogInformation("No user found for username '{Username}' or no associated email", input.Username);
                return OtpGenerator.Create();   // We don't know this user - return a random 'reference code' in case it is a bad actor
            }

            // This will also invalidate any previous OTPs for this user and reason
            var otpData = await _otpService.AddOtpDataForUserAsync(OtpReason.PasswordReset, user, input.CorrelationId, cancellationToken);

            await SendChangePasswordEmail(user, otpData, cancellationToken);

            // Always returning a success result even if there's an error - we don't want to give
            // potential attackers any information about the validity of the username or other issue.
            return otpData.ReferenceCode;
        }
    }

    private ValueTask SendChangePasswordEmail(UserEntity user, UserOtpData otpData, CancellationToken cancellationToken)
    {
        var emailConfig = new EmailChangePasswordInfo
        {
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            SiteName = user.Site.Name,
            ReferenceCode = otpData.ReferenceCode,
            VerificationCode = otpData.OtpCode!,
            TempPassword = otpData.TempPassword!,
            OtpExpiryMinutes = otpData.OtpExpiryMinutes,
            RequestedAt = _timeProvider.GetLocalDateTimeNow()
        };

        return _sendEmailChannelWriter.SubmitAsync(EmailType.ChangePassword, emailConfig, cancellationToken);
    }
}
