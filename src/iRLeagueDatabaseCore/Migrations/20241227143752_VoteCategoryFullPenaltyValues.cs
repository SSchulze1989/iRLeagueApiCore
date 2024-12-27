using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    public partial class VoteCategoryFullPenaltyValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- pre migration
                -- safe the DefaultPenalty values as ints

                CREATE TEMPORARY TABLE TmpVoteCategoryPenaltyValues (
	                CatId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	                DefaultPenalty INT NOT NULL
                );

                INSERT INTO TmpVoteCategoryPenaltyValues (CatId, DefaultPenalty)
	                SELECT CatId, DefaultPenalty
	                FROM VoteCategories;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "DefaultPenalty",
                table: "VoteCategories",
                type: "json",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(@"
                -- post migration
                -- restore DefaultPenalty values as json string

                UPDATE VoteCategories `votes` 
	                JOIN TmpVoteCategoryPenaltyValues `tmp` 
		                ON `tmp`.CatId=`votes`.CatId
	                SET `votes`.DefaultPenalty = CONCAT('{\""Type\"": 0, \""Points\"": ', tmp.DefaultPenalty, '}');

                DROP TABLE TmpVoteCategoryPenaltyValues;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DefaultPenalty",
                table: "VoteCategories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true);
        }
    }
}
