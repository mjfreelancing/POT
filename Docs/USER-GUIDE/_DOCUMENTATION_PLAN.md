# User Guide Documentation Plan

This document outlines the structure and content requirements for each user guide. Work through guides in the numbered order for logical onboarding flow (new user perspective - starting from scratch with no data).

---

## 1. Accounts.md

**Purpose**: Foundation layer - create and manage financial accounts (FIRST STEP for new users)

**Sections**:

- Overview
  - What the dashboard shows
  - Purpose: at-a-glance financial health and quick actions
- Available to: All users (Site Owner, Admin, Viewer)
- Key Concepts
  - Period filtering (7, 14, 30 days)
  - Dynamic metrics (adapt to selected period)
  - Visual indicators (badges, colors)
  - localStorage persistence
- Step-by-Step Guide
  - **Viewing Dashboard**
    - Screenshots of main dashboard
    - Explain each section (accounts, expenses, income)
  - **Using Period Filters**
    - How to change period (7/14/30 days dropdown)
    - What happens to metrics (dynamic calculation)
    - How selection persists
  - **Understanding Visual Indicators**
    - Overdue badge (red) - meaning and interpretation
    - Due Soon badge (orange) - meaning and interpretation
    - Frequency badges (color coding: blue/green/purple/amber/slate)
    - Excluded items (red styling)
  - **Quick Item Actions**
    - Context menu on individual expense/income items
    - Mark as Paid/Received workflow
    - What happens to overdue vs future items
- Common Workflows
  - "I want to see expenses due in the next 2 weeks"
  - "I need to mark an overdue bill as paid"
  - "I want to see what's coming up this month"
- Tips & Best Practices
  - Start each session by checking dashboard
  - Use 7-day view for daily planning, 30-day for monthly overview
  - Understand badge meanings for quick status assessment

---

## 2. Accounts.md

**Purpose**: Foundation layer - create and manage financial accounts

**Sections**:

- Overview
  - What accounts are (bank accounts, credit cards, savings, etc.)
  - Why accounts are required (link expenses/income, track balances)
  - Relationship to projections
- Available to: All users (Site Owner, Admin, Viewer - view only)
- Key Concepts
  - Account types (checking, savings, credit card)
  - Current balance (manual entry)
  - Reserved amount (funds set aside)
  - Available calculation (Balance - Reserved - Accrued Expenses)
  - Expense accruals (what they are and why they matter)
  - Daily Need (stable daily funding target)
  - Difference between Daily Need (stable) and Projection Accruals (dynamic)
- Step-by-Step Guide
  - **Creating an Account**
    - Screenshot of create account form
    - What to enter:
      - Description (e.g., "Chase Checking", "Capital One Credit Card")
      - Account Type (dropdown options)
      - Current Balance (how to determine, where to get this number)
      - Reserved Amount (optional, what it means)
    - Save and view result
  - **Viewing Accounts List**
    - Screenshot of accounts table
    - What each column shows
    - How to filter/search
  - **Editing an Account**
    - Update balance (most common action)
    - Modify description or type
    - Adjust reserved amount
  - **Deleting an Account**
    - When/why you'd delete
    - Warning about linked expenses/income
  - **Accruing Account Expenses**
    - What this action does
    - When to use it
    - How it affects Available calculation
- Common Workflows
  - "I just got my bank statement, balance changed"
  - "I want to set aside $500 for an emergency fund"
  - "I need to add my new credit card"
  - "An account was closed"
- Permission-Based Features
  - Site Owners & Admins: Create, edit, delete, accrue
  - Viewers: View only
- Tips & Best Practices
  - Update balances regularly (weekly or monthly)
  - Use Reserved for funds you don't want to spend
  - Name accounts clearly (include bank name)
  - Accrue expenses before checking projections for accuracy

---

## 3. Expenses.md

**Purpose**: Track bills and payments, manage due dates, bulk actions

**Sections**:

- Overview
  - What expenses are (bills, subscriptions, payments)
  - Purpose: never miss a payment, project future cash needs
  - Recurring vs one-time expenses
