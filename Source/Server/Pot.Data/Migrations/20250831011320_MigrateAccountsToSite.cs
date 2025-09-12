using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateAccountsToSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only proceed if accounts exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Check if any accounts exist
                    IF EXISTS (SELECT 1 FROM ""Account"") THEN
                        -- Create default site
                        INSERT INTO ""Site"" (""Description"", ""RowId"", ""Etag"")
                        SELECT 'Default', gen_random_uuid(), 1
                        WHERE NOT EXISTS (SELECT 1 FROM ""Site"" WHERE ""Description"" = 'Default');

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
                            ('income:view', gen_random_uuid(), 1);

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

                        -- Create admin user
                        -- PasswordHash is for a default password 'changeme'
                        INSERT INTO ""User"" (""Username"", ""PasswordHash"", ""SiteId"", ""RowId"", ""Etag"")
                        SELECT 'Admin', 'AQAAAAIAAYagAAAAEOPv70L4yNF+Tv14uDjDVeKcTKOLR3HpdNBTRwn3Adgmshe87c10TDitJNRCDTBUMQ==',
                               (SELECT ""Id"" FROM ""Site"" WHERE ""Description"" = 'Default'),
                               gen_random_uuid(), 1;

                        -- Link admin user to Admin role
                        INSERT INTO ""UserRole"" (""RolesId"", ""UsersId"")
                        SELECT (SELECT ""Id"" FROM ""Role"" WHERE ""Name"" = 'Admin'),
                               (SELECT ""Id"" FROM ""User"" WHERE ""Username"" = 'Admin');

                        -- Link existing accounts to default site
                        UPDATE ""Account""
                        SET ""SiteId"" = (SELECT ""Id"" FROM ""Site"" WHERE ""Description"" = 'Default')
                        WHERE ""SiteId"" IS NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No down migration needed since this is conditional data migration
            // If accounts didn't exist, nothing was added
            // If accounts existed, we don't want to remove the site they're now linked to
        }
    }
}
