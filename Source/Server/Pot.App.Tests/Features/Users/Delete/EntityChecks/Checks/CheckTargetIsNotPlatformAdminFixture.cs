using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Users.Delete.EntityChecks;
using Pot.App.Features.Users.Delete.EntityChecks.Checks;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.Delete.EntityChecks.Checks;

public class CheckTargetIsNotPlatformAdminFixture : PotFixtureBase
{
    private const string LastPlatformAdministratorMessage =
        "This is the last enabled platform administrator. Another platform administrator must be configured first.";

    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();

    public class HandleAsync : CheckTargetIsNotPlatformAdminFixture
    {
        [Fact]
        public async Task Should_Reject_When_The_Target_Is_The_Last_Enabled_Platform_Administrator()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            SetOtherEnabledPlatformAdministrators(0);

            var error = await CheckAsync(target, target.RowId);

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.RowId));
            error.ErrorMessage.ShouldBe(LastPlatformAdministratorMessage);
        }

        [Fact]
        public async Task Should_Allow_When_Another_Enabled_Platform_Administrator_Remains()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");
            var otherConfigured = Guid.NewGuid();

            SetOtherEnabledPlatformAdministrators(1);

            var error = await CheckAsync(target, target.RowId, otherConfigured);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Not_Count_A_Configured_Administrator_That_Is_Not_Enabled()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            SetOtherEnabledPlatformAdministrators(0);

            var error = await CheckAsync(target, target.RowId, Guid.NewGuid());

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(LastPlatformAdministratorMessage);
        }

        [Fact]
        public async Task Should_Exclude_The_Target_From_The_Live_Administrator_Count()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");
            var otherConfigured = Guid.NewGuid();

            SetOtherEnabledPlatformAdministrators(1);

            await CheckAsync(target, otherConfigured, target.RowId);

            await _userRepositoryFake.Received(1).CountEnabledPlatformAdminsAsync(
                Arg.Is<IReadOnlyList<Guid>>(rowIds => rowIds.Count == 1 && rowIds[0] == otherConfigured),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Allow_When_The_Target_Is_Not_A_Configured_Platform_Administrator()
        {
            var target = EntityFactory.CreateUser(EntityFactory.CreateSite(), "not-platform-admin");

            var error = await CheckAsync(target, Guid.NewGuid());

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity target, params Guid[] platformAdminRowIds)
        {
            var state = new InputState
            {
                Caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "caller"),
                Target = target,
                Password = "Password123!",
                PlatformAdminRowIds = platformAdminRowIds
            };

            return CreateCheck().HandleAsync(state, CancellationToken.None);
        }

        private void SetOtherEnabledPlatformAdministrators(int count)
        {
            _userRepositoryFake
                .CountEnabledPlatformAdminsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(count);
        }
    }

    private CheckTargetIsNotPlatformAdmin CreateCheck()
    {
        return new CheckTargetIsNotPlatformAdmin(
            _userRepositoryFake,
            Substitute.For<ILogger<CheckTargetIsNotPlatformAdmin>>());
    }
}
