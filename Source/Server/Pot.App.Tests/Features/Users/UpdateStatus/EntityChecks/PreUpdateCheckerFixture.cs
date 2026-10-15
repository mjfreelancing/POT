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

namespace Pot.App.Tests.Features.Users.UpdateStatus.EntityChecks;

public class PreUpdateCheckerFixture : PotFixtureBase
{
    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();

    public class CanSaveAsync : PreUpdateCheckerFixture
    {
        [Fact]
        public async Task Should_Report_The_Platform_Administrator_Refusal_When_The_Last_Enabled_Administrator_Check_Runs_First()
        {
            var error = await CheckBothRulesAsync(platformAdminCheckFirst: false);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe("A platform administrator cannot be disabled.");
        }

        [Fact]
        public async Task Should_Report_The_Platform_Administrator_Refusal_When_The_Platform_Administrator_Check_Runs_First()
        {
            var error = await CheckBothRulesAsync(platformAdminCheckFirst: true);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe("A platform administrator cannot be disabled.");
        }

        /// <summary>
        /// Composes the two protected-identity checks in a chosen order against a target that both rules would
        /// refuse, so the refusal reported cannot depend on the order the checks happen to be registered in.
        /// The platform-administrator refusal is the one without an in-app remedy, so it has to win.
        /// </summary>
        private async Task<ApiDetailError?> CheckBothRulesAsync(bool platformAdminCheckFirst)
        {
            var site = EntityFactory.CreateSite();
            var target = CreateOnlyEnabledAdministrator(site);
            SetEnabledAdministrators(target.RowId);

            var lastEnabledAdministratorCheck = new CheckTargetIsNotLastEnabledSiteAdmin(
                _userRepositoryFake,
                Substitute.For<ILogger<CheckTargetIsNotLastEnabledSiteAdmin>>());

            var platformAdministratorCheck = new CheckTargetIsNotPlatformAdmin(
                Substitute.For<ILogger<CheckTargetIsNotPlatformAdmin>>());

            PreUpdateCheckBase[] checks = platformAdminCheckFirst
                ? [platformAdministratorCheck, lastEnabledAdministratorCheck]
                : [lastEnabledAdministratorCheck, platformAdministratorCheck];

            var input = new Input
            {
                RowId = target.RowId,
                Etag = target.Etag,
                Status = UserStatus.Disabled,
                PlatformAdminRowIds = [target.RowId]
            };

            var checker = new PreUpdateChecker(checks, Substitute.For<ILogger<PreUpdateChecker>>());

            return await checker.CanSaveAsync(input, target, CancellationToken.None);
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

    private static UserEntity CreateOnlyEnabledAdministrator(SiteEntity site)
    {
        var user = EntityFactory.CreateUser(site, "only-admin", "only-admin@example.com", "Only Admin");

        user.Roles.Add(new RoleEntity
        {
            RowId = Guid.NewGuid(),
            Name = Role.Admin
        });

        return user;
    }
}
