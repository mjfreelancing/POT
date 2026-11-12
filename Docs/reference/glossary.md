# Glossary

Terms with a precise meaning in POT. If a page needs one of these defined, it links here.

## Money and obligation

| Term          | Meaning                                                                                                                                                                    |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Balance**   | The account's current balance. A payment reduces it on the day the payment is assumed; the accrual figures are not taken out of it.                                          |
| **Reserved**  | An amount the user sets aside per account and does not intend to spend. It never leaves `Balance`; it is excluded from `Available`.                                          |
| **Accrued**   | The amount set aside for the expense cycles currently in progress. An expense due today accrues its full amount, so a bill due today is accrued and is not arrears.            |
| **Arrears**   | One billed amount for every occurrence whose due date has passed without being settled. It is held for every day of the projection window, and it is not deducted from the forecast `Balance`, so it lowers `Available` only. |
| **Committed** | `Accrued` plus `Arrears` - what the account balance is committed to cover.                                                                                                    |
| **Available** | On the accounts read, `Balance - Reserved - Committed`. See [Cashflow model](../explanation/cashflow-model.md) for how the projection chart composes the same idea day by day. |

## Accrual metrics

| Term              | Meaning                                                                                                              |
| ----------------- | ---------------------------------------------------------------------------------------------------------------------- |
| **Daily Accrual** | The dynamic, operational metric: what each day of the current cycle needs. It varies during a cycle by design.          |
| **Daily Need**    | The stable metric: the long-run daily funding figure for an account, which does not swing with where a cycle sits.       |
| **Accrual policy**| Per expense. `Automatic` contributes to accrual; `None` keeps the due-date schedule and the balance debits but contributes nothing to accrual. |

## Accounts and money movement

| Term           | Meaning                                                                                     |
| -------------- | --------------------------------------------------------------------------------------------- |
| **Income**     | Money expected or received on a schedule. It is added to `Balance` on its due date.            |
| **Expense**    | Money owed on a schedule. It is subtracted from `Balance` on its due date.                     |
| **Renewal**    | Moving a recurring item's `NextDue` to its next occurrence. It changes the schedule only.      |

See [Renewal Calculators](../../Source/Server/IMPLEMENTATION.md#renewal-calculators) for the renewal rules and
[Ports and environments](ports-and-environments.md) for the runtime facts.
