using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests;

public class DbContextBaseFixture : PotFixtureBase
{
    public class GetTableNameFromEntity : DbContextBaseFixture
    {
        [Fact]
        public void Should_Return_The_Entity_Type_Name_Without_The_Suffix()
        {
            var account = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Test Account", 1000.0);

            DbContextBase.GetTableNameFromEntity(account).ShouldBe("Account");
        }

        [Fact]
        public void Should_Return_The_Table_Name_For_Other_Entity_Types()
        {
            var site = EntityFactory.CreateSite();

            DbContextBase.GetTableNameFromEntity(site).ShouldBe("Site");
        }
    }

    public class SaveChanges : DbContextBaseFixture
    {
        [Fact]
        public void Should_Set_The_Etag_When_Adding_An_Entity()
        {
            using var dbContext = CreateContext();

            var site = EntityFactory.CreateSite();
            dbContext.Add(site);

            dbContext.SaveChanges();

            site.Etag.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task Should_Set_The_Etag_When_Adding_An_Entity_Async()
        {
            using var dbContext = CreateContext();

            var site = EntityFactory.CreateSite();
            dbContext.Add(site);

            await dbContext.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

            site.Etag.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Should_Set_The_Etag_When_Updating_An_Entity()
        {
            using var dbContext = CreateContext();

            var site = EntityFactory.CreateSite();
            dbContext.Add(site);
            dbContext.SaveChanges();

            site.Etag = 0;
            site.Description = "Updated Description";

            dbContext.SaveChanges();

            site.Etag.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Should_Set_The_Etag_When_Changes_Are_Accepted()
        {
            using var dbContext = CreateContext();

            var site = EntityFactory.CreateSite();
            dbContext.Add(site);

            dbContext.SaveChanges(acceptAllChangesOnSuccess: true);

            site.Etag.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task Should_Set_The_Etag_When_Changes_Are_Accepted_Async()
        {
            using var dbContext = CreateContext();

            var site = EntityFactory.CreateSite();
            dbContext.Add(site);

            await dbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true, Xunit.TestContext.Current.CancellationToken);

            site.Etag.ShouldBeGreaterThan(0);
        }
    }

    private static PotDbContext CreateContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var currentUserContext = Substitute.For<ICurrentUserContext>();

        return new PotDbContext(dbOptions, currentUserContext);
    }
}
