# 014 – E2E Test Coverage Discovery (R9)

**Purpose:** Complete coverage map of all UI operations discovered by live-browser exploration.  
Maps every user journey to a test classification, data strategy, and key locators before any fixture-managed or isolated-sequence tests are written.

**Status:** In Progress — initial exploration complete; under review; additional MCP scenarios may be added.  
**Last Updated:** 2026-05-17

**Exploration method:** MCP Playwright browser tools against the live development stack (`http://localhost:5175`).  
**Exploration state:** Mode 2 — site `POT E2E Site`, two canonical E2E users, one account "Main Checking", one expense "Monthly Rent", one income "Monthly Salary".

---

## 1. Route Inventory

All routes confirmed from `src/routes/AppRoutes.tsx` and live navigation.

| Route                            | Type                | Notes                                                       |
| -------------------------------- | ------------------- | ----------------------------------------------------------- |
| `/login`                         | Public              | Login, Forgot Password, Sign Up dialogs                     |
| `/logout`                        | Public              | Triggers logout + redirects to `/login`                     |
| `/`                              | Protected           | Redirects to `/dashboard`                                   |
| `/dashboard`                     | Protected           | Quick Actions + overview sections                           |
| `/projections`                   | Protected           | Chart view with view selector, period, date, account filter |
| `/accounts`                      | Protected           | Accounts table                                              |
| `/accounts/create`               | Protected overlay   | Sheet rendered as child route                               |
| `/accounts/edit/:id`             | Protected overlay   | Sheet rendered as child route                               |
| `/expenses`                      | Protected           | Expenses table with search + account filter                 |
| `/expenses/create`               | Protected overlay   | Sheet rendered as child route                               |
| `/expenses/create?duplicate=:id` | Protected overlay   | Pre-filled duplicate sheet                                  |
| `/expenses/edit/:id`             | Protected overlay   | Sheet rendered as child route                               |
| `/incomes`                       | Protected           | Income table with search + account filter                   |
| `/incomes/create`                | Protected overlay   | Sheet rendered as child route                               |
| `/incomes/edit/:id`              | Protected overlay   | Sheet rendered as child route                               |
| `/users`                         | Protected           | Users table                                                 |
| `/users/invite`                  | Protected overlay   | Sheet rendered as child route                               |
| `/approvals/pending`             | Protected           | Platform admin only; not in sidebar for non-platform-admin  |
| `*` (unknown)                    | Protected catch-all | Redirects to `/dashboard`                                   |

**Route guard behaviour:**

- Unauthenticated access to any protected route → redirect to `/login`
- Unknown route when authenticated → redirect to `/dashboard`
- Root `/` → redirect to `/dashboard`

---

## 2. Sidebar Navigation

Sidebar items confirmed from `src/components/nav/AppSidebarMenus.tsx`.

| Label       | Href/Action          | Required Permission          | Admin Visible           | Viewer Visible |
| ----------- | -------------------- | ---------------------------- | ----------------------- | -------------- |
| Dashboard   | `/dashboard`         | (authenticated)              | ✓                       | ✓              |
| Projections | `/projections`       | (authenticated)              | ✓                       | ✓              |
| Accounts    | `/accounts`          | (authenticated)              | ✓                       | ✓              |
| Expenses    | `/expenses`          | (authenticated)              | ✓                       | ✓              |
| Income      | `/incomes`           | (authenticated)              | ✓                       | ✓              |
| Users       | `/users`             | `user:manage` or `user:view` | ✓                       | ✓              |
| Approvals   | `/approvals/pending` | `platform:manage`            | ✓ (platform admin only) | ✗              |
| Export...   | Opens dialog         | `maintenance:export`         | ✓                       | ✗              |
| Import...   | Opens dialog         | `maintenance:import`         | ✓                       | ✗              |

**Mobile behaviour:** Export and Import are hidden at mobile viewport (`isMobile === true`).  
**Sidebar toggle:** At mobile viewport (≤ ~768px) the sidebar is collapsed by default. `Toggle Sidebar` button is visible in the header; clicking it opens the sidebar overlay.

---

## 3. Authentication / Login Page

### 3.1 Login Form

| Operation                     | Expected Outcome                                                                                 | Classification                    |
| ----------------------------- | ------------------------------------------------------------------------------------------------ | --------------------------------- |
| Load `/login`                 | Username + Password fields visible, Login button, "Forgot your password?" link, "Sign up" link   | parallel-safe                     |
| Submit empty form             | Login button disabled — cannot submit                                                            | parallel-safe                     |
| Submit with wrong credentials | Inline error "Invalid username or password" in form (generic div, no `[role="alert"]`, no toast) | parallel-safe (transient session) |
| Submit with valid credentials | Redirect to `/dashboard`, refresh token cookie `pot_refresh_token` set                           | parallel-safe (transient session) |

