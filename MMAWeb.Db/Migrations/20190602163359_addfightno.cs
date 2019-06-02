using Microsoft.EntityFrameworkCore.Migrations;

namespace MMAWeb.Db.Migrations
{
    public partial class addfightno : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FightNumber",
                table: "FightResults",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FightNumber",
                table: "FightResults");
        }
    }
}
