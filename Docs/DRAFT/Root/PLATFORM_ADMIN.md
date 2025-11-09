# Platform Admin Configuration

## Overview

POT supports **runtime platform administrator elevation** through environment variables. This allows specific users (identified by their GUID) to receive the special `platform:manage` permission without storing it in the database.

**Important:** Platform admins still need normal database roles (Admin or Viewer). The environment variable only adds the `platform:manage` permission on top of their existing permissions. This permission grants access to cross-site administrative features like approving pending users.

**Security:** User GUIDs are used instead of usernames to obscure platform admin identities and prevent enumeration.

## Key Features

- ✅ **Zero Database Footprint**: Platform admin status is determined at runtime, not stored in database
- ✅ **Multiple Admins**: Support comma-separated list of user GUIDs
- ✅ **Environment-Specific**: Different admins for dev, staging, production
- ✅ **Not in Source Control**: Configuration via environment variables only
- ✅ **All Permissions**: Platform admins get `platform:manage` permission added to their database role permissions
- ✅ **Bypass Site Isolation**: Can manage pending users across all sites
- ✅ **Security Through Obscurity**: Uses GUIDs instead of usernames to prevent admin enumeration

## How It Works

1. Platform admin user IDs (GUIDs) are configured via `PLATFORM_ADMIN_USERIDS` environment variable
2. When a user authenticates, the `PermissionService` checks if their user ID matches the configured list
3. If matched, the special `platform:manage` permission is added to their existing database permissions
4. Platform admins still need to be assigned normal roles in the database (Admin or Viewer)
5. The `platform:manage` permission grants access to special `/platform/*` endpoints that bypass site isolation

**Security Note:** Using GUIDs instead of usernames prevents attackers from enumerating platform admin identities.

## Configuration

### Development (Local Docker)

**Step 1:** Sign up and get your user GUID

After signing up, connect to the database and run:

```sql
SELECT "RowId", "Username" FROM "User" WHERE "Username" = 'your_username';
```

**Step 2:** Add your GUID to `.env.development`:

```bash
# Platform Admin Configuration
# Comma-separated list of user GUIDs (RowIds)
PLATFORM_ADMIN_USERIDS=550e8400-e29b-41d4-a716-446655440000
```

**Step 3:** Restart the server

```bash
docker-compose restart pot-server
```

### Production (Azure)

Add environment variable in Azure App Service Configuration:

```
Name: PlatformAdmin__UserIds
Value: 550e8400-e29b-41d4-a716-446655440000,6ba7b810-9dad-11d1-80b4-00c04fd430c8
```

**Note:** Azure uses double underscore (`__`) for nested configuration sections.

### Docker Compose (Already Configured)

The `docker-compose-client-server.yml` file automatically maps the environment variable:

```yaml
environment:
  - PLATFORMADMIN__USERIDS=${PLATFORM_ADMIN_USERIDS}
```

## Usage Examples

### Single Platform Admin

```bash
PLATFORM_ADMIN_USERIDS=550e8400-e29b-41d4-a716-446655440000
```

### Multiple Platform Admins

```bash
PLATFORM_ADMIN_USERIDS=550e8400-e29b-41d4-a716-446655440000,6ba7b810-9dad-11d1-80b4-00c04fd430c8,7c9e6679-7425-40de-944b-e07fc1f90ae7
```

### Spaces Are Trimmed

```bash
# These are equivalent:
PLATFORM_ADMIN_USERIDS=550e8400-e29b-41d4-a716-446655440000,6ba7b810-9dad-11d1-80b4-00c04fd430c8
PLATFORM_ADMIN_USERIDS=550e8400-e29b-41d4-a716-446655440000, 6ba7b810-9dad-11d1-80b4-00c04fd430c8
PLATFORM_ADMIN_USERIDS= 550e8400-e29b-41d4-a716-446655440000 , 6ba7b810-9dad-11d1-80b4-00c04fd430c8
```

### Invalid GUIDs Are Ignored