**Login form locators:**

- Username: `page.getByLabel('Username')`
- Password: `page.getByLabel('Password')`
- Submit: `page.getByRole('button', { name: 'Login' })`
- Inline error: element within the login form containing the error text (no stable role/testid yet — record as locator gap)

### 3.2 Forgot Password Dialog

| Operation                              | Expected Outcome                                                                   | Classification |
| -------------------------------------- | ---------------------------------------------------------------------------------- | -------------- |
| Click "Forgot your password?"          | Dialog opens with title "Reset Password", username field, "Send Reset Code" button | parallel-safe  |
| "Send Reset Code" with empty field     | Button disabled — cannot submit                                                    | parallel-safe  |
| "Send Reset Code" with username filled | Step 2 (not yet explored; expected: code + new password fields)                    | —              |

**Forgot Password locators:**

- Trigger: `page.getByRole('button', { name: 'Forgot your password?' })` (rendered as `<button>`)
- Dialog: `page.getByRole('dialog')`
- Username: `page.getByLabel('Username')` scoped inside dialog
- Send button: `page.getByRole('button', { name: 'Send Reset Code' })`

### 3.3 Sign Up Dialog

| Operation                                      | Expected Outcome                                                                                   | Classification |
| ---------------------------------------------- | -------------------------------------------------------------------------------------------------- | -------------- |
| Click "Sign up"                                | Dialog opens with title "Create Account", Username + Email fields, "Send Verification Code" button | parallel-safe  |
| "Send Verification Code" with empty fields     | Button disabled                                                                                    | parallel-safe  |
| Fill username + email → Send Verification Code | Step 2 (not yet explored; expected: OTP/code entry field)                                          | —              |

**Sign Up locators:**

- Trigger: `page.getByRole('button', { name: 'Sign up' })`
- Dialog: `page.getByRole('dialog')`
- Username: `page.locator('[name="username"]')` scoped inside dialog
- Email: `page.locator('[name="email"]')` scoped inside dialog
- Send button: `page.getByRole('button', { name: 'Send Verification Code' })`

> **Note:** Multi-step Sign Up (step 2: verification code entry) and multi-step Password Reset (step 2: code + new password) were not fully explored. These flows require a real email or a test OTP strategy. Defer until OTP testing strategy is decided (see PRD D7).

---

## 4. Dashboard

### 4.1 Quick Actions (Admin only)

| Operation                        | Expected Outcome                                                                                                    | Classification    |
| -------------------------------- | ------------------------------------------------------------------------------------------------------------------- | ----------------- |
| Load dashboard as admin          | "Quick Actions" section visible with 4 cards: Income Renewal, Expense Accrual, Budget Reminder, Income Confirmation | parallel-safe     |
| Load dashboard as viewer         | "Quick Actions" section not present                                                                                 | parallel-safe     |
| Click "Income Renewal" card      | Toast `[data-sonner-toast]` with text containing "Income Renewal"                                                   | isolated-sequence |
| Click "Expense Accrual" card     | Toast with text containing "Expense Accrual"                                                                        | isolated-sequence |
| Click "Budget Reminder" card     | Toast (email trigger)                                                                                               | isolated-sequence |
| Click "Income Confirmation" card | Toast with text containing "Income Confirmation"                                                                    | isolated-sequence |

**Quick Action locators:**

- Quick Actions heading: `page.getByRole('heading', { name: 'Quick Actions' })`
- Individual cards: generic divs. Locate via heading text inside card. Cannot use `getByRole('button')`.
- Click pattern: `page.evaluate(() => document.querySelector('[text*="Income Renewal"]')?.click())` or scope by heading within section
- Toast: `page.locator('[data-sonner-toast]')`

> **Testability gap:** Quick Action cards have no accessible role or `data-testid`. Recommend adding `role="button"` or `data-testid="quick-action-{id}"` to each card for reliable E2E targeting.

### 4.2 Overview Sections

| Section                                          | Viewer | Admin | Classification                                     |
| ------------------------------------------------ | ------ | ----- | -------------------------------------------------- |
| "Accounts Overview" collapsible section          | ✓      | ✓     | parallel-safe (with seeded data)                   |
| "Expenses Overview" collapsible section (30-day) | ✓      | ✓     | parallel-safe (with seeded data)                   |
| "Income Overview" collapsible section (30-day)   | ✓      | ✓     | parallel-safe (with seeded data)                   |
| "$0.00" empty-state values                       | ✓      | ✓     | parallel-safe (baseline DB with no financial data) |

**Collapse/expand locators:**

- Section toggle buttons: `page.getByRole('button', { name: 'Accounts Overview' })` etc.

---

## 5. Accounts Page

### 5.1 Page State

