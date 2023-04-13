using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class addClinicIdToPrescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ClinicId",
                table: "Prescriptions",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Clinics_ClinicId",
                table: "Prescriptions",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Clinics_ClinicId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_ClinicId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Prescriptions");
        }
    }
}
