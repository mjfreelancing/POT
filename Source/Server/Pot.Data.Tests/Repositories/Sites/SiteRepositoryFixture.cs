using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Sites;
using Pot.Data.Repositories.Users;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.Sites;

public class SiteRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public SiteRepository Repository { get; }
        public SiteEntity Site { get; }
        public UserEntity User { get; }

        public TestContext(PotDbContext dbContext, SiteRepository repository, SiteEntity site, UserEntity user)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            User = user;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class GetCurrentSite : SiteRepositoryFixture
    {
        [Fact]
        public void Should_Return_The_Site_Of_The_Current_User()
        {
            using var context = CreateTestContext();

            var result = context.Repository.GetCurrentSite();

            result.RowId.ShouldBe(context.Site.RowId);
            result.Name.ShouldBe(context.Site.Name);
        }
    }

    public class Sites : SiteRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Sites()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.Sites.ToListAsync(Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(context.Site.RowId);
        }
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site);

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.SaveChanges();

        var userRepository = new UserRepository(dbContext, currentUserContext);
        var repository = new SiteRepository(dbContext, userRepository);

        return new TestContext(dbContext, repository, site, user);
    }
}
