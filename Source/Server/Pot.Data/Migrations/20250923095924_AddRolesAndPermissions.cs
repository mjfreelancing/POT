using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Create roles
                    INSERT INTO ""Role"" (""Name"", ""RowId"", ""Etag"")
                    VALUES
                        ('Admin', gen_random_uuid(), 1),
                        ('Viewer', gen_random_uuid(), 1);

                    -- Create permissions
                    INSERT INTO ""Permission"" (""Name"", ""RowId"", ""Etag"")
                    VALUES
                        ('site:manage', gen_random_uuid(), 1),
                        ('site:view', gen_random_uuid(), 1),
                        ('user:manage', gen_random_uuid(), 1),
                        ('user:view', gen_random_uuid(), 1),
                        ('account:manage', gen_random_uuid(), 1),
                        ('account:view', gen_random_uuid(), 1),
                        ('expense:manage', gen_random_uuid(), 1),
                        ('expense:view', gen_random_uuid(), 1),
                        ('income:manage', gen_random_uuid(), 1),
                        ('income:view', gen_random_uuid(), 1),
                        ('maintenance:export', gen_random_uuid(), 1),
                        ('maintenance:import', gen_random_uuid(), 1);

                    -- Link roles to permissions
                    -- Admin gets all permissions
                    INSERT INTO ""RolePermission"" (""PermissionsId"", ""RolesId"")
                    SELECT p.""Id"", 
                            (SELECT ""Id"" FROM ""Role"" WHERE ""Name"" = 'Admin')
                    FROM ""Permission"" p;

                    -- Viewer gets view permissions
                    INSERT INTO ""RolePermission"" (""PermissionsId"", ""RolesId"")
                    SELECT p.""Id"", 
                            (SELECT ""Id"" FROM ""Role"" WHERE ""Name"" = 'Viewer')
                    FROM ""Permission"" p
                    WHERE p.""Name"" LIKE '%:view';
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
