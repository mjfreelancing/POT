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

public class CheckSiteNameMatchesFixture : PotFixtureBase
{
    public class HandleAsync : CheckSiteNameMatchesFixture
    {
        [Fact]
        public async Task Should_Reject_A_Different_Site_Name()
        {
            var (caller, site) = ArrangeCallerAndSite();

            var error = await CheckAsync(caller, site, "Another Site");

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.SiteName));
            error.ErrorMessage.ShouldBe("The site name does not match your site.");
        }

        [Fact]
        public async Task Should_Ignore_Surrounding_Whitespace_And_Case()
        {
            var (caller, site) = ArrangeCallerAndSite();

            var error = await CheckAsync(caller, site, "  test site  ");

            error.ShouldBeNull();
        }

        private static Task<ApiDetailError?> CheckAsync(UserEntity caller, SiteEntity site, string siteName)
        {
            var state = new InputState
            {
                Caller = caller,
                Site = site,
                SiteName = siteName,
                Username = caller.Username,
                Password = "Password123!",
                PlatformAdminRowIds = []
            };

            return CreateCheck().HandleAsync(state, CancellationToken.None);
        }
    }

    private static CheckSiteNameMatches CreateCheck()
    {
        return new CheckSiteNameMatches(Substitute.For<ILogger<CheckSiteNameMatches>>());
    }

    private static (UserEntity Caller, SiteEntity Site) ArrangeCallerAndSite()
    {
        var site = EntityFactory.CreateSite(name: "Test Site");
        var caller = EntityFactory.CreateUser(site, "caller");

        return (caller, site);
    }
}
