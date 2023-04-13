using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class AddReviewOfSystemToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewOfSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsHbp = table.Column<bool>(type: "bit", nullable: false),
                    IsLbp = table.Column<bool>(type: "bit", nullable: false),
                    IsEpilepsy = table.Column<bool>(type: "bit", nullable: false),
                    IsAids = table.Column<bool>(type: "bit", nullable: false),
                    IsStd = table.Column<bool>(type: "bit", nullable: false),
                    IsStomach = table.Column<bool>(type: "bit", nullable: false),
                    IsFainting = table.Column<bool>(type: "bit", nullable: false),
                    IsWeightLoss = table.Column<bool>(type: "bit", nullable: false),
                    IsRadiation = table.Column<bool>(type: "bit", nullable: false),
                    IsJointReplacement = table.Column<bool>(type: "bit", nullable: false),
                    IsHeartSurgery = table.Column<bool>(type: "bit", nullable: false),
                    IsHeartAttack = table.Column<bool>(type: "bit", nullable: false),
                    IsThyroid = table.Column<bool>(type: "bit", nullable: false),
                    IsHeartDisease = table.Column<bool>(type: "bit", nullable: false),
                    IsHeartMurmur = table.Column<bool>(type: "bit", nullable: false),
                    IsHepa = table.Column<bool>(type: "bit", nullable: false),
                    IsRheumatic = table.Column<bool>(type: "bit", nullable: false),
                    IsHayFever = table.Column<bool>(type: "bit", nullable: false),
                    IsRespiratory = table.Column<bool>(type: "bit", nullable: false),
                    IsJaundice = table.Column<bool>(type: "bit", nullable: false),
                    IsTuberculosis = table.Column<bool>(type: "bit", nullable: false),
                    IsSwollenAnkles = table.Column<bool>(type: "bit", nullable: false),
                    IsKidney = table.Column<bool>(type: "bit", nullable: false),
                    IsDiabetes = table.Column<bool>(type: "bit", nullable: false),
                    IsChestPain = table.Column<bool>(type: "bit", nullable: false),
                    IsStroke = table.Column<bool>(type: "bit", nullable: false),
                    IsCancer = table.Column<bool>(type: "bit", nullable: false),
                    IsAnemia = table.Column<bool>(type: "bit", nullable: false),
                    IsAngina = table.Column<bool>(type: "bit", nullable: false),
                    IsAsthma = table.Column<bool>(type: "bit", nullable: false),
                    IsEmphysema = table.Column<bool>(type: "bit", nullable: false),
                    IsBleeding = table.Column<bool>(type: "bit", nullable: false),
                    IsBloodDisease = table.Column<bool>(type: "bit", nullable: false),
                    IsHeadInjury = table.Column<bool>(type: "bit", nullable: false),
                    IsArthritis = table.Column<bool>(type: "bit", nullable: false),
                    Other = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewOfSystems", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewOfSystems");
        }
    }
}
