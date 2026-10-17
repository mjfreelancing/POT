using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Me.Delete.EntityChecks;
using Pot.App.Features.Me.Delete.EntityChecks.Checks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Me.Delete.EntityChecks.Checks;

public class CheckIsNotPlatformAdminFixture : PotFixtureBase
{
    private const string LastPlatformAdministratorMessage =
        "This is the last enabled platform administrator. Another platform administrator must be configured first.";

    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();

    public class HandleAsync : CheckIsNotPlatformAdminFixture
    {
        [Fact]
        public async Task Should_Reject_When_The_Caller_Is_The_Last_Enabled_Platform_Administrator()
        {
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            SetOtherEnabledPlatformAdministrators(0);

            var error = await CheckAsync(caller, caller.RowId);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(LastPlatformAdministratorMessage);
        }

        [Fact]
        public async Task Should_Allow_When_Another_Enabled_Platform_Administrator_Remains()
        {
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            SetOtherEnabledPlatformAdministrators(1);

            var error = await CheckAsync(caller, caller.RowId, Guid.NewGuid());

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Not_Count_A_Configured_Administrator_That_Is_Not_Enabled()
        {
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "platform-admin");

            SetOtherEnabledPlatformAdministrators(0);

            var error = await CheckAsync(caller, Guid.NewGuid(), caller.RowId);

            error.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_Allow_A_User_Who_Is_Not_A_Platform_Administrator()
        {
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "not-platform-admin");

            var error = await CheckAsync(caller, Guid.NewGuid());

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity caller, params Guid[] platformAdminRowIds)
        {
            var state = new InputState
            {
                Caller = caller,
                Username = caller.Username,
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

    private CheckIsNotPlatformAdmin CreateCheck()
    {
        return new CheckIsNotPlatformAdmin(
            _userRepositoryFake,
            Substitute.For<ILogger<CheckIsNotPlatformAdmin>>());
    }
}
