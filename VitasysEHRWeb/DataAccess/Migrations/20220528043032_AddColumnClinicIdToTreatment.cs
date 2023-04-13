using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddColumnClinicIdToTreatment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Treatments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Treatments_ClinicId",
                table: "Treatments",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Treatments_Clinics_ClinicId",
                table: "Treatments",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Treatments_Clinics_ClinicId",
                table: "Treatments");

            migrationBuilder.DropIndex(
                name: "IX_Treatments_ClinicId",
                table: "Treatments");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Treatments");
        }
    }
}
