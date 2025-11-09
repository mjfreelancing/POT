using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.App.Features.Auth.Signup.Complete.Models;
using Pot.App.Features.Otp;
using Pot.Data.Entities;
using Pot.Data.Repositories.Otp;
using Pot.Data.Repositories.Roles;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Auth.Signup.Complete;

internal sealed class VerifySignupService : VerificationServiceBase, IVerifySignupService
{
    private static readonly EnrichedResult<Output> InvalidOutput = EnrichedResult.Success(new Output
    {
        Status = OutputStatus.Invalid,
        Message = "Invalid. Try again."     // Could be invalid code or username - not giving any hints
    });

    private static readonly EnrichedResult<Output> ExpiredOutput = EnrichedResult.Success(new Output
    {
        Status = OutputStatus.Expired,
        Message = "Expired. Send a new request."
    });

    private static readonly EnrichedResult<Output> TooManyAttemptsOutput = EnrichedResult.Success(new Output
    {
        Status = OutputStatus.TooManyAttempts,
        Message = "Too Many Attempts. Send a new request.",
        RetryMinutes = TooManyAttemptsWaitMinutes
    });

    private static readonly EnrichedResult<Output> UsernameTakenOutput = EnrichedResult.Success(new Output
    {
        Status = OutputStatus.UsernameTaken,
        Message = "Username is taken. Choose another name."
    });

    private static readonly EnrichedResult<Output> SuccessOutput = EnrichedResult.Success(new Output
    {
        Status = OutputStatus.Success,
        Message = "Success"
    });

    private readonly IPersistableOtpRepository _otpRepository;
    private readonly IPersistableUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ISendEmailChannelWriter _sendEmailChannelWriter;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public VerifySignupService(IPersistableOtpRepository otpRepository, IPersistableUserRepository userRepository,
        IRoleRepository roleRepository, IOtpService otpService, ISendEmailChannelWriter sendEmailChannelWriter,
        ITimeProvider timeProvider, ILogger<VerifySignupService> logger)
        : base(OtpReason.Signup, otpService, otpRepository, logger)
    {
        _otpRepository = otpRepository.WhenNotNull();
        _userRepository = userRepository.WhenNotNull();
        _roleRepository = roleRepository;
        _sendEmailChannelWriter = sendEmailChannelWriter.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> VerifySignupAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { input.Username, input.ReferenceCode, input.VerificationCode });

        // _userRepository and _otpRepository share the same DbContext so only need to track / update one of them
        using var otpTracking = _otpRepository.WithTracking();

        var verificationOutput = await ProcessVerificationAsync(
            input.Username,
            input.ReferenceCode,
            input.VerificationCode,
            otp => { },
            cancellationToken).ConfigureAwait(false);

        var output = (EnrichedResult<Output>)verificationOutput;

        if (output.IsSuccess)
        {
            try
            {
                // This could fail if the username has been taken while verifying this signup request
                await _otpRepository
                    .SaveAsync(cancellationToken)
                    .ConfigureAwait(false);

                // There is a chance this could fail
                await SendApprovalsToPlatformAdminsAsync(input.Username, input.PlatformAdminRowIds, cancellationToken).ConfigureAwait(false);
            }
            catch (UniqueConstraintException ex)
            {
                // TODO: Create a constant for "IX_User_Username" - or even an extension method on UniqueConstraintException to check for specific constraint.
                //       Or, even better, create an interceptor that might be able to deal with this explicitly and raise a more specific exception.
                if (string.Compare(ex.ConstraintName, "IX_User_Username", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _logger.LogError(ex, "Failed to create user for username '{Username}' - likely taken since signup request", input.Username);
                    return UsernameTakenOutput;
                }

                if (string.Compare(ex.ConstraintName, "IX_Site_Name", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // Should never happen if the validation on site name uniqueness is implemented correctly
                    _logger.LogError(ex, "Failed to create user for username '{Username}' since the assigned site name '{SiteName}' is not unique",
                        input.Username, GetDefaultSiteNameForUser(input.Username));

                    throw;
                }
            }
        }

        return output;
    }

    protected override EnrichedResult GetInvalidOutput()
    {
        return InvalidOutput;
    }

    protected override EnrichedResult GetExpiredOutput()
    {
        return ExpiredOutput;
    }

    protected override EnrichedResult GetTooManyAttemptsOutput()
    {
        return TooManyAttemptsOutput;
    }

    protected override async Task<EnrichedResult> ProcessVerificationCodeMatchAsync(OneTimePasswordEntity mostRecentOtp,
        CancellationToken cancellationToken)
    {
        // Check the username does not exist
        var user = await _userRepository
            .GetByUsernameOrDefaultAsync(mostRecentOtp.Username, cancellationToken)
            .ConfigureAwait(false);

        if (user is not null)
        {
            _logger.LogInformation("The username '{Username}' has been taken", mostRecentOtp.Username);
            return UsernameTakenOutput;
        }

        var site = new SiteEntity
        {
            Name = GetDefaultSiteNameForUser(mostRecentOtp.Username),
        };

        // Need to make this easier !!!!
        var role = await _roleRepository
            .GetByNameAsync(Role.Admin, cancellationToken)
            .ConfigureAwait(false);

        // Create a new user entity since the username is available - but handle conflict when saved in case it was 'just' taken.
        // Start off as Approval so a platform admin can approve before allowing login
        user = new UserEntity
        {
            Username = mostRecentOtp.Username,
            Email = mostRecentOtp.Email,
            DisplayName = mostRecentOtp.Username,
            Status = UserStatus.Approval,
            PasswordHash = mostRecentOtp.TempPasswordHash!,
            Site = site,
            Roles = [role]
        };

        _userRepository.Add(user);

        mostRecentOtp.Status = OtpStatus.Used;
        mostRecentOtp.VerifiedUtc = _timeProvider.GetUtcDateTimeNow();

        return SuccessOutput;
    }

    private static string GetDefaultSiteNameForUser(string username)
    {
        return $"{username}'s Site";
    }

    private async Task SendApprovalsToPlatformAdminsAsync(string username, Guid[] platformAdminRowIds, CancellationToken cancellationToken)
    {
        var userEmail = await _userRepository.Users
            .Where(user => user.Username == username)
            .Select(user => user.Email)
            .SingleAsync(cancellationToken)
            .ConfigureAwait(false);

        var platformAdmins = await _userRepository.Users
            .Where(user => platformAdminRowIds.Contains(user.RowId))
            .Select(user => new { user.DisplayName, user.Email })
            .ToListAsync(cancellationToken);

        foreach (var platformAdmin in platformAdmins)
        {
            await _sendEmailChannelWriter.SubmitAsync(EmailType.PendingApproval, new EmailPendingApprovalInfo
            {
                // The platform admin is approving this user - use their display name
                Username = platformAdmin.DisplayName,
                Email = platformAdmin.Email,

                UserUsername = username,
                UserEmail = userEmail
            }, cancellationToken);
        }
    }
}
