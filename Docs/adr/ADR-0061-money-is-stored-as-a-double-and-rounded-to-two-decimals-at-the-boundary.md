---
id: ADR-0061
title: Money is stored as a double and rounded to two decimals at the boundary
status: proposed
date: 2026-11-16
area: cross-cutting
related-prds: [PRD-0037]
---

# ADR-0061: Money is stored as a double and rounded to two decimals at the boundary

**Summary:** In the context of a single-currency product that stores money as double precision and
publishes it as a JSON number, facing figures that are a fraction of a cent away from what a person
expects to read, we decided to keep the stored type and round every monetary value to two decimals at the
boundary at which it is produced, to achieve two-decimal money everywhere it is read, accepting that the
arithmetic underneath remains binary and that the stored type would have to change before multi-currency
or compounding.

## Context and Problem Statement

Money is `double` throughout: `AccountEntity.Balance` and `Reserved`, `ExpenseEntity.Amount`,
`IncomeEntity.Amount`, every input and output model, the CSV import rows and the export metadata. The
contract publishes it as a JSON number with `format: double`, and the client types it as a number and
formats it with `Intl.NumberFormat`.

Rounding exists in one place only: each expense cycle's accrued amount, rounded to two decimals away from
zero. Everything around it is unrounded — an account's totals are summed from those results, the daily
accrual rate is a raw division, the available balance is `Balance - Reserved - TotalCommitted`, and the
projection loop carries its running balance between days at full precision.

Two consequences are observable. A remark on the accrual calculator states that monetary results are
rounded to two decimals away from zero, which is true of the accrued amount and of nothing else, so the
code claims a guarantee its callers cannot rely on. And an available balance that lands a fraction below
zero renders as `-$0.00`, because the client formats a negative value with two decimals.

Binary floating point drifts when many amounts are summed, but the drift has no reachable wrong answer
today: no monetary value is compared against zero, the export writes stored values rather than sums, and
the one division already rounds.

## Decision Drivers

- A figure a person reads should be the figure the product means, to the cent.
- The cost of representing money exactly is dominated by the client, where no decimal type exists in the
  language.
- The data set is small, so the decision can be revisited cheaply if a condition changes.

## Considered Options

1. Keep `double`, and round every published monetary value to two decimals at the boundary (chosen).
2. Convert to `decimal` end to end: `decimal` arithmetic on the server, `numeric(18,2)` storage, and a
   wire and client that carry money exactly.
3. Store integer cents as `long`.
4. Leave the values unrounded, and correct the remark so the code stops overstating what it does.

## Decision Outcome

Chosen option: 1. The rules it imposes:

- Every monetary value published is rounded to two decimals away from zero at the boundary.
- The rounding applies to the decimal the value represents, not to its binary approximation. A `double`
  holding `1.005` actually holds `1.0049999999999999`, which is below the midpoint, so no midpoint mode
  returns `1.01`; the value is converted to a decimal representation for the rounding step.
- Derived figures are computed from rounded components, so the figures a reader sees reconcile.
- A projection iteration rounds its values and carries the rounded values forward, so a shorter forecast
  cannot disagree with a longer one.
- Monetary values are normalised to two decimals when they are written, so stored data is two decimals
  regardless of the path that wrote it.

### Consequences

- Good, because no published figure can show a fraction of a cent, and the `-$0.00` artefact disappears.
- Good, because the accrual calculator's remark becomes true rather than needing to be narrowed.
- Good, because no schema, contract or stored type changes, so the change is confined to the calculators
  and one client helper.
- Bad, because the arithmetic underneath is still binary, so a sum of many values is still only as exact
  as `double` allows.
- Bad, because `double` cannot hold every integer beyond 2^53. Monetary values are far below that, which
  is a property of the current product rather than of the type.
- Bad, because JavaScript has no decimal type, so the client rounds its own totals rather than computing
  them exactly.
- Bad, because a value that is not finite has no two-decimal form and cannot be rounded. Converting it to
  a decimal throws, so a non-finite result passes through unrounded: the rounding step must not be the
  thing that fails on malformed input. The product can still publish such a value, which is a defect of
  whatever produced it rather than of the rounding.

This decision would be reopened by any of: multi-currency with exchange-rate multiplication chains;
interest or compounding; reconciliation against a bank statement to the cent; regulatory reporting; or one
observed cent-level mismatch. The first of those is the one to watch, because per-site currency is already
planned.

### Confirmation

- Test: the accrual calculator fixtures assert two-decimal values, including a value at a decimal
  midpoint that a `double` would round the other way.
- Test: a projection fixture asserts that the figures for a given day are identical in a short and a long
  forecast.
- Review: a new monetary output is rounded at the boundary that produces it, and a derived figure is
  computed from rounded components.

## Pros and Cons of the Options

### Rounding at the boundary (chosen)

- Good: two-decimal money everywhere it is read, with no schema change and no contract change.
- Bad: exactness is presentational. Arithmetic remains binary, so the rule has to be applied at every
  boundary rather than being a property of the type.

### Decimal end to end

- Good: exact storage and exact arithmetic, which removes the class of defect rather than the symptom.
- Bad: the client has no decimal type, so a real conversion needs a decimal library at every aggregate
  site or a string wire format for every money property. Converting the database while leaving the wire a
  JSON number would be the worst combination: full migration cost, and the client exactly as imprecise as
  it is today.

### Integer cents

- Good: exact, cheap arithmetic, and no rounding rules at all.
- Bad: a breaking contract change, a migration over live data, and a conversion at every client surface
  that displays money.

### Leaving the values unrounded

- Good: nothing to implement.
- Bad: the `-$0.00` artefact stays, and the remark on the accrual calculator keeps claiming a guarantee
  the code does not provide.

## More Information

- The requirements, the client half of the work and the manual test plan are in
  [PRD-0037](../prd/PRD-0037-money-rounding-at-boundaries.md).
