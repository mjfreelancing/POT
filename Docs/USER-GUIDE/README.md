# POT User Guide

This guide provides detailed instructions for using each feature in POT (Pay On Time).

Each page includes step-by-step instructions with screenshots, explains what to enter and how to interpret results, and details how features vary based on your permissions (Site Owner, Admin, or Viewer).

## Getting Started - Build Your Financial System

For a brand new site with no data, work through these guides in order:

1. **[Accounts](Accounts.md)** - **START HERE** - Set up your financial accounts (checking, savings, credit cards)
2. **[Expenses](Expenses.md)** - Add your bills and payments (requires accounts to exist first)
3. **[Income](Income.md)** - Add your income sources and payment schedules
4. **[Dashboard](Dashboard.md)** - View your financial overview (now populated with real data)
5. **[Projections](Projections.md)** - See future account balances based on your expenses and income

## Data Management

Once your system is set up, use these tools to maintain and protect your data:

6. **[Export](Export.md)** - Export (backup) your financial data
7. **[Import](Import.md)** - Import (restore) data from export files

## Configuration & Collaboration

Customize your experience and manage access:

8. **[Settings](Settings.md)** - Configure user preferences and site settings
9. **[Users](Users.md)** - Invite and manage users (Site Owners & Admins only)

## Platform Administration

For platform administrators only:

10. **[Platform Admin](PlatformAdmin.md)** - Approve new users and platform-level tasks (requires `platform:manage` permission)

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

- **Site Owner** - Full control over site settings, users, and all financial data
- **Admin** - Can manage financial data and invite users (cannot modify site settings)
- **Viewer** - Can view financial data but cannot modify anything

**Platform Admin** is a special role with `platform:manage` permission for cross-site administration tasks like approving new user registrations.

---

## Documentation Structure

Each guide follows this pattern:

- **Overview** - What the feature does and why you'd use it
- **Available to** - Which roles can access this feature
- **Key Concepts** - Important terms and ideas to understand
- **Step-by-Step Guide** - Detailed instructions with screenshots showing:
  - What data to enter
  - Where to find information
  - How to interpret results
- **Common Workflows** - Real-world scenarios and how to handle them
- **Permission-Based Features** - What each role can do
- **Visual Indicators** - Understanding badges, colors, and status indicators
- **Tips & Best Practices** - Helpful advice for effective use

---

For a feature overview, see [Features](../FEATURES.md).

For initial setup instructions, see [Getting Started](../GETTING-STARTED.md).

For technical details, see [Architecture](../ARCHITECTURE.md).
