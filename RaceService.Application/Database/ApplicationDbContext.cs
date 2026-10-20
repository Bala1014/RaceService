using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RaceService.Application.Database.Conventions;
using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Application.Domain.Entities.Integration;
using RaceService.Application.Domain.Entities.Results;
using RaceService.Application.Domain.Entities.Standings;
using RaceService.Application.Domain.Entities.Venues;

namespace RaceService.Application.Database;

/// <summary>
/// The RaceService persistence context. Owns one Postgres schema, <c>race</c>, and nothing else:
/// identity and forum data belong to their own services and are never joined to from here.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>Schema every table in this service lives in.</summary>
    public const string Schema = "race";

    /// <summary>Initialises the context.</summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Catalog
    public DbSet<Series> Series => Set<Series>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<PointsScheme> PointsSchemes => Set<PointsScheme>();
    public DbSet<PointsSchemeEntry> PointsSchemeEntries => Set<PointsSchemeEntry>();
    public DbSet<VehicleClass> VehicleClasses => Set<VehicleClass>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();

    // Venues
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<TrackLayout> TrackLayouts => Set<TrackLayout>();
    public DbSet<LapRecord> LapRecords => Set<LapRecord>();

    // Events
    public DbSet<RaceEvent> RaceEvents => Set<RaceEvent>();
    public DbSet<Session> Sessions => Set<Session>();

    // Competitors
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DriverCareerStat> DriverCareerStats => Set<DriverCareerStat>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<EventEntry> EventEntries => Set<EventEntry>();
    public DbSet<EventEntryDriver> EventEntryDrivers => Set<EventEntryDriver>();

    // Results
    public DbSet<SessionResult> SessionResults => Set<SessionResult>();
    public DbSet<SessionResultDriver> SessionResultDrivers => Set<SessionResultDriver>();
    public DbSet<SessionLap> SessionLaps => Set<SessionLap>();
    public DbSet<PitStop> PitStops => Set<PitStop>();
    public DbSet<Penalty> Penalties => Set<Penalty>();

    // Standings
    public DbSet<DriverStanding> DriverStandings => Set<DriverStanding>();
    public DbSet<TeamStanding> TeamStandings => Set<TeamStanding>();

    // Integration
    public DbSet<ExternalReference> ExternalReferences => Set<ExternalReference>();
    public DbSet<ImportRun> ImportRuns => Set<ImportRun>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ApplyClientGeneratedKeys(modelBuilder);
        ApplyEnumsAsText(modelBuilder);
        ApplySoftDeleteFilters(modelBuilder);

        // Must run last: it renames what every step above has produced.
        modelBuilder.ApplySnakeCaseNames();
    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Every timestamp in this service is UTC. See UtcDateTimeConverter for why this is
        // enforced at the boundary rather than trusted to callers.
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>()
            .HaveColumnType("timestamp with time zone");

        // Points can be fractional but never need more than two places; temperatures and speeds
        // are the same shape. Individual properties override this where they need to.
        configurationBuilder.Properties<decimal>()
            .HavePrecision(10, 3);

        // Slugs, country codes and colour hexes are all short, bounded strings.
        configurationBuilder.Properties<string>()
            .HaveMaxLength(512);
    }

    /// <summary>
    /// Declares primary keys as never store-generated.
    /// </summary>
    /// <remarks>
    /// Keys are UUID v7 values assigned in the constructor, so the database never produces one.
    /// Saying so matters beyond documentation: EF's default for a Guid key is
    /// <c>ValueGeneratedOnAdd</c>, and when a new entity reaches the context through a tracked
    /// parent's navigation rather than through <c>DbSet.Add</c>, EF decides whether it is an
    /// insert or an update by asking whether the key is already set. With a client-generated key
    /// that answer is always yes, so EF emits an UPDATE for a row that does not exist and the
    /// insert of its children fails on a foreign key. <c>ValueGeneratedNever</c> removes the
    /// ambiguity and the whole class of bug with it.
    /// </remarks>
    private static void ApplyClientGeneratedKeys(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var key = entityType.FindPrimaryKey();

            if (key is { Properties.Count: 1 } && key.Properties[0].Name == nameof(Entity.Id))
            {
                key.Properties[0].ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
            }
        }
    }

    /// <summary>
    /// Stores enums as text rather than as integers.
    /// </summary>
    /// <remarks>
    /// An integer enum column makes the database unreadable without the C# source to hand, and
    /// worse, it makes reordering an enum member a silent data corruption. Text costs a few bytes
    /// and makes <c>select status, count(*) from race.session group by 1</c> answerable by anyone.
    /// </remarks>
    private static void ApplyEnumsAsText(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

                if (type.IsEnum)
                {
                    property.SetProviderClrType(typeof(string));
                    property.SetMaxLength(40);
                }
            }
        }
    }

    /// <summary>
    /// Adds <c>where is_deleted = false</c> to every query over a soft-deletable entity, so a
    /// forgotten filter cannot leak archived records into a public response.
    /// </summary>
    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType) || entityType.BaseType is not null)
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var body = Expression.Equal(
                Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted)),
                Expression.Constant(false));

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }
}
