# PebbleJar
Author: Zoya Ivanova https://github.com/izoya

A humble personal-finance API built with C#. It connects to [Akahu](https://www.akahu.nz/) 
and stores account and transaction history locally, with filtering and reports coming soon.

I built it for fun and learning C#, but also out of mild frustration with the online banking apps' features (or lack thereof).

This is a single-user project for now. There is no auth or UI yet. Use the HTTP request collection 
or any API client to call the API. 

## Run it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

From the repository root:

```sh
dotnet tool restore
dotnet ef database update --project src/PebbleJar.Infrastructure --startup-project src/PebbleJar.Api
dotnet run --project src/PebbleJar.Api
```

The app prints its local URL when it starts. A quick health check is available at `/health`. 
In development mode, the OpenAPI document is at `/openapi/v1.json`. 

Requests collection: `src/PebbleJar.Api/PebbleJar.Api.http`

Tests:
```sh
dotnet test PebbleJar.slnx --no-restore
```

### Akahu tokens

Create a personal app at [my.akahu.nz](https://my.akahu.nz/) and follow 
Akahu's [getting-started guide](https://developers.akahu.nz/docs/getting-started) to get the tokens. 
Keep them out of source control.

Set them as user secrets:

```sh
dotnet user-secrets -p src/PebbleJar.Api set "Akahu:AppIdToken" "your-app-id-token"
dotnet user-secrets -p src/PebbleJar.Api set "Akahu:UserAccessToken" "your-user-access-token"
```

`example.env` shows the environment-variable equivalents. 
`Akahu:BaseUrl` already defaults to the Akahu API.

## What it can do

- Pull accounts from Akahu and keep local copies up to date.
- Pull transactions for one account or every enabled account.
- Idempotent transactions sync.
- Search stored transactions with limited filters (for now).

## Some landmarks

This is intentionally a small app, but it has a few choices where I had more fun than I probably should have:

- **A local-first data model:** SQLite stores the useful local model, provider IDs, and the original provider payload for later debugging or remapping.
- **Careful sync behaviour:** Initial sync is capped at 90 days. 
  A later sync without specified date range overlaps by seven days from the latest stored transaction. 
  This makes missed or late-arriving transactions less likely without fetching everything every time.
- **Safe secret handling:** Credentials are wrapped in `SecretString`, which redacts them in logs, debug views, and JSON. 
- **Explicit API behaviour:** Minimal API endpoints use typed results, request validation, cancellation tokens, 
  JSON in `snake_case`, and sensible request timeouts.
- **Explicit mapping outcomes:** A deliberately overengineered mapping result type: `Application.Results.MappingResult`. 
  This was my playground for modelling a Rust-style data-carrying enum in C#: complete, partial, and failed mapping outcomes, 
  each with its own data.
- **External APIs' quirks:** Unexpected fields are reported rather than silently ignored. 
- **Database discipline:** Audit timestamps are added consistently to auditable entities.

## Project map

| Folder | What lives there |
| --- | --- |
| `src/PebbleJar.Api` | HTTP endpoints and app setup |
| `src/PebbleJar.Application` | Queries, results, and repository contracts |
| `src/PebbleJar.Domain` | Core finance types and rules |
| `src/PebbleJar.Infrastructure` | SQLite, EF Core, and the Akahu client |
| `src/PebbleJar.Extensions` | Small reusable helpers, including secret redaction |
| `tests` | Focused unit tests |

## Intended scope limits

- One local user only.
- Due to Sqlite limited timezone support, date filters and grouping 
use the device's local timezone, assuming the bundled API and UI run on the same device. 
- No UI yet.
- Akahu is the only connection provider.
- This is an actively learning project, not a finished finance product.

## Non-exhaustive non-functional TODO list

- E2e exceptions handling. Use Result type for business logic exceptions (`ErrorOr`, `OneOf`..?).
- Run transactions sync in background jobs, with status polling.
- Integration tests.
- Retries for temporary provider failures and rate limits.
- CI workflow.
