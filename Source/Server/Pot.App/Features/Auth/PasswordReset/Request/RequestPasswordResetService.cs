using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Auth.PasswordReset.Request.Models;
using Pot.App.Features.Otp;
using Pot.App.Features.Otp.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Auth.PasswordReset.Request;

internal sealed class RequestPasswordResetService : IRequestPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ILogger _logger;

    public RequestPasswordResetService(IUserRepository userRepository, IOtpService otpService,
        ISendEmailChannelWriter sendEmailChannelWriter, ILogger<RequestPasswordResetService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _otpService = otpService.WhenNotNull();
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<string> RequestResetAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { input.Username, input.CorrelationId });

        using (_userRepository.WithTracking())
        {
            // Tracking is required otherwise the user cannot be attached to the OneTimePasswordEntity when
            // it is created since EF will assume it is a new entity rather than an existing one.
            var user = await _userRepository
                .GetByUsernameOrDefaultAsync(input.Username, cancellationToken)
                .ConfigureAwait(false);

            if (user?.Email is null)
            {
                _logger.LogInformation("No user found for username '{Username}' or no associated email", input.Username);
                return OtpGenerator.Create();   // We don't know this user - return a random 'reference code' in case it is a bad actor
            }

            // This will also invalidate any previous OTPs for this user and reason
            var otpData = await _otpService
                .AddOtpDataForUserAsync(OtpReason.PasswordReset, user, input.CorrelationId, cancellationToken)
                .ConfigureAwait(false);

            await SendChangePasswordEmail(user, otpData, cancellationToken).ConfigureAwait(false);

            // Always returning a success result even if there's an error - we don't want to give
            // potential attackers any information about the validity of the username or other issue.
            return otpData.ReferenceCode;
        }
    }

    private ValueTask SendChangePasswordEmail(UserEntity user, UserOtpData otpData, CancellationToken cancellationToken)
    {
        var emailConfig = new EmailOtpInfo
        {
            Username = user.Username,
            Email = user.Email,
            ReferenceCode = otpData.ReferenceCode,
            VerificationCode = otpData.OtpCode!,
            TempPassword = otpData.TempPassword!,
            OtpExpiryMinutes = otpData.OtpExpiryMinutes
        };

        return _sendEmailChannelWriter.SubmitAsync(EmailType.ChangePassword, emailConfig, cancellationToken);
    }
}
