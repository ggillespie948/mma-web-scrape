using Microsoft.EntityFrameworkCore.Migrations;

namespace RacingWebScrape.Db.Migrations
{
    public partial class AddPoiionIntToResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "ResultEntires",
                nullable: true,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "ResultEntires");
        }
    }
}
