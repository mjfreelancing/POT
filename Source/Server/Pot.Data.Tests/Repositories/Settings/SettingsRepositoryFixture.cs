using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Settings;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.Settings;

public class SettingsRepositoryFixture : PotFixtureBase
{
    private const string EnabledKey = "Enabled";
    private const string ReminderDaysKey = "ReminderDays";

    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public SettingsRepository Repository { get; }
        public SiteEntity Site { get; }
        public SiteEntity OtherSite { get; }

        public TestContext(PotDbContext dbContext, SettingsRepository repository, SiteEntity site, SiteEntity otherSite)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            OtherSite = otherSite;
        }

        public Task<int> AddSettingsAsync(params SettingEntity[] settings)
        {
            foreach (var setting in settings)
            {
                DbContext.Add(setting);
            }

            return DbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class GetAllSettingsAsync : SettingsRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_All_Settings_For_The_Current_Site()
        {
            using var context = CreateTestContext();

            await context.AddSettingsAsync(
                CreateSetting(context.Site, EnabledKey, "true"),
                CreateSetting(context.Site, ReminderDaysKey, "7"));

            var result = await context.Repository.GetAllSettingsAsync(Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(2);
            result.ShouldHaveValues(setting => setting.Key, new[] { EnabledKey, ReminderDaysKey });
        }

        [Fact]
        public async Task Should_Not_Return_Settings_From_Another_Site()
        {
            using var context = CreateTestContext();

            await context.AddSettingsAsync(
                CreateSetting(context.Site, EnabledKey, "true"),
                CreateSetting(context.OtherSite, EnabledKey, "false"));

            var result = await context.Repository.GetAllSettingsAsync(Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].Value.ShouldBe("true");
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_There_Are_No_Settings()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetAllSettingsAsync(Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }
    }

    public class GetSettingAsync : SettingsRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Setting_When_Category_And_Key_Match()
        {
            using var context = CreateTestContext();

            await context.AddSettingsAsync(
                CreateSetting(context.Site, EnabledKey, "true"),
                CreateSetting(context.Site, ReminderDaysKey, "7"));

            var result = await context.Repository.GetSettingAsync(SettingCategory.EmailBudgetReminder, ReminderDaysKey, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.Key.ShouldBe(ReminderDaysKey);
            result.Value.ShouldBe("7");
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Key_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            await context.AddSettingsAsync(CreateSetting(context.Site, EnabledKey, "true"));

            var result = await context.Repository.GetSettingAsync(SettingCategory.EmailBudgetReminder, ReminderDaysKey, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Setting_Belongs_To_Another_Site()
        {
            using var context = CreateTestContext();

            await context.AddSettingsAsync(CreateSetting(context.OtherSite, EnabledKey, "true"));

            var result = await context.Repository.GetSettingAsync(SettingCategory.EmailBudgetReminder, EnabledKey, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }
    }

    public class GetSettingsForCategoryAsync : SettingsRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Only_Settings_For_The_Requested_Category_And_Current_Site()
        {
            using var context = CreateTestContext();

            await context.AddSettingsAsync(
                CreateSetting(context.Site, EnabledKey, "true"),
                CreateSetting(context.Site, ReminderDaysKey, "7"),
                CreateSetting(context.OtherSite, EnabledKey, "false"));

            var result = await context.Repository.GetSettingsForCategoryAsync(SettingCategory.EmailBudgetReminder, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(2);
            result.ShouldHaveValues(setting => setting.Key, new[] { EnabledKey, ReminderDaysKey });
            result.ShouldAllMatch(setting => setting.Category == SettingCategory.EmailBudgetReminder);
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_No_Settings_Exist_For_The_Category()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetSettingsForCategoryAsync(SettingCategory.EmailBudgetReminder, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }
    }

    private static SettingEntity CreateSetting(SiteEntity site, string key, string value)
    {
        return new SettingEntity
        {
            RowId = Guid.NewGuid(),
            Site = site,
            Category = SettingCategory.EmailBudgetReminder,
            Key = key,
            Value = value
        };
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site);
        var otherSite = EntityFactory.CreateSite("Other Site", "Another Site");

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.Add(otherSite);
        dbContext.SaveChanges();

        var repository = new SettingsRepository(dbContext);

        return new TestContext(dbContext, repository, site, otherSite);
    }
}
