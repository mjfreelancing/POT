# Authentication & Security

Comprehensive guide to POT's authentication system, user signup workflow, and security features.

## Table of Contents

- [Overview](#overview)
- [User Signup Workflow](#user-signup-workflow)
- [Email Verification](#email-verification)
- [Platform Admin Approval](#platform-admin-approval)
- [Login Flow](#login-flow)
- [Logout Flow](#logout-flow)
- [Password Reset Flow](#password-reset-flow)
- [Change Password Flow](#change-password-flow)
- [JWT Tokens](#jwt-tokens)
- [Token Refresh Flow](#token-refresh-flow)
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
  message?: string;
};

// Note: refreshToken is set as HTTP-only cookie, not in response body
```

**Why This Pattern?**

1. Prevents Axios interceptor issues (200 OK doesn't trigger error handlers)
2. Type-safe with exhaustive checking
3. Clear user messaging for each status
4. Extensible for future statuses (e.g., `PasswordExpired`, `LockedOut`)

### Login Scenarios

**Scenario 1: Successful Login**

- User status: `Enabled`
- Response: `{ status: 'Success', accessToken }`
- Refresh token set as HTTP-only cookie (automatic, not in response body)
- User redirected to dashboard
- Access token stored in AuthContext memory
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
const handleLogin = async (values: LoginCredentials) => {
  setAuthError(null);
  setApprovalMessage(null);
  const controller = new AbortController();

  try {
    const result = await loginMutation.mutateAsync({
      data: values,
      signal: controller.signal,
    });

    if (result.success) {
      const response = result.value;

      if (response.status === "Approval") {
        setApprovalMessage(response.message);
        return;
      }

      if (response.status === "Success") {
        if (response.accessToken) {
          // Store access token in memory
          // Refresh token already set as HTTP-only cookie by backend
          login(response.accessToken);
          invalidateCache(["me"]); // Force immediate user info fetch
          navigate("/");
          return;
        }
      }
    } else {
      // Handle error result
      if (result.error instanceof AuthenticationError) {
        setAuthError({
          title: result.error.code,
          description: result.error.description,
        });
      }
    }
  } finally {
    controller.abort();
  }
};
```

### Backend Implementation

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(
    Request request,
    HttpContext httpContext,
    IAuthService authService,
    IOptions<AuthenticationOptions> authOptions,
    IProblemDetailsInspector problemDetailsInspector,
    CancellationToken cancellationToken)
{
    var authResult = await authService.LoginAsync(
        request.Username.Trim(),
        request.Password,
        cancellationToken
    );

    if (!authResult.IsSuccess)
    {
        // Authentication failed - bad credentials
        return TypedResults.Problem(authResult.Error!.ToProblemDetails());
    }

    if (authResult.Value is null)
    {
        // Successful credential validation but account pending approval
        return Response.Approval(
            "Your account is pending approval. You'll receive an email when your account is activated."
        );
    }

    // Successful login - set refresh token as HTTP-only cookie
    RefreshTokenCookieHelper.SetRefreshTokenCookie(
        httpContext,
        authResult.Value.RefreshToken,
        authOptions
    );

    return Response.Success(authResult.Value);
}
```

---

## Logout Flow

### Overview

Logout is a two-phase process that clears both client-side and server-side authentication state.

### Process

**Step 1: Call Logout Endpoint**

1. Frontend calls `POST /api/auth/logout`
2. Access token sent in `Authorization` header (for authentication)
3. Backend invalidates refresh token in database
4. Backend clears HTTP-only refresh token cookie

**Step 2: Clear Client State**

1. Frontend removes access token from memory
2. Frontend clears user info and permissions from state
3. Frontend clears all React Query cache (prevents data leakage)
4. User redirected to login page

### Backend Implementation

**`Logout/Handler.cs`:**

```csharp
public static async Task<IResult> Invoke(
    HttpContext httpContext,
    IHttpUserService userService,
    IAuthService authService,
    IOptions<AuthenticationOptions> authOptions,
    CancellationToken cancellationToken)
{
    var userInfo = await userService.GetMeInfoAsync(httpContext, cancellationToken);

    if (userInfo is not null)
    {
        _ = await authService.LogoutAsync(userInfo.RowId, cancellationToken);
    }

    // Clear HTTP-only refresh token cookie
    RefreshTokenCookieHelper.ClearRefreshTokenCookie(httpContext, authOptions);

    return Results.Ok();
}

// AuthService.LogoutAsync implementation:
user.RefreshToken = null;
user.RefreshTokenExpiryUtc = null;
user.TokenVersion++; // Invalidate all existing access tokens
```

**What `LogoutAsync` Does:**

- Sets user's `RefreshToken` field to `null` in database
- Sets `RefreshTokenExpiryUtc` to `null`
- Increments `TokenVersion` to invalidate all existing access tokens
- Any subsequent refresh attempts will fail (even if attacker has old cookie)

**What `ClearRefreshTokenCookie` Does:**

- Sets cookie value to empty string
- Sets `MaxAge` to 0
- Sets `Expires` to past date
- Browser immediately deletes the cookie

### Frontend Implementation

**`AuthContext.tsx`:**

```typescript
const logout = useCallback(async () => {
  // Call server logout endpoint first (need access token)
  try {
    await logoutMutation.mutateAsync({});
    logger.info("Auth", "Logged out from the server");
  } catch (error) {
    logger.error("Auth", "Error while logging out from the server", error);
  }

  // Clear client-side state
  setTokens(undefined); // Remove access token from memory
  userStore.clearUserInfo(); // Clear user info and permissions
  queryClient.clear(); // Clear all cached API data
}, [setTokens, userStore, logoutMutation, queryClient]);
```

### Security Considerations

**Why Clear Cache?**

- React Query caches all API responses
- Cache may contain sensitive user data
- Clearing cache prevents data leakage if another user logs in

**Why Invalidate Database Token?**

- HTTP-only cookie is browser-controlled
- Attacker might have extracted cookie through other means
- Database invalidation ensures stolen cookies become useless

**Race Condition Handling:**

- If logout API call fails, client still clears local state
- User appears logged out even if server-side logout failed
- Next API request will fail (no valid access token)
- Attempting to refresh will fail (database token invalidated on success, or cookie cleared on failure)

### Logout Scenarios

**Scenario 1: Normal Logout**

1. User clicks logout button
2. Server invalidates database token and clears cookie
3. Client clears memory and cache
4. User redirected to login page
5. ✅ Complete cleanup

**Scenario 2: Logout API Fails**

1. User clicks logout button
2. Server request fails (network error, server down)
3. Client still clears memory and cache
4. Cookie might still exist, but no access token to use it
5. ⚠️ Partial cleanup - user still effectively logged out on client

**Scenario 3: Forced Logout (Token Expired)**

1. Refresh token expires (30 days)
2. Auto-refresh attempt fails
3. Frontend detects failure, triggers logout
4. Same cleanup as normal logout
5. ✅ Graceful handling of token expiration

### Logout Manager Pattern

POT uses a centralized `logoutManager` for consistent logout behavior across the app:

```typescript
// Instead of calling logout directly
const { logout } = useAuthContext();
logout(); // ❌ Creates dependency issues

// Use logout manager
import logoutManager from "@/features/auth/logoutManager";
logoutManager.logout(); // ✅ Centralized, no circular dependencies
```

**Why This Pattern?**

- Prevents circular dependencies in token refresh logic
- Provides consistent logout behavior everywhere
- Allows logout from anywhere (e.g., token refresh error, route guards)

---

## Password Reset Flow

### Overview

Password reset allows users who forgot their password to regain account access. Uses a dual-code OTP system similar to email verification.

### Process

**Phase 1: Request Password Reset**

1. User enters username in password reset dialog
2. Frontend calls `POST /api/auth/password-reset/send`
3. Backend generates two 6-digit codes:
   - **Reference Code** - Returned in API response AND sent in email
   - **Verification Code** - Sent in same email
4. Codes expire after 15 minutes
5. Backend sends an email containing both codes
6. Frontend displays reference code returned from API
7. User enters verification code from email

**Phase 2: Verify and Reset**

1. User receives email with codes and temporary password
2. User enters verification code in dialog
3. Frontend calls `POST /api/auth/password-reset/verify` with:
   - Username
   - Reference code
   - Verification code
4. Backend validates codes and user
5. Backend updates user's password to the temporary password (already generated in Phase 1)
6. Frontend shows success message with next steps
7. User can now log in with temporary password from the email

**Phase 3: Change Temporary Password**

1. User logs in with temporary password
2. User navigates to Settings → Change Password
3. User sets permanent password
4. System invalidates old tokens (forces re-login)

### Security Features

**Why Two Codes?**

- Reference code confirms user received correct email
- Verification code proves email ownership
- Assists the user identify which verification code to use if they request multiple resets. Only the last verification code sent will work.

**Code Expiry:**

- Both codes expire after 15 minutes
- Single-use codes (invalidated after use)
- Reduces window of opportunity for attackers

**No Username Validation:**

- API always returns 200 OK (even for invalid usernames)
- Prevents username enumeration attacks
- Attackers can't determine which usernames exist

**Temporary Password:**

- Randomly generated secure password
- Sent via email (user's verified contact method)
- Forces user to set permanent password

### Backend Implementation

**Request Password Reset (`Send/Handler.cs`):**

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(
    Request request,
    HttpContext httpContext,
    IRequestPasswordResetService passwordResetService,
    CancellationToken cancellationToken)
{
    var correlationId = httpContext.Request.TryGetCorrelationId(out var id)
        ? id
        : httpContext.TraceIdentifier;

    var referenceCode = await passwordResetService.RequestResetAsync(input, cancellationToken);

    // Always return 200 OK - don't reveal if username exists
    // Returns reference code immediately (even for invalid usernames - security)
    return Response.Ok(referenceCode);
}

// RequestPasswordResetService implementation:
// - Generates both reference code and verification code
// - Sends email immediately with BOTH codes
// - Email template shows both codes for user convenience
```

**Verify Password Reset (`Verify/Handler.cs`):**

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(
    Request request,
    IVerifyPasswordResetService passwordResetService,
    CancellationToken cancellationToken)
{
    var output = await passwordResetService.VerifyResetAsync(input, cancellationToken);

    return output.IsSuccess
        ? Response.Ok(output.Value)
        : TypedResults.Problem(output.Error.ToProblemDetails());
}
```

**`VerifyResetAsync` performs:**

1. Validates codes haven't expired
2. Validates codes match database records
3. Updates user's password to the temporary password hash (generated in Phase 1)
4. Marks OTP codes as used
5. No email sent (user already has temporary password from Phase 1 email)

### Frontend Implementation

**`PasswordResetDialog.tsx`:**

```typescript
const handleUsernameSubmit = async (username: string) => {
  updateUsername(username);

  const result = await sendPasswordReset(username);

  if (result.success) {
    // Store reference code and move to OTP verification
    updateReferenceCode(result.value.referenceCode);
    goToOtpVerification();
  } else {
    // Show error to user
    onError({
      title: result.error.code,
      description: result.error.description,
    });
  }
};

const handleVerificationSubmit = async (verificationCode: string) => {
  const result = await verifyPasswordReset({
    username: data.username,
    referenceCode: data.referenceCode,
    verificationCode,
  });

  if (result.success) {
    goToSuccess(); // Show success message with next steps
  } else {
    // Handle specific error cases (expired, invalid, rate limited)
    handleVerificationError(result.error);
  }
};
```

### User Journey

**Happy Path:**

1. User clicks "Forgot Password?" on login page
2. Enters username → Receives reference code on screen
3. Checks email → Finds verification code AND temporary password
4. Enters verification code → Success message displayed
5. Logs in with temporary password from email
6. Changes password in settings
7. Uses new password for future logins

**Error Scenarios:**

- **Invalid username:** Always shows success (security)
- **Codes expired:** Clear message, option to request new codes
- **Too many attempts:** Rate limited, shows retry time
- **Invalid verification code:** Clear error, allows retry

### Email Template

**Change Password Email (sent during Phase 1):**

- Subject: "Password Reset Request - POT"
- Contains reference code (for user verification)
- Contains verification code (for form submission)
- Contains temporary password (generated immediately, usable after verification)
- Includes expiry time (15 minutes for codes)
- Instructions to verify codes, then log in with temporary password
- Security notice about unsolicited requests

---

## Change Password Flow

### Overview

Allows authenticated users to change their password. Requires current password for security. Available in user settings.

### Process

1. User navigates to Settings (user menu or `/settings`)
2. User fills out change password form:
   - Current password (for verification)
   - New password
   - Confirm new password (must match)
3. Frontend validates passwords match
4. Frontend calls `POST /me/change-password`
5. Backend validates current password
6. Backend updates password in database
7. Backend invalidates refresh token (forces re-login)
8. Frontend shows success dialog
9. User clicks "Sign In with New Password" → Automatically logged out
10. User logs in with new password

### Security Features

**Current Password Required:**

- Prevents unauthorized password changes
- Protects if user leaves device unlocked
- Verifies user identity before critical change

**Automatic Logout After Change:**

- Invalidates all existing sessions
- Forces re-authentication with new password
- Prevents stolen tokens from remaining active

**Password Validation:**

- Minimum length (typically 8 characters)
- Requires uppercase, lowercase, number, special character
- Password strength indicator
- Prevents common/weak passwords

### Backend Implementation

**`ChangePassword/Handler.cs`:**

```csharp
public static async Task<Results<Ok, ProblemHttpResult>> Invoke(
    HttpContext httpContext,
    Request request,
    IHttpUserService userService,
    IAuthService authService,
    CancellationToken cancellationToken)
{
    var userInfo = await userService.GetMeInfoAsync(httpContext, cancellationToken);

    if (userInfo is null)
    {
        return CreateInvalidUserOrPasswordError();
    }

    var passwordChanged = await authService.ChangePasswordAsync(
        userInfo.RowId,
        request.CurrentPassword,
        request.NewPassword,
        cancellationToken
    );

    return passwordChanged.IsSuccess
        ? TypedResults.Ok()
        : CreateInvalidUserOrPasswordError();
}

// AuthService.ChangePasswordAsync implementation:
user.PasswordHash = _passwordHasher.GetHash(user, newPassword);
user.RefreshToken = null;
user.RefreshTokenExpiryUtc = null;
user.TokenVersion++; // Invalidate all existing access tokens
```

**`ChangePasswordAsync` performs:**

1. Hashes provided current password
2. Compares with database password hash
3. If match: Hashes new password and updates database
4. Sets `RefreshToken` to `null` and `RefreshTokenExpiryUtc` to `null`
5. Increments `TokenVersion` to invalidate all existing access tokens
6. Returns success or error

**Why Invalidate Refresh Token?**

- Old sessions should not remain active after password change
- User might be changing password due to security concern
- Forces all devices to re-authenticate with new password

### Frontend Implementation

**`ChangePasswordForm.tsx`:**

```typescript
async function onSubmit(values: ChangePasswordFields) {
  setError(null);

  const result = await changePassword({
    currentPassword: values.currentPassword,
    newPassword: values.newPassword,
  });

  if (result && !result.success) {
    // Show error in error sheet
    setError({
      title: result.error.code,
      description: result.error.description,
    });
    return;
  }

  if (result && result.success) {
    // Show success dialog (blocks UI)
    setShowPasswordChangedDialog(true);
  }
}

function handleLogoutAfterPasswordChange() {
  // Logout manager handles complete cleanup
  logoutManager.logout();
}
```

**`PasswordChangedDialog`:**

- Modal dialog (user must acknowledge)
- Explains password was changed successfully
- Explains automatic logout is about to happen
- "Sign In with New Password" button triggers logout and redirect to login

### Form Validation

**Client-Side (Zod Schema):**

```typescript
const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Current password is required"),
    newPassword: z
      .string()
      .min(8, "Password must be at least 8 characters")
      .regex(/[A-Z]/, "Password must contain uppercase letter")
      .regex(/[a-z]/, "Password must contain lowercase letter")
      .regex(/[0-9]/, "Password must contain number")
      .regex(/[^A-Za-z0-9]/, "Password must contain special character"),
    confirmPassword: z.string().min(1, "Please confirm your password"),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });
```

**Server-Side:**

- BCrypt password hashing
- Current password verification
- New password complexity validation
- Database transaction for atomic update

### Error Handling

**Common Errors:**

- **Invalid Current Password:**

  - Error: "Invalid user or password"
  - User must re-enter correct current password
  - No hints about which is wrong (security)

- **Weak New Password:**

  - Error: Specific requirement not met
  - Frontend shows which rules failed
  - User corrects and resubmits

- **Passwords Don't Match:**
  - Client-side validation catches this
  - Never sent to server

---

## Token Refresh Flow (Proactive)

### Overview

POT implements **proactive token refresh** - tokens are refreshed automatically **before** they expire, not when API calls fail. This ensures uninterrupted user experience with no visible authentication delays.

### Why Proactive Refresh?

**Traditional (Reactive) Approach:**

1. Access token expires
2. User makes API request
3. Request fails with 401
4. Frontend detects failure, calls refresh endpoint
5. Retries original request
6. **User sees delay or error message**

**POT's Proactive Approach:**

1. Timer calculates when token will expire
2. Refresh happens **before expiry** (while token still valid)
3. New token seamlessly replaces old one
4. API requests never fail due to expired tokens
5. **User never notices authentication at all**

### Token Refresh Timer

**Configuration:**

- Access tokens last **15 minutes** (fixed)
- Refresh timer triggers at **12 minutes** (80% of lifespan)
- 3-minute buffer ensures refresh completes before expiry
- Timer automatically resets after each successful refresh

**Calculation (`calculateRefreshTime`):**

```typescript
export function calculateRefreshTime(
  accessToken: string,
  refreshBeforeExpiryPercentage: number = 0.8
): number {
  const decoded = jwtDecode<TokenPayload>(accessToken);
  const expiryTime = decoded.exp * 1000; // Convert to milliseconds
  const currentTime = Date.now();
  const timeUntilExpiry = expiryTime - currentTime;

  // Refresh at 80% of token lifetime
  return Math.floor(timeUntilExpiry * refreshBeforeExpiryPercentage);
}
```

**Example:**

- Token issued at 10:00 AM
- Token expires at 10:15 AM (15 minutes)
- 80% of 15 minutes = 12 minutes
- Refresh timer triggers at 10:12 AM
- New token received at 10:12 AM
- New timer set for 12 minutes (10:24 AM)

### Implementation

**`accessTokenRefreshTimer.ts`:**

```typescript
type AccessTokenRefreshConfig = {
  currentTokens: AuthTokens;
  onRefreshSuccess: (accessToken: string) => void;
  onRefreshError: (error: unknown) => void;
};

type TokenRefreshHandle = {
  start: () => void;
  stop: () => void;
};

function createTokenRefreshTimer({
  currentTokens,
  onRefreshSuccess,
  onRefreshError,
}: TokenRefreshConfig): TokenRefreshHandle {
  let timerId: number | undefined;

  const stopTimer = () => {
    if (timerId) {
      window.clearTimeout(timerId);
      timerId = undefined;
    }
  };

  const startTimer = () => {
    stopTimer(); // Clear any existing timer first

    const refreshTimeMs = calculateRefreshTime(currentTokens.accessToken);

    if (!refreshTimeMs) {
      return;
    }

    logger.info("Auth", "Setting up refresh timer");

    timerId = window.setTimeout(async () => {
      logger.info("Auth", "Refresh timer triggered");

      try {
        const response = await authClient.post<AuthTokens>(
          "/auth/refresh",
          {}, // No body - refresh token from HTTP-only cookie
          {
            headers: {
              Authorization: `Bearer ${currentTokens.accessToken}`,
            },
          }
        );

        logger.info("Auth", "Token refreshed successfully");
        onRefreshSuccess(response.data.accessToken);
      } catch (error) {
        logger.error("Auth", "Failed to refresh token", error);
        onRefreshError(error);
      }
    }, refreshTimeMs);
  };

  return { start: startTimer, stop: stopTimer };
}
```

### Integration with Auth Context

**`AuthContext.tsx`:**

```typescript
const login = useCallback(
  (accessToken: string) => {
    logger.info("Auth", "User logged in");
    setTokens({ accessToken });
  },
  [setTokens]
);

const logout = useCallback(async () => {
  try {
    await logoutMutation.mutateAsync({});
    logger.info("Auth", "Logged out from the server");
  } catch (error) {
    logger.error("Auth", "Error while logging out from the server", error);
  }

  setTokens(undefined);
  userStore.clearUserInfo();
  queryClient.clear(); // Clear all cached data
}, [setTokens, userStore, logoutMutation, queryClient]);

// Setup proactive refresh timer
const refreshTimerRef = useRef<AccessTokenRefreshHandle | undefined>(undefined);

useEffect(() => {
  if (accessToken) {
    refreshTimerRef.current = createAccessTokenRefreshTimer({
      currentAccessToken: accessToken,
      onRefreshSuccess: login, // Reuses login to store new token
      onRefreshError: () => logout(), // Logout on refresh failure
    });

    refreshTimerRef.current.start();

    return () => {
      refreshTimerRef.current?.stop();
    };
  }
}, [tokens, login, logout]);
```

### Backend Refresh Endpoint

**`Refresh/Handler.cs`:**

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(
    HttpContext httpContext,
    [FromHeader(Name = "Authorization")]
    [Description("The access token to be refreshed (optional on initial page load)")]
    string? accessToken,
    IAuthService authService,
    IOptions<AuthenticationOptions> authOptions,
    CancellationToken cancellationToken)
{
    // Get refresh token from HTTP-only cookie
    var refreshToken = RefreshTokenCookieHelper.GetRefreshTokenFromCookie(httpContext, authOptions);

    if (refreshToken.IsNullOrEmpty())
    {
        return AuthUtils.CreateAuthErrorResult();
    }

    // Access token is optional (may not be present on page refresh)
    if (!accessToken.IsNullOrEmpty() && accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        accessToken = accessToken["Bearer ".Length..];
    }

    var authTokens = await authService.RefreshAsync(accessToken, refreshToken!, cancellationToken);

    if (!authTokens.IsSuccess)
    {
        return TypedResults.Problem(authTokens.Error!.ToProblemDetails());
    }

    // Set new refresh token as HTTP-only cookie
    RefreshTokenCookieHelper.SetRefreshTokenCookie(httpContext, authTokens.Value!.RefreshToken, authOptions);

    return Response.Ok(authTokens.Value!);
}
```

**`RefreshAsync` performs:**1. Validates refresh token from cookie exists in database 2. Validates refresh token not expired 3. Validates access token belongs to same user (prevents token swap attacks) 4. Generates new access token (15-minute expiry) 5. Generates new refresh token (30-day expiry) 6. Updates database with new refresh token 7. Returns both tokens

### Refresh Scenarios

**Scenario 1: Normal Proactive Refresh**

- 10:00 AM - User logs in, receives access token
- 10:12 AM - Timer triggers, refreshes token
- 10:24 AM - Timer triggers again, refreshes token
- ✅ User never notices, seamless experience

**Scenario 2: Page Refresh During Session**

- User has valid access token in session storage
- Page refreshed at 10:05 AM
- Token still valid for 10 more minutes
- New refresh timer set for 10:09 AM (80% of remaining time)
- ✅ Refresh cycle continues seamlessly

**Scenario 3: Refresh Token Expired**

- User inactive for 30+ days
- Refresh token expired in database
- Proactive refresh attempt fails
- Frontend detects failure, triggers logout
- ✅ Graceful logout, user returns to login page

**Scenario 4: Multiple Tabs Open**

- User opens app in two browser tabs
- Tab A refreshes at 10:12 AM, gets new tokens
- Tab B still has old access token (not expired yet)
- Tab B makes request at 10:13 AM - **succeeds** (old token still valid)
- Tab B's refresh timer triggers at 10:14 AM - **fails** (refresh token already used)
- Tab B logs out, shows login screen
- ⚠️ Multi-tab support requires additional coordination (not currently implemented)

### Token Versioning

**Why Token Version Claim?**

- JWT `TokenVersion` claim matches database `TokenVersion` field
- When password changes, `TokenVersion` incremented
- Old tokens become invalid even if not expired
- Forces re-authentication after critical changes

**Flow:**

1. User changes password
2. Database `TokenVersion` incremented from 1 to 2
3. Old access token still has `TokenVersion: 1`
4. Next API request validates token version against database
5. Mismatch detected → 401 Unauthorized
6. Proactive refresh attempt also fails (refresh token invalidated)
7. User logged out and redirected to login

### Comparison with Page Refresh Flow

POT has two distinct refresh mechanisms:

| Aspect               | Proactive Refresh                    | Page Refresh                     |
| -------------------- | ------------------------------------ | -------------------------------- |
| **Trigger**          | Timer (before token expires)         | Page load/reload                 |
| **Purpose**          | Keep session alive during active use | Restore session after navigation |
| **Timing**           | Every 12 minutes during use          | On every page load               |
| **Token Source**     | Memory (current access token)        | Session storage (persisted)      |
| **User Impact**      | Invisible background operation       | Brief loading state              |
| **Failure Handling** | Logout and redirect                  | Show login page                  |

**Why Both?**

- Proactive refresh handles **active sessions** (user interacting with app)
- Page refresh handles **session restoration** (user returns after browser tab switch, navigation, etc.)
- Together they provide seamless authentication experience

---

## JWT Tokens

### Token Architecture: HTTP-Only Cookies + In-Memory Storage

POT uses a **dual-storage approach** for maximum security:

**Access Token (In-Memory Only):**

- Short-lived (15 minutes)
- Contains user claims (user ID, token ID, token version)
- Used for API authentication via `Authorization: Bearer <token>` header
- **Stored in React state only** - never persists to disk
- Lost on page refresh (intentionally)

**Refresh Token (HTTP-Only Cookie):**

- Long-lived (30 days)
- Used to obtain new access tokens
- **Stored in HTTP-only cookie** - JavaScript cannot access it
- Automatically sent by browser with requests
- Rotated on use for security

**Why This Architecture?**

This architecture addresses the fundamental security challenge of single-page applications (SPAs):

1. **The localStorage Problem:**

   - Traditional approach: Store both tokens in localStorage
   - **Critical vulnerability:** Any XSS attack can steal tokens from localStorage
   - localStorage is accessible to all JavaScript on the page (including malicious scripts)
   - Compromised tokens = full account access until token expires

2. **The Cookie-Only Problem:**

   - Alternative approach: Store both tokens in cookies
   - Access tokens would need to be readable by JavaScript (to add to API headers)
   - This defeats the security benefit of HTTP-only cookies

3. **Our Solution - Best of Both Worlds:**
   - **Refresh token (long-lived, sensitive)** → HTTP-only cookie
     - JavaScript cannot read it (XSS-proof)
     - Automatic transmission by browser
     - Only server can read and validate
   - **Access token (short-lived, needed by JavaScript)** → In-memory React state
     - Accessible to JavaScript for API requests
     - Lost on page refresh (limits XSS exposure window)
     - Short expiry (15 minutes) limits damage if stolen

### Token Storage Security Comparison

| Storage Location                | Access Token         | Refresh Token        | Security                                                          |
| ------------------------------- | -------------------- | -------------------- | ----------------------------------------------------------------- |
| **localStorage (Old)**          | ❌ Vulnerable to XSS | ❌ Vulnerable to XSS | **Low** - Both tokens stolen by XSS                               |
| **Cookies (Non-HttpOnly)**      | ❌ Vulnerable to XSS | ❌ Vulnerable to XSS | **Low** - JavaScript can read cookies                             |
| **HTTP-Only Cookies (Current)** | N/A                  | ✅ XSS-proof         | **High** - Refresh token completely safe from XSS                 |
| **In-Memory (Current)**         | ⚠️ Lost on refresh   | N/A                  | **Medium** - Time-limited XSS exposure (15 minute token lifetime) |
| **Combined Approach (Current)** | In-Memory            | HTTP-Only Cookie     | **Highest** - Balanced security & usability                       |

### Page Refresh Flow: Automatic Re-Authentication

**The Challenge:**
When a user refreshes the page, all JavaScript memory is cleared, including the access token. Without the access token, the user appears logged out.

**The Solution:**
On every page load, the frontend automatically attempts to get a new access token using the HTTP-only refresh token cookie:

**Step-by-Step Process:**

1. **Page loads** → React app initializes → `AccessTokenContext` component mounts
2. **Check memory:** Is there an access token in React state?
   - **Yes** → User is authenticated, proceed normally
   - **No** → Memory was cleared (page refresh or first visit)
3. **Attempt refresh:** Frontend calls `POST /api/auth/refresh` with **no request body**
   - Browser automatically includes the `pot_refresh_token` HTTP-only cookie
   - Frontend does **not** send the access token (it doesn't have one)
4. **Backend validates refresh token:**
   - Reads `pot_refresh_token` from cookie (`Refresh/Handler.cs`)
   - Looks up user in database using the refresh token value
   - Validates token hasn't expired (checks `RefreshTokenExpiryUtc` column)
5. **Backend generates new tokens:**
   - Creates new access token (JWT with user claims)
   - Creates new refresh token (rotates for security)
   - Sets new refresh token as HTTP-only cookie
   - Returns new access token in response body
6. **Frontend stores new access token:**
   - Saves to React state (in-memory)
   - Syncs with axios interceptor (adds to future API requests)
   - User is now authenticated
7. **User sees dashboard** → Seamless experience, no login required

**Code Implementation:**

**Frontend (`AccessTokenContext.tsx`):**

```typescript
// Runs on every page load
useEffect(() => {
  async function initializeAuth() {
    const existingToken = tokenProvider.getAccessToken();

    if (existingToken) {
      // Already have token in memory, use it
      setAccessToken(existingToken);
      return;
    }

    // No token in memory - try to refresh from cookie
    logger.info(
      "AccessTokenContext",
      "No access token in memory, attempting refresh from cookie"
    );

    const result = await refreshAccessToken(); // Calls /auth/refresh

    if (result.success) {
      // Got new token, store in memory
      setAccessToken(result.value.accessToken);
    } else {
      // No valid refresh cookie - user must log in
      logger.info("AccessTokenContext", "No valid refresh cookie found");
    }
  }

  initializeAuth();
}, []);
```

**Backend (`Refresh/Handler.cs`):**

```csharp
public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(
    HttpContext httpContext,
    string? accessToken,  // Optional - may be null on page refresh
    IAuthService authService,
    IOptions<AuthenticationOptions> authOptions,
    CancellationToken cancellationToken)
{
    // Get refresh token from HTTP-only cookie
    var refreshToken = RefreshTokenCookieHelper.GetRefreshTokenFromCookie(httpContext, authOptions);

    if (refreshToken.IsNullOrEmpty())
    {
        return AuthUtils.CreateAuthErrorResult();
    }

    // Look up user - access token is optional (may be null on page refresh)
    var authTokens = await authService.RefreshAsync(accessToken, refreshToken, cancellationToken);

    if (!authTokens.IsSuccess)
    {
        return TypedResults.Problem(authTokens.Error.ToProblemDetails());
    }

    // Set new refresh token as HTTP-only cookie
    RefreshTokenCookieHelper.SetRefreshTokenCookie(httpContext, authTokens.Value.RefreshToken, authOptions);

    return Response.Ok(authTokens.Value);
}
```

**Backend (`AuthService.cs` - handling null access token):**

```csharp
public async Task<EnrichedResult<AuthTokens?>> RefreshAsync(string? accessToken, string refreshToken, CancellationToken cancellationToken)
{
    Guid userRowId;

    if (accessToken.IsNullOrEmpty())
    {
        // No access token provided (page refresh scenario)
        // Look up user directly from refresh token
        var userFromRefresh = await _userRepository.Users
            .SingleOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);

        if (userFromRefresh is null)
        {
            return CreateAuthError();
        }

        userRowId = userFromRefresh.RowId;
    }
    else
    {
        // Access token provided (normal refresh scenario)
        // Extract user ID from expired access token
        var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
        var subject = principal.Claims.SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);

        if (subject is null || !Guid.TryParse(subject.Value, out userRowId))
        {
            return CreateAuthError();
        }
    }

    // Continue with token generation...
}
```

**Why This Works:**

1. **Browser automatically sends cookies** - No JavaScript required
2. **Server validates cookie** - Checks database for matching refresh token
3. **Server trusts its own cookie** - No need for client-side access token validation
4. **Stateless authentication** - No server-side session storage needed
5. **Seamless UX** - User never sees a login screen on page refresh

**Security Benefits:**

- **XSS Protection:** Even if attacker injects malicious JavaScript, they cannot:
  - Read the refresh token (HTTP-only cookie is invisible to JavaScript)
  - Steal long-term access (access token expires in 15 minutes)
- **CSRF Protection:** `SameSite=Lax` prevents cross-site cookie transmission on POST requests
- **Token Rotation:** Every refresh generates a new refresh token (limits reuse)
- **Time-Limited Exposure:** Access token in memory only exists while browser tab is open

**Trade-offs:**

- ✅ **Gain:** Refresh token completely protected from XSS
- ✅ **Gain:** Automatic re-authentication on page refresh
- ⚠️ **Trade-off:** Access token still vulnerable to XSS (but limited to 15 minute window)
- ⚠️ **Trade-off:** Additional complexity (dual storage approach)

**Why Not Store Access Token in HTTP-Only Cookie Too?**

If we stored the access token in an HTTP-only cookie, JavaScript couldn't read it to add to API request headers. We'd need the server to:

1. Read the cookie
2. Add it to the response headers
3. Have the frontend read the response headers
4. Add to the next request headers

This creates unnecessary complexity and defeats the purpose of stateless JWT authentication.

### Cookie Configuration: Development vs Production

POT uses **environment-aware cookie security** to work seamlessly in both local development and production:

**Development (HTTP on localhost):**

```json
// appsettings.Development.json
{
  "Authentication": {
    "Cookie": {
      "SecureOnly": false // Allows cookies over HTTP
      // Domain not set - cookies restricted to exact origin (localhost:5241)
    }
  }
}
```

**Production (HTTPS on Azure/domain):**

```json
// appsettings.json (or Azure environment variable)
{
  "Authentication": {
    "Cookie": {
      "SecureOnly": true, // Requires HTTPS
      "Domain": ".payontime.com.au" // Enable cross-subdomain cookie sharing
    }
  }
}
```

**Azure Environment Variable:**

```
Authentication__Cookie__Domain = .payontime.com.au
```

**Cookie Properties:**

| Property   | Development | Production          | Purpose                                      |
| ---------- | ----------- | ------------------- | -------------------------------------------- |
| `HttpOnly` | `true`      | `true`              | Prevents JavaScript access (XSS protection)  |
| `Secure`   | `false`     | `true`              | HTTP allowed (dev) / HTTPS required (prod)   |
| `SameSite` | `Lax`       | `Lax`               | CSRF protection, allows same-site subdomains |
| `MaxAge`   | 30 days     | 30 days             | Cookie lifetime                              |
| `Path`     | `/`         | `/`                 | Available to all paths                       |
| `Domain`   | (not set)   | `.payontime.com.au` | Allows subdomain sharing                     |

**Why `SameSite=Lax`?**

`SameSite` controls when browsers send cookies with cross-origin requests:

- **`Strict`** - Cookie **never** sent on cross-origin requests

  - ❌ Blocks all cross-subdomain requests
  - Not suitable for our architecture

- **`Lax`** - Cookie sent on "safe" top-level navigation (GET requests)

  - ✅ Works with `Domain=.payontime.com.au` for cross-subdomain requests
  - ✅ Cookies sent with XHR/fetch when `withCredentials: true` is set
  - ✅ CSRF protection (blocks POST from other sites)
  - ✅ Standard security practice for same-site cookies

- **`None`** - Cookie sent on all cross-origin requests
  - Requires `Secure=true` (HTTPS only)
  - Less CSRF protection
  - Not needed for our architecture

**Security Notes:**

- `SameSite=Lax` combined with:
  - `Secure=true` (HTTPS only)
  - Strict CORS `AllowedOrigins` (only allows `https://payontime.com.au`)
  - HTTP-only cookie (no JavaScript access)
  - `withCredentials: true` in axios configuration
  - Short-lived access tokens (15 minutes)

**Local Development Proxy Setup:**

The frontend uses a **Vite proxy** in development for simplicity and to avoid CORS preflight requests:

**`vite.config.ts`:**

```typescript
export default defineConfig({
  server: {
    proxy: {
      "/api": {
        target: "http://localhost:5242", // Backend server
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
```

**How It Works:**

1. Frontend runs on `http://localhost:5175`
2. Backend runs on `http://localhost:5242`
3. Frontend makes request to `/api/auth/login`
4. Vite proxy forwards to `http://localhost:5242/api/auth/login`
5. Backend sends cookie with `Domain=localhost, SameSite=Lax`
6. Browser accepts cookie (proxy makes request appear from same origin)
7. Future requests to `/api/*` automatically include cookie (withCredentials: true)

**Without Proxy:**

- Frontend: `http://localhost:5175`
- Backend: `http://localhost:5242`
- Different ports = different origins = cookies may not work

**With Proxy:**

- Frontend request: `http://localhost:5175/api/auth/login`
- Browser thinks: Same origin (localhost:5175)
- Vite forwards: `http://localhost:5242/api/auth/login`
- Backend sets cookie: `Domain=localhost, SameSite=Lax, Secure=false`
- Browser accepts: Cookie available for same-origin requests
- ✅ Cookies work seamlessly

**Production (Azure with Custom Domain):**

In production, both containers share a parent domain:

- Client: `https://payontime.com.au`
- API: `https://api.payontime.com.au`
- Cookie: `Domain=.payontime.com.au` (note the leading dot)

**The Leading Dot:**

- `.payontime.com.au` (with dot) = cookie available to all subdomains
- `payontime.com.au` (no dot) = cookie only available to exact domain
- Browser automatically adds leading dot for subdomain sharing

**How Browser Decides to Send Cookie:**

1. Request to `api.payontime.com.au`
2. Check cookie domain: `.payontime.com.au`
3. Does `api.payontime.com.au` match `.payontime.com.au`? Yes (subdomain)
4. Check `SameSite=Lax`: Is this a top-level GET or credentialed request? Yes
5. Check `Secure=true`: Is HTTPS? Yes
6. Check `withCredentials: true`: Set in axios? Yes
7. ✅ Send cookie with request

**Production Configuration in `nginx.azure.conf`:**

Production client container uses nginx without API proxy:

```nginx
server {
    listen 80;
    server_name localhost;

    location / {
        root /usr/share/nginx/html;
        index index.html index.htm;
        try_files $uri $uri/ /index.html;
    }

    # NO /api/ proxy - client calls API directly
    # Frontend built with VITE_API_BASE_URL=https://api.payontime.com.au/api
}
```

**Why No Proxy in Production?**

- Azure Container Apps have separate URLs
- Cannot share Docker network (unlike local docker-compose)
- Must call API directly with full URL
- Cookies still work because of shared parent domain
- HTTPS required for `Secure=true` cookies

### Cookie Troubleshooting

**Problem: "User logged out on page refresh"**

**Cause:** Cookie not being sent to `/api/auth/refresh` endpoint

**Solutions:**

1. **Check cookie exists:**

   - Open DevTools → Application → Cookies
   - Look for `pot_refresh_token` cookie
   - Verify domain, path, expiry

2. **Check cookie is sent:**

   - Open DevTools → Network → `/api/auth/refresh` request
   - Look in Request Headers for `Cookie: pot_refresh_token=...`
   - If missing, cookie not being sent

3. **Check `SameSite` policy:**

   - Cookie should be `SameSite=Lax` (not `Strict`)
   - `Strict` blocks cookie on page refresh
   - Verify in DevTools → Application → Cookies

4. **Check `withCredentials` is configured:**

   - Axios must have `withCredentials: true` to send cookies
   - Regular axios client: Set in `axios.defaults.withCredentials`
   - Auth client: Set in `authClient.defaults.withCredentials`
   - Without this, cookies won't be sent even if they exist

5. **Check domains match:**

   - Development: Both `localhost`
   - Production: Cookie domain `.payontime.com.au` matches `api.payontime.com.au`
   - If mismatched, update frontend API URL or backend CORS

6. **Check HTTPS in production:**
   - Cookie must have `Secure=true` in production
   - All requests must use HTTPS
   - Mixed content (HTTP + HTTPS) will fail

**Problem: "CORS error when calling API"**

**Cause:** API `Cors__AllowedOrigins` doesn't include client URL

**Solutions:**

1. **Development:**

   - `Cors__AllowedOrigins=http://localhost:5175`
   - Must match Vite dev server URL (check `vite.config.ts` server port)

2. **Production:**

   - `Cors__AllowedOrigins=https://payontime.com.au`
   - Must match custom domain (not Azure default URL)
   - Update after deploying custom domain

3. **Check `AllowCredentials()`:**
   - CORS configuration must include `.AllowCredentials()`
   - Required for cookie transmission
   - Already configured in `CorsOptionsSetup.cs`

**Problem: "Cookies work in development but not Azure"**

**Cause 1:** Client container built with wrong API URL

**Solution:**

- Rebuild client image with production API URL
- Use build arg: `--build-arg VITE_API_BASE_URL=https://api.payontime.com.au/api`
- Push and redeploy to Azure

**Cause 2:** Environment variables not updated for custom domain

**Solution:**

- Update API container environment variables:
  - `Cors__AllowedOrigins=https://payontime.com.au`
  - `Jwt__Issuer=https://api.payontime.com.au`
  - `Jwt__Audience=https://api.payontime.com.au`

**Cause 3:** Cookie `Secure=true` but API still using HTTP

**Solution:**

- Verify custom domain has valid SSL certificate
- Check API URL uses `https://` not `http://`
- Azure Container Apps provides HTTPS by default

### Token Structure (Continued)

### Token Claims

**Access Token Claims (JWT):**

- `jti` (JWT ID) - Unique token identifier (GUID)
- `sub` (subject) - User ID (GUID/RowId)
- `TokenVersion` - User's token version number (custom claim)
- `exp` (expiration) - Token expiry timestamp (15 minutes from creation)
- `iat` (issued at) - Token creation timestamp
- `iss` (issuer) - Token issuer (from JWT configuration)
- `aud` (audience) - Token audience (from JWT configuration)

**What's NOT in the Token:**

- ❌ Username - Not included in JWT claims
- ❌ Email - Not included in JWT claims
- ❌ Permissions - Not included in JWT claims
- ❌ Site ID - Not included in JWT claims

**Why Minimal Claims?**

The access token contains only the essential user identifier (`sub`). When the backend receives a request:

1. Validates the JWT signature and expiration
2. Extracts the `sub` (user ID) claim
3. Queries the database for current user data (username, email, permissions, site)
4. Performs authorization checks with fresh data

This approach ensures:

- Tokens remain small and fast to validate
- Permissions are always current (not stale from token creation)
- User changes (disabled, role changes) take effect immediately
- Token version allows invalidating all user tokens at once

### Token Lifecycle

**Generation:**

1. User logs in successfully
2. Backend validates credentials
3. Backend generates both tokens
4. Access token returned in login response body
5. Refresh token set as HTTP-only cookie (automatic, not in response body)
6. Frontend stores access token in memory (AuthContext)

**Usage:**

1. Frontend includes access token in API requests
2. Backend validates token signature
3. Backend checks expiration
4. Backend extracts claims for authorization
5. Request proceeds if valid

**Refresh:**

1. Access token expires
2. Frontend detects 401 Unauthorized or proactively refreshes before expiry
3. Frontend calls refresh endpoint (refresh token sent automatically via HTTP-only cookie)
4. Backend validates refresh token from cookie
5. Backend issues new access token and rotates refresh token
6. New refresh token set as HTTP-only cookie, access token returned in response
7. Frontend stores new access token in memory and retries original request

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
- Refresh token rotation on each refresh
- Logout clears HTTP-only refresh token cookie and invalidates database refresh token

**Inactivity Timeout:**

- Access tokens expire after 15 minutes
- Refresh tokens expire after 30 days
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
