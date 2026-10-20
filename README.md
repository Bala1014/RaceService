# RaceService

The racing domain of RacingVacing: championships, calendars, circuits, entries, session results
and championship standings. One independently deployable service with its own PostgreSQL schema.

Identity and the forum are **not** here. They belong to their own services; this one only
validates the JWTs the UserService issues and stores a loose `UserId` reference where an entry
was submitted by a person.

## The domain model in one paragraph

A **Series** (Formula 1, WEC) runs a **Season** each year. A season has a calendar of
**RaceEvents** — race weekends — each held on a **TrackLayout**, which is one configuration of a
**Track**. A weekend contains **Sessions**: practice, qualifying, sprint, race. A season's entry
list is a set of **Entries**, one per car; a car contesting a particular weekend is an
**EventEntry**, and its crew for that weekend is a set of **EventEntryDrivers**. Results attach
to a session and a car (**SessionResult**), with each driver's share of the car's laps and points
in **SessionResultDriver**. **DriverStanding** and **TeamStanding** are projections rebuilt from
those results.

```
Series ─┬─ Season ─┬─ RaceEvent ─┬─ Session ─── SessionResult ─── SessionResultDriver
        │          │             │                   │
        │          │             │                   ├─ SessionLap
        │          │             │                   ├─ PitStop
        │          │             │                   └─ Penalty
        │          │             └─ EventEntry ─── EventEntryDriver ─── Driver
        │          ├─ Entry ─── Team
        │          ├─ PointsScheme ─── PointsSchemeEntry
        │          └─ DriverStanding / TeamStanding
        └─ VehicleClass

Track ─── TrackLayout ─┬─ RaceEvent
                       └─ LapRecord
```

### Why it is shaped this way

- **A weekend is not a race.** A modern Formula 1 weekend has six or more scheduled sessions with
  their own start times, statuses and results. A single start time on a "race" row cannot express
  that, and sprint weekends break it entirely.
- **Each running of an event is its own row.** Grouping historic results under a recurring "race"
  row means a past year cannot reference that year's entry list, grid or circuit layout.
- **A result belongs to a car in a session** — one pair of keys, so a result cannot name a driver
  who was not in the car. Who drove and what they earned lives in `SessionResultDriver`, which is
  what makes endurance racing and its minimum-drive-time rules expressible.
- **Classes are first-class.** Without `VehicleClass` a 24-hour race has no meaningful winner
  beyond the outright car.
- **Career totals and standings are projections**, stamped with when they were last rebuilt, not
  hand-maintained counters that quietly drift from the results that justify them.

## Running it

```bash
docker compose up --build          # Postgres + the service on http://localhost:5001
```

Or against your own Postgres:

```bash
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=raceservice;Username=postgres;Password=..."
dotnet run --project RaceService.Api
```

Migrations are applied at start-up and, outside Production, the database is seeded from
`RaceService.Application/Database/SeedData/seed.json` when it is empty. Both are switchable via
`Database:MigrateOnStartup` and `Database:SeedOnStartup`.

API docs are at `/scalar/v1` in Development; the OpenAPI document is at `/openapi/v1.json`.

```bash
dotnet build
dotnet test
```

### Migrations

```bash
dotnet ef migrations add <Name> --project RaceService.Application --startup-project RaceService.Application
dotnet ef database update      --project RaceService.Application --startup-project RaceService.Application
```

## API surface

Everything is under `/api/v1`. Reads are anonymous; writes need a JWT with the `Editor` or
`Administrator` role.

