using RaceService.Api.Extensions;

namespace RaceService.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthorization();


        builder.Services.AddPostgres();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.ApplyMigrations();
        app.Run();
    }
}
