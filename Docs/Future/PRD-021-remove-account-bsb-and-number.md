# Remove Account BSB and Account Number PRD

**Status:** Proposed
**Priority:** High
**Last Updated:** 2026-09-24
**Feature ID:** 021
**Scope:** Removal of the BSB and account number fields from the account data model and every surface that persists, returns, renders, or exports them. Covers `Pot.Data`, `Pot.App`, `Pot.AspNetCore`, the React client, the maintenance export/import package format, and the ERD/Postman artifacts. No change to the export/import mechanism itself, no encryption work, and no audit-logging work.

## Problem Statement

An account stores `Bsb` (`varchar(7)`, constrained to `###-###`) and `Number` (`varchar(20)`) in plaintext, and that pair is surfaced on every read path. BSB plus account number plus an account holder name is the data set required to establish an unauthorised direct debit. POT is projection-only and never initiates a payment, so the full values deliver no product capability.

The pair is not load-bearing. Expenses, incomes, accruals, and projections all resolve their account through `AccountRowId`, and maintenance imports match rows on `RowId`. `Description` is already unique per site:

```csharp
[Index("SiteId", nameof(Description), IsUnique = true)]
```

So account identity, duplicate detection, and display recognition are all satisfied without the bank details. The current design is maximum blast radius for zero functional benefit: every copy — database dump, backup archive, CSV export, API response, screenshot — is independently exploitable and cannot be recalled.

## Current Exposure

| Surface                       | Exposure                                                  | Location                                                                         |
| ----------------------------- | --------------------------------------------------------- | -------------------------------------------------------------------------------- |
| Database                      | Full BSB and full account number, plaintext               | `Pot.Data/Entities/AccountEntity.cs`                                             |
| `GET /accounts/{id}`          | Both values in the response body (`account:view`)         | `Pot.AspNetCore/Features/Accounts/Get/Response.cs`                               |
| `GET /accounts`               | Both values per row in the response body (`account:view`) | `Pot.AspNetCore/Features/Accounts/GetAll/Response.cs`                            |
| Accounts table                | Full values rendered per row                              | `features/accounts/components/AccountsTable.tsx`                                 |
| Account mobile card           | `BSB: {bsb}` plus the number                              | `features/accounts/components/AccountMobileCard.tsx`                             |
| Dashboard account card        | `BSB: {bsb}` plus the number                              | `features/dashboard/components/AccountCard.tsx`                                  |
| Delete confirmation           | Both values embedded in the prompt text                   | `features/accounts/components/AccountActions.tsx`                                |
| CSV export                    | `Bsb` and `Number` columns (`maintenance:export`)         | `Pot.App/Features/Maintenance/Export/Accounts/AccountsExporter.cs`               |
| Duplicate-check error payload | Both values echoed back in the API error                  | `Accounts/{Create,Update}/EntityChecks/Checks/CheckAccountNumberDoesNotExist.cs` |

There is no masking, redaction, encryption, or reveal-consent on any of these paths. The only access control is the `account:view` permission, which is site-scoped — so every user who can read balances also reads the full account number.

## Solution

### Data model

`AccountEntity` loses both properties and the composite unique index:

- Remove `Bsb` and `Number`, including their attributes.
- Remove `[Index(nameof(Bsb), nameof(Number), IsUnique = true)]`.
- Keep `[Index("SiteId", nameof(Description), IsUnique = true)]`, which becomes the sole account identity constraint.
- Delete `Pot.Data/Annotations/AccountBsbAttribute.cs` (its only consumer is `AccountEntity`).
- Remove `AccountSpecifications.IsSameBsbNumber`; `IsSameDescription` remains.
- Remove the `(string bsb, string number)` overloads of `AccountExistsAsync` and `GetAccountOrDefaultAsync` from `IAccountRepository` and `AccountRepository`. They are used only by the checks being deleted; every other caller uses the `Guid` overloads.
- Add a migration that drops `IX_Account_Bsb_Number` and both columns from the `Account` table. The migration is to be eperformed by the human so they can execute it using the IDE.
- Remove `Bsb` and `Number` from `Pot.TestUtils/EntityFactory.cs`.

### Application layer

