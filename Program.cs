using Asp.Versioning;
using FlagForge.Data;
using FlagForge.Data.Services;
using FlagForge.Data.Validations;
using FlagForge.Data.ViewModels;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<FeatureFlagService>();
builder.Services.AddTransient<TenantService>();
builder.Services.AddTransient<EnvironmentService>();

const int apiVersion = 1;
builder
    .Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(apiVersion, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(setup =>
    {
        setup.AddApiVersionParametersWhenVersionNeutral = true;
        setup.GroupNameFormat = "'v'VVV";
        setup.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<
    IValidator<CreateEnvironmentRequest>,
    CreateEnvironmentRequestValidator
>();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnectionString")
    ?? throw new InvalidOperationException("Missing configuration : DefaultConnectionString");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlagForge v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapControllers();

app.Run();
