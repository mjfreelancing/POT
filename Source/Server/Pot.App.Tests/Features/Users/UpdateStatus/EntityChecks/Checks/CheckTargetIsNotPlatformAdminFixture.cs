using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Users.UpdateStatus.EntityChecks;
using Pot.App.Features.Users.UpdateStatus.EntityChecks.Checks;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.UpdateStatus.EntityChecks.Checks;

public class CheckTargetIsNotPlatformAdminFixture : PotFixtureBase
{
    public class HandleAsync : CheckTargetIsNotPlatformAdminFixture
    {
        [Fact]
        public async Task Should_Reject_Disabling_A_Configured_Platform_Administrator()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");
            var check = CreateCheck();

            var error = await CheckAsync(check, target, UserStatus.Disabled, target.RowId);

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.Status));
            error.AttemptedValue.ShouldBe(UserStatus.Disabled.Name);
            error.ErrorMessage.ShouldBe("A platform administrator cannot be disabled.");
        }

        [Fact]
        public async Task Should_Reject_Disabling_When_One_Of_Several_Platform_Administrators_Is_The_Target()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");
            var check = CreateCheck();
            var configured = new[] { Guid.NewGuid(), target.RowId, Guid.NewGuid() };

            var error = await CheckAsync(check, target, UserStatus.Disabled, configured);

            error.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_Allow_Enabling_A_Configured_Platform_Administrator()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");
            var check = CreateCheck();

            var error = await CheckAsync(check, target, UserStatus.Enabled, target.RowId);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Reject_Moving_A_Configured_Platform_Administrator_Back_To_Pending()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");
            var check = CreateCheck();

            var error = await CheckAsync(check, target, UserStatus.Pending, target.RowId);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe("A platform administrator cannot be disabled.");
        }

        [Fact]
        public async Task Should_Reject_Moving_A_Configured_Platform_Administrator_Back_To_Approval()
        {
            // The rule covers a move to any status other than Enabled, not just Disabled, because only an
            // enabled platform administrator can administer the platform.
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");
            var check = CreateCheck();

            var error = await CheckAsync(check, target, UserStatus.Approval, target.RowId);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe("A platform administrator cannot be disabled.");
        }

        [Fact]
        public async Task Should_Allow_Disabling_A_User_Who_Is_Not_A_Platform_Administrator()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "not-platform-admin");
            var check = CreateCheck();

            var error = await CheckAsync(check, target, UserStatus.Disabled, Guid.NewGuid());

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_Disabling_When_No_Platform_Administrators_Are_Configured()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "not-platform-admin");
            var check = CreateCheck();

            var error = await CheckAsync(check, target, UserStatus.Disabled);

            error.ShouldBeNull();
        }

        private static Task<ApiDetailError?> CheckAsync(CheckTargetIsNotPlatformAdmin check, UserEntity target,
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

        private static CheckTargetIsNotPlatformAdmin CreateCheck()
        {
            return new CheckTargetIsNotPlatformAdmin(
                Substitute.For<ILogger<CheckTargetIsNotPlatformAdmin>>());
        }
    }
}
