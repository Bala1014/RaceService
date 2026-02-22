using System;
using System.Collections.Generic;
using System.Text;

namespace RaceService.Application.Helpers
{
    public static class PostgresHelper
    {
        public static string GetPostgresConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "raceservice";
            var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "bala99";
            return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }
    }
}