| Operation                    | Expected Outcome                                                                  | Classification                   |
| ---------------------------- | --------------------------------------------------------------------------------- | -------------------------------- |
| Load `/accounts` (empty)     | Empty state message visible, "Add Account" button present (admin)                 | parallel-safe                    |
| Load `/accounts` (with data) | Table with columns: Account Name, BSB, Account Number, Balance, Reserved, Actions | parallel-safe (with seeded data) |
| Column headers visible       | Account Name, BSB, Account Number, Balance, Reserved                              | parallel-safe                    |

**Toolbar locators:**

- Add button: `page.getByRole('button', { name: /add.*account/i })` or `page.getByLabel('Add a new account')`

### 5.2 Create Account

| Operation                | Expected Outcome                                                                | Classification  |
| ------------------------ | ------------------------------------------------------------------------------- | --------------- |
| Click "Add Account"      | Navigates to `/accounts/create`, sheet/dialog opens with title "Create Account" | fixture-managed |
| Submit empty form        | Validation errors on required fields                                            | fixture-managed |
| Fill valid data + Submit | Account created, redirect to `/accounts`, row appears in table, toast success   | fixture-managed |

**Create form fields** (confirmed via live exploration):

- Account Name (text, required)
- BSB (text, format `000-000`)
- Account Number (text)
- Opening Balance (number)
- Reserved Amount (number)

**Form locators:**

- Dialog: `page.getByRole('dialog')`
- Form fields: `page.getByLabel('Account Name')` etc. scoped inside dialog

### 5.3 Edit Account

| Operation                      | Expected Outcome                                                              | Classification  |
| ------------------------------ | ----------------------------------------------------------------------------- | --------------- |
| Click row "Open menu" → "Edit" | Navigates to `/accounts/edit/:id`, sheet/dialog opens pre-filled              | fixture-managed |
| Modify a field + Submit        | Account updated, redirect to `/accounts`, row updated in table, toast success | fixture-managed |
| Viewer clicks "Edit"           | Edit button disabled (`aria-disabled="true"`)                                 | parallel-safe   |

### 5.4 Delete Account

| Operation                        | Expected Outcome                              | Classification  |
| -------------------------------- | --------------------------------------------- | --------------- |
| Click row "Open menu" → "Delete" | Confirmation dialog appears ("Are you sure?") | fixture-managed |
| Confirm deletion                 | Account removed from table, toast success     | fixture-managed |
| Viewer sees "Delete"             | Menu item disabled (`aria-disabled="true"`)   | parallel-safe   |

**Row action locators:**

- Open menu: `page.getByRole('button', { name: 'Open menu' })` scoped to row
- Menu item: `page.getByRole('menuitem', { name: 'Edit' })` / `page.getByRole('menuitem', { name: 'Delete' })`
- Confirmation dialog: `page.getByRole('dialog')`
- Confirm button: `page.getByRole('button', { name: 'Delete' })` inside dialog

---

## 6. Expenses Page

### 6.1 Page State

| Operation                                | Expected Outcome                                                                                               | Classification                   |
| ---------------------------------------- | -------------------------------------------------------------------------------------------------------------- | -------------------------------- |
| Load `/expenses` (empty, no accounts)    | Empty state, "Add Expense" button disabled with tooltip "Create at least one account first"                    | parallel-safe                    |
| Load `/expenses` (empty, accounts exist) | Empty state, "Add Expense" button enabled                                                                      | parallel-safe (with seeded data) |
| Load `/expenses` (with data)             | Table with columns: Description, Account, Category(?), Amount, Due Date, Frequency, Paid Date, Status, Actions | parallel-safe (with seeded data) |
| Search by description                    | Filters table rows, `FilterResultsCount` updates                                                               | fixture-managed                  |
| Account filter (multi-account)           | Filter dropdown appears only when `accountsInItems.length > 1`, sets `?accountId=` URL param                   | fixture-managed                  |

**Search locator:** `page.getByLabel('Search expenses by description')` (from `ariaLabel="Search expenses by description"`)  
**Account filter locator:** `page.getByLabel('Filter by account')` (Radix Select, NOT native `<select>`)

> **Account filter note:** Filter is only rendered when items span more than one account. Testing requires at least two accounts with expenses. URL state: `?accountId={id}`.

### 6.2 Create Expense

| Operation                | Expected Outcome                                                   | Classification  |
| ------------------------ | ------------------------------------------------------------------ | --------------- |
| Click "Add Expense"      | Navigates to `/expenses/create`, sheet slides in from right        | fixture-managed |
| Submit empty form        | Validation errors on required fields                               | fixture-managed |
| Fill valid data + Submit | Expense created, sheet closes, row appears in table, toast success | fixture-managed |

**Create form fields** (confirmed via live exploration):

