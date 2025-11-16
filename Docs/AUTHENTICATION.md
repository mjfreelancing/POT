# Authentication & Security

Comprehensive guide to POT's authentication system, user signup workflow, and security features.

## Table of Contents

- [Overview](#overview)
- [User Signup Workflow](#user-signup-workflow)
- [Email Verification](#email-verification)
- [Platform Admin Approval](#platform-admin-approval)
- [Login Flow](#login-flow)
- [JWT Tokens](#jwt-tokens)
- [Permissions System](#permissions-system)
- [Platform Admin Configuration](#platform-admin-configuration)
- [Security Features](#security-features)

---

## Overview

POT uses a multi-stage authentication system combining:

1. **Email Verification** - Verify email ownership with OTP codes
2. **Platform Admin Approval** - Manual approval of new signups by platform administrators
3. **JWT Authentication** - Token-based authentication for API requests
4. **Role-Based Permissions** - Granular access control with `resource:action` format

**User Journey:**

```
Signup → Email Verification → Pending Approval → Admin Approves → Login → Access Application
```

**Security Layers:**

- Email verification prevents fake accounts
- Admin approval ensures only legitimate users gain access
- JWT tokens secure API communication
- Permissions control what users can do

---

## User Signup Workflow

### Phase 1: User Information Collection

**Location:** `/signup` page

**Process:**

1. User enters username, email, and password
2. Frontend validates:
   - Username format and length
   - Email format
   - Password strength
3. Real-time username availability checking
4. Form validation with clear error messaging

**Backend Validation:**

- Username uniqueness check (case-insensitive)
- Email format validation
- Password complexity requirements
- Race condition handling for concurrent signups

### Phase 2: Email Verification

**Process:**

1. Backend generates two 6-digit codes:
   - **Reference Code** - Displayed in verification screen for user reference
   - **Verification Code** - Sent via email, user enters this
2. Verification email sent to user with both codes
3. User enters verification code in frontend form
4. Backend validates code and marks email as verified
5. User status changes from `Pending` to `Approval`

**Security Features:**

- Codes expire after 15 minutes
- Single-use codes (invalidated after verification)
- Automatic cleanup of expired codes
- Rate limiting on verification attempts

**Resend Functionality:**

- Countdown timer prevents spam
- New codes generated on resend
- Previous codes invalidated

### Phase 3: Platform Admin Notification

**Process:**

1. After successful email verification, system sends notification email to all platform administrators
2. Email contains:
   - New user's username and email
   - Link to `/approvals/pending` page
   - Approval workflow instructions

**Platform Admin Email Template:**

- Subject: "New User Signup - [Username]"
- Template: `ApprovalEmail.razor` (HTML) + `ApprovalEmail.text` (plain text)
- Accessible formatting with clear call-to-action

### Phase 4: Approval & Account Activation

**Platform Admin Actions:**

1. Navigate to **Platform → Approvals** page
2. View pending signups in table format
3. Choose action from dropdown menu:
   - **Approve** - Enable account and send approval email
   - **Reject** - Disable account and lock username

**Approval Action:**

- User status: `Approval` → `Enabled`
- Approval email sent with temporary password
- User removed from pending approvals list
- User can now log in

**Rejection Action:**

- User status: `Approval` → `Disabled`
- Username permanently locked (prevents reuse)
- No email sent to rejected user
- User removed from pending approvals list

**Security Considerations:**

- ETag-based optimistic locking prevents concurrent approval conflicts
- All actions logged with admin user ID and timestamp
- No confirmation dialogs (consistent with other admin operations)

---

## Email Verification

### Dual-Code System

**Why Two Codes?**

- **Reference Code** - User sees this in the verification screen to confirm correct email
- **Verification Code** - User enters this to prove email ownership

**Code Properties:**

- 6 digits each
- 1 million possible combinations per code
- 15-minute expiry window
- Single-use (invalidated after successful verification)

### Email Template

**Welcome Email Features:**

- HTML email with plain text fallback
- Clear visual design with step-by-step instructions
- Reference Code display for verification matching
- Verification Code for entry
- Security notices and expiry information
- Accessible formatting for screen readers

**Template Location:**

- Razor template: `WelcomeEmail.razor`
- Plain text: `WelcomeEmail.text`

### Verification Process

**Frontend:**

1. User receives email with codes
2. Enters verification code in form
3. Backend validates and confirms
4. User sees "Pending Approval" message

**Backend:**

1. Validate code format and expiry
2. Check code matches user's stored code
3. Mark email as verified
4. Update user status to `Approval`
5. Send platform admin notifications
6. Invalidate used code

**Error Handling:**

- Invalid code: Clear error message, allow retry
- Expired code: Prompt to resend
- Too many attempts: Rate limiting with timeout

---

## Platform Admin Approval

### Approval Workflow

**Accessing Approvals:**

- Navigate to **Platform → Approvals** in sidebar
- Requires `platform:manage` permission
- Only visible to platform administrators

**Pending Approvals Table:**

- **Username** - Requested username
- **Email** - User's email address
- **Actions** - Dropdown menu (Approve or Reject)

**Approval Process:**

1. Click actions menu (⋮) for pending user
2. Select **Approve**
3. System performs:
   - Changes status: `Approval` → `Enabled`
   - Sends approval email with temporary password
   - Removes from pending list
   - Adds to users list
4. Success toast: "User Approved - [username] has been approved."

**Rejection Process:**

1. Click actions menu (⋮) for pending user
2. Select **Reject**
3. System performs:
   - Changes status: `Approval` → `Disabled`
   - Permanently locks username
   - Removes from pending list
   - No email sent
4. Success toast: "User Rejected - [username] has been rejected."

### Real-Time Updates

- Table refreshes after approval/rejection
- Cache invalidation ensures data consistency
- Success toasts confirm actions
- Empty state shown when no pending approvals

### Username Locking

**Purpose:** Prevent username squatting and malicious re-registration attempts

**Behavior:**

- Rejected usernames cannot be reused
- Prevents same username from signing up again
- Protects against impersonation attempts
- Permanent lock (no expiration)

---

## Login Flow

### Status-Based Authentication Response

POT uses a status-based response pattern instead of HTTP error codes:

```typescript
type LoginResponse = {
  status: "Success" | "Approval";
  accessToken?: string;
  refreshToken?: string;
  message?: string;
};
```

**Why This Pattern?**

1. Prevents Axios interceptor issues (200 OK doesn't trigger error handlers)
2. Type-safe with exhaustive checking
3. Clear user messaging for each status
4. Extensible for future statuses (e.g., `PasswordExpired`, `LockedOut`)

### Login Scenarios

**Scenario 1: Successful Login**

- User status: `Enabled`
- Response: `{ status: 'Success', accessToken, refreshToken }`
- User redirected to dashboard
- Tokens stored in AuthContext
- Normal application access

**Scenario 2: Pending Approval**

- User status: `Approval`
- Response: `{ status: 'Approval', message: '...' }`
- No tokens issued
- Informative message displayed:
  > "Your account is currently awaiting approval from a platform administrator. You will receive an email once your account has been approved."
- User can safely close dialog
- Email sent when approved

**Scenario 3: Invalid Credentials**

- Response: `401 Unauthorized`
- Error message: "Invalid username or password"
- Standard error handling

**Scenario 4: Disabled Account**

- Response: `403 Forbidden`
- Error message: Account-specific reason
- User cannot log in

### Frontend Handling

```typescript
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
  }

  // Exhaustive check ensures all statuses handled
  const _exhaustive: never = response.status;
};
```

### Backend Implementation

```csharp
// Check user status before issuing tokens
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

---

## JWT Tokens

### Token Structure

**Access Token:**

- Short-lived (typically 15-60 minutes)
- Contains user claims (ID, username, permissions)
- Used for API authentication
- Sent in `Authorization: Bearer <token>` header

**Refresh Token:**

- Long-lived (typically 7-30 days)
- Used to obtain new access tokens
- Stored securely (HttpOnly cookies in production)
- Rotated on use for security

### Token Claims

**Standard Claims:**

- `sub` (subject) - User ID (GUID)
- `unique_name` - Username
- `email` - User email
- `exp` (expiration) - Token expiry timestamp
- `iat` (issued at) - Token creation timestamp

**Custom Claims:**

- `permissions` - Array of permission strings (`resource:action`)
- `site_id` - Site ID for multi-tenant filtering

### Token Lifecycle

**Generation:**

1. User logs in successfully
2. Backend validates credentials
3. Backend generates both tokens
4. Tokens returned in login response
5. Frontend stores in AuthContext

**Usage:**

1. Frontend includes access token in API requests
2. Backend validates token signature
3. Backend checks expiration
4. Backend extracts claims for authorization
5. Request proceeds if valid

**Refresh:**

1. Access token expires
2. Frontend detects 401 Unauthorized
3. Frontend calls refresh endpoint with refresh token
4. Backend validates refresh token
5. Backend issues new access token
6. Frontend retries original request with new token

**Security Features:**

- HMACSHA512 signing algorithm
- 128-character secret key (configured in environment)
- Token expiration enforcement
- Refresh token rotation
- Secure storage (no localStorage for tokens)

---

## Permissions System

### Permission Format

**Pattern:** `resource:action`

**Examples:**

- `account:view` - View accounts
- `account:manage` - Create, update, delete accounts
- `expense:view` - View expenses
- `expense:manage` - Manage expenses
- `platform:manage` - Platform administration

**Lowercase Convention:**

- All permissions are lowercase
- Use hyphens for multi-word resources (e.g., `import-export:manage`)

### Permission Hierarchy

**Roles:**

1. **Admin** - Full access to site features

   - `site:manage`, `site:view`
   - `user:manage`, `user:view`
   - `account:manage`, `account:view`
   - `expense:manage`, `expense:view`
   - `income:manage`, `income:view`
   - `maintenance:export`, `maintenance:import`

2. **Viewer** - Read-only access

   - `account:view`
   - `expense:view`
   - `income:view`

3. **Platform Admin** (via environment variable)
   - `platform:manage` - Added to any existing role permissions
   - Transcends site boundaries
   - Can approve/reject signups across all sites

### Frontend Permission Checking

**PermissionGuard Component:**

```tsx
// Hide content from unauthorized users
<PermissionGuard permissions={["expense:manage"]}>
  <DeleteButton />
</PermissionGuard>

// Multiple permissions (all required)
<PermissionGuard permissions={["expense:manage", "expense:view"]} mode="all">
  <RestrictedContent />
</PermissionGuard>

// Multiple permissions (any required)
<PermissionGuard permissions={["expense:manage", "expense:view"]} mode="any">
  <RestrictedContent />
</PermissionGuard>
```

**WithPermission Component:**

```tsx
// Disable button without permission
<WithPermission permissions={["account:manage"]}>
  <Button>Delete Account</Button>
</WithPermission>
```

### Backend Permission Checking

**Endpoint Protection:**

```csharp
// Require specific permission
app.MapGet("/api/expenses", GetExpenses)
    .RequirePermission("expense:view");

// Require multiple permissions
app.MapPost("/api/expenses", CreateExpense)
    .RequirePermission("expense:manage");
```

**Service Layer:**

```csharp
// Check permissions in business logic
if (!_permissionService.HasPermission("expense:manage"))
{
    return Results.Forbid();
}
```

### Adding New Permissions

**Process:**

1. Add permission to database via migration
2. Assign to appropriate roles
3. Update `.github/copilot-instructions.md` documentation
4. Use in frontend permission components
5. Protect backend endpoints

**Example Migration:**

```sql
INSERT INTO "Permission" ("Name", "Description")
VALUES ('new-feature:manage', 'Manage new feature');

INSERT INTO "RolePermission" ("RoleId", "PermissionId")
SELECT r."Id", p."Id"
FROM "Role" r, "Permission" p
WHERE r."Name" = 'Admin' AND p."Name" = 'new-feature:manage';
```

---

## Platform Admin Configuration

### Environment-Based Configuration

**Platform administrators are identified via environment variables, not database roles.**

**Development (Docker):**

Edit `Source/Docker/.env.development`:

```bash
# Platform Admin Configuration
# Comma-separated list of user GUIDs (RowIds)
PLATFORM_ADMIN_USERIDS=550e8400-e29b-41d4-a716-446655440000
```

**Production (Azure):**

Add environment variable in Azure App Service:

```
Name: PlatformAdmin__UserIds
Value: 550e8400-e29b-41d4-a716-446655440000,6ba7b810-9dad-11d1-80b4-00c04fd430c8
```

**Note:** Azure uses double underscore (`__`) for nested configuration sections.

### Getting User GUIDs

**Method 1: Database Query**

```sql
SELECT "RowId", "Username", "Email"
FROM "User"
WHERE "Username" = 'your_username';
```

**Method 2: API Endpoint**

- Call `/me` endpoint after login
- Response includes user GUID

### How It Works

1. Platform admin user IDs configured via `PLATFORM_ADMIN_USERIDS` environment variable
2. When user authenticates, `PermissionService` checks if user ID matches configured list
3. If matched, `platform:manage` permission added to user's existing permissions
4. Platform admins still need normal database roles (Admin or Viewer)
5. `platform:manage` permission grants access to `/platform/*` endpoints

**Security Benefits:**

- Uses GUIDs instead of usernames (prevents enumeration)
- No database footprint (can't be changed via application)
- Environment-specific (different admins for dev/staging/production)
- Multiple admins supported (comma-separated list)
- Transparent caching for performance

### Platform Admin Capabilities

**What `platform:manage` Grants:**

- View pending approvals across all sites
- Approve/reject user signups
- Access platform-level endpoints (bypass site isolation)
- Future: Platform-level reporting and system administration

**Regular Permissions (From Database Role):**

- Platform admins should be assigned **Admin** role in database
- Provides normal site-level permissions
- `platform:manage` is additive (enhances existing permissions)

### Multiple Platform Admins

**Single Admin:**

```bash
PLATFORM_ADMIN_USERIDS=550e8400-e29b-41d4-a716-446655440000
```

**Multiple Admins:**

```bash
PLATFORM_ADMIN_USERIDS=550e8400-e29b-41d4-a716-446655440000,6ba7b810-9dad-11d1-80b4-00c04fd430c8,7c9e6679-7425-40de-944b-e07fc1f90ae7
```

**Spaces Trimmed:**

```bash
# These are equivalent:
PLATFORM_ADMIN_USERIDS=guid1,guid2
PLATFORM_ADMIN_USERIDS=guid1, guid2
PLATFORM_ADMIN_USERIDS= guid1 , guid2
```

**Invalid GUIDs Ignored:**

```bash
# Only valid GUIDs recognized
PLATFORM_ADMIN_USERIDS=valid-guid,invalid-guid,not-a-guid
```

### Configuration Changes

**Updating Platform Admins:**

1. Update environment variable
2. Restart application
3. Changes take effect immediately

**Removing Platform Admin:**

1. Remove GUID from environment variable
2. Restart application
3. User loses `platform:manage` permission (keeps database role permissions)

### Troubleshooting

**Issue:** Platform admin not working after configuration

**Solutions:**

- Verify environment variable name: `PLATFORM_ADMIN_USERIDS` (local) or `PlatformAdmin__UserIds` (Azure)
- Verify GUID format (36 characters with hyphens)
- Check GUID matches database `RowId` exactly (case-insensitive)
- Restart application after config changes
- Check application logs for "identified as platform admin" message

**Logging:**
When platform admin authenticates, log entry appears:

```
[INF] User with ID '550e8400-e29b-41d4-a716-446655440000' identified as platform admin - granting platform:manage permission
```

---

## Security Features

### Password Requirements

**Minimum Requirements:**

- Minimum length (configurable, typically 8 characters)
- Must contain uppercase letter
- Must contain lowercase letter
- Must contain number
- Must contain special character

**Password Hashing:**

- BCrypt hashing algorithm
- Salted and hashed passwords
- Never stored in plain text
- Computationally expensive to crack

### Email Verification

**Purpose:**

- Verify email ownership
- Prevent fake accounts
- Ensure user can receive notifications

**Security:**

- Time-limited codes (15 minutes)
- Single-use codes
- Rate limiting
- Automatic cleanup

### Account Approval

**Purpose:**

- Manual review of new signups
- Prevent spam accounts
- Control user base growth

**Security:**

- Platform admin-only access
- Username locking on rejection
- Audit trail of approvals/rejections

### Session Management

**Features:**

- Token-based authentication (no server-side sessions)
- Automatic token expiration
- Refresh token rotation
- Logout invalidates tokens

**Inactivity Timeout:**

- Access tokens expire (short-lived)
- Refresh tokens expire (long-lived)
- User must log in again after token expiration

### CORS Configuration

**Purpose:**

- Prevent unauthorized domains from accessing API
- Allow only trusted origins

**Configuration:**

```csharp
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5175") // Frontend URL
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("content-disposition")); // For file downloads
```

### Correlation IDs

**Purpose:**

- Trace authentication requests across system
- Debug authentication issues
- Audit security events

**Usage:**

- Generated for each auth request
- Included in logs
- Helps correlate frontend and backend events

### Rate Limiting

**Applied To:**

- Login attempts
- Email verification attempts
- Password reset requests
- API requests

**Protection Against:**

- Brute force attacks
- Spam
- Resource exhaustion

---

## Best Practices

### For Users

1. **Use strong passwords** - Follow password requirements, use password manager
2. **Verify email promptly** - Check spam folder if email not received
3. **Wait for approval** - Don't attempt to log in until approved
4. **Change temporary password** - Immediately after first login
5. **Log out when done** - Especially on shared devices

### For Administrators

1. **Review signups promptly** - Don't leave users waiting
2. **Verify user identity** - Check email domains, contact if suspicious
3. **Document rejections** - Keep record of why users were rejected
4. **Secure platform admin GUIDs** - Treat as sensitive credentials
5. **Monitor approval activity** - Review logs regularly

### For Developers

1. **Never hardcode credentials** - Use environment variables
2. **Validate all inputs** - Backend validation is mandatory
3. **Use Result pattern** - Don't throw exceptions for auth failures
4. **Check permissions consistently** - Frontend and backend
5. **Log security events** - Authentication, authorization, failures
6. **Test auth flows** - Verify all scenarios (success, failure, edge cases)

---

## Additional Resources

- [Platform Admin Setup Guide](FIRST-TIME-SETUP.md) - Step-by-step first-time configuration
- [Getting Started](GETTING-STARTED.md) - Complete setup guide
- [Frontend Developer Guide](../Source/Client/pot-react/DEVELOPER.md) - Frontend auth patterns
- [Backend Developer Guide](../Source/Server/DEVELOPER.md) - Backend auth implementation

---

**Related Topics:**

- [User Management](USER-GUIDE/Users.md) - Managing users after approval
- [Approvals](USER-GUIDE/Approvals.md) - Platform admin approval workflow
- [Settings](USER-GUIDE/Settings.md) - Password changes and profile updates
