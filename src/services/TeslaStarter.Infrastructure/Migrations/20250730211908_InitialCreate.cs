using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeslaStarter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TeslaAccount_TeslaAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TeslaAccount_LinkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TeslaAccount_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    TeslaAccount_RefreshToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TeslaAccount_AccessToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TeslaAccount_TokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TeslaAccount_LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeslaAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VehicleIdentifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "ix_users_external_id",
                table: "users",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_tesla_account_id",
                table: "vehicles",
                column: "TeslaAccountId");

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_tesla_account_id_vehicle_identifier",
                table: "vehicles",
                columns: new[] { "TeslaAccountId", "VehicleIdentifier" });

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_vehicle_identifier",
                table: "vehicles",
                column: "VehicleIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "vehicles");
        }
    }
}
