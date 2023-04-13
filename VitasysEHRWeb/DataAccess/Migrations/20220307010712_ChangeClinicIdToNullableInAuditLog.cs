using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class ChangeClinicIdToNullableInAuditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Clinics_ClinicId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "ClinicId",
                table: "AuditLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Clinics_ClinicId",
                table: "AuditLogs",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Clinics_ClinicId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "ClinicId",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Clinics_ClinicId",
                table: "AuditLogs",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
