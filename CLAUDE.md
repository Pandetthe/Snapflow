# Snapflow

Kanban board for teams. Monorepo: ASP.NET Core 10 API (`server/`), SvelteKit web client (`web/`), Aspire AppHost (`deployment/AppHost`). Boards update live over SignalR.

## Commands

**Always use the project script for verification, not raw `dotnet`/`npm` commands.** It prints clean ✓/✗ output and the full log only on failure (`VERBOSE=1` for everything).

```bash
scripts/check.sh          # server: restore + build + format check + tests, web: lint + check + unit tests
scripts/check.sh server   # only the server
scripts/check.sh web      # only the web client
```

- Run the app: `dotnet run --project deployment/AppHost/AppHost.csproj`, then open the gateway URL (web at `/`, API under `/api`).
- Integration tests use Testcontainers; without a Docker/Podman socket they are silently skipped.
- Migrations (from `server/`): `dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Presentation --output-dir Persistence/Migrations`.
- Web formatting: `npm run format --prefix web` (the pre-commit hook only formats C#).

## Server architecture

Clean Architecture with vertical slices and CQRS, **no mediator library**. Layers: Common <- Domain <- Application <- Infrastructure <- Presentation. Application uses EF Core directly through `IAppDbContext` (no repositories).

- Slice: `Application/<Slice>/<Action>/{XCommand,XHandler,XValidator,XResponse}.cs`. Scaffold with the `dotnet new` templates in `server/templates/`.
- ArchitectureTests enforce: sealed command/query records; `internal` handlers named `*Handler` in the command's namespace; a validator for every command except those listed in `CommandsWithoutUserInput` (`ValidationTests.cs`); slices must not reference each other (a new slice goes into `VerticalSliceTests.cs`).
- Handlers return `Result`/`Result<T>` with errors from `Domain/<Agg>/<Agg>Errors.cs` and never check permissions. Multi-step writes use `dbContext.InTransactionAsync`; `IEntityRankService.GenerateRankAsync` (LexoRank) only works inside a transaction.
- Rich domain: private setters, `static Create`, behaviour methods taking `actorId`, `at`, `connectionId`. Limits live in `<Agg>Options.cs`. Soft delete via `ISoftDeletable`, cascades via `SoftDeleteCascadeExtensions`.
- Endpoints: `internal sealed class X : IEndpoint` in `Presentation/Endpoints/<Area>`, auto-discovered. Routes have no `/api` prefix. Authenticated by default (`.AllowAnonymous()` to opt out). Board routes must name the parameter `boardId` and use `.RequireAuthorization(BoardPermissions.*)`; new permissions go in `BoardPermissions.cs` + `MemberRolePermissions.cs`.
- A board change clients must see needs: a domain event, a handler in `Hubs/Board/ClientEventHandlers`, a method on `IBoardHubClient`, and an entry in `Hubs/Board/CacheInvalidationHandlers` (else cached GETs go stale). Hub methods in `Hubs/Board/*.cs` mirror the endpoints.

## Web architecture

- Svelte 5 runes only (no `export let`, no `svelte/store`); rune code outside components lives in `*.svelte.ts`. Tailwind v4, theme tokens in `src/app.css`. UI kit in `src/lib/ui` (import from `$lib/ui/components`).
- Feature code in `src/lib/features/<feature>/{api,types,components,state,stores}`. API types are hand-written, no generated client.
- Services extend `BaseService` and return `Result<T>`, never throw. Use `$lib/server/api.server.ts` in `+page.server.ts`/hooks (pass `event`), `$lib/core/api.client.ts` in the browser. Env via `$env/dynamic/*`.
- Board mutations go through the SignalR hub (`features/boards/hub`), not REST; a broadcast may arrive before the hub response, so dedupe by id.
- New external origins must be added to the CSP in `hooks.server.ts`.

## Key constraints

- `TreatWarningsAsErrors` with `AnalysisMode=All` and code style enforced in build: every warning breaks the build.
- Package versions go in `server/Directory.Packages.props`, never in a csproj.
- **Always use LINQ method syntax**, never query syntax.
- **Never validate that an ID is `> 0`**: let the handler return `NotFound`.
- One `.editorconfig` at the repo root covers formatting and style (LF line endings). Analyzer severities live in `server/.globalconfig`, because the server Docker build only sees `server/`.

## Git

- Pre-commit hook runs `dotnet format` on staged `.cs` files. Enable once: `git config core.hooksPath .git-hooks`.
- Conventional Commits, lowercase imperative subject, scopes like `web`, `server`, `api`, `auth`, `ci`. Release notes are built from them.
- CalVer (`YYYY.M.patch`), bumped only by the `publish` workflow; never edit versions by hand.

## Workflow

`.claude/commands/` has `/research_codebase`, `/create_plan`, `/iterate_plan`, `/implement_plan`, `/validate_plan`, `/debug` and `/create_handoff`. Plans, research and handoffs are written to `.claude/{plans,research,handoffs}/` (gitignored).
