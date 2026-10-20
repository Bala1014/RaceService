using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RaceService.Application.Database;

/// <summary>
/// Builds a context for <c>dotnet ef</c> at design time.
/// </summary>
/// <remarks>
/// Without this the CLI has to boot the API host to find a context, which means design-time
/// tooling fails whenever the host cannot start — no configuration, no secrets, no network. The
/// connection string here is never used to run migrations against a real database; EF only needs
/// a provider to know which SQL dialect to generate.
/// </remarks>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <inheritdoc />
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("RACESERVICE_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=raceservice;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .Options;

        return new ApplicationDbContext(options);
    }
}
