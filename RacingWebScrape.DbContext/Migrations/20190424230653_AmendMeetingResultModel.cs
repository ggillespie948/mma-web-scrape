using Microsoft.EntityFrameworkCore.Migrations;

namespace RacingWebScrape.Db.Migrations
{
    public partial class AmendMeetingResultModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RaceNumber",
                table: "MeetingResults",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RaceNumber",
                table: "MeetingResults");
        }
    }
}
