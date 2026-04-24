using System.Text.Json;
using FlagForge.Data.Persistence;
using FlagForge.Data.Persistence.Interfaces;
using FlagForge.Data.Services;
using FlagForge.Extensions;
using FlagForge.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddTransient<FeatureFlagService>();
builder.Services.AddTransient<TenantService>();
builder.Services.AddTransient<EnvironmentService>();
builder.Services.AddTransient<AuthService>();
builder.Services.AddScoped<IDbExceptionTranslator, DbExceptionTranslator>();
builder.AddJwtValidation();
builder.AddCustomCors();
builder.AddApiVersioning();
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddValidators();
builder.ConfigurePostgres();
builder.ConfigureRedis();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.RunMigration();
app.ConfigureSwagger();
app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
