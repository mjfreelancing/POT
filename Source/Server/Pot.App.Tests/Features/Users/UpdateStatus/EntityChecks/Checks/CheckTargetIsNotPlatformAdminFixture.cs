using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Users.UpdateStatus.EntityChecks;
using Pot.App.Features.Users.UpdateStatus.EntityChecks.Checks;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.UpdateStatus.EntityChecks.Checks;

public class CheckTargetIsNotPlatformAdminFixture : PotFixtureBase
{
    public class HandleAsync : CheckTargetIsNotPlatformAdminFixture
    {
        [Fact]
        public async Task Should_Reject_Disabling_The_Last_Enabled_Platform_Administrator()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            SetOtherEnabledPlatformAdministrators(0);

            var error = await CheckAsync(CreateCheck(), target, UserStatus.Disabled, target.RowId);

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.Status));
            error.AttemptedValue.ShouldBe(UserStatus.Disabled.Name);
            error.ErrorMessage.ShouldBe(LastPlatformAdministratorMessage);
        }

        [Fact]
        public async Task Should_Reject_Disabling_When_Configured_Administrators_Are_Not_Enabled()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            // Configured ids that no longer belong to an enabled user do not keep the platform operable.
            SetOtherEnabledPlatformAdministrators(0);

            var error = await CheckAsync(CreateCheck(), target, UserStatus.Disabled, Guid.NewGuid(), target.RowId);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(LastPlatformAdministratorMessage);
        }

        [Fact]
        public async Task Should_Allow_Disabling_When_Another_Enabled_Platform_Administrator_Remains()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            SetOtherEnabledPlatformAdministrators(1);

            var error = await CheckAsync(CreateCheck(), target, UserStatus.Disabled, Guid.NewGuid(), target.RowId);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Count_Only_The_Other_Configured_Administrators()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");
            var otherConfigured = Guid.NewGuid();

            SetOtherEnabledPlatformAdministrators(1);

            await CheckAsync(CreateCheck(), target, UserStatus.Disabled, otherConfigured, target.RowId);

            await _userRepositoryFake.Received(1).CountEnabledPlatformAdminsAsync(
                Arg.Is<IReadOnlyList<Guid>>(rowIds => rowIds.Count == 1 && rowIds[0] == otherConfigured),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Allow_Enabling_A_Configured_Platform_Administrator()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            var error = await CheckAsync(CreateCheck(), target, UserStatus.Enabled, target.RowId);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Reject_Moving_A_Configured_Platform_Administrator_Back_To_Pending()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            SetOtherEnabledPlatformAdministrators(0);

            var error = await CheckAsync(CreateCheck(), target, UserStatus.Pending, target.RowId);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(LastPlatformAdministratorMessage);
        }

        [Fact]
        public async Task Should_Reject_Moving_A_Configured_Platform_Administrator_Back_To_Approval()
        {
            // The rule covers a move to any status other than Enabled, not just Disabled, because only an
            // enabled platform administrator can administer the platform.
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            SetOtherEnabledPlatformAdministrators(0);

            var error = await CheckAsync(CreateCheck(), target, UserStatus.Approval, target.RowId);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(LastPlatformAdministratorMessage);
        }

        [Fact]
        public async Task Should_Allow_Disabling_A_User_Who_Is_Not_A_Platform_Administrator()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "not-platform-admin");

            var error = await CheckAsync(CreateCheck(), target, UserStatus.Disabled, Guid.NewGuid());

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_Disabling_When_No_Platform_Administrators_Are_Configured()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "not-platform-admin");

            var error = await CheckAsync(CreateCheck(), target, UserStatus.Disabled);

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(CheckTargetIsNotPlatformAdmin check, UserEntity target,
            UserStatus status, params Guid[] platformAdminRowIds)
        {
            var input = new Input
            {
                RowId = target.RowId,
                Etag = target.Etag,
                Status = status,
                PlatformAdminRowIds = platformAdminRowIds
            };

            var state = new InputState
            {
                Input = input,
                Target = target,
                UserEtag = target.Etag,
                PlatformAdminRowIds = platformAdminRowIds
            };

            return check.HandleAsync(state, CancellationToken.None);
        }

        private CheckTargetIsNotPlatformAdmin CreateCheck()
        {
            return new CheckTargetIsNotPlatformAdmin(
                _userRepositoryFake,
                Substitute.For<ILogger<CheckTargetIsNotPlatformAdmin>>());
        }

        private void SetOtherEnabledPlatformAdministrators(int count)
        {
            _userRepositoryFake
                .CountEnabledPlatformAdminsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(count);
        }
    }

    private const string LastPlatformAdministratorMessage =
        "This is the last enabled platform administrator. Another platform administrator must be configured first.";

    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();
}
