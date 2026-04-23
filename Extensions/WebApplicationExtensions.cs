using FlagForge.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FlagForge.Extensions;

public static class WebApplicationExtensions
{
    public static void RunMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Log.Error("Database migration failed");
            throw;
        }
    }

    public static void ConfigureSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlagForge v1"));
    }
}
