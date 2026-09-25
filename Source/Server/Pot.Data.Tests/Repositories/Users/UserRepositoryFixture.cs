using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Roles;
using Pot.Data.Repositories.Users;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.Users;

public class UserRepositoryFixture
{
    // PotDbContext defaults to QueryTrackingBehavior.NoTrackingWithIdentityResolution, so reading a role
    // produces a detached instance unless a WithTracking() scope is open. Adding a user that references a
    // detached role walks the graph, marks the existing role as inserted, and PostgreSQL rejects it with a
    // duplicate PK_Role violation. The scope may be opened on any repository because every repository shares
    // the scoped DbContext - VerifySignupService relies on exactly that.
    public class Add : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Not_Insert_The_Role_When_The_User_And_Role_Are_Added_Inside_A_Tracking_Scope()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var dbContext = CreateSeededContext();

            var site = EntityFactory.CreateSite(name: $"Tracked Role Site {Guid.NewGuid():N}");
            var user = EntityFactory.CreateUser(site, $"tracked-role-{Guid.NewGuid():N}");

            var userRepository = CreateUserRepository(dbContext, user);
            var roleRepository = new RoleRepository(dbContext);

            using (userRepository.WithTracking())
            {
                var adminRole = await roleRepository.GetByNameAsync(Role.Admin, cancellationToken);

                user.Roles.Add(adminRole);

                userRepository.Add(user);

                // The role already exists, so adding the user must not schedule it for insertion.
                dbContext.Entry(adminRole).State.ShouldNotBe(EntityState.Added);

                _ = await userRepository.SaveAsync(cancellationToken);
            }

            (await dbContext.Roles.CountAsync(cancellationToken)).ShouldBe(1);

            var savedUser = await dbContext.Users
                .Include(item => item.Roles)
                .SingleAsync(item => item.RowId == user.RowId, cancellationToken);

