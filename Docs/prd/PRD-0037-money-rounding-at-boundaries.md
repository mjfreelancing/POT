# Money Rounding at Boundaries PRD

**Status:** Planned
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

| Requirement | Verified by                                                                                             |
| ----------- | ------------------------------------------------------------------------------------------------------- |
| A1, A2      | `Pot.App.Tests/Calculators/AccrualCalculatorFixture.cs`, including a value at a decimal midpoint        |
| A3          | The server build — the conversion changes no signature                                                  |
| B1, B2      | `AccrualCalculatorFixture`, and the accounts read path in the hosted suite                              |
| B3          | `AccrualCalculatorFixture` — the published figures reconcile                                            |
| B4          | Review of the remark against the implementation                                                         |
| C1, C2, C3  | `Pot.App.Tests/Features/Projections/ProjectionsServiceFixture.cs`, a short forecast against a long one  |
| D1, D2      | `Pot.App.Tests/Features/Maintenance/ExportImportRoundTripFixture.cs`, a three-decimal row stored as two |
| D3          | The migration set is unchanged                                                                          |
| E1, E2, E3  | `tests/lib/moneyUtils.test.ts` — rounding and negative zero                                             |
| E4          | The chart data hook's unit test                                                                         |
| F1          | `ExportImportRoundTripFixture`                                                                          |

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
- a rounded total, including the negative-zero case — `tests/lib/moneyUtils.test.ts`;
- a zero total rendering as `$0.00` rather than `-$0.00` in a real browser — a case added to
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

Filled in by the delivery skill at close-out, from the stage plan.

### Deliberately broken, one at a time

| Fault introduced | What the suite answered | Verdict     |
| ---------------- | ----------------------- | ----------- |
| Not yet run      | Not yet run             | Not yet run |

### Measured baselines

| Measure     | Value       | Date        | Conditions  |
| ----------- | ----------- | ----------- | ----------- |
| Not yet run | Not yet run | Not yet run | Not yet run |

### What the evidence cannot show

- Not yet run.