```bash
# Only the valid GUID will be recognized
PLATFORM_ADMIN_USERIDS=550e8400-e29b-41d4-a716-446655440000,invalid-guid,not-a-guid
```

## Platform Admin Capabilities

Platform admins receive their normal role permissions PLUS the special `platform:manage` permission.

### What `platform:manage` Grants

- **Approve Pending Users**: Can approve new signups from any site
- **Cross-Site Visibility**: Can see data from all sites (when using platform endpoints)
- **System Administration**: Access to platform-level administrative features

### Regular Permissions (From Database Role)

Platform admins should be assigned the **Admin** role in the database, which includes:

- `site:manage`, `site:view` - Site management
- `user:manage`, `user:view` - User management
- `account:manage`, `account:view` - Account management
- `expense:manage`, `expense:view` - Expense management
- `income:manage`, `income:view` - Income management
- `maintenance:export`, `maintenance:import` - Data management

**Note:** The environment variable only adds `platform:manage`. Normal permissions come from the database role assignment.

## Security Considerations

### ✅ Best Practices

1. **Limit Platform Admins**: Only assign to trusted individuals
2. **Secure GUID Storage**: Treat user GUIDs as sensitive data
3. **Rotate When Needed**: Update the environment variable if an admin leaves
4. **Different Per Environment**: Use different admins for dev vs production
5. **Audit Logging**: Platform admin actions are logged with user ID context
6. **Database Access Control**: Limit who can query the User table to get GUIDs

### ⚠️ Important Notes

- Platform admin status is checked on EVERY request
- Configuration changes require application restart
- Empty or missing configuration means no platform admins
- Platform admins still need database role assignments (recommend Admin role)
- The `platform:manage` permission is additive - it enhances, not replaces, database permissions

## Deployment Checklist

### First-Time Azure Deployment

1. Deploy application to Azure
2. Sign up with your username
3. Connect to the Azure PostgreSQL database
4. Get your user GUID: `SELECT "RowId" FROM "User" WHERE "Username" = 'your_username'`
5. Add `PlatformAdmin__UserIds` environment variable in Azure portal with your GUID
6. Restart the app service
7. You now have platform admin access
8. Other users who sign up will have `Pending` status and need your approval

### Adding/Removing Platform Admins

1. Update environment variable (`.env.development` or Azure config)
2. Restart application
3. Changes take effect immediately upon restart

### Troubleshooting

**Issue**: Platform admin not working after configuration

**Solutions**:

- Verify environment variable name: `PLATFORM_ADMIN_USERIDS` (local) or `PlatformAdmin__UserIds` (Azure)
- Verify GUID format is correct (36 characters with hyphens)
- Check GUID matches EXACTLY the RowId in database (case-insensitive for GUIDs)
- Restart the application after config changes
- Check application logs for "identified as platform admin" message

**How to get your user GUID:**

```sql
-- Connect to PostgreSQL database
SELECT "RowId", "Username", "Email"
FROM "User"
WHERE "Username" = 'your_username';
```

## Logging

When a platform admin authenticates, you'll see log entries like:

```
[INF] User with ID '550e8400-e29b-41d4-a716-446655440000' identified as platform admin - granting platform:manage permission
```

This helps audit platform admin actions without exposing usernames in logs.

## Migration Path

If you want to move from runtime platform admins to database-backed SuperAdmin role:

1. Create migration to add `SuperAdmin` role
2. Manually assign role to users in database
3. Remove `PLATFORM_ADMIN_USERIDS` environment variable
4. Restart application

Both systems can coexist - environment-based platform admins have priority over database roles.

## Security Advantages

Using GUIDs instead of usernames provides several security benefits:

1. **No Username Enumeration**: Attackers cannot determine platform admin identities from configuration
2. **Obscurity**: GUIDs are meaningless without database access
3. **Database Access Required**: Would-be attackers need database credentials to get GUIDs
4. **Log Security**: Logs show GUIDs not usernames, protecting admin identity
5. **Configuration Security**: Even if `.env` file is exposed, admin identities remain hidden
