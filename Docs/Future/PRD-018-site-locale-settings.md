# Site Locale and Time Zone Settings PRD

**Status:** Proposed (discovery draft)
**Priority:** Medium
**Last Updated:** 2026-09-23
**Feature ID:** 018
**Scope:** Server-side per-site locale configuration (time zone and regional formatting) plus the client settings surface to edit it. Replaces the hardcoded `Australia/Sydney` offset in `Pot.App/AppContext.cs` and the hardcoded `"$"` currency symbol. No email template changes, no UI language localisation.
**Audience:** Future implementation agent(s), reviewers, and maintainers.

## Purpose

Define how a site's locale — its time zone and regional formatting conventions — is configured, persisted, and resolved at runtime, so that every date/time and money value the application derives or displays is based on the site it belongs to rather than a single hardcoded zone.

This PRD is a dependency of [PRD 019 — Email Outbox](PRD-019-email-outbox-durable-dispatch.md): the outbox keys a scheduled notification by the occurrence's **site-local** date, so the notion of "the site's local day" must exist before that key can be defined.

## Problem Statement

`IAppContext.TimeZoneOffset` returns the offset for a hardcoded IANA zone, recalculated on every access:

```csharp
var ianaTz = "Australia/Sydney";
var windowsTz = TZConvert.IanaToWindows(ianaTz);
var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? windowsTz : ianaTz);

return tzInfo.GetUtcOffset(DateTime.UtcNow);
```

`IAppContext` is a singleton (`IPotSingletonDependency`) and `ITimeProvider` is also a singleton, so the entire application treats Sydney time as "local" time for every site and every user. As a result:

1. **Accrual dates are wrong outside AEST/AEDT.** The canonical accrual start date and accrual recalculation derive from `GetLocalDateNow()` (`AccrueExpenseCalculator`, `AccrueExpensesService`, `CreateExpenseService`, `UpdateExpenseService`, `DeleteExpenseService`), so an expense created late in the day in another region is anchored to the wrong calendar day.
2. **"Today" is the Sydney date.** Projections and accrual-status responses report `Today = timeProvider.GetLocalDateNow()` (`ProjectionsService`, `Features/Projections/Get/Handler`, `Features/Accruals/Status/Handler`).
3. **The reminder hour is per-site but the clock is not.** `EmailBudgetReminderSettings.LocalHourTrigger` is stored per site, yet the hour it is compared against comes from the shared, Sydney-based `ITimeProvider`. Two sites in different zones cannot both honour their configured reminder hour.
4. **Export filenames are stamped with Sydney time** (`Features/Maintenance/Export/Handler`).

There is a second, smaller hardcoding: money and dates are formatted with fixed en-AU/AUD assumptions. `BudgetReminderService` sets `CurrencySymbol = "$"` when building the reminder email, and the client's `formatMoneyValue` defaults to `currency = 'AUD'` with an `en-AU` locale (`Source/Client/pot-react/src/lib/moneyUtils.ts`).

`AppContext.cs` already documents the intended design — an IANA zone identifier persisted in the database, translated to a Windows identifier only where the OS requires it, with the client detecting the user's zone via `Intl.DateTimeFormat().resolvedOptions().timeZone` and offering a picker from a package such as `@vvo/tzdb`. Only the comment exists today; there is no setting, no per-site resolution, and no editing surface.

## Current Behaviour

### Time zone resolution

- `Pot.App/AppContext.cs` — `IAppContext` (`IPotSingletonDependency`) exposes `TimeSpan TimeZoneOffset`, hardcoded to `Australia/Sydney` and recomputed per access.
- `Pot.App/Concerns/Time/PotTimeProvider.cs` — `ITimeProvider` (`IPotSingletonDependency`) derives "local" values by `utcNow.ToOffset(_appContext.TimeZoneOffset).DateTime`, returned with `DateTimeKind.Local`. The kind is misleading in a container: the values are in the site's zone, not the host's (which is typically UTC).
- `Pot.App/Extensions/DateTimeExtensions.cs` — `ConvertToLocalDateTime(...)` also converts through `appContext.TimeZoneOffset`.
- The offset is a `TimeSpan`, so zone identity is lost: conversions use _now's_ offset and cannot account for a DST transition at another point in the day, nor for the fact that the offset is only valid for part of the year.

### Settings mechanism (the intended storage)

