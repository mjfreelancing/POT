# Remove Account BSB and Account Number PRD

**Status:** Complete
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

`accounts.csv` loses two columns, so the package format advances to `MetadataV4` (`MetadataV1` → `V2` → `V3` → `V4`). The importer continues to accept **only the current version**, exactly as it does today.

#### Why the version bump is required

`CurrentVersion` must become `4`. Removing the columns without bumping would leave two incompatible `accounts.csv` shapes both claiming to be v3, and the importer would accept either and misread one of them. The bump is what makes a superseded package fail cleanly at the existing gate instead of importing wrongly, and it is why a stale export has to be re-exported after upgrading.

`ImportDataService`'s `metadataVersion != MetadataBase.CurrentVersion` check is unchanged; no minimum-version constant is introduced.

#### Row mapping

`AccountCsvRow` maps columns by position (`[Index(n)]`), so removing the two columns re-indexes every subsequent property rather than deleting two entries. `ExpenseCsvRow` and `IncomeCsvRow` are untouched. Only the current version is ever read, so the positional reader never sees a superseded column set, and the package has a single producer — the server writes both the CSV entries and the binary metadata — so the exporter and importer are always released in step.

#### Metadata

The metadata payload is an `int32` version followed by a `DateTime`, identical for every version; the version-specific readers differ only in the type they construct, and `ReadMetadataVersion()` reads the version straight from the stream. `MetadataV4`, `MetadataV4Reader`, and `MetadataV4Writer` are therefore added for the version constant and the column documentation only.

- `MetadataBase.CurrentVersion` becomes `4`; `MetadataReaderFactory` and `MetadataWriterFactory` produce the V4 reader/writer; `ExportDataService` emits `MetadataV4`.
- `ImportDataService` logs the version read from the package rather than `metadata.Version`, which is `MetadataV4`'s type constant and says nothing about the file.
- `AccountData`, `AccountsExporter`, `IAccountCsvRow`, `AccountCsvRow`, and `AccountsImporter` drop both fields.

### Reference artifacts

- `Docs/pot_erd.d2`: remove the `Bsb` and `Number` lines from `finance.Account`.
- `Data/Postman/POT.postman_collection.json`: remove `bsb`/`number` from the account create and update request bodies.
- `Docs/USER-GUIDE/Accounts.md` already describes account creation as "description, balance, and optional reserved amount" and requires no change.

## Out of Scope

- Encryption, tokenisation, or column-level protection. Removal supersedes the need.
- Masked storage or masked display variants (BSB plus last-3/4 digits) — rejected in favour of removal.
- Audit logging of bank-detail access, because no bank details remain to audit.
- Existing plaintext copies on disk (`Data/Exports`, `Data/Backups`, `Data/ProdLike Backups`, `Data/Azure/Backups`, `Data/Azure/Exports`). Their disposition is the repository owner's concern; this PRD neither inventories nor purges them.
- The maintenance import/export transport, endpoints, permissions, and version gate. The CSV column set changes, which re-indexes `AccountCsvRow`.
- `Projections` and `Accruals` behaviour: both already resolve accounts by `AccountRowId`.

## Tests

Coverage is removal-shaped: delete the tests that assert the removed behaviour, and re-point the fixtures that used the pair as a unique key.