- Description (text, required)
- Account (Radix combobox, required) — click trigger → `[role="option"]` items
- Amount (number, required)
- Due Date (custom date picker — click button → calendar popover → Today / Accept / Cancel)
- Frequency Number (number) + Frequency Period (Radix combobox: days/weeks/months/years)
- Category (Radix combobox or text?)
- Accrual fields (Accrual Mode Radix combobox — "None", "Advance", "Arrears"; conditionally reveals additional fields)
- Notes / Tags (if present — not fully confirmed)

**Accrual field note:** When Accrual Mode ≠ "None", additional fields appear (accrual amount, accrual period). Requires two sub-tests (no-accrual and with-accrual paths).

### 6.3 Row Actions (Expense)

| Action                          | Expected Outcome                                                                                     | Classification  |
| ------------------------------- | ---------------------------------------------------------------------------------------------------- | --------------- |
| "Mark as Paid"                  | Paid Date column populates, row status changes, toast success                                        | fixture-managed |
| "Toggle Exclusion" (exclude)    | Row gains visual indicator (`border-l-4 border-l-slate-400`), "Excluded" badge appears in row, toast | fixture-managed |
| "Toggle Exclusion" (re-include) | Visual indicator removed, toast                                                                      | fixture-managed |
| "Edit"                          | Navigates to `/expenses/edit/:id`, sheet opens pre-filled                                            | fixture-managed |
| "Duplicate"                     | Navigates to `/expenses/create?duplicate=:id`, sheet opens pre-filled with same data                 | fixture-managed |
| "Delete"                        | Confirmation dialog; on confirm, row removed + toast                                                 | fixture-managed |
| Viewer row actions              | "Open menu" button not present / all actions disabled                                                | parallel-safe   |

**Row action locators:**

- Open menu: `page.getByRole('button', { name: 'Open menu' })` scoped to row
- Actions: `page.getByRole('menuitem', { name: 'Mark as Paid' })` etc.
- Excluded row: `page.locator('tr').filter({ has: page.getByText('Excluded') })`

---

## 7. Income Page

### 7.1 Page State

| Operation                            | Expected Outcome                                                                                           | Classification                   |
| ------------------------------------ | ---------------------------------------------------------------------------------------------------------- | -------------------------------- |
| Load `/incomes` (empty, no accounts) | Empty state, "Add Income" button disabled                                                                  | parallel-safe                    |
| Load `/incomes` (with data)          | Table with columns: Description, Account, Amount, Expected Date, Frequency, Received Date, Status, Actions | parallel-safe (with seeded data) |
| Search by description                | Filters table rows                                                                                         | fixture-managed                  |
| Account filter                       | Same behaviour as Expenses (multi-account only, `?accountId=` URL param)                                   | fixture-managed                  |

**Search locator:** `page.getByLabel('Search incomes by description')` (expected — mirror of expenses pattern)  
**Add button:** `page.getByLabel('Add a new income')`

### 7.2 Create Income

| Operation                | Expected Outcome                                    | Classification  |
| ------------------------ | --------------------------------------------------- | --------------- |
| Click "Add Income"       | Navigates to `/incomes/create`, sheet opens         | fixture-managed |
| Submit empty form        | Validation errors                                   | fixture-managed |
| Fill valid data + Submit | Income created, row appears in table, toast success | fixture-managed |

**Create form fields** (confirmed via live exploration — no accrual fields):

- Description (text, required)
- Account (Radix combobox, required)
- Amount (number, required)
- Expected Date (custom date picker)
- Frequency Number + Frequency Period (Radix combobox)

### 7.3 Row Actions (Income)

| Action                                                     | Expected Outcome                                                     | Classification  |
| ---------------------------------------------------------- | -------------------------------------------------------------------- | --------------- |
| "Mark as Received" (unpaid)                                | Received Date populates, row status changes, toast                   | fixture-managed |
| "Mark as Paid" (received state variant — label may differ) | Reverts received status or confirms payment                          | fixture-managed |
| "Edit"                                                     | Navigates to `/incomes/edit/:id`, sheet opens pre-filled             | fixture-managed |
| "Duplicate"                                                | Navigates to `/incomes/create?duplicate=:id`, sheet opens pre-filled | fixture-managed |
| "Delete"                                                   | Confirmation dialog; on confirm row removed + toast                  | fixture-managed |
| Viewer row actions                                         | All disabled                                                         | parallel-safe   |

> **Note:** Income row action label for marking received may be "Mark as Received" (not "Mark as Paid"). Confirm exact label from live data.

---

## 8. Users Page

### 8.1 Page State

| Operation               | Expected Outcome                                                        | Classification |
| ----------------------- | ----------------------------------------------------------------------- | -------------- |
| Load `/users` as admin  | Table with columns: Username, Email, Status, Last Login, Roles, Actions | parallel-safe  |
| Load `/users` as viewer | Table visible, row action menus absent                                  | parallel-safe  |

