using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Me.Delete.EntityChecks;
using Pot.App.Features.Me.Delete.EntityChecks.Checks;
using Pot.App.Features.Me.Delete.Models;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Me.Delete.EntityChecks.Checks;

public class CheckUsernameMatchesFixture : PotFixtureBase
{
    public class HandleAsync : CheckUsernameMatchesFixture
    {
        [Fact]
        public async Task Should_Reject_A_Different_Username()
        {
            var error = await CheckAsync("caller", "someone-else");

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.Username));
            error.ErrorMessage.ShouldBe("The username does not match your account.");
        }

        [Fact]
        public async Task Should_Allow_The_Callers_Username()
        {
            var error = await CheckAsync("caller", "caller");

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Ignore_Surrounding_Whitespace_And_Case()
        {
            var error = await CheckAsync("Caller", "  caller  ");

            error.ShouldBeNull();
        }

        private static Task<ApiDetailError?> CheckAsync(string callerUsername, string typedUsername)
        {
            var state = new InputState
            {
                Caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), callerUsername),
                Username = typedUsername,
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
}
