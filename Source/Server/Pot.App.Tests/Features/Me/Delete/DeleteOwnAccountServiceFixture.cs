using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Features.Me.Delete;
using Pot.App.Features.Me.Delete.EntityChecks;
using Pot.App.Features.Me.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Me.Delete;

public class DeleteOwnAccountServiceFixture : PotFixtureBase
{
    private readonly IPersistableUserRepository _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
    private readonly IAccountDeleteChecker _deleteCheckerFake = Substitute.For<IAccountDeleteChecker>();
    private readonly ISendEmailChannelWriter _emailWriterFake = Substitute.For<ISendEmailChannelWriter>();
    private readonly ITimeProvider _timeProviderFake = Substitute.For<ITimeProvider>();

    public class DeleteAccountAsync : DeleteOwnAccountServiceFixture
    {
        [Fact]
        public async Task Should_Remove_The_Callers_Account_And_Return_Success()
        {
            var caller = ArrangeCaller();

            var result = await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            await _userRepositoryFake.Received(1).DeleteUserWithDependentsAsync(
                caller.RowId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Run_The_Deletion_Rules_Before_Removing_Anything()
        {
            var caller = ArrangeCaller();
            var input = CreateInput(caller);

            await CreateService().DeleteAccountAsync(input, CancellationToken.None);

            await _deleteCheckerFake.Received(1).CanDeleteAsync(
                input, caller, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Return_The_Refusal_Without_Removing_Anything()
        {
            var caller = ArrangeCaller();

            var refusal = ApiDetailErrorFactory.CreateBadRequestError(nameof(Input.Username), caller.RowId, "Refused");

            _deleteCheckerFake
                .CanDeleteAsync(Arg.Any<Input>(), Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
                .Returns(refusal);

            var result = await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBeSameAs(refusal);

            await _userRepositoryFake.DidNotReceive().DeleteUserWithDependentsAsync(
                Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _emailWriterFake.DidNotReceive().SubmitAsync(
                Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Notify_The_Caller_And_The_Remaining_Administrators()
        {
            var caller = ArrangeCaller();

            await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailConfigBase>(config => config.Email == caller.Email),
                Arg.Any<CancellationToken>());

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailConfigBase>(config => config.Email == OtherAdministratorEmail),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Not_Notify_The_Caller_Twice_When_They_Are_Also_An_Administrator()
        {
            var caller = ArrangeCaller(includeCallerAsAdministrator: true);

            await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            // Two recipients in total: the caller and the other administrator.
            await _emailWriterFake.Received(2).SubmitAsync(
                Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Report_The_Deletion_As_Self_Inflicted()
        {
            var caller = ArrangeCaller();

            await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            // Both recipients - the caller and the other administrator - are told the same thing.
            await _emailWriterFake.Received(2).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.DeletedUserLabel == "Casey Caller (caller)" &&
                    info.DeletedByLabel == "Self-deleted" &&
                    info.SiteName == "Self Delete Site"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Address_The_Callers_Copy_To_Them_As_A_Requested_Deletion()
        {
            var caller = ArrangeCaller();

            await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.Email == caller.Email &&
                    info.IsRecipientDeletedUser &&
                    info.Heading == "Your Account Was Deleted" &&
                    info.Introduction == "Your account on the site Self Delete Site has been deleted, as you requested. You can no longer sign in." &&
                    info.ClosingNote == "This is an automated confirmation. If you did not request this, contact your site administrator." &&
                    info.Details.Select(detail => detail.Label).SequenceEqual(new[] { "Site", "Deleted at" })),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Name_The_Caller_In_The_Administrators_Copy()
        {
            var caller = ArrangeCaller();

            await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.Email == OtherAdministratorEmail &&
                    !info.IsRecipientDeletedUser &&
                    info.Heading == "User Deleted" &&
                    info.Introduction == "A user account on your site has been deleted." &&
                    info.ClosingNote == "This is an automated confirmation. No further action is required." &&
                    info.Details.Select(detail => detail.Label).SequenceEqual(new[] { "User", "Site", "Deleted by", "Deleted at" })),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Address_Each_Recipient_By_Their_Display_Name()
        {
            var caller = ArrangeCaller();

            await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.Email == caller.Email &&
                    info.DisplayName == caller.DisplayName &&
                    info.RecipientName == caller.DisplayName),
                Arg.Any<CancellationToken>());

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.Email == OtherAdministratorEmail &&
                    info.DisplayName == OtherAdministratorDisplayName &&
                    info.RecipientName == OtherAdministratorDisplayName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Report_When_The_Account_Was_Deleted()
        {
            var caller = ArrangeCaller();

            await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            await _emailWriterFake.Received(2).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.DeletedAt == DeletedAt &&
                    info.DeletedAtLabel == "Wed, 21 Oct 2026, 2:30 PM"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Still_Succeed_When_A_Notification_Cannot_Be_Submitted()
        {
            var caller = ArrangeCaller();

            // NSubstitute's fluent Returns consumes the substitute's ValueTask once; CA2012 cannot see through the extension.
#pragma warning disable CA2012
            _emailWriterFake
                .SubmitAsync(Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask>(_ => throw new InvalidOperationException("The email channel is unavailable"));
#pragma warning restore CA2012

            var result = await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Return_Not_Found_When_The_Caller_Is_Removed_Before_The_Sweep()
        {
            var caller = ArrangeCaller();

            _userRepositoryFake
                .DeleteUserWithDependentsAsync(caller.RowId, Arg.Any<CancellationToken>())
                .Returns((UserDeletionCounts?)null);

            var result = await CreateService().DeleteAccountAsync(CreateInput(caller), CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            (result.Error as ApiErrorBase)!.ErrorType.ShouldBe(ErrorType.NotFound);
        }

        private UserEntity ArrangeCaller(bool includeCallerAsAdministrator = false)
        {
            var site = EntityFactory.CreateSite(name: "Self Delete Site");
            var caller = EntityFactory.CreateUser(site, "caller", "caller@example.com", "Casey Caller");

            _userRepositoryFake.GetCurrentUser(includeSite: true).Returns(caller);
            _timeProviderFake.GetLocalDateTimeNow().Returns(DeletedAt);

            _userRepositoryFake
                .DeleteUserWithDependentsAsync(caller.RowId, Arg.Any<CancellationToken>())
                .Returns(new UserDeletionCounts(1, 1));

            var administrators = new List<GetAllUserInfo>
            {
                CreateAdminInfo(Guid.NewGuid(), "other-admin", OtherAdministratorEmail)
            };

            if (includeCallerAsAdministrator)
            {
                administrators.Add(CreateAdminInfo(caller.RowId, caller.Username, caller.Email));
            }

            _userRepositoryFake
                .GetEnabledAdminsForCurrentSiteAsync(Arg.Any<CancellationToken>())
                .Returns(administrators);

            return caller;
        }

        private static Input CreateInput(UserEntity caller)
        {
            return new Input
            {
                Username = caller.Username,
                Password = "Password123!",
                PlatformAdminRowIds = []
            };
        }

        private static GetAllUserInfo CreateAdminInfo(Guid rowId, string username, string email)
        {
            return new GetAllUserInfo
            {
                RowId = rowId,
                Etag = 1,
                Username = username,
                DisplayName = OtherAdministratorDisplayName,
                Email = email,
                Status = UserStatus.Enabled.Name,
                Roles = [Role.Admin.Name]
            };
        }
    }

    private const string OtherAdministratorEmail = "other-admin@example.com";
    private const string OtherAdministratorDisplayName = "Administrator";

    private static readonly DateTime DeletedAt = new(2026, 10, 21, 14, 30, 0, DateTimeKind.Local);

    private DeleteOwnAccountService CreateService()
    {
        return new DeleteOwnAccountService(
            _userRepositoryFake,
            _deleteCheckerFake,
            _emailWriterFake,
            _timeProviderFake,
            Substitute.For<ILogger<DeleteOwnAccountService>>());
    }
}