### 8.2 Change Role

| Operation                                | Expected Outcome                                           | Classification  |
| ---------------------------------------- | ---------------------------------------------------------- | --------------- |
| Admin clicks "Open menu" → "Change Role" | Dialog opens with title "Change Role", role options listed | fixture-managed |
| Select role + Submit                     | User role updated, row updated, toast                      | fixture-managed |

### 8.3 Disable / Enable User

| Operation                   | Expected Outcome                                                         | Classification  |
| --------------------------- | ------------------------------------------------------------------------ | --------------- |
| Admin clicks "Disable User" | Immediate action (no confirmation dialog), user status → disabled, toast | fixture-managed |
| Admin clicks "Enable User"  | Immediate action, user status → enabled, toast                           | fixture-managed |

### 8.4 Invite User

| Operation                   | Expected Outcome                                           | Classification  |
| --------------------------- | ---------------------------------------------------------- | --------------- |
| Admin clicks "Invite User"  | Navigates to `/users/invite`, sheet opens with invite form | fixture-managed |
| Submit empty form           | Validation errors on required fields                       | fixture-managed |
| Fill valid data + Submit    | Invite sent/user created, toast; sheet closes              | fixture-managed |
| Viewer "Invite User" button | Button disabled (`aria-disabled`)                          | parallel-safe   |

**Invite form fields** (confirmed via live exploration):

- Username (text, required)
- Email (text, required)
- Role (Radix combobox — "Admin" / "Viewer")

**Validation observed:**

- Username already taken → form-level error
- Invalid email → form-level error

---

## 9. Projections Page

### 9.1 View and Filter Controls

| Operation                             | Expected Outcome                                                                      | Classification                                               |
| ------------------------------------- | ------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| Load `/projections`                   | Chart visible, view selector (5 options), period buttons, date picker, account filter | parallel-safe (empty data) / isolated-sequence (seeded data) |
| Change view selector option           | Chart re-renders for selected view                                                    | isolated-sequence                                            |
| Click period button (e.g., "30 days") | Chart updates for selected period                                                     | isolated-sequence                                            |
| Change date via date picker           | Chart shifts to selected start date                                                   | isolated-sequence                                            |
| Select account from account filter    | Chart filters to that account                                                         | isolated-sequence                                            |

**View selector options** (Radix combobox, confirmed via live exploration):

- Option 1–5 (exact labels not captured — recommend re-checking source)

**Locators:**

- Account filter: `page.getByLabel('Filter by account')`
- Period buttons: `page.getByRole('button', { name: /30 days|60 days|90 days|6 months|1 year/i })`
- Date picker: `page.getByRole('button', { name: /\d{4}/ })` (button displaying current date)

---

## 10. Export Dialog

| Operation                    | Expected Outcome                                    | Classification    |
| ---------------------------- | --------------------------------------------------- | ----------------- |
| Click "Export..." in sidebar | Dialog opens with single export confirmation button | isolated-sequence |
| Confirm export               | `.export` file downloaded, dialog closes, toast     | isolated-sequence |
| Viewer has no "Export..."    | Sidebar item absent                                 | parallel-safe     |

**Export locators:**

- Sidebar link: `page.getByRole('link', { name: 'Export...' })` (or button via onClick sidebar item)
- Dialog: `page.getByRole('dialog')`
- Confirm button: `page.getByRole('button', { name: /export/i })` inside dialog

---

## 11. Import Dialog

| Operation                                 | Expected Outcome                                              | Classification    |
| ----------------------------------------- | ------------------------------------------------------------- | ----------------- |
| Click "Import..." in sidebar              | Dialog opens with "Browse" file picker + "Import Data" button | isolated-sequence |
| Select valid `.export` file + Import Data | Import completes, toast success, data visible                 | isolated-sequence |
| Select invalid file + Import Data         | Error feedback (toast or inline error)                        | isolated-sequence |
| Viewer has no "Import..."                 | Sidebar item absent                                           | parallel-safe     |

**Import locators:**

- Sidebar link: `page.getByRole('link', { name: 'Import...' })` (or onClick item)
- Dialog: `page.getByRole('dialog')`
- File input: `page.locator('input[type="file"]')` inside dialog
- Import button: `page.getByRole('button', { name: 'Import Data' })`

---

## 12. Settings Dialog (User Dropdown)

| Operation                                | Expected Outcome                         | Classification |
| ---------------------------------------- | ---------------------------------------- | -------------- |
| Click user display name (top of sidebar) | Dropdown menu with "Settings", "Log Out" | parallel-safe  |
| Click "Settings"                         | Dialog opens with accordion sections     | parallel-safe  |

**Settings dialog sections** (confirmed via live exploration):

