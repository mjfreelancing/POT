# README Updates for Authentication & Approval System

## Changes Required

Replace or update the following sections in README.md to reflect the new approval workflow and login changes.

---

## 1. Update "User Registration & Account Creation" Section

Replace the existing section with:

### User Registration & Account Creation

POT provides a comprehensive user registration system that combines email verification with platform administrator approval, ensuring secure onboarding of new users.

#### Signup Process

The registration process follows a secure **four-phase workflow**:

1. **User Information Collection**

   - Username and email address input
   - Real-time username availability checking
   - Form validation with clear error messaging

2. **Email Verification**

   - Dual-code OTP system for enhanced security
   - Reference Code (6 digits) - displayed for verification
   - Verification Code (6 digits) - entered by user
   - 15-minute expiry window for security
   - Resend functionality with countdown timer

3. **Platform Admin Notification**

   - Automatic email sent to all platform administrators
   - Contains user details (username, email)
   - Link to pending approvals page for quick action
   - Admins notified immediately upon successful verification

4. **Approval & Account Activation**
   - User status set to `Approval` (awaiting admin decision)
   - Platform admin reviews signup via `/approvals/pending` page
   - Admin can **Approve** (enables account) or **Reject** (locks username)
   - Upon approval:
     - User status changes to `Enabled`
     - Approval email sent with temporary password
     - User can now log in to the application

#### Login Flow with Approval Status

**Status-Based Login Response:**

The login system uses a 200 OK response with a status field instead of HTTP error codes:

```typescript
type LoginResponse = {
  status: "Success" | "Approval";
  accessToken?: string;
  refreshToken?: string;
  message?: string;
};
```

**Login Scenarios:**

1. **Successful Login** (`status: 'Success'`)

   - User has `Enabled` status
   - Receives access and refresh tokens
   - Redirected to dashboard
   - Normal application access

2. **Pending Approval** (`status: 'Approval'`)
   - User has `Approval` status (awaiting admin approval)
   - No tokens issued
   - Informative message displayed:
     > "Your account is currently awaiting approval from a platform administrator. You will receive an email once your account has been approved."
   - User can safely close the dialog
   - Email will be sent when approved

**Key Features:**

- **No 401 errors**: Status-based responses prevent error interceptor issues
- **Clear messaging**: Users know exactly why they can't log in
- **Type-safe**: Exhaustive type checking ensures all statuses are handled
- **User-friendly**: Approval message displayed prominently with clock icon

#### Email Verification System

**Welcome Email Features:**

- HTML email with plain text fallback for accessibility
- Clear visual design with step-by-step instructions
- Reference Code display for verification matching
- Verification Code for entry
- Security notices and expiry information

**Platform Admin Notification Email:**

- Sent to all platform administrators when a user signs up
- Contains:
  - New user's username and email
  - Direct link to `/approvals/pending` page
  - Information about the approval workflow
- Subject: "New User Signup - [Username]"

**User Approval Email:**

- Sent when a platform admin approves the user
- Contains:
  - Welcome message with account activation notice
  - Temporary password for first login
  - Link to login page
  - Instructions for password reset on first login
- Subject: "Account Approved - Welcome to POT"

#### Security Features

**Username Protection:**

- Unique username validation across the system
- Race condition handling between registration and verification
- Real-time availability checking during signup
- **Username locking**: Rejected signups permanently lock the username

**OTP Security:**

- 6-digit codes with 1-million possible combinations
- 15-minute expiry window
- Single-use verification (codes invalidate after use)
- Automatic cleanup of expired OTPs
- Rate limiting on failed verification attempts

**Approval Security:**

- Platform-level permissions required (`platform:manage`)
- Approval/rejection actions fully audited
- Approval emails contain temporary passwords for secure first login
- ETag-based concurrency control for approval actions

#### User Experience Features

**Intelligent Error Handling:**

- Username taken detection with auto-navigation back to form
- Clear error messages with actionable guidance
- Inline validation feedback
- Graceful handling of network issues

**Approval Status Communication:**

- Clear messaging during signup about approval requirement
- Prominent approval notice on login attempt
- Email notifications at each stage (signup, approval)
- Instructions for next steps in all communications

**Accessibility Support:**

- Screen reader compatible error messages
- Keyboard navigation throughout the flow
- High contrast code displays in emails
- Plain text email fallback

**Progress Management:**

- Visual progress indicators during signup
- State preservation during dialog interactions
- Cancel and restart functionality
- Clear success confirmation

#### Getting Started After Signup

Once registration and approval are complete:

1. **Receive Approval Email:** Contains your temporary password
2. **First Login:** Use your username and the temporary password
3. **Password Reset:** Set a permanent password during your first login
4. **Site Customization:** Access Settings → Site Settings to customize your site name
5. **Account Setup:** Add your bank accounts and financial information
6. **Data Entry:** Begin tracking your income, expenses, and financial goals

The signup system is designed to balance security and user experience, ensuring that only authorized users gain access while maintaining a smooth onboarding process.