- Available to: All users (Site Owner, Admin for management; Viewers can view only)
- Key Concepts
  - NextDue date (when bill is due)
  - Frequency types (Days, Weeks, Months, Years, OneTime)
  - FrequencyCount (e.g., every 2 weeks)
  - AccrualStart (when expense starts accruing)
  - EndDate (optional, when expense stops)
  - Account assignment (which account pays this)
  - ExcludeFromCalcs (temporarily exclude from projections)
  - Overdue vs Future items
  - Renewal logic (how advancing works)
- Step-by-Step Guide
  - **Creating an Expense**
    - Screenshot of create form
    - What to enter:
      - Description (e.g., "Electric Bill", "Netflix Subscription")
      - Amount (monthly cost)
      - NextDue date (when payment is due)
      - Frequency (dropdown: Days/Weeks/Months/Years/OneTime)
      - FrequencyCount (e.g., 1 for monthly, 2 for bi-weekly)
      - AccrualStart (usually same as NextDue, or earlier if accruing over time)
      - EndDate (optional, leave blank for ongoing)
      - Account (which account pays this expense)
      - ExcludeFromCalcs checkbox (when to use)
    - Save and view in table
  - **Viewing Expenses Table**
    - Screenshot of expenses table
    - Understanding columns:
      - Description (with notes icon if present)
      - Amount
      - NextDue (with badges: Overdue/Due Soon)
      - Frequency (color-coded badges)
      - EndDate
      - Account
    - Row styling (excluded items in red)
    - Bulk selection checkboxes
  - **Understanding Visual Indicators**
    - Overdue badge (red, prominent) - bill is past due
    - Due Soon badge (orange, smaller) - bill due within 7 days
    - Frequency badges:
      - Days (blue)
      - Weeks (green)
      - Months (purple)
      - Years (amber)
      - One Time (slate/gray)
    - Excluded items (red text, red left border, "Excluded" badge)
  - **Editing an Expense**
    - When to edit (amount changed, date adjustment)
    - What fields to update
  - **Deleting an Expense**
    - When to delete (no longer applicable)
    - Confirmation dialog
  - **Excluding an Expense**
    - When to use (temporarily skip in projections)
    - How to toggle ExcludeFromCalcs
    - Visual result (red styling)
  - **Bulk Mark as Paid**
    - Screenshot showing bulk selection
    - Select multiple expenses (checkboxes)
    - Click "Mark as Paid" action
    - Smart filtering in confirmation:
      - Shows overdue items that will advance multiple times
      - Shows future items that will advance once
      - Clear summary of what will happen
    - Confirm action
    - Result: NextDue dates updated, expenses "renewed"
  - **Individual Mark as Paid (Table)**
    - Row action menu (three dots)
    - Click "Mark as Paid"
    - Same smart behavior as bulk
    - Confirmation dialog
  - **Adding Notes**
    - How to add/edit notes
    - Note icon appears in Description column
- Common Workflows
  - "I have several overdue bills to mark as paid"
  - "I paid my electric bill early"
  - "My rent amount changed"
  - "I want to temporarily exclude grocery expenses from projections"
  - "I need to add a new monthly subscription"
  - "A yearly payment is coming up, I want to see the impact"
- Permission-Based Features
  - Site Owners & Admins: Full management (create, edit, delete, mark as paid, exclude)
  - Viewers: View only (cannot mark as paid or modify)
- Tips & Best Practices
  - Add all recurring bills first (highest impact on projections)
  - Use AccrualStart strategically for large bills (spread impact)
  - Mark expenses as paid regularly (weekly check-in)
  - Use notes for important details (account numbers, confirmation codes)
  - Exclude items temporarily to test "what if" scenarios
  - Bulk actions save time when catching up on multiple overdue items

---

## 4. Income.md

**Purpose**: Track income sources, payment schedules, bulk actions

**Sections**:

- Overview
  - What income entries are (salary, bonuses, payments)
  - Purpose: project incoming funds, plan cash flow
  - Recurring vs one-time income
