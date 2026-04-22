## FlagForge Backend

FlagForge is evolving into a stateless, multi-tenant feature flag API.

Current backend direction:

- .NET 8
- EF Core 8
- PostgreSQL as the source of truth
- PostgreSQL `jsonb` for flag rules
- Redis as the read cache for SDK evaluation
- Environment-scoped API keys
- Polling-based SDK sync using monotonic versions

---

## Architecture Notes

The control plane writes to PostgreSQL through EF Core. PostgreSQL remains the durable source of truth.

The data plane should serve read-heavy SDK traffic from Redis:

- SDK requests authenticate with an environment API key.
- API resolves the environment from the API key.
- API checks the cached version.
- SDK downloads a full flag snapshot only when the version changes.

The API should remain stateless. Redis entries can always be rebuilt from PostgreSQL.

---

## Data Model

### Tenants

Represents an account or organization.

| Column      | Type           | Notes                       |
| ----------- | -------------- | --------------------------- |
| `TenantId`  | `uuid`         | Primary key                 |
| `Name`      | `varchar(200)` | Unique in the current model |
| `Plan`      | `varchar(20)`  | `Tier1`, `Tier2`, `Tier3`   |
| `CreatedAt` | `timestamptz`  | UTC timestamp               |

### Environments

Represents an isolated runtime environment for a tenant.

| Column          | Type           | Notes                             |
| --------------- | -------------- | --------------------------------- |
| `EnvironmentId` | `uuid`         | Primary key                       |
| `TenantId`      | `uuid`         | FK to `Tenants`                   |
| `Name`          | `varchar(50)`  | Example: `dev`, `staging`, `prod` |
| `ApiKeyHash`    | `varchar(128)` | Unique SHA-256 hash of the API key |

Indexes:

- Unique `ApiKeyHash`
- Unique `{ TenantId, Name }`

The raw environment API key is returned only when an environment is created. Persisted data stores the SHA-256 hash, not the secret itself.

### FeatureFlags

Represents environment-scoped flag state.

| Column          | Type           | Notes                                |
| --------------- | -------------- | ------------------------------------ |
| `FlagId`        | `uuid`         | Primary key                          |
| `EnvironmentId` | `uuid`         | FK to `Environments`                 |
| `Name`          | `varchar(200)` | Flag name/key within the environment |
| `Enabled`       | `boolean`      | Global on/off switch                 |
| `Rules`         | `jsonb`        | Targeting rules                      |
| `Version`       | `bigint`       | Monotonic version for SDK sync       |
| `UpdatedAt`     | `timestamptz`  | UTC timestamp                        |

Indexes:

- Unique `{ EnvironmentId, Name }`
- `{ EnvironmentId, Version }`

### UsageAuditLogs

Stores evaluation audit events. This should move behind an async logging path before high-volume SDK traffic uses it.

| Column             | Type           | Notes                |
| ------------------ | -------------- | -------------------- |
| `LogId`            | `uuid`         | Primary key          |
| `TenantId`         | `uuid`         | FK to `Tenants`      |
| `EnvironmentId`    | `uuid`         | FK to `Environments` |
| `FlagName`         | `varchar(200)` | Evaluated flag       |
| `EvaluationResult` | `boolean`      | Returned value       |
| `Timestamp`        | `timestamptz`  | UTC timestamp        |

Indexes:

- `{ TenantId, Timestamp }`
- `{ EnvironmentId, Timestamp }`

---

## Redis Model

Redis is the read cache, not the source of truth.

Suggested keys:

```text
flags:{apiKey} -> JSON snapshot of all flags for one environment
version:{apiKey} -> integer monotonic environment version
rate:{apiKey}:{yyyyMMdd} -> daily request count
rate:{apiKey}:{yyyyMMddHH} -> hourly request count
```

Suggested TTLs:

```text
rate:{apiKey}:{yyyyMMdd} -> 24h
rate:{apiKey}:{yyyyMMddHH} -> 1h
```

Design decision: cache by API key because SDKs naturally authenticate with API keys. Internally, writes should still be modeled around `TenantId` and `EnvironmentId` so the database remains normalized.

---

## Local PostgreSQL Setup

If you run the API directly on your machine, `Host=localhost` is correct.

Start PostgreSQL with Docker:

```sh
docker run --name flagforge-postgres \
  -e POSTGRES_DB=flagforge \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  -d postgres:16
```

The default local connection string is in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnectionString": "Host=localhost;Port=5432;Database=flagforge;Username=postgres;Password=postgres"
  }
}
```

For non-local environments, use secrets or environment variables instead of committing credentials.

When running through `docker-compose.yml`, the API container must not use `localhost` for PostgreSQL. Inside the API container, `localhost` is the API container itself. Compose overrides the connection string with:

```yaml
ConnectionStrings__DefaultConnectionString: Host=postgres;Port=5432;Database=flagforge;Username=postgres;Password=postgres
```

`postgres` is the Docker Compose service name and resolves to the PostgreSQL container on the Compose network.

---

## Migrations

Apply the current PostgreSQL baseline:

```sh
dotnet ef database update
```

The previous SQL Server migrations were removed because they used SQL Server-specific column types and metadata. The current baseline migration is:

```text
Migrations/20260421120000_InitialPostgresFeatureFlagModel.cs
```

Generate future migrations from the backend project directory:

```sh
dotnet ef migrations add <MigrationName>
```

If the database container starts empty, apply migrations before calling the API:

```sh
dotnet ef database update
```

The current API does not automatically run migrations on startup.

---

## Run Locally

Restore and build:

```sh
dotnet restore
dotnet build
```

Run the API:

```sh
dotnet run
```

Swagger is available in development mode.

Run with Docker Compose:

```sh
docker compose up --build
```

Then call the API on:

```text
http://localhost:8080/api/feature-flags
```

---

## Incremental Next Steps

Recommended follow-up refactors:

1. Add dedicated tenant/environment management endpoints.
2. Hash API keys in PostgreSQL instead of storing raw API keys.
3. Add Redis snapshot publishing after feature flag writes.
4. Increment environment-level version on every flag change.
5. Add SDK sync endpoints: version check and snapshot download.
6. Move usage audit logging to a background queue/outbox path.
