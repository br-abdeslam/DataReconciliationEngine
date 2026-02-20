using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataReconciliationEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerSitesDuplicateDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ComparisonConfigId",
                table: "ComparisonRuns",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RunType",
                table: "ComparisonRuns",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "DuplicateGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LatRound = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    LonRound = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    CandidateKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecordsCount = table.Column<int>(type: "int", nullable: false),
                    CertainCount = table.Column<int>(type: "int", nullable: false),
                    ProbableCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuplicateGroups_ComparisonRuns_RunId",
                        column: x => x.RunId,
                        principalTable: "ComparisonRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuplicateRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DuplicateGroupId = table.Column<int>(type: "int", nullable: false),
                    CustomerSitesId = table.Column<int>(type: "int", nullable: false),
                    StreetRaw = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumberRaw = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BoxRaw = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZipRaw = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CityRaw = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StreetNorm = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumberNorm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BoxNorm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZipNorm = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CityNorm = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(17,14)", precision: 17, scale: 14, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(17,14)", precision: 17, scale: 14, nullable: true),
                    CompletenessScore = table.Column<int>(type: "int", nullable: false),
                    IsMasterSuggested = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuplicateRecords_DuplicateGroups_DuplicateGroupId",
                        column: x => x.DuplicateGroupId,
                        principalTable: "DuplicateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonRuns_RunType_RunDate",
                table: "ComparisonRuns",
                columns: new[] { "RunType", "RunDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateGroups_RunId_CandidateKey",
                table: "DuplicateGroups",
                columns: new[] { "RunId", "CandidateKey" });

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateGroups_RunId_RecordsCount",
                table: "DuplicateGroups",
                columns: new[] { "RunId", "RecordsCount" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateRecords_CustomerSitesId",
                table: "DuplicateRecords",
                column: "CustomerSitesId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateRecords_DuplicateGroupId",
                table: "DuplicateRecords",
                column: "DuplicateGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuplicateRecords");

            migrationBuilder.DropTable(
                name: "DuplicateGroups");

            migrationBuilder.DropIndex(
                name: "IX_ComparisonRuns_RunType_RunDate",
                table: "ComparisonRuns");

            migrationBuilder.DropColumn(
                name: "RunType",
                table: "ComparisonRuns");

            migrationBuilder.AlterColumn<int>(
                name: "ComparisonConfigId",
                table: "ComparisonRuns",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
