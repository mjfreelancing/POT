# Current Work Documentation

> **📋 IMPORTANT**: This file is the **source of truth** for all ongoing feature development discussions, decisions, and implementation plans. Always reference this file for context when continuing work on features to maintain consistency and avoid losing important decisions or architectural choices.

---

## 🔐 User Signup & Authentication Feature

**Date Started**: October 6, 2025  
**Status**: Planning & Design Phase  
**Priority**: High

### 📋 Feature Overview

Adding user signup functionality to the POT application with email verification using One-Time-Password (OTP) system. This feature will also lay the groundwork for "forgot password" functionality.

### 🎯 Requirements

1. **Sign Up Flow**:

   - User enters: username, display name, email, password
   - Backend stores OTP with expiration (few minutes)
   - OTP sent via email to user
   - User enters OTP to complete signup
   - Failed signup allows retry (dialog can be cancelled)

2. **User & Site Creation**:

   - Convert verified user to real user
   - Create new site associated with user
   - Assign all roles to initial user (super-admin)

3. **Future Extensibility**:
   - OTP infrastructure reusable for "forgot password"
   - Foundation for email verification workflows

### 🎨 UX Design Decisions

#### Two-Step Approach (Approved)

- **Step 1**: Email collection and initial validation
- **Step 2**: Complete profile + OTP verification
- **Rationale**: Reduces cognitive load, allows early email validation

#### Dialog State Management

- **States**: `collecting-info` → `verifying-email` → `completed`
- **Features**: Progress indicators, clear visual distinctions
- **Navigation**: Back button support, cancellation allowed

#### OTP User Experience

- **Resend functionality** with cooldown timer
- **Auto-focus** on OTP input field
- **Paste support** for 6-digit codes
- **Clear error messaging** for expired/invalid codes
- **Timeout handling** with retry options

### 🏗️ Technical Architecture

#### Database Design

```sql
PendingUsers {
  Id: guid (PK)
  Email: varchar (unique)
  Username: varchar
  DisplayName: varchar
  PasswordHash: varchar
  OtpCode: varchar
  OtpExpiry: datetime
  CreatedAt: datetime
  CorrelationId: guid -- for request tracing
}
```

#### Frontend Structure (POT Conventions)

```
src/features/auth/
├── components/
│   ├── SignupDialog.tsx
│   ├── SignupForm.tsx
│   ├── OtpVerification.tsx
│   └── SignupProgress.tsx
├── hooks/
│   ├── useSignup.ts
│   └── useOtpVerification.ts
├── types/
│   └── signupTypes.ts
└── context/
    └── SignupContext.tsx
```

#### State Management Pattern

```tsx
type SignupState = "form" | "verifying" | "completed";

type SignupData = {
  email: string;
  username: string;
  displayName: string;
  password: string;
};
```

### 🔒 Security Considerations

#### Rate Limiting

- Limit signup attempts per IP/email
- Progressive delays for failed OTP attempts
- CAPTCHA for suspicious activity (future consideration)

#### OTP Security

- **Format**: 6-digit codes (1M combinations)
- **Expiry**: 5-10 minute window
- **Usage**: Single-use codes that invalidate after verification
- **Generation**: Secure random generation

#### Password Requirements

- Minimum 8 characters
- Mix of uppercase, lowercase, numbers
- No common passwords (leverage existing POT validation)

### 🚀 Implementation Phases

#### Phase 1: Core Signup

1. Create `PendingUsers` migration
2. Build signup dialog with form validation
3. Implement OTP generation/email service
4. Create verification flow

#### Phase 2: UX Polish

1. Add progress indicators
2. Implement resend functionality
3. Add proper error handling with ErrorSheet
4. Success animations/feedback

#### Phase 3: Security Hardening

1. Rate limiting implementation
2. Audit logging
3. Security headers
4. Input sanitization

### 🤔 Open Questions & Decisions Needed

1. **Email Service**: SendGrid, AWS SES, or SMTP?
2. **Username Requirements**: Unique across all sites or per-site?
3. **Site Creation**: Auto-generate site name or let user choose?
4. **Cleanup Strategy**: How often to purge expired `PendingUsers`?
5. **OTP Delivery**: Email only, or SMS option in future?

### 📝 Code Patterns to Follow

#### Error Handling

- Use POT's custom `Result<TSuccess, TFail>` pattern
- Display critical errors with `ErrorSheet`
- Use toast notifications for transient feedback

#### State Management

- Zustand for global auth state
- React Context for signup flow state
- React Query for API calls with Result pattern

#### Logging

- Log component mount/unmount with `logger.info()`
- Log key user actions (signup attempts, OTP verification)
- Include correlation IDs for traceability

### 🔄 Future Extensions

This OTP infrastructure will support:

- Password reset flows
- Email change verification
- Two-factor authentication
- Admin-initiated user invitations

---

## � STRATEGY PIVOT: Implementing "Forgot Password" First

