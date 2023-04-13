using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class RemovedRestoProsthosTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToothDetails_RestoProsthos_RestoProsthoId",
                table: "ToothDetails");

            migrationBuilder.DropTable(
                name: "RestoProsthos");

            migrationBuilder.DropIndex(
                name: "IX_ToothDetails_RestoProsthoId",
                table: "ToothDetails");

            migrationBuilder.DropColumn(
                name: "RestoProsthoId",
                table: "ToothDetails");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestoProsthoId",
                table: "ToothDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RestoProsthos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestoProsthos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToothDetails_RestoProsthoId",
                table: "ToothDetails",
                column: "RestoProsthoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToothDetails_RestoProsthos_RestoProsthoId",
                table: "ToothDetails",
                column: "RestoProsthoId",
                principalTable: "RestoProsthos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
