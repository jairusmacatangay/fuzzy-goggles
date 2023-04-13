using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class UpdatedTreatmentRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecords_Encounters_EncounterId",
                table: "TreatmentRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecords_Treatments_TreatmentId",
                table: "TreatmentRecords");

            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "TreatmentRecords");

            migrationBuilder.AlterColumn<int>(
                name: "TreatmentId",
                table: "TreatmentRecords",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EncounterId",
                table: "TreatmentRecords",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "TreatmentRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecords_Encounters_EncounterId",
                table: "TreatmentRecords",
                column: "EncounterId",
                principalTable: "Encounters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecords_Treatments_TreatmentId",
                table: "TreatmentRecords",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecords_Encounters_EncounterId",
                table: "TreatmentRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecords_Treatments_TreatmentId",
                table: "TreatmentRecords");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "TreatmentRecords");

            migrationBuilder.AlterColumn<int>(
                name: "TreatmentId",
                table: "TreatmentRecords",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EncounterId",
                table: "TreatmentRecords",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                table: "TreatmentRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecords_Encounters_EncounterId",
                table: "TreatmentRecords",
                column: "EncounterId",
                principalTable: "Encounters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecords_Treatments_TreatmentId",
                table: "TreatmentRecords",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
