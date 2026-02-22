using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceService.Application.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    RaceWins = table.Column<string>(type: "text", nullable: false)
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
                    LengthInKm = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    BestLapTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
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
                    TrackId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackId = table.Column<int>(type: "integer", nullable: false),
                    Laps = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Race", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Race_Track_TrackId1",
                        column: x => x.TrackId1,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PastResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    RaceId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastResult_Race_RaceId1",
                        column: x => x.RaceId1,
                        principalTable: "Race",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RaceEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeriesType = table.Column<string>(type: "text", nullable: false),
                    CarClass = table.Column<string>(type: "text", nullable: false),
                    Driver1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Driver2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Driver4Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Driver3Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverResId = table.Column<Guid>(type: "uuid", nullable: false),
                    CarManufacturer = table.Column<string>(type: "text", nullable: false),
                    CarModel = table.Column<string>(type: "text", nullable: false),
                    PUManufacturer = table.Column<string>(type: "text", nullable: false),
                    RaceEntryName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceEntry_Driver_Driver1Id",
                        column: x => x.Driver1Id,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceEntry_Driver_Driver2Id",
                        column: x => x.Driver2Id,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceEntry_Driver_Driver3Id",
                        column: x => x.Driver3Id,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceEntry_Driver_Driver4Id",
                        column: x => x.Driver4Id,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceEntry_Driver_DriverResId",
                        column: x => x.DriverResId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceEntry_Race_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Race",
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
                    LapsCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    BestLapTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    StartingPosition = table.Column<int>(type: "integer", nullable: false),
                    PastResultId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceResult_PastResult_PastResultId",
                        column: x => x.PastResultId,
                        principalTable: "PastResult",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RaceResult_RaceEntry_RaceEntryId",
                        column: x => x.RaceEntryId,
                        principalTable: "RaceEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PastResult_RaceId1",
                table: "PastResult",
                column: "RaceId1");

            migrationBuilder.CreateIndex(
                name: "IX_Race_TrackId1",
                table: "Race",
                column: "TrackId1");

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_Driver1Id",
                table: "RaceEntry",
                column: "Driver1Id");

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_Driver2Id",
                table: "RaceEntry",
                column: "Driver2Id");

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_Driver3Id",
                table: "RaceEntry",
                column: "Driver3Id");

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_Driver4Id",
                table: "RaceEntry",
                column: "Driver4Id");

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_DriverResId",
                table: "RaceEntry",
                column: "DriverResId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_RaceId",
                table: "RaceEntry",
                column: "RaceId");

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
                name: "RaceResult");

            migrationBuilder.DropTable(
                name: "PastResult");

            migrationBuilder.DropTable(
                name: "RaceEntry");

            migrationBuilder.DropTable(
                name: "Driver");

            migrationBuilder.DropTable(
                name: "Race");

            migrationBuilder.DropTable(
                name: "Track");
        }
    }
}
