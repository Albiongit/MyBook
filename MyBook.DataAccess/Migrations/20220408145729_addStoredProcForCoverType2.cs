using Microsoft.EntityFrameworkCore.Migrations;

namespace MyBook.DataAccess.Migrations
{
    public partial class addStoredProcForCoverType2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE PROC usp_GetCoverType(@Id int) 
                                                AS 
                                                BEGIN 
                                                 SELECT * FROM   dbo.CoverTypes  WHERE  (Id = @Id) 
                                                END;");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE usp_GetCoverType");

        }
    }
}
