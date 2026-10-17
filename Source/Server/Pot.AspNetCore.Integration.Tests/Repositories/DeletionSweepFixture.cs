using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.Sites;
using Pot.Data.Repositories.Sites.Dtos;
using Pot.Data.Repositories.Users;
using Pot.Data.Repositories.Users.Dtos;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Repositories;

/// <summary>
/// Proves the ordered deletion sweeps against a real PostgreSQL database.
/// </summary>
/// <remarks>
/// Every foreign key on an entity is <c>Restrict</c>, so any ordering other than the one implemented is
/// rejected by the database. An in-memory provider cannot prove that, which is why these cases live here.
/// </remarks>
public class DeletionSweepFixture : IntegrationFixtureBase
{
    [Fact]
    public async Task Should_Remove_A_User_With_Sessions_And_One_Time_Codes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = EntityFactory.CreateSite($"User Sweep Site {Guid.NewGuid():N}");
        var caller = EntityFactory.CreateUser(site, $"caller-{Guid.NewGuid():N}");
        var target = EntityFactory.CreateUser(site, $"target-{Guid.NewGuid():N}");
        var colleague = EntityFactory.CreateUser(site, $"colleague-{Guid.NewGuid():N}");

        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

            dbContext.AddRange(site, caller, target, colleague);
            await dbContext.SaveChangesAsync(cancellationToken);

            var linkedOtp = CreateOtp(target.Username);

            dbContext.AddRange(
                CreateSession(target),
                CreateSession(target, revokedUtc: DateTime.UtcNow.AddMinutes(-5)),
                CreateSession(colleague),
                CreateOtp(target.Username),
                CreateOtp(colleague.Username),
                linkedOtp);

            // The one-time password has no user property, only the relationship, so the link is set through
            // the shadow foreign key after the row is tracked.
            dbContext.Entry(linkedOtp).Property<int?>("UserId").CurrentValue = target.Id;

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        UserDeletionCounts? counts;

        using (var scope = CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<ICurrentUserContext>().SetUserRowId(caller.RowId);

            var repository = scope.ServiceProvider.GetRequiredService<IPersistableUserRepository>();

            counts = await repository.DeleteUserWithDependentsAsync(target.RowId, cancellationToken);
        }

        counts.ShouldNotBeNull();
        counts!.Sessions.ShouldBe(2);
        counts.OneTimePasswords.ShouldBe(2);

        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

            (await dbContext.Users.AnyAsync(user => user.RowId == target.RowId, cancellationToken)).ShouldBeFalse();
            (await dbContext.AuthSessions.AnyAsync(session => session.UserId == target.Id, cancellationToken)).ShouldBeFalse();
            (await dbContext.OneTimePasswords.AnyAsync(otp => otp.Username == target.Username, cancellationToken)).ShouldBeFalse();

            (await dbContext.Users.AnyAsync(user => user.RowId == colleague.RowId, cancellationToken)).ShouldBeTrue();
            (await dbContext.AuthSessions.AnyAsync(session => session.UserId == colleague.Id, cancellationToken)).ShouldBeTrue();
            (await dbContext.OneTimePasswords.AnyAsync(otp => otp.Username == colleague.Username, cancellationToken)).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Should_Not_Remove_A_User_From_Another_Site()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var callerSite = EntityFactory.CreateSite($"Caller Site {Guid.NewGuid():N}");
        var caller = EntityFactory.CreateUser(callerSite, $"caller-{Guid.NewGuid():N}");
        var targetSite = EntityFactory.CreateSite($"Target Site {Guid.NewGuid():N}");
        var target = EntityFactory.CreateUser(targetSite, $"target-{Guid.NewGuid():N}");

        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

            dbContext.AddRange(callerSite, caller, targetSite, target);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        using (var scope = CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<ICurrentUserContext>().SetUserRowId(caller.RowId);

            var repository = scope.ServiceProvider.GetRequiredService<IPersistableUserRepository>();

            var counts = await repository.DeleteUserWithDependentsAsync(target.RowId, cancellationToken);

            counts.ShouldBeNull();
        }

        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

            (await dbContext.Users.AnyAsync(user => user.RowId == target.RowId, cancellationToken)).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Should_Remove_A_Site_With_Every_Dependent_Record()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = EntityFactory.CreateSite($"Sweep Site {Guid.NewGuid():N}");
        var caller = EntityFactory.CreateUser(site, $"caller-{Guid.NewGuid():N}");
        var colleague = EntityFactory.CreateUser(site, $"colleague-{Guid.NewGuid():N}");
        var otherSite = EntityFactory.CreateSite($"Other Sweep Site {Guid.NewGuid():N}");
        var otherUser = EntityFactory.CreateUser(otherSite, $"other-{Guid.NewGuid():N}");

        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

            AddSiteGraph(dbContext, site, caller, colleague);
            AddSiteGraph(dbContext, otherSite, otherUser);

            await dbContext.SaveChangesAsync(cancellationToken);

            var linkedOtp = CreateOtp(colleague.Username);

            dbContext.AddRange(
                CreateSession(caller),
                CreateSession(colleague),
                CreateSession(otherUser),
                CreateOtp(caller.Username),
                CreateOtp(otherUser.Username),
                linkedOtp);

            dbContext.Entry(linkedOtp).Property<int?>("UserId").CurrentValue = colleague.Id;

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        SiteDeletionCounts? counts;

        using (var scope = CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<ICurrentUserContext>().SetUserRowId(caller.RowId);

            var repository = scope.ServiceProvider.GetRequiredService<IPersistableSiteRepository>();

            counts = await repository.DeleteCurrentSiteWithDependentsAsync(cancellationToken);
        }

