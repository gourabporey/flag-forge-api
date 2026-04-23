using System.Text;
using Asp.Versioning;
using FlagForge.Auth;
using FlagForge.Data;
using FlagForge.Data.Services;
using FlagForge.Data.Validations;
using FlagForge.Data.ViewModels;
using FlagForge.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<FeatureFlagService>();
builder.Services.AddTransient<TenantService>();
builder.Services.AddTransient<EnvironmentService>();
builder.Services.AddTransient<AuthService>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "LocalDevelopment",
        policy =>
        {
            policy
                .WithOrigins()
                .WithOrigins("http://localhost:3000", "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

var jwtOptions =
    builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing configuration: Jwt");
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes.");
}

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();
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
app.UseMiddleware<ApiKeyMiddleware>();
app.UseCors("LocalDevelopment");
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapControllers();

app.Run();
