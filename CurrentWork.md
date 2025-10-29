# POT - Current Work & Decisions Log

## Site Management Feature ✅ COMPLETED (October 26, 2025)

### Overview

Added comprehensive site management functionality to the Account Settings dialog, allowing users to view and edit their site name and description.

### Implementation Summary

- **Backend**: Server `/me` endpoint now returns full site details (rowId, etag, name, description)
- **Frontend**: Added Site Settings section to UserSettingsSheet with permission-based access
- **Permissions**: `site:view` (view-only) and `site:manage` (edit capabilities)

### Files Created/Modified

```
Client:
├── src/data/site.ts (new)                              # Site schema and types
├── src/data/user.ts (updated)                          # Added site field to User schema
├── src/api/hooks/useSite.ts (new)                      # Site API hook for updates
├── src/features/userSettings/sections/siteSettings/
│   ├── SiteSettingsForm.tsx (new)                      # Site editing form
│   └── siteSettingsSchema.ts (new)                     # Form validation schema
└── src/features/userSettings/UserSettingsSheet.tsx     # Added Site Settings accordion section

Documentation:
└── README.md                                           # Added User Settings section
```

### Key Features Implemented

- **Permission-based UI**: Section hidden without `site:view`/`site:manage`, readonly mode for view-only
- **Form Validation**: Required site name, optional description with Zod schema validation
- **State Management**: Updates user store after successful changes
- **Error Handling**: Comprehensive error display with ErrorSheet component
- **Success Feedback**: Toast notifications for successful updates
- **Optimistic Concurrency**: ETag-based conflict resolution

---

## User Management Feature 🚧 IN PROGRESS

### Progress Update (October 27, 2025)

#### ✅ COMPLETED: Navigation & Basic Page Structure

- **Menu Item**: Added "Users" menu item to main navigation under "Manage" section
- **Permissions**: Uses OR logic - shows for users with `user:manage` OR `user:view` permissions
- **Navigation Component**: Enhanced `MenuGroup` component to support permission arrays with OR logic
- **Routing**: Added `/users` route with lazy loading
- **Page Structure**: Created `UsersPage.tsx` with proper:
  - Permission checks using `hasAnyPermission(['user:manage', 'user:view'])`
  - `PageHeader` component with sidebar trigger
  - Consistent styling and layout patterns
  - Mount/unmount logging

#### Files Created/Modified

```
Client:
├── src/components/nav/MenuGroup.tsx (updated)          # Enhanced to support permission arrays
├── src/components/nav/AppSidebarMenus.tsx (updated)    # Added Users menu item
├── src/features/users/UsersPage.tsx (new)             # Basic page structure
└── src/routes/AppRoutes.tsx (updated)                 # Added /users route
```

### Requirements & Decisions

#### Core Functionality Needed

1. **View Users & Roles**: Display existing users with their assigned roles and readonly view of resulting permissions
2. **Invite New Users**: Admin function to send email invitations with username/email
3. **Manage User Roles**: Assign/remove roles with live preview of combined permissions
4. **Future Extensibility**: Design to support additional user management features

#### UI Design Approach - Option 1: Main Navigation Section ✅ SELECTED

**Location**: New main navigation item "Users" or "Team Management"
**Rationale**:

- Follows existing patterns (Accounts, Expenses, Income)
- Scalable for future user management features
- Clear separation from personal account settings
- Permission-based access using existing `user:manage`/`user:view` permissions

### Simplified Page Layout & Components

#### Main Users Page Structure (Simplified)