1. **Site Details** — Display Name, Site Name fields (admin only? confirm permission)
2. **User Details** — username/email display or edit fields
3. **Change Password** — Current Password, New Password, Confirm Password
4. **Budget Reminders** — toggle/config for reminder emails

| Section          | Operation                             | Classification                             |
| ---------------- | ------------------------------------- | ------------------------------------------ |
| Site Details     | Update site display name              | fixture-managed                            |
| User Details     | Update user profile (username/email?) | fixture-managed                            |
| Change Password  | Submit valid new password             | fixture-managed                            |
| Change Password  | Submit mismatched new/confirm         | parallel-safe (no state change on failure) |
| Budget Reminders | Toggle on/off                         | fixture-managed                            |

**Settings locators:**

- User menu trigger: `page.getByRole('button', { name: 'e2e_admin' })` (or user display name)
- Settings item: `page.getByRole('menuitem', { name: 'Settings' })`
- Dialog: `page.getByRole('dialog')`
- Accordion: `page.getByRole('button', { name: 'Site Details' })` etc.

---

## 13. Pending Approvals Page

| Operation                               | Expected Outcome                                                                           | Classification |
| --------------------------------------- | ------------------------------------------------------------------------------------------ | -------------- |
| Admin navigates to `/approvals/pending` | Page renders, `GET /api/approvals/pending` succeeds (empty list when no pending approvals) | parallel-safe  |
| Viewer: "Approvals" not in sidebar      | Sidebar item absent for viewer role                                                        | parallel-safe  |
| Unauthenticated access                  | Redirect to `/login`                                                                       | parallel-safe  |

> **Note (potentially resolved):** During discovery, `GET /api/approvals/pending` returned 403 for `e2e_admin` because the user's RowId (`58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5`) was absent from the `PlatformAdmin:UserIds` configuration. Fix applied: RowId added to `PLATFORM_ADMIN_USERIDS` in `.env.development` and `--PlatformAdmin:UserIds` in `playwright.config.ts`. The Approvals sidebar item should now appear for `e2e_admin` and the API call should succeed.

---

## 14. Role Comparison Summary

| Feature Area                        | Admin                   | Viewer                                |
| ----------------------------------- | ----------------------- | ------------------------------------- |
| Quick Actions (dashboard)           | ✓ visible + clickable   | ✗ not rendered                        |
| Add / Edit / Delete Accounts        | ✓ buttons enabled       | ✗ `disabled` + `aria-disabled="true"` |
| Add / Edit / Delete Expenses        | ✓ enabled               | ✗ disabled                            |
| Add / Edit / Delete Income          | ✓ enabled               | ✗ disabled                            |
| Row action menus (Expenses/Income)  | ✓                       | ✗ menus not rendered                  |
| Invite User                         | ✓ enabled               | ✗ disabled                            |
| User row action menus               | ✓                       | ✗ not rendered                        |
| Export... sidebar item              | ✓                       | ✗ absent                              |
| Import... sidebar item              | ✓                       | ✗ absent                              |
| Approvals sidebar item              | ✓ (platform admin only) | ✗ absent                              |
| Settings dialog                     | ✓                       | ✓ (some sections may differ)          |
| Projections (read)                  | ✓                       | ✓                                     |
| Accounts / Expenses / Income (read) | ✓                       | ✓                                     |
| Users table (read)                  | ✓                       | ✓                                     |

**Viewer disabled-control pattern:**

- `<button>`: `disabled` attribute + `aria-disabled="true"` → `.isDisabled()` returns true
- Menu items: `data-disabled=""` + `aria-disabled="true"` → cannot be clicked without `{ force: true }`

---

## 15. Mobile / Responsive Behaviour

| Behaviour                         | Detail                                                                     |
| --------------------------------- | -------------------------------------------------------------------------- |
| Sidebar at ≤ ~768px wide viewport | Collapsed by default; nav links not visible                                |
| Toggle Sidebar button             | Visible in header when sidebar is collapsed; click reveals sidebar overlay |
| Export / Import sidebar items     | Hidden at mobile viewport regardless of permissions                        |
| After clicking Toggle Sidebar     | Sidebar opens and links become visible again                               |

**Mobile locators:**

- Toggle button: `page.getByRole('button', { name: 'Toggle Sidebar' })`
- After toggle: `page.getByRole('link', { name: 'Accounts' })` etc.

---

## 16. Key Locator Reference

