using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Features.Sites.Delete;
using Pot.App.Features.Sites.Delete.EntityChecks;
using Pot.App.Features.Sites.Delete.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Sites;
using Pot.Data.Repositories.Sites.Dtos;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Sites.Delete;

public class DeleteSiteServiceFixture : PotFixtureBase
{
    private readonly IPersistableSiteRepository _siteRepositoryFake = Substitute.For<IPersistableSiteRepository>();
    private readonly IUserRepository _userRepositoryFake = Substitute.For<IUserRepository>();
    private readonly ISiteDeleteChecker _deleteCheckerFake = Substitute.For<ISiteDeleteChecker>();
    private readonly ISendEmailChannelWriter _emailWriterFake = Substitute.For<ISendEmailChannelWriter>();
    private readonly ITimeProvider _timeProviderFake = Substitute.For<ITimeProvider>();

    public class DeleteSiteAsync : DeleteSiteServiceFixture
    {
        [Fact]
        public async Task Should_Remove_The_Callers_Site_And_Return_Success()
        {
            var (_, site) = ArrangeCallerAndSite();

            var result = await CreateService().DeleteSiteAsync(CreateInput(site), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            await _siteRepositoryFake.Received(1).DeleteCurrentSiteWithDependentsAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Run_The_Deletion_Rules_Before_Removing_Anything()
        {
            var (caller, site) = ArrangeCallerAndSite();
            var input = CreateInput(site);

            await CreateService().DeleteSiteAsync(input, CancellationToken.None);

            await _deleteCheckerFake.Received(1).CanDeleteAsync(
                input, caller, site, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Return_The_Refusal_Without_Removing_Anything()
        {
            var (_, site) = ArrangeCallerAndSite();

            var refusal = ApiDetailErrorFactory.CreateBadRequestError(nameof(Input.SiteName), site.RowId, "Refused");

            _deleteCheckerFake
                .CanDeleteAsync(Arg.Any<Input>(), Arg.Any<UserEntity>(), Arg.Any<SiteEntity>(), Arg.Any<CancellationToken>())
                .Returns(refusal);

            var result = await CreateService().DeleteSiteAsync(CreateInput(site), CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBeSameAs(refusal);

            await _siteRepositoryFake.DidNotReceive().DeleteCurrentSiteWithDependentsAsync(Arg.Any<CancellationToken>());
            await _emailWriterFake.DidNotReceive().SubmitAsync(
                Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Notify_Every_Enabled_Administrator_Of_The_Site()
        {
            var (caller, site) = ArrangeCallerAndSite();

            await CreateService().DeleteSiteAsync(CreateInput(site), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.SiteDeleted,
                Arg.Is<EmailConfigBase>(config => config.Email == caller.Email),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Name_The_Deleted_Site_And_The_Administrator_In_The_Notification()
        {
            var (_, site) = ArrangeCallerAndSite();

            await CreateService().DeleteSiteAsync(CreateInput(site), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.SiteDeleted,
                Arg.Is<EmailSiteDeletedInfo>(info => info.DeletedSiteName == site.Name &&
                    info.DeletedByLabel == "Casey Caller (caller)"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Address_Each_Recipient_By_Their_Display_Name()
        {
            var (caller, site) = ArrangeCallerAndSite();

            await CreateService().DeleteSiteAsync(CreateInput(site), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.SiteDeleted,
                Arg.Is<EmailSiteDeletedInfo>(info => info.Email == caller.Email &&
                    info.DisplayName == caller.DisplayName &&
                    info.RecipientName == caller.DisplayName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Report_When_The_Site_Was_Deleted()
        {
            var (_, site) = ArrangeCallerAndSite();

            await CreateService().DeleteSiteAsync(CreateInput(site), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.SiteDeleted,
                Arg.Is<EmailSiteDeletedInfo>(info => info.DeletedAt == DeletedAt &&
                    info.DeletedAtLabel == "Wed, 21 Oct 2026, 2:30 PM"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Still_Succeed_When_A_Notification_Cannot_Be_Submitted()
        {
            var (_, site) = ArrangeCallerAndSite();

            // NSubstitute's fluent Returns consumes the substitute's ValueTask once; CA2012 cannot see through the extension.
#pragma warning disable CA2012
            _emailWriterFake
                .SubmitAsync(Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask>(_ => throw new InvalidOperationException("The email channel is unavailable"));
#pragma warning restore CA2012

            var result = await CreateService().DeleteSiteAsync(CreateInput(site), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
        }

        private (UserEntity Caller, SiteEntity Site) ArrangeCallerAndSite()
        {
            var site = EntityFactory.CreateSite(name: "Delete Site");
            var caller = EntityFactory.CreateUser(site, "caller", "caller@example.com", "Casey Caller");

            _userRepositoryFake.GetCurrentUser(includeSite: true).Returns(caller);
            _siteRepositoryFake.GetCurrentSite().Returns(site);
            _timeProviderFake.GetLocalDateTimeNow().Returns(DeletedAt);

            _userRepositoryFake
                .GetEnabledAdminsForCurrentSiteAsync(Arg.Any<CancellationToken>())
                .Returns(
                [
                    new GetAllUserInfo
                    {
                        RowId = caller.RowId,
                        Etag = caller.Etag,
                        Username = caller.Username,
                        DisplayName = caller.DisplayName,
                        Email = caller.Email,
                        Status = UserStatus.Enabled.Name,
                        Roles = [Role.Admin.Name]
                    }
                ]);

            _siteRepositoryFake
                .DeleteCurrentSiteWithDependentsAsync(Arg.Any<CancellationToken>())
                .Returns(new SiteDeletionCounts(1, 1, 1, 1, 0, 0, 1));

            return (caller, site);
        }

        private static Input CreateInput(SiteEntity site)
        {
            return new Input
            {
                SiteName = site.Name,
                Username = "caller",
                Password = "Password123!",
                PlatformAdminRowIds = []
            };
        }
    }

    private static readonly DateTime DeletedAt = new(2026, 10, 21, 14, 30, 0, DateTimeKind.Local);

    private DeleteSiteService CreateService()
    {
        return new DeleteSiteService(
            _siteRepositoryFake,
            _userRepositoryFake,
            _deleteCheckerFake,
            _emailWriterFake,
            _timeProviderFake,
            Substitute.For<ILogger<DeleteSiteService>>());
    }
}
