# Cashflow model

How POT turns a set of scheduled expenses and income into the numbers a user sees: what is committed,
what is available, and what a projection says the balance will be. The definitions of the individual
terms are in the [glossary](../reference/glossary.md).

## Two accrual metrics

An expense does not simply cost its amount on its due date. Between cycles the user is expected to be
putting money aside, and POT measures that with two different figures:

| Metric          | Code                    | Behaviour                                                                 |
| --------------- | ----------------------- | ------------------------------------------------------------------------- |
| Daily Accrual   | `DailyExpenseAccrual`   | The operational figure: what each remaining day of the current cycle needs. It varies as the cycle progresses. |
| Daily Need      | `StableExpenseAccrual`  | The long-run figure: an average daily funding level that does not swing with where a cycle sits.        |

Both are computed on the server by the accrual calculators, so the API and the client can never disagree
about them.

## The accrual cycle

An expense accrues from `AccrualStart` towards `NextDue`. The daily rate is the amount spread over the
days in the cycle, so the accumulated figure rises through the cycle and reaches the full amount on the
due date. Two consequences are deliberate:

- **A bill due today is accrued in full and is not arrears.** It is the cycle in progress, not a missed
  payment.
- **Renewal moves the cursor, not the figures.** On the due date the payment is counted against the
  balance, the schedule advances, and the next cycle starts accruing from the day it was paid.

`AccrualPolicy` decides whether an expense takes part at all. `Automatic` accrues; `None` keeps the
schedule and the balance debits but contributes nothing to accrual, which is what a bill the user does not
budget for looks like.

## Arrears

When a due date passes without the occurrence being settled, the shortfall does not stay in the accrual
figure: it becomes arrears, one billed amount per unsettled occurrence. Arrears behaves differently from
the cycle in progress on purpose - it is held for every day of the window rather than draining as the
cycle advances, and it is not deducted from the forecast balance, so it reduces what is available without
pretending the money has already left the account.

## Available funds

Two reads answer "how much can I spend", and they compose the same idea over different horizons.

**Accounts** (the current picture):

```text
Available = Balance - Reserved - Committed
Committed = Accrued + Arrears
```

**The projection chart** (day by day), where the forecast also nets off the part of the day's accrual that
the day's assumed payments already settle:

```text
balance - reserved - unpaidAccrual - arrears
```

`unpaidAccrual` is `Accrued` less the accrual the day's payments settle. That subtraction is why the
projection publishes components rather than a single derived available figure, and why the numbers can
differ slightly from the accounts columns: the chart assumes the window's payments happen on their due
dates.

## How a projection is produced

A projection is a day-by-day simulation, not a stored result:

1. Every account's rows are copied into an in-memory cursor per row, seeded from the persisted schedule.
2. For each day, the rows due on that date are measured against the cursor as it stands at the start of
   the day - so the payment lands on its own date.
3. The schedule is then folded forward for every row due on or before that date.
4. Balances, reserved, arrears and the accrual figures are recorded for the day.

Nothing about the walk is persisted; the schedules are only advanced by an explicit renew. That is what
keeps a projection reproducible and what keeps the stored schedule a decision the user has made rather
than a side effect of viewing a chart.

## Where the rules live

- The accrual arithmetic: `Pot.App/Calculators`.
- The renewal rules: [Renewal Calculators](../../Source/Server/IMPLEMENTATION.md#renewal-calculators).
- The projection walk: `Pot.App/Features/Projections`.
