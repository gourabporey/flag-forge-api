using FlagForge.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlagForge.Data.Configurations;

public class UsageAuditLogConfiguration : IEntityTypeConfiguration<UsageAuditLog>
{
    public void Configure(EntityTypeBuilder<UsageAuditLog> builder)
    {
        builder.ToTable("UsageAuditLogs");

        builder.HasKey(x => x.LogId);

        builder.Property(x => x.FlagName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Timestamp });
        builder.HasIndex(x => new { x.EnvironmentId, x.Timestamp });

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Environment)
            .WithMany()
            .HasForeignKey(x => x.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
