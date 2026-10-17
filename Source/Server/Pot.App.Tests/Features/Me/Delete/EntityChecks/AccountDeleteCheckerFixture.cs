using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Auth;
using Pot.App.Features.Me.Delete.EntityChecks;
using Pot.App.Features.Me.Delete.EntityChecks.Checks;
using Pot.App.Features.Me.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Me.Delete.EntityChecks;

public class AccountDeleteCheckerFixture : PotFixtureBase
{
    private const string PasswordMessage = "The password is incorrect.";
    private const string LastPlatformAdministratorMessage =
        "This is the last enabled platform administrator. Another platform administrator must be configured first.";

    public class CanDeleteAsync : AccountDeleteCheckerFixture
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_Report_The_Wrong_Password_First_Whatever_Order_The_Checks_Are_Supplied_In(bool reverseOrder)
        {
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "caller");

            var checks = CreateChecks(caller, passwordIsValid: false);

            if (reverseOrder)
            {
                checks = [.. checks.Reverse()];
            }

            var input = CreateInput(caller);

            var error = await CreateChecker(checks).CanDeleteAsync(input, caller, CancellationToken.None);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(PasswordMessage);
        }

        [Fact]
        public async Task Should_Report_The_Platform_Administrator_Refusal_Before_The_Site_Administrator_Rule()
        {
            // Both rules refuse: the caller is the site's only enabled administrator and the only configured
            // platform administrator.
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "caller");
            var checks = CreateChecks(caller, passwordIsValid: true);

            var input = CreateInput(caller, caller.RowId);

            var error = await CreateChecker(checks).CanDeleteAsync(input, caller, CancellationToken.None);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(LastPlatformAdministratorMessage);
        }

        private static IAccountDeleteCheck[] CreateChecks(UserEntity caller, bool passwordIsValid)
        {
            var passwordHasher = Substitute.For<IUserPasswordHasher>();

            passwordHasher
                .IsValidPasswordHash(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(passwordIsValid);

            var userRepository = Substitute.For<IUserRepository>();

            // The caller is the site's only enabled administrator and the last enabled platform administrator.
            userRepository
                .GetEnabledAdminsForCurrentSiteAsync(Arg.Any<CancellationToken>())
                .Returns([CreateAdminInfo(caller)]);

            userRepository
                .CountEnabledPlatformAdminsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(0);

            return
            [
                new CheckIsNotLastSiteAdmin(userRepository, Substitute.For<ILogger<CheckIsNotLastSiteAdmin>>()),
                new CheckIsNotPlatformAdmin(userRepository, Substitute.For<ILogger<CheckIsNotPlatformAdmin>>()),
                new CheckUsernameMatches(Substitute.For<ILogger<CheckUsernameMatches>>()),
                new CheckPasswordIsCorrect(passwordHasher, Substitute.For<ILogger<CheckPasswordIsCorrect>>())
            ];
        }

        private static AccountDeleteChecker CreateChecker(IEnumerable<IAccountDeleteCheck> checks)
        {
            return new AccountDeleteChecker(checks, Substitute.For<ILogger<AccountDeleteChecker>>());
        }

        private static Input CreateInput(UserEntity caller, params Guid[] platformAdminRowIds)
        {
            return new Input
            {
                Username = caller.Username,
                Password = "Password123!",
                PlatformAdminRowIds = platformAdminRowIds
            };
        }

        private static GetAllUserInfo CreateAdminInfo(UserEntity user)
        {
            return new GetAllUserInfo
            {
                RowId = user.RowId,
                Etag = user.Etag,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Status = UserStatus.Enabled.Name,
                Roles = [Role.Admin.Name]
            };
        }
    }
}
