# Sales API

A sales management API. It implements a complete CRUD for sales records, following Domain-Driven Design and Clean Architecture, with the quantity-based discount rule described in [Business Rules](#business-rules) below.

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Business Rules](#business-rules)
- [Caching](#caching)
- [Getting Started](#getting-started)
- [Authentication](#authentication)
- [API Reference](#api-reference)
- [Running Tests](#running-tests)
- [Secret Scanning](#secret-scanning)
- [Project Structure](#project-structure)

## Architecture

The solution follows Clean Architecture, with dependencies pointing inward toward the Domain:

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `SalesApi.Domain` | Entities, value objects, domain events and business rules. No dependency on any other layer. |
| Application | `SalesApi.Application` | Use cases (MediatR commands/queries/handlers), validation, AutoMapper profiles. |
| ORM | `SalesApi.ORM` | EF Core `DbContext`, entity configurations, migrations, repository implementations, and the MongoDB sale event store. |
| IoC | `SalesApi.IoC` | Dependency injection wiring between layers. |
| WebApi | `SalesApi.WebApi` | Controllers, request/response DTOs, middleware, Swagger, JWT configuration. |
| Common | `SalesApi.Common` | Cross-cutting concerns (security, logging, validation helpers) shared by every layer. |

Key design decisions:

- **DDD aggregate boundary**: `Sale` is the aggregate root; `SaleItem` has no controller, repository or standalone existence of its own - it's only ever created, changed or cancelled through `Sale` (`Sale.AddItem`, `Sale.ReplaceItems`, `Sale.CancelItem`, `Sale.Cancel`).
- **External Identities pattern**: `Sale` references `Customer`, `Branch` and `Product` (via `SaleItem`) as denormalized value objects (`ExternalCustomer`, `ExternalBranch`, `ExternalProduct`), snapshotting the referenced entity's description at the time of the sale instead of a live foreign key into another domain.
- **Domain events**: `SaleCreatedEvent`, `SaleModifiedEvent`, `SaleCancelledEvent` and `ItemCancelledEvent` are raised by the aggregate and dispatched via MediatR notifications when the `DbContext` saves changes. Each one is logged and appended to a `sale_events` collection in MongoDB (event type, sale id, item id when applicable, timestamp); they're not published to a message broker. The MongoDB write is best-effort: events are dispatched after the sale is committed in PostgreSQL, so a MongoDB outage logs a warning instead of failing a request whose sale was already saved.
- **Validation in the MediatR pipeline**: every command and query has a FluentValidation validator in the Application layer, and a `ValidationBehavior` runs it before the handler. Invalid input never reaches business logic, each rule lives in one place, and a failure becomes a `400 ValidationError` through the exception middleware.

## Tech Stack

- **.NET 8** / **C#**
- **PostgreSQL** with **EF Core** for persistence
- **MongoDB** for the sale events log
- **Redis** (through `IDistributedCache`) as a cache for sale reads
- **MediatR** for the command/query pipeline
- **AutoMapper** for object-to-object mapping
- **FluentValidation** for command/query validation, run in the MediatR pipeline
- **JWT Bearer authentication**
- **Docker Compose** for local orchestration
- **xUnit**, **NSubstitute** and **Bogus** for unit testing
- **Serilog** for structured logging
- **Swagger / Swashbuckle** for interactive API documentation

## Business Rules

The core business rule, quantity-based discounting per product line:

| Quantity | Discount |
|---|---|
| 1-3 items | No discount |
| 4-9 items | 10% |
| 10-20 items | 20% |
| Above 20 items | Rejected - a line item cannot sell more than 20 identical units |

The discount is computed by `SaleItem` itself from its `Quantity` - it is never accepted as input from the API client, since that would let a caller bypass the rule entirely.

## Caching

`GET /api/Sales/{id}` uses the **cache-aside** pattern with Redis:

1. The handler looks up `sale:{id}` in the cache. On a hit, it returns the cached result without touching PostgreSQL or AutoMapper.
2. On a miss, it reads the sale from PostgreSQL, maps it, and stores the result with a 10-minute TTL.
3. A sale that doesn't exist is not cached: a sale created right after would stay hidden until the TTL expired.

Key design decisions:

- **What is cached**: the `GetBySaleIdResult` read model, not the `Sale` aggregate. The aggregate has private constructors, protected collections and domain events, so it can't round-trip through JSON without bypassing its invariants; the read model is plain data, already in response shape.
- **Invalidation after the commit**: every command that changes a sale (`UpdateSale`, `CancelSale`, `CancelSaleItem`, `AddSaleItem`, `DeleteSale`) removes the key right after saving. Removing it *before* the commit would let a concurrent read re-cache the old value from the database. The short window between commit and removal is accepted eventual consistency.
- **TTL as a safety net**: since writes invalidate the key, the TTL only bounds how long a stale entry can survive if an invalidation is lost (for example, a network blip while removing the key).
- **The cache is optional**: `ResilientDistributedCache` decorates the Redis `IDistributedCache`. If Redis is down, reads degrade to a miss (and fall back to PostgreSQL), writes to the cache are skipped, and a failed invalidation is logged as an error. Handlers don't know about Redis failures, and a Redis outage never fails a request. Connection timeouts are set to 1 second so an outage slows requests down instead of holding them for the default 5 seconds; a circuit breaker would be the next step to skip Redis entirely while it's down.
- The list endpoint (`GET /api/Sales`, with filters and paging) is deliberately not cached: invalidating every page and filter combination a sale could appear in is far more complex than the gain.

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/) and Docker Compose
- .NET 8 SDK (only needed if you want to build/test outside a container)

### Running with Docker

Secrets (database passwords and the JWT signing key) are never committed. They come from a `.env` file at the repository root, which git ignores. Create it from the template and fill every value:

```bash
cp .env.example .env
```

`.env.example` shows how to generate random values. Use letters and digits only (the Mongo password goes inside a URL), and at least 32 characters for `JWT_SECRET_KEY`: the API refuses to start with a shorter key. `docker compose` stops with a clear message if any variable is missing.

The API container serves HTTPS with the ASP.NET Core development certificate, mounted from your machine. Visual Studio exports it automatically; from the command line, export it once before the first run (pick any password):

```bash
dotnet dev-certs https --trust
dotnet dev-certs https -ep "$APPDATA/ASP.NET/Https/SalesApi.WebApi.pfx" -p <password>
dotnet user-secrets set "Kestrel:Certificates:Development:Password" "<password>" --project src/SalesApi.WebApi
```

On Linux or macOS, use `~/.aspnet/https` instead of `$APPDATA/ASP.NET/Https`, and point the two certificate volumes in `docker-compose.yml` to `~/.microsoft/usersecrets` and `~/.aspnet/https`.

From the repository root:

```bash
docker compose up -d --build
```

This starts the API, PostgreSQL, MongoDB and Redis (all three with health checks gating the API's startup). **Database migrations are applied automatically** on application startup - no manual `dotnet ef database update` step is needed.

Stop everything with `docker compose down -v`: PostgreSQL and MongoDB keep no data between runs, and `-v` removes their anonymous volumes so they don't pile up in Docker.

The API container publishes ports 8080 (HTTP) and 8081 (HTTPS) to random host ports. Find the mapped port with:

```bash
docker compose port salesapi.webapi 8081
```

Then open `https://localhost:<mapped-port>/swagger` for the interactive API documentation.

To see the sale events stored in MongoDB (also reachable from MongoDB Compass at `mongodb://developer:<MONGO_PASSWORD>@localhost:27017`, with the value from `.env`):

```bash
docker exec sales_api_nosql sh -c 'mongosh -u developer -p "$MONGO_INITDB_ROOT_PASSWORD" --authenticationDatabase admin --quiet --eval "db.getSiblingDB(\"sales_api\").sale_events.find().toArray()"'
```

To watch the cache at work, leave Redis's monitor running in a second terminal and call `GET /api/Sales/{id}` twice (a miss, then a hit), then change the sale (an invalidation):

```bash
docker exec -it sales_api_cache sh -c 'redis-cli -a "$REDIS_PASSWORD" --no-auth-warning MONITOR'
```

### Running locally (without Docker)

You'll need local PostgreSQL, MongoDB and Redis instances (the `docker compose` ones work: `docker compose up -d salesapi.database salesapi.nosql salesapi.cache`). Configuration comes from environment variables, never from `appsettings*.json`. In PowerShell, load `.env` into the session and point the connection strings at `localhost`:

```powershell
Get-Content .env | Where-Object { $_ -match '^\w+=' } | ForEach-Object {
    $name, $value = $_ -split '=', 2
    Set-Item "env:$name" $value
}
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=sales_api;Username=developer;Password=$env:POSTGRES_PASSWORD"
$env:ConnectionStrings__MongoDb = "mongodb://developer:$env:MONGO_PASSWORD@localhost:27017/?serverSelectionTimeoutMS=3000"
$env:ConnectionStrings__Redis = "localhost:6379,password=$env:REDIS_PASSWORD,abortConnect=false"
$env:Jwt__SecretKey = $env:JWT_SECRET_KEY
dotnet run --project src/SalesApi.WebApi
```

## Authentication

Every endpoint requires a JWT bearer token, **except**:

- `POST /api/Auth` (login)
- `POST /api/Users` (user creation) - there is no seed data in the database, so this is the only way to create the first account

Flow to try the API from a fresh database:

1. `POST /api/Users` - create an account.
2. `POST /api/Auth` with the same credentials - returns a JWT `token`.
3. Send that token as `Authorization: Bearer <token>` on every other request, or paste it into Swagger's **Authorize** button.

## API Reference

### Auth

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/Auth` | Anonymous | Authenticates a user and returns a JWT token |

### Users

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/Users` | Anonymous | Creates a user (see note above) |
| GET | `/api/Users/{id}` | Required | Retrieves a user by id |
| GET | `/api/Users` | Required | Lists users with pagination and ordering (`_page`/`_size`/`_order`, same convention as Sales) |
| PUT | `/api/Users` | Required | Updates a user |
| DELETE | `/api/Users/{id}` | Required | Deletes a user |

### Sales

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/Sales` | Required | Creates a sale with its items |
| GET | `/api/Sales/{id}` | Required | Retrieves the full detail of a sale, including its items |
| GET | `/api/Sales` | Required | Lists sales with pagination, ordering and filtering (see below) |
| PUT | `/api/Sales/{id}` | Required | Replaces a sale's item list (corrects a sale after creation) |
| DELETE | `/api/Sales/{id}` | Required | Deletes a sale |
| POST | `/api/Sales/{id}/cancel` | Required | Cancels a sale |
| POST | `/api/Sales/{id}/items` | Required | Adds one item to an existing sale |
| DELETE | `/api/Sales/{id}/items/{itemId}` | Required | Cancels one item of a sale |

`GET /api/Sales` supports:
- `_page` / `_size` - pagination (defaults: 1 / 10, `_size` up to 100)
- `_order` - e.g. `_order=totalAmount desc, saleNumber`
- `customerName` - case-insensitive exact or wildcard (`value*`, `*value`, `*value*`) filter
- `status` - `Active` or `Cancelled`
- `_minTotalAmount` / `_maxTotalAmount`, `_minDate` / `_maxDate` - range filters

Filtering, ordering and pagination run in the database for both Sales and Users; only the requested page is loaded.

Successful responses share a common envelope:

```json
{
  "success": true,
  "message": "Sale retrieved successfully",
  "data": { }
}
```

Every non-2xx response - validation failures, missing resources, authentication/authorization errors, conflicts, or anything unexpected - follows a single error format instead:

```json
{
  "type": "ValidationError",
  "error": "Invalid input data",
  "detail": "Customer name is required."
}
```

`type` is a machine-readable identifier (`ValidationError`, `ResourceNotFound`, `AuthenticationError`, `AuthorizationError`, `Conflict`, `ServerError`), `error` a short human-readable summary, and `detail` the specifics of that occurrence. HTTP status codes: `400` for validation errors, `401` for missing/invalid authentication, `403` for an authenticated user without permission, `404` for a missing resource, `409` for a state conflict (e.g. cancelling an already-cancelled sale), `500` for anything unexpected.

## Running Tests

The solution has three test tiers:

```bash
# Unit - no external dependencies, runs anywhere
dotnet test tests/SalesApi.Unit/SalesApi.Unit.csproj

# Integration and Functional - need Docker running (Testcontainers spins up real,
# throwaway Postgres and MongoDB instances per run; nothing to configure manually)
dotnet test tests/SalesApi.Integration/SalesApi.Integration.csproj
dotnet test tests/SalesApi.Functional/SalesApi.Functional.csproj

# Or all three at once
dotnet test SalesApi.sln
```

- **Unit** covers the domain's business rules (discount tiers, aggregate invariants, domain events), the Application layer's handlers (success paths, not-found, and business-specific edge cases), the command/query validators and the `ValidationBehavior` that runs them, and an AutoMapper configuration test that builds the real mapper (rather than a mock) to catch broken or incomplete profiles.
- **Integration** exercises `SaleRepository` against a real Postgres instance - specifically the EF Core Added-vs-Modified ambiguity around `SaleItem`'s client-generated key, which a mocked repository can never catch since it only ever manifested against a real database.
- **Functional** boots the actual WebApi host (`WebApplicationFactory`) behind real Postgres and MongoDB instances and drives it through plain HTTP: the discount tiers, the quantity ceiling, the full cancel/update/add-item flows, list filtering/ordering/pagination, user updates, sale events landing in MongoDB, and the `{type, error, detail}` error shape for 400/401/404/409 - the same scenarios verified manually via curl during development, now automated.

### Coverage report

From the repository root, on Windows:

```bash
coverage-report.bat
```

It installs the `coverlet.console` and `reportgenerator` global tools if missing, runs all three tiers with coverage (so Docker must be running) and writes an HTML report to `TestResults/CoverageReport/index.html`. On Linux/macOS, the equivalent is:

```bash
dotnet test SalesApi.sln --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory ./TestResults
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:Html
```

## Secret Scanning

Every commit is checked for secrets by [gitleaks](https://github.com/gitleaks/gitleaks), through a versioned pre-commit hook. Besides gitleaks' default rules, [`.gitleaks.toml`](.gitleaks.toml) flags literal passwords in config files (`docker-compose*.yml`, `appsettings*.json`, `.env`) and credentials embedded in connection URLs. Enable it once per clone:

```bash
git config core.hooksPath .githooks
```

A commit with a secret is refused, showing the file and line. To scan the whole history: `gitleaks git --log-opts="--all"`.

## Project Structure

```
sales-api-dotnet/
├── src/
│   ├── SalesApi.Domain/
│   ├── SalesApi.Application/
│   ├── SalesApi.ORM/
│   ├── SalesApi.IoC/
│   ├── SalesApi.WebApi/
│   └── SalesApi.Common/
├── tests/
│   ├── SalesApi.Unit/
│   ├── SalesApi.Integration/
│   └── SalesApi.Functional/
├── docker-compose.yml
└── SalesApi.sln
```
