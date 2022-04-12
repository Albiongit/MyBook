using Microsoft.EntityFrameworkCore.Migrations;

namespace MyBook.DataAccess.Migrations
{
    public partial class addStoredProcForCoverType5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE PROC usp_CreateCoverType(@Name varchar(100))
                                                    AS
                                                    BEGIN
                                                     INSERT INTO dbo.CoverTypes(Name)
                                                    VALUES (@Name)
                                                    END;");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE usp_CreateCoverType");
        }
    }
}
