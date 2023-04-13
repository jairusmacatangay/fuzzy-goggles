using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class RemovedFKeysInEncounters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_DentalCharts_DentalChartId",
                table: "Encounters");

            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_Patients_PatientId",
                table: "Encounters");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_DentalChartId",
                table: "Encounters");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_PatientId",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "EncounterId",
                table: "TreatmentRecords");

            migrationBuilder.DropColumn(
                name: "DentalChartId",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Encounters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EncounterId",
                table: "TreatmentRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DentalChartId",
                table: "Encounters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "Encounters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_DentalChartId",
                table: "Encounters",
                column: "DentalChartId");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_PatientId",
                table: "Encounters",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Encounters_DentalCharts_DentalChartId",
                table: "Encounters",
                column: "DentalChartId",
                principalTable: "DentalCharts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Encounters_Patients_PatientId",
                table: "Encounters",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
