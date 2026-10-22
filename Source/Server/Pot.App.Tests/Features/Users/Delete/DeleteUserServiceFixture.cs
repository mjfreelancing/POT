using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Features.Users.Delete;
using Pot.App.Features.Users.Delete.EntityChecks;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.Delete;

public class DeleteUserServiceFixture : PotFixtureBase
{
    private readonly IPersistableUserRepository _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
    private readonly IUserDeleteChecker _deleteCheckerFake = Substitute.For<IUserDeleteChecker>();
    private readonly ISendEmailChannelWriter _emailWriterFake = Substitute.For<ISendEmailChannelWriter>();
    private readonly ITimeProvider _timeProviderFake = Substitute.For<ITimeProvider>();

    public class DeleteUserAsync : DeleteUserServiceFixture
    {
        [Fact]
        public async Task Should_Remove_The_Target_And_Return_Success()
        {
            var (_, target) = ArrangeCallerAndTarget();

            var result = await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            await _userRepositoryFake.Received(1).DeleteUserWithDependentsAsync(
                target.RowId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Run_The_Deletion_Rules_Before_Removing_Anything()
        {
            var (caller, target) = ArrangeCallerAndTarget();
            var input = CreateInput(target);

            await CreateService().DeleteUserAsync(input, CancellationToken.None);

            await _deleteCheckerFake.Received(1).CanDeleteAsync(
                input, caller, target, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Return_Not_Found_When_The_Target_Is_Not_In_The_Callers_Site()
        {
            var (_, target) = ArrangeCallerAndTarget();
            ArrangeTarget(null);

            var result = await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            (result.Error as ApiErrorBase)!.ErrorType.ShouldBe(ErrorType.NotFound);

            await _userRepositoryFake.DidNotReceive().DeleteUserWithDependentsAsync(
                Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Return_The_Refusal_Without_Removing_Anything()
        {
            var (_, target) = ArrangeCallerAndTarget();

            var refusal = ApiDetailErrorFactory.CreateBadRequestError(nameof(Input.RowId), "Refused");

            _deleteCheckerFake
                .CanDeleteAsync(Arg.Any<Input>(), Arg.Any<UserEntity>(), Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
                .Returns(refusal);

            var result = await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBeSameAs(refusal);

            await _userRepositoryFake.DidNotReceive().DeleteUserWithDependentsAsync(
                Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _emailWriterFake.DidNotReceive().SubmitAsync(
                Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Return_Not_Found_When_The_Target_Is_Removed_Before_The_Sweep()
        {
            var (_, target) = ArrangeCallerAndTarget();

            _userRepositoryFake
                .DeleteUserWithDependentsAsync(target.RowId, Arg.Any<CancellationToken>())
                .Returns((UserDeletionCounts?)null);

            var result = await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            (result.Error as ApiErrorBase)!.ErrorType.ShouldBe(ErrorType.NotFound);
        }

        [Fact]
        public async Task Should_Notify_The_Deleted_User_And_Every_Enabled_Administrator()
        {
            var (caller, target) = ArrangeCallerAndTarget();

            await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailConfigBase>(config => config.Email == target.Email),
                Arg.Any<CancellationToken>());

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailConfigBase>(config => config.Email == caller.Email),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Name_The_Deleted_User_And_The_Administrator_In_Each_Notification()
        {
            var (_, target) = ArrangeCallerAndTarget();

            await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            // One message per recipient - the deleted user and the administrator - each naming the same event.
            await _emailWriterFake.Received(2).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.DeletedUserLabel == "Taylor Target (target)" &&
                    info.DeletedByLabel == "Casey Caller (caller)" &&
                    info.SiteName == "Delete Site"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Name_A_User_By_Username_Alone_When_The_Display_Name_Matches_It_Ignoring_Case()
        {
            var (_, target) = ArrangeCallerAndTarget(targetDisplayName: "TARGET");

            await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            await _emailWriterFake.Received(2).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.DeletedUserLabel == "target"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Address_Each_Recipient_By_Their_Display_Name()
        {
            var (caller, target) = ArrangeCallerAndTarget();

            await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.Email == target.Email &&
                    info.DisplayName == target.DisplayName &&
                    info.RecipientName == target.DisplayName),
                Arg.Any<CancellationToken>());

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.Email == caller.Email &&
                    info.DisplayName == caller.DisplayName &&
                    info.RecipientName == caller.DisplayName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Address_The_Deleted_Users_Copy_To_Them()
        {
            var (_, target) = ArrangeCallerAndTarget();

            await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.Email == target.Email &&
                    info.IsRecipientDeletedUser &&
                    info.Heading == "Your Account Was Deleted" &&
                    info.Introduction == "Your account on the site Delete Site has been deleted by an administrator. You can no longer sign in." &&
                    info.ClosingNote == "This is an automated confirmation. No further action is required." &&
                    info.Details.Select(detail => detail.Label).SequenceEqual(new[] { "Site", "Deleted by", "Deleted at" })),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Name_The_Deleted_User_In_The_Administrators_Copy()
        {
            var (caller, target) = ArrangeCallerAndTarget();

            await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            var expectedDetailsSection = string.Join(Environment.NewLine,
                "User:       Taylor Target (target)",
                "Site:       Delete Site",
                "Deleted by: Casey Caller (caller)",
                "Deleted at: Wed, 21 Oct 2026, 2:30 PM",
                string.Empty);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.Email == caller.Email &&
                    !info.IsRecipientDeletedUser &&
                    info.Heading == "User Deleted" &&
                    info.Introduction == "A user account on your site has been deleted." &&
                    info.ClosingNote == "This is an automated confirmation. No further action is required." &&
                    info.DetailsSection == expectedDetailsSection),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Report_When_The_User_Was_Deleted()
        {
            var (_, target) = ArrangeCallerAndTarget();

            await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            await _emailWriterFake.Received(2).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.DeletedAt == DeletedAt &&
                    info.DeletedAtLabel == "Wed, 21 Oct 2026, 2:30 PM"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Still_Succeed_When_A_Notification_Cannot_Be_Submitted()
        {
            var (_, target) = ArrangeCallerAndTarget();

            // NSubstitute's fluent Returns consumes the substitute's ValueTask once; CA2012 cannot see through the extension.
#pragma warning disable CA2012
            _emailWriterFake
                .SubmitAsync(Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask>(_ => throw new InvalidOperationException("The email channel is unavailable"));
#pragma warning restore CA2012

            var result = await CreateService().DeleteUserAsync(CreateInput(target), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
        }

        private (UserEntity Caller, UserEntity Target) ArrangeCallerAndTarget(string targetDisplayName = "Taylor Target")
        {
            var site = EntityFactory.CreateSite(name: "Delete Site");

            var caller = EntityFactory.CreateUser(site, "caller", "caller@example.com", "Casey Caller");
            var target = EntityFactory.CreateUser(site, "target", "target@example.com", targetDisplayName);

            _userRepositoryFake.GetCurrentUser(includeSite: true).Returns(caller);
            _timeProviderFake.GetLocalDateTimeNow().Returns(DeletedAt);

            ArrangeTarget(target);

            _userRepositoryFake
                .DeleteUserWithDependentsAsync(target.RowId, Arg.Any<CancellationToken>())
                .Returns(new UserDeletionCounts(2, 1));

            _userRepositoryFake
                .GetEnabledAdminsForCurrentSiteAsync(Arg.Any<CancellationToken>())
                .Returns([CreateAdminInfo(caller)]);

            return (caller, target);
        }

        private void ArrangeTarget(UserEntity? target)
        {
            _userRepositoryFake
                .GetForCurrentSiteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(target);
        }

        private static Input CreateInput(UserEntity target)
        {
            return new Input
            {
                RowId = target.RowId,
                Password = "Password123!",
                PlatformAdminRowIds = []
            };
        }

        private static GetAllUserInfo CreateAdminInfo(UserEntity user)
        {
            return new GetAllUserInfo
            {
                RowId = user.RowId,
                Etag = user.Etag,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Status = UserStatus.Enabled.Name,
                Roles = [Role.Admin.Name]
            };
        }
    }

    private static readonly DateTime DeletedAt = new(2026, 10, 21, 14, 30, 0, DateTimeKind.Local);

    private DeleteUserService CreateService()
    {
        return new DeleteUserService(
            _userRepositoryFake,
            _deleteCheckerFake,
            _emailWriterFake,
            _timeProviderFake,
            Substitute.For<ILogger<DeleteUserService>>());
    }
}