```
┌─────────────────────────────────────────────────────────────────┐
│ Users                                        [+ Invite User]     │
├─────────────────────────────────────────────────────────────────┤
│ ┌─ Users List ──────────────────────────────────────────────┐   │
│ │ Username    │ Email           │ Role         │ Status    │⋮│   │
│ │─────────────┼─────────────────┼──────────────┼───────────┼─│   │
│ │ admin       │ admin@site.com  │ Super Admin  │ Enabled   │⋮│   │
│ │ john.doe    │ john@email.com  │ Viewer       │ Enabled   │⋮│   │
│ │ jane.smith  │ jane@email.com  │ Manager      │ Disabled  │⋮│   │
│ │ bob.jones   │ bob@email.com   │ Viewer       │ Pending   │⋮│   │
│ └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

#### Simplified Component Breakdown

**1. Simple Users Table**

- No bulk selection, filtering, or complex features
- Basic columns: Username, Email, Role, Status, Actions menu
- Clean, minimal design following existing table patterns

**2. Invite User Sheet (With Dynamic Role Selection)**

```
┌─ Invite New User ────────────────────────┐
│ [✕]                                      │
│                                          │
│ Username: [________________]             │
│ Email:    [________________]             │
│                                          │
│ Assign Role: [Select Role ▼]             │
│ ○ Super Admin                            │
│ ○ Site Manager                           │
│ ○ Financial Manager                      │
│ ● Viewer (default)                       │
│ ○ Custom Role 1                          │
│ ○ Custom Role 2                          │
│                                          │
│          [Cancel] [Send Invitation]      │
└──────────────────────────────────────────┘
```

**3. Dynamic Role Management Dialog**

```
┌─ Manage Role: john.doe ──────────────────┐
│ [✕]                                      │
│                                          │
│ Current Role: Viewer                     │
│                                          │
│ Assign Role: [Select Role ▼]             │
│ ○ Super Admin                            │
│ ○ Site Manager                           │
│ ○ Financial Manager                      │
│ ● Viewer                                 │
│ ○ Custom Role 1                          │
│ ○ Custom Role 2                          │
│ ○ (No Role)                              │
│                                          │
│              [Cancel] [Save Changes]     │
└──────────────────────────────────────────┘
```

#### Simplified Technical Architecture

**File Structure (Minimal)**

```
src/features/users/
├── components/
│   ├── UsersTable.tsx              # Users table with Enable/Disable actions
│   ├── InviteUserSheet.tsx         # Invitation form with role selection
│   ├── UserRoleDialog.tsx          # Role assignment dialog
│   └── UserStatusBadge.tsx         # Status indicator (Enabled/Disabled/Pending)
├── hooks/
│   ├── useUsers.ts                 # User CRUD operations
│   ├── useRoles.ts                 # Fetch available roles from server
│   ├── useInviteUser.ts           # Send invitation with role assignment
│   └── useToggleUserStatus.ts     # Enable/disable user accounts
├── UsersPage.tsx                  # Main page component
└── index.ts
```

**Data Models (Confirmed Facts Only)**

```typescript
// Role definition from server - STRUCTURE TO BE CONFIRMED
type Role = {
  // TODO: Confirm with server team - currently only known to have 'name'
  name: string; // ✅ CONFIRMED: Role has a name
  // QUESTIONS NEEDED:
  // - Does role have an ID/rowId field?
  // - Are permissions included in role response?
  // - Are there additional fields (description, isSystemRole, etc.)?
};

// Extend existing User type with role and status
type UserWithRole = User & {
  role?: string; // QUESTION: Is this role name string or full role object?
  status: "enabled" | "disabled" | "pending"; // ✅ CONFIRMED: These status types
};

