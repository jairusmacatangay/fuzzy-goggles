using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class RenamedToothDetailsToToothAndUpdatedTheColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToothDetails");

            migrationBuilder.CreateTable(
                name: "Tooth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DentalChartId = table.Column<int>(type: "int", nullable: false),
                    ToothNo = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    BuccalOrLabial = table.Column<int>(type: "int", nullable: false),
                    Lingual = table.Column<int>(type: "int", nullable: false),
                    Occlusal = table.Column<int>(type: "int", nullable: false),
                    Mesial = table.Column<int>(type: "int", nullable: false),
                    Distal = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tooth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tooth_DentalCharts_DentalChartId",
                        column: x => x.DentalChartId,
                        principalTable: "DentalCharts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tooth_DentalChartId",
                table: "Tooth",
                column: "DentalChartId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tooth");

            migrationBuilder.CreateTable(
                name: "ToothDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DentalChartId = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBuccal = table.Column<bool>(type: "bit", nullable: false),
                    IsDistal = table.Column<bool>(type: "bit", nullable: false),
                    IsLingual = table.Column<bool>(type: "bit", nullable: false),
                    IsMesial = table.Column<bool>(type: "bit", nullable: false),
                    IsOcclusal = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToothDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToothDetails_DentalCharts_DentalChartId",
                        column: x => x.DentalChartId,
                        principalTable: "DentalCharts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToothDetails_DentalChartId",
                table: "ToothDetails",
                column: "DentalChartId");
        }
    }
}
