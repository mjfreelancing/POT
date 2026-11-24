# Money Rounding at Boundaries PRD

**Status:** Complete
**Priority:** Medium
**Last Updated:** 2026-11-18
**Feature ID:** 0037
**Discovery:** complete
**Scope:** Round every monetary value the product publishes to two decimal places, so a figure a person
reads is a two-decimal figure, and the accrual calculator's claim that monetary results are rounded
becomes true. Covers the server calculators (account totals, the daily accrual rate and the derived
available balance), the projection loop and the values it carries between days, the maintenance CSV
export, the storage of monetary input, and the totals the client adds up for itself. It does not cover the
stored type: money stays a `double` and the contract stays a JSON number with `format: double`. Converting
to decimal or to integer cents is out of scope, with the conditions that would reopen it recorded in
ADR-0061. Per-site currency belongs to its own track.

## Problem Statement

Monetary values are rounded in exactly one place: each expense cycle's accrued amount, rounded to two
decimal places away from zero. Everything around it is unrounded arithmetic on `double`.

- An account's totals are summed from those per-expense results without rounding.
- The daily accrual rate is a raw division.
- The derived available balance is `Balance - Reserved - TotalCommitted`, computed unrounded.
- The projection loop carries its running balance and today's arrears between days at full precision.
- The client adds money up itself — the dashboard sums four values across accounts, and the projection
  detail panels and chart data sum again.

Two defects follow.

The **documentation** defect: the accrual calculator's remark states that monetary results are rounded to
two decimal places away from zero. That is true of the accrued amount and of nothing else, so the code
claims a guarantee its callers cannot rely on. The claim is why nobody has looked closer.

The **display** defect: an available balance that lands a fraction of a cent below zero renders as
`-$0.00`. Verified: `Intl.NumberFormat` with the AUD currency renders `-0` and `-1e-10` as `-$0.00`. The
same artefact is reachable in the client's own totals, which no server-side change can prevent, because
those sums happen after the response is received.

Rounding at the boundary removes both, and makes a total mean what two-decimal money means, without
changing the stored type or the contract.

## Terms

- **Boundary** — the last point at which a monetary value is computed before it is published or stored.
  Rounding happens there, not at every intermediate step.
- **Monetary value** — any money amount the product publishes: account totals, the daily and stable
  accrual rates, the available balance, projection figures, exported CSV amounts, and the client's
  aggregates of them.
- **Decimal-correct rounding** — rounding the decimal a person would read (`1.005` → `1.01`) rather than
  the binary value a `double` actually holds (`1.0049999999999999` → `1.00`).

## Functional Requirements

### A. The rounding rule

- **A1.** A monetary value is rounded to two decimal places, away from zero, at the boundary.
- **A2.** The rounding is applied to the decimal the value represents, not to its binary approximation, so
  a value such as `1.005` rounds to `1.01`. No `MidpointRounding` mode achieves this on a `double`; the
  value must be converted to a decimal representation for the rounding step.
- **A3.** The conversion is local to the rounding step. Calculators keep computing in `double`; the
  conversion does not change the type of any input, output or stored value.

### B. What the server rounds

- **B1.** Every monetary value the application computes and publishes is rounded. A value passed through
  from storage is published as stored: a write has already normalised it (D1), so there is no second rule
  to apply on read.
- **B2.** Rounding happens inside the calculators, so every caller receives rounded values and the rule
  can be tested in one place.
- **B3.** The available balance is derived from the rounded components (`Balance`, `Reserved`,
  `TotalCommitted`), so the figures a person reads on an account always reconcile.
- **B4.** The accrual calculator's remark accurately describes what is rounded.

### C. Projections

- **C1.** Every value published for a projection day is rounded.
- **C2.** The rounded values are fed back into the loop state, so a running balance carried to the next
  day is the rounded figure rather than the full-precision one.
- **C3.** A forecast of N days presents, for each of those days, exactly the figures those days present in
  any longer forecast for the same start date.

### D. What is stored

- **D1.** A monetary value is normalised to two decimals when it is written, in the application layer, so
  the API and the maintenance import both store two decimals through the same rule.
- **D2.** Normalising rounds rather than rejects: a value carrying more decimals is stored as the rounded
  value, not refused.
- **D3.** No schema change, no data migration, and no change to what is stored for a value that already
  has two decimals.

### E. The client

- **E1.** A total the client computes itself is rounded to two decimals before it is displayed.
- **E2.** Rounding alone does not remove the artefact: a rounded negative total can still be negative
  zero, which formats as `-$0.00`. A displayed total whose rounded value is zero is shown as zero, never
  as a negative zero.
