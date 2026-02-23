using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrickcalServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithGacha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Grade = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Tribe = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Class = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BaseAtk = table.Column<int>(type: "integer", nullable: false),
                    BaseDef = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCurrencies",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Eleaf = table.Column<int>(type: "integer", nullable: false),
                    Gold = table.Column<int>(type: "integer", nullable: false),
                    Faith = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCurrencies", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "GachaBanners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PickupCardId = table.Column<int>(type: "integer", nullable: false),
                    IsPickupEldain = table.Column<bool>(type: "boolean", nullable: false),
                    PickupRate = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaBanners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GachaBanners_Cards_PickupCardId",
                        column: x => x.PickupCardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CardId = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    DuplicateCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCards_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "Id", "BaseAtk", "BaseDef", "Class", "Grade", "Name", "Tribe" },
                values: new object[,]
                {
                    { 1, 120, 80, "전사", "Star1", "리나", "인간" },
                    { 2, 110, 70, "궁수", "Star1", "코볼", "수인" },
                    { 3, 130, 60, "마법사", "Star1", "미란다", "엘프" },
                    { 4, 90, 140, "탱커", "Star1", "톤즈", "기계" },
                    { 5, 80, 90, "힐러", "Star1", "피넛", "수인" },
                    { 6, 140, 50, "암살자", "Star1", "쿠로", "마족" },
                    { 7, 115, 75, "궁수", "Star1", "도트", "인간" },
                    { 8, 125, 65, "마법사", "Star1", "모스", "정령" },
                    { 9, 118, 85, "전사", "Star1", "버키", "기계" },
                    { 10, 85, 95, "힐러", "Star1", "릴리", "엘프" },
                    { 11, 200, 120, "마법사", "Star2", "세라핀", "엘프" },
                    { 12, 210, 150, "전사", "Star2", "카이론", "수인" },
                    { 13, 190, 110, "궁수", "Star2", "노바", "기계" },
                    { 14, 160, 220, "탱커", "Star2", "루시안", "인간" },
                    { 15, 150, 180, "힐러", "Star2", "유키", "정령" },
                    { 16, 230, 100, "암살자", "Star2", "아스카", "마족" },
                    { 17, 350, 200, "궁수", "Star3", "아르테미스", "엘프" },
                    { 18, 380, 250, "전사", "Star3", "발키리", "인간" },
                    { 19, 400, 180, "마법사", "Star3", "오르페우스", "정령" },
                    { 20, 500, 350, "지배자", "Eldain", "엘다인", "신족" }
                });

            migrationBuilder.InsertData(
                table: "UserCurrencies",
                columns: new[] { "UserId", "Eleaf", "Faith", "Gold" },
                values: new object[] { "testuser", 10000, 0, 5000 });

            migrationBuilder.InsertData(
                table: "GachaBanners",
                columns: new[] { "Id", "EndDate", "IsActive", "IsPickupEldain", "Name", "PickupCardId", "PickupRate", "StartDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, false, "신규 사도 픽업", 18, 0.008m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, true, "엘다인 특별 픽업", 20, 0.006m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GachaBanners_PickupCardId",
                table: "GachaBanners",
                column: "PickupCardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCards_CardId",
                table: "UserCards",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCards_UserId_CardId",
                table: "UserCards",
                columns: new[] { "UserId", "CardId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GachaBanners");

            migrationBuilder.DropTable(
                name: "UserCards");

            migrationBuilder.DropTable(
                name: "UserCurrencies");

            migrationBuilder.DropTable(
                name: "Cards");
        }
    }
}
