using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddTreatmentToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Treatments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    TreatmentTypeId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "decimal(10,2)", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treatments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Treatments_TreatmentTypes_TreatmentTypeId",
                        column: x => x.TreatmentTypeId,
                        principalTable: "TreatmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Treatments_TreatmentTypeId",
                table: "Treatments",
                column: "TreatmentTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Treatments");
        }
    }
}
