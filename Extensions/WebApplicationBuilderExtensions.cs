using System.Diagnostics.CodeAnalysis;
using System.Text;
using Asp.Versioning;
using FlagForge.Auth;
using FlagForge.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewRelic.LogEnrichers.Serilog;
using Serilog;

namespace FlagForge.Extensions;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderExtensions
{
    private const string CorsAllowedOrigin = "CorsSettings:AllowedOrigin";
    private const int DefaultApiVersion = 1;
    private const string DefaultConnectionString = "DefaultConnectionString";

    public static void AddCustomCors(this WebApplicationBuilder builder)
    {
        var allowedOrigins =
            builder.Configuration.GetValue<string>(CorsAllowedOrigin)?.Split(",").ToArray()
            ?? throw new InvalidOperationException($"{CorsAllowedOrigin} is empty or null");

        Log.Information("Allowed Origins: {AllowedOrigins}", allowedOrigins);

        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("Allowed Origins is not setup properly");
        }

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
            });
        });
    }

    public static void AddJwtValidation(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection(JwtOptions.SectionName)
        );

        var jwtOptions =
            builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Missing configuration: Jwt");

        if (
            string.IsNullOrWhiteSpace(jwtOptions.Key)
            || Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32
        )
        {
            throw new InvalidOperationException("Jwt:Key must be at least 32 bytes.");
        }

        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Key)
                    ),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });
    }

    public static void AddApiVersioning(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(DefaultApiVersion, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(setup =>
            {
                setup.AddApiVersionParametersWhenVersionNeutral = true;
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });
    }

    public static void ConfigurePostgres(this WebApplicationBuilder builder)
    {
        var connectionString =
            builder.Configuration.GetConnectionString(DefaultConnectionString)
            ?? throw new InvalidOperationException(
                "Missing configuration : DefaultConnectionString"
            );

        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    }

    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.WithNewRelicLogsInContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
