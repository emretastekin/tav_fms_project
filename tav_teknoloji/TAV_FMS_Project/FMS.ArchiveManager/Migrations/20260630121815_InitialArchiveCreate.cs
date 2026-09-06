using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FMS.ArchiveManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialArchiveCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchivedFlights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginalFlightId = table.Column<string>(type: "text", nullable: false),
                    FlightNumber = table.Column<string>(type: "text", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedFlights", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivedFlights");
        }
    }
}
