using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataReconciliationEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComparisonResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunId = table.Column<int>(type: "int", nullable: false),
                    ComparisonConfigId = table.Column<int>(type: "int", nullable: false),
                    MatchKeyValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExistsInSystemA = table.Column<bool>(type: "bit", nullable: false),
                    ExistsInSystemB = table.Column<bool>(type: "bit", nullable: false),
                    LogicalFieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValueSystemA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueSystemB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMatch = table.Column<bool>(type: "bit", nullable: false),
                    ComparedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComparisonRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComparisonConfigId = table.Column<int>(type: "int", nullable: false),
                    RunDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRecords = table.Column<int>(type: "int", nullable: false),
                    TotalMismatches = table.Column<int>(type: "int", nullable: false),
                    TotalMissingInA = table.Column<int>(type: "int", nullable: false),
                    TotalMissingInB = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TableComparisonConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComparisonName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemA_Table = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemB_Table = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchColumn_SystemA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchColumn_SystemB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableComparisonConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FieldMappingConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComparisonConfigId = table.Column<int>(type: "int", nullable: false),
                    LogicalFieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemA_Column = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemB_Column = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldMappingConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldMappingConfigurations_TableComparisonConfigurations_ComparisonConfigId",
                        column: x => x.ComparisonConfigId,
                        principalTable: "TableComparisonConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldMappingConfigurations_ComparisonConfigId",
                table: "FieldMappingConfigurations",
                column: "ComparisonConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComparisonResults");

            migrationBuilder.DropTable(
                name: "ComparisonRuns");

            migrationBuilder.DropTable(
                name: "FieldMappingConfigurations");

            migrationBuilder.DropTable(
                name: "TableComparisonConfigurations");
        }
    }
}
