using System;
using FlagForge.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlagForge.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260421120000_InitialPostgresFeatureFlagModel")]
public partial class InitialPostgresFeatureFlagModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Plan = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenants", x => x.TenantId);
            });

        migrationBuilder.CreateTable(
            name: "Environments",
            columns: table => new
            {
                EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                ApiKeyHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Environments", x => x.EnvironmentId);
                table.ForeignKey(
                    name: "FK_Environments_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "TenantId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FeatureFlags",
            columns: table => new
            {
                FlagId = table.Column<Guid>(type: "uuid", nullable: false),
                EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Enabled = table.Column<bool>(type: "boolean", nullable: false),
                Rules = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                Version = table.Column<long>(type: "bigint", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FeatureFlags", x => x.FlagId);
                table.ForeignKey(
                    name: "FK_FeatureFlags_Environments_EnvironmentId",
                    column: x => x.EnvironmentId,
                    principalTable: "Environments",
                    principalColumn: "EnvironmentId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UsageAuditLogs",
            columns: table => new
            {
                LogId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                EnvironmentId = table.Column<Guid>(type: "uuid", nullable: false),
                FlagName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                EvaluationResult = table.Column<bool>(type: "boolean", nullable: false),
                Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UsageAuditLogs", x => x.LogId);
                table.ForeignKey(
                    name: "FK_UsageAuditLogs_Environments_EnvironmentId",
                    column: x => x.EnvironmentId,
                    principalTable: "Environments",
                    principalColumn: "EnvironmentId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UsageAuditLogs_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "TenantId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Environments_ApiKeyHash",
            table: "Environments",
            column: "ApiKeyHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Environments_TenantId_Name",
            table: "Environments",
            columns: new[] { "TenantId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_FeatureFlags_EnvironmentId_Name",
            table: "FeatureFlags",
            columns: new[] { "EnvironmentId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_FeatureFlags_EnvironmentId_Version",
            table: "FeatureFlags",
            columns: new[] { "EnvironmentId", "Version" });

        migrationBuilder.CreateIndex(
            name: "IX_Tenants_Name",
            table: "Tenants",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UsageAuditLogs_EnvironmentId_Timestamp",
            table: "UsageAuditLogs",
            columns: new[] { "EnvironmentId", "Timestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_UsageAuditLogs_TenantId_Timestamp",
            table: "UsageAuditLogs",
            columns: new[] { "TenantId", "Timestamp" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "FeatureFlags");
        migrationBuilder.DropTable(name: "UsageAuditLogs");
        migrationBuilder.DropTable(name: "Environments");
        migrationBuilder.DropTable(name: "Tenants");
    }
}
