using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Update.EntityChecks;
using Pot.App.Features.Accounts.Update.EntityChecks.Checks;
using Pot.App.Features.Accounts.Update.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Accounts.Update.EntityChecks.Checks;

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

        public AccountEntity AddAccount(SiteEntity site, string description, string number)
        {
            var account = EntityFactory.CreateAccount(site, description, balance: 0.0d);
            account.Number = number;

            DbContext.Add(account);
            DbContext.SaveChanges();

            return account;
        }

        public Task<ApiDetailError?> UpdateDescriptionAsync(AccountEntity accountToUpdate, string newDescription)
        {
            var input = new Input
            {
                RowId = accountToUpdate.RowId,
                Bsb = accountToUpdate.Bsb,
                Number = accountToUpdate.Number,
                Description = newDescription,
                Balance = accountToUpdate.Balance,
                Reserved = accountToUpdate.Reserved
            };

            var state = new InputState
            {
                Input = input,
                AccountToUpdate = accountToUpdate
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
        public async Task Should_Allow_Changing_Description_To_One_That_Only_Exists_On_Another_Site()
        {
            using var context = CreateTestContext(out var otherSite);

            var accountToUpdate = context.AddAccount(context.CurrentSite, "Old Account", "11111111");
            context.AddAccount(otherSite, "Everyday", "22222222");

            var error = await context.UpdateDescriptionAsync(accountToUpdate, "Everyday");

            error.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Reject_Changing_Description_To_One_Used_By_Another_Account_On_The_Same_Site()
        {
            using var context = CreateTestContext(out _);

            var accountToUpdate = context.AddAccount(context.CurrentSite, "Old Account", "11111111");
            context.AddAccount(context.CurrentSite, "Everyday", "22222222");

            var error = await context.UpdateDescriptionAsync(accountToUpdate, "Everyday");

            error.ShouldNotBeNull();
            error.PropertyName.ShouldBe(nameof(AccountEntity.Description));
            error.AttemptedValue.ShouldBe("Everyday");
            error.ErrorMessage.ShouldBe("The account description already exists");
        }

        [Fact]
        public async Task Should_Allow_When_Description_Is_Unchanged()
        {
            using var context = CreateTestContext(out _);

            var accountToUpdate = context.AddAccount(context.CurrentSite, "Everyday", "11111111");

            var error = await context.UpdateDescriptionAsync(accountToUpdate, "Everyday");

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