- **E3.** The rounding and the negative-zero normalisation live in one shared helper that every display
  site uses.
- **E4.** The projection chart's aggregated series is rounded on the same rule.

### F. Maintenance export

- **F1.** The CSV export publishes rounded monetary values. Because stored values are normalised on write
  (D1), this is expected to change nothing for data created after this feature.

## Requirement Coverage

Delivery replaces each row with the test that proves it, adding whatever tests are needed to complete the
coverage. Every row must name a test before this document closes: the approval of this PRD is conditional
on that, not on the tests that happen to exist today.

| Requirement | Verified by                                                                                                     |
| ----------- | --------------------------------------------------------------------------------------------------------------- |
| A1, A2      | `MonetaryRoundingFixture` — two decimals away from zero, and a decimal midpoint a `double` rounds the other way |
| A3          | The solution builds in Release with no signature changed                                                        |
| B1, B2      | `AccrualCalculatorFixture` — every published figure is two decimals, including each row's arrears               |
| B3          | `Get/AccountOutputFixture`, `GetAll/AccountOutputFixture`, and the hosted `AccrualDerivationFixture`            |
| B4          | The accrual calculator's remark, reviewed against the implementation                                            |
| C1, C2      | `ProjectionsServiceFixture` — every day's figures are two decimals, and the balance carries forward             |
| C3          | `ProjectionsServiceFixture` — a short forecast and a long one agree on every day they share                     |
| D1, D2      | The six account, expense and income write fixtures — writing `10.005` stores `10.01`                            |
| D3          | The migration set is unchanged                                                                                  |
| E1, E2, E3  | `tests/lib/moneyUtils.test.ts`, `AccountsOverview.test.tsx`, and the dashboard browser case                     |
| E4          | `useProjectionChartData.test.ts` — the series and the total round to two decimals                               |
| F1          | `ExportImportRoundTripFixture` — an imported three-decimal amount exports as two                                |

## Out of Scope

- **Converting stored money to `decimal` or `numeric`** — ADR-0061 records the decision and the five
  conditions that would reopen it (multi-currency with exchange-rate chains, interest or compounding,
  reconciliation to the cent, regulatory reporting, an observed cent-level mismatch).
- **Integer cents** — the same record considers and declines it; it would break the contract and the
  client.
- **The contract's `format`** — money stays a JSON number with `format: double`.
- **Rejecting an over-precise import** — money is normalised rather than refused (D2).
- **Client-side decimal arithmetic** — the client rounds its totals; it does not gain a decimal type.
- **Per-site currency** — its own track.

## Known Risks and Accepted Limitations

- **Precision above 2^53.** A `double` cannot hold every integer beyond 2^53. Monetary values are
  two-decimal amounts far below that, so the limit is unreachable, but it is the limit that makes
  `double` unsuitable if balances ever grow by orders of magnitude.
- **Two different expressions can still round differently.** Rounding makes each figure a two-decimal
  figure; it does not make two different formulas agree.
- **Client aggregates over many rows.** Rounding a total removes the negative-zero artefact but cannot
  make a JavaScript sum exact. With many accounts, a total can still differ from the arithmetically exact
  sum in the last cent.
- **Existing data is assumed to be two decimals.** Nothing has ever validated that. A stored amount that
  carried more decimals is published and exported as stored until the row is next written, which is the
  write that normalises it.

## Assumptions

- No stored monetary value currently carries more than two decimals.
- The application and the maintenance import are the only ways monetary values are written.
- One currency is used; no monetary value is an exchange-rate product.

## Decisions Carried Into This Document

| Decision                               | Resolution                                                                                                | ADR      |
| -------------------------------------- | --------------------------------------------------------------------------------------------------------- | -------- |
| Stored type and where rounding happens | Keep `double`; round at the boundary. Decimal and integer cents declined; five reopen conditions recorded | ADR-0061 |
| Rounding correctness                   | Round the decimal representation, not the binary value                                                    | ADR-0061 |
| Which values are rounded               | Every monetary value published, including pass-throughs and the client's own totals                       | —        |
| Where rounding happens                 | Inside the calculators                                                                                    | —        |
| Derived figures                        | Computed from rounded components, so a card's figures reconcile                                           | —        |
| Projection iterations                  | Round each day and carry the rounded values forward                                                       | —        |
| Negative results                       | No clamping; a genuinely negative balance stays negative                                                  | —        |
| Negative zero                          | Normalised in the client helper, so a zero total never renders as `-$0.00`                                | —        |
| Stored precision                       | Normalise to two decimals on write in the application layer, rather than rejecting input                  | —        |
| Assertion policy                       | Exact equality on rounded values in the affected suites                                                   | —        |
| Chart series                           | Rounded on the same rule as every other aggregate                                                         | —        |