- `Accounts/Create/Models/Input.cs`, `Accounts/Update/Models/Input.cs`: remove both properties.
- `Accounts/Create/CreateAccountService.cs`, `Accounts/Update/UpdateAccountService.cs`: remove the assignments to the entity.
- Delete `Accounts/Create/EntityChecks/Checks/CheckAccountNumberDoesNotExist.cs` and `Accounts/Update/EntityChecks/Checks/CheckAccountNumberDoesNotExist.cs`. Both are discovered through `IEnumerable<IPreCreateCheck>` / `IEnumerable<IPreUpdateCheck>` injection, so no registration code changes.
- `Accounts/Update/EntityChecks/Checks/CheckDescriptionDoesNotExist.cs`: the check currently excludes the account being edited by testing `IsSameBsbNumber(input.Bsb, input.Number).Not()`. Replace that with the `Id`-based self-exclusion already used by the expense and income equivalents:

  ```csharp
  var predicate = AccountSpecifications
      .IsSameDescription(input.Description).Expression
      .And(account => account.Id != state.AccountToUpdate.Id);
  ```

  This removes the last dependency on the pair without introducing `RowId` into an internal uniqueness predicate. The existing `account.Description != input.Description` guard stays: with self-exclusion expressed in the predicate it is a short-circuit that avoids a round-trip when the description is unchanged, no longer a correctness requirement.

- `Accounts/Get/Models/Output.cs`, `Accounts/GetAll/Models/Output.cs` and their `EntityMapping` classes: remove both fields.

### API contract

- `Accounts/Create/Request.cs`, `Accounts/Create/RequestValidator.cs`, `Accounts/Create/Mappings/RequestMapping.cs`: remove `Bsb` and `Number` and their `IsNotEmpty` rules.
- `Accounts/Update/Request.cs`, `Accounts/Update/RequestValidator.cs`, `Accounts/Update/Mappings/RequestMapping.cs`: same.
- `Accounts/Get/Response.cs`, `Accounts/GetAll/Response.cs`: remove both properties and their `[Description]` metadata.

This is a breaking contract change for both request and response payloads. It is intentional: `PUT /accounts/{id}` no longer round-trips a value the client should not hold, which also removes the read-only-BSB-in-edit-mode workaround in the client form.

### Client

- `data/account.ts`: remove `bsb` and `number` from the schema.
- `features/accounts/schemas/accountFormSchema.ts`: remove both fields and the `XXX-XXX` regex rule.
- `features/accounts/components/AccountForm.tsx`: remove the BSB and Account Number form fields.
- Delete `components/input/BsbInput.tsx` and its export from `components/input/index.ts`. It has no remaining consumer.
- `features/accounts/components/AccountsTable.tsx`: remove the `bsb_number` column.
- `features/accounts/components/AccountMobileCard.tsx`, `features/dashboard/components/AccountCard.tsx`: remove the BSB/number lines. The account is identified by `Description`, which is already unique per site.
- `features/accounts/components/AccountActions.tsx`: the delete confirmation identifies the account by description instead of `({bsb}) {number}`.
- `features/accounts/create/CreateAccountSheet.tsx`, `features/accounts/edit/EditAccountSheet.tsx`, `features/accounts/edit/useAccountEditor.ts`: remove both fields, including the dirty-check comparisons in `useAccountEditor`.
- `hooks/useAccountFilter.ts`: remove `bsb` and `number` from the virtual "Not Assigned" account literal.

### Export/import package

`accounts.csv` loses two columns, so the package format advances to v4 (`MetadataV1` → `V2` → `V3` → `V4`). The importer accepts a **version window** of v3–v4 instead of only the current version, which is possible only because the CSV mapping changes at the same time.

#### Version window

`MetadataBase` gains `MinimumSupportedVersion` (`3`) beside `CurrentVersion` (`4`), and `ImportDataService` rejects a package whose version falls outside `[MinimumSupportedVersion, CurrentVersion]`. The upper bound restates the check that exists today; the lower bound is a deliberate constant, raised only when an older shape genuinely becomes unreadable.

#### Name-based CSV mapping (all three files)

The import rows map columns by position (`[Index(n)]`), while the exporters write them by name (`AddField(nameof(...))`). Removing two columns from the middle of `accounts.csv` therefore shifts every later field for a positional reader: a v3 file would put `Bsb` into `Description` and the account number into `Balance`, yielding either a misleading "invalid format" 422 or silently wrong money values. Relaxing the version gate without changing the mapping would convert a clean rejection into misread data.

- `AccountCsvRow`, `ExpenseCsvRow`, and `IncomeCsvRow` switch to `[Name(nameof(...))]` mapping, so column order stops mattering.
- A v3 `accounts.csv` then reads correctly with the v4 row type: `Bsb` and `Number` are unmapped and ignored, and every v4 property name is already a header in v3.
- One row type per file reads every supported version, so the window adds no version-specific reader code. The exporter already depends on this tolerance: it writes nine `accounts.csv` columns while `AccountCsvRow` maps eight, so an unmapped trailing column is ignored today.