- **Data** — `Pot.Data.Tests/Specifications/AccountSpecificationsFixture.cs`: delete the `IsSameBsbNumber` fixture; `IsSameDescription` coverage remains.
- **Application** — delete the `CheckAccountNumberDoesNotExist` fixtures under `Pot.App.Tests/Features/Accounts/{Create,Update}/EntityChecks/Checks/`; update `Checks/CheckDescriptionDoesNotExistFixture.cs` for the `Id`-based self-exclusion, including a case asserting an account may keep its own description unchanged; remove `Bsb`/`Number` from the account construction in `Features/Expenses/{Create,Renew,ToggleExclude,Update}/*ServiceFixture.cs` and `Features/Projections/ProjectionsServiceFixture.cs`; update `Pot.TestUtils/EntityFactory.cs` consumers.
- **Integration** — `Pot.AspNetCore.Integration.Tests` currently has no accounts fixtures (its coverage is auth, health, pipeline and security), so there is nothing to update. The API contract change is covered by the account service and entity-check unit tests plus the E2E create/edit/delete lifecycle, which drives the real endpoints. A dedicated accounts integration fixture remains a pre-existing gap, not one this change introduces.
- **Client unit** — delete `tests/components/input/BsbInput.test.tsx`; drop the BSB cases from `tests/features/accounts/schemas/accountFormSchema.test.ts`; update `tests/data/account.test.ts`, `tests/features/dashboard/components/AccountCard.test.tsx`, `tests/shared/factories/accountFactory.ts`, and `tests/integration/CoreFlows.CreateAccountFlow.test.tsx` (including deletion of the BSB-masking test).
- **E2E** — `e2e/tests/crud/accountsCrud.test.ts` stops filling the BSB field; the BSB format-validation test in `e2e/tests/feedback/formValidation.test.ts` is deleted; the `BSB:` card assertion in `e2e/tests/mobile/mobileCardGrids.test.ts` is replaced with a description-based assertion; `e2e/helpers/api.ts` switches its per-call uniqueness from the `(Bsb, Number)` pair to a unique description per site, since that index comment documents the collision-avoidance this removal invalidates.
- **Migration** — verify the drop applies cleanly against a database at the previous revision, and that the unique per-site description index continues to reject duplicates.
- **Maintenance round-trip and version gate** — the maintenance path previously had no test coverage, so this is new ground. Two fixtures guard it:
  - `Pot.App.Tests/Features/Maintenance/ExportImportRoundTripFixture.cs` — runs the real `AccountsExporter` output back through the real `AccountCsvRow` mapping, so every `accounts.csv` column must land on its intended property. This is the regression guard for the re-index: the exporter writes by name while the importer reads by position, so a missed re-index moves values between fields rather than failing. It also asserts the version written into the package equals `MetadataBase.CurrentVersion`, which is the integer the gate compares.
  - `Pot.App.Tests/Features/Maintenance/ImportVersionGateFixture.cs` — a package whose version is not current (1, 2, 3 and 5) is refused with the existing version error, the metadata is never deserialised, and no rows are read or written.

  The expenses and incomes CSVs are deliberately not round-tripped: their columns are unchanged by this work, so the misalignment risk the test exists for is specific to accounts.

## Validation

Implemented as two reviewable increments — client first, then the breaking server change — each gated and validated on 2026-09-24. Nothing was deployed between them.

- **Server** — `Pot.Shared.Tests` 27, `Pot.Data.Tests` 72, `Pot.AspNetCore.Tests` 71, `Pot.App.Tests` 439 and `Pot.AspNetCore.Integration.Tests` 28: **637 tests green / 0 failed**. The 27 integration failures seen before the migration existed were all `PendingModelChangesWarning` and cleared once it was generated.
- **Client** — cold `tsc -b --force` exit 0; `npm run lint` 0 problems; unit 802 of 804, the two failures being the known `authInterceptor` load flake that passes 9/9 standalone; `npm run build` exit 0.
- **E2E** — full matrix across chromium, edge, mobile-chrome and mobile-safari: **269 passed / 66 skipped / 0 failed (7.2 m)**, with `test-results/.last-run.json` reporting `status: passed` and an empty `failedTests`. The total sits 4 below the previous baseline of 273 because this PRD deletes one E2E test — the BSB format test — which ran once per project. The skipped count is unchanged at 66, which is the evidence that the `accountsCrud` lifecycle test re-enabled for the server increment is passing again rather than newly skipped.
- **Migration** — the E2E log shows it applying in sequence onto the preceding revision: `DROP INDEX "IX_Account_Bsb_Number"`, `ALTER TABLE "Account" DROP COLUMN "Bsb"`, then `DROP COLUMN "Number"`.
- **Package format** — the v4 conversion is proven by the harness's own import: `Imported 63 records`, `Found 3 account(s)`, `Accrued expenses for all accounts`, `Renewed 6 income(s)` and `Renewed 54 expense(s)`. Reaching that point required converting the committed `e2e/seed/financial.export`, because a version bump invalidates every existing package. A superseded package is still refused cleanly by the gate; the `InvalidCastException` seen during conversion was an artefact of editing the version integer without the embedded type name (see ADR-012 §9).

## Design Notes (Rationale and Rejected Alternatives)

Reference notes for future maintainers, recording why the design looks the way it does.

