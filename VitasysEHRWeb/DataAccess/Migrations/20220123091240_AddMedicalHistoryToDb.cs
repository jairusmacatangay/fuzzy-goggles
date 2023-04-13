using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddMedicalHistoryToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllergyId = table.Column<int>(type: "int", nullable: false),
                    ROSId = table.Column<int>(type: "int", nullable: false),
                    IsGoodHealth = table.Column<bool>(type: "bit", nullable: false),
                    IsMedTreatment = table.Column<bool>(type: "bit", nullable: false),
                    IsConditionTreated = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    IsIllnessOperation = table.Column<bool>(type: "bit", nullable: false),
                    IllnessOperation = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    IsHospitalized = table.Column<bool>(type: "bit", nullable: false),
                    Hospitalized = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    IsMedication = table.Column<bool>(type: "bit", nullable: false),
                    Medication = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    IsTobacco = table.Column<bool>(type: "bit", nullable: false),
                    IsDrugs = table.Column<bool>(type: "bit", nullable: false),
                    BleedingTime = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    BloodPressure = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    IsPregnant = table.Column<bool>(type: "bit", nullable: false),
                    IsNursing = table.Column<bool>(type: "bit", nullable: false),
                    IsBirthControl = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_Allergies_AllergyId",
                        column: x => x.AllergyId,
                        principalTable: "Allergies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_ReviewOfSystems_ROSId",
                        column: x => x.ROSId,
                        principalTable: "ReviewOfSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_AllergyId",
                table: "MedicalHistories",
                column: "AllergyId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_ROSId",
                table: "MedicalHistories",
                column: "ROSId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalHistories");
        }
    }
}