#### Metadata

The metadata payload is an `int32` version followed by a `DateTime`, identical for every version; the version-specific readers differ only in the type they construct, and `ReadMetadataVersion()` reads the version straight from the stream. `MetadataV4`, `MetadataV4Reader`, and `MetadataV4Writer` are therefore added for the version constant and the column documentation only.

- `MetadataBase.CurrentVersion` becomes `4`; `MetadataReaderFactory` and `MetadataWriterFactory` produce the V4 reader/writer; `ExportDataService` emits `MetadataV4`.
- `ImportDataService` deserializes with the latest metadata type and logs the version read from the stream, not `metadata.Version` (a compile-time constant that would report v4 for a v3 package).
- `AccountData`, `AccountsExporter`, `IAccountCsvRow`, `AccountCsvRow`, and `AccountsImporter` drop both fields.

#### Gate before writing the mapping change

Name-based mapping is viable only if CsvHelper fails loudly when a mapped property has no matching header. If it throws a `CsvHelperException` subtype, that already surfaces through `IsInvalidImportPayloadException` as a 422 and the approach proceeds as designed. If it instead leaves the property at its default, a later version that drops or renames a column would import zero-filled values undetected. Settle this with a test (or a throwaway spike) before changing the mapping; if the behaviour is the silent case, **stop and review options** rather than proceeding.

### Reference artifacts

- `Docs/pot_erd.d2`: remove the `Bsb` and `Number` lines from `finance.Account`.
- `Data/Postman/POT.postman_collection.json`: remove `bsb`/`number` from the account create and update request bodies.
- `Docs/USER-GUIDE/Accounts.md` already describes account creation as "description, balance, and optional reserved amount" and requires no change.

## Out of Scope

- Encryption, tokenisation, or column-level protection. Removal supersedes the need.
- Masked storage or masked display variants (BSB plus last-3/4 digits) — rejected in favour of removal.
- Audit logging of bank-detail access, because no bank details remain to audit.
- Existing plaintext copies on disk (`Data/Exports`, `Data/Backups`, `Data/ProdLike Backups`, `Data/Azure/Backups`, `Data/Azure/Exports`). Their disposition is the repository owner's concern; this PRD neither inventories nor purges them.
- The maintenance import/export transport, endpoints, or permissions. The CSV column set, the version gate, and the CSV mapping mechanism do change, as described above.
- `Projections` and `Accruals` behaviour: both already resolve accounts by `AccountRowId`.

## Tests

Coverage is removal-shaped: delete the tests that assert the removed behaviour, and re-point the fixtures that used the pair as a unique key.

- **Data** — `Pot.Data.Tests/Specifications/AccountSpecificationsFixture.cs`: delete the `IsSameBsbNumber` fixture; `IsSameDescription` coverage remains.
- **Application** — delete the `CheckAccountNumberDoesNotExist` fixtures under `Pot.App.Tests/Features/Accounts/{Create,Update}/EntityChecks/Checks/`; update `Checks/CheckDescriptionDoesNotExistFixture.cs` for the `Id`-based self-exclusion, including a case asserting an account may keep its own description unchanged; remove `Bsb`/`Number` from the account construction in `Features/Expenses/{Create,Renew,ToggleExclude,Update}/*ServiceFixture.cs` and `Features/Projections/ProjectionsServiceFixture.cs`; update `Pot.TestUtils/EntityFactory.cs` consumers.
- **Integration** — update the accounts create/update/get contract fixtures under `Pot.AspNetCore.Integration.Tests/Features/Accounts/` so request and response payload assertions match the new shape, and confirm a duplicate `Description` within a site still returns the existing 422 entity-exists response.
- **Client unit** — delete `tests/components/input/BsbInput.test.tsx`; drop the BSB cases from `tests/features/accounts/schemas/accountFormSchema.test.ts`; update `tests/data/account.test.ts`, `tests/features/dashboard/components/AccountCard.test.tsx`, `tests/shared/factories/accountFactory.ts`, and `tests/integration/CoreFlows.CreateAccountFlow.test.tsx` (including deletion of the BSB-masking test).
- **E2E** — `e2e/tests/crud/accountsCrud.test.ts` stops filling the BSB field; the BSB format-validation test in `e2e/tests/feedback/formValidation.test.ts` is deleted; the `BSB:` card assertion in `e2e/tests/mobile/mobileCardGrids.test.ts` is replaced with a description-based assertion; `e2e/helpers/api.ts` switches its per-call uniqueness from the `(Bsb, Number)` pair to a unique description per site, since that index comment documents the collision-avoidance this removal invalidates.
- **Migration** — verify the drop applies cleanly against a database at the previous revision, and that the unique per-site description index continues to reject duplicates.
- **Import compatibility** — the maintenance import/export path currently has no test coverage, so this is new ground. Cover: a v3 package and a v4 package both import; a version below the minimum and one above the current version are both rejected; a v3 `accounts.csv` carrying `Bsb`/`Number` imports with those columns ignored and every remaining field landing on the correct property; expenses and incomes behave the same under name-based mapping; and a header missing a mapped column produces the existing 422 rather than importing defaults. The missing-header case is the gate above and is written first.