- `Pot.Data/Entities/SettingEntity.cs` — `Category` (`SettingCategory` enriched enum), `Key`, `Value`, nullable `Site`; unique index on `(SiteId, Category, Key)`.
- `Pot.Shared/Enumerations/SettingCategory.cs` — currently only `EmailBudgetReminder = new(1)`; database values are stored with a maximum length of 50 characters.
- `Pot.App/Features/Settings/Models/EmailBudgetReminder/*` — the established per-category pattern: a `sealed partial class` split into properties (`EmailBudgetReminderSettings.cs`), metadata/defaults/type resolvers (`.Metadata.cs`), and validation (`.Validation.cs`) implementing `ISettingValueValidatable` via the static-abstract `ValidateValue` pattern.
- `Pot.App/Features/Settings/GetAll/GetAllSettingsService.cs` — `DefaultSettingsRegistry` maps a `SettingCategory` to its defaults; `Pot.App/Features/Settings/Upsert/UpsertSettingService.cs` — `SettingValueValidators` maps a category to its validator.
- `Pot.Data/PotDbContext.cs` — `SetupQueryFilters` filters `SettingEntity` to `setting.Site != null && setting.Site.Id == GetCurrentUserSiteId()`, where the site is resolved from the scoped `ICurrentUserContext.UserRowId`. Settings are therefore already per-site by construction.
- Client: `Source/Client/pot-react/src/features/userSettings/POTSettingsSheet.tsx` with per-section forms (`sections/budgetReminders/BudgetRemindersForm.tsx`, `sections/settingsSectionForm.ts`), typed through `src/data/settings.ts`.

### Site

- `Pot.Data/Entities/SiteEntity.cs` — `Name`, `Description`, `Users`, `Accounts`, `Settings`. There is no locale field; site attributes are limited to name/description (`Sites/Update`).

## Proposed Solution

Candidate requirements; nothing is implemented.

- **R1 — Locale settings category.** Add `SettingCategory.SiteLocale` with keys for the site's IANA time zone identifier (for example `Australia/Sydney`) and its currency code (ISO 4217, for example `AUD`), following the existing three-file per-category pattern (`Properties` / `Metadata` / `Validation`). The `TimeZoneId` validator must verify the value resolves on the current platform (via `TimeZoneConverter` on Windows) rather than accepting an arbitrary string.
- **R2 — Site-scoped resolution.** Resolve the locale from the current site's settings so that request handling, background work, and client reads all observe the same value. This requires resolving a scoped, site-filtered setting from code that is currently singleton — see OD-04.
- **R3 — Zone-aware conversion contract.** Replace the `TimeSpan TimeZoneOffset`-based conversion with a `TimeZoneInfo`-based one (resolve the IANA id to a `TimeZoneInfo` and convert with `ConvertTimeFromUtc`), so conversions are correct for the target instant across DST boundaries. This is a contract change to `IAppContext`/`ITimeProvider`.
- **R4 — Correct `DateTimeKind`.** Stop returning `DateTimeKind.Local` for site-local values; the kind is not the host's local kind. `Unspecified` (or an explicit `DateTimeOffset`) is the accurate representation.
- **R5 — Currency for money presentation.** Derive the reminder email's `CurrencySymbol` from the site locale rather than the literal `"$"`, and feed the configured currency/locale into client formatting instead of the hardcoded `AUD`/`en-AU` defaults.
- **R6 — Client editing surface.** Add a locale section to the settings sheet with a time-zone picker seeded from the browser's detected zone (`Intl.DateTimeFormat().resolvedOptions().timeZone`) and a currency selection, saving through the existing settings upsert endpoints.
- **R7 — Fallback for unset sites.** Define and document the behaviour when a site has no locale configured, rather than silently inheriting Sydney. See OD-05.
- **R8 — Reminder hour alignment.** The reminder trigger comparison uses the site's local hour (this is what makes `LocalHourTrigger` meaningful), which is a behaviour change for every site not in Sydney.

## Non-Goals

- **No UI language translation or localisation of strings and email templates.** "Locale" in this PRD means time zone and regional number/date/currency formatting only. Text localisation is a separate initiative with its own scope.
- **No change to storage format for timestamps.** UTC remains the persisted representation; only the conversion and presentation layer changes.
- **No per-user locale override** unless OD-01 decides otherwise.
- **No retroactive correction of existing data.** Rows already written under the Sydney assumption (accrual start dates, reminder history) are not recalculated.

## Tests

Planned coverage, following the existing split between unit and integration tests:

- **Settings model** — validation of `TimeZoneId` (valid IANA id, Windows-mapped id, unknown id rejected) and currency code, mirroring the `EmailBudgetReminderSettings.Validation` fixtures.
- **Resolution** — the site's configured zone is applied for a request in that site; a second site with a different zone resolves independently; an unset site follows the R7 fallback.
- **Conversion** — DST boundary cases for a southern-hemisphere zone (offset differs either side of the transition), plus a `DateTimeOffset`-based assertion replacing the current offset-only assertions in `Pot.App.Tests/Concerns/Time/PotTimeProviderFixture.cs` (which currently stubs `TimeZoneOffset` and will need rework under R3).
- **Consumers** — accrual date derivation, projections `Today`, and the reminder trigger hour each asserted against a non-Sydney site.
- **Integration** — settings upsert/get round-trip for the new category through `ApiWebApplicationFactory`.
- **Client** — settings form (zone picker seed, save, validation errors) under `Source/Client/pot-react/tests/**`, plus money formatting against a non-AUD currency.

## Open Decisions Backlog

