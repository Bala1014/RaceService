using System.Text.Json.Serialization;
using RaceService.Api.Extensions;
using RaceService.Api.Middleware;
using RaceService.Application;
using RaceService.Application.Database;
using Scalar.AspNetCore;
using Serilog;

namespace RaceService.Api;

/// <summary>Entry point for the RaceService API.</summary>
public class Program
{
    /// <summary>Builds and runs the web host.</summary>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Structured logging from the very first line, so a start-up failure is diagnosable
        // rather than a bare stack trace on stderr.
        builder.Host.UseSerilog((context, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            // Enums travel as strings. An integer would tie every client to the declaration order
            // of a C# enum, which is exactly the coupling a versioned contract exists to avoid.
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        builder.Services.AddRaceServiceApplication(builder.Configuration);
        builder.Services.AddRaceServiceApi(builder.Configuration);

        var app = builder.Build();

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options => options.RouteTemplate = "openapi/{documentName}.json");
            app.MapScalarApiReference(options => options
                .WithTitle("RacingVacing RaceService")
                .WithOpenApiRoutePattern("/openapi/{documentName}.json"));
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseRateLimiter();

        // Order matters: output caching sits after authentication so a cached public response is
        // never served in place of an authenticated one.
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseOutputCache();

        app.MapControllers();

        // Liveness answers "is the process up"; readiness additionally checks the database, which
        // is what a load balancer should gate traffic on.
        app.MapHealthChecks("/health/live", new()
        {
            Predicate = _ => false
        });

        app.MapHealthChecks("/health/ready");

        await MigrateAndSeedAsync(app);

        await app.RunAsync();
    }

    /// <summary>
    /// Applies migrations and, outside production, seeds reference data.
    /// </summary>
    /// <remarks>
    /// Migrating on start-up is convenient for a single-instance deployment and is what this
    /// service does today. It is worth knowing the limit: with several instances starting at
    /// once they race for the migration lock, and a failed migration takes the process down. When
    /// this service scales out, move this to a job that runs once before the rollout.
    /// </remarks>
    private static async Task MigrateAndSeedAsync(WebApplication app)
    {
        if (app.Configuration.GetValue("Database:MigrateOnStartup", true) is false)
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();

        var seed = app.Configuration.GetValue("Database:SeedOnStartup", !app.Environment.IsProduction());

        await initializer.InitialiseAsync(seed);
    }
}
