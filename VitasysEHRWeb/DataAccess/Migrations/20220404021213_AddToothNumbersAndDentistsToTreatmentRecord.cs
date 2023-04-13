using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddToothNumbersAndDentistsToTreatmentRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dentists",
                table: "TreatmentRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToothNumbers",
                table: "TreatmentRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dentists",
                table: "TreatmentRecords");

            migrationBuilder.DropColumn(
                name: "ToothNumbers",
                table: "TreatmentRecords");
        }
    }
}
