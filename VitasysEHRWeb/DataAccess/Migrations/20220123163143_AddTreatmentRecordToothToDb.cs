using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddTreatmentRecordToothToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_ToothTreatmentRecord_TreatmentRecordsId",
                table: "ToothTreatmentRecord",
                column: "TreatmentRecordsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToothTreatmentRecord");
        }
    }
}