- **Why removal rather than protection.** Masking or encryption would preserve the ability to hold the values, which the product never needs; every protection scheme also adds a decryption or reveal path that can itself be misused. Because account identity already rests on the per-site unique `Description`, removal costs nothing functional and eliminates the data class rather than guarding it.
- **Why the pair was there at all.** It served as a secondary uniqueness key (`IX_Account_Bsb_Number`) and as human recognition in the UI. The per-site unique `Description` index now covers both roles, and the ERD already carried it.
- **Self-exclusion by `Id`, mirroring the expense and income checks.** `CheckDescriptionDoesNotExist` for expenses and incomes already excludes the edited row with `.And(entity => entity.Id != state.ExpenseToUpdate.Id)`, so the account equivalent uses the same shape rather than inventing a variant. `Id` is `EntityBase`'s internal primary key and is already present on the tracked `AccountToUpdate`; `RowId` is the externally exposed identifier and has no role in an internal uniqueness predicate.
- **`AccountSpecifications.IsSameDescription` keeps its single-argument signature.** The expense and income equivalents take a parent `accountId` because their uniqueness is per-account, which the site-level query filter does not cover. Account uniqueness is per-site and `PotDbContext.SetupQueryFilters` already filters `AccountEntity` by `GetCurrentUserSiteId()`, so a site or account parameter would be redundant.
- **The global-uniqueness check disappears with the pair.** `AccountExistsAsync(bsb, number)` deliberately called `IgnoreQueryFilters()` because account numbers were globally unique, unlike the per-site description index. No replacement global check is needed: the description constraint is intentionally site-scoped.
- **Rejected: BSB plus last-3/4 digits.** Considered as a middle ground. Rejected because the residual value has no use in a projection tool, and the partial value would perpetuate the idea that bank details belong in the product.
- **Rejected: keep the columns but stop reading them.** Leaves the plaintext data at rest, which is the primary risk, while creating an orphaned schema element.
- **Only the current version is ever imported, and the v1 precedent is why.** A version window's floor collapses the moment a release _adds_ a column whose value cannot be safely defaulted, and the v1 rejection records exactly that reasoning — `AccrualPolicy` could have been defaulted, but the user would not review the result, so the import was rejected instead. A window would therefore pay only in the narrowing case, which is this change, at the cost of a permanent compatibility matrix and retained v3 packages as test assets that a later version would retire anyway.
- **Rejected: a v3–v4 version window.** Considered and dropped for the reason above. Under a single supported version a superseded package fails cleanly at the gate, which is the correct outcome for an import the code cannot read faithfully.
- **Rejected: switching the CSV row types to name-based mapping.** Its purpose was to enable the window. It is also not unambiguously safer: a renamed column would leave the mapped property at its default, whereas the positional reader tends to misalign into a `FormatException`. Re-indexing `AccountCsvRow` is the smaller and more predictable change.
- **The positional column-order coupling is accepted.** A package is only ever produced by the server, which writes the CSV entries and the binary metadata together, and only the current version is read; hand-crafting one would mean reproducing the binary metadata entry exactly. There is therefore no independent producer whose column order could disagree with the importer's indices. A mismatch could only come from the exporter and importer being edited out of step within one version, which is a code defect the round-trip test catches rather than a compatibility problem the design has to absorb.
- **Rejected: version-specific row types (`AccountCsvRowV3` plus `AccountCsvRowV4`).** They exist only to serve a window, and there is no window.
- **Rejected: writing `Bsb`/`Number` as empty placeholder columns.** Would hold the format stable while leaving the positional coupling in place, and would leave the export appearing to model fields the product no longer has.
- **No deployment until the whole change is complete.** Server and client therefore reach production together, so the contract break is atomic. The work is nonetheless built as ordered code increments; those are commits, not rollout stages — the client cannot create or update accounts between them, which is acceptable precisely because nothing is exposed until every increment lands. A staged deployment was rejected because the de-expose step is the security win and splitting it from the column drop would leave the plaintext data in place for the duration of the delay.
- **The duplicate-check error payload needs no separate fix.** That check echoed the full BSB and account number into an API error message; deleting the check removes the leak rather than redacting it.
- **A version bump invalidates any committed package, including the E2E seed.** `Source/Client/pot-react/e2e/seed/financial.export` is a committed package the Playwright harness imports to populate the test database, so it had to be converted with this change. A package cannot simply be relabelled: the metadata entry is an `int32` version, a length-prefixed UTF-8 **assembly-qualified type name**, and a `DateTime`, and `MetadataSerializer.Deserialize<T>` constructs the type the payload *names*, whatever `MetadataReaderFactory` returns. A payload naming `MetadataV3` therefore yields a `MetadataV3`, and the cast in `Deserialize<TMetadata>` throws `InvalidCastException` (a 500) rather than tripping the version gate. A genuine stale package still fails cleanly with a 422 — the 500 is only reachable by tampering with the version integer.
- **How to convert a package to a new version.** Rewrite the `metadata` entry with the current serializer (`MetadataSerializer.Serialize(new MetadataV<N> { CreatedAt = … })`) so the version, the type name and its length prefix are all correct for the new version, apply the format delta to the CSV entries, and leave every other entry byte-identical. A same-length byte substitution also works (v3 → v4, both ten characters), but it stops working as soon as the class name changes length: `MetadataV9` → `MetadataV10` is one byte longer and shifts every subsequent offset, so it is not the technique to rely on.
- **Consider recording a different metadata layout later.** The embedded type name carries no information — the entry holds two scalars and names the type only because `WriteObject`/`ReadObject` writes the concrete type. Writing the version and timestamp directly would make the metadata entry identical for every version, remove the need for version-specific readers and writers, and reduce converting a package to a one-byte change. That is itself a package format change (so it would be `MetadataV5`) and is out of scope here.
