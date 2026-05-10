using RaceService.Api.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace RaceService.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthorization();
        builder.Services.AddControllers(); 
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();
        builder.Services.AddSwaggerGen();
        builder.Services.AddPostgres();

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.MapSwagger("/openapi/{documentName}.json");
            app.MapScalarApiReference();
        }
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.ApplyMigrations();
        app.MapControllers();           
        app.MapHealthChecks("/health");            
        app.Run();
    }
}
