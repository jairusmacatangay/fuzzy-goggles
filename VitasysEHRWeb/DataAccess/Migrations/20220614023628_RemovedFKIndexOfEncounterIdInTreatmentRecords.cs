using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class RemovedFKIndexOfEncounterIdInTreatmentRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecords_Encounters_EncounterId",
                table: "TreatmentRecords");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentRecords_EncounterId",
                table: "TreatmentRecords");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TreatmentRecords_EncounterId",
                table: "TreatmentRecords",
                column: "EncounterId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecords_Encounters_EncounterId",
                table: "TreatmentRecords",
                column: "EncounterId",
                principalTable: "Encounters",
                principalColumn: "Id");
        }
    }
}
