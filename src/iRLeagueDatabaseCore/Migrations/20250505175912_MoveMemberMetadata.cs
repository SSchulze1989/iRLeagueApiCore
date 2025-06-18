using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    /// <inheritdoc />
    public partial class MoveMemberMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DanLisaId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "DiscordId",
                table: "Members");

            migrationBuilder.AddColumn<string>(
                name: "CountryFlag",
                table: "LeagueMembers",
                type: "varchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscordId",
                table: "LeagueMembers",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "LeagueMembers",
                type: "varchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile",
                table: "LeagueMembers",
                type: "json",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryFlag",
                table: "LeagueMembers");

            migrationBuilder.DropColumn(
                name: "DiscordId",
                table: "LeagueMembers");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "LeagueMembers");

            migrationBuilder.DropColumn(
                name: "Profile",
                table: "LeagueMembers");

            migrationBuilder.AddColumn<string>(
                name: "DanLisaId",
                table: "Members",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscordId",
                table: "Members",
                type: "longtext",
                nullable: true);
        }
    }
}
