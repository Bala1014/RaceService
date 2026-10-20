using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceService.Application.Migrations
{
    /// <inheritdoc />
    public partial class InitialRaceDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "race");

            migrationBuilder.CreateTable(
                name: "driver",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    abbreviation = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    country_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    date_of_death = table.Column<DateOnly>(type: "date", nullable: true),
                    place_of_birth = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    permanent_number = table.Column<int>(type: "integer", nullable: true),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    headshot_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    biography = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    website_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_driver", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_reference",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    internal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_payload_json = table.Column<string>(type: "jsonb", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_reference", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "import_run",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    resource = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    records_read = table.Column<int>(type: "integer", nullable: false),
                    records_created = table.Column<int>(type: "integer", nullable: false),
                    records_updated = table.Column<int>(type: "integer", nullable: false),
                    records_skipped = table.Column<int>(type: "integer", nullable: false),
                    records_failed = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    triggered_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_run", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturer",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    country_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    supplies_chassis = table.Column<bool>(type: "boolean", nullable: false),
                    supplies_engines = table.Column<bool>(type: "boolean", nullable: false),
                    supplies_tyres = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_manufacturer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "series",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    short_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    discipline = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    sanctioning_body = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    primary_color_hex = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    website_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    inaugural_year = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_series", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "team",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    official_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    short_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    country_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    base_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    base_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    founded_year = table.Column<int>(type: "integer", nullable: true),
                    logo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    primary_color_hex = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    website_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_team", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "track",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    official_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    time_zone_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, defaultValue: "UTC"),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    opened_year = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    website_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_track", x => x.id);
                    table.CheckConstraint("ck_track_latitude_range", "latitude is null or (latitude between -90 and 90)");
                    table.CheckConstraint("ck_track_longitude_range", "longitude is null or (longitude between -180 and 180)");
                });

            migrationBuilder.CreateTable(
                name: "driver_career_stat",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    series_id = table.Column<Guid>(type: "uuid", nullable: true),
                    starts = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    podiums = table.Column<int>(type: "integer", nullable: false),
                    pole_positions = table.Column<int>(type: "integer", nullable: false),
                    fastest_laps = table.Column<int>(type: "integer", nullable: false),
                    retirements = table.Column<int>(type: "integer", nullable: false),
                    laps_completed = table.Column<int>(type: "integer", nullable: false),
                    laps_led = table.Column<int>(type: "integer", nullable: false),
                    career_points = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    championships_won = table.Column<int>(type: "integer", nullable: false),
                    best_finish_position = table.Column<int>(type: "integer", nullable: true),
                    first_season_year = table.Column<int>(type: "integer", nullable: true),
                    last_season_year = table.Column<int>(type: "integer", nullable: true),
                    recalculated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_driver_career_stat", x => x.id);
                    table.ForeignKey(
                        name: "fk_driver_career_stat_driver_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "race",
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_driver_career_stat_series_series_id",
                        column: x => x.series_id,
                        principalSchema: "race",
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "season",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    series_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    banner_image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    planned_round_count = table.Column<int>(type: "integer", nullable: true),
                    dropped_scores_count = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_season", x => x.id);
                    table.CheckConstraint("ck_season_date_order", "start_date is null or end_date is null or start_date <= end_date");
                    table.CheckConstraint("ck_season_year_range", "year between 1894 and 2200");
                    table.ForeignKey(
                        name: "fk_season_series_series_id",
                        column: x => x.series_id,
                        principalSchema: "race",
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_class",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    series_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    short_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    color_hex = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_class", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_class_series_series_id",
                        column: x => x.series_id,
                        principalSchema: "race",
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "track_layout",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    track_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    length_meters = table.Column<int>(type: "integer", nullable: false),
                    number_of_turns = table.Column<int>(type: "integer", nullable: true),
                    direction = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    track_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    elevation_change_meters = table.Column<int>(type: "integer", nullable: true),
                    longest_straight_meters = table.Column<int>(type: "integer", nullable: true),
                    map_image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_track_layout", x => x.id);
                    table.CheckConstraint("ck_track_layout_length_positive", "length_meters > 0");
                    table.ForeignKey(
                        name: "fk_track_layout_track_track_id",
                        column: x => x.track_id,
                        principalSchema: "race",
                        principalTable: "track",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "points_scheme",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    applies_to_session_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    championship_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    fastest_lap_points = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    fastest_lap_max_position = table.Column<int>(type: "integer", nullable: true),
                    pole_position_points = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    minimum_distance_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    points_multiplier = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false, defaultValue: 1m),
                    minimum_driver_distance_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_points_scheme", x => x.id);
                    table.ForeignKey(
                        name: "fk_points_scheme_season_season_id",
                        column: x => x.season_id,
                        principalSchema: "race",
                        principalTable: "season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entry",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    car_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    vehicle_class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entrant_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    chassis_manufacturer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    chassis_model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    engine_manufacturer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    engine_model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    tyre_supplier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    livery_image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    primary_color_hex = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    is_eligible_for_points = table.Column<bool>(type: "boolean", nullable: false),
                    submitted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entry", x => x.id);
                    table.ForeignKey(
                        name: "fk_entry_manufacturer_chassis_manufacturer_id",
                        column: x => x.chassis_manufacturer_id,
                        principalSchema: "race",
                        principalTable: "manufacturer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entry_manufacturer_engine_manufacturer_id",
                        column: x => x.engine_manufacturer_id,
                        principalSchema: "race",
                        principalTable: "manufacturer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entry_manufacturer_tyre_supplier_id",
                        column: x => x.tyre_supplier_id,
                        principalSchema: "race",
                        principalTable: "manufacturer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entry_season_season_id",
                        column: x => x.season_id,
                        principalSchema: "race",
                        principalTable: "season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entry_team_team_id",
                        column: x => x.team_id,
                        principalSchema: "race",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entry_vehicle_class_vehicle_class_id",
                        column: x => x.vehicle_class_id,
                        principalSchema: "race",
                        principalTable: "vehicle_class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "race_event",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    track_layout_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    official_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    short_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    round_number = table.Column<int>(type: "integer", nullable: true),
                    start_time_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, defaultValue: "UTC"),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    short_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    poster_image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    banner_image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    scheduled_distance_meters = table.Column<int>(type: "integer", nullable: true),
                    scheduled_duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    has_sprint = table.Column<bool>(type: "boolean", nullable: false),
                    is_non_championship = table.Column<bool>(type: "boolean", nullable: false),
                    previous_edition_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_race_event", x => x.id);
                    table.CheckConstraint("ck_race_event_round_positive", "round_number is null or round_number >= 1");
                    table.CheckConstraint("ck_race_event_time_order", "end_time_utc >= start_time_utc");
                    table.ForeignKey(
                        name: "fk_race_event_race_event_previous_edition_id",
                        column: x => x.previous_edition_id,
                        principalSchema: "race",
                        principalTable: "race_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_race_event_season_season_id",
                        column: x => x.season_id,
                        principalSchema: "race",
                        principalTable: "season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_race_event_track_layout_track_layout_id",
                        column: x => x.track_layout_id,
                        principalSchema: "race",
                        principalTable: "track_layout",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "points_scheme_entry",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    points_scheme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_points_scheme_entry", x => x.id);
                    table.CheckConstraint("ck_points_scheme_entry_position_positive", "position >= 1");
                    table.ForeignKey(
                        name: "fk_points_scheme_entry_points_scheme_points_scheme_id",
                        column: x => x.points_scheme_id,
                        principalSchema: "race",
                        principalTable: "points_scheme",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "driver_standing",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    after_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    points_before_drops = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    starts = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    podiums = table.Column<int>(type: "integer", nullable: false),
                    pole_positions = table.Column<int>(type: "integer", nullable: false),
                    fastest_laps = table.Column<int>(type: "integer", nullable: false),
                    retirements = table.Column<int>(type: "integer", nullable: false),
                    previous_position = table.Column<int>(type: "integer", nullable: true),
                    recalculated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_driver_standing", x => x.id);
                    table.ForeignKey(
                        name: "fk_driver_standing_driver_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "race",
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_driver_standing_race_event_after_event_id",
                        column: x => x.after_event_id,
                        principalSchema: "race",
                        principalTable: "race_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_driver_standing_season_season_id",
                        column: x => x.season_id,
                        principalSchema: "race",
                        principalTable: "season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_driver_standing_vehicle_class_vehicle_class_id",
                        column: x => x.vehicle_class_id,
                        principalSchema: "race",
                        principalTable: "vehicle_class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "event_entry",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    race_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    car_number_override = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    starting_grid_position = table.Column<int>(type: "integer", nullable: true),
                    starts_from_pit_lane = table.Column<bool>(type: "boolean", nullable: false),
                    is_eligible_for_points = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_entry", x => x.id);
                    table.CheckConstraint("ck_event_entry_grid_positive", "starting_grid_position is null or starting_grid_position >= 1");
                    table.ForeignKey(
                        name: "fk_event_entry_entry_entry_id",
                        column: x => x.entry_id,
                        principalSchema: "race",
                        principalTable: "entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_event_entry_race_event_race_event_id",
                        column: x => x.race_event_id,
                        principalSchema: "race",
                        principalTable: "race_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    race_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    weekend_order = table.Column<int>(type: "integer", nullable: false),
                    scheduled_start_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    scheduled_end_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    actual_start_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    actual_end_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    scheduled_duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    scheduled_laps = table.Column<int>(type: "integer", nullable: true),
                    completed_laps = table.Column<int>(type: "integer", nullable: true),
                    is_points_scoring = table.Column<bool>(type: "boolean", nullable: false),
                    results_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    results_published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    weather_summary = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    air_temperature_c = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    track_temperature_c = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    humidity_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    red_flag_count = table.Column<int>(type: "integer", nullable: true),
                    safety_car_count = table.Column<int>(type: "integer", nullable: true),
                    status_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session", x => x.id);
                    table.CheckConstraint("ck_session_actual_time_order", "actual_end_utc is null or actual_start_utc is null or actual_end_utc >= actual_start_utc");
                    table.CheckConstraint("ck_session_scheduled_time_order", "scheduled_end_utc is null or scheduled_end_utc >= scheduled_start_utc");
                    table.CheckConstraint("ck_session_sequence_positive", "sequence_number >= 1");
                    table.ForeignKey(
                        name: "fk_session_race_event_race_event_id",
                        column: x => x.race_event_id,
                        principalSchema: "race",
                        principalTable: "race_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_standing",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    after_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    points_before_drops = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    starts = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    podiums = table.Column<int>(type: "integer", nullable: false),
                    pole_positions = table.Column<int>(type: "integer", nullable: false),
                    fastest_laps = table.Column<int>(type: "integer", nullable: false),
                    previous_position = table.Column<int>(type: "integer", nullable: true),
                    recalculated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_team_standing", x => x.id);
                    table.ForeignKey(
                        name: "fk_team_standing_race_event_after_event_id",
                        column: x => x.after_event_id,
                        principalSchema: "race",
                        principalTable: "race_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_team_standing_season_season_id",
                        column: x => x.season_id,
                        principalSchema: "race",
                        principalTable: "season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_team_standing_team_team_id",
                        column: x => x.team_id,
                        principalSchema: "race",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_team_standing_vehicle_class_vehicle_class_id",
                        column: x => x.vehicle_class_id,
                        principalSchema: "race",
                        principalTable: "vehicle_class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "event_entry_driver",
                schema: "race",
                columns: table => new
                {
                    event_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lineup_position = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    grade = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    is_starting_driver = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_entry_driver", x => new { x.event_entry_id, x.driver_id });
                    table.CheckConstraint("ck_event_entry_driver_lineup_positive", "lineup_position >= 1");
                    table.ForeignKey(
                        name: "fk_event_entry_driver_driver_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "race",
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_event_entry_driver_event_entry_event_entry_id",
                        column: x => x.event_entry_id,
                        principalSchema: "race",
                        principalTable: "event_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lap_record",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    track_layout_id = table.Column<Guid>(type: "uuid", nullable: false),
                    series_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vehicle_class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    record_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    lap_time_ms = table.Column<int>(type: "integer", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    set_on = table.Column<DateOnly>(type: "date", nullable: false),
                    vehicle_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lap_record", x => x.id);
                    table.CheckConstraint("ck_lap_record_time_positive", "lap_time_ms > 0");
                    table.ForeignKey(
                        name: "fk_lap_record_driver_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "race",
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lap_record_series_series_id",
                        column: x => x.series_id,
                        principalSchema: "race",
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lap_record_session_session_id",
                        column: x => x.session_id,
                        principalSchema: "race",
                        principalTable: "session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lap_record_team_team_id",
                        column: x => x.team_id,
                        principalSchema: "race",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lap_record_track_layout_track_layout_id",
                        column: x => x.track_layout_id,
                        principalSchema: "race",
                        principalTable: "track_layout",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lap_record_vehicle_class_vehicle_class_id",
                        column: x => x.vehicle_class_id,
                        principalSchema: "race",
                        principalTable: "vehicle_class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "penalty",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    time_penalty_seconds = table.Column<int>(type: "integer", nullable: true),
                    grid_position_drop = table.Column<int>(type: "integer", nullable: true),
                    licence_points = table.Column<int>(type: "integer", nullable: true),
                    fine_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    fine_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    lap_number = table.Column<int>(type: "integer", nullable: true),
                    issued_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    applied_to_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_served = table.Column<bool>(type: "boolean", nullable: false),
                    is_rescinded = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_penalty", x => x.id);
                    table.ForeignKey(
                        name: "fk_penalty_driver_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "race",
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_penalty_event_entry_event_entry_id",
                        column: x => x.event_entry_id,
                        principalSchema: "race",
                        principalTable: "event_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_penalty_session_applied_to_session_id",
                        column: x => x.applied_to_session_id,
                        principalSchema: "race",
                        principalTable: "session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_penalty_session_session_id",
                        column: x => x.session_id,
                        principalSchema: "race",
                        principalTable: "session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pit_stop",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stop_number = table.Column<int>(type: "integer", nullable: false),
                    lap_number = table.Column<int>(type: "integer", nullable: false),
                    stationary_time_ms = table.Column<int>(type: "integer", nullable: true),
                    total_pit_lane_time_ms = table.Column<int>(type: "integer", nullable: true),
                    tyre_compound_fitted = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    refuelled_stop = table.Column<bool>(type: "boolean", nullable: false),
                    driver_changed = table.Column<bool>(type: "boolean", nullable: false),
                    driver_in_id = table.Column<Guid>(type: "uuid", nullable: true),
                    driver_out_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_under_caution = table.Column<bool>(type: "boolean", nullable: false),
                    is_penalty_served = table.Column<bool>(type: "boolean", nullable: false),
                    note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pit_stop", x => x.id);
                    table.CheckConstraint("ck_pit_stop_number_positive", "stop_number >= 1");
                    table.CheckConstraint("ck_pit_stop_total_covers_stationary", "total_pit_lane_time_ms is null or stationary_time_ms is null or total_pit_lane_time_ms >= stationary_time_ms");
                    table.ForeignKey(
                        name: "fk_pit_stop_driver_driver_in_id",
                        column: x => x.driver_in_id,
                        principalSchema: "race",
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_pit_stop_driver_driver_out_id",
                        column: x => x.driver_out_id,
                        principalSchema: "race",
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_pit_stop_event_entry_event_entry_id",
                        column: x => x.event_entry_id,
                        principalSchema: "race",
                        principalTable: "event_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pit_stop_session_session_id",
                        column: x => x.session_id,
                        principalSchema: "race",
                        principalTable: "session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_lap",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lap_number = table.Column<int>(type: "integer", nullable: false),
                    lap_time_ms = table.Column<int>(type: "integer", nullable: true),
                    sector1_ms = table.Column<int>(type: "integer", nullable: true),
                    sector2_ms = table.Column<int>(type: "integer", nullable: true),
                    sector3_ms = table.Column<int>(type: "integer", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: true),
                    speed_trap_kph = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    tyre_compound = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    tyre_age_laps = table.Column<int>(type: "integer", nullable: true),
                    is_pit_in_lap = table.Column<bool>(type: "boolean", nullable: false),
                    is_pit_out_lap = table.Column<bool>(type: "boolean", nullable: false),
                    is_personal_best = table.Column<bool>(type: "boolean", nullable: false),
                    is_session_best = table.Column<bool>(type: "boolean", nullable: false),
                    is_under_caution = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted_by_stewards = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session_lap", x => x.id);
                    table.CheckConstraint("ck_session_lap_number_positive", "lap_number >= 1");
                    table.CheckConstraint("ck_session_lap_time_positive", "lap_time_ms is null or lap_time_ms > 0");
                    table.ForeignKey(
                        name: "fk_session_lap_driver_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "race",
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_session_lap_event_entry_event_entry_id",
                        column: x => x.event_entry_id,
                        principalSchema: "race",
                        principalTable: "event_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_session_lap_session_session_id",
                        column: x => x.session_id,
                        principalSchema: "race",
                        principalTable: "session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_result",
                schema: "race",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: true),
                    class_position = table.Column<int>(type: "integer", nullable: true),
                    starting_position = table.Column<int>(type: "integer", nullable: true),
                    laps_completed = table.Column<int>(type: "integer", nullable: false),
                    laps_led = table.Column<int>(type: "integer", nullable: false),
                    total_time_ms = table.Column<long>(type: "bigint", nullable: true),
                    gap_to_leader_ms = table.Column<long>(type: "bigint", nullable: true),
                    gap_to_leader_laps = table.Column<int>(type: "integer", nullable: true),
                    gap_to_ahead_ms = table.Column<long>(type: "bigint", nullable: true),
                    best_lap_time_ms = table.Column<int>(type: "integer", nullable: true),
                    best_lap_number = table.Column<int>(type: "integer", nullable: true),
                    average_speed_kph = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    top_speed_kph = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status_detail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_classified = table.Column<bool>(type: "boolean", nullable: false),
                    points_scored = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    is_fastest_lap = table.Column<bool>(type: "boolean", nullable: false),
                    is_pole_position = table.Column<bool>(type: "boolean", nullable: false),
                    pit_stop_count = table.Column<int>(type: "integer", nullable: false),
                    penalty_time_ms = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session_result", x => x.id);
                    table.CheckConstraint("ck_session_result_laps_led_within_completed", "laps_led <= laps_completed");
                    table.CheckConstraint("ck_session_result_laps_not_negative", "laps_completed >= 0 and laps_led >= 0");
                    table.CheckConstraint("ck_session_result_position_positive", "position is null or position >= 1");
                    table.ForeignKey(
                        name: "fk_session_result_event_entry_event_entry_id",
                        column: x => x.event_entry_id,
                        principalSchema: "race",
                        principalTable: "event_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_session_result_session_session_id",
                        column: x => x.session_id,
                        principalSchema: "race",
                        principalTable: "session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_result_driver",
                schema: "race",
                columns: table => new
                {
                    session_result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    laps_driven = table.Column<int>(type: "integer", nullable: false),
                    best_lap_time_ms = table.Column<int>(type: "integer", nullable: true),
                    time_driven_ms = table.Column<long>(type: "bigint", nullable: true),
                    points_scored = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    is_eligible_for_points = table.Column<bool>(type: "boolean", nullable: false),
                    set_best_lap = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session_result_driver", x => new { x.session_result_id, x.driver_id });
                    table.CheckConstraint("ck_session_result_driver_laps_not_negative", "laps_driven >= 0");
                    table.ForeignKey(
                        name: "fk_session_result_driver_driver_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "race",
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_session_result_driver_session_result_session_result_id",
                        column: x => x.session_result_id,
                        principalSchema: "race",
                        principalTable: "session_result",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_driver_active_last_name",
                schema: "race",
                table: "driver",
                columns: new[] { "is_active", "last_name" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_driver_full_name",
                schema: "race",
                table: "driver",
                column: "full_name",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_driver_permanent_number",
                schema: "race",
                table: "driver",
                column: "permanent_number",
                filter: "permanent_number is not null and is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_driver_slug",
                schema: "race",
                table: "driver",
                column: "slug",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_driver_career_stat_series_wins",
                schema: "race",
                table: "driver_career_stat",
                columns: new[] { "series_id", "wins" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ux_driver_career_stat_driver_overall",
                schema: "race",
                table: "driver_career_stat",
                column: "driver_id",
                unique: true,
                filter: "series_id is null");

            migrationBuilder.CreateIndex(
                name: "ux_driver_career_stat_driver_series",
                schema: "race",
                table: "driver_career_stat",
                columns: new[] { "driver_id", "series_id" },
                unique: true,
                filter: "series_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_driver_standing_after_event_id",
                schema: "race",
                table: "driver_standing",
                column: "after_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_driver_standing_current_by_position",
                schema: "race",
                table: "driver_standing",
                columns: new[] { "season_id", "position" },
                filter: "after_event_id is null");

            migrationBuilder.CreateIndex(
                name: "ix_driver_standing_driver_season",
                schema: "race",
                table: "driver_standing",
                columns: new[] { "driver_id", "season_id" });

            migrationBuilder.CreateIndex(
                name: "ix_driver_standing_vehicle_class_id",
                schema: "race",
                table: "driver_standing",
                column: "vehicle_class_id");

            migrationBuilder.CreateIndex(
                name: "ux_driver_standing_season_driver_class_event",
                schema: "race",
                table: "driver_standing",
                columns: new[] { "season_id", "driver_id", "vehicle_class_id", "after_event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entry_chassis_manufacturer_id",
                schema: "race",
                table: "entry",
                column: "chassis_manufacturer_id");

            migrationBuilder.CreateIndex(
                name: "ix_entry_engine_manufacturer_id",
                schema: "race",
                table: "entry",
                column: "engine_manufacturer_id");

            migrationBuilder.CreateIndex(
                name: "ix_entry_season_class_number",
                schema: "race",
                table: "entry",
                columns: new[] { "season_id", "vehicle_class_id", "car_number" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_entry_submitted_by_user",
                schema: "race",
                table: "entry",
                column: "submitted_by_user_id",
                filter: "submitted_by_user_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_entry_team_season",
                schema: "race",
                table: "entry",
                columns: new[] { "team_id", "season_id" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_entry_tyre_supplier_id",
                schema: "race",
                table: "entry",
                column: "tyre_supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_entry_vehicle_class_id",
                schema: "race",
                table: "entry",
                column: "vehicle_class_id");

            migrationBuilder.CreateIndex(
                name: "ux_entry_season_car_number",
                schema: "race",
                table: "entry",
                columns: new[] { "season_id", "car_number" },
                unique: true,
                filter: "is_deleted = false and status <> 'Withdrawn'");

            migrationBuilder.CreateIndex(
                name: "ix_event_entry_entry",
                schema: "race",
                table: "event_entry",
                column: "entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_entry_event_grid",
                schema: "race",
                table: "event_entry",
                columns: new[] { "race_event_id", "starting_grid_position" },
                filter: "starting_grid_position is not null");

            migrationBuilder.CreateIndex(
                name: "ix_event_entry_event_status",
                schema: "race",
                table: "event_entry",
                columns: new[] { "race_event_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_event_entry_event_entry",
                schema: "race",
                table: "event_entry",
                columns: new[] { "race_event_id", "entry_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_entry_driver_driver",
                schema: "race",
                table: "event_entry_driver",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "ux_event_entry_driver_lineup_position",
                schema: "race",
                table: "event_entry_driver",
                columns: new[] { "event_entry_id", "lineup_position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_event_entry_driver_single_starter",
                schema: "race",
                table: "event_entry_driver",
                column: "event_entry_id",
                unique: true,
                filter: "is_starting_driver = true");

            migrationBuilder.CreateIndex(
                name: "ix_external_reference_type_internal",
                schema: "race",
                table: "external_reference",
                columns: new[] { "entity_type", "internal_id" });

            migrationBuilder.CreateIndex(
                name: "ux_external_reference_provider_type_external",
                schema: "race",
                table: "external_reference",
                columns: new[] { "provider", "entity_type", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_import_run_in_progress",
                schema: "race",
                table: "import_run",
                column: "status",
                filter: "status = 'Running'");

            migrationBuilder.CreateIndex(
                name: "ix_import_run_provider_resource_started",
                schema: "race",
                table: "import_run",
                columns: new[] { "provider", "resource", "started_at_utc" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_lap_record_driver_id",
                schema: "race",
                table: "lap_record",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_lap_record_layout_series_type_time",
                schema: "race",
                table: "lap_record",
                columns: new[] { "track_layout_id", "series_id", "record_type", "lap_time_ms" });

            migrationBuilder.CreateIndex(
                name: "ix_lap_record_layout_type_current",
                schema: "race",
                table: "lap_record",
                columns: new[] { "track_layout_id", "record_type", "is_current" });

            migrationBuilder.CreateIndex(
                name: "ix_lap_record_series_id",
                schema: "race",
                table: "lap_record",
                column: "series_id");

            migrationBuilder.CreateIndex(
                name: "ix_lap_record_session_id",
                schema: "race",
                table: "lap_record",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_lap_record_team_id",
                schema: "race",
                table: "lap_record",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_lap_record_vehicle_class_id",
                schema: "race",
                table: "lap_record",
                column: "vehicle_class_id");

            migrationBuilder.CreateIndex(
                name: "ix_manufacturer_chassis_active",
                schema: "race",
                table: "manufacturer",
                columns: new[] { "supplies_chassis", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_manufacturer_engines_active",
                schema: "race",
                table: "manufacturer",
                columns: new[] { "supplies_engines", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ux_manufacturer_slug",
                schema: "race",
                table: "manufacturer",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_penalty_applied_session_served",
                schema: "race",
                table: "penalty",
                columns: new[] { "applied_to_session_id", "is_served" },
                filter: "applied_to_session_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_penalty_driver_issued",
                schema: "race",
                table: "penalty",
                columns: new[] { "driver_id", "issued_at_utc" },
                filter: "driver_id is not null and is_rescinded = false");

            migrationBuilder.CreateIndex(
                name: "ix_penalty_event_entry_id",
                schema: "race",
                table: "penalty",
                column: "event_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_penalty_session_entry",
                schema: "race",
                table: "penalty",
                columns: new[] { "session_id", "event_entry_id" });

            migrationBuilder.CreateIndex(
                name: "ix_pit_stop_driver_in_id",
                schema: "race",
                table: "pit_stop",
                column: "driver_in_id");

            migrationBuilder.CreateIndex(
                name: "ix_pit_stop_driver_out_id",
                schema: "race",
                table: "pit_stop",
                column: "driver_out_id");

            migrationBuilder.CreateIndex(
                name: "ix_pit_stop_event_entry_id",
                schema: "race",
                table: "pit_stop",
                column: "event_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_pit_stop_session_lap",
                schema: "race",
                table: "pit_stop",
                columns: new[] { "session_id", "lap_number" });

            migrationBuilder.CreateIndex(
                name: "ix_pit_stop_session_stationary_time",
                schema: "race",
                table: "pit_stop",
                columns: new[] { "session_id", "stationary_time_ms" },
                filter: "stationary_time_ms is not null");

            migrationBuilder.CreateIndex(
                name: "ux_pit_stop_session_entry_number",
                schema: "race",
                table: "pit_stop",
                columns: new[] { "session_id", "event_entry_id", "stop_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_points_scheme_season_session_championship",
                schema: "race",
                table: "points_scheme",
                columns: new[] { "season_id", "applies_to_session_type", "championship_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_points_scheme_entry_scheme_position",
                schema: "race",
                table: "points_scheme_entry",
                columns: new[] { "points_scheme_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_race_event_layout_start",
                schema: "race",
                table: "race_event",
                columns: new[] { "track_layout_id", "start_time_utc" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_race_event_previous_edition_id",
                schema: "race",
                table: "race_event",
                column: "previous_edition_id");

            migrationBuilder.CreateIndex(
                name: "ix_race_event_published_start",
                schema: "race",
                table: "race_event",
                columns: new[] { "is_published", "start_time_utc" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_race_event_season_start",
                schema: "race",
                table: "race_event",
                columns: new[] { "season_id", "start_time_utc" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_race_event_status_start",
                schema: "race",
                table: "race_event",
                columns: new[] { "status", "start_time_utc" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_race_event_season_round",
                schema: "race",
                table: "race_event",
                columns: new[] { "season_id", "round_number" },
                unique: true,
                filter: "is_deleted = false and round_number is not null");

            migrationBuilder.CreateIndex(
                name: "ux_race_event_season_slug",
                schema: "race",
                table: "race_event",
                columns: new[] { "season_id", "slug" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_season_status_year",
                schema: "race",
                table: "season",
                columns: new[] { "status", "year" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_season_series_slug",
                schema: "race",
                table: "season",
                columns: new[] { "series_id", "slug" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_season_series_year",
                schema: "race",
                table: "season",
                columns: new[] { "series_id", "year" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_series_active_discipline_order",
                schema: "race",
                table: "series",
                columns: new[] { "is_active", "discipline", "display_order" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_series_slug",
                schema: "race",
                table: "series",
                column: "slug",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_session_event_order",
                schema: "race",
                table: "session",
                columns: new[] { "race_event_id", "weekend_order" });

            migrationBuilder.CreateIndex(
                name: "ix_session_results_status_start",
                schema: "race",
                table: "session",
                columns: new[] { "results_status", "scheduled_start_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_session_scheduled_start",
                schema: "race",
                table: "session",
                column: "scheduled_start_utc");

            migrationBuilder.CreateIndex(
                name: "ix_session_status_scheduled_start",
                schema: "race",
                table: "session",
                columns: new[] { "status", "scheduled_start_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_session_event_type_sequence",
                schema: "race",
                table: "session",
                columns: new[] { "race_event_id", "type", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_session_lap_driver_session",
                schema: "race",
                table: "session_lap",
                columns: new[] { "driver_id", "session_id" },
                filter: "driver_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_session_lap_event_entry_id",
                schema: "race",
                table: "session_lap",
                column: "event_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_session_lap_session_lap_number",
                schema: "race",
                table: "session_lap",
                columns: new[] { "session_id", "lap_number" });

            migrationBuilder.CreateIndex(
                name: "ix_session_lap_session_time",
                schema: "race",
                table: "session_lap",
                columns: new[] { "session_id", "lap_time_ms" },
                filter: "lap_time_ms is not null and is_deleted_by_stewards = false");

            migrationBuilder.CreateIndex(
                name: "ux_session_lap_session_entry_lap",
                schema: "race",
                table: "session_lap",
                columns: new[] { "session_id", "event_entry_id", "lap_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_session_result_event_entry",
                schema: "race",
                table: "session_result",
                column: "event_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_session_result_session_order",
                schema: "race",
                table: "session_result",
                columns: new[] { "session_id", "position", "class_position" });

            migrationBuilder.CreateIndex(
                name: "ux_session_result_session_entry",
                schema: "race",
                table: "session_result",
                columns: new[] { "session_id", "event_entry_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_session_result_session_position",
                schema: "race",
                table: "session_result",
                columns: new[] { "session_id", "position" },
                unique: true,
                filter: "position is not null");

            migrationBuilder.CreateIndex(
                name: "ux_session_result_single_fastest_lap",
                schema: "race",
                table: "session_result",
                column: "session_id",
                unique: true,
                filter: "is_fastest_lap = true");

            migrationBuilder.CreateIndex(
                name: "ux_session_result_single_pole",
                schema: "race",
                table: "session_result",
                column: "session_id",
                unique: true,
                filter: "is_pole_position = true");

            migrationBuilder.CreateIndex(
                name: "ix_session_result_driver_driver_points",
                schema: "race",
                table: "session_result_driver",
                columns: new[] { "driver_id", "points_scored" });

            migrationBuilder.CreateIndex(
                name: "ix_team_name",
                schema: "race",
                table: "team",
                column: "name",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_team_slug",
                schema: "race",
                table: "team",
                column: "slug",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_team_standing_after_event_id",
                schema: "race",
                table: "team_standing",
                column: "after_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_team_standing_current_by_position",
                schema: "race",
                table: "team_standing",
                columns: new[] { "season_id", "position" },
                filter: "after_event_id is null");

            migrationBuilder.CreateIndex(
                name: "ix_team_standing_team_id",
                schema: "race",
                table: "team_standing",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_team_standing_vehicle_class_id",
                schema: "race",
                table: "team_standing",
                column: "vehicle_class_id");

            migrationBuilder.CreateIndex(
                name: "ux_team_standing_season_team_class_event",
                schema: "race",
                table: "team_standing",
                columns: new[] { "season_id", "team_id", "vehicle_class_id", "after_event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_track_country_name",
                schema: "race",
                table: "track",
                columns: new[] { "country", "name" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_track_slug",
                schema: "race",
                table: "track",
                column: "slug",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_track_layout_single_current",
                schema: "race",
                table: "track_layout",
                column: "track_id",
                unique: true,
                filter: "is_current = true");

            migrationBuilder.CreateIndex(
                name: "ux_track_layout_track_slug",
                schema: "race",
                table: "track_layout",
                columns: new[] { "track_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_class_series_order",
                schema: "race",
                table: "vehicle_class",
                columns: new[] { "series_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "ux_vehicle_class_global_slug",
                schema: "race",
                table: "vehicle_class",
                column: "slug",
                unique: true,
                filter: "series_id is null");

            migrationBuilder.CreateIndex(
                name: "ux_vehicle_class_series_slug",
                schema: "race",
                table: "vehicle_class",
                columns: new[] { "series_id", "slug" },
                unique: true,
                filter: "series_id is not null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "driver_career_stat",
                schema: "race");

            migrationBuilder.DropTable(
                name: "driver_standing",
                schema: "race");

            migrationBuilder.DropTable(
                name: "event_entry_driver",
                schema: "race");

            migrationBuilder.DropTable(
                name: "external_reference",
                schema: "race");

            migrationBuilder.DropTable(
                name: "import_run",
                schema: "race");

            migrationBuilder.DropTable(
                name: "lap_record",
                schema: "race");

            migrationBuilder.DropTable(
                name: "penalty",
                schema: "race");

            migrationBuilder.DropTable(
                name: "pit_stop",
                schema: "race");

            migrationBuilder.DropTable(
                name: "points_scheme_entry",
                schema: "race");

            migrationBuilder.DropTable(
                name: "session_lap",
                schema: "race");

            migrationBuilder.DropTable(
                name: "session_result_driver",
                schema: "race");

            migrationBuilder.DropTable(
                name: "team_standing",
                schema: "race");

            migrationBuilder.DropTable(
                name: "points_scheme",
                schema: "race");

            migrationBuilder.DropTable(
                name: "driver",
                schema: "race");

            migrationBuilder.DropTable(
                name: "session_result",
                schema: "race");

            migrationBuilder.DropTable(
                name: "event_entry",
                schema: "race");

            migrationBuilder.DropTable(
                name: "session",
                schema: "race");

            migrationBuilder.DropTable(
                name: "entry",
                schema: "race");

            migrationBuilder.DropTable(
                name: "race_event",
                schema: "race");

            migrationBuilder.DropTable(
                name: "manufacturer",
                schema: "race");

            migrationBuilder.DropTable(
                name: "team",
                schema: "race");

            migrationBuilder.DropTable(
                name: "vehicle_class",
                schema: "race");

            migrationBuilder.DropTable(
                name: "season",
                schema: "race");

            migrationBuilder.DropTable(
                name: "track_layout",
                schema: "race");

            migrationBuilder.DropTable(
                name: "series",
                schema: "race");

            migrationBuilder.DropTable(
                name: "track",
                schema: "race");
        }
    }
}
