# Merge Status

Last updated: 2026-05-06
Current phase: 3b.3 committed — Phase 3 complete. Next: Phase 3c reconciliation.

This document tracks where the merge stands right now. The architectural
plan is in `MergePlan.md` and is unchanged. This file records what's been
decided and built since the plan was written, plus what's left.

## TL;DR

- Repo: `UBB-SE-2026-921-1`. Database: `ISS-921-1` on `DESKTOP-M6HSOV2`,
  Windows auth.
- Build green across all four projects (`PussyCats.App`, `PussyCats.Library`,
  `PussyCats.Api`, `PussyCats.Tests`).
- Phases 0, 1, 2, 3a committed. Phase 3b underway.
- DI not wired in `App.xaml.cs` yet. That's Phase 5. Don't add it during
  3b.
- View models from both original repos have not been touched. They
  still reference original namespaces and won't build against the
  merged code. Phase 5 ports them.

## Phase status

### Phase 0 — Skeleton (done, committed)

Four-project solution created with all references wired:
`App→Library`, `Api→Library`, `Tests→Library+App`, `Library→nothing`.
Solution uses legacy `.sln`, not `.slnx`, because WinUI platform
mappings need it. Tests retargeted to `net10.0-windows10.0.26100.0`
because referencing the WinUI App project requires it.

### Phase 1 — Domain + interfaces (done, committed)

`PussyCats.Library/Domain/` holds 19 entities and 9 enums. 13 DTOs in
`Library/DTOs/`. 13 `IRepository` interfaces in
`Library/Repositories/<aggregate>/`. All repository methods are
`async`, return `Task<T>`, and accept `CancellationToken ct = default`
as the last parameter.

Key domain decisions made:

- `User` is the merged rich shape from PussyCats's `UserProfile`,
  with matchmaking's preference fields folded in
  (`PreferredEmploymentType`, `WorkModePreference`,
  `LocationPreference`).
- `UserLevel` collapsed into two fields on `User`: `CurrentLevel`,
  `TotalExperiencePoints`. No separate entity.
- Skills are three tables: `Skill` (catalog), `UserSkill` (per-user
  with `Score`), `JobSkill` (per-job with `RequiredLevel`). Plus
  `SkillGroup` for role-compatibility scoring (separate concept from
  `Skill`, do not merge).
- `Match` keeps matchmaking's `MatchStatus` enum and the state
  machine. Navigation properties (`Match.User`, `Match.Job`)
  preferred over bare FKs per the assignment rule.
- `Developer`, `Interaction`, `Post`, `Chat` deferred per
  `MergePlan.md §8`. Stays mocked for now.

### Phase 2 — EF Core + initial migration (done, committed)

`PussyCatsDbContext` and per-entity `IEntityTypeConfiguration<T>`
classes under `Library/Persistence/Configurations/`. EF
`*Repository` implementations alongside the interfaces in
`Library/Repositories/<aggregate>/`.

`InitialCreate` migration applied to `ISS-921-1`. 18 tables, 18
foreign keys, 23 indexes, 365 seed rows.

Key persistence decisions:

- Cascade behavior: User-owned data cascades on User delete (Documents,
  WorkExperiences, Projects, ExtraCurricularActivities, SkillTests,
  PersonalityTestResults, Recommendations). `User → Match` and
  `Job → Match` are `Restrict` to preserve match history.
  `Skill → anything` is `Restrict` because the catalog is reference
  data. Verified column-by-column post-deploy.
- `Project.Technologies` stored as a single `nvarchar(max)` JSON
  column on the `Projects` table via EF Core 8 primitive collections.
  Not a separate junction table. **LINQ queries against this column
  must use `EF.Functions.JsonContains`, not `.Contains()`** —
  `.Contains()` won't translate.
- `SkillGroupSkills` join: `SkillGroup → Cascade`, `Skill → Restrict`.
  The Cascade-on-both-sides version was caught and fixed before
  apply.