| Element                      | Recommended Locator                                                         |
| ---------------------------- | --------------------------------------------------------------------------- |
| Toast notification           | `page.locator('[data-sonner-toast]')`                                       |
| Form field validation error  | `page.locator('[data-slot="form-message"]')`                                |
| Modal dialog                 | `page.getByRole('dialog')`                                                  |
| Sheet (slide-in panel)       | `page.locator('[data-slot="sheet-content"]')`                               |
| Row open-menu button         | `page.getByRole('button', { name: 'Open menu' })` (scoped to row)           |
| Menu item (inside open menu) | `page.getByRole('menuitem', { name: '...' })`                               |
| Sidebar nav link             | `page.getByRole('link', { name: '...' })`                                   |
| Custom combobox (Radix)      | Click trigger text → `page.getByRole('option', { name: '...' })`            |
| Custom date picker           | Click button with date text → calendar appears with Today / Cancel / Accept |
| Account filter combobox      | `page.getByLabel('Filter by account')`                                      |
| Search input (expenses)      | `page.getByLabel('Search expenses by description')`                         |
| Search input (income)        | `page.getByLabel('Search incomes by description')`                          |
| Clear search button          | `page.getByLabel('Clear search input')`                                     |
| Add account button           | `page.getByLabel('Add a new account')`                                      |
| Add expense button           | `page.getByLabel('Add a new expense')`                                      |
| Add income button            | `page.getByLabel('Add a new income')`                                       |
| Toggle sidebar button        | `page.getByRole('button', { name: 'Toggle Sidebar' })`                      |
| POT logo / home link         | `page.getByLabel('Navigate to Pay On Time homepage')`                       |
| Error boundary alert         | `page.getByRole('alert')` (full-screen fallback)                            |

---

## 17. Validation Patterns

| Pattern                                | Details                                                                                          |
| -------------------------------------- | ------------------------------------------------------------------------------------------------ |
| Login — invalid credentials            | `[role="alert"]` element inside form containing the error text                                   |
| Create/Edit forms — required fields    | `[data-slot="form-message"]` elements appear below each invalid field                            |
| Create/Edit forms — submit with errors | Form stays open; error messages visible                                                          |
| Invite User — duplicate username       | Form-level error (confirmed observed)                                                            |
| Password change — mismatch             | Form-level error on "Confirm Password" field                                                     |
| Toast — success operations             | `[data-sonner-toast]` with descriptive text (e.g., "Account created", "Income Renewal Complete") |
| Disabled controls                      | `aria-disabled="true"` on both buttons and Radix menu items                                      |

> **Resolved:** `role="alert"` added to the login inline error element in `LoginForm.tsx`. Use `page.getByRole('alert')` scoped within the login card for reliable assertion.

---

## 18. Test Classification Summary

### 18.1 Parallel-Safe Tests (no state change)

- Login page renders (fields, buttons, links visible)
- Login submit disabled when fields empty
- Login with wrong credentials → inline error (transient session, context closed immediately)
- Forgot Password dialog opens, Send Reset Code disabled when empty
- Sign Up dialog opens, Send Verification Code disabled when empty
- Dashboard loads as admin (sections visible, Quick Actions present)
- Dashboard loads as viewer (Quick Actions absent, mutating buttons disabled)
- Sidebar links visible for both roles (correct items, correct absence)
- Accounts page empty state
- Expenses page empty state (with no accounts: "Add Expense" disabled with tooltip)
- Income page empty state
- Users table loads (columns present)
- Projections page loads (controls visible)
- Pending Approvals page accessible by URL (platform admin configured — 403 potentially resolved)
- Viewer: mutating buttons disabled (aria-disabled pattern)
- Viewer: Export/Import absent from sidebar
- Unauthenticated access → redirect to `/login`
- Unknown route → redirect to `/dashboard`
- Mobile: sidebar collapsed by default, toggle button present
- Column headers visible (Accounts, Expenses, Income, Users tables)
- Settings dialog accessible via user menu

### 18.2 Fixture-Managed Tests (create own records, clean up in afterEach)

All grouped in `test.describe.serial()` blocks with `afterEach` cleanup:

- Account CRUD (create valid, create invalid/validation, edit, delete)
- Expense CRUD (create valid — no accrual, create valid — with accrual, create invalid, edit, delete)
- Expense row actions: Mark as Paid, Toggle Exclusion (include / exclude cycle), Duplicate
- Income CRUD (create valid, create invalid, edit, delete)
- Income row actions: Mark as Received, Duplicate
- Invite User (valid, duplicate username error, invalid email error)
- Change User Role
- Disable/Enable User
- Settings: update Site Details, User Details, Change Password, Budget Reminders toggle
- Account filter (requires two accounts + expenses/income spanning both)
- Search filter on Expenses / Income

### 18.3 Isolated-Sequence Tests (separate npm script, workers: 1, Mode 3 seed)

- Dashboard overview values match known seeded data amounts
- Quick Actions: Income Renewal → toast + data update
- Quick Actions: Expense Accrual → toast + data update
- Quick Actions: Budget Reminder → toast
- Quick Actions: Income Confirmation → toast
- Projections chart with seeded data — view selector options, period buttons, date filter
- Export: download `.export` file (admin)
- Import: upload `.export` file → data appears in UI (admin)

