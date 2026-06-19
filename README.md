# ICMarkets Assessment

A .NET 8 Web API. It reads blockchain data from the BlockCypher API for BTC, ETH, DASH and LTC,
saves each reading in a SQLite database with a timestamp, and shows the history and the latest
status through Swagger.

## What it does

- A background job calls BlockCypher every 60 seconds for all 5 chains.
- Every reading is saved with a `CreatedAt` time.
- The history endpoints return the data newest first.
- You can also trigger a manual refresh for one chain.

## Important Notes on design

This service has only a few endpoints. Some parts are bigger than it needs. I added them to show how I
would build a larger system. Here is what I think about each one.

- **CQRS with MediatR**: not really needed at this size, but it is cheap and keeps things consistent
  and easy to test. Fine to keep.
- **Validation pipeline**: useful. It runs all validators before the handler, so handlers stay clean.
- **Repository over EF Core**: mostly to show the pattern. EF's `DbSet` is already like a repository.
  The extra layer helps only if you want to hide EF or change the database later.
- **Unit of Work over EF Core**: `SaveChanges` is already a unit of work. The explicit transaction is
  useful here because two tables are written together (the current row and the event log).
- **Event store**: the main extra part. See below.
- **Background poller + rate limiter + Polly retry**: useful, because the BlockCypher free tier has limits.
- **Architecture tests**: cheap and helpful.

### Two tables: state and history

There are two tables:

- `blockchain`: one row per chain, updated each time. This is the current state, used by `/latest`.
- `events`: an append-only log. This is the history, used by `/history`.

For this size you could use one table with timestamps instead. Latest would be the newest row per chain,
and history would be all rows sorted by time. That is simpler. The two-table split is the part I added
to show the write model / read model idea and an event log. In a real service this small I would join
them into one table.

### Revision (concurrency)

`Revision` is a version number on each chain row. It starts at 1 and goes up by 1 on every update. It is an optimistic lock and also it show how many times a record is updated. `Revision` is an EF shadow property. It is set in `OnModelCreating`, not on the `BlockchainModel` class,
so the `domain entity stays clean`. The cost is that you read and write it through the change tracker with
a string name (`"Revision"`), which is easy to typo. A normal `int` property would be easier to use.
I choose the shadow property to `keep persistence out of the domain`.

### Rate limit and the two exceptions

There are two layers that protect the BlockCypher limit, and two exception types so you can tell them
apart in the logs.

- **Local limit**: a token bucket in front of the call. If the bucket is empty and the queue is full,
  the client throws `BlockCypherRateLimitException`. This means we stopped ourselves before calling.
- **Server limit**: if a call goes out and BlockCypher returns 429, the client throws
  `BlockCypherTooManyRequestException`. This means we passed our own limit and hit the real one. so we need to do something about it, extending subscription or change subscription to another plan with higher rate or limit the number of request. In general if it happen repeatly it show an issue that we need to decide, that is why it is diffrent than local limit

## Project structure

The code uses Clean Architecture. The rule is `Api -> Infrastructure -> Application -> Domain`.
The Domain project depends on nothing. The `ArchitectureTests` project checks this rule automatically.

```
ICMarkets.Domain          entities, the chain list, domain events, exceptions
ICMarkets.Application     commands and queries (MediatR), validators, validation pipeline, interfaces
ICMarkets.Infrastructure  EF Core + SQLite, repositories, unit of work, event store, BlockCypher client, poller
ICMarkets.Api             controllers, Program.cs, Swagger, CORS, health checks, error handler, logging
tests                     UnitTests, IntegrationTests, FunctionalTests, ArchitectureTests
```

Patterns used: CQRS, Repository, Unit of Work, and an Event Store. For a small service this is more
than it needs. I added them to show the patterns.

## Run with .NET

You need the .NET 8 SDK. SQLite is just a file, so there is no database server to install.

```bash
dotnet run --project ICMarkets.Api
```

The app creates and updates the database tables on startup, so you do not need to run migrations by hand.
Then open Swagger at the URL shown in the console, for example `http://localhost:5131/swagger`.

## Run with Docker

```bash
docker compose up --build
```

The API starts on `http://localhost:8080`. Swagger is at `http://localhost:8080/swagger`.
The database file is kept in a Docker volume, so the data stays after a restart.
The tables are created on startup, so no manual migration step is needed.

## Endpoints

| Method | Route                                    | What it does                                       |
|--------|------------------------------------------|----------------------------------------------------|
| GET    | `/api/blockchains`                       | list of supported chains                           |
| GET    | `/api/blockchains/latest`                | latest saved status for every chain                |
| GET    | `/api/blockchains/latest/{identifier}`   | latest status for one chain (404 if none)          |
| GET    | `/api/blockchains/history`               | history of all chains, newest first, paged         |
| GET    | `/api/blockchains/history/{identifier}`  | history for one chain, newest first, paged         |
| POST   | `/api/blockchains/refresh/{identifier}`  | get the current reading now and save it            |
| GET    | `/health`, `/health/ready`               | liveness, and readiness (readiness checks the DB)  |

Chain identifiers: `btc-main`, `btc-test3`, `eth-main`, `dash-main`, `ltc-main`.
History is sorted by time, newest first.

## Configuration

Settings come from `appsettings.json`.

| Section             | Key                             | Default                           |
|---------------------|---------------------------------|-----------------------------------|
| `ConnectionStrings` | `Default`                       | `Data Source=icmarkets.db`        |
| `BlockCypher`       | `BaseUrl`                       | `https://api.blockcypher.com/v1/` |
| `BlockCypher`       | `Token`                         | empty (optional)                  |
| `BlockCypher`       | `TimeoutSeconds` / `RetryCount` | `30` / `3`                        |
| `BlockCypher`       | `RateLimit*`                    | token bucket settings             |
| `Polling`           | `Enabled` / `IntervalSeconds`   | `true` / `60`                     |
| `Cors`              | `AllowedOrigins`                | empty means allow any origin      |

The BlockCypher.token is optional. Without it the API still works, but with a lower rate limit.

## Tests

```bash
dotnet test
```

There are four test projects. None of them need Docker or any outside service.

- **UnitTests** — domain, pagination, validators, the pull command handler, the validation pipeline,
  and the BlockCypher client (mapping, 429 handling, local rate limit).
- **IntegrationTests** — repositories, unit of work and the event store on a real SQLite file,
  including saving and loading the event payload.
- **FunctionalTests** — the HTTP endpoints from end to end with `WebApplicationFactory`, using a fake
  BlockCypher client and a temporary database. Also CORS and the health checks.
- **ArchitectureTests** — the layer dependency rules (NetArchTest).
