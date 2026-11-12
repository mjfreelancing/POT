# POT User Guide

This guide provides detailed instructions for using each feature in POT (Pay On Time).

Each page includes step-by-step instructions with screenshots, explains what to enter and how to interpret results, and details how features vary based on your permissions (Admin or Viewer).

## Getting started - build your financial system

For a brand new site with no data, work through these guides in order.

### 1. [Accounts](Accounts.md) - start here

Set up the accounts you spend from and save into.

- Add every account - checking, savings, credit cards, investments.
- Keep a current balance for each, plus a reserved amount you do not want to spend.
- Accrual totals are computed from each expense's schedule whenever accounts are read, so they always reflect the current schedules.
- Available funds account for the reserved buffer plus committed obligations - the cycles in progress and any past-due arrears. See the [glossary](../reference/glossary.md).

### 2. [Expenses](Expenses.md)

Add the bills and payments you owe. Needs at least one account.

- Recurring expenses on a schedule of days, weeks, months or years, and one-time expenses.
- An accrual start date, so a bill accrues from when it was last settled.
- Mark one or many as paid, including a mixed selection: an overdue item catches up through every period it missed, a future item advances once.
- Badges for overdue, due soon, excluded and frequency.

### 3. [Income](Income.md)

Add the money you expect to receive.

- Recurring income such as a salary or pension, and one-time income such as a bonus or refund.
- Payment schedules, and the account each amount lands in.
- Mark one or many as received, with the same overdue and future handling as expenses.
- The same badge vocabulary as expenses.

### 4. [Dashboard](Dashboard.md)

The day-to-day view of your finances.

- Quick actions that renew overdue expenses and incomes in one click.
- An account overview with current balances and the stable **Daily Need** rollup.
- Expenses and income due soon, filtered to 7, 14 or 30 day windows.
- Actions on an individual item to mark it paid or received, or advance it a period.
- Badges for overdue and due soon, frequency indicators and status colours.

Daily Need is the long-run daily funding requirement across active obligations. It stays stable across
payment dates and month lengths by design; [Cashflow model](../explanation/cashflow-model.md) explains why
there are two accrual figures.

### 5. [Projections](Projections.md)

See where the balances are heading.

- Charts of projected account balances across the window you choose.
- Several metrics - balance, income received and expenses paid - with the obligation deductions you choose to apply.
- Account-level detail as well as the totals.
- Useful for spotting a cash-flow gap early and for testing the effect of changing a bill.

## Data management

### 6. [Export](Export.md) and [Import](Import.md)

- Export your accounts, income and expenses to a file you keep.
- Import a previous export to restore or move your data.
- Worth doing before a large change, and useful as an archive.

## Configuration and collaboration

### 7. [Settings](Settings.md)

- Update your username and email, and change your password.
- Site name and description (site admins only).
- Display preferences.

### 8. [Users](Users.md) (admins only)

- Invite people, and resend an invitation that has not been accepted.
- Give each person the Admin or Viewer role.
- See who is active and who is still pending.

## Platform administration

### 9. [Platform Admin](PlatformAdmin.md) (requires `platform:manage`)

- Approve or reject new accounts - nobody can sign in until their account has been approved.
- Platform-wide tasks that cross sites.
- Configuring the grant itself: [Configure platform administrators](../how-to/configure-platform-admins.md).

## Security

### 10. [Security](Security.md)

- What keeps one site's data separate from another's.
- How sessions, passwords and approval protect your account, and what is not covered.
- The mechanics, for an interested reader: [Authentication](../explanation/authentication.md).

---

## New User Quick Start

If you're setting up POT for the first time:

1. **Create Accounts** (Guide #1)
   - Add your bank accounts (checking, savings)
   - Add credit cards
   - Enter current balances

2. **Add Expenses** (Guide #2)
   - List all recurring bills (rent, utilities, subscriptions)
   - Add one-time expenses if needed
   - Assign each to an account

3. **Add Income** (Guide #3)
   - Add salary/paycheck schedule
   - Add any other regular income
   - Assign each to an account

4. **View Results**
   - Check Dashboard (Guide #4) to see upcoming items
   - Check Projections (Guide #5) to see your future cash flow

5. **Optional: Export Backup** (Guide #6)
   - Save your initial setup

---

## Permissions Overview

POT uses role-based permissions to control access:

- **Admin** - Full control over site settings, users, and all financial data
- **Viewer** - Can view financial data but cannot modify anything

**Platform Admin** holds the `platform:manage` permission, which comes from deployment configuration rather
than from a role. It allows cross-site administration, such as approving new accounts. The grant does not
replace a role: a platform administrator still needs a role inside a site.

---

## Writing a guide page

Each page follows the same order, so a reader who has used one guide can find their way around the next:

1. **Overview** - what the feature does and why you would use it
2. **Available to** - which roles can use it
3. **Key Concepts** - the terms a reader needs, if the feature is not obvious
4. **Step-by-Step Guide** - what to enter, where to find it, how to read the result
5. **Common Workflows** - realistic scenarios, not a feature list
6. **Permission-Based Features** - what each role can and cannot do
7. **Visual Indicators** - what the badges and colours mean
8. **Tips and Best Practices** - only advice that is not obvious from the steps

Write short sentences in the imperative ("Select **Save**"), name buttons and fields exactly as they
appear, and say why a step matters rather than only how to do it.

Screenshots should show the forms and dialogs, the tables, the action and confirmation menus, the badges
and colour coding a reader will see, and the before/after state of anything that changes data.

Link between related pages - Accounts, Expenses, Income, Dashboard and Projections all refer to one
another - and link a term to the [glossary](../reference/glossary.md) rather than redefining it.

Before calling a page finished: every section is filled, the screenshots match the current UI, the
cross-references resolve, the permission differences are stated, and you have followed the steps yourself
against the running application.

---

For initial setup instructions, see [Getting Started](../tutorials/getting-started.md).

For technical details, see [Architecture](../ARCHITECTURE.md).