---

## 19. Deferred / Not Yet Explored

| Area                                                   | Notes                                                                                                            |
| ------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------- |
| Sign Up step 2 (verification code entry)               | Requires real email or OTP test strategy (see PRD D7)                                                            |
| Password Reset step 2 (code + new password)            | Same dependency as above                                                                                         |
| Approvals page with platform:manage permission         | `e2e_admin` does not hold `platform:manage`; need platform admin user or permission grant                        |
| Column sorting behaviour                               | Sort buttons confirmed present in table headers; sort direction change and API re-fetch not verified             |
| Pagination behaviour                                   | No pagination observed with current data volume; need 20+ rows to trigger                                        |
| Projections view selector — exact option labels        | Source not read in detail; recommend `src/features/projections/ProjectionsPage.tsx`                              |
| Income row action label verification                   | "Mark as Received" vs "Mark as Paid" label on income rows — confirm exact text from source                       |
| Settings dialog — which sections are viewer-accessible | Not tested with viewer; check `WithPermission` guards on each accordion panel                                    |
| Error boundary fallback                                | `[role="alert"]` full-screen error — not triggered during exploration                                            |
| `/logout` route behaviour                              | Route exists in code (`LogoutRoute` component); behaviour: calls `logoutManager.logout()` → redirect to `/login` |

---

## 20. Recommended Product-Code Testability Improvements

These are minimal changes that would improve E2E locator reliability. They have no runtime behaviour impact.

| Element                   | Gap                               | Recommendation                                                              |
| ------------------------- | --------------------------------- | --------------------------------------------------------------------------- |
| Quick Action cards        | No accessible role or testid      | Add `role="button"` or `data-testid="quick-action-{slug}"` to each card     |
| Login inline error        | ~~No `[role="alert"]` or testid~~ | **Resolved** — `role="alert"` added to the error `<div>` in `LoginForm.tsx` |
| Projections view selector | Labels unknown without code read  | Verify label text in source; add `data-testid` if labels are dynamic        |

---

## Appendix A: Data Strategy by Phase

| Phase             | Seed Mode                            | Data Present                      | Tests Applicable                                   |
| ----------------- | ------------------------------------ | --------------------------------- | -------------------------------------------------- |
| Blank             | Mode 1 (migrations only)             | No site, no users                 | Not used by current tests                          |
| Baseline (Gate A) | Mode 2 (migrations + `baseline.sql`) | Site + 2 users, no financial data | All parallel-safe + fixture-managed                |
| Seeded            | Mode 3 (Mode 2 + import artifact)    | Site + users + financial data     | Isolated-sequence + projections + dashboard values |

## Appendix B: Form Sheet vs Dialog Pattern

| Feature             | Component             | URL Change        | Locator                       |
| ------------------- | --------------------- | ----------------- | ----------------------------- |
| Create/Edit Account | Centered modal dialog | Yes (route child) | `getByRole('dialog')`         |
| Create/Edit Expense | Right-side sheet      | Yes (route child) | `[data-slot="sheet-content"]` |
| Create/Edit Income  | Right-side sheet      | Yes (route child) | `[data-slot="sheet-content"]` |
| Invite User         | Right-side sheet      | Yes (route child) | `[data-slot="sheet-content"]` |
| Export dialog       | Centered modal dialog | No (state-driven) | `getByRole('dialog')`         |
| Import dialog       | Centered modal dialog | No (state-driven) | `getByRole('dialog')`         |
| Forgot Password     | Centered modal dialog | No (state-driven) | `getByRole('dialog')`         |
| Sign Up             | Centered modal dialog | No (state-driven) | `getByRole('dialog')`         |
| Settings            | Centered modal dialog | No (state-driven) | `getByRole('dialog')`         |
| Change Role         | Centered modal dialog | No (state-driven) | `getByRole('dialog')`         |

> **Sheet vs dialog navigation note:** When a right-side sheet is open and occupies the overlay, navigating away (`page.goto(url)`) is more reliable than pressing Escape to close it before the next action.

## Appendix C: Radix Custom Control Interaction Patterns

```typescript
// Custom combobox (Radix Select / Combobox) — NOT a native <select>
// DO NOT use .selectOption()
await page.getByLabel("Account").click(); // opens dropdown
await page.getByRole("option", { name: "Main Checking" }).click();

// Custom date picker
await page.getByRole("button", { name: /Jan.*2026/ }).click(); // opens calendar
await page.getByRole("button", { name: "Today" }).click(); // select today
await page.getByRole("button", { name: "Accept" }).click(); // confirm

// Radix dropdown menu
await page.getByRole("button", { name: "Open menu" }).click(); // opens context menu
await page.getByRole("menuitem", { name: "Edit" }).click(); // select action
```