// Invitation structure - TO BE CONFIRMED
type UserInvitation = {
  username: string; // ✅ CONFIRMED
  email: string; // ✅ CONFIRMED
  role: string; // QUESTION: Is this role name or role ID?
};
```

#### Updated Core Functionality

1. **View Users**: Table showing username, email, role, status (Enabled/Disabled/Pending)
2. **Dynamic Role Loading**: Fetch available roles from server API endpoint
3. **Invite User**: Form with username, email, AND role selection from server-provided roles
4. **Manage User Role**: Change assigned role via dropdown populated with current available roles
5. **Enable/Disable User**: Toggle user access without deleting the account
6. **Streamlined Process**: No OTP codes - users receive username + temporary password + role assignment
7. **Future Role Creation**: Framework ready for adding custom role creation functionality

#### Permission Integration (Minimal)

- **Page Access**: Requires `user:view` permission
- **Invite/Role Management**: Requires `user:manage` permission
- **Readonly Mode**: Hide action buttons when lacking `user:manage`

#### Updated Email Process

**Invitation Email Template**:

- **Subject**: "You've been invited to join [Site Name] on POT"
- **Content**: Welcome message with:
  - Site name and inviter information
  - Assigned role notification
  - Username and temporary password
  - Direct login instructions (no OTP verification needed)
  - Password change reminder for first login

**Server Process**:

1. Admin submits invitation with username, email, and role
2. Server creates complete user account with assigned role
3. Server generates temporary password
4. Server sends welcome email with login credentials
5. User logs in normally and changes password

#### User Actions Available

**For Each User Row**:

- **Change Role**: Modify assigned role via dropdown dialog
- **Enable/Disable**: Toggle user access (replaces delete functionality)
- **Resend Invitation**: For pending users, resend welcome email
- **View Permissions**: Optional readonly view of effective permissions from role

### Required Server Endpoints

The following endpoints need to be implemented on the server to support the user management functionality. This serves as a development checklist with concrete implementation details.

#### 1. Get Available Roles

**Endpoint**: `GET /api/roles`
**Purpose**: Fetch all available roles for dropdown selection in invite/role assignment UI
**Auth**: Requires `user:view` permission
**Response**:

```json
[
  {
    "name": "Admin",
    "rowId": "62143921-82e3-44ed-825b-03798dd3f8f0",
    "etag": 1
  },
  {
    "name": "Viewer",
    "rowId": "1120215f-816a-44b7-a43f-1630726a0e2f",
    "etag": 1
  }
]
```

**Implementation Status**: ✅ Implemented

#### 2. Get Site Users

**Endpoint**: `GET /api/users`
**Purpose**: Fetch all users for the current site with their roles and status
**Auth**: Requires `user:view` permission
**Response**:

```json
[
  {
    "username": "mjfreelancing",
    "displayName": "MJF (DEMO)",
    "email": "malcolm@mjfreelancing.com",
    "roles": ["Admin"],
    "lastLoggedInUtc": "2025-10-26T11:46:03.11356Z",
    "rowId": "8826aa3a-914d-4d04-af05-fbed5b5b621f",
    "etag": 1761479163151
  },
  {
    "username": "MJFREELANCING2",
    "displayName": "MJF2 (DEMO2)",
    "email": "malcolm@mjfreelancing.com",
    "roles": ["Admin"],
    "lastLoggedInUtc": null,
    "rowId": "058b4772-2e87-4702-83ce-c8644e8b0e3d",
    "etag": 1761449584051
  }
]
```

**Implementation Status**: ✅ Implemented

#### 3. Invite User

**Endpoint**: `POST /api/users/invite`
**Purpose**: Create new user account and send invitation email with role assignment
**Auth**: Requires `user:manage` permission
**Request Body**:

```json
{
  "username": "john.doe",
  "email": "john@email.com",
  "roleId": "viewer" // Role ID from /api/roles
}
```

**Response**:

```json
{
  "success": true,
  "data": {
    "userId": "string",
    "username": "john.doe",
    "email": "john@email.com",
    "status": "pending",
    "invitationSent": true,
    "temporaryPassword": "temp123" // For admin reference, not stored
  }
}
```

**Server Actions**:

1. Validate role exists and user has permission to assign it
2. Create user account with assigned role for current site
3. Generate temporary password
4. Send invitation email with username, temp password, and role info
5. Set user status to "pending"

**Implementation Status**: ❌ Not Implemented

#### 4. Update User Role

**Endpoint**: `PUT /api/users/{userId}/role`
**Purpose**: Change a user's assigned role
**Auth**: Requires `user:manage` permission
**Request Body**:

```json
{
  "roleId": "site-manager" // New role ID, or null to remove role
}
```

**Response**:

```json
{
  "success": true,
  "data": {
    "userId": "string",
    "username": "john.doe",
    "previousRole": {
      "id": "viewer",
      "name": "Viewer"
    },
    "newRole": {
      "id": "site-manager",
      "name": "Site Manager"
    }
  }
}
```

**Implementation Status**: ❌ Not Implemented

#### 5. Toggle User Status

**Endpoint**: `PUT /api/users/{userId}/status`
**Purpose**: Enable or disable user access to the site
**Auth**: Requires `user:manage` permission
**Request Body**:

```json
{
  "status": "disabled" // "enabled" | "disabled"
}
```

**Response**:

```json
{
  "success": true,
  "data": {
    "userId": "string",
    "username": "john.doe",
    "previousStatus": "enabled",
    "newStatus": "disabled",
    "changedAt": "2025-10-26T10:00:00Z"
  }
}
```

**Implementation Status**: ❌ Not Implemented

#### 6. Resend User Invitation

**Endpoint**: `POST /api/users/{userId}/resend-invitation`
**Purpose**: Resend invitation email for pending users with new temporary password
**Auth**: Requires `user:manage` permission
**Response**:

```json
{
  "success": true,
  "data": {
    "userId": "string",
    "username": "john.doe",
    "email": "john@email.com",
    "invitationResent": true,
    "newTemporaryPassword": "temp456"
  }
}
```

**Server Actions**:

1. Verify user status is "pending"
2. Generate new temporary password
3. Send updated invitation email
4. Update user record with new temp password

**Implementation Status**: ❌ Not Implemented

#### Implementation Notes

**Database Considerations**:

- User identification: Use `rowId` (GUID) for all API operations
- Role assignment: Store `roleId` in UserRole table with `siteId` association
- Status tracking: Add `status` field to User table or UserRole table
- Temporary passwords: Store hashed temporary password with expiration

**Email Template Requirements**:

- **Subject**: "You've been invited to join [Site Name] on POT"
- **Content**: Site name, inviter info, assigned role, username, temp password, login instructions
- **Template Location**: `Pot.RazorEmail` project for consistency

**Permission Validation**:

- All endpoints must validate user has appropriate permissions for current site
- Role assignment must validate the target role exists and is assignable
- Status changes must prevent disabling the last admin user

**Error Handling**:

- Return consistent error format matching existing API patterns
- Handle duplicate usernames/emails gracefully
- Validate role assignments against available roles

### Next Steps - Remaining Work

#### Remaining Implementation

1. **Server API endpoints**: Need to implement the 3 missing endpoints:
   - `POST /api/users/invite` (user invitation with role)
   - `PUT /api/users/{userId}/status` (enable/disable user)
   - `POST /api/users/{userId}/resend-invitation` (resend invitation)
2. **Connect invite functionality**: Wire up `InviteUserSheet` to actual API
3. **Add user status management**: Enable/disable functionality once API supports it
4. **Create invitation email template**: Server-side email template for invitations

---

## Current Architecture Limitations & Future Considerations

### Role/Permission Management Scope (IMPORTANT)

#### Current Implementation:

- **Global Roles/Permissions**: All roles and permissions are defined globally in the system
- **No Site-Level Customization**: Individual sites cannot create their own custom roles or permissions
- **Predefined Role Set**: Only system-defined roles (Admin, Viewer, Manager, etc.) are available
- **Consistent Across Sites**: All sites share the same role structure and permission model

#### Schema Implications:

```sql
-- Current: Global roles (no site association)
Role: Id, Name, Description
Permission: Id, Name, Description
RolePermission: RoleId, PermissionId

