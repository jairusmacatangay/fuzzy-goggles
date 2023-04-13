using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddStartTimeAndEndTimeInClinic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EndTime",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Clinics");
        }
    }
}
