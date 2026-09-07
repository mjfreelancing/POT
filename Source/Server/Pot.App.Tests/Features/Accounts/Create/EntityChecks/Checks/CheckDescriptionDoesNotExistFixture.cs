using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Create.EntityChecks;
using Pot.App.Features.Accounts.Create.EntityChecks.Checks;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Accounts.Create.EntityChecks.Checks;

public class CheckDescriptionDoesNotExistFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public SiteEntity CurrentSite { get; }
        private CheckDescriptionDoesNotExist Check { get; }

        public TestContext(PotDbContext dbContext, CheckDescriptionDoesNotExist check, SiteEntity currentSite)
        {
            DbContext = dbContext;
            Check = check;
            CurrentSite = currentSite;
        }

        public void AddAccount(SiteEntity site, string description)
        {
            var account = EntityFactory.CreateAccount(site, description, balance: 0.0d);

            DbContext.Add(account);
            DbContext.SaveChanges();
        }

        public Task<ApiDetailError?> CheckAsync(string description)
        {
            var accountToCreate = EntityFactory.CreateAccount(CurrentSite, description, balance: 0.0d);

            var state = new InputState
            {
                AccountToCreate = accountToCreate
            };

            return Check.HandleAsync(state, CancellationToken.None);
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class HandleAsync : CheckDescriptionDoesNotExistFixture
    {
        [Fact]
        public async Task Should_Allow_Description_That_Only_Exists_On_Another_Site()
        {
            using var context = CreateTestContext(out var otherSite);

            context.AddAccount(otherSite, "Everyday");

            var error = await context.CheckAsync("Everyday");

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Reject_Description_That_Already_Exists_On_The_Same_Site()
        {
            using var context = CreateTestContext(out _);

            context.AddAccount(context.CurrentSite, "Everyday");

            var error = await context.CheckAsync("Everyday");

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(AccountEntity.Description));
            error.AttemptedValue.ShouldBe("Everyday");
            error.ErrorMessage.ShouldBe("The account description already exists");
        }

        [Fact]
        public async Task Should_Allow_Description_That_Is_Not_Used_Anywhere()
        {
            using var context = CreateTestContext(out _);

            var error = await context.CheckAsync("New Account");

            error.ShouldBeNull();
        }
    }

    private static TestContext CreateTestContext(out SiteEntity otherSite)
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var currentSite = EntityFactory.CreateSite("Site 1", "First Site");
        var currentUser = EntityFactory.CreateUser(currentSite, "user1", "user1@example.com", "User 1");

        var otherSiteEntity = EntityFactory.CreateSite("Site 2", "Second Site");
        var otherUser = EntityFactory.CreateUser(otherSiteEntity, "user2", "user2@example.com", "User 2");

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(currentUser.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(currentSite);
        dbContext.Add(currentUser);
        dbContext.Add(otherSiteEntity);
        dbContext.Add(otherUser);
        dbContext.SaveChanges();

        var repository = new AccountRepository(dbContext);
        var check = new CheckDescriptionDoesNotExist(repository, NullLogger<CheckDescriptionDoesNotExist>.Instance);

        otherSite = otherSiteEntity;

        return new TestContext(dbContext, check, currentSite);
    }
}