| Area | Endpoints |
| --- | --- |
| Series | `GET /series`, `GET /series/{id}`, `GET /series/slug/{slug}`, `GET /series/{slug}/seasons/{year}`, `POST`/`PUT`/`DELETE` |
| Seasons | `GET /seasons/{id}`, `GET /seasons/{id}/entries`, `GET /seasons/{id}/standings/drivers`, `GET /seasons/{id}/standings/teams`, `POST /seasons/{id}/standings/recalculate` |
| Races | `GET /races`, `GET /races/upcoming`, `GET /races/calendar`, `GET /races/{id}`, `GET /races/{seriesSlug}/{year}/{eventSlug}`, `GET /races/{id}/sessions`, `GET /races/{id}/entries`, `POST`/`PUT`/`PATCH .../status`/`DELETE` |
| Sessions | `GET /sessions/live`, `GET /sessions/{id}`, `GET /sessions/{id}/results`, `.../laps`, `.../pit-stops`, `.../penalties`, `PUT .../results`, `PUT .../laps`, `POST .../pit-stops`, `POST .../penalties`, `PATCH .../status` |
| Tracks | `GET /tracks`, `GET /tracks/{id}`, `GET /tracks/slug/{slug}`, `POST`, `PUT`, `POST /tracks/{id}/layouts`, `DELETE` |
| Drivers | `GET /drivers`, `GET /drivers/{id}`, `GET /drivers/slug/{slug}`, `GET /drivers/{id}/results`, `POST`, `PUT`, `POST /drivers/{id}/career-stats/recalculate`, `DELETE` |
| Teams | `GET /teams`, `GET /teams/{id}`, `GET /teams/slug/{slug}`, `POST`, `PUT`, `DELETE` |
| Entries | `PUT /entries/{id}`, `PATCH /entries/{id}/status`, `PUT /entries/event-entries/{id}/lineup`, `DELETE /entries/event-entries/{id}` |
| Ops | `GET /health/live`, `GET /health/ready` |

A session's classification is replaced as a whole set (`PUT .../results`), never appended row by
row: positions must be contiguous and unique, exactly one car holds the fastest lap, and points
depend on every other row, so the table is only ever valid as a unit.

## Conventions worth knowing

- **Time is UTC everywhere.** Read endpoints accept `?timeZone=<IANA>` and return each instant
  three ways: UTC, at the circuit, and in the caller's zone. No local timestamp is ever stored.
- **Durations are integer milliseconds.** They sort, compare and aggregate with plain arithmetic
  and survive JSON round-trips; formatted strings (`1:14.231`, `+7.614`) come back alongside them
  so every client renders the same thing.
- **Tables and columns are snake_case**, so the database is pleasant to query by hand.
- **Enums are stored as text**, so `select status, count(*) from race.session group by 1` is
  answerable without the C# source, and reordering an enum member cannot corrupt data.
- **Keys are UUID v7** — time-ordered, so inserts stay at the right-hand edge of the index
  instead of scattering across it.
- **Historic records are archived, never deleted.** Soft-deletable entities are filtered out of
  every query automatically, and the service refuses to remove a driver, team, track or session
  that has results behind it.
- **Errors are RFC 9457 problem documents** carrying a stable `code` (`Entry.DuplicateCarNumber`)
  so clients branch on the code, not on English prose.

## Configuration

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings__Postgres` | Full connection string. Falls back to `POSTGRES_HOST`/`PORT`/`DB`/`USER`/`PASSWORD`. |
| `Database__MigrateOnStartup` | Apply pending migrations at boot. Default true. |
| `Database__SeedOnStartup` | Seed reference data when empty. Default true outside Production. |
| `Jwt__Authority` | UserService OpenID authority. Set this in production; keys rotate automatically. |
| `Jwt__Issuer` / `Jwt__Audience` | Token validation. |
| `Jwt__SigningKey` | Symmetric key. **Local development only** — use `Jwt__Authority` in production. |
| `Cors__AllowedOrigins__0..n` | Browser origins allowed to call the API. |

See `RaceService.Api/.env.example`.

> Migrations run on start-up, which suits a single instance. When this service scales to more
> than one, move migration to a job that runs once before the rollout: concurrent instances
> otherwise race for the migration lock.
