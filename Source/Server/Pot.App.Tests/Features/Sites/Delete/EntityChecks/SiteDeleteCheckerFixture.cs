using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Auth;
using Pot.App.Features.Sites.Delete.EntityChecks;
using Pot.App.Features.Sites.Delete.EntityChecks.Checks;
using Pot.App.Features.Sites.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Sites.Delete.EntityChecks;

public class SiteDeleteCheckerFixture : PotFixtureBase
{
    private const string PasswordMessage = "The password is incorrect.";
    private const string SiteNameMessage = "The site name does not match your site.";

    public class CanDeleteAsync : SiteDeleteCheckerFixture
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_Report_The_Wrong_Password_First_Whatever_Order_The_Checks_Are_Supplied_In(bool reverseOrder)
        {
            var (caller, site) = ArrangeCallerAndSite();

            var checks = CreateChecks(caller, passwordIsValid: false);

            if (reverseOrder)
            {
                checks = [.. checks.Reverse()];
            }

            var input = CreateInput(siteName: "Wrong Site");

            var error = await CreateChecker(checks).CanDeleteAsync(input, caller, site, CancellationToken.None);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(PasswordMessage);
        }

        [Fact]
        public async Task Should_Report_A_Typed_Value_Before_The_Platform_Administrator_Rule()
        {
            // The caller mistyped the site name and the site holds the last platform administrator. The
            // confirmation problem is the one they can fix.
            var (caller, site) = ArrangeCallerAndSite();

            var checks = CreateChecks(caller, passwordIsValid: true);

            var input = CreateInput(siteName: "Wrong Site");

            var error = await CreateChecker(checks).CanDeleteAsync(input, caller, site, CancellationToken.None);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(SiteNameMessage);
        }

        private static ISiteDeleteCheck[] CreateChecks(UserEntity caller, bool passwordIsValid)
        {
            var passwordHasher = Substitute.For<IUserPasswordHasher>();

            passwordHasher
                .IsValidPasswordHash(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(passwordIsValid);

            var userRepository = Substitute.For<IUserRepository>();

            userRepository
                .GetAllForCurrentSiteAsync(Arg.Any<CancellationToken>())
                .Returns([CreateUserInfo(caller)]);

            userRepository
                .CountEnabledPlatformAdminsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(0);

            return
            [
                new CheckSiteHasNoPlatformAdmin(userRepository, Substitute.For<ILogger<CheckSiteHasNoPlatformAdmin>>()),
                new CheckUsernameMatches(Substitute.For<ILogger<CheckUsernameMatches>>()),
                new CheckSiteNameMatches(Substitute.For<ILogger<CheckSiteNameMatches>>()),
                new CheckPasswordIsCorrect(passwordHasher, Substitute.For<ILogger<CheckPasswordIsCorrect>>())
            ];
        }

        private static SiteDeleteChecker CreateChecker(IEnumerable<ISiteDeleteCheck> checks)
        {
            return new SiteDeleteChecker(checks, Substitute.For<ILogger<SiteDeleteChecker>>());
        }

        private static Input CreateInput(string siteName)
        {
            return new Input
            {
                SiteName = siteName,
                Username = "caller",
                Password = "Password123!",
                PlatformAdminRowIds = []
            };
        }

        private static (UserEntity Caller, SiteEntity Site) ArrangeCallerAndSite()
        {
            var site = EntityFactory.CreateSite(name: "Test Site");
            var caller = EntityFactory.CreateUser(site, "caller");

            return (caller, site);
        }

        private static GetAllUserInfo CreateUserInfo(UserEntity user)
        {
            return new GetAllUserInfo
            {
                RowId = user.RowId,
                Etag = user.Etag,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Status = UserStatus.Enabled.Name,
                Roles = []
            };
        }
    }
}
