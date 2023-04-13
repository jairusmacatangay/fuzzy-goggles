using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddFolderChangesToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Folders_FolderTypeId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "FolderTypeId",
                table: "Documents",
                newName: "FolderId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_FolderTypeId",
                table: "Documents",
                newName: "IX_Documents_FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "FolderId",
                table: "Documents",
                newName: "FolderTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_FolderId",
                table: "Documents",
                newName: "IX_Documents_FolderTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Folders_FolderTypeId",
                table: "Documents",
                column: "FolderTypeId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
