using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Sites.Delete.EntityChecks;
using Pot.App.Features.Sites.Delete.EntityChecks.Checks;
using Pot.App.Features.Sites.Delete.Models;
using Pot.Data.Entities;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Sites.Delete.EntityChecks.Checks;

public class CheckUsernameMatchesFixture : PotFixtureBase
{
    public class HandleAsync : CheckUsernameMatchesFixture
    {
        [Fact]
        public async Task Should_Reject_A_Different_Username()
        {
            var (caller, site) = ArrangeCallerAndSite();

            var error = await CheckAsync(caller, site, "someone-else");

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.Username));
            error.ErrorMessage.ShouldBe("The username does not match your account.");
        }

        [Fact]
        public async Task Should_Allow_The_Callers_Username()
        {
            var (caller, site) = ArrangeCallerAndSite();

            var error = await CheckAsync(caller, site, caller.Username);

            error.ShouldBeNull();
        }

        private static Task<ApiDetailError?> CheckAsync(UserEntity caller, SiteEntity site, string username)
        {
            var state = new InputState
            {
                Caller = caller,
                Site = site,
                SiteName = site.Name,
                Username = username,
                Password = "Password123!",
                PlatformAdminRowIds = []
            };

            return CreateCheck().HandleAsync(state, CancellationToken.None);
        }
    }

    private static CheckUsernameMatches CreateCheck()
    {
        return new CheckUsernameMatches(Substitute.For<ILogger<CheckUsernameMatches>>());
    }

    private static (UserEntity Caller, SiteEntity Site) ArrangeCallerAndSite()
    {
        var site = EntityFactory.CreateSite(name: "Test Site");
        var caller = EntityFactory.CreateUser(site, "caller");

        return (caller, site);
    }
}
