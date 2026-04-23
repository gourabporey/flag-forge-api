using FlagForge.Data.Services;
using FlagForge.Extensions;
using FlagForge.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.Services.AddTransient<FeatureFlagService>();
builder.Services.AddTransient<TenantService>();
builder.Services.AddTransient<EnvironmentService>();
builder.Services.AddTransient<AuthService>();
builder.AddJwtValidation();
builder.AddCustomCors();
builder.AddApiVersioning();
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddValidators();
builder.ConfigurePostgres();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlagForge v1"));
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapControllers();

app.Run();
