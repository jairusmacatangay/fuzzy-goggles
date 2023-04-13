using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddedClinicToFolderModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Folders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Folders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ClinicId",
                table: "Folders",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Clinics_ClinicId",
                table: "Folders",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Clinics_ClinicId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_ClinicId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Folders");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Folders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
