using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RaceService.Api.Authorization;
using RaceService.Application.Abstractions;

namespace RaceService.Api.Extensions;

/// <summary>Wires up the HTTP-facing concerns of the service.</summary>
public static class ApiServiceExtensions
{
    /// <summary>Adds versioning, auth, caching, rate limiting and CORS.</summary>
    public static IServiceCollection AddRaceServiceApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Model-binding failures return the same RFC 9457 shape as domain failures, so a
                // client has one error format to handle rather than two.
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problem = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation.Failed",
                        Instance = context.HttpContext.Request.Path
                    };

                    problem.Extensions["code"] = "Validation.Failed";
                    problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                    return new BadRequestObjectResult(problem);
                };
            });

        services.AddProblemDetails();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "RacingVacing RaceService",
                Version = "v1",
                Description = "Race calendars, sessions, entries, results and championship standings."
            });

            // XML comments become the endpoint descriptions, so the docs are written next to the
            // code they describe rather than drifting in a separate file.
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "RaceService.Api.xml");

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            var contractsXml = Path.Combine(AppContext.BaseDirectory, "RaceService.Contracts.xml");

            if (File.Exists(contractsXml))
            {
                options.IncludeXmlComments(contractsXml);
            }

            options.AddSecurityDefinition("Bearer", new()
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT issued by the UserService."
            });

            options.AddSecurityRequirement(new()
            {
                [new()
                {
                    Reference = new()
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = []
            });
        });

        AddAuthentication(services, configuration);
        AddAuthorization(services);
        AddCaching(services);
        AddRateLimiting(services);
        AddCors(services, configuration);

        services.AddHealthChecks()
            .AddDbContextCheck<Application.Database.ApplicationDbContext>("database");

        return services;
    }

    /// <summary>
    /// Validates tokens issued by the UserService.
    /// </summary>
    /// <remarks>
    /// This service verifies signatures; it never issues them. In production the authority is the
    /// UserService's OpenID configuration and keys are fetched and rotated automatically. The
    /// symmetric-key path exists only for local development, where running a second service just
    /// to call a race endpoint is friction with no security benefit.
    /// </remarks>
    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Jwt:Authority"];
        var signingKey = configuration["Jwt:SigningKey"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                if (!string.IsNullOrWhiteSpace(authority))
                {
                    options.Authority = authority;
                    options.RequireHttpsMetadata = true;
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RoleClaimType = "role",
                    NameClaimType = "sub",
                    // Tokens are short-lived; the five-minute default skew is far too generous.
                    ClockSkew = TimeSpan.FromSeconds(30),
                    IssuerSigningKey = string.IsNullOrWhiteSpace(signingKey)
                        ? null
                        : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
                };
            });
    }

    private static void AddAuthorization(IServiceCollection services)
        => services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Administrator, policy => policy.RequireRole(Roles.Administrator))
            // An administrator can do anything an editor can, so the editor policy accepts both
            // rather than requiring admins to hold two roles.
            .AddPolicy(Policies.Editor, policy => policy.RequireRole(Roles.Administrator, Roles.Editor))
            .AddPolicy(Policies.Entrant, policy => policy.RequireRole(Roles.Administrator, Roles.Editor, Roles.Entrant));

    /// <summary>
    /// Output caching for read endpoints.
    /// </summary>
    /// <remarks>
    /// Calendars and results are read far more often than they change, and the same handful of
    /// queries dominate traffic. Caching varies on the query string and on the requested display
    /// time zone, since two callers asking for different zones must not share a response.
    /// Authenticated requests are never cached, so an admin previewing a draft cannot have it
    /// served to the public.
    /// </remarks>
    private static void AddCaching(IServiceCollection services)
        => services.AddOutputCache(options =>
        {
            options.AddPolicy("catalog", builder => builder
                .Expire(TimeSpan.FromMinutes(10))
                .SetVaryByQuery("*")
                .Cache());

            options.AddPolicy("races", builder => builder
                .Expire(TimeSpan.FromMinutes(2))
                .SetVaryByQuery("*")
                .Cache());

            options.AddPolicy("results", builder => builder
                .Expire(TimeSpan.FromMinutes(5))
                .SetVaryByQuery("*")
                .Cache());

            // A live session changes minute to minute, so this window is short enough to stay
            // useful while still absorbing a traffic spike during a race.
            options.AddPolicy("live", builder => builder
                .Expire(TimeSpan.FromSeconds(20))
                .SetVaryByQuery("*")
                .Cache());
        });

    private static void AddRateLimiting(IServiceCollection services)
        => services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    // Partition by user where there is one, by IP otherwise, so one noisy
                    // anonymous client cannot exhaust the budget of every signed-in user.
                    context.User.Identity?.IsAuthenticated == true
                        ? context.User.FindFirst("sub")?.Value ?? "authenticated"
                        : context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            if (origins.Length > 0)
            {
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            }
            else
            {
                // No configured origins means no browser client is expected. Falling back to
                // AllowAnyOrigin here would quietly make a misconfigured production deployment
                // world-readable from any site.
                policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
            }
        }));
    }
}
