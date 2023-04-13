using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class MadeInvoiceIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecords_Invoices_InvoiceId",
                table: "TreatmentRecords");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "TreatmentRecords",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecords_Invoices_InvoiceId",
                table: "TreatmentRecords",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecords_Invoices_InvoiceId",
                table: "TreatmentRecords");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "TreatmentRecords",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecords_Invoices_InvoiceId",
                table: "TreatmentRecords",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
