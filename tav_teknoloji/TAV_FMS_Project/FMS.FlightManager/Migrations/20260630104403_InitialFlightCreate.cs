using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FMS.FlightManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialFlightCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlightNumber = table.Column<string>(type: "text", nullable: false),
                    AirlineCode = table.Column<string>(type: "text", nullable: false),
                    DepartureStation = table.Column<string>(type: "text", nullable: false),
                    ArrivalStation = table.Column<string>(type: "text", nullable: false),
                    ScheduleTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightNumber",
                table: "Flights",
                column: "FlightNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flights");
        }
    }
}