**Date**: October 8, 2025  
**Rationale**: Strategic decision to implement password reset before signup for several key benefits:

### 🎯 Why "Forgot Password" First?

1. **✅ UI Already Prepared**: Login dialog already has "Forgot your password?" link
2. **✅ Email Infrastructure**: Establishes email sending foundation for both features
3. **✅ OTP Architecture**: Designs reusable OTP system from the start
4. **✅ Simpler Scope**: Single-purpose flow, easier to test and validate
5. **✅ Immediate Value**: Helps existing users (even test users) recover access

### 🏗️ Unified OTP Architecture Design

Instead of separate tables, create a **unified OTP system** that supports multiple use cases:

#### Option A: Unified OTP Table (RECOMMENDED)

```sql
OneTimePasswords {
  Id: guid (PK)
  UserId: guid (nullable, FK to Users.Id) -- null for signup, populated for password reset
  Email: varchar (indexed) -- always populated for all use cases
  Purpose: varchar -- 'PASSWORD_RESET', 'EMAIL_VERIFICATION', 'SIGNUP'
  OtpCode: varchar(6) -- 6-digit numeric code
  ExpiryDateTimeUtc: datetime -- when OTP expires
  IsUsed: boolean (default false) -- prevents replay attacks
  CreatedUtc: datetime -- for audit trail and timing analysis
  VerifiedUtc: datetime (nullable) -- when OTP was successfully used
  CorrelationId: guid -- for request tracing and telemetry

  -- Indexes
  INDEX idx_otp_email_purpose (Email, Purpose)
  INDEX idx_otp_expiry (ExpiryDateTimeUtc)
  INDEX idx_otp_correlation (CorrelationId)
}
```

**Design Rationale**:

1. **Append-only table** - never delete records for complete audit trail
2. **UserId nullable** - null for signup (no user exists yet), populated for password reset
3. **Email always populated** - query mechanism for both flows
4. **IsUsed prevents replay** - OTP can only be used once even if not expired
5. **Timestamps in UTC** - proper timezone handling
6. **VerifiedUtc tracks usage** - precise timestamp when OTP was successfully verified
7. **No metadata needed** - password reset is simple, signup uses separate PendingUsers table
8. **Strategic indexes** - optimized for common query patterns

### 🔍 Detailed Field Explanations

**Q: Why both UserId and Email?**

- **Password Reset**: Query by UserId (known user), Email for validation
- **Signup**: Query by Email (no UserId exists yet), use separate PendingUsers table
- **Rate Limiting**: Track attempts per email across all purposes

**Q: Append-only vs Cleanup Strategy?**

- **Append-only approach** for security audit trail
- **Cleanup strategy**: Archive old records (>90 days) to separate audit table
- **Active queries** only look at recent, non-expired records via indexes

**Q: Why VerifiedUtc separate from IsUsed?**

- **IsUsed**: Boolean flag for quick validation logic
- **VerifiedUtc**: Precise timestamp for analytics and audit requirements
- **Analytics**: Track average time-to-verify, success rates, etc.

**Q: How does signup work without Metadata?**

- **OneTimePasswords table**: Stores OTP with email and Purpose='SIGNUP'
- **Separate PendingUsers table**: Stores username, displayName, passwordHash
- **Link via email**: Both tables use email as the common identifier
- **Cleaner separation**: OTP concerns separate from user data storage

#### Option B: Add OTP Fields to Users Table

```sql
-- Add to existing Users table:
Users {
  // ...existing fields...
  OtpCode: varchar (nullable)
  OtpExpiry: datetime (nullable)
  OtpPurpose: varchar (nullable)
  OtpCorrelationId: guid (nullable)
}

-- Keep separate PendingUsers for signup
PendingUsers {
  // ...as designed above...
}
```

### 🎯 Recommended Approach: Option A (Unified)

**Benefits**:

- Single responsibility for OTP management
- Supports multiple use cases (reset, signup, 2FA future)
- Clean separation of concerns
- Easier to implement rate limiting across all OTP types
- Audit trail for all OTP operations

### 📋 Updated Implementation Plan

#### Phase 1: Email Infrastructure + Forgot Password

1. **Email Service Setup** (SendGrid/SMTP)
2. **Create OneTimePasswords table**
3. **Implement password reset flow**:
   - Email input → OTP generation → Email sending
   - OTP verification → Password reset form
   - New password submission
4. **Basic rate limiting**

#### Phase 2: Signup Feature

1. **Extend OTP system** for signup purpose
2. **PendingUsers logic** in OTP metadata OR separate table decision
3. **Site creation workflow**
4. **Super-admin assignment**

#### Phase 3: Polish & Security

1. **Enhanced rate limiting**
2. **Audit logging**
3. **Security hardening**

### 🔄 Forgot Password User Flow

```
1. User clicks "Forgot your password?" on login
2. Dialog: Enter email address
3. Backend: Generate OTP, send email
4. Dialog: Enter OTP (with resend option)
5. Dialog: Set new password
6. Success: Redirect to login or auto-login
```

