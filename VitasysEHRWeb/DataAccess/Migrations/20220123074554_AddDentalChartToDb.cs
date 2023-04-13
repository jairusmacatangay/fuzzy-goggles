using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddDentalChartToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DentalCharts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    OralCavityId = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DentalCharts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DentalCharts_OralCavities_OralCavityId",
                        column: x => x.OralCavityId,
                        principalTable: "OralCavities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DentalCharts_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DentalCharts_OralCavityId",
                table: "DentalCharts",
                column: "OralCavityId");

            migrationBuilder.CreateIndex(
                name: "IX_DentalCharts_PatientId",
                table: "DentalCharts",
                column: "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DentalCharts");
        }
    }
}
