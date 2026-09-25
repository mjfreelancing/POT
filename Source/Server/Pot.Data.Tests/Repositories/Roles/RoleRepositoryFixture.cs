using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Roles;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.Roles;

public class RoleRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public RoleRepository Repository { get; }
        public SiteEntity Site { get; }
        public UserEntity User { get; }
        public UserEntity SecondUser { get; }
        public RoleEntity AdminRole { get; }
        public RoleEntity ViewerRole { get; }
        public PermissionEntity ManagePermission { get; }
        public PermissionEntity ViewPermission { get; }

        public TestContext(PotDbContext dbContext, RoleRepository repository, SiteEntity site, UserEntity user, UserEntity secondUser,
            RoleEntity adminRole, RoleEntity viewerRole, PermissionEntity managePermission, PermissionEntity viewPermission)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            User = user;
            SecondUser = secondUser;
            AdminRole = adminRole;
            ViewerRole = viewerRole;
            ManagePermission = managePermission;
            ViewPermission = viewPermission;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class GetByNameAsync : RoleRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Role_When_It_Exists()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetByNameAsync(Role.Admin, Xunit.TestContext.Current.CancellationToken);

            result.RowId.ShouldBe(context.AdminRole.RowId);
            result.Name.ShouldBe(Role.Admin);
        }

        [Fact]
        public async Task Should_Throw_When_The_Role_Does_Not_Exist()
        {
            using var context = CreateTestContext();

            context.DbContext.Remove(context.ViewerRole);
            await context.DbContext.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

            await Should.ThrowAsync<InvalidOperationException>(
                () => context.Repository.GetByNameAsync(Role.Viewer, Xunit.TestContext.Current.CancellationToken));
        }
    }

    public class GetRolesAsync : RoleRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Only_The_Requested_Roles()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetRolesAsync([context.ViewerRole.RowId], Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(context.ViewerRole.RowId);
        }

        [Fact]
        public async Task Should_Return_All_Requested_Roles()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetRolesAsync(
                [context.AdminRole.RowId, context.ViewerRole.RowId],
                Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(2);
            result.ShouldHaveValues(role => role.Name, new[] { Role.Admin, Role.Viewer });
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_No_Roles_Match()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetRolesAsync([Guid.NewGuid()], Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }
    }

    public class GetRolesForUserAsync : RoleRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Roles_Assigned_To_The_User()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetRolesForUserAsync(context.User.RowId, includePermissions: false, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(context.AdminRole.RowId);
        }

        [Fact]
        public async Task Should_Return_The_Roles_For_The_Requested_User_Only()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetRolesForUserAsync(context.SecondUser.RowId, includePermissions: false, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(context.ViewerRole.RowId);
        }

        [Fact]
        public async Task Should_Include_Permissions_When_Requested()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetRolesForUserAsync(context.User.RowId, includePermissions: true, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(context.AdminRole.RowId);

            var permissions = result[0].Permissions;
            permissions.Count.ShouldBe(2);
            permissions.ShouldHaveValues(permission => permission.Name, new[] { Permission.SiteManage, Permission.SiteView });
        }

        [Fact]
        public async Task Should_Not_Include_Permissions_When_Not_Requested()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetRolesForUserAsync(context.User.RowId, includePermissions: false, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].Permissions.ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_The_User_Has_No_Roles()
        {
            using var context = CreateTestContext();

            var result = await context.Repository.GetRolesForUserAsync(Guid.NewGuid(), includePermissions: true, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site, "admin-user");
        var secondUser = EntityFactory.CreateUser(site, "viewer-user");

        var adminRole = new RoleEntity { RowId = Guid.NewGuid(), Name = Role.Admin };
        var viewerRole = new RoleEntity { RowId = Guid.NewGuid(), Name = Role.Viewer };

        var managePermission = new PermissionEntity { RowId = Guid.NewGuid(), Name = Permission.SiteManage };
        var viewPermission = new PermissionEntity { RowId = Guid.NewGuid(), Name = Permission.SiteView };

        adminRole.Permissions.Add(managePermission);
        adminRole.Permissions.Add(viewPermission);

        user.Roles.Add(adminRole);
        secondUser.Roles.Add(viewerRole);

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.Add(secondUser);
        dbContext.Add(adminRole);
        dbContext.Add(viewerRole);
        dbContext.Add(managePermission);
        dbContext.Add(viewPermission);
        dbContext.SaveChanges();

        var repository = new RoleRepository(dbContext);

        return new TestContext(dbContext, repository, site, user, secondUser, adminRole, viewerRole, managePermission, viewPermission);
    }
}
