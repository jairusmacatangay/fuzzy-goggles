using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class RemovedTeethTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToothDetails_Teeth_ToothId",
                table: "ToothDetails");

            migrationBuilder.DropTable(
                name: "ToothTreatmentRecord");

            migrationBuilder.DropTable(
                name: "Teeth");

            migrationBuilder.DropIndex(
                name: "IX_ToothDetails_ToothId",
                table: "ToothDetails");

            migrationBuilder.DropColumn(
                name: "ToothId",
                table: "ToothDetails");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ToothId",
                table: "ToothDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Teeth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToothNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teeth", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToothTreatmentRecord",
                columns: table => new
                {
                    TeethId = table.Column<int>(type: "int", nullable: false),
                    TreatmentRecordsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToothTreatmentRecord", x => new { x.TeethId, x.TreatmentRecordsId });
                    table.ForeignKey(
                        name: "FK_ToothTreatmentRecord_Teeth_TeethId",
                        column: x => x.TeethId,
                        principalTable: "Teeth",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToothTreatmentRecord_TreatmentRecords_TreatmentRecordsId",
                        column: x => x.TreatmentRecordsId,
                        principalTable: "TreatmentRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToothDetails_ToothId",
                table: "ToothDetails",
                column: "ToothId");

            migrationBuilder.CreateIndex(
                name: "IX_ToothTreatmentRecord_TreatmentRecordsId",
                table: "ToothTreatmentRecord",
                column: "TreatmentRecordsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToothDetails_Teeth_ToothId",
                table: "ToothDetails",
                column: "ToothId",
                principalTable: "Teeth",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