- 152-skill catalog seeded, 48 SkillGroups with 153 group/skill
  memberships, 3 sample companies, 3 sample jobs, 6 JobSkill rows.
- Connection string lives in `PussyCats.Api/appsettings.Development.json`
  only. **Never in App.**

Connection string format that works with this setup:
Server=DESKTOP-M6HSOV2;Database=ISS-921-1;Trusted_Connection=True;TrustServerCertificate=True;

### Phase 3a — Port matchmaking services (done, committed)

`PussyCats.App/Services/` contains 12 service ports plus the algorithm
interface stub. All are async, take `CancellationToken`, use
`ConfigureAwait(false)`, return `IReadOnlyList<T>` where applicable.

Services ported from `matchmaking/Services/`:

- `UserService`, `JobService`, `CompanyService`
- `UserSkillService` (renamed during port — was `SkillService` in
  matchmaking, conflated catalog and per-user; the merged version
  wires only to `IUserSkillRepository`)
- `JobSkillService`, `CooldownService`
- `MatchService` (state machine preserved verbatim — see
  `IsDecisionTransitionAllowed` and `SubmitDecisionAsync`)
- `CompanyStatusService`, `UserStatusService`, `SkillGapService`
- `CompanyRecommendationService`, `UserRecommendationService`

Interface added: `IRecommendationAlgorithm` (signatures only, no
implementation yet — Phase 3b ports the algorithm class from
`matchmaking/algorithm/`).

DTOs moved from `matchmaking/Models/` into `Library/DTOs/`:
`ApplicationCardModel`, `MissingSkillModel`, `UnderscoredSkillModel`,
`SkillGapSummaryModel`.

Key decisions during 3a:

- `IUserStatusMatchRepository` retired. Status filters
  (`m.Status == MatchStatus.Rejected`) live in service bodies, not
  repository methods, per CodingStyle §8.
- `IJobSkillRepository.GetAllAsync` added to support
  `JobSkillService.GetAllAsync`.
- `CompanyStatusService.ComputeCompatibilityFallback` substitutes
  `User.City` for the missing `User.Location`. **Comment in the
  source flags that this comparison can produce false negatives
  because `User.City` stores bare city names while `Job.Location`
  may include country.** Phase 6 should normalize.
- `CompanyRecommendationService` is stateful (queue + currentIndex).
  Comment at the top of the class flags that **DI registration must
  be Transient or per-view-model** in Phase 5 to avoid leaking
  applicants between users.
- `IRecommendationAlgorithm` was originally specified as an empty
  marker interface but has two real call sites in the recommendation
  services. It carries two real method signatures now. The matchmaking
  algorithm implementation is Phase 3b work.

Async patterns set in 3a (followed by all subsequent service ports):

- `AddAsync` returns `Task<T>` with the saved entity. Service surfaces
  pass it through. View models that don't need the result can
  ignore.
- Sync wrappers around async methods are forbidden (CodingStyle §11).
  Methods that touch I/O are async, end in `Async`, take a
  `CancellationToken`. Pure-computation methods stay sync.

### Phase 3b — Port PussyCatsApp services (in progress)

Subdivided into three sessions because the source is messier than
matchmaking and there are real design decisions to make. Each
sub-phase commits separately.

**3b.1 — design-call services (done, committed)**

Ported four PussyCatsApp services with real design calls plus extended
the existing matchmaking-side `MatchService`. Includes a Library schema migration
(`AddSelectedRoleAndJobRole`) that adds:

- `PersonalityTestResult.SelectedRole` (nullable `JobRole?`)
- `Job.JobRole` (non-nullable, with seed data updated for the three
  existing seed jobs)

Migration applied to `ISS-921-1`. `__EFMigrationsHistory` has both
`InitialCreate` and `AddSelectedRoleAndJobRole` rows.

Services in 3b.1:

- `PreferenceService` — facade on `IUserRepository`. The original
  `IPreferenceRepository` was retired (preferences are User fields
  now). View models bind to `IPreferenceService` and don't notice.
