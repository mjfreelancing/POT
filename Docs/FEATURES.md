# POT Features

This guide describes the main features available in POT (Pay On Time).

## Dashboard

Your financial command center providing an at-a-glance view of your financial health:

- **Quick Actions** - One-click automation for common tasks (renew overdue expenses/incomes, accrue account expenses, and more)
- **Account Overview** - Current balances across all your accounts plus stable **Daily Need** rollup
- **Upcoming Expenses & Income** - Bills and payments due in the near future
- **Period Filtering** - View expenses and income for 7, 14, or 30 day windows with dynamic metrics
- **Quick Item Actions** - Context menu on individual items to mark as paid/received or advance to next period
- **Visual Indicators** - Overdue and due-soon badges, frequency indicators, and status colors
- **Financial Status** - Real-time indicators of your financial position

Daily Need is the long-run daily funding requirement across active obligations. It is designed to stay stable across payment dates and calendar month-length changes.

The dashboard adapts to your selected time period, showing only relevant metrics and adjusting calculations dynamically.

[View detailed Dashboard guide →](USER-GUIDE/Dashboard.md)

## Financial Projections

Visualize your future financial position based on scheduled income and expenses:

- **Interactive Charts** - View account balances projected months into the future
- **Multiple Metrics** - Track balance, available funds, income received, and expenses paid
- **Flexible Time Periods** - View projections for 1-12 months ahead
- **Account-Level Details** - See projections for individual accounts

The projection system helps you:

- Identify potential cash flow issues before they occur
- Plan for large expenses by reviewing the impact of expense and income changes
- Ensure you always have enough funds to cover bills
- Make informed financial decisions

[View detailed Projections guide →](USER-GUIDE/Projections.md)

## Account Management

Manage all your financial accounts in one place:

- **Multiple Accounts** - Add all your bank accounts for credit cards, savings, and investments
- **Current Balances** - Maintain account balances for accurate projections
- **Quick Filtering** - Easily find and manage specific accounts
- **Expense Accruals** - Selectively update expense accrual calculations for any account
- **Accrual-Aware Availability** - Available funds account for reserved and accrued obligations

[View detailed Accounts guide →](USER-GUIDE/Accounts.md)

## Expense Management

Never miss a payment deadline:

- **Recurring Expenses** - Set up bills that repeat (days, weeks, months, years)
- **One-Time Expenses** - Configure individual purchases or payments
- **Payment Schedules** - Define when expenses are due
- **Accrual Start Date** - Specify when to start accruing the expense
- **Account Assignment** - Link expenses to specific accounts
- **Visual Status Indicators** - Color-coded badges for overdue, due soon, excluded, and frequency
- **Bulk Mark as Paid** - Select multiple expenses and mark them as paid in one action:
  - Automatically handles mixed selections (overdue and future items together)
  - Overdue items advance through multiple periods until caught up
  - Future items advance exactly once to next period (for early payment)
  - Smart confirmation dialog shows breakdown of what will happen to each type
- **Enhanced Table Features**:
  - Due date badges (Overdue in red, Due Soon in orange)
  - Frequency badges color-coded by type (Days=blue, Weeks=green, Months=purple, Years=amber)
  - Excluded items styled distinctly with red accents
  - Subtle row borders for improved readability

[View detailed Expenses guide →](USER-GUIDE/Expenses.md)

## Income Management

Track all your income sources:

- **Recurring Income** - Salaries, pensions, regular payments
- **One-Time Income** - Expected bonuses, tax refunds
- **Payment Schedules** - Define when income is received
- **Account Assignment** - Track which account receives each income
- **Visual Status Indicators** - Color-coded badges for overdue, due soon, excluded, and frequency
- **Bulk Mark as Received** - Select multiple incomes and mark them as received in one action:
  - Automatically handles mixed selections (overdue and future items together)
  - Overdue items advance through multiple periods until caught up
  - Future items advance exactly once to next period (for early receipt)
  - Smart confirmation dialog shows breakdown of what will happen to each type
- **Enhanced Table Features**:
  - Due date badges (Overdue in red, Due Soon in orange)
  - Frequency badges color-coded by type (Days=blue, Weeks=green, Months=purple, Years=amber)
  - Excluded items styled distinctly with red accents
  - Subtle row borders for improved readability

[View detailed Income guide →](USER-GUIDE/Income.md)

## Data Management

Keep your financial data safe and portable:

- **Export Data** - Download your accounts, incomes and expenses
- **Import Data** - Restore data from previous exports

Perfect for:

- Backing up your data before major changes
- Recovering from errors
- Archiving financial records

[View detailed Export guide →](USER-GUIDE/Export.md) | [View detailed Import guide →](USER-GUIDE/Import.md)

## User Settings

Customize your POT experience:

- **Profile Management** - Update your username and email
- **Password Changes** - Keep your account secure
- **Site Settings** - Customize site name and description (site owners only)
- **Display Preferences** - Configure how data is displayed

[View detailed Settings guide →](USER-GUIDE/Settings.md)

## User Management

Collaborate with others on financial management (site owners and admins):

- **User Invitations** - Invite family members or trusted individuals
- **Role Assignment** - Grant Admin or Viewer permissions
- **Access Control** - Manage who can view or modify financial data
- **User Status** - Monitor active and pending users
- **Invitation Management** - Resend invitations

[View detailed Users guide →](USER-GUIDE/Users.md)

## Security Features

POT takes security seriously:

- **Email Verification** - Verify your email address during signup
- **Account Approval** - Platform admin approval for new accounts
- **JWT Authentication** - Secure token-based authentication
- **Role-Based Permissions** - Granular access control for all features
- **Secure Password Requirements** - Enforce strong passwords
- **Session Management** - Automatic logout on inactivity

[View detailed Authentication & Security guide →](AUTHENTICATION.md)

## Platform Administration

For platform administrators only:

- **Platform Permissions** - Special `platform:manage` permission for platform specific tasks
- **User Approval** - Approve or reject new user registrations

[View detailed Platform Admin guide →](USER-GUIDE/PlatformAdmin.md)

---

For setup instructions, see [Getting Started](GETTING-STARTED.md).

For technical details, see [Architecture](ARCHITECTURE.md).
