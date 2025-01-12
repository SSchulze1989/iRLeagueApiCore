using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    /// <inheritdoc />
    public partial class AddResultRowCountryCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "ScoredResultRows",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "ResultRows",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "ScoredResultRows");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "ResultRows");
        }
    }
}