-- User roles are site-specific (this part works correctly)
UserRole: UserId, RoleId, SiteId
```

#### **User Management Phase 1 Scope (Current)**:

- ✅ View existing users with their assigned roles
- ✅ Invite new users and assign existing roles
- ✅ Change user role assignments (from predefined roles)
- ✅ Enable/disable user access
- ❌ Create new custom roles
- ❌ Create new custom permissions
- ❌ Site-specific role definitions

#### **Future Enhancement Requirements** (If Site-Specific Roles Needed):

**Schema Changes Required:**

```sql
-- Add site association to roles
Role: Id, Name, Description, SiteId (nullable for global roles)

-- OR create site-specific role tables
SiteRole: Id, SiteId, Name, Description, BaseRoleId (optional)
SitePermission: Id, SiteId, Name, Description, BasePermissionId (optional)
SiteRolePermission: SiteRoleId, SitePermissionId
```

**Application Changes Required:**

- Role management UI within each site's admin area
- Permission builder/editor interface
- Role inheritance from global templates
- Migration strategy for existing role assignments
- API endpoints for site-specific role CRUD operations

#### **Decision Rationale** (October 26, 2025):

For the initial user management implementation, we're deliberately **excluding role/permission management** because:

1. **Current schema supports global roles perfectly** for most use cases
2. **Simpler implementation** - focus on core user management functionality
3. **Most sites will use standard roles** (Admin, Manager, Viewer patterns)
4. **Can be added later** without breaking existing functionality
5. **Clear separation of concerns** - user management vs. system administration

#### **Documentation Note for README**:

When transferring these notes to the README, ensure this limitation is clearly documented in the User Management section with the note: _"Current implementation uses predefined global roles. Site-specific role creation requires schema modifications and is planned for a future release."_

---

_Last Updated: October 26, 2025_
