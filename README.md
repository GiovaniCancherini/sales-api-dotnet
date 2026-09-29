# Sales API

A sales management API. It implements a complete CRUD for sales records, following Domain-Driven Design and Clean Architecture, with the quantity-based discount rule described in [Business Rules](#business-rules) below.

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Business Rules](#business-rules)
- [Getting Started](#getting-started)
- [Authentication](#authentication)
- [API Reference](#api-reference)
- [Running Tests](#running-tests)
- [Project Structure](#project-structure)

## Architecture

The solution follows Clean Architecture, with dependencies pointing inward toward the Domain:

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `SalesApi.Domain` | Entities, value objects, domain events and business rules. No dependency on any other layer. |
| Application | `SalesApi.Application` | Use cases (MediatR commands/queries/handlers), validation, AutoMapper profiles. |
| ORM | `SalesApi.ORM` | EF Core `DbContext`, entity configurations, migrations, repository implementations. |
| IoC | `SalesApi.IoC` | Dependency injection wiring between layers. |
| WebApi | `SalesApi.WebApi` | Controllers, request/response DTOs, middleware, Swagger, JWT configuration. |
| Common | `SalesApi.Common` | Cross-cutting concerns (security, logging, validation helpers) shared by every layer. |

Key design decisions:

- **DDD aggregate boundary**: `Sale` is the aggregate root; `SaleItem` has no controller, repository or standalone existence of its own - it's only ever created, changed or cancelled through `Sale` (`Sale.AddItem`, `Sale.ReplaceItems`, `Sale.CancelItem`, `Sale.Cancel`).
- **External Identities pattern**: `Sale` references `Customer`, `Branch` and `Product` (via `SaleItem`) as denormalized value objects (`ExternalCustomer`, `ExternalBranch`, `ExternalProduct`), snapshotting the referenced entity's description at the time of the sale instead of a live foreign key into another domain.
- **Domain events**: `SaleCreatedEvent`, `SaleModifiedEvent`, `SaleCancelledEvent` and `ItemCancelledEvent` are raised by the aggregate and dispatched via MediatR notifications when the `DbContext` saves changes. They're logged, not published to a message broker.

## Tech Stack

- **.NET 8** / **C#**
- **PostgreSQL** with **EF Core** for persistence
- **MediatR** for the command/query pipeline
- **AutoMapper** for object-to-object mapping
- **FluentValidation** for request/command validation
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

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/) and Docker Compose
- .NET 8 SDK (only needed if you want to build/test outside a container)

### Running with Docker

From the repository root:

```bash
docker compose up -d --build
```

This starts the API, a PostgreSQL container (with a health check gating the API's startup) and a couple of infrastructure containers (MongoDB, Redis) that ship with the template but aren't used by this implementation. **Database migrations are applied automatically** on application startup - no manual `dotnet ef database update` step is needed.

The API container publishes ports 8080 (HTTP) and 8081 (HTTPS) to random host ports. Find the mapped port with:

```bash
docker compose port salesapi.webapi 8081
```

Then open `https://localhost:<mapped-port>/swagger` for the interactive API documentation.

### Running locally (without Docker)

You'll need a local PostgreSQL instance and a connection string in `src/SalesApi.WebApi/appsettings.Development.json`, then:

```bash
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
- `_page` / `_size` - pagination (defaults: 1 / 10)
- `_order` - e.g. `_order=totalAmount desc, saleNumber`
- `customerName`, `status` - exact or wildcard (`*value*`) filters
- `_minTotalAmount` / `_maxTotalAmount`, `_minDate` / `_maxDate` - range filters

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

# Integration and Functional - need Docker running (Testcontainers spins up a
# real, throwaway Postgres per run; nothing to configure manually)
dotnet test tests/SalesApi.Integration/SalesApi.Integration.csproj
dotnet test tests/SalesApi.Functional/SalesApi.Functional.csproj

# Or all three at once
dotnet test SalesApi.sln
```

- **Unit** covers the domain's business rules (discount tiers, aggregate invariants, domain events), the Application layer's handlers (success paths, not-found, validation failures, and business-specific edge cases), and an AutoMapper configuration test that builds the real mapper (rather than a mock) to catch broken or incomplete profiles.
- **Integration** exercises `SaleRepository` against a real Postgres instance - specifically the EF Core Added-vs-Modified ambiguity around `SaleItem`'s client-generated key, which a mocked repository can never catch since it only ever manifested against a real database.
- **Functional** boots the actual WebApi host (`WebApplicationFactory`) behind a real Postgres instance and drives it through plain HTTP: the discount tiers, the quantity ceiling, the full cancel/update/add-item flows, and the `{type, error, detail}` error shape for 400/401/404/409 - the same scenarios verified manually via curl during development, now automated.

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
