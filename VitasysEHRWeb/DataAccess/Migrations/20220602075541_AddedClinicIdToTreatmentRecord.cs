using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddedClinicIdToTreatmentRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "TreatmentRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentRecords_ClinicId",
                table: "TreatmentRecords",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecords_Clinics_ClinicId",
                table: "TreatmentRecords",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecords_Clinics_ClinicId",
                table: "TreatmentRecords");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentRecords_ClinicId",
                table: "TreatmentRecords");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "TreatmentRecords");
        }
    }
}
