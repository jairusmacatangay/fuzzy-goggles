using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    public partial class UpdatedDBForAppointments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentTimeId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AppointmentTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeSlot = table.Column<string>(type: "nvarchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentTime", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentTimeId",
                table: "Appointments",
                column: "AppointmentTimeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AppointmentTime_AppointmentTimeId",
                table: "Appointments",
                column: "AppointmentTimeId",
                principalTable: "AppointmentTime",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AppointmentTime_AppointmentTimeId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "AppointmentTime");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AppointmentTimeId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AppointmentTimeId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Appointments");

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
