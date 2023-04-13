using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class UpdatedDentalChartsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateAdded",
                table: "DentalCharts",
                newName: "EncounterDate");

            migrationBuilder.AddColumn<int>(
                name: "ClinidId",
                table: "DentalCharts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DentalCharts_ClinidId",
                table: "DentalCharts",
                column: "ClinidId");

            migrationBuilder.AddForeignKey(
                name: "FK_DentalCharts_Clinics_ClinidId",
                table: "DentalCharts",
                column: "ClinidId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DentalCharts_Clinics_ClinidId",
                table: "DentalCharts");

            migrationBuilder.DropIndex(
                name: "IX_DentalCharts_ClinidId",
                table: "DentalCharts");

            migrationBuilder.DropColumn(
                name: "ClinidId",
                table: "DentalCharts");

            migrationBuilder.RenameColumn(
                name: "EncounterDate",
                table: "DentalCharts",
                newName: "DateAdded");
        }
    }
}
