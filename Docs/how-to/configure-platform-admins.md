# Configure platform administrators

A platform administrator is a user who holds `platform:manage` because the deployment says so, not
because of a role in the database. The grant comes from configuration, so adding or removing one is a
configuration change plus a restart.

Making yourself the first platform admin is part of
[First-time setup](../tutorials/first-time-setup.md); this page covers the setting itself.

## Where the value lives

| Environment         | Setting                                                      |
| ------------------- | ------------------------------------------------------------ |
| Docker stack        | `PLATFORM_ADMIN_USERIDS` in `Source/Docker/.env.development` |
| Azure Container App | the `PlatformAdmin__UserIds` application setting             |
| API run directly    | `PlatformAdmin:UserIds` in `appsettings.Development.json`    |

The double underscore in `PlatformAdmin__UserIds` is ASP.NET Core's separator for a nested key, so it
sets `PlatformAdmin:UserIds`.

## Find the user's RowId

Platform admins are named by the user's public identifier, not the internal integer id:

```sql
SELECT "RowId" FROM "User" WHERE "Username" = '<username>';
```

## Rules the API enforces at startup

- The value is a comma-separated list. Spaces around entries are trimmed, and empty entries are ignored,
  so all of these are the same: `guid1,guid2`, `guid1, guid2`, ` guid1 , guid2`.
- Every entry must parse as a GUID. **One invalid entry fails startup validation** and the API will not
  start, which is deliberate: a typo would otherwise silently leave someone without the grant.
- Empty is valid. It means this deployment has no platform administrators.

## What the grant does

`platform:manage` is added to the user's normal permissions, which is what allows them to approve
sign-ups, administer users across sites, and reach the platform-only routes.

It does **not** replace a role. A platform administrator still needs a role inside a site - normally
`Admin` - to work with that site's data. The grant is what lets them act on every site and approve
accounts; the role is what lets them act within one.

## Change it

1. Edit the value in the file or setting for your environment.
2. Restart the API. The option is bound and validated at startup, so a change does not take effect while
   the process is running.
3. Sign in again as that user.

## Verify it

A user whose `RowId` is in the list is identified as a platform administrator on each request, and the
sign-in logs a line naming them. If the grant appears not to work, check that the value is set on the
running container or process - not only in your local copy of the env file - and that the API restarted
after the change.

## Several administrators

List all of them in the same value. There is no limit, and each is independent: removing one does not
affect the others.

Treat the list as sensitive. It names the accounts that can approve users and act across every site, so a
platform admin id is as important as the account's password.

## Related

- [Auth and sessions](../reference/auth-and-sessions.md) - what `platform:manage` and the other permissions mean
- [Ports and environments](../reference/ports-and-environments.md) - which environment you are configuring
- [Auth and approval endpoints](../explanation/authentication.md) - the approval flow the grant enables
