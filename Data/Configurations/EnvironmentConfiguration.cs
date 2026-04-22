using FlagForge.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlagForge.Data.Configurations;

public class EnvironmentConfiguration : IEntityTypeConfiguration<FeatureFlagEnvironment>
{
    public void Configure(EntityTypeBuilder<FeatureFlagEnvironment> builder)
    {
        builder.ToTable("Environments");

        builder.HasKey(x => x.EnvironmentId);

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ApiKeyHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.ApiKeyHash)
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Environments)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
