using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanetsAndUniverse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // From now on every player has a home planet. Players who joined before planets existed have none;
            // that data only exists in development databases, so they simply join again.
            migrationBuilder.Sql("DELETE FROM \"Players\";");

            migrationBuilder.AddColumn<int>(
                name: "EconomySpeed",
                table: "Seasons",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Galaxies",
                table: "Seasons",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "SystemsPerGalaxy",
                table: "Seasons",
                type: "integer",
                nullable: false,
                defaultValue: 499);

            migrationBuilder.CreateTable(
                name: "Planets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Galaxy = table.Column<int>(type: "integer", nullable: false),
                    SolarSystem = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    MaxTemperature = table.Column<int>(type: "integer", nullable: false),
                    ResourcesUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Construction_Building = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Construction_TargetLevel = table.Column<int>(type: "integer", nullable: true),
                    Construction_CostMetal = table.Column<double>(type: "double precision", nullable: true),
                    Construction_CostCrystal = table.Column<double>(type: "double precision", nullable: true),
                    Construction_CostDeuterium = table.Column<double>(type: "double precision", nullable: true),
                    Construction_StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Construction_CompletesAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    BuildingLevels = table.Column<string>(type: "jsonb", nullable: false),
                    Resources_Crystal = table.Column<double>(type: "double precision", nullable: false),
                    Resources_Deuterium = table.Column<double>(type: "double precision", nullable: false),
                    Resources_Metal = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Planets_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Planets_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Planets_Construction_CompletesAt",
                table: "Planets",
                column: "Construction_CompletesAt");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_PlayerId",
                table: "Planets",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_SeasonId_Galaxy_SolarSystem_Position",
                table: "Planets",
                columns: new[] { "SeasonId", "Galaxy", "SolarSystem", "Position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Planets");

            migrationBuilder.DropColumn(
                name: "EconomySpeed",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "Galaxies",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "SystemsPerGalaxy",
                table: "Seasons");
        }
    }
}