## Design Notes (Rationale and Rejected Alternatives)

Reference notes for future maintainers, recording why the design looks the way it does.

- **Why removal rather than protection.** Masking or encryption would preserve the ability to hold the values, which the product never needs; every protection scheme also adds a decryption or reveal path that can itself be misused. Because account identity already rests on the per-site unique `Description`, removal costs nothing functional and eliminates the data class rather than guarding it.
- **Why the pair was there at all.** It served as a secondary uniqueness key (`IX_Account_Bsb_Number`) and as human recognition in the UI. The per-site unique `Description` index now covers both roles, and the ERD already carried it.
- **Self-exclusion by `Id`, mirroring the expense and income checks.** `CheckDescriptionDoesNotExist` for expenses and incomes already excludes the edited row with `.And(entity => entity.Id != state.ExpenseToUpdate.Id)`, so the account equivalent uses the same shape rather than inventing a variant. `Id` is `EntityBase`'s internal primary key and is already present on the tracked `AccountToUpdate`; `RowId` is the externally exposed identifier and has no role in an internal uniqueness predicate.
- **`AccountSpecifications.IsSameDescription` keeps its single-argument signature.** The expense and income equivalents take a parent `accountId` because their uniqueness is per-account, which the site-level query filter does not cover. Account uniqueness is per-site and `PotDbContext.SetupQueryFilters` already filters `AccountEntity` by `GetCurrentUserSiteId()`, so a site or account parameter would be redundant.
- **The global-uniqueness check disappears with the pair.** `AccountExistsAsync(bsb, number)` deliberately called `IgnoreQueryFilters()` because account numbers were globally unique, unlike the per-site description index. No replacement global check is needed: the description constraint is intentionally site-scoped.
- **Rejected: BSB plus last-3/4 digits.** Considered as a middle ground. Rejected because the residual value has no use in a projection tool, and the partial value would perpetuate the idea that bank details belong in the product.
- **Rejected: keep the columns but stop reading them.** Leaves the plaintext data at rest, which is the primary risk, while creating an orphaned schema element.
- **v3 is supported on import, and the asymmetry with v1 is the reason.** The v1 rejection exists because v2 *added* `AccrualPolicy`, so importing v1 would mean inventing semantics for a field the user never set. v3 → v4 only *removes* columns whose values this change deliberately discards, so nothing is guessed and no correctness is traded away. Supporting v3 therefore costs a mapping change, not a semantic assumption.
- **Name-based mapping is a prerequisite for the window, not a tidy-up.** With positional rows on one side and named columns on the other, the column set was the only thing holding the format together. Making both sides name-based allows the column set to be a superset in one direction and a subset in the other, which is what lets one row type serve both v3 and v4.
- **Rejected: version-specific row types (`AccountCsvRowV3` plus `AccountCsvRowV4`).** Keeps the positional coupling, duplicates a row type per supported version, and needs a captured package per version as a test asset. Name-based mapping reads every supported version with one row type and can be tested by reordering or adding columns in a fixture.
- **Rejected: writing `Bsb`/`Number` as empty placeholder columns.** Would hold the format stable while leaving the coupling in place, and would leave the export appearing to model fields the product no longer has.
- **`MinimumSupportedVersion` is a deliberate constant, not a derived value.** A window is a standing commitment to keep reading those shapes, so lowering or raising the floor should be a conscious decision rather than a side effect of adding a version.
- **Single-release sequencing.** Server and client ship together in one release, so the contract break is atomic. A staged rollout was rejected because the de-expose step is the security win and splitting it from the column drop would leave the plaintext data in place for the duration of the delay.
- **The duplicate-check error payload needs no separate fix.** That check echoed the full BSB and account number into an API error message; deleting the check removes the leak rather than redacting it.
