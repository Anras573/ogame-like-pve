using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResearchAndShipyard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CurrentResearch_CompletesAt",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentResearch_CostCrystal",
                table: "Players",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentResearch_CostDeuterium",
                table: "Players",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentResearch_CostMetal",
                table: "Players",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentResearch_PlanetId",
                table: "Players",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentResearch_Research",
                table: "Players",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CurrentResearch_StartedAt",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentResearch_TargetLevel",
                table: "Players",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResearchLevels",
                table: "Players",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Players",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "ShipyardOrders",
                table: "Planets",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ShipyardProgressAt",
                table: "Planets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Units",
                table: "Planets",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.CreateIndex(
                name: "IX_Players_CurrentResearch_CompletesAt",
                table: "Players",
                column: "CurrentResearch_CompletesAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_CurrentResearch_CompletesAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CurrentResearch_CompletesAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CurrentResearch_CostCrystal",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CurrentResearch_CostDeuterium",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CurrentResearch_CostMetal",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CurrentResearch_PlanetId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CurrentResearch_Research",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CurrentResearch_StartedAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CurrentResearch_TargetLevel",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ResearchLevels",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ShipyardOrders",
                table: "Planets");

            migrationBuilder.DropColumn(
                name: "ShipyardProgressAt",
                table: "Planets");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "Planets");
        }
    }
}