## Effect on Earlier Documents

| Document | What changes                                                                                                                                                                                                           | Amendment |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- |
| PRD-0023 | States that the accrual column values change only through the corrections it describes. Boundary rounding changes them as well, and turns the values it writes as approximate (`≈3.23`) into exact two-decimal figures | AM-1      |

No other Complete document makes a claim this feature makes wrong. `Docs/explanation/cashflow-model.md`
and `Docs/standards/server-code.md` describe the model and the coding rules without asserting anything
about monetary precision.

## Manual Test Plan

**Not required** — every behaviour this plan would have checked is asserted by an automated suite, and a
manual pass would repeat those assertions against the same code:

- the rounding rule, a value at a decimal midpoint, and a figure derived from rounded components —
  `Pot.App.Tests/Calculators/AccrualCalculatorFixture.cs`;
- a short forecast against a longer one for the same day —
  `Pot.App.Tests/Features/Projections/ProjectionsServiceFixture.cs`;
- a three-decimal amount stored as two. The import takes rows rather than a file, so a fixture supplies
  the row directly and nothing about file reading needs exercising —
  `Pot.App.Tests/Features/Maintenance/ExportImportRoundTripFixture.cs`;
- a rounded total, including the negative-zero case — `tests/lib/moneyUtils.test.ts`, and
  `tests/features/dashboard/AccountsOverview.test.tsx` for the dashboard's own total;
- a zero total rendering as `$0.00` rather than `-$0.00` in a real browser —
  `dashboard renders an account total that sums to nothing as $0.00` in
  `e2e/tests/rendering/dashboardRendering.test.ts`.

That last one is the only behaviour a unit test cannot fully prove, because the formatting is the
browser's `Intl`. It belongs in the end-to-end suite rather than in a manual pass, where it would be
checked once and never again.

This plan is `Not required` only while every row of Requirement Coverage names a test. A row still
unnamed at close-out means a behaviour is unproven, and the manual plan returns.

## Amendments

Append-only, oldest first. The text above is this document as approved; read it through these entries.

- **AM-1 (2026-11-18, Clarified) — what is rounded on read (B1).** B1 asked for every published value to be
  rounded, including values passed through from storage. Delivery showed those values are copied from the
  entity unchanged in the accounts and projection mappings, so rounding them would add a second rule at
  every read for data a write already normalises. B1 now covers the values the application computes, and
  states that a stored value is published as stored because the write path normalises it (D1). The risk of
  pre-existing data carrying more decimals is recorded against that reading.

## Verification Evidence

Each behaviour was written as a failing test first and then made to pass, so the table records what the suite
said before the change and a later reader can repeat the check.

### Red before green

| Requirement | Test                                                      | What it said before the change                                                    |
| ----------- | --------------------------------------------------------- | --------------------------------------------------------------------------------- |
| A1, A2      | `MonetaryRoundingFixture`                                 | `1.005` rounded to `1.00` without the conversion to a decimal                     |
| A2          | `AccrualCalculatorFixture` — midpoint value               | an accrued amount whose exact value is a midpoint landed on the wrong cent        |
| B1          | `AccrualCalculatorFixture` — per-row arrears              | `300.29999999999995` for three past-due occurrences of `100.10`                   |
| B1          | `MonetaryRoundingFixture` — negative zero                 | a value that rounds to nothing came back as `-0`, which serialises as `-0`        |
| B3          | `Get/AccountOutputFixture`, `GetAll/AccountOutputFixture` | `0.09999999999999998` for `0.3 - 0.1 - 0.1`                                       |
| B3          | `AccrualDerivationFixture` (hosted)                       | the same figure on the wire, because the response recomputed it                   |
| C1          | `ProjectionsServiceFixture` — every day two decimals      | a balance of `2366.6800000000003`                                                 |
| C2          | `ProjectionsServiceFixture` — the carry                   | `998.9999999999998` after ten payments of `0.10`                                  |
| C3          | `ProjectionsServiceFixture` — short against long          | passed already: the loop walks forward, so this is a guard rather than a red test |
| D1, D2      | the six account, expense and income write fixtures        | `10.005` was stored as `10.005`                                                   |
| F1          | `ExportImportRoundTripFixture`                            | an imported `10.005` was exported as `10.005`                                     |
| E1, E2      | `moneyUtils.test.ts`, `AccountsOverview.test.tsx`         | `-$0.00` for a total that rounds to nothing                                       |
| E4          | `useProjectionChartData.test.ts`                          | the plotted series and the total carried a negative zero                          |