- `UserProfileService` — `SaveAsync(int, User)` is a facade that
  branches internally to `AddAsync` or `UpdateAsync` based on
  existence. Phase 4 controllers call `AddAsync`/`UpdateAsync`
  directly per HTTP verb semantics; only services use the facade.
- `PersonalityTestService` — `SaveResultAsync(int, IReadOnlyDictionary<Question, AnswerValue>, JobRole, ct)`
  computes scores AND persists structured `PersonalityTestResult` +
  `PersonalityTraitScore` rows. The original blob-string `Save` is
  replaced. The 24 hardcoded questions stay in the service file
  (`MergePlan §4`). Trait-score `int`/`double` mismatch handled with
  `Math.Round` cast; flagged in the source.
- `CompatibilityService` — adds `IUserRepository` dependency to
  read `user.ParsedCv` directly, replacing the original's
  `IUserSkillRepository.GetParsedCvByUserId` workaround that was a
  raw SQL leak.
- `MatchService` (existing 3a file) gains
  `GetMatchesForUserAsync` and `GetMatchStatisticsAsync`. The
  statistics method groups matches by `JobRole` enum. Constants
  preserved verbatim: `LastMonth = 1`, `LastSixMonths = 6`,
  `LastYear = 12`.

**3b.2 — mechanical ports + constants source-of-truth (not started, playbook ready)**

Six-step session. No DI registration, no commit until the end (Phase 3
commits are 3b.1, 3b.2, 3b.3 as three reviewable chunks). Source files
live under `PussyCatsApp/Services/`; targets land in
`PussyCats.App/Services/` unless noted. Universal porting rules from
3b.1 apply (sync→async + `CancellationToken`, `ConfigureAwait(false)`
in services, `IReadOnlyList<T>` returns, etc.).

*Step 1 — `SimpleModelOperations` (the constants source-of-truth).*
New file `PussyCats.App/Services/SimpleModelOperations.cs`. Static class.
Constants exposed as `public const int` (match source types — keep float
if the original was float):
- `GoldScoreThreshold = 90`, `SilverScoreThreshold = 70`,
  `BronzeScoreThreshold = 50`
- `GoldExperiencePoints = 100`, `SilverExperiencePoints = 60`,
  `BronzeExperiencePoints = 30`, `ParticipantExperiencePoints = 10`
- Level thresholds discovered during 3b.1: `Level2 = 100`, `Level3 = 250`,
  `Level4 = 500`, `Level5 = 800`

`AssignTier` static method ported verbatim. **SkillTestService and
UserProfileService reference these from `SimpleModelOperations`; they
do not redefine locally.**

*Step 2 — `SkillTestService` (+ `ISkillTestService`).*
Constructor: `ISkillTestRepository`. All methods sync→async +
`CancellationToken`:
- `GetTestsForUser` → `GetTestsForUserAsync`
- `CanRetakeTest` → `CanRetakeTestAsync` (loads `SkillTest` by id — I/O)
- `SubmitRetake` → `SubmitRetakeAsync`
- `GetSkillTestById` → `GetSkillTestByIdAsync`

Local constant: `RetakeEligibilityMonths = 3` stays on this class — it's a
skill-test rule, not a tier rule. The static `GetExperiencePoints` helper
ports but switches on `SimpleModelOperations.GoldScoreThreshold` etc.,
not local copies.

*Step 3 — `DocumentService` (+ `IDocumentService`).*
Constructor: `IDocumentRepository` **and** `ILocalFileStorageService`.
The file-storage dependency stays — DocumentService orchestrates metadata
persistence with file storage (a service-layer concern). Mechanical
sigdiffs:
- `GetDocumentsByUserId` → `repo.GetByUserIdAsync`
- `AddDocument(Document)` (was `void`) → `repo.AddAsync(Document, ct)`
  returns `Task<Document>`; service returns the inserted Document.
