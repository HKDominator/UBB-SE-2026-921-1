using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PussyCats.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedRoleAndJobRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelectedRole",
                table: "PersonalityTestResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JobRole",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Jobs",
                keyColumn: "JobId",
                keyValue: 1,
                column: "JobRole",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Jobs",
                keyColumn: "JobId",
                keyValue: 2,
                column: "JobRole",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Jobs",
                keyColumn: "JobId",
                keyValue: 3,
                column: "JobRole",
                value: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedRole",
                table: "PersonalityTestResults");

            migrationBuilder.DropColumn(
                name: "JobRole",
                table: "Jobs");
        }
    }
}
