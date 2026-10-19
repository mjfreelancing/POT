using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Me.Delete.EntityChecks;
using Pot.App.Features.Me.Delete.EntityChecks.Checks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Me.Delete.EntityChecks.Checks;

public class CheckIsNotLastSiteAdminFixture : PotFixtureBase
{
    private const string OnlyAdministratorMessage =
        "You are your site's only enabled administrator. Promote or enable another administrator first, or delete the site instead.";

    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();

    public class HandleAsync : CheckIsNotLastSiteAdminFixture
    {
        [Fact]
        public async Task Should_Reject_When_The_Caller_Is_The_Sites_Only_Enabled_Administrator()
        {
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "only-admin");

            SetEnabledAdministrators(caller.RowId);

            var error = await CheckAsync(caller);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(OnlyAdministratorMessage);
        }

        [Fact]
        public async Task Should_Allow_When_Another_Enabled_Administrator_Remains()
        {
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "first-admin");

            SetEnabledAdministrators(caller.RowId, Guid.NewGuid());

            var error = await CheckAsync(caller);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_A_User_Who_Is_Not_An_Administrator()
        {
            var caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "viewer");

            SetEnabledAdministrators(Guid.NewGuid());

            var error = await CheckAsync(caller);

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity caller)
        {
            var state = new InputState
            {
                Caller = caller,
                Username = caller.Username,
                Password = "Password123!",
                PlatformAdminRowIds = []
            };

            return CreateCheck().HandleAsync(state, CancellationToken.None);
        }

        private void SetEnabledAdministrators(params Guid[] rowIds)
        {
            var administrators = rowIds
                .Select(rowId => new GetAllUserInfo
                {
                    RowId = rowId,
                    Etag = 1,
                    Username = $"admin-{rowId:N}",
                    DisplayName = "Site Administrator",
                    Email = $"admin-{rowId:N}@example.com",
                    Status = UserStatus.Enabled.Name,
                    Roles = [Role.Admin.Name]
                })
                .ToList();

            _userRepositoryFake
                .GetEnabledAdminsForCurrentSiteAsync(Arg.Any<CancellationToken>())
                .Returns(administrators);
        }
    }

    private CheckIsNotLastSiteAdmin CreateCheck()
    {
        return new CheckIsNotLastSiteAdmin(
            _userRepositoryFake,
            Substitute.For<ILogger<CheckIsNotLastSiteAdmin>>());
    }
}