- `GetDocumentById` → `repo.GetByIdAsync`
- `DeleteDocument` → `repo.RemoveAsync`

`ILocalFileStorageService` ports in 3b.3 with upload methods stubbed to
`NotImplementedException`; DocumentService still compiles, Phase 5 wires
the real file routing.

*Step 4 — `UserLevelService`.*
If recon's "static helper, no repo" call is right, port verbatim as a
static class. If recon was wrong and it has a repo dependency, port like
the others (sync→async + `CancellationToken`). If `MatchService`'s
3b.1-added XP/level statistics overlap with `UserLevelService`: flag,
don't merge — Phase 7 polish.

*Step 5 — `PredefinedLocations`.*
New file `PussyCats.App/Configuration/PredefinedLocations.cs`:

```csharp
namespace PussyCats.App.Configuration;
public static class PredefinedLocations
{
    public static IReadOnlyList<string> All { get; } = new[] { /* ~80 cities */ };
}
```

City strings copied **verbatim** from `PussyCatsApp/Services/PreferenceService.LoadPredefinedLocations`.

*Step 6 — touch existing files.*
- `PussyCats.App/Services/UserProfileService.cs` — replace inlined
  `GetExperiencePoints` and `GetLevelFromXp` helpers with
  `SimpleModelOperations.X` references. Delete the now-unused private
  methods.
- `PussyCats.App/Services/PreferenceService.cs` — replace the
  `SearchLocationsAsync` empty-list stub with `OrdinalIgnoreCase`
  `Contains` against `PredefinedLocations.All`, wrapped in
  `Task.FromResult` (no actual I/O; interface stays async per rule 13's
  spirit; sync alternative is also defensible).

End of session: `dotnet build` (full output), list new + modified files,
list deviations. Do not touch `CVParsingService`, `CompletenessService`,
`ImageStorageService`, `LocalFileStorageService`, `PdfExportService` —
those are 3b.3.

**3b.3 — pure helpers (done, committed)**

Final piece of Phase 3. Five PussyCatsApp services with no `IRepository`
dependencies. Port verbatim, preserve every constant and every line of
logic. Sync→async only where there's actual I/O. No DI, no commit.

Files to port (each with its interface, all into
`PussyCats.App/Services/`):
- `CVParsingService` (+ `ICVParsingService`)
- `CompletenessService` (+ `ICompletenessService`)
- `ImageStorageService` (+ `IImageStorageService`)
- `LocalFileStorageService` (+ `ILocalFileStorageService`)
- `PdfExportService` (+ `IPdfExportService`)

*File-storage write methods get stubbed.* For `LocalFileStorageService`
and `ImageStorageService`: read methods (load existing file by path)
port fully working — they only read from a known local directory.
Write/upload methods (save file, store image, etc.) get their bodies
replaced with:

```csharp
// Phase 5 routes uploads through /api/files; silent disk writes during
// demo would mask the bug.
throw new NotImplementedException(
    "Phase 5 routes file uploads through /api/files per MergePlan §4.");
```

Signatures, interface, and class structure stay intact — only the upload-
method bodies become the throw. Reason: silent disk writes during demo
are worse than a loud failure that points at the right phase.

*PdfExportService exception to the "no `Microsoft.UI.*` in services"
guideline.* WebView2 is the printer; the UI dependency is intrinsic.
Add at the top of the class:

```csharp
// justification: PdfExportService relies on WebView2 for HTML→PDF
// conversion; Microsoft.UI dependency is intrinsic to the chosen
// approach. Considered a service-layer exception per the layering
// rule in CodingStyle.
```

