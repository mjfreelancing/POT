using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Users.UpdateStatus.EntityChecks;
using Pot.App.Features.Users.UpdateStatus.EntityChecks.Checks;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.UpdateStatus.EntityChecks.Checks;

public class CheckTargetIsNotLastEnabledSiteAdminFixture : PotFixtureBase
{
    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();

    public class HandleAsync : CheckTargetIsNotLastEnabledSiteAdminFixture
    {
        [Fact]
        public async Task Should_Reject_Disabling_The_Sites_Only_Enabled_Administrator()
        {
            var target = CreateUserInRole(EntityFactory.CreateSite(), "only-admin", Role.Admin);
            SetEnabledAdministrators(target.RowId);

            var error = await CheckAsync(target, UserStatus.Disabled);

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(Input.Status));
            error.AttemptedValue.ShouldBe(UserStatus.Disabled.Name);
            error.ErrorMessage.ShouldBe("This is the site's only enabled administrator. Promote or enable another administrator first.");
        }

        [Fact]
        public async Task Should_Allow_Disabling_An_Administrator_When_Another_Enabled_Administrator_Remains()
        {
            var target = CreateUserInRole(EntityFactory.CreateSite(), "first-admin", Role.Admin);
            SetEnabledAdministrators(target.RowId, Guid.NewGuid());

            var error = await CheckAsync(target, UserStatus.Disabled);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_Enabling_The_Sites_Only_Administrator()
        {
            var target = CreateUserInRole(EntityFactory.CreateSite(), "only-admin", Role.Admin);
            SetEnabledAdministrators(target.RowId);

            var error = await CheckAsync(target, UserStatus.Enabled);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_Disabling_A_User_Who_Is_Not_An_Administrator()
        {
            var target = CreateUserInRole(EntityFactory.CreateSite(), "viewer", Role.Viewer);
            SetEnabledAdministrators(Guid.NewGuid());

            var error = await CheckAsync(target, UserStatus.Disabled);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_Disabling_A_Viewer_When_The_Target_Is_The_Sites_Only_Enabled_User()
        {
            var target = CreateUserInRole(EntityFactory.CreateSite(), "viewer", Role.Viewer);
            SetEnabledAdministrators();

            var error = await CheckAsync(target, UserStatus.Disabled);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Refuse_When_The_Target_Is_The_Sites_Only_Administrator_Even_If_Another_Platform_Administrator_Remains()
        {
            // The platform-administrator rule allows this change, so the site rule has to protect the tenant.
            var target = CreateUserInRole(EntityFactory.CreateSite(), "only-admin", Role.Admin);
            SetEnabledAdministrators(target.RowId);
            SetOtherEnabledPlatformAdministrators(1);

            var error = await CheckAsync(target, UserStatus.Disabled, target.RowId);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe("This is the site's only enabled administrator. Promote or enable another administrator first.");
        }

        [Fact]
        public async Task Should_Defer_To_The_Platform_Administrator_Rule_When_The_Target_Is_The_Last_Enabled_Platform_Administrator()
        {
            // Both rules would refuse this change. The platform-administrator refusal is the one with no in-app
            // remedy, so it has to be the one reported, whatever order the checks run in.
            var target = CreateUserInRole(EntityFactory.CreateSite(), "only-admin", Role.Admin);
            SetEnabledAdministrators(target.RowId);
            SetOtherEnabledPlatformAdministrators(0);

            var error = await CheckAsync(target, UserStatus.Disabled, target.RowId);

            error.ShouldBeNull();
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity target, UserStatus status, params Guid[] platformAdminRowIds)
        {
            var input = new Input
            {
                RowId = target.RowId,
                Etag = target.Etag,
                Status = status,
                PlatformAdminRowIds = platformAdminRowIds
            };

            var state = new InputState
            {
                Input = input,
                Target = target,
                UserEtag = target.Etag,
                PlatformAdminRowIds = platformAdminRowIds
            };

            return CreateCheck().HandleAsync(state, CancellationToken.None);
        }

        private CheckTargetIsNotLastEnabledSiteAdmin CreateCheck()
        {
            return new CheckTargetIsNotLastEnabledSiteAdmin(
                _userRepositoryFake,
                Substitute.For<ILogger<CheckTargetIsNotLastEnabledSiteAdmin>>());
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

        private void SetOtherEnabledPlatformAdministrators(int count)
        {
            _userRepositoryFake
                .CountEnabledPlatformAdminsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(count);
        }
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