- Available to: All users (Site Owner, Admin for management; Viewers can view only)
- Key Concepts
  - NextDue date (when payment is expected)
  - Frequency types (Days, Weeks, Months, Years, OneTime)
  - FrequencyCount (e.g., bi-weekly = 2 weeks)
  - EndDate (optional, when income stops - contracts, temp jobs)
  - Account assignment (which account receives payment)
  - ExcludeFromCalcs (temporarily exclude from projections)
  - Overdue vs Future items
  - Renewal logic (same as expenses, but for income)
- Step-by-Step Guide
  - **Creating Income**
    - Screenshot of create form
    - What to enter:
      - Description (e.g., "Salary", "Freelance Project", "Tax Refund")
      - Amount (payment amount)
      - NextDue date (when you expect payment)
      - Frequency (Days/Weeks/Months/Years/OneTime)
      - FrequencyCount (e.g., 2 for bi-weekly)
      - EndDate (optional, for temporary income like contract work)
      - Account (which account receives payment)
      - ExcludeFromCalcs checkbox (when to use)
    - Save and view in table
  - **Viewing Income Table**
    - Screenshot of income table
    - Understanding columns:
      - Description (with notes icon if present)
      - Amount
      - NextDue (with badges: Overdue/Due Soon)
      - Frequency (color-coded badges)
      - EndDate
      - Account
    - Row styling (excluded items in red)
    - Bulk selection checkboxes
  - **Understanding Visual Indicators**
    - Same as Expenses:
      - Overdue badge (red) - payment was expected
      - Due Soon badge (orange) - payment expected within 7 days
      - Frequency badges (same color scheme)
      - Excluded items (red styling)
  - **Editing Income**
    - When to edit (amount changed, date adjustment)
    - What fields to update
  - **Deleting Income**
    - When to delete (income source ended)
    - Confirmation dialog
  - **Excluding Income**
    - When to use (temporarily skip in projections)
    - How to toggle ExcludeFromCalcs
    - Visual result (red styling)
  - **Bulk Mark as Received**
    - Screenshot showing bulk selection
    - Select multiple income items (checkboxes)
    - Click "Mark as Received" action
    - Smart filtering in confirmation:
      - Shows overdue items that will advance multiple times (catch up)
      - Shows future items that will advance once (early receipt)
      - Clear summary of what will happen
    - Confirm action
    - Result: NextDue dates updated, income "renewed"
  - **Individual Mark as Received (Table)**
    - Row action menu (three dots)
    - Click "Mark as Received"
    - Same smart behavior as bulk
    - Confirmation dialog
  - **Adding Notes**
    - How to add/edit notes
    - Note icon appears in Description column
- Common Workflows
  - "I got paid today, need to mark salary as received"
  - "My freelance payment came in early"
  - "I received a bonus, want to add it"
  - "I want to see cash flow without my tax refund (exclude)"
  - "I need to add a new contract with an end date"
  - "Multiple paychecks are overdue in the system"
- Permission-Based Features
  - Site Owners & Admins: Full management (create, edit, delete, mark as received, exclude)
  - Viewers: View only (cannot mark as received or modify)
- Tips & Best Practices
  - Add all regular income sources first (salary, pensions)
  - For irregular income, use OneTime and add as confirmed
  - Mark income as received regularly (weekly/bi-weekly with paycheck)
  - Use EndDate for temporary work (contracts, seasonal jobs)
  - Exclude income temporarily to see "worst case" cash flow
  - Notes helpful for tracking invoice numbers or payment sources

---

## 5. Projections.md

**Purpose**: View future account balances based on expenses and income

**Sections**:

- Overview
  - What projections show (future account balances)
  - Purpose: identify cash flow issues before they happen
  - How projections are calculated
  - Relationship to accounts/expenses/income
  - Projection Accruals are dynamic operational values
  - Dashboard Daily Need is a separate stable planning value
- Available to: All users (all roles can view projections)
- Key Concepts
  - Projection period (1-12 months)
  - Metrics tracked:
    - Balance (account balance over time)
    - Available (balance minus reserved minus accrued expenses)
    - Expenses Paid (cumulative expense payments)
    - Incomes Received (cumulative income receipts)
  - Account-level vs All Accounts view
  - Daily vs monthly granularity
  - Expense accrual impact
  - How renewals affect projections
