using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create citext extension - required for Azure PostgreSQL
            // Azure requires admin privileges to create extensions, but once created,
            // the migration can proceed. This SQL command is idempotent.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS citext;");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Site",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Site", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    PermissionsId = table.Column<int>(type: "integer", nullable: false),
                    RolesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.PermissionsId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_RolePermission_Permission_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Bsb = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Balance = table.Column<double>(type: "double precision", nullable: false),
                    Reserved = table.Column<double>(type: "double precision", nullable: false),
                    TotalExpenseAccrued = table.Column<double>(type: "double precision", nullable: false),
                    DailyExpenseAccrual = table.Column<double>(type: "double precision", nullable: false),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Account_Site_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Site",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RefreshTokenExpiryUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Site_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Site",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expense",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExcludeFromCalcs = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    AccrualStart = table.Column<DateOnly>(type: "date", nullable: false),
                    NextDue = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Frequency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FrequencyCount = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    Accrued = table.Column<double>(type: "double precision", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expense_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Income",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExcludeFromCalcs = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    NextDue = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Frequency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FrequencyCount = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Income", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Income_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    RolesId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UsersId",
                        column: x => x.UsersId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Bsb_Number",
                table: "Account",
                columns: new[] { "Bsb", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_Description",
                table: "Account",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_Etag",
                table: "Account",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Account_RowId",
                table: "Account",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_SiteId",
                table: "Account",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_AccountId_Description",
                table: "Expense",
                columns: new[] { "AccountId", "Description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expense_Etag",
                table: "Expense",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_NextDue",
                table: "Expense",
                column: "NextDue");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_RowId",
                table: "Expense",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Income_AccountId_Description",
                table: "Income",
                columns: new[] { "AccountId", "Description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Income_Etag",
                table: "Income",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Income_NextDue",
                table: "Income",
                column: "NextDue");

            migrationBuilder.CreateIndex(
                name: "IX_Income_RowId",
                table: "Income",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Etag",
                table: "Permission",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Name",
                table: "Permission",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permission_RowId",
                table: "Permission",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_Etag",
                table: "Role",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_RowId",
                table: "Role",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_RolesId",
                table: "RolePermission",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_Site_Description",
                table: "Site",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Site_Etag",
                table: "Site",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Site_RowId",
                table: "Site",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_User_Etag",
                table: "User",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_User_RowId",
                table: "User",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_SiteId",
                table: "User",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UsersId",
                table: "UserRole",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expense");

            migrationBuilder.DropTable(
                name: "Income");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Site");
        }
    }
}
