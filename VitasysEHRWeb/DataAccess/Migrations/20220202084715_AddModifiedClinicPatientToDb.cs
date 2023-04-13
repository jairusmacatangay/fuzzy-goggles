using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddModifiedClinicPatientToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicPatient");

            migrationBuilder.CreateTable(
                name: "ClinicPatients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ClinicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicPatients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicPatients_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClinicPatients_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPatients_ClinicId",
                table: "ClinicPatients",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPatients_PatientId",
                table: "ClinicPatients",
                column: "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicPatients");

            migrationBuilder.CreateTable(
                name: "ClinicPatient",
                columns: table => new
                {
                    ClinicsId = table.Column<int>(type: "int", nullable: false),
                    PatientsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicPatient", x => new { x.ClinicsId, x.PatientsId });
                    table.ForeignKey(
                        name: "FK_ClinicPatient_Clinics_ClinicsId",
                        column: x => x.ClinicsId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClinicPatient_Patients_PatientsId",
                        column: x => x.PatientsId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPatient_PatientsId",
                table: "ClinicPatient",
                column: "PatientsId");
        }
    }
}
