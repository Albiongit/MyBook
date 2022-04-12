using Microsoft.EntityFrameworkCore.Migrations;

namespace MyBook.DataAccess.Migrations
{
    public partial class addStoredProcForCoverType4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE PROC usp_DeleteCoverType(@Id int)
                                                    AS
                                                    BEGIN
                                                     DELETE FROM dbo.CoverTypes
                                                    WHERE Id = @Id
                                                    END;");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE usp_DeleteCoverType");
        }
    }
}
