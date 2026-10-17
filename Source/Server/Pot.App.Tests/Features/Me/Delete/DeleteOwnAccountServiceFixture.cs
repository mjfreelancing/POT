using Microsoft.Extensions.Logging;
using NSubstitute;
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

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.UserDeleted,
                Arg.Is<EmailUserDeletedInfo>(info => info.Email == caller.Email
                    && info.DeletedUsername == caller.Username
                    && info.DeletedByLabel == "themselves"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Still_Succeed_When_A_Notification_Cannot_Be_Submitted()
        {
            var caller = ArrangeCaller();

            _emailWriterFake
                .SubmitAsync(Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask>(_ => throw new InvalidOperationException("The email channel is unavailable"));

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
            var caller = EntityFactory.CreateUser(site, "caller", "caller@example.com", "Caller");

            _userRepositoryFake.GetCurrentUser(includeSite: true).Returns(caller);

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
                DisplayName = "Administrator",
                Email = email,
                Status = UserStatus.Enabled.Name,
                Roles = [Role.Admin.Name]
            };
        }
    }

    private const string OtherAdministratorEmail = "other-admin@example.com";

    private DeleteOwnAccountService CreateService()
    {
        return new DeleteOwnAccountService(
            _userRepositoryFake,
            _deleteCheckerFake,
            _emailWriterFake,
            Substitute.For<ILogger<DeleteOwnAccountService>>());
    }
}
