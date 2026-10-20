using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaceService.Application.Abstractions;
using RaceService.Application.Database;
using RaceService.Application.Database.Interceptors;
using RaceService.Application.Services;

namespace RaceService.Application;

/// <summary>Registers the application and persistence layers.</summary>
public static class DependencyInjection
{
    /// <summary>Adds the DbContext, interceptors and domain services.</summary>
    public static IServiceCollection AddRaceServiceApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<AuditingInterceptor>();

        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            options.UseNpgsql(
                ResolveConnectionString(configuration),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", ApplicationDbContext.Schema);
                    // A transient network blip should not surface as a 500 on a read endpoint.
                    npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null);
                });

            options.AddInterceptors(provider.GetRequiredService<AuditingInterceptor>());

            // Read endpoints never mutate, and skipping change tracking on them is the single
            // largest easy win for a read-heavy site. Write paths opt back in explicitly.
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            options.ConfigureWarnings(warnings =>
            {
                // Child rows such as sessions and results are reached only through an event that
                // already carries the soft-delete filter, so EF's caution here is noise.
                warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
            });
        });

        services.AddScoped<DatabaseInitializer>();

        services.AddScoped<ISlugService, SlugService>();
        services.AddScoped<ITimeZoneService, TimeZoneService>();
        services.AddScoped<ISeriesService, SeriesService>();
        services.AddScoped<ISeasonService, SeasonService>();
        services.AddScoped<ITrackService, TrackService>();
        services.AddScoped<IRaceEventService, RaceEventService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IEntryService, EntryService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IStandingsService, StandingsService>();
        services.AddScoped<IPointsCalculator, PointsCalculator>();

        return services;
    }

    /// <summary>
    /// Prefers a standard connection string, then falls back to the discrete environment
    /// variables the container images set. Cloud Run injects the latter, a developer's machine
    /// typically has the former.
    /// </summary>
    private static string ResolveConnectionString(IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString("Postgres");

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        var host = configuration["POSTGRES_HOST"] ?? "localhost";
        var port = configuration["POSTGRES_PORT"] ?? "5432";
        var database = configuration["POSTGRES_DB"] ?? "raceservice";
        var username = configuration["POSTGRES_USER"] ?? "postgres";
        var password = configuration["POSTGRES_PASSWORD"] ?? "postgres";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}
