using RaceService.Application.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using RaceService.Application.Helpers;
namespace RaceService.Api.Extensions
{
    public static class PostgresExtension
    {

        public static IServiceCollection AddPostgres(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(PostgresHelper.GetPostgresConnectionString()
                ,
                    b => b.MigrationsAssembly("RaceService.Application"))
                );
            services.AddTransient<DbInitializer>();
            return services;
        }

        public static void ApplyMigrations(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                dbContext.Init();
            }
        }
    }
}
