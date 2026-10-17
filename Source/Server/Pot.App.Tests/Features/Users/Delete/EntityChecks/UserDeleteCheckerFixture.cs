using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Auth;
using Pot.App.Features.Users.Delete.EntityChecks;
using Pot.App.Features.Users.Delete.EntityChecks.Checks;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.Delete.EntityChecks;

public class UserDeleteCheckerFixture : PotFixtureBase
{
    private const string PasswordMessage = "The password is incorrect.";
    private const string SelfMessage = "You cannot delete your own account with this request.";

    public class CanDeleteAsync : UserDeleteCheckerFixture
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_Report_The_Wrong_Password_First_Whatever_Order_The_Checks_Are_Supplied_In(bool reverseOrder)
        {
            var site = EntityFactory.CreateSite();
            var target = EntityFactory.CreateUser(site, "target");

            var checks = CreateChecks(passwordIsValid: false);

            if (reverseOrder)
            {
                checks = [.. checks.Reverse()];
            }

            var error = await CreateChecker(checks).CanDeleteAsync(CreateInput(target), target, target, CancellationToken.None);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(PasswordMessage);
        }

        [Fact]
        public async Task Should_Report_The_Self_Refusal_Before_The_Site_Administrator_Rule()
        {
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "caller");

            var checks = CreateChecks(passwordIsValid: true);

            var error = await CreateChecker(checks).CanDeleteAsync(CreateInput(caller), caller, caller, CancellationToken.None);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(SelfMessage);
        }

        private IUserDeleteCheck[] CreateChecks(bool passwordIsValid)
        {
            var passwordHasher = Substitute.For<IUserPasswordHasher>();

            passwordHasher
                .IsValidPasswordHash(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(passwordIsValid);

            var userRepository = Substitute.For<IUserRepository>();

            return
            [
                new CheckTargetIsNotLastEnabledSiteAdmin(userRepository, Substitute.For<ILogger<CheckTargetIsNotLastEnabledSiteAdmin>>()),
                new CheckTargetIsNotPlatformAdmin(userRepository, Substitute.For<ILogger<CheckTargetIsNotPlatformAdmin>>()),
                new CheckTargetIsNotSelf(Substitute.For<ILogger<CheckTargetIsNotSelf>>()),
                new CheckPasswordIsCorrect(passwordHasher, Substitute.For<ILogger<CheckPasswordIsCorrect>>())
            ];
        }

        private static UserDeleteChecker CreateChecker(IEnumerable<IUserDeleteCheck> checks)
        {
            return new UserDeleteChecker(checks, Substitute.For<ILogger<UserDeleteChecker>>());
        }

        private static Input CreateInput(UserEntity target)
        {
            return new Input
            {
                RowId = target.RowId,
                Password = "Password123!",
                PlatformAdminRowIds = []
            };
        }
    }
}
