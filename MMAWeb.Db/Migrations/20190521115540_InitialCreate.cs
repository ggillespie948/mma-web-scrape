using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MMAWeb.Db.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotionMeetings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    PromotionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionMeetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionMeetings_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FightResults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PromotionMeetingId = table.Column<int>(nullable: false),
                    IsMainCard = table.Column<bool>(nullable: false),
                    FighterNameA = table.Column<string>(nullable: true),
                    FighterNameB = table.Column<string>(nullable: true),
                    DecisionSummary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FightResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FightResults_PromotionMeetings_PromotionMeetingId",
                        column: x => x.PromotionMeetingId,
                        principalTable: "PromotionMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FightResults_PromotionMeetingId",
                table: "FightResults",
                column: "PromotionMeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionMeetings_PromotionId",
                table: "PromotionMeetings",
                column: "PromotionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FightResults");

            migrationBuilder.DropTable(
                name: "PromotionMeetings");

            migrationBuilder.DropTable(
                name: "Promotions");
        }
    }
}
