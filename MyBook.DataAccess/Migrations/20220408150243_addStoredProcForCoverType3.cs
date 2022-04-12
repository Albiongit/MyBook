using Microsoft.EntityFrameworkCore.Migrations;

namespace MyBook.DataAccess.Migrations
{
    public partial class addStoredProcForCoverType3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE PROC usp_UpdateCoverType(@Id int, @Name varchar(100))
                                                    AS
                                                    BEGIN
                                                     UPDATE dbo.CoverTypes
                                                     SET Name = @Name
                                                     WHERE Id = @Id
                                                    END;");

            

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE usp_UpdateCoverType");
        }
    }
}
