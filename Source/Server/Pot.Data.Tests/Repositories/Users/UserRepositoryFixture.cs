using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    // One substitute per instance is enough: xUnit constructs a new fixture for every test method, so the
    // received-call list cannot leak between tests. It does mean a test that asserts on log output must not
    // also exercise TestContext.Repository, because those calls would land in the same list.
    private readonly ILogger<UserRepository> _logger;

    public UserRepositoryFixture()
    {
        _logger = Substitute.For<ILogger<UserRepository>>();
    }

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

    public class GetEnabledUsersForAllSitesAsync : UserRepositoryFixture
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

            var result = await context.Repository.GetEnabledUsersForAllSitesAsync(Xunit.TestContext.Current.CancellationToken);

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

            var result = await context.Repository.GetEnabledUsersForAllSitesAsync(Xunit.TestContext.Current.CancellationToken);

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

    public class GetByUsernameAsync : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_User_When_The_Username_Exists()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByUsernameAsync(context.User.Username, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(context.User.RowId);
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Username_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByUsernameAsync("missing-user", Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }
    }

    public class GetByUsernameWithSiteAsync : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_User_With_Its_Site()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByUsernameWithSiteAsync(context.User.Username, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(context.User.RowId);
            result.Site.ShouldNotBeNull();
            result.Site.RowId.ShouldBe(context.Site.RowId);
        }

        [Fact]
        public async Task Should_Resolve_A_User_In_Another_Site()
        {
            using var context = CreateTestContext();

            var otherSiteUser = EntityFactory.CreateUser(context.OtherSite, "other-site-user");

            await context.AddUsersAsync(otherSiteUser);

            var result = await context.Repository.GetByUsernameWithSiteAsync(otherSiteUser.Username, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.Site.RowId.ShouldBe(context.OtherSite.RowId);
        }

        [Fact]
        public async Task Should_Return_Null_When_The_Username_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByUsernameWithSiteAsync("missing-user", Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }
    }

    public class GetForCurrentSiteAsync : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_A_User_In_The_Current_Site()
        {
            using var context = CreateTestContext();

            var sameSiteUser = EntityFactory.CreateUser(context.Site, "same-site-user");

            await context.AddUsersAsync(sameSiteUser);

            var result = await context.Repository.GetForCurrentSiteAsync(sameSiteUser.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(sameSiteUser.RowId);
        }

        [Fact]
        public async Task Should_Return_Null_For_A_User_In_Another_Site()
        {
            using var context = CreateTestContext();

            var otherSiteUser = EntityFactory.CreateUser(context.OtherSite, "other-site-user");

            await context.AddUsersAsync(otherSiteUser);

            var result = await context.Repository.GetForCurrentSiteAsync(otherSiteUser.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_Null_For_A_User_That_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetForCurrentSiteAsync(Guid.NewGuid(), Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }
    }

    public class GetEnabledAdminsForCurrentSiteAsync : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Only_Enabled_Administrators_In_The_Current_Site()
        {
            using var context = CreateTestContext();

            var disabledAdmin = EntityFactory.CreateUser(context.Site, "disabled-admin");
            disabledAdmin.Status = UserStatus.Disabled;
            disabledAdmin.Roles.Add(context.AdminRole);

            var viewerRole = new RoleEntity
            {
                RowId = Guid.NewGuid(),
                Name = Role.Viewer
            };

            var viewer = EntityFactory.CreateUser(context.Site, "viewer");
            viewer.Roles.Add(viewerRole);

            var otherSiteAdmin = EntityFactory.CreateUser(context.OtherSite, "other-site-admin");
            otherSiteAdmin.Roles.Add(context.AdminRole);

            await context.AddUsersAsync(disabledAdmin, viewer, otherSiteAdmin);

            var result = await context.Repository.GetEnabledAdminsForCurrentSiteAsync(Xunit.TestContext.Current.CancellationToken);

            // The seeded caller is the current site's only enabled administrator: a disabled administrator,
            // a viewer, and another site's administrator all have to be excluded.
            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(context.User.RowId);
        }
    }

    public class CrossTenantRefusalLogging : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Log_A_Warning_When_The_Row_Exists_In_Another_Site()
        {
            using var context = CreateTestContext();

            var otherSiteUser = EntityFactory.CreateUser(context.OtherSite, "other-site-user");

            await context.AddUsersAsync(otherSiteUser);

            var repository = CreateUserRepository(context.DbContext, context.User);

            var result = await repository.GetForCurrentSiteAsync(otherSiteUser.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();

            var warning = GetWarningMessages().ShouldContainSingle(
                message => message.Contains(otherSiteUser.RowId.ToString(), StringComparison.OrdinalIgnoreCase));

            // The record has to name the caller as well as the requested row, or it cannot be acted on.
            warning.ShouldContain(context.User.RowId.ToString(), Case.Insensitive);
        }

        [Fact]
        public async Task Should_Not_Log_When_The_Row_Does_Not_Exist_At_All()
        {
            using var context = CreateTestContext();

            var repository = CreateUserRepository(context.DbContext, context.User);

            var result = await repository.GetForCurrentSiteAsync(Guid.NewGuid(), Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();

            GetWarningMessages().ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_Not_Log_When_The_Row_Belongs_To_The_Callers_Site()
        {
            using var context = CreateTestContext();

            var sameSiteUser = EntityFactory.CreateUser(context.Site, "same-site-user");

            await context.AddUsersAsync(sameSiteUser);

            var repository = CreateUserRepository(context.DbContext, context.User);

            var result = await repository.GetForCurrentSiteAsync(sameSiteUser.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();

            GetWarningMessages().ShouldBeEmpty();
        }

        private List<string> GetWarningMessages()
        {
            var warnings = new List<string>();

            foreach (var call in _logger.ReceivedCalls())
            {
                var arguments = call.GetArguments();

                if (arguments.Length == 5 && arguments[0] is LogLevel level && level == LogLevel.Warning)
                {
                    warnings.Add(arguments[2]?.ToString() ?? string.Empty);
                }
            }

            return warnings;
        }
    }

    public class CrossTenantReads : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Approvals_From_Another_Site()
        {
            using var context = CreateTestContext();

            var otherSiteApproval = EntityFactory.CreateUser(context.OtherSite, "other-site-approval");
            otherSiteApproval.Status = UserStatus.Approval;

            await context.AddUsersAsync(otherSiteApproval);

            var result = await context.Repository.GetPendingApprovalsAsync(Xunit.TestContext.Current.CancellationToken);

            result.Select(user => user.RowId).ShouldContain(otherSiteApproval.RowId);
        }

        [Fact]
        public async Task Should_Not_Return_Users_That_Are_Not_Awaiting_Approval()
        {
            using var context = CreateTestContext();

            // The seeded caller is enabled, so it is not part of the approval population.
            var result = await context.Repository.GetPendingApprovalsAsync(Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotContain(user => user.RowId == context.User.RowId);
        }

        [Fact]
        public async Task Should_Return_A_User_By_Id_From_Another_Site()
        {
            using var context = CreateTestContext();

            var otherSiteUser = EntityFactory.CreateUser(context.OtherSite, "other-site-user");

            await context.AddUsersAsync(otherSiteUser);

            var result = await context.Repository.GetByIdForApprovalAsync(otherSiteUser.RowId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldNotBeNull();
            result.RowId.ShouldBe(otherSiteUser.RowId);
        }

        [Fact]
        public async Task Should_Return_Only_The_Requested_Notification_Recipients()
        {
            using var context = CreateTestContext();

            var otherSiteUser = EntityFactory.CreateUser(context.OtherSite, "other-site-recipient");
            var unrelatedUser = EntityFactory.CreateUser(context.Site, "unrelated-recipient");

            await context.AddUsersAsync(otherSiteUser, unrelatedUser);

            var result = await context.Repository
                .GetByIdsForPlatformNotificationAsync([otherSiteUser.RowId], Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(otherSiteUser.RowId);
        }
    }

    public class WithoutACurrentUser : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Refuse_A_Site_Scoped_Lookup_Rather_Than_Throw()
        {
            using var context = CreateContextWithoutACurrentUser();

            var result = await context.Repository
                .GetForCurrentSiteAsync(context.SeededUserId, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_No_Users_In_The_Current_Site()
        {
            using var context = CreateContextWithoutACurrentUser();

            var result = await context.Repository.GetAllForCurrentSiteAsync(Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_Return_No_Administrators_In_The_Current_Site()
        {
            using var context = CreateContextWithoutACurrentUser();

            var result = await context.Repository.GetEnabledAdminsForCurrentSiteAsync(Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }

        private sealed class NoUserContext : IDisposable
        {
            public PotDbContext DbContext { get; }
            public UserRepository Repository { get; }
            public Guid SeededUserId { get; }

            public NoUserContext(PotDbContext dbContext, UserRepository repository, Guid seededUserId)
            {
                DbContext = dbContext;
                Repository = repository;
                SeededUserId = seededUserId;
            }

            public void Dispose()
            {
                DbContext.Dispose();
            }
        }

        /// <summary>
        /// Seeds a row that could be reached if a site were resolved, and a context that reports no user at all.
        /// The migration worker reports an empty identifier, so the site-scoped reads have to refuse rather than
        /// throw.
        /// </summary>
        private NoUserContext CreateContextWithoutACurrentUser()
        {
            var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var site = EntityFactory.CreateSite();
            var adminRole = new RoleEntity
            {
                RowId = Guid.NewGuid(),
                Name = Role.Admin
            };

            var user = EntityFactory.CreateUser(site, "seeded-without-a-context");
            user.Roles.Add(adminRole);

            var currentUserContext = Substitute.For<ICurrentUserContext>();
            currentUserContext.UserRowId.Returns(Guid.Empty);

            var dbContext = new PotDbContext(dbOptions, currentUserContext);

            dbContext.Add(site);
            dbContext.Add(adminRole);
            dbContext.Add(user);
            dbContext.SaveChanges();

            var repository = new UserRepository(dbContext, currentUserContext, _logger);

            return new NoUserContext(dbContext, repository, user.RowId);
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

    private TestContext CreateTestContext()
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

        var repository = new UserRepository(dbContext, currentUserContext, _logger);

        return new TestContext(dbContext, repository, site, otherSite, user, adminRole);
    }

    public class GetSecurityState : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Report_The_Expiry_And_No_Revocation_When_The_Session_Is_Active()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var dbContext = CreateSeededContext();

            var user = dbContext.Users.Single();
            var session = CreateAuthSession(user, expiresUtc: DateTime.UtcNow.AddDays(1));

            dbContext.AuthSessions.Add(session);
            await dbContext.SaveChangesAsync(cancellationToken);

            var repository = CreateUserRepository(dbContext, user);

            var securityState = await repository.GetSecurityStateAsync(user.RowId, session.RowId, cancellationToken);

            securityState.ShouldNotBeNull();
            securityState.TokenVersion.ShouldBe(user.TokenVersion);
            securityState.SessionExpiresUtc.ShouldBe(session.ExpiresUtc);
            securityState.SessionRevokedUtc.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Report_The_Revoked_Time_When_The_Session_Was_Revoked()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var dbContext = CreateSeededContext();

            var user = dbContext.Users.Single();
            var revokedUtc = DateTime.UtcNow.AddMinutes(-5);
            var session = CreateAuthSession(user, expiresUtc: DateTime.UtcNow.AddDays(1), revokedUtc);

            dbContext.AuthSessions.Add(session);
            await dbContext.SaveChangesAsync(cancellationToken);

            var repository = CreateUserRepository(dbContext, user);

            var securityState = await repository.GetSecurityStateAsync(user.RowId, session.RowId, cancellationToken);

            securityState.ShouldNotBeNull();
            securityState.SessionExpiresUtc.ShouldBe(session.ExpiresUtc);
            securityState.SessionRevokedUtc.ShouldBe(revokedUtc);
        }

        [Fact]
        public async Task Should_Report_No_Session_Expiry_When_The_Session_Does_Not_Exist()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var dbContext = CreateSeededContext();

            var user = dbContext.Users.Single();
            var repository = CreateUserRepository(dbContext, user);

            var securityState = await repository.GetSecurityStateAsync(user.RowId, Guid.NewGuid(), cancellationToken);

            securityState.ShouldNotBeNull();

            // A null expiry is how the token pipeline recognises a session that no longer exists, so the
            // projection must not fall back to default(DateTime).
            securityState.SessionExpiresUtc.ShouldBeNull();
            securityState.SessionRevokedUtc.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Not_Report_Another_Users_Session_When_The_Token_Names_It()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var dbContext = CreateSeededContext();

            var user = dbContext.Users.Single();
            var otherUser = EntityFactory.CreateUser(user.Site, $"other-{Guid.NewGuid():N}");
            var otherUsersSession = CreateAuthSession(otherUser, expiresUtc: DateTime.UtcNow.AddDays(1));

            dbContext.Users.Add(otherUser);
            dbContext.AuthSessions.Add(otherUsersSession);
            await dbContext.SaveChangesAsync(cancellationToken);

            var repository = CreateUserRepository(dbContext, user);

            var securityState = await repository.GetSecurityStateAsync(user.RowId, otherUsersSession.RowId, cancellationToken);

            securityState.ShouldNotBeNull();

            // The session lookup is scoped to the subject's own sessions, so another user's session is
            // indistinguishable from one that does not exist.
            securityState.SessionExpiresUtc.ShouldBeNull();
            securityState.SessionRevokedUtc.ShouldBeNull();
        }

        private static AuthSessionEntity CreateAuthSession(UserEntity user, DateTime expiresUtc, DateTime? revokedUtc = null)
        {
            var session = new AuthSessionEntity
            {
                RowId = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenHash = Guid.NewGuid().ToString("N"),
                CreatedUtc = DateTime.UtcNow.AddDays(-1),
                ExpiresUtc = expiresUtc,
                RevokedUtc = revokedUtc
            };

            return session;
        }
    }

    public class CountEnabledPlatformAdminsAsync : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Count_Only_Configured_Users_That_Are_Enabled()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var context = CreateDeletionTestContext();

            var enabled = EntityFactory.CreateUser(context.Site, $"enabled-{Guid.NewGuid():N}");
            var disabled = EntityFactory.CreateUser(context.Site, $"disabled-{Guid.NewGuid():N}");
            disabled.Status = UserStatus.Disabled;

            await context.SeedAsync(enabled, disabled);

            var configuredRowIds = new[]
            {
                context.Caller.RowId,
                enabled.RowId,
                disabled.RowId,
                Guid.NewGuid()
            };

            var result = await context.Repository.CountEnabledPlatformAdminsAsync(configuredRowIds, cancellationToken);

            result.ShouldBe(2);
        }
    }

    public class DeleteUserWithDependentsAsync : UserRepositoryFixture
    {
        [Fact]
        public async Task Should_Remove_The_User_With_All_Sessions_And_One_Time_Passwords()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var context = CreateDeletionTestContext();

            var activeSession = CreateSession(context.Target);
            var revokedSession = CreateSession(context.Target, revokedUtc: DateTime.UtcNow.AddMinutes(-5));
            var linkedOtp = CreateOtp(context.Target.Username, OtpReason.PasswordReset);
            var signupOtp = CreateOtp(context.Target.Username, OtpReason.Signup);

            await context.SeedAsync(activeSession, revokedSession);
            await context.SeedOtpAsync(linkedOtp, context.Target);
            await context.SeedAsync(signupOtp);

            var result = await context.Repository.DeleteUserWithDependentsAsync(context.Target.RowId, cancellationToken);

            result.ShouldNotBeNull();
            result!.Sessions.ShouldBe(2);
            result.OneTimePasswords.ShouldBe(2);

            var userRemains = await context.DbContext.Users.AnyAsync(user => user.RowId == context.Target.RowId, cancellationToken);
            var sessionsRemain = await context.DbContext.AuthSessions.AnyAsync(session => session.UserId == context.Target.Id, cancellationToken);
            var otpsRemain = await context.DbContext.OneTimePasswords.AnyAsync(otp => otp.Username == context.Target.Username, cancellationToken);

            userRemains.ShouldBeFalse();
            sessionsRemain.ShouldBeFalse();
            otpsRemain.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Leave_Other_Users_And_Their_Records_Untouched()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var context = CreateDeletionTestContext();

            var colleagueSession = CreateSession(context.Colleague);
            var colleagueOtp = CreateOtp(context.Colleague.Username, OtpReason.Signup);
            var foreignSession = CreateSession(context.ForeignUser);

            await context.SeedAsync(colleagueSession, colleagueOtp, foreignSession);

            var result = await context.Repository.DeleteUserWithDependentsAsync(context.Target.RowId, cancellationToken);

            result.ShouldNotBeNull();

            (await context.DbContext.Users.AnyAsync(user => user.RowId == context.Colleague.RowId, cancellationToken)).ShouldBeTrue();
            (await context.DbContext.AuthSessions.AnyAsync(session => session.UserId == context.Colleague.Id, cancellationToken)).ShouldBeTrue();
            (await context.DbContext.OneTimePasswords.AnyAsync(otp => otp.Username == context.Colleague.Username, cancellationToken)).ShouldBeTrue();
            (await context.DbContext.Users.AnyAsync(user => user.RowId == context.ForeignUser.RowId, cancellationToken)).ShouldBeTrue();
            (await context.DbContext.AuthSessions.AnyAsync(session => session.UserId == context.ForeignUser.Id, cancellationToken)).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Return_Not_Found_When_The_Target_Belongs_To_Another_Site()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var context = CreateDeletionTestContext();

            var result = await context.Repository.DeleteUserWithDependentsAsync(context.ForeignUser.RowId, cancellationToken);

            result.ShouldBeNull();

            (await context.DbContext.Users.AnyAsync(user => user.RowId == context.ForeignUser.RowId, cancellationToken)).ShouldBeTrue();
        }
    }

    private sealed class DeletionTestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public UserRepository Repository { get; }
        public SiteEntity Site { get; }
        public UserEntity Caller { get; }
        public UserEntity Target { get; }
        public UserEntity Colleague { get; }
        public SiteEntity OtherSite { get; }
        public UserEntity ForeignUser { get; }

        public DeletionTestContext(PotDbContext dbContext, UserRepository repository, SiteEntity site, UserEntity caller,
            UserEntity target, UserEntity colleague, SiteEntity otherSite, UserEntity foreignUser)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            Caller = caller;
            Target = target;
            Colleague = colleague;
            OtherSite = otherSite;
            ForeignUser = foreignUser;
        }

        public Task<int> SeedAsync(params EntityBase[] entities)
        {
            // A user references a site that was seeded earlier and has since been detached. Attaching that
            // site first stops the add from walking the graph and inserting a second copy of it.
            foreach (var user in entities.OfType<UserEntity>())
            {
                if (user.Site is not null && DbContext.Entry(user.Site).State == EntityState.Detached)
                {
                    DbContext.Attach(user.Site);
                }
            }

            DbContext.AddRange(entities);

            return SaveAndDetachAsync();
        }

        public Task<int> SeedOtpAsync(OneTimePasswordEntity otp, UserEntity user)
        {
            DbContext.Add(otp);

            // The one-time password has no user property, only the relationship, so the link is set through
            // the shadow foreign key after the row is tracked.
            DbContext.Entry(otp).Property<int?>("UserId").CurrentValue = user.Id;

            return SaveAndDetachAsync();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }

        private async Task<int> SaveAndDetachAsync()
        {
            var result = await DbContext.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

            DbContext.ChangeTracker.Clear();

            return result;
        }
    }

    private static DeletionTestContext CreateDeletionTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite(name: $"Deletion Site {Guid.NewGuid():N}");
        var otherSite = EntityFactory.CreateSite(name: $"Other Deletion Site {Guid.NewGuid():N}");
        var caller = EntityFactory.CreateUser(site, $"caller-{Guid.NewGuid():N}");
        var target = EntityFactory.CreateUser(site, $"target-{Guid.NewGuid():N}");
        var colleague = EntityFactory.CreateUser(site, $"colleague-{Guid.NewGuid():N}");
        var foreignUser = EntityFactory.CreateUser(otherSite, $"foreign-{Guid.NewGuid():N}");

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(caller.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.AddRange(site, otherSite, caller, target, colleague, foreignUser);
        dbContext.SaveChanges();

        // Detach the seeded rows so the repository sees them exactly as a fresh request scope would.
        dbContext.ChangeTracker.Clear();

        var repository = new UserRepository(dbContext, currentUserContext, Substitute.For<ILogger<UserRepository>>());

        return new DeletionTestContext(dbContext, repository, site, caller, target, colleague, otherSite, foreignUser);
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

    private static OneTimePasswordEntity CreateOtp(string username, OtpReason reason)
    {
        return new OneTimePasswordEntity
        {
            RowId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString("N"),
            Username = username,
            Email = $"{username}@example.com",
            Reason = reason,
            RefCode = "123456",
            OtpCode = "654321",
            Status = OtpStatus.Active,
            CreatedUtc = DateTime.UtcNow,
            ExpiryUtc = DateTime.UtcNow.AddMinutes(10)
        };
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

    private UserRepository CreateUserRepository(PotDbContext dbContext, UserEntity user)
    {
        var currentUserContext = Substitute.For<ICurrentUserContext>();

        currentUserContext.UserRowId.Returns(user.RowId);

        return new UserRepository(dbContext, currentUserContext, _logger);
    }
}
