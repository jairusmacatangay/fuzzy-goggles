using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddOralCavityToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OralCavities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsGingivitis = table.Column<bool>(type: "bit", nullable: false),
                    IsEarlyPerio = table.Column<bool>(type: "bit", nullable: false),
                    IsModPerio = table.Column<bool>(type: "bit", nullable: false),
                    IsAdvPerio = table.Column<bool>(type: "bit", nullable: false),
                    ClassType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOverjet = table.Column<bool>(type: "bit", nullable: false),
                    IsOverbite = table.Column<bool>(type: "bit", nullable: false),
                    IsMidlineDeviation = table.Column<bool>(type: "bit", nullable: false),
                    IsCrossbite = table.Column<bool>(type: "bit", nullable: false),
                    OrthoApplication = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsClenching = table.Column<bool>(type: "bit", nullable: false),
                    IsClicking = table.Column<bool>(type: "bit", nullable: false),
                    IsTrismus = table.Column<bool>(type: "bit", nullable: false),
                    IsMusclePain = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OralCavities", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OralCavities");
        }
    }
}
