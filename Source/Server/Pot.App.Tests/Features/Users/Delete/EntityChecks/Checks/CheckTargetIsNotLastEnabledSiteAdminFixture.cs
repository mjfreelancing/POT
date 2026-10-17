using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Users.Delete.EntityChecks;
using Pot.App.Features.Users.Delete.EntityChecks.Checks;
using Pot.App.Features.Users.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.Delete.EntityChecks.Checks;

public class CheckTargetIsNotLastEnabledSiteAdminFixture : PotFixtureBase
{
    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();

    public class HandleAsync : CheckTargetIsNotLastEnabledSiteAdminFixture
    {
        [Fact]
        public async Task Should_Reject_When_The_Target_Is_The_Sites_Only_Enabled_Administrator()
        {
            var target = CreateUserInRole(EntityFactory.CreateSite(), "only-admin", Role.Admin);

            SetEnabledAdministrators(target.RowId);

            var error = await CheckAsync(target);

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.RowId));
            error.ErrorMessage.ShouldBe("This is the site's only enabled administrator. Promote or enable another administrator first.");
        }

        [Fact]
        public async Task Should_Allow_When_Another_Enabled_Administrator_Remains()
        {
            var target = CreateUserInRole(EntityFactory.CreateSite(), "first-admin", Role.Admin);

            SetEnabledAdministrators(target.RowId, Guid.NewGuid());

            var error = await CheckAsync(target);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_When_The_Target_Is_Not_An_Enabled_Administrator()
        {
            var target = CreateUserInRole(EntityFactory.CreateSite(), "viewer", Role.Viewer);

            SetEnabledAdministrators(Guid.NewGuid());

            var error = await CheckAsync(target);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_When_The_Site_Has_No_Enabled_Administrators_At_All()
        {
            var target = CreateUserInRole(EntityFactory.CreateSite(), "viewer", Role.Viewer);

            SetEnabledAdministrators();

            var error = await CheckAsync(target);

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity target)
        {
            var state = new InputState
            {
                Caller = EntityFactory.CreateUser(EntityFactory.CreateSite(), "caller"),
                Target = target,
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

    private CheckTargetIsNotLastEnabledSiteAdmin CreateCheck()
    {
        return new CheckTargetIsNotLastEnabledSiteAdmin(
            _userRepositoryFake,
            Substitute.For<ILogger<CheckTargetIsNotLastEnabledSiteAdmin>>());
    }

    private static UserEntity CreateUserInRole(SiteEntity site, string username, Role role)
    {
        var user = EntityFactory.CreateUser(site, username, $"{username}@example.com", $"{username} display");

        user.Roles.Add(new RoleEntity
        {
            RowId = Guid.NewGuid(),
            Name = role
        });

        return user;
    }
}
