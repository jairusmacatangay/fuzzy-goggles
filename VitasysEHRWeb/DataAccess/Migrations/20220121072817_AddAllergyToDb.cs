using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddAllergyToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Allergies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsAnesthesia = table.Column<bool>(type: "bit", nullable: false),
                    IsAntibiotics = table.Column<bool>(type: "bit", nullable: false),
                    IsSulfa = table.Column<bool>(type: "bit", nullable: false),
                    IsAspirin = table.Column<bool>(type: "bit", nullable: false),
                    IsLatex = table.Column<bool>(type: "bit", nullable: false),
                    Other = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergies", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Allergies");
        }
    }
}