- Step-by-Step Guide
  - **Viewing Projections**
    - Screenshot of projections page
    - Chart explanation (x-axis: time, y-axis: dollars)
    - Metric toggles (show/hide specific metrics)
    - Account selector (filter to specific account or view all)
  - **Selecting Projection Period**
    - Dropdown showing 1-12 months
    - Impact of longer periods (more data, broader view)
    - When to use each period:
      - 1-3 months: immediate planning
      - 6 months: medium-term planning
      - 12 months: annual planning
  - **Interpreting the Chart**
    - Balance line (total funds in account)
    - Available line (what you can actually spend)
    - Expenses Paid line (cumulative payments)
    - Incomes Received line (cumulative receipts)
    - Identifying problem areas:
      - Balance dipping below zero = overdraft
      - Available below expected = cash flow issue
      - Gaps between income and expenses = savings opportunity
  - **Filtering by Account**
    - Select specific account from dropdown
    - View projection for that account only
    - Use case: checking if specific account will cover its assigned expenses
  - **Understanding the Data**
    - How expenses accrue daily (gradual buildup)
    - How expenses "pay" on NextDue date (balance decrease)
    - How income "arrives" on NextDue date (balance increase)
    - Impact of excluded items (they don't appear in projection)
  - **Taking Action from Projections**
    - Spotted shortfall → adjust expenses or move funds
    - Spotted surplus → consider savings or investments
    - Test scenarios with ExcludeFromCalcs toggle
- Common Workflows
  - "Will I have enough to pay rent next month?"
  - "When should I move money from savings to checking?"
  - "Can I afford this new subscription?"
  - "What happens if I delay this expense?"
  - "I want to see cash flow for my checking account only"
- Tips & Best Practices
  - Check projections weekly (after updating balances)
  - Use 3-6 month view for best planning balance
  - Focus on Available line (most accurate for spending power)
  - Accrue expenses before viewing for accuracy
  - Test "what if" scenarios with exclude toggles
  - Plan transfers between accounts when you see shortfalls
  - Don't panic over small negative blips (timing differences)

---

## 6. Export.md

**Purpose**: Backup financial data to files

**Sections**:

- Overview
  - What export does (creates backup files)
  - What data is included (accounts, expenses, income)
  - File format (CSV)
  - Use cases (backup, archival, data portability)
- Available to: All users (all roles can export)
- Step-by-Step Guide
  - **Exporting All Data**
    - Screenshot of export page
    - Click "Export All" button
    - File download begins
    - File naming convention (timestamp)
  - **Exporting Individual Tables**
    - Select Accounts only
    - Select Expenses only
    - Select Income only
    - When to use selective export
  - **Understanding Export Files**
    - CSV format (can open in Excel, Google Sheets)
    - Columns included in each file
    - Date formats
  - **Storage Best Practices**
    - Where to save backups (cloud, external drive)
    - How often to export (monthly recommended)
    - File naming for easy retrieval
- Common Workflows
  - "I want to backup my data before making major changes"
  - "I need an archive for tax purposes"
  - "I'm switching computers, need to transfer data"
  - "I want to analyze my expenses in a spreadsheet"
- Tips & Best Practices
  - Export regularly (monthly or before major updates)
  - Store backups in multiple locations
  - Include date in filename for version tracking
  - Test imports occasionally to verify backup integrity

---

## 7. Import.md

**Purpose**: Restore data from export files (updates existing items or adds new ones; does not delete existing data)

**Sections**:

- Overview
  - What import does (restores data from POT export files)
  - When to use (recovery, restoring from backup, adding exported data to current site)
  - What data can be imported (accounts, expenses, income from export files only)
  - Import behavior (updates items by ID, adds new items, retains other existing data)
- Available to: Site Owners & Admins only
- Key Concepts
  - Update behavior (matches by ID, updates existing items)
  - Add behavior (items not found by ID are added as new)
  - Retention behavior (existing items not in import file are kept)
  - File format requirements (must be POT export format)
  - Validation errors
  - No deletion (import never removes existing data)
- Step-by-Step Guide
  - **Preparing for Import**
    - Backup current data first (export recommended)
    - Ensure file is from POT export (correct format)
    - Review file contents for accuracy
    - Understand that existing data not in file will be retained
  - **Importing Data**
    - Screenshot of import page
    - Select export file (drag-drop or browse)
    - Click Import button
    - Wait for processing
  - **Handling Validation Errors**
    - Common errors:
      - Missing required columns
      - Invalid date formats
      - Missing account references
      - Incorrect file format
    - How to fix and retry
  - **Verifying Import Success**
    - Check counts (how many items updated vs added)
    - Review data in tables (verify updates applied)
    - Check that other existing data was retained
    - Check projections to ensure accuracy
- Common Workflows
  - "I need to restore from a backup after partial data loss"
  - "I exported from production, want to add that data to my test site"
  - "I have an old export and want to recover specific items"
  - "I manually edited an export file and want to update items in bulk"
- Permission-Based Features
  - Site Owners: Full import access
  - Admins: Full import access
  - Viewers: Cannot import (view only)
- Tips & Best Practices
  - Always export before importing (safety net)
  - Understand import doesn't delete existing data not in file
  - Use for selective updates or additions
  - Test imports on a test site first if uncertain
  - Verify projections after import to ensure data integrity
  - Remember: import file must be from POT export (not arbitrary CSV)

---

## 8. Settings.md

**Purpose**: Configure user preferences and site settings

**Sections**:

- Overview
  - What settings control (user profile, site configuration)
  - Who can access what (role-based)
- Available to: All users (different options by role)
- Step-by-Step Guide
  - **Updating User Profile**
    - Screenshot of profile section
    - Fields:
      - Username (display name)
      - Email address
    - Save changes
  - **Changing Password**
    - Screenshot of password section
    - Current password (required)
    - New password (requirements)
    - Confirm new password
    - Save
  - **Site Settings (Site Owners Only)**
    - Screenshot of site settings section
    - Fields:
      - Site Name (appears in header)
      - Site Description
    - Save changes
  - **Display Preferences**
    - Screenshot of preferences section
    - Options (if any exist)
    - Save preferences
- Permission-Based Features
  - All Users: Profile, password
  - Site Owners: Site settings (name, description)
  - Admins & Viewers: Cannot modify site settings
- Common Workflows
  - "I need to change my password"
  - "I want to update my email address"
  - "I need to rename the site"
  - "I want to update site description"
- Tips & Best Practices
  - Use strong passwords (required by system)
  - Keep email up to date for recovery
  - Only site owner should modify site settings

---

## 9. Users.md

**Purpose**: Invite and manage users (collaboration)

**Sections**:

- Overview
  - What user management allows (invite others to collaborate)
  - Roles available (Admin, Viewer)
  - Who can manage users (Site Owners, Admins)
- Available to: Site Owners (full access), Admins (limited access)
- Key Concepts
  - Invitation workflow
  - User roles and permissions
  - Active vs Pending users
  - Invitation expiration
  - Self-action prevention (can't lock yourself out)
- Step-by-Step Guide
  - **Viewing Users List**
    - Screenshot of users table
    - Columns:
      - Username
      - Email
      - Role
      - Status (Active/Pending)
    - Own row highlighted (can't act on yourself)
  - **Inviting a User**
    - Screenshot of invite dialog
    - Enter email address
    - Select role (Admin or Viewer)
    - Send invitation
    - What happens next (user receives email)
  - **Resending Invitation**
    - When to resend (user didn't receive)
    - Click resend action
    - New invitation email sent
  - **Changing User Role**
    - When to change (promote to Admin, demote to Viewer)
    - Select new role
    - Save change
    - Effect immediate
  - **Deleting a User**
    - When to delete (user no longer needs access)
    - Click delete action
    - Confirmation dialog
    - User removed (cannot be undone)
  - **Understanding Invitation Status**
    - Pending: Invitation sent, not yet accepted
    - Active: User has signed up and can access site
- Common Workflows
  - "I want to invite my spouse to view our finances"
  - "I need to add an accountant as a Viewer"
  - "I want to make my partner an Admin"
  - "Someone left the household, need to remove access"
  - "User didn't receive invitation, need to resend"
- Permission-Based Features
  - Site Owners:
    - Invite Admins and Viewers
    - Change any user's role
    - Delete any user except themselves
    - Resend any invitation
  - Admins:
    - Invite Viewers only
    - Change Viewer roles
    - Delete Viewers
    - Cannot modify Site Owners or other Admins
    - Cannot delete themselves
  - Viewers:
    - No user management access
- Tips & Best Practices
  - Use Viewer role for family members who only need to see data
  - Use Admin role for trusted partners who help manage finances
  - Check pending invitations periodically (resend if needed)
  - Don't delete users unless you're sure (cannot be undone)
  - Communication: Let users know what role you're assigning and why

---

## 10. PlatformAdmin.md

**Purpose**: Platform-level administration (approve new users)

**Sections**:

- Overview
  - What platform admin does (cross-site administration)
  - Required permission (`platform:manage`)
  - Use case: POT hosting provider managing multiple sites
- Available to: Platform Admins only (special permission)
- Key Concepts
  - User approval workflow
  - Why approval is required (spam prevention)
  - Approved vs Rejected users
  - Approval scope (cross-site)
- Step-by-Step Guide
  - **Viewing Pending Approvals**
    - Screenshot of approvals page
    - Table showing:
      - Username
      - Email
      - Registration date
      - Actions (Approve/Reject)
  - **Approving a User**
    - Review user details
    - Click Approve button
    - User can now log in
    - Confirmation shown
  - **Rejecting a User**
    - Review user details
    - Click Reject button
    - User registration denied
    - Confirmation shown
  - **Understanding Approval Impact**
    - Approved users can access their site
    - Rejected users cannot access (registration invalid)
- Common Workflows
  - "New user registered, I need to approve them"
  - "Suspicious registration, I need to reject it"
  - "I want to review all pending approvals"
- Permission-Based Features
  - Platform Admins: Full approval access
  - Site Owners, Admins, Viewers: No access (site-specific only)
- Tips & Best Practices
  - Review registration details carefully
  - Look for spam indicators (generic emails, weird usernames)
  - Approve legitimate users promptly (good user experience)
  - When in doubt, you can always reject (user can re-register)

---

## Documentation Guidelines

### Screenshot Requirements

Each guide should include screenshots showing:

1. **Key UI Elements**
   - Forms (create/edit dialogs)
   - Tables (data views)
   - Action menus (context menus, bulk actions)
   - Confirmation dialogs

2. **Visual Indicators**
   - Badges (Overdue, Due Soon, Excluded)
   - Color coding (frequency badges)
   - Status styling (red for excluded, highlights)

3. **Workflows**
   - Before/after states
   - Multi-step processes
   - Results of actions

### Writing Style

- **Clear and concise** - Short sentences, simple language
- **Actionable** - Use imperatives (Click, Enter, Select)
- **Specific** - Provide exact field names, button labels
- **Visual** - Reference screenshots frequently
- **Contextual** - Explain why, not just how
- **Practical** - Include real-world examples

### Section Order Consistency

Every guide should follow this order when applicable:

1. Overview
2. Available to (permissions)
3. Key Concepts (if complex feature)
4. Step-by-Step Guide (detailed instructions)
5. Common Workflows (scenarios)
6. Permission-Based Features (role differences)
7. Tips & Best Practices

### Cross-References

Link between related guides:

- Dashboard → Expenses, Income, Accounts
- Accounts → Expenses, Income, Projections
- Expenses → Accounts, Projections, Dashboard
- Income → Accounts, Projections, Dashboard
- Projections → Accounts, Expenses, Income

---

## Review Checklist

Before marking a guide as complete:

- [ ] All sections present and filled
- [ ] Screenshots added and annotated
- [ ] Cross-references to related guides
- [ ] Real-world examples included
- [ ] Permission-based features clearly documented
- [ ] Common workflows address user questions
- [ ] Tips section provides value
- [ ] Tested by following instructions step-by-step
- [ ] Accurate to current implementation
- [ ] Consistent tone and style
