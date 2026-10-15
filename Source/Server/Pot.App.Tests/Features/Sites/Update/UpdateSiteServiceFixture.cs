using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Sites.Update;
using Pot.App.Features.Sites.Update.EntityChecks;
using Pot.App.Features.Sites.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Sites;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Sites.Update;

public class UpdateSiteServiceFixture : PotFixtureBase
{
    private const string UpdatedSiteName = "Updated Site";

    public class Constructor : UpdateSiteServiceFixture
    {
        private readonly IPersistableSiteRepository _siteRepositoryFake;
        private readonly IPreUpdateChecker _preUpdateCheckerFake;

        public Constructor()
        {
            _siteRepositoryFake = Substitute.For<IPersistableSiteRepository>();
            _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
        }

        [Fact]
        public void Should_Throw_When_SiteRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateSiteService>>();

                _ = new UpdateSiteService(null!, _preUpdateCheckerFake, logger);
            });

            exception.ParamName.ShouldBe("siteRepository");
        }

        [Fact]
        public void Should_Throw_When_PreUpdateChecker_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateSiteService>>();

                _ = new UpdateSiteService(_siteRepositoryFake, null!, logger);
            });

            exception.ParamName.ShouldBe("preUpdateChecker");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new UpdateSiteService(_siteRepositoryFake, _preUpdateCheckerFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class UpdateSiteAsync : UpdateSiteServiceFixture
    {
        private readonly IPersistableSiteRepository _siteRepositoryFake;
        private readonly IPreUpdateChecker _preUpdateCheckerFake;

        public UpdateSiteAsync()
        {
            _siteRepositoryFake = Substitute.For<IPersistableSiteRepository>();
            _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
        }

        [Fact]
        public async Task Should_Fail_And_Not_Write_When_The_Target_Is_Not_The_Callers_Site()
        {
            _siteRepositoryFake
                .GetCurrentSite()
                .Returns(EntityFactory.CreateSite());

            var service = CreateService();
            var input = CreateInput(Guid.NewGuid());

            var result = await service.UpdateSiteAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _siteRepositoryFake.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Fail_And_Not_Write_When_The_PreUpdateChecker_Refuses()
        {
            var site = EntityFactory.CreateSite();
            var checkerError = ApiDetailErrorFactory.CreateEntityConstraintError("Name", UpdatedSiteName, "The site name is not allowed");

            _siteRepositoryFake
                .GetCurrentSite()
                .Returns(site);

            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<SiteEntity>(), Arg.Any<CancellationToken>())
                .Returns(checkerError);

            var service = CreateService();
            var input = CreateInput(site.RowId);

            var result = await service.UpdateSiteAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe(checkerError);

            site.Name.ShouldNotBe(UpdatedSiteName);

            await _siteRepositoryFake.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Update_The_Site_When_The_Target_Is_The_Callers_Site()
        {
            var site = EntityFactory.CreateSite();

            _siteRepositoryFake
                .GetCurrentSite()
                .Returns(site);

            var service = CreateService();
            var input = CreateInput(site.RowId);

            var result = await service.UpdateSiteAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            site.Name.ShouldBe(UpdatedSiteName);

            await _siteRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        private UpdateSiteService CreateService()
        {
            var logger = Substitute.For<ILogger<UpdateSiteService>>();

            return new UpdateSiteService(_siteRepositoryFake, _preUpdateCheckerFake, logger);
        }

        private static Input CreateInput(Guid rowId)
        {
            return new Input
            {
                RowId = rowId,
                Etag = 1,
                Name = UpdatedSiteName,
                Description = "Updated description"
            };
        }
    }
}
