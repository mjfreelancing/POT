# Authentication and Authorization in POT

POT implements a comprehensive authentication and authorization system using JWT tokens and permission-based access control.

## Overview

The authorization system consists of four main components working together:
1. JWT-based authentication for secure user identity
2. Dynamic policy-based authorization for flexible permission checks
3. Role-based access control for grouping permissions
4. Database-backed permission storage

## Authentication Flow

### 1. User Authentication

1. User submits credentials to the `/api/auth/login` endpoint
2. `AuthService` validates the credentials:
   - Retrieves user from database
   - Validates password using `UserPasswordHasher`
   - On success, generates JWT token using `JwtProvider`
3. JWT token is returned to client containing user claims

### 2. JWT Configuration

JWT handling is configured through a chain of specialized components:

- `JwtOptionsSetup`: Loads JWT settings from configuration
- `JwtBearerOptionsSetup`: Configures token validation parameters
- `JwtProvider`: Generates tokens with appropriate claims

The setup disables default claim type mapping to preserve original claim types, which is important for our permission system.

## Authorization System

### Components and Their Roles

1. **PermissionRequirement**
   - Represents a required permission (e.g., `account:view`)
   - Used by ASP.NET Core's authorization system to evaluate access

2. **PermissionAuthorizationHandler**
   - Evaluates if a user has a required permission
   - Works by:
     1. Extracting user ID from JWT claims
     2. Loading user's permissions from database
     3. Checking if required permission exists in user's permission set

3. **PermissionAuthorizationPolicyProvider**
   - Creates authorization policies dynamically
   - Converts permission strings into authorization policies
   - Eliminates need for explicit policy registration
   - Enables using permission strings directly when specifying the required authorization (e.g., `RequireAuthorization("account:view")`)

4. **PermissionService**
   - Manages database interaction for permissions
   - Loads user's effective permissions through role relationships
   - Handles permission inheritance through roles

### How They Work Together

When an endpoint requires authorization:

1. Request arrives with JWT token
2. Framework extracts user identity and claims
3. Authorization middleware checks for required permissions
4. PermissionAuthorizationPolicyProvider creates policy from permission string
5. PermissionAuthorizationHandler evaluates the permission
6. PermissionService loads actual permissions from database
7. Access is granted or denied based on permission match

## Role and Permission Model

### Permission Structure

The system uses a resource:action pattern for permissions:

1. **Site Management**
   - `site:manage`: Full site management capabilities
   - `site:view`: Read-only site access

2. **User Management**
   - `user:manage`: User administration capabilities
   - `user:view`: View user information

3. **Account Management**
   - `account:manage`: Create/update account data
   - `account:view`: View account information

4. **Expense Management**
   - `expense:manage`: Create/update expense data
   - `expense:view`: View expense information

5. **Income Management**
   - `income:manage`: Create/update income data
   - `income:view`: View income information

### Role Structure

1. **Admin Role**
   - Automatically receives all permissions
   - Full system access

2. **Viewer Role**
   - Receives only view permissions
   - Read-only access across all features
   - Suitable for reporting or auditing users

### Database Structure

The permission system uses four main entities:

1. `UserEntity`
   - Core user information
   - Links to roles via many-to-many relationship

2. `RoleEntity`
   - Named role definitions
   - Links to permissions via many-to-many relationship

3. `PermissionEntity`
   - Individual permission definitions
   - Stored as resource:action strings

4. `SiteEntity`
   - Represents a tenant in the system
   - Users and accounts belong to sites

## Using the Authorization System

### Protecting Endpoints

To require permissions on an endpoint:

```csharp
routeGroupBuilder
    .MapGet(AccountsEndpoints.GetAll, GetAll.Handler.Invoke)
    .RequireAuthorization("acount:view")
```

The permission string automatically becomes a policy requirement.

### Best Practices

1. **Permission Naming**
   - Use lowercase resource:action format
   - Keep permission names consistent

2. **Role Assignment**
   - Assign roles instead of individual permissions
   - Use Admin role sparingly
   - Create specific roles for specific needs

3. **Security Considerations**
   - Always verify permissions server-side
   - Don't expose permission checks in API responses
   - Log authorization failures for security monitoring

## Maintenance and Evolution

1. **Adding New Permissions**
   - Add permission to database through migration
   - Update role assignments as needed
   - Document in this guide

2. **Creating New Roles**
   - Consider permission grouping carefully
   - Document role purpose and permissions
   - Create through database migration
