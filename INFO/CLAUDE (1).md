# Workspace context

This workspace holds the UBB SE 2026 merge assignment plus reference copies
of the other teams' repos.

## Folders

- `UBB-SE-2026-921-1/` — **the merged repo. All new code goes here.** Four
  projects: `PussyCats.App`, `PussyCats.Library`, `PussyCats.Api`,
  `PussyCats.Tests`. Docs duplicated in-tree under `UBB-SE-2026-921-1/docs/`.
- `UBB-SE-2026-PussyCats/` — our team's original candidate-side repo (PussyCatsApp).
  Reference only, do not modify.
- `UBB-SE-2026-UBB_Frat_Leaders/` — other team's repo. Reference only (UML diagrams
  in `uml.md` are useful when reconciling shared concepts). Do not modify.
- `UBB-SE-2026-Varis_vs_Clavicular/` — other team's repo. Reference only. Do not modify.
- `lab3/`, `lab1iss*`, `lab2sgbd`, `isslab1prob2`, `testlab1` — unrelated lab
  exercises, ignore unless explicitly asked.

The original `PussyCatsApp/` and `matchmaking/` source trees referenced in older
docs are not present in this workspace; the merged code in `UBB-SE-2026-921-1/`
is the canonical source.

## Plan and status

- `MergePlan.md` — architectural plan (4 projects, IRepository swap pattern, phases).
- `MergeStatus.md` — current state of the merge. Read this first; it supersedes
  the plan when they disagree.
- `CodingStyle.md` — 22 conventions.
- `CodeReviewChecklist.md` — review against this before declaring a phase done.

These four files exist both at the workspace root and at
`UBB-SE-2026-921-1/docs/`. Treat the workspace-root copies as the live working
versions; the in-repo copies are committed snapshots.

## Tech

- .NET 10, WinUI 3, EF Core, ASP.NET Core Web API.
- xUnit + FluentAssertions for tests.
- Build: `dotnet build`. Tests: `dotnet test`. Migrations: see plan §4.
- Database: `ISS-921-1` on `DESKTOP-M6HSOV2`, Windows auth (per MergeStatus.md).
