using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddToothDetailToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToothDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DentalChartId = table.Column<int>(type: "int", nullable: false),
                    ToothId = table.Column<int>(type: "int", nullable: false),
                    ConditionId = table.Column<int>(type: "int", nullable: false),
                    RestoProsthoId = table.Column<int>(type: "int", nullable: false),
                    IsBuccal = table.Column<bool>(type: "bit", nullable: false),
                    IsLingual = table.Column<bool>(type: "bit", nullable: false),
                    IsOcclusal = table.Column<bool>(type: "bit", nullable: false),
                    IsMesial = table.Column<bool>(type: "bit", nullable: false),
                    IsDistal = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToothDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToothDetails_Conditions_ToothId",
                        column: x => x.ToothId,
                        principalTable: "Conditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToothDetails_DentalCharts_DentalChartId",
                        column: x => x.DentalChartId,
                        principalTable: "DentalCharts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToothDetails_RestoProsthos_RestoProsthoId",
                        column: x => x.RestoProsthoId,
                        principalTable: "RestoProsthos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToothDetails_Teeth_ToothId",
                        column: x => x.ToothId,
                        principalTable: "Teeth",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToothDetails_DentalChartId",
                table: "ToothDetails",
                column: "DentalChartId");

            migrationBuilder.CreateIndex(
                name: "IX_ToothDetails_RestoProsthoId",
                table: "ToothDetails",
                column: "RestoProsthoId");

            migrationBuilder.CreateIndex(
                name: "IX_ToothDetails_ToothId",
                table: "ToothDetails",
                column: "ToothId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToothDetails");
        }
    }
}
