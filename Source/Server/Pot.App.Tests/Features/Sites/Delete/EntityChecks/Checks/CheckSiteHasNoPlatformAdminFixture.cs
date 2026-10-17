using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Sites.Delete.EntityChecks;
using Pot.App.Features.Sites.Delete.EntityChecks.Checks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Sites.Delete.EntityChecks.Checks;

public class CheckSiteHasNoPlatformAdminFixture : PotFixtureBase
{
    private const string RefusalMessage =
        "This site contains the last enabled platform administrator. Another platform administrator must be configured first.";

    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();

    public class HandleAsync : CheckSiteHasNoPlatformAdminFixture
    {
        [Fact]
        public async Task Should_Reject_When_The_Site_Holds_The_Last_Enabled_Platform_Administrator()
        {
            var (caller, site) = ArrangeSiteWithCaller();

            ArrangeSiteUsers(caller);
            ArrangePlatformAdmins(enabledInSite: 1, enabledOutsideSite: 0);

            var error = await CheckAsync(caller, site, caller.RowId);

            error.ShouldNotBeNull();
            error.ErrorMessage.ShouldBe(RefusalMessage);
        }

        [Fact]
        public async Task Should_Allow_When_Another_Enabled_Platform_Administrator_Is_Outside_The_Site()
        {
            var (caller, site) = ArrangeSiteWithCaller();
            var outsideAdmin = Guid.NewGuid();

            ArrangeSiteUsers(caller);
            ArrangePlatformAdmins(enabledInSite: 1, enabledOutsideSite: 1);

            var error = await CheckAsync(caller, site, caller.RowId, outsideAdmin);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_When_The_Sites_Configured_Account_Is_Not_Enabled()
        {
            // Nothing outside the site can operate the platform, but nothing inside can either, so the deletion
            // does not remove a live operator.
            var (caller, site) = ArrangeSiteWithCaller();

            ArrangeSiteUsers(caller);
            ArrangePlatformAdmins(enabledInSite: 0, enabledOutsideSite: 0);

            var error = await CheckAsync(caller, site, caller.RowId);

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Allow_When_No_Configured_Platform_Administrator_Belongs_To_The_Site()
        {
            var (caller, site) = ArrangeSiteWithCaller();

            ArrangeSiteUsers(caller);
            ArrangePlatformAdmins(enabledInSite: 1, enabledOutsideSite: 1);

            var error = await CheckAsync(caller, site, Guid.NewGuid(), Guid.NewGuid());

            error.ShouldBeNull();
        }

        private static (UserEntity Caller, SiteEntity Site) ArrangeSiteWithCaller()
        {
            var site = EntityFactory.CreateSite(name: "Site To Delete");
            var caller = EntityFactory.CreateUser(site, "caller");

            return (caller, site);
        }

        private void ArrangeSiteUsers(params UserEntity[] users)
        {
            _userRepositoryFake
                .GetAllForCurrentSiteAsync(Arg.Any<CancellationToken>())
                .Returns([.. users.Select(CreateUserInfo)]);
        }

        private void ArrangePlatformAdmins(int enabledInSite, int enabledOutsideSite)
        {
            _userRepositoryFake
                .CountEnabledPlatformAdminsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var rowIds = callInfo.Arg<IReadOnlyList<Guid>>();

                    // The checker asks about the identifiers outside the site first, then those inside it.
                    return rowIds.Contains(CallerRowId) ? enabledInSite : enabledOutsideSite;
                });
        }

        private Task<ApiDetailError?> CheckAsync(UserEntity caller, SiteEntity site, params Guid[] platformAdminRowIds)
        {
            CallerRowId = caller.RowId;

            var state = new InputState
            {
                Caller = caller,
                Site = site,
                SiteName = site.Name,
                Username = caller.Username,
                Password = "Password123!",
                PlatformAdminRowIds = platformAdminRowIds
            };

            return CreateCheck().HandleAsync(state, CancellationToken.None);
        }

        private Guid CallerRowId { get; set; }

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

    private CheckSiteHasNoPlatformAdmin CreateCheck()
    {
        return new CheckSiteHasNoPlatformAdmin(
            _userRepositoryFake,
            Substitute.For<ILogger<CheckSiteHasNoPlatformAdmin>>());
    }
}