            savedUser.Roles.Count.ShouldBe(1);
            savedUser.Roles.Single().Name.ShouldBe(Role.Admin);
        }

        [Fact]
        public async Task Should_Mark_The_Role_As_Added_When_The_Role_Is_Read_Outside_A_Tracking_Scope()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var dbContext = CreateSeededContext();

            var site = EntityFactory.CreateSite(name: $"Untracked Role Site {Guid.NewGuid():N}");
            var user = EntityFactory.CreateUser(site, $"untracked-role-{Guid.NewGuid():N}");

            var userRepository = CreateUserRepository(dbContext, user);
            var roleRepository = new RoleRepository(dbContext);

            // No tracking scope, so this is a detached copy of an existing row.
            var adminRole = await roleRepository.GetByNameAsync(Role.Admin, cancellationToken);

            user.Roles.Add(adminRole);

            userRepository.Add(user);

            dbContext.Entry(adminRole).State.ShouldBe(EntityState.Added);
        }
    }

    public class GetCurrentUser : UserRepositoryFixture
    {
        [Fact]
        public void Should_Return_The_Current_User()
        {
            using var context = CreateTestContext();

            var result = context.Repository.GetCurrentUser(includeSite: true);

            result.RowId.ShouldBe(context.User.RowId);
            result.Username.ShouldBe(context.User.Username);
        }

        [Fact]
        public void Should_Not_Load_The_Site_When_Not_Requested()
        {
            using var context = CreateTestContext();

            var result = context.Repository.GetCurrentUser(includeSite: false);

            result.Site.ShouldBeNull();
        }

        [Fact]
        public void Should_Load_The_Site_When_Requested()
        {
            using var context = CreateTestContext();

            var result = context.Repository.GetCurrentUser(includeSite: true);

            result.Site.ShouldNotBeNull();
            result.Site.RowId.ShouldBe(context.Site.RowId);
        }

        [Fact]
        public void Should_Throw_When_The_Current_User_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            // The repository is scoped to a user that has not been persisted.
            var unpersistedUser = EntityFactory.CreateUser(context.Site);
            var repository = CreateUserRepository(context.DbContext, unpersistedUser);

            Should.Throw<InvalidOperationException>(() => repository.GetCurrentUser(includeSite: false));
        }
    }

    public class GetEnabledUsersAsync : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Only_Enabled_Users()
        {
            using var context = CreateTestContext();

            var enabledUser = EntityFactory.CreateUser(context.Site, "enabled-user");
            var disabledUser = EntityFactory.CreateUser(context.Site, "disabled-user");
            disabledUser.Status = UserStatus.Disabled;
            var pendingUser = EntityFactory.CreateUser(context.Site, "pending-user");
            pendingUser.Status = UserStatus.Pending;

            await context.AddUsersAsync(enabledUser, disabledUser, pendingUser);

            var result = await context.Repository.GetEnabledUsersAsync(Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(2);
            result.ShouldContain(user => user.Username == context.User.Username);
            result.ShouldContain(user => user.Username == enabledUser.Username);
            result.ShouldNotContain(user => user.Username == disabledUser.Username);
            result.ShouldNotContain(user => user.Username == pendingUser.Username);
        }

        [Fact]
        public async Task Should_Return_User_Details_And_Roles()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetEnabledUsersAsync(Xunit.TestContext.Current.CancellationToken);

            var currentUser = result.Single(user => user.RowId == context.User.RowId);

            currentUser.Username.ShouldBe(context.User.Username);
            currentUser.Email.ShouldBe(context.User.Email);
            currentUser.DisplayName.ShouldBe(context.User.DisplayName);
            currentUser.Status.ShouldBe(nameof(UserStatus.Enabled));
            currentUser.Roles.ShouldContain(nameof(Role.Admin));
            currentUser.Etag.ShouldBeGreaterThan(0);
        }
    }

    public class GetAllForCurrentSiteAsync : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Only_Users_For_The_Current_Site()
        {
            using var context = CreateTestContext();

            var sameSiteUser = EntityFactory.CreateUser(context.Site, "same-site-user");
            var otherSiteUser = EntityFactory.CreateUser(context.OtherSite, "other-site-user");

            await context.AddUsersAsync(sameSiteUser, otherSiteUser);

            var result = await context.Repository.GetAllForCurrentSiteAsync(Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(2);
            result.ShouldContain(user => user.Username == context.User.Username);
            result.ShouldContain(user => user.Username == sameSiteUser.Username);
            result.ShouldNotContain(user => user.Username == otherSiteUser.Username);
        }
    }

    public class GetByUsernameOrDefaultAsync : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_User_When_The_Username_Exists()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByUsernameOrDefaultAsync(context.User.Username, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(context.User.RowId);
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Username_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByUsernameOrDefaultAsync("missing-user", Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }
    }

    public class AuthSessions : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Auth_Sessions()
        {
            using var context = CreateTestContext();

            var session = new AuthSessionEntity
            {
                RowId = Guid.NewGuid(),
                User = context.User,
                RefreshTokenHash = $"hash-{Guid.NewGuid():N}",
                CreatedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddDays(1)
            };

            context.DbContext.Add(session);
            await context.DbContext.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

            var result = await context.Repository.AuthSessions.ToListAsync(Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(session.RowId);
        }
    }

    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public UserRepository Repository { get; }
        public SiteEntity Site { get; }
        public SiteEntity OtherSite { get; }
        public UserEntity User { get; }
        public RoleEntity AdminRole { get; }

        public TestContext(PotDbContext dbContext, UserRepository repository, SiteEntity site, SiteEntity otherSite, UserEntity user, RoleEntity adminRole)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            OtherSite = otherSite;
            User = user;
            AdminRole = adminRole;
        }

        public Task<int> AddUsersAsync(params UserEntity[] users)
        {
            foreach (var user in users)
            {
                DbContext.Add(user);
            }

            return DbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var otherSite = EntityFactory.CreateSite("Other Site", "Another Site");
        var user = EntityFactory.CreateUser(site);

        var adminRole = new RoleEntity
        {
            RowId = Guid.NewGuid(),
            Name = Role.Admin
        };

        user.Roles.Add(adminRole);

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(otherSite);
        dbContext.Add(user);
        dbContext.Add(adminRole);
        dbContext.SaveChanges();

        var repository = new UserRepository(dbContext, currentUserContext);

        return new TestContext(dbContext, repository, site, otherSite, user, adminRole);
    }

    private static PotDbContext CreateSeededContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        var site = EntityFactory.CreateSite(name: $"Seed Site {Guid.NewGuid():N}");
        var user = EntityFactory.CreateUser(site, $"seed-{Guid.NewGuid():N}");

        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        // Mirrors the Admin role created by the AddRolesAndPermissions migration.
        var adminRole = new RoleEntity
        {
            RowId = Guid.NewGuid(),
            Name = Role.Admin
        };

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.Add(adminRole);
        dbContext.SaveChanges();

        // Detach everything so the role is an existing row that has to be read back, which is how a
        // fresh repository scope sees it.
        dbContext.ChangeTracker.Clear();

        return dbContext;
    }

    private static UserRepository CreateUserRepository(PotDbContext dbContext, UserEntity user)
    {
        var currentUserContext = Substitute.For<ICurrentUserContext>();

        currentUserContext.UserRowId.Returns(user.RowId);

        return new UserRepository(dbContext, currentUserContext);
    }
}
