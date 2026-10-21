using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.App.Features.Auth.PasswordReset.Request;
using Pot.App.Features.Auth.PasswordReset.Request.Models;
using Pot.App.Features.Otp;
using Pot.App.Features.Otp.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Auth.PasswordReset.Request;

public class RequestPasswordResetServiceFixture : PotFixtureBase
{
    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();
    private readonly IOtpService _otpServiceFake = Substitute.For<IOtpService>();
    private readonly ISendEmailChannelWriter _emailWriterFake = Substitute.For<ISendEmailChannelWriter>();
    private readonly ITimeProvider _timeProviderFake = Substitute.For<ITimeProvider>();

    public class RequestResetAsync : RequestPasswordResetServiceFixture
    {
        [Fact]
        public async Task Should_Return_The_Reference_Code_Of_The_Email()
        {
            ArrangeUser();

            var result = await CreateService().RequestResetAsync(CreateInput(), CancellationToken.None);

            result.ShouldBe(ReferenceCode);
        }

        [Fact]
        public async Task Should_Name_The_Account_And_Its_Site_In_The_Email()
        {
            ArrangeUser();

            await CreateService().RequestResetAsync(CreateInput(), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.ChangePassword,
                Arg.Is<EmailChangePasswordInfo>(info => info.Username == "jsmith" &&
                    info.DisplayName == "Jane Smith" &&
                    info.RecipientName == "Jane Smith" &&
                    info.Email == "jane@example.com" &&
                    info.AccountLabel == "Jane Smith (jsmith)" &&
                    info.SiteName == "Reset Site" &&
                    info.RequestedAtLabel == "Wed, 21 Oct 2026, 2:30 PM" &&
                    info.ReferenceCode == ReferenceCode &&
                    info.VerificationCode == "123456" &&
                    info.TempPassword == "Temp-Password-1" &&
                    info.OtpExpiryMinutes == 15),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Not_Send_An_Email_When_The_Username_Is_Unknown()
        {
            _userRepositoryFake
                .GetByUsernameWithSiteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((UserEntity?)null);

            await CreateService().RequestResetAsync(CreateInput(), CancellationToken.None);

            await _emailWriterFake.DidNotReceive().SubmitAsync(
                Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());
        }

        private void ArrangeUser()
        {
            var site = EntityFactory.CreateSite(name: "Reset Site");
            var user = EntityFactory.CreateUser(site, "jsmith", "jane@example.com", "Jane Smith");

            _userRepositoryFake
                .GetByUsernameWithSiteAsync("jsmith", Arg.Any<CancellationToken>())
                .Returns(user);

            var otpData = new UserOtpData
            {
                TempPassword = "Temp-Password-1",
                TempPasswordHash = "hash",
                ReferenceCode = ReferenceCode,
                OtpCode = "123456",
                OtpExpiryMinutes = 15
            };

            _otpServiceFake
                .AddOtpDataForUserAsync(OtpReason.PasswordReset, user, Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(otpData);

            _timeProviderFake.GetLocalDateTimeNow().Returns(RequestedAt);
        }

        private static Input CreateInput()
        {
            return new Input
            {
                Username = "jsmith",
                CorrelationId = "correlation-id"
            };
        }
    }

    private const string ReferenceCode = "REF123";

    private static readonly DateTime RequestedAt = new(2026, 10, 21, 14, 30, 0, DateTimeKind.Local);

    private RequestPasswordResetService CreateService()
    {
        return new RequestPasswordResetService(
            _userRepositoryFake,
            _otpServiceFake,
            _emailWriterFake,
            _timeProviderFake,
            Substitute.For<ILogger<RequestPasswordResetService>>());
    }
}
