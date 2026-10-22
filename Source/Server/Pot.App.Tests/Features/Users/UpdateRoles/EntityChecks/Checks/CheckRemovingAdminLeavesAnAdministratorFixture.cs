using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Users.UpdateRoles.EntityChecks;
using Pot.App.Features.Users.UpdateRoles.EntityChecks.Checks;
using Pot.App.Features.Users.UpdateRoles.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.UpdateRoles.EntityChecks.Checks;

public class CheckRemovingAdminLeavesAnAdministratorFixture : PotFixtureBase
{
    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();

    public class HandleAsync : CheckRemovingAdminLeavesAnAdministratorFixture
    {
        [Fact]
        public async Task Should_Reject_Removing_Admin_From_The_Sites_Only_Enabled_Administrator()
        {
            var target = CreateUserInSite("only-admin");
            var adminRole = AddRole(target, Role.Admin);
            SetEnabledAdministrators(target.RowId);

            var error = await CheckAsync(target, []);

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(InputState.RoleIds));
            error.ErrorMessage.ShouldBe("This is the site's only enabled administrator. Promote another administrator first.");

            target.Roles.ShouldContain(role => role.RowId == adminRole.RowId);
        }

        [Fact]
        public async Task Should_Allow_Removing_Admin_When_Another_Enabled_Administrator_Remains()
        {
            var target = CreateUserInSite("first-admin");
            AddRole(target, Role.Admin);
            SetEnabledAdministrators(target.RowId, Guid.NewGuid());

            var error = await CheckAsync(target, []);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_Removing_Admin_When_No_Enabled_Administrator_Remains()
        {
            // The target already cannot administer the site, so removing the role does not leave the site
            // without an administrator - it already was.
            var target = CreateUserInSite("disabled-admin");
            AddRole(target, Role.Admin);
            target.Status = UserStatus.Disabled;
            SetEnabledAdministrators();

            var error = await CheckAsync(target, []);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_Removing_Admin_When_The_Target_Does_Not_Hold_It()
        {
            var target = CreateUserInSite("viewer");
            AddRole(target, Role.Viewer);
            SetEnabledAdministrators(target.RowId);

            var error = await CheckAsync(target, []);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_Replacing_Admin_With_Another_Role_When_A_Second_Administrator_Remains()
        {
            var target = CreateUserInSite("first-admin");
            AddRole(target, Role.Admin);
            var viewerRole = AddRole(target, Role.Viewer);
            SetEnabledAdministrators(target.RowId, Guid.NewGuid());

            var error = await CheckAsync(target, [viewerRole.RowId]);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_Keeping_Admin_When_The_Site_Has_No_Other_Administrator()
        {
            var target = CreateUserInSite("only-admin");
            var adminRole = AddRole(target, Role.Admin);
            SetEnabledAdministrators(target.RowId);

            var error = await CheckAsync(target, [adminRole.RowId]);

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity target, Guid[] roleIds)
        {
            var input = new Input
            {
                RowId = target.RowId,
                Etag = target.Etag,
                RoleIds = roleIds
            };

            var state = new InputState
            {
                Input = input,
                Target = target,
                UserEtag = target.Etag,
                RoleIds = roleIds
            };

            return CreateCheck().HandleAsync(state, CancellationToken.None);
        }

        private CheckRemovingAdminLeavesAnAdministrator CreateCheck()
        {
            return new CheckRemovingAdminLeavesAnAdministrator(
                _userRepositoryFake,
                Substitute.For<ILogger<CheckRemovingAdminLeavesAnAdministrator>>());
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

    private static UserEntity CreateUserInSite(string username)
    {
        return EntityFactory.CreateUser(EntityFactory.CreateSite(), username, $"{username}@example.com", $"{username} display");
    }

    private static RoleEntity AddRole(UserEntity user, Role role)
    {
        var roleEntity = new RoleEntity
        {
            RowId = Guid.NewGuid(),
            Name = role
        };

        user.Roles.Add(roleEntity);

        return roleEntity;
    }
}
