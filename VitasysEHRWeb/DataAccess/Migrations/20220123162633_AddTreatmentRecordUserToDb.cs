using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddTreatmentRecordUserToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTreatmentRecord",
                columns: table => new
                {
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TreatmentRecordsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTreatmentRecord", x => new { x.UsersId, x.TreatmentRecordsId });
                    table.ForeignKey(
                        name: "FK_UserTreatmentRecord_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTreatmentRecord_TreatmentRecords_TreatmentRecordsId",
                        column: x => x.TreatmentRecordsId,
                        principalTable: "TreatmentRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTreatmentRecord_TreatmentRecordsId",
                table: "UserTreatmentRecord",
                column: "TreatmentRecordsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTreatmentRecord");
        }
    }
}