*Constants to preserve verbatim (recon-flagged business rules):*
- `CVParsingService`: `MaxSkills=30`, `MaxSkillLength=60`,
  `MaxFirstNameLength=50`, `MaxLastNameLength=60`, `MaxCountryLength=100`,
  `MaxCityLength=100`, `MaxUniversityLength=200`, `MaxGitHubLength=200`,
  `MaxLinkedInLength=200`, `MaxAddressLength=500`,
  `MaxMotivationLength=1000`, `MaxCompanyNameLength=150`,
  `MaxJobTitleLength=100`, `MaxWorkDescriptionLength=500`,
  `MinValidDate=1980-01-01`, `MaxYearsAheadForDate=1`. Also all
  in-method local constants inside private validation helpers — preserve
  every one.
- `CompletenessService`: `TotalFields=21`, plus the 21-string `Labels`
  array (order matters — tied to case index in the prompt-generation
  switch).
- `ImageStorageService`: `BytesPerKilobyte=1024`, `MaxFileSizeInMb=5`.
- `LocalFileStorageService` and `PdfExportService` — port whatever
  constants exist verbatim.

End of session: `dotnet build` (full output); confirm green across all
four projects; full list of upload/write methods now throwing
`NotImplementedException`, by service; deviations.

Phase 3 is complete once 3b.3 commits.

### Phase 3c — Reconciliation (not started)

`UserService` and `UserProfileService` are kept side-by-side through
3b. 3c reviews whether they should remain separate (different
concerns: identity vs. profile content) or be merged into one
service. PussyCats's `MatchService` methods (`GetMatchesForUserAsync`,
`GetMatchStatisticsAsync`) are already absorbed into the
matchmaking-side `MatchService` during 3b.1, so no further match
reconciliation work.

### Phase 4 — Web API (not started)

Controllers per the route plan in `MergePlan §5`. Each ~20-50
lines, thin, delegates to injected `IXRepository` or to a
service. DI registers EF repositories. Local on
`https://localhost:7000`. File-storage routes go live here.

### Phase 5 — App services + RepositoryProxies + view-model migration (not started)

Two streams. First, build `*RepositoryProxy` HTTP clients for each
`IRepository` and wire DI in `App.xaml.cs` to bind interfaces to
proxies (NEVER to the EF repos — assignment hard rule). Second,
migrate the original view models from PussyCats and matchmaking onto
the merged service surfaces. Several known deltas need fixing here:

- `UserProfileService.UpdateAccountStatusAsync` now takes `bool`
  instead of `"ACTIVE"`/`"INACTIVE"` strings. View models that
  built the string need adjusting.
- `UserRecommendationViewModel` (matchmaking) constructs a
  `SkillRepository` inline — bypass through service.
- `JobRecommendationResult` lost display helpers
  (`JobTitleLine`, `DescriptionExcerpt`, `BuildExcerpt`,
  `TakeTopSkills`). View models need them either restored on the
  DTO or computed in a wrapper.

### Phase 6 — UI shell (not started)

Single `MainWindow` with `NavigationView`. Candidate/Company
toggle. All pages migrated into one Frame. No new windows opened.
Replace popup-as-window patterns with `ContentDialog`/`Flyout`.

### Phase 7 — Tests (not started)

Per-aggregate `Fake*Repository` in `Tests/Fakes/`. Port matchmaking
xUnit tests as-is. Rewrite PussyCats MSTest tests to xUnit. Service
tests use fakes only — no DB, no network.

### Phase 8 — Polish (not started)

StyleCop pass. Coverage. Dry-run demo. Tag `v4.0`.

## Open items / known issues

These are flagged in code or in conversation but haven't been
resolved yet:

- `CompanyStatusService.ComputeCompatibilityFallback`: `User.City`
  vs `Job.Location` format mismatch produces silent false
  negatives. TODO in source.
- `CompanyRecommendationService` and `UserRecommendationService`
  cannot be DI-registered until 3b ports the
  `RecommendationAlgorithm` class from `matchmaking/algorithm/`.
- The empty `Questions` table from Phase 2 is dead weight — the
  questions live in `PersonalityTestService` per
  `MergePlan §4`. Drop in Phase 8.