| ID    | Decision                                                                   | Candidate Options                                                                                              | Status  |
| ----- | -------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- | ------- |
| OD-01 | Locale granularity                                                         | Site-level only · Site-level with per-user override                                                            | Pending |
| OD-02 | Which fields constitute "locale"                                           | Time zone only · Time zone + currency · Time zone + currency + date/number formats                             | Pending |
| OD-03 | How the zone is modelled                                                   | Persisted IANA id resolved to `TimeZoneInfo` · Persisted fixed offset                                          | Pending |
| OD-04 | How singleton `IAppContext`/`ITimeProvider` obtain a site-filtered setting | Make time resolution scoped · Read through an explicit ambient/site context · Cache per site with invalidation | Pending |
| OD-05 | Behaviour for sites with no configured locale                              | Migration seeds `Australia/Sydney` for existing sites · Explicit fallback to UTC · Block until configured      | Pending |
| OD-06 | Source of the client's initial zone value                                  | Browser-detected zone as the default · Always explicit selection                                               | Pending |
| OD-07 | Whether `CurrencySymbol` remains a string or becomes the ISO code + format | Symbol derived server-side · ISO code sent and formatted client-side                                           | Pending |

## Outstanding Questions Register

1. Does the site locale need to apply to the reminder email's date formatting (`NextDue` is currently rendered with a fixed format), or only to the currency symbol?
2. Should the export filename timestamp follow the site locale (it is currently the only non-request-scoped consumer)?
3. Are there sites today with users in more than one zone, i.e. is site granularity actually sufficient?
4. Is the `Australia/Sydney` fallback acceptable as a permanent default, or must sites be forced to choose?
5. Does changing accrual date derivation (R1–R4) retroactively alter any in-flight accrual state, and does the accrual dirty-flag behaviour (PRD-009) need revisiting as a result?

## Assumptions and Constraints Register

| ID    | Assumption / Constraint                                                                                                   | Confidence |
| ----- | ------------------------------------------------------------------------------------------------------------------------- | ---------- |
| AC-01 | Settings are already site-scoped by `PotDbContext` query filters, so no new ownership model is required.                  | High       |
| AC-02 | `TimeZoneConverter` remains the cross-platform mapping mechanism for IANA ↔ Windows ids.                                  | High       |
| AC-03 | The single deployment (`IAppContext` hardcodes one zone) means no data written to date contains mixed zones.              | Medium     |
| AC-04 | Changing `IAppContext`/`ITimeProvider` shape is a breaking internal contract change with a wide but shallow blast radius. | High       |
| AC-05 | Client money formatting is centralised in `moneyUtils.ts`, so currency changes have one client seam.                      | Medium     |

## Discovery Exit Criteria

Before this PRD moves from Proposed to Planning:

1. OD-01, OD-02 and OD-03 are decided, because they determine the settings schema and the conversion contract.
2. OD-04 is decided, because it determines whether `IAppContext`/`ITimeProvider` change lifetime — the largest blast radius in this feature.
3. The list of local-time consumers is confirmed complete against `GetLocalDateNow` / `GetLocalDateTimeNow` / `GetLocalTimeZoneOffset` / `ConvertToLocalDateTime` usage.
4. The reminder-hour behaviour change (R8) is explicitly accepted, since it alters when existing sites receive reminders.

## Design Notes (Rationale and Rejected Alternatives)

Reference notes for future maintainers.

- **Persist the IANA identifier, not an offset.** A fixed offset cannot represent a DST transition, and cannot answer "what was the local date at 23:30 on this date?" with a different answer in summer and winter. The zone identity is the durable value; the offset is derived.
- **Do not add a locale column to `SiteEntity`.** The settings mechanism already provides per-site key/value storage, validation, defaults, an editing surface, and query filtering. A parallel column would duplicate all of that and split the source of truth. (Subject to OD-02: if the field count stays tiny and no other category needs it, a column remains arguable — recorded as a rejected alternative rather than a settled choice.)
- **The current `TimeSpan` contract is the root defect, not just the hardcoded string.** Swapping `Australia/Sydney` for a configurable string while keeping `TimeSpan` would fix the reported symptom and leave the DST and zone-identity defects in place.
- **`DateTimeKind.Local` is wrong here.** Values are in the site's zone; the host's zone in a container is usually UTC. Labelling them `Local` invites incorrect host-based conversions downstream.
- **This is a prerequisite for PRD-019, not an optimisation.** The outbox dedupe key is a site-local occurrence date; without a site-local date the key is undefined, which is why the outbox work is sequenced behind this PRD.

## Related Documents

- Future index: [Docs/Future/README.md](README.md)
- Dependent feature: [Email Outbox and Durable Dispatch](PRD-019-email-outbox-durable-dispatch.md)
- Settings feature area: `Source/Server/Pot.App/Features/Settings`, `Source/Client/pot-react/src/features/userSettings`
- Accrual behaviour that depends on local dates: [Accrual Dirty Flag Rules and UI Status](PRD-009-accrual-dirty-flag-rules-and-ui-status-prd.md)
