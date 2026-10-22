using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Auth;
using Pot.App.Errors;
using Pot.App.Features.Sites.Delete.EntityChecks;
using Pot.App.Features.Sites.Delete.EntityChecks.Checks;
using Pot.App.Features.Sites.Delete.Models;
using Pot.Data.Entities;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Sites.Delete.EntityChecks.Checks;

public class CheckPasswordIsCorrectFixture : PotFixtureBase
{
    private readonly IUserPasswordHasher _passwordHasherFake = Substitute.For<IUserPasswordHasher>();

    public class HandleAsync : CheckPasswordIsCorrectFixture
    {
        [Fact]
        public async Task Should_Reject_A_Wrong_Password()
        {
            SetPasswordIsValid(isValid: false);

            var (caller, site) = ArrangeCallerAndSite();

            var error = await CheckAsync(caller, site, "WrongPassword1!");

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.Password));
            error.ErrorMessage.ShouldBe("The password is incorrect.");
        }

        [Fact]
        public async Task Should_Allow_A_Correct_Password()
        {
            SetPasswordIsValid(isValid: true);

            var (caller, site) = ArrangeCallerAndSite();

            var error = await CheckAsync(caller, site, "Password123!");

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity caller, SiteEntity site, string password)
        {
            var state = new InputState
            {
                Caller = caller,
                Site = site,
                SiteName = site.Name,
                Username = caller.Username,
                Password = password,
                PlatformAdminRowIds = []
            };

            return CreateCheck().HandleAsync(state, CancellationToken.None);
        }

        private void SetPasswordIsValid(bool isValid)
        {
            _passwordHasherFake
                .IsValidPasswordHash(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(isValid);
        }
    }

    private CheckPasswordIsCorrect CreateCheck()
    {
        return new CheckPasswordIsCorrect(
            _passwordHasherFake,
            Substitute.For<ILogger<CheckPasswordIsCorrect>>());
    }

    private static (UserEntity Caller, SiteEntity Site) ArrangeCallerAndSite()
    {
        var site = EntityFactory.CreateSite(name: "Test Site");
        var caller = EntityFactory.CreateUser(site, "caller");

        return (caller, site);
    }
}
