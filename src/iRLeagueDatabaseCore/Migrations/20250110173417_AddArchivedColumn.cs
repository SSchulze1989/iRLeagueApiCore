using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Teams",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "StatisticSets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Standings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "StandingConfigurations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Sessions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "SessionResults",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Seasons",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Scorings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ScoredSessionResults",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ScoredEventResults",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Schedules",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ReviewComments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ResultConfigurations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "PointRules",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Leagues",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "IncidentReviews",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "FilterOptions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Events",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "EventResults",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ChampSeasons",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "StatisticSets");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Standings");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "StandingConfigurations");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "SessionResults");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Scorings");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ScoredSessionResults");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ScoredEventResults");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ReviewComments");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ResultConfigurations");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "PointRules");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "IncidentReviews");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "FilterOptions");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "EventResults");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ChampSeasons");
        }
    }
}
