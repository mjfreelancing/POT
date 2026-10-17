using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Users.Delete.EntityChecks;
using Pot.App.Features.Users.Delete.EntityChecks.Checks;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Entities;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.Delete.EntityChecks.Checks;

public class CheckTargetIsNotSelfFixture : PotFixtureBase
{
    public class HandleAsync : CheckTargetIsNotSelfFixture
    {
        [Fact]
        public async Task Should_Reject_When_The_Caller_Is_The_Target()
        {
            var site = EntityFactory.CreateSite();
            var caller = EntityFactory.CreateUser(site, "caller");

            var error = await CheckAsync(caller, caller);

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.RowId));
            error.ErrorMessage.ShouldBe(
                "You cannot delete your own account with this request.");
        }

        [Fact]
        public async Task Should_Allow_When_The_Target_Is_Another_User()
        {
            var site = EntityFactory.CreateSite();
            var caller = EntityFactory.CreateUser(site, "caller");
            var target = EntityFactory.CreateUser(site, "target");

            var error = await CheckAsync(caller, target);

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity caller, UserEntity target)
        {
            var state = new InputState
            {
                Caller = caller,
                Target = target,
                Password = "Password123!",
                PlatformAdminRowIds = []
            };

            return CreateCheck().HandleAsync(state, CancellationToken.None);
        }
    }

    private CheckTargetIsNotSelf CreateCheck()
    {
        return new CheckTargetIsNotSelf(Substitute.For<ILogger<CheckTargetIsNotSelf>>());
    }
}
