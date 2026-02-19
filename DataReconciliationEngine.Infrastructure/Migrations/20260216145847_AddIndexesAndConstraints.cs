using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataReconciliationEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SystemB_Table",
                table: "TableComparisonConfigurations",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SystemA_Table",
                table: "TableComparisonConfigurations",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MatchColumn_SystemB",
                table: "TableComparisonConfigurations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MatchColumn_SystemA",
                table: "TableComparisonConfigurations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ComparisonName",
                table: "TableComparisonConfigurations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SystemB_Column",
                table: "FieldMappingConfigurations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SystemA_Column",
                table: "FieldMappingConfigurations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LogicalFieldName",
                table: "FieldMappingConfigurations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ValueSystemB",
                table: "ComparisonResults",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ValueSystemA",
                table: "ComparisonResults",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MatchKeyValue",
                table: "ComparisonResults",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LogicalFieldName",
                table: "ComparisonResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_TableComparisonConfigs_ComparisonName",
                table: "TableComparisonConfigurations",
                column: "ComparisonName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonRuns_ConfigId_RunDate",
                table: "ComparisonRuns",
                columns: new[] { "ComparisonConfigId", "RunDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonResults_ComparisonConfigId",
                table: "ComparisonResults",
                column: "ComparisonConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonResults_RunId_IsMatch",
                table: "ComparisonResults",
                columns: new[] { "RunId", "IsMatch" });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonResults_RunId_LogicalFieldName",
                table: "ComparisonResults",
                columns: new[] { "RunId", "LogicalFieldName" });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonResults_RunId_MatchKeyValue",
                table: "ComparisonResults",
                columns: new[] { "RunId", "MatchKeyValue" });

            migrationBuilder.AddForeignKey(
                name: "FK_ComparisonResults_ComparisonRuns_RunId",
                table: "ComparisonResults",
                column: "RunId",
                principalTable: "ComparisonRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComparisonResults_TableComparisonConfigurations_ComparisonConfigId",
                table: "ComparisonResults",
                column: "ComparisonConfigId",
                principalTable: "TableComparisonConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComparisonRuns_TableComparisonConfigurations_ComparisonConfigId",
                table: "ComparisonRuns",
                column: "ComparisonConfigId",
                principalTable: "TableComparisonConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComparisonResults_ComparisonRuns_RunId",
                table: "ComparisonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ComparisonResults_TableComparisonConfigurations_ComparisonConfigId",
                table: "ComparisonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ComparisonRuns_TableComparisonConfigurations_ComparisonConfigId",
                table: "ComparisonRuns");

            migrationBuilder.DropIndex(
                name: "IX_TableComparisonConfigs_ComparisonName",
                table: "TableComparisonConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_ComparisonRuns_ConfigId_RunDate",
                table: "ComparisonRuns");

            migrationBuilder.DropIndex(
                name: "IX_ComparisonResults_ComparisonConfigId",
                table: "ComparisonResults");

            migrationBuilder.DropIndex(
                name: "IX_ComparisonResults_RunId_IsMatch",
                table: "ComparisonResults");

            migrationBuilder.DropIndex(
                name: "IX_ComparisonResults_RunId_LogicalFieldName",
                table: "ComparisonResults");

            migrationBuilder.DropIndex(
                name: "IX_ComparisonResults_RunId_MatchKeyValue",
                table: "ComparisonResults");

            migrationBuilder.AlterColumn<string>(
                name: "SystemB_Table",
                table: "TableComparisonConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "SystemA_Table",
                table: "TableComparisonConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "MatchColumn_SystemB",
                table: "TableComparisonConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "MatchColumn_SystemA",
                table: "TableComparisonConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ComparisonName",
                table: "TableComparisonConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "SystemB_Column",
                table: "FieldMappingConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "SystemA_Column",
                table: "FieldMappingConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LogicalFieldName",
                table: "FieldMappingConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ValueSystemB",
                table: "ComparisonResults",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ValueSystemA",
                table: "ComparisonResults",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MatchKeyValue",
                table: "ComparisonResults",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "LogicalFieldName",
                table: "ComparisonResults",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
