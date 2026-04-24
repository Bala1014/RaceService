using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceService.Application.Migrations
{
    /// <inheritdoc />
    public partial class _25042026_addedRaceEntryDrivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Driver",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    RaceWins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Driver", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Track",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: false),
                    LengthInKm = table.Column<decimal>(type: "numeric", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    BestLapTime = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Track", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Race",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartTimeUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    Laps = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Race", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Race_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PastResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastResult_Race_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Race",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeriesType = table.Column<string>(type: "text", nullable: false),
                    CarClass = table.Column<string>(type: "text", nullable: false),
                    CarManufacturer = table.Column<string>(type: "text", nullable: false),
                    CarModel = table.Column<string>(type: "text", nullable: false),
                    PUManufacturer = table.Column<string>(type: "text", nullable: false),
                    RaceEntryName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceEntry_Race_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Race",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceEntryDriver",
                columns: table => new
                {
                    RaceEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceEntryDriver", x => new { x.RaceEntryId, x.DriverId });
                    table.ForeignKey(
                        name: "FK_RaceEntryDriver_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceEntryDriver_RaceEntry_RaceEntryId",
                        column: x => x.RaceEntryId,
                        principalTable: "RaceEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PastResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    LapsCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    BestLapTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    StartingPosition = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceResult_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceResult_PastResult_PastResultId",
                        column: x => x.PastResultId,
                        principalTable: "PastResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceResult_RaceEntry_RaceEntryId",
                        column: x => x.RaceEntryId,
                        principalTable: "RaceEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PastResult_RaceId",
                table: "PastResult",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Race_TrackId",
                table: "Race",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_RaceId",
                table: "RaceEntry",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntryDriver_DriverId",
                table: "RaceEntryDriver",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_DriverId",
                table: "RaceResult",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_PastResultId",
                table: "RaceResult",
                column: "PastResultId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_RaceEntryId",
                table: "RaceResult",
                column: "RaceEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaceEntryDriver");

            migrationBuilder.DropTable(
                name: "RaceResult");

            migrationBuilder.DropTable(
                name: "Driver");

            migrationBuilder.DropTable(
                name: "PastResult");

            migrationBuilder.DropTable(
                name: "RaceEntry");

            migrationBuilder.DropTable(
                name: "Race");

            migrationBuilder.DropTable(
                name: "Track");
        }
    }
}
