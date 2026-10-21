using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.App.Features.Auth.Signup.Complete;
using Pot.App.Features.Auth.Signup.Complete.Models;
using Pot.App.Features.Otp;
using Pot.Data.Entities;
using Pot.Data.Repositories.Otp;
using Pot.Data.Repositories.Roles;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Auth.Signup.Complete;

public class VerifySignupServiceFixture : PotFixtureBase
{
    private const string Username = "signup-user";
    private const string RequesterEmail = "signup-user@example.com";
    private const string ReferenceCode = "389965";
    private const string VerificationCode = "204446";

    private readonly IPersistableOtpRepository _otpRepositoryFake = Substitute.For<IPersistableOtpRepository>();
    private readonly IPersistableUserRepository _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
    private readonly IRoleRepository _roleRepositoryFake = Substitute.For<IRoleRepository>();
    private readonly IOtpService _otpServiceFake = Substitute.For<IOtpService>();
    private readonly ISendEmailChannelWriter _sendEmailChannelWriterFake = Substitute.For<ISendEmailChannelWriter>();
    private readonly ITimeProvider _timeProviderFake = Substitute.For<ITimeProvider>();
    private readonly ILogger<VerifySignupService> _loggerFake = Substitute.For<ILogger<VerifySignupService>>();

    public class VerifySignupAsync : VerifySignupServiceFixture
    {
        [Fact]
        public async Task Should_Warn_And_Notify_Nobody_When_No_Platform_Administrator_Is_Configured()
        {
            ArrangeAMatchingSignup();
            ArrangeConfiguredAdministrators();

            var result = await CreateService().VerifySignupAsync(CreateInput([]), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            await _sendEmailChannelWriterFake
                .DidNotReceive()
                .SubmitAsync(EmailType.PendingApproval, Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());

            GetWarnings().ShouldContainSingle(
                message => message.Contains("No platform administrators are configured", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Should_Warn_With_The_Unresolved_Ids_When_A_Configured_Administrator_Does_Not_Exist()
        {
            ArrangeAMatchingSignup();

            var existingAdministrator = EntityFactory.CreateUser(EntityFactory.CreateSite(), "existing-admin");
            var staleRowId = Guid.NewGuid();

            ArrangeConfiguredAdministrators(existingAdministrator);

            var result = await CreateService().VerifySignupAsync(
                CreateInput([existingAdministrator.RowId, staleRowId]),
                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            await _sendEmailChannelWriterFake
                .Received(1)
                .SubmitAsync(EmailType.PendingApproval, Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());

            var warning = GetWarnings().ShouldContainSingle(
                message => message.Contains("do not resolve to a user", StringComparison.OrdinalIgnoreCase));

            // The id that did not resolve has to be named, or the warning cannot be acted on.
            warning.ShouldContain(staleRowId.ToString(), Case.Insensitive);
            warning.ShouldNotContain(existingAdministrator.RowId.ToString(), Case.Insensitive);
        }

        [Fact]
        public async Task Should_Notify_Every_Configured_Administrator_Without_Warning_When_They_All_Resolve()
        {
            ArrangeAMatchingSignup();

            var firstAdministrator = EntityFactory.CreateUser(EntityFactory.CreateSite(), "first-admin");
            var secondAdministrator = EntityFactory.CreateUser(EntityFactory.CreateSite(), "second-admin");

            ArrangeConfiguredAdministrators(firstAdministrator, secondAdministrator);

            var result = await CreateService().VerifySignupAsync(
                CreateInput([firstAdministrator.RowId, secondAdministrator.RowId]),
                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            await _sendEmailChannelWriterFake
                .Received(2)
                .SubmitAsync(EmailType.PendingApproval, Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());

            GetWarnings().ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_Address_Each_Administrator_By_Username_And_Display_Name()
        {
            ArrangeAMatchingSignup();

            var administrator = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin", "admin@example.com", "Pat Admin");

            ArrangeConfiguredAdministrators(administrator);

            await CreateService().VerifySignupAsync(CreateInput([administrator.RowId]), CancellationToken.None);

            await _sendEmailChannelWriterFake
                .Received(1)
                .SubmitAsync(
                    EmailType.PendingApproval,
                    Arg.Is<EmailPendingApprovalInfo>(info => info.Username == "platform-admin" &&
                        info.DisplayName == "Pat Admin" &&
                        info.RecipientName == "Pat Admin" &&
                        info.UserUsername == Username),
                    Arg.Any<CancellationToken>());
        }
    }

    /// <summary>
    /// Makes the verification code match, with the username free at that point and the requester resolvable
    /// afterwards, so the notification is reached.
    /// </summary>
    private void ArrangeAMatchingSignup()
    {
        var otp = new OneTimePasswordEntity
        {
            RowId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            Username = Username,
            Email = RequesterEmail,
            Reason = OtpReason.Signup,
            RefCode = ReferenceCode,
            OtpCode = VerificationCode,
            Status = OtpStatus.Active,
            TempPasswordHash = "hash",
            CreatedUtc = DateTime.UtcNow,
            ExpiryUtc = DateTime.UtcNow.AddMinutes(15)
        };

        _otpRepositoryFake
            .GetRequestsForUsernameAndRefCodeAsync(OtpReason.Signup, Username, ReferenceCode, Arg.Any<CancellationToken>())
            .Returns([otp]);

        _roleRepositoryFake
            .GetByNameAsync(Role.Admin, Arg.Any<CancellationToken>())
            .Returns(new RoleEntity
            {
                RowId = Guid.NewGuid(),
                Name = Role.Admin
            });

        // Resolved twice: the first lookup proves the username is free, the second resolves the requester for the
        // notification.
        _userRepositoryFake
            .GetByUsernameAsync(Username, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null, EntityFactory.CreateUser(EntityFactory.CreateSite(), Username, RequesterEmail, "Signup User"));
    }

    private void ArrangeConfiguredAdministrators(params UserEntity[] administrators)
    {
        _userRepositoryFake
            .GetByIdsForPlatformNotificationAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([.. administrators]);
    }

    private VerifySignupService CreateService()
    {
        return new VerifySignupService(
            _otpRepositoryFake,
            _userRepositoryFake,
            _roleRepositoryFake,
            _otpServiceFake,
            _sendEmailChannelWriterFake,
            _timeProviderFake,
            _loggerFake);
    }

    private List<string> GetWarnings()
    {
        var warnings = new List<string>();

        foreach (var call in _loggerFake.ReceivedCalls())
        {
            var arguments = call.GetArguments();

            if (arguments.Length == 5 && arguments[0] is LogLevel level && level == LogLevel.Warning)
            {
                warnings.Add(arguments[2]?.ToString() ?? string.Empty);
            }
        }

        return warnings;
    }

    private static Input CreateInput(Guid[] platformAdminRowIds)
    {
        return new Input
        {
            Username = Username,
            ReferenceCode = ReferenceCode,
            VerificationCode = VerificationCode,
            PlatformAdminRowIds = platformAdminRowIds
        };
    }
}