        counts.ShouldNotBeNull();
        counts!.Expenses.ShouldBe(1);
        counts.Incomes.ShouldBe(1);
        counts.Accounts.ShouldBe(1);
        counts.Settings.ShouldBe(1);
        counts.OneTimePasswords.ShouldBe(2);
        counts.Sessions.ShouldBe(2);
        counts.Users.ShouldBe(2);

        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

            (await dbContext.Set<SiteEntity>().AnyAsync(item => item.RowId == site.RowId, cancellationToken)).ShouldBeFalse();
            (await dbContext.Users.AnyAsync(user => user.Site.Id == site.Id, cancellationToken)).ShouldBeFalse();
            (await dbContext.Accounts.IgnoreQueryFilters().AnyAsync(account => account.Site.Id == site.Id, cancellationToken)).ShouldBeFalse();
            (await dbContext.Expenses.IgnoreQueryFilters().AnyAsync(expense => expense.Account.Site.Id == site.Id, cancellationToken)).ShouldBeFalse();
            (await dbContext.Incomes.IgnoreQueryFilters().AnyAsync(income => income.Account.Site.Id == site.Id, cancellationToken)).ShouldBeFalse();
            (await dbContext.Settings.IgnoreQueryFilters().AnyAsync(setting => setting.Site.Id == site.Id, cancellationToken)).ShouldBeFalse();
            (await dbContext.AuthSessions.AnyAsync(session => session.UserId == caller.Id, cancellationToken)).ShouldBeFalse();
            (await dbContext.OneTimePasswords.AnyAsync(otp => otp.Username == caller.Username, cancellationToken)).ShouldBeFalse();

            (await dbContext.Set<SiteEntity>().AnyAsync(item => item.RowId == otherSite.RowId, cancellationToken)).ShouldBeTrue();
            (await dbContext.Users.AnyAsync(user => user.RowId == otherUser.RowId, cancellationToken)).ShouldBeTrue();
            (await dbContext.Accounts.IgnoreQueryFilters().AnyAsync(account => account.Site.Id == otherSite.Id, cancellationToken)).ShouldBeTrue();
            (await dbContext.Expenses.IgnoreQueryFilters().AnyAsync(expense => expense.Account.Site.Id == otherSite.Id, cancellationToken)).ShouldBeTrue();
            (await dbContext.Incomes.IgnoreQueryFilters().AnyAsync(income => income.Account.Site.Id == otherSite.Id, cancellationToken)).ShouldBeTrue();
            (await dbContext.Settings.IgnoreQueryFilters().AnyAsync(setting => setting.Site.Id == otherSite.Id, cancellationToken)).ShouldBeTrue();
            (await dbContext.AuthSessions.AnyAsync(session => session.UserId == otherUser.Id, cancellationToken)).ShouldBeTrue();
            (await dbContext.OneTimePasswords.AnyAsync(otp => otp.Username == otherUser.Username, cancellationToken)).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Should_Count_Only_Enabled_Platform_Administrators()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = EntityFactory.CreateSite($"Platform Admin Site {Guid.NewGuid():N}");
        var caller = EntityFactory.CreateUser(site, $"caller-{Guid.NewGuid():N}");
        var enabled = EntityFactory.CreateUser(site, $"enabled-{Guid.NewGuid():N}");
        var disabled = EntityFactory.CreateUser(site, $"disabled-{Guid.NewGuid():N}");
        disabled.Status = UserStatus.Disabled;

        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

            dbContext.AddRange(site, caller, enabled, disabled);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        using (var scope = CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<ICurrentUserContext>().SetUserRowId(caller.RowId);

            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var configuredRowIds = new[] { enabled.RowId, disabled.RowId, Guid.NewGuid() };

            var count = await repository.CountEnabledPlatformAdminsAsync(configuredRowIds, cancellationToken);

            count.ShouldBe(1);
        }
    }

    private static void AddSiteGraph(PotDbContext dbContext, SiteEntity site, params UserEntity[] users)
    {
        var account = EntityFactory.CreateAccount(site, $"{site.Name} Account", 100);

        var expense = EntityFactory.CreateExpense(account, excludeFromCalc: false, "Expense", 10,
            "2026-01-01", "2026-02-01", null, Frequency.Months, 1);

        var income = EntityFactory.CreateIncome(account, excludeFromCalc: false, "Income", 10,
            "2026-01-01", null, Frequency.Months, 1);

        var setting = new SettingEntity
        {
            RowId = Guid.NewGuid(),
            Category = SettingCategory.EmailBudgetReminder,
            Key = "theme",
            Value = "dark",
            Site = site
        };

        dbContext.Add(site);
        dbContext.AddRange(users);
        dbContext.AddRange(account, expense, income, setting);
    }

    private static AuthSessionEntity CreateSession(UserEntity user, DateTime? revokedUtc = null)
    {
        return new AuthSessionEntity
        {
            RowId = Guid.NewGuid(),
            UserId = user.Id,
            RefreshTokenHash = Guid.NewGuid().ToString("N"),
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresUtc = DateTime.UtcNow.AddDays(7),
            RevokedUtc = revokedUtc
        };
    }

    private static OneTimePasswordEntity CreateOtp(string username)
    {
        return new OneTimePasswordEntity
        {
            RowId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString("N"),
            Username = username,
            Email = $"{username}@example.com",
            Reason = OtpReason.Signup,
            RefCode = "123456",
            OtpCode = "654321",
            Status = OtpStatus.Active,
            CreatedUtc = DateTime.UtcNow,
            ExpiryUtc = DateTime.UtcNow.AddMinutes(10)
        };
    }
}
