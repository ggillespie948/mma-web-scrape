using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RacingWebScrape.Db.Migrations
{
    public partial class InitData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    RaceTitleContentId = table.Column<string>(nullable: true),
                    ResultsContentId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseMeetings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    MeetingDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseMeetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseMeetings_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingResults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RaceTime = table.Column<DateTime>(nullable: false),
                    RaceTitle = table.Column<string>(nullable: true),
                    CourseMeetingId = table.Column<int>(nullable: false),
                    RunnerInformation = table.Column<string>(nullable: true),
                    WinningJockey = table.Column<string>(nullable: true),
                    WinningTrainer = table.Column<string>(nullable: true),
                    FullResultURL = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingResults_CourseMeetings_CourseMeetingId",
                        column: x => x.CourseMeetingId,
                        principalTable: "CourseMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultEntires",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MeetingResultId = table.Column<int>(nullable: false),
                    Place = table.Column<string>(nullable: true),
                    HorseName = table.Column<string>(nullable: true),
                    HorseNumber = table.Column<string>(nullable: true),
                    JockeyName = table.Column<string>(nullable: true),
                    TrainerName = table.Column<string>(nullable: true),
                    PriceFraction = table.Column<string>(nullable: true),
                    PriceDecimal = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultEntires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultEntires_MeetingResults_MeetingResultId",
                        column: x => x.MeetingResultId,
                        principalTable: "MeetingResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseMeetings_CourseId",
                table: "CourseMeetings",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingResults_CourseMeetingId",
                table: "MeetingResults",
                column: "CourseMeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultEntires_MeetingResultId",
                table: "ResultEntires",
                column: "MeetingResultId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResultEntires");

            migrationBuilder.DropTable(
                name: "MeetingResults");

            migrationBuilder.DropTable(
                name: "CourseMeetings");

            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