### 💡 Design Benefits of This Approach

1. **Incremental Complexity**: Start simple, add signup complexity later
2. **Shared Components**: OTP verification component reused for signup
3. **Proven Email**: Test email delivery with simpler use case first
4. **Architecture Validation**: Validates OTP system before more complex signup flow
5. **User Value**: Immediate benefit for existing users

### 🤔 Updated Decision Points

1. **Email Service**: Start with SMTP for simplicity, upgrade to SendGrid later?
2. **OTP Storage**: Confirm unified table approach vs separate tables
3. **Password Requirements**: Use existing validation or create new?
4. **Auto-login**: After password reset, redirect to login or auto-authenticate?

### 📝 Implementation Order

1. 🔥 **FIRST**: Forgot Password (this update)
2. 🚀 **SECOND**: Signup (leverage OTP infrastructure)
3. 🎨 **THIRD**: UX polish for both flows
4. 🔒 **FOURTH**: Security hardening

### 🏗️ Forgot Password Flow Components

Following POT patterns:

```tsx
type PasswordResetState =
  | "email-input"
  | "otp-verification"
  | "new-password"
  | "success";

type PasswordResetData = {
  email: string;
  otp?: string;
  newPassword?: string;
};

function ForgotPasswordDialog() {
  const [state, setState] = useState<PasswordResetState>("email-input");
  const [resetData, setResetData] = useState<PasswordResetData>({ email: "" });
  const [error, setError] = useState<DisplayError | null>(null);

  // Use Result pattern with proper error handling
  const requestResetMutation = usePasswordResetRequest();
  const verifyOtpMutation = usePasswordResetVerification();
  const updatePasswordMutation = usePasswordUpdate();

  return (
    <Dialog>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}

      {state === "email-input" && <EmailInputForm />}
      {state === "otp-verification" && <OtpVerificationForm />}
      {state === "new-password" && <NewPasswordForm />}
      {state === "success" && <SuccessMessage />}
    </Dialog>
  );
}
```

This approach gives us faster time-to-value and a more robust foundation for the signup feature.

---

## 🏗️ FINAL ENTITY DESIGN: OneTimePassword

**Date**: October 10, 2025  
**Status**: ✅ **READY FOR MIGRATION**

### 📋 Entity Implementation Summary

#### Final OneTimePasswordEntity

```csharp
public sealed class OneTimePasswordEntity : EntityBase
{
    [Required][MediumString] public required string CorrelationId { get; set; }
    [Required][MediumString][Citext] public required string Email { get; set; }
    [Required] public required OtpReason Reason { get; set; }
    [Required][OtpCode] public required string OtpCode { get; set; }
    [Required] public required bool IsUsed { get; set; }
    [Required] public required DateTime CreatedUtc { get; set; }
    [Required] public required DateTime ExpiryUtc { get; set; }
    public DateTime? VerifiedUtc { get; set; }
    public UserEntity? User { get; set; } // FK: UserId (nullable)
}
```

#### Supporting Components

- **OtpReason**: EnrichedEnum with `Signup`, `PasswordReset`
- **OtpCodeAttribute**: RegularExpressionAttribute for `^\d{6}$` validation

### 🎯 Index Strategy (6 Indexes)

**Optimized for all query patterns**:

1. **Password Reset**: `UserId + Reason + ExpiryUtc + IsUsed`
2. **Signup Validation**: `Email + Reason + ExpiryUtc + IsUsed`
3. **Cleanup Operations**: `ExpiryUtc`
4. **Debugging/Tracing**: `CorrelationId`
5. **Rate Limiting (User)**: `UserId + CreatedUtc`
6. **Rate Limiting (Email)**: `Email + CreatedUtc`

### ✅ Key Decisions Made

1. **Append-only table** - never delete for complete audit trail
2. **UserId nullable** - null for signup, populated for password reset
3. **Email always required** - common query field for both flows
4. **UTC timestamps** - proper timezone handling
5. **RegularExpressionAttribute** - built-in validation instead of custom
6. **Comprehensive indexing** - covers all query patterns efficiently
7. **No Metadata column** - cleaner separation with PendingUsers table for signup

### 🔍 Query Examples

Each index supports specific query patterns documented in entity comments:

- **Validation queries**: Find valid, non-expired, unused OTPs
- **Rate limiting queries**: Count recent attempts per user/email
- **Cleanup queries**: Find expired records for archival
- **Debugging queries**: Lookup by correlation ID for tracing

### 🚀 Next Steps

1. **Create EF Core migration** for OneTimePassword table
2. **Implement email service** (SMTP initially)
3. **Build password reset API endpoints**
4. **Create frontend password reset dialog**

This entity design provides a solid, performant foundation for both password reset and future signup functionality.

---

## 📚 References

- POT Architecture: See `.github/copilot-instructions.md`
- Review Process: See `Copilot prompt.md`
- Current Login: See existing auth components in `src/features/auth/`
