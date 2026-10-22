using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Auth;
using Pot.App.Errors;
using Pot.App.Features.Me.Delete.EntityChecks;
using Pot.App.Features.Me.Delete.EntityChecks.Checks;
using Pot.App.Features.Me.Delete.Models;
using Pot.Data.Entities;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Me.Delete.EntityChecks.Checks;

public class CheckPasswordIsCorrectFixture : PotFixtureBase
{
    private readonly IUserPasswordHasher _passwordHasherFake = Substitute.For<IUserPasswordHasher>();

    public class HandleAsync : CheckPasswordIsCorrectFixture
    {
        [Fact]
        public async Task Should_Reject_When_The_Password_Does_Not_Match_The_Callers_Hash()
        {
            SetPasswordIsValid(isValid: false);

            var error = await CheckAsync(EntityFactory.CreateUser(EntityFactory.CreateSite(), "caller"), "WrongPassword1!");

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.Password));
            error.ErrorMessage.ShouldBe("The password is incorrect.");
        }

        [Fact]
        public async Task Should_Allow_When_The_Password_Matches()
        {
            SetPasswordIsValid(isValid: true);

            var error = await CheckAsync(EntityFactory.CreateUser(EntityFactory.CreateSite(), "caller"), "Password123!");

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity caller, string password)
        {
            var state = new InputState
            {
                Caller = caller,
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
}