The browser case is proven twice over: it renders `$0.00` where the raw total is `-2.78e-17`, and its
`collectApiFailures` assertion was made to fail by putting a non-GUID id back into the payload, which the API
refused with a `400` that every other assertion in the test ignored.

### Suites

| Suite                                 | Result                                                            |
| ------------------------------------- | ----------------------------------------------------------------- |
| `dotnet build pot.sln -c Release`     | 0 warnings, 0 errors                                              |
| Server unit and hosted suites         | 1296 passed, 0 failed (296 hosted, 9m29s)                         |
| `npm run build`                       | built, 62 precache entries                                        |
| Client unit suite                     | 996 passed in 174 files                                           |
| Prodlike E2E matrix, four projects    | 322 passed, 0 failed (7m36s)                                      |
| Server coverage (`code_coverage.ps1`) | line 90.6%, branch 78.4%; every class this change touched at 100% |

The branches those last two tests close were uncovered before this change — the importer's row id in
`CreateAccountService` and the not-found path in `UpdateAccountService` — so the change leaves no class it
touched with a gap.

### Deliberately broken, one at a time

Four faults were injected one at a time into the finished change, each run against the fixtures that name it,
then reverted so that the next run started from the code as delivered. The purpose is to show that a test
fails for the reason claimed: a test that still passes with the fix removed was not checking the fix.

| Fault introduced                                                                                                                 | What the suite answered                                                                                                                                                                                                         | Verdict                                                                                                                       |
| -------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| `MonetaryRounding.Round` returns its argument unchanged — the change reverted at the one place every server value passes through | 31 failed, 114 passed of 145: `1.005` stayed `1.005`; three arrears of `100.10` summed to `300.29999999999995`; a projected day read `46.66666666666667`; a value that rounds to nothing came back as `-2.7755575615628914E-17` | caught — every requirement that names a rounded figure failed                                                                 |
| `Round` calls `Math.Round` on the `double` and keeps the result, so the conversion to `decimal` is gone                          | `Round(1.005)` returned `1`; the accrual midpoint case read `1` where `1.01` was expected                                                                                                                                       | caught — the conversion is load-bearing: the `double` holding `1.005` actually holds `1.0049999999999999`, below the midpoint |
| `Round` keeps the conversion to `decimal` and drops only the negative-zero guard                                                 | `Should_Publish_Zero_For_A_Negative_Value_That_Rounds_To_Nothing` failed on `double.IsNegative(rounded)`: expected `False`, got `True`                                                                                          | caught — the guard is a separate decision from the rounding, and its own test holds it                                        |
| client `roundMoneyValue` drops only its zero guard                                                                               | `roundMoneyValue(0.3 - 0.1 - 0.2)` returned `-0`, and `formatMoneyValue` rendered `-$0.00`                                                                                                                                      | caught on the client, independently of the server                                                                             |

The last one is why the guard exists twice. The server can be made to send a plain `0`, and the browser still
computes totals of its own from the payload, so it can arrive at a negative zero by itself.

### What the evidence cannot show

- The faults were injected after the change was complete and the suites green, not while it was being written.
  Each was reverted and the working tree checked against the committed content before the next was applied, but
  one person ran the loop once; no second person repeated it.
- The browser case is proven by its own assertion, not by a fault. Re-running the four-project E2E matrix once per
  fault would have cost roughly eight minutes each, so the client fault was confirmed against the unit test
  alone. What the browser renders for a negative zero rests on the E2E case that was already run against the
  finished change.
- `decimal` has no negative zero, so a value arriving from the database can never produce one; every test of the
  guard constructs a computed `double` in memory. Nothing proves a stored value cannot carry the sign, only that
  the type cannot.
- Values stored before this change with more than two decimals are still published as stored (AM-1). The
  evidence covers the code, not the rows an existing database already holds.
- That every published money value passes through the helper is established by the call-site inventory, not by
  the faults. A path that never reaches the helper fails no fault, so a path added later without rounding would
  be caught only by its own test.
- Rounding adds a decimal conversion to each call. Nothing measures what that costs, and at the row counts a
  projection carries it was not judged worth measuring.
