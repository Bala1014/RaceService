using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceService.Application.Migrations
{
    /// <inheritdoc />
    public partial class _21062026_addedIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Race_Track_TrackId",
                table: "Race");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntryDriver_Driver_DriverId",
                table: "RaceEntryDriver");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceResult_Driver_DriverId",
                table: "RaceResult");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceResult_RaceEntry_RaceEntryId",
                table: "RaceResult");

            migrationBuilder.DropIndex(
                name: "IX_RaceResult_PastResultId",
                table: "RaceResult");

            migrationBuilder.DropIndex(
                name: "IX_PastResult_RaceId",
                table: "PastResult");

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "Track",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Track",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Track",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "LengthInKm",
                table: "Track",
                type: "numeric(6,3)",
                precision: 6,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Track",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Track",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SeriesType",
                table: "RaceEntry",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RaceEntryName",
                table: "RaceEntry",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PUManufacturer",
                table: "RaceEntry",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CarModel",
                table: "RaceEntry",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CarManufacturer",
                table: "RaceEntry",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CarClass",
                table: "RaceEntry",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Race",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nationality",
                table: "Driver",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Driver",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Driver",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_PastResultId_Position",
                table: "RaceResult",
                columns: new[] { "PastResultId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntryDriver_RaceEntryId_Role",
                table: "RaceEntryDriver",
                columns: new[] { "RaceEntryId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Race_StartTimeUTC",
                table: "Race",
                column: "StartTimeUTC");

            migrationBuilder.CreateIndex(
                name: "IX_Race_Status_StartTimeUTC",
                table: "Race",
                columns: new[] { "Status", "StartTimeUTC" });

            migrationBuilder.CreateIndex(
                name: "IX_PastResult_RaceId_Year",
                table: "PastResult",
                columns: new[] { "RaceId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_Driver_Number",
                table: "Driver",
                column: "Number");

            migrationBuilder.AddForeignKey(
                name: "FK_Race_Track_TrackId",
                table: "Race",
                column: "TrackId",
                principalTable: "Track",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntryDriver_Driver_DriverId",
                table: "RaceEntryDriver",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceResult_Driver_DriverId",
                table: "RaceResult",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceResult_RaceEntry_RaceEntryId",
                table: "RaceResult",
                column: "RaceEntryId",
                principalTable: "RaceEntry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Race_Track_TrackId",
                table: "Race");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntryDriver_Driver_DriverId",
                table: "RaceEntryDriver");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceResult_Driver_DriverId",
                table: "RaceResult");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceResult_RaceEntry_RaceEntryId",
                table: "RaceResult");

            migrationBuilder.DropIndex(
                name: "IX_RaceResult_PastResultId_Position",
                table: "RaceResult");

            migrationBuilder.DropIndex(
                name: "IX_RaceEntryDriver_RaceEntryId_Role",
                table: "RaceEntryDriver");

            migrationBuilder.DropIndex(
                name: "IX_Race_StartTimeUTC",
                table: "Race");

            migrationBuilder.DropIndex(
                name: "IX_Race_Status_StartTimeUTC",
                table: "Race");

            migrationBuilder.DropIndex(
                name: "IX_PastResult_RaceId_Year",
                table: "PastResult");

            migrationBuilder.DropIndex(
                name: "IX_Driver_Number",
                table: "Driver");

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "Track",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Track",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Track",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "LengthInKm",
                table: "Track",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,3)",
                oldPrecision: 6,
                oldScale: 3);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Track",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Track",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "SeriesType",
                table: "RaceEntry",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "RaceEntryName",
                table: "RaceEntry",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "PUManufacturer",
                table: "RaceEntry",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CarModel",
                table: "RaceEntry",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CarManufacturer",
                table: "RaceEntry",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CarClass",
                table: "RaceEntry",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Race",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Nationality",
                table: "Driver",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Driver",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Driver",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_PastResultId",
                table: "RaceResult",
                column: "PastResultId");

            migrationBuilder.CreateIndex(
                name: "IX_PastResult_RaceId",
                table: "PastResult",
                column: "RaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Race_Track_TrackId",
                table: "Race",
                column: "TrackId",
                principalTable: "Track",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntryDriver_Driver_DriverId",
                table: "RaceEntryDriver",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceResult_Driver_DriverId",
                table: "RaceResult",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceResult_RaceEntry_RaceEntryId",
                table: "RaceResult",
                column: "RaceEntryId",
                principalTable: "RaceEntry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
