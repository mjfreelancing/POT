using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Auth.Signup.Request.Models;
using Pot.App.Features.Otp;
using Pot.App.Features.Otp.Models;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Auth.Signup.Request;

internal sealed class RequestSignupService : IRequestSignupService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ILogger _logger;

    public RequestSignupService(IUserRepository userRepository, IOtpService otpService,
        ISendEmailChannelWriter sendEmailChannelWriter, ILogger<RequestSignupService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _otpService = otpService.WhenNotNull();
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> RequestSignupAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { input.Username, input.CorrelationId });

        // Check if the username is unique - will deal with the unique site name when signup is completed.
        // There's also a slim chance that the username could be taken between this check and the signup
        // completion that will need to be dealt with (user will have to start again).
        var user = await _userRepository
            .GetByUsernameOrDefaultAsync(input.Username, cancellationToken)
            .ConfigureAwait(false);

        if (user is not null)
        {
            _logger.LogInformation("The username '{Username}' is already taken", input.Username);

            var usernameTakenOutput = new Output
            {
                Status = OutputStatus.UsernameTaken,
                Message = "Username is already taken",
                ReferenceCode = null
            };

            return EnrichedResult.Success(usernameTakenOutput);
        }

        using (_userRepository.WithTracking())
        {
            // This will also invalidate any previous OTPs for this user and reason
            var otpData = await _otpService
                .AddOtpDataForUserAsync(OtpReason.Signup, input.Username, input.Email, input.CorrelationId, cancellationToken)
                .ConfigureAwait(false);

            await SendSignupEmail(input, otpData, cancellationToken).ConfigureAwait(false);

            var output = new Output
            {
                Status = OutputStatus.Success,
                Message = "Success",
                ReferenceCode = otpData.ReferenceCode
            };

            return EnrichedResult.Success(output);
        }
    }

    private ValueTask SendSignupEmail(Input input, UserOtpData otpData, CancellationToken cancellationToken)
    {
        var emailConfig = new EmailOtpInfo
        {
            Username = input.Username,
            Email = input.Email,
            ReferenceCode = otpData.ReferenceCode,
            VerificationCode = otpData.OtpCode!,
            TempPassword = otpData.TempPassword!,
            OtpExpiryMinutes = otpData.OtpExpiryMinutes
        };

        return _sendEmailChannelWriter.SubmitAsync(EmailType.Signup, emailConfig, cancellationToken);
    }
}