- `UserStatusService` and similar services have an N+1 query
  pattern (one repository call per match in a loop). Preserved from
  matchmaking originals. Phase 6 demo will surface real
  performance issues if they exist.
- The CV parser caps `Motivation` at 1000 chars; the DB column caps
  at 2000. Intentional — parser is the friendly cap, DB is the
  safety net.
- `IRecommendationAlgorithm` interface in `App/Services` violates
  CodingStyle §6 strictly speaking (interfaces with implementations
  in App rather than Library), but the algorithm uses Library
  domain types so moving it later is trivial.
- `Preference` DTO in `Library/DTOs/` exists only to preserve
  `IPreferenceService.GetByUserIdAsync`'s legacy return shape. The
  service translates User fields into `Preference` objects on the way
  out and parses them back on the way in. Slated for replacement when
  view models migrate in Phase 5 — consider a flat `UserPreferences`
  record instead.

- `UserProfileService.RecalculateLevelAsync` inlines XP and level
  thresholds. Phase 3b.2 will replace them with
  `SimpleModelOperations.X` references when that file lands.

- New level thresholds discovered during port: Level2=100, Level3=250,
  Level4=500, Level5=800. Add to SimpleModelOperations during 3b.2.

- `MatchService.GetPositionKey` returns display labels for
  `MatchesPerPosition`. Original PussyCatsApp stored arbitrary strings;
  only "Backend" and "Frontend" had test coverage. The other six labels
  are designer-chosen for legibility ("UI/UX Design", "AI/ML Engineering",
  etc.) and may be revised by Phase 6 design.

- `PersonalityTestResult.SelectedRole` is nullable. Null = test not
  taken or role not yet selected. Code reading this field must
  null-check.

- `CompletenessService` index 18 ("Preferred Roles"): original used
  `UserProfile.PreferredJobRoles` (list). `User` has no equivalent list;
  mapped to `user.PersonalityResult?.SelectedRole != null`. Phase 5 view
  model migration should reconsider this mapping.

- `CvParsingService` uses internal helper types (`CvData`, `CvWorkExperience`,
  `CvProject`, `CvActivity` in `CvData.cs`) instead of domain types for
  deserialization, to avoid `XmlSerializer` circular-reference failure caused
  by `WorkExperience.User` nav property. `ProcessSkills` returns
  `List<UserSkill>` with skeleton `Skill` objects (Name only, IsVerified=false,
  Score=0) rather than `List<string>`. These placeholder `UserSkill` rows must
  not be persisted directly — Phase 4 API controllers resolve skill names
  against the catalog before saving.

- `App.MainAppWindow` static property added to `App.xaml.cs` (required by
  `PdfExportService.DownloadPdfAsync` for the FileSavePicker window handle).
  Phase 5 DI wiring should leave this in place.

## Working norms (carried from earlier sessions)

- One phase per Claude Code session. Fresh context each time.
- Commit at every phase boundary. Small commits over big ones.
- For service-layer work: consult original repos for state machines
  and constants. Preserve verbatim. Flag deviations.
- View models stay frozen until Phase 5. Service ports in 3b must
  not break view-model compilation surfaces (use facades when the
  data layer's natural shape would).
- Push back on design issues before commit, not after.
- Stop and show output before destructive or irreversible actions
  (migrations, `database update`, anything mutating).

## Reading order for the new teammate

1. `MergePlan.md` — the full architectural plan.
2. `CodingStyle.md` — 22 rules. Especially 6, 8, 10, 11, 12, 13.
3. `CodeReviewChecklist.md` — what reviewers look for.
4. This file — current state.
5. `PussyCats.Library/Domain/` — 30 minutes browsing the entities
   and enums to internalize the merged shape.
6. `PussyCats.Library/Repositories/` — interface signatures. The
   repo public surface is the contract everything else depends on.
7. `PussyCats.App/Services/` — current service ports. Read
   `MatchService` first because the state machine is the
   highest-stakes piece of preserved business logic in the codebase.
