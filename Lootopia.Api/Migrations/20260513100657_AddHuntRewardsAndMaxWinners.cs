using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lootopia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHuntRewardsAndMaxWinners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxWinners",
                table: "Hunts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RewardItemId",
                table: "Hunts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hunts_RewardItemId",
                table: "Hunts",
                column: "RewardItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hunts_Items_RewardItemId",
                table: "Hunts",
                column: "RewardItemId",
                principalTable: "Items",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hunts_Items_RewardItemId",
                table: "Hunts");

            migrationBuilder.DropIndex(
                name: "IX_Hunts_RewardItemId",
                table: "Hunts");

            migrationBuilder.DropColumn(
                name: "MaxWinners",
                table: "Hunts");

            migrationBuilder.DropColumn(
                name: "RewardItemId",
                table: "Hunts");
        }
    }
}