---

## 2. Add New "Platform Administration" Section

Insert this new section after "User Management":

### Platform Administration

POT includes a platform-level administration system for managing user signups and system-wide operations. Platform administrators have elevated permissions that transcend individual site boundaries.

#### Platform Administrator Setup

**Configuration via Environment Variables:**

Platform administrators are identified using globally unique identifiers (GUIDs) stored in an environment variable:

```bash
# In .env or docker-compose environment
PLATFORM_ADMIN_USERIDS=guid1,guid2,guid3
```

**Key Features:**

- **Multi-tenant compatible**: Platform admins can manage users across all sites
- **Secure identification**: Uses immutable user GUIDs instead of usernames
- **Environment-based**: Configuration via environment variables (no database dependency)
- **Multiple admins**: Comma-separated list supports multiple platform administrators

**Obtaining User GUIDs:**

User GUIDs can be found:

- In the `User` table's `RowId` column
- Via the `/me` API endpoint (returns current user's GUID)
- In application logs during user operations

#### Pending Approvals Management

**Accessing the Approvals Page:**

- Navigate to **Platform → Approvals** in the sidebar menu
- Requires `platform:manage` permission
- Only visible to platform administrators
- URL: `/approvals/pending`

**Approvals Page Features:**

1. **Pending Users Table**

   - **Username**: The requested username
   - **Email**: User's email address
   - **Actions**: Approve or Reject dropdown menu

2. **Real-time Updates**

   - Table automatically refreshes after approval/rejection
   - Cache invalidation ensures consistent data
   - Success toasts confirm actions

3. **Empty State**
   - Friendly message when no pending approvals exist
   - "No pending approvals - All user signups have been processed"

#### Approval Workflow

**Approving a User:**

1. Click the actions menu (three dots) for the pending user
2. Select **Approve**
3. System performs:
   - Changes user status from `Approval` → `Enabled`
   - Sends approval email with temporary password
   - Removes user from pending approvals list
   - Updates users list with new active user
4. Success toast confirms: "User Approved - [username] has been approved."

**Rejecting a User:**

1. Click the actions menu (three dots) for the pending user
2. Select **Reject**
3. System performs:
   - Changes user status from `Approval` → `Disabled`
   - **Permanently locks the username** (cannot be reused)
   - Removes user from pending approvals list
   - No email sent to rejected user
4. Success toast confirms: "User Rejected - [username] has been rejected."

**Important Notes:**

- **No confirmation dialogs**: Actions are immediate (consistent with other admin operations)
- **Username locking**: Rejected usernames become permanently unavailable
- **Audit trail**: All approval/rejection actions are logged with timestamps and admin user IDs
- **Concurrency control**: ETag-based optimistic locking prevents conflicts

#### Platform Permissions

**`platform:manage` Permission:**

- Required for all platform admin operations
- Transcends site boundaries (uses `.IgnoreQueryFilters()`)
- Automatically granted to users listed in `PLATFORM_ADMIN_USERIDS`
- Cannot be assigned via role management (environment-only)

**Permission Scope:**

- View pending approvals across all sites
- Approve/reject user signups
- Access platform-level reporting (future feature)
- Manage system-wide settings (future feature)

**Security Considerations:**

- Platform permissions are separate from site permissions
- A user can be both a platform admin and a site admin
- Platform admin status is checked on every request
- Environment changes require application restart

#### Integration with User Management

**User Status Lifecycle:**

1. **Pending**: Invited by site admin, awaiting first login
2. **Approval**: Signed up independently, awaiting platform admin approval
3. **Enabled**: Active user, can log in and use the application
4. **Disabled**: Account suspended or rejected

**Platform Admin View of Users:**

- Platform admins can see users from all sites (when using platform features)
- Site-level user management still respects site boundaries
- Approval page uses platform-level queries
- Regular user management uses site-filtered queries

#### Email Templates

**Admin Notification Email** (sent on signup):

- Template: `ApprovalEmail.razor` (HTML) + `ApprovalEmail.text` (plain text)
- Sent to: All platform administrators
- Contains: Username, email, link to approvals page
- Trigger: Successful OTP verification during signup

**User Approval Email** (sent when approved):

- Template: _(To be implemented)_
- Sent to: The approved user
- Contains: Approval confirmation, temporary password, login link
- Trigger: Platform admin approves user

#### API Endpoints

**Platform Admin Endpoints:**

```typescript
// Get all pending approvals
GET /api/approvals/pending
Response: { username, email, rowId, etag }[]
Requires: platform:manage permission

// Update pending user status
PUT /api/approvals/{userId}/status
Body: { etag, status: 'Approved' | 'Rejected' }
Requires: platform:manage permission
Actions:
  - Approved: Status → Enabled, sends approval email
  - Rejected: Status → Disabled, locks username
```

**Implementation Notes:**

- All platform endpoints use `/api/approvals/*` prefix
- Status update uses `Approved`/`Rejected` actions (distinct from user statuses)
- ETag required for all mutations (optimistic concurrency)
- 403 Forbidden returned for non-platform admins

#### Future Enhancements

**Planned Features:**

- Separate "Rejected" status (instead of "Disabled")
- Filtering rejected users from normal user lists
- Signup date tracking in approvals table
- Site name display in approvals list
- Bulk approval/rejection operations
- Approval workflow history and audit log
- Platform admin dashboard with metrics

---

## 3. Update "Security and Data Privacy" Section

Add this subsection under "Security Considerations":

#### Platform-Level Security

**Environment-Based Platform Admin Configuration:**

```bash
# Platform administrator configuration (Docker Compose)
environment:
  - PLATFORM_ADMIN_USERIDS=00000000-0000-0000-0000-000000000001,00000000-0000-0000-0000-000000000002
```

**Security Features:**

- **Immutable identification**: Uses GUIDs instead of usernames (username changes don't affect admin status)
- **Environment-only**: Cannot be changed via API or database manipulation
- **Startup validation**: Invalid GUIDs logged at application startup
- **Multi-admin support**: Multiple platform administrators supported
- **Transparent caching**: Platform admin status cached for performance

**Permission Isolation:**

- Platform permissions (`platform:*`) separate from site permissions (`site:*`, `user:*`, etc.)
- Platform admins can still have site-specific roles
- `.IgnoreQueryFilters()` used for platform operations (bypasses site isolation)
- All platform actions fully audited

**Username Security:**

- Usernames are globally unique across all sites
- Rejected signups permanently lock usernames
- Prevents username squatting and impersonation
- Case-insensitive username comparison (CITEXT database type)

---

## 4. Update Authentication Architecture Section

Update the "Authentication Flow" subsection (around line 2607) to include:

#### Login Status Handling

**Status-Based Authentication Response:**

POT's login system uses a status-based response pattern instead of traditional HTTP error codes:

```typescript
type LoginResponse = {
  status: "Success" | "Approval";
  accessToken?: string;
  refreshToken?: string;
  message?: string;
};
```

**Implementation Benefits:**

1. **Prevents Interceptor Issues**: 200 OK responses avoid triggering Axios error interceptors
2. **Type Safety**: Exhaustive type checking ensures all statuses are handled
3. **Clear User Messaging**: Status-specific messages inform users of their account state
4. **Extensible**: Easy to add new statuses (e.g., `PasswordExpired`, `LockedOut`)

**Frontend Handling:**

```typescript
// LoginPage.tsx pattern
const handleLogin = async (credentials) => {
  const response = await loginMutation.mutateAsync(credentials);

  if (response.status === "Approval") {
    setApprovalMessage(response.message);
    return; // Early return, show approval message
  }

  if (response.status === "Success") {
    if (response.accessToken && response.refreshToken) {
      login(response.accessToken, response.refreshToken);
      navigate("/dashboard");
      return;
    }
    // Handle missing tokens
  }

  // Exhaustive check ensures all statuses handled
  const _exhaustive: never = response.status;
};
```

**Backend Implementation:**

```csharp
// Login endpoint returns 200 OK with status field
if (user.Status == UserStatus.Approval)
{
    return Results.Ok(new LoginResponse
    {
        Status = LoginStatus.Approval,
        Message = "Your account is currently awaiting approval..."
    });
}

// Success case returns tokens
return Results.Ok(new LoginResponse
{
    Status = LoginStatus.Success,
    AccessToken = accessToken,
    RefreshToken = refreshToken
});
```

**Error Cases:**

- Invalid credentials: Still returns 401 Unauthorized (handled normally)
- Account disabled: Returns 403 Forbidden with error message
- Server errors: Returns 500 Internal Server Error

---

## 5. Additional Documentation Files

Consider creating these supplementary documentation files:

1. **`PLATFORM_ADMIN_SETUP.md`**

   - Detailed guide for setting up platform administrators
   - Step-by-step GUID retrieval instructions
   - Environment variable configuration examples
   - Troubleshooting common issues

2. **`APPROVAL_WORKFLOW.md`**

   - Detailed flowcharts of the approval process
   - Email template documentation
   - API endpoint specifications
   - Frontend component architecture

3. **`AUTHENTICATION_GUIDE.md`**
   - Comprehensive authentication flow diagrams
   - Status-based response pattern explanation
   - Token refresh mechanism details
   - Security best practices

---

## Summary of Changes

**Major Updates:**

1. ✅ Expanded user registration section with approval workflow
2. ✅ Added complete platform administration section
3. ✅ Documented status-based login responses
4. ✅ Added platform-level security information
5. ✅ Documented new permission system (`platform:manage`)

**Key New Sections:**

- Platform Administrator Setup
- Pending Approvals Management
- Approval Workflow
- Platform Permissions
- Login Status Handling

**Benefits:**

- Complete documentation of the approval system
- Clear guidance for platform administrator setup
- Comprehensive security and permission documentation
- User-focused explanation of signup and login experience
