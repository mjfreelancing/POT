# User and Site Deletion PRD

**Status:** Planned
**Priority:** Medium
**Last Updated:** 2026-10-17
**Feature ID:** 022
**Scope:** Three deletion actions, all confined to the caller's own site and identity. (1) A site administrator removes another user from their site. (2) A signed-in user removes their own account. (3) A site administrator deletes their whole site. Includes the safeguards that keep a site administrable, the safeguards that keep the platform operable, and the confirmation, authorisation and notification behaviour of all three actions. Cross-tenant operations by a platform administrator are out of scope and are addressed by PRD-027.

## Problem Statement

There is no supported way to remove a user, and no supported way to retire a site.

A site administrator can disable a user, but disabling is reversible and keeps the account. There is no permanent option, and no way to clear an invitation that was never accepted or an account that is no longer wanted.

A site can also never be closed. Every self-service signup creates a site, and the only current way to remove one — or one of its users — is hand-written database work that must remove dependent records first. That work is error-prone and is not available to the product's own administrators.

At the same time, both operations are dangerous in a way that disabling is not. Removing the wrong account can leave a site with nobody able to administer it, and removing a platform operator can leave the platform with nobody able to operate it. Deleting a site destroys other people's accounts and a tenant's entire financial history. The feature therefore needs explicit safeguards, explicit confirmation, and a clear statement of what is and is not recoverable.

## Actors and Authority

| Actor                  | Definition                                                                                                                  |
| ---------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Site administrator     | A user holding the `Admin` role in their own site. Only enabled users can sign in, so in practice an enabled administrator. |
| Signed-in user         | Any authenticated user, whatever their role.                                                                                |
| Platform administrator | An account identified by deployment configuration, not by a role in the database.                                           |

| Action              | Who may perform it                                 | What is destroyed                                                      |
| ------------------- | -------------------------------------------------- | ---------------------------------------------------------------------- |
| Remove another user | A site administrator, for a user in their own site | That user's account and everything that identifies them                |
| Remove own account  | The user themselves                                | Their own account, and everything that identifies them                 |
| Delete site         | A site administrator, for their own site           | Every user, session, account, expense, income and setting of that site |

A platform administrator has no additional authority in this feature. Every action here is limited to the caller's own site and identity.

## Terms

- **Enabled administrator** — a user in a site holding the `Admin` role whose status is enabled. Only enabled administrators count anywhere in this document. Disabled, pending and never-activated administrators do not count.
- **Live platform administrator** — a configured platform administrator identifier that belongs to an enabled user. Configured identifiers that do not belong to an enabled user do not count.

## Functional Requirements

### A. A site must always remain administrable

- **R1.** Every site must always retain at least one enabled administrator.
- **R2.** The rule in R1 applies to every path that could remove the last enabled administrator: removing the user, disabling the user, and taking away the `Admin` role.
- **R3.** Only enabled accounts count towards R1. A disabled, pending or never-activated administrator does not satisfy the rule.
- **R4.** A user cannot perform an action on themselves that would breach R1. This covers removing their own account and disabling their own account, but not deleting the whole site (E2).
- **R5.** A site must never be left in existence with no users. Removing the last user is refused; deleting the site removes the site itself and is the permitted alternative.
- **R6.** When an action is refused under R1–R5, the caller is told plainly why, and what they can do instead: promote or enable another administrator first, or — where the caller is the site's only user — delete the whole site.

### B. The platform must remain operable

- **R7.** A configured platform administrator may be removed or disabled only while at least one other live platform administrator exists.
- **R8.** Removing or disabling the last live platform administrator is refused. This includes the last platform administrator attempting to remove or disable themselves.
- **R9.** To remove the last live platform administrator, an operator must first remove them from deployment configuration and redeploy; the account can then be removed through the normal actions in this document.
- **R10.** A platform administrator's site roles are not restricted by these rules. The safeguards in R7–R9 concern the account itself and its enabled state.

### C. Removing another user from the same site

- **C1.** Only a site administrator may remove another user, and only a user who belongs to the same site.
- **C2.** A site administrator may remove another administrator. Only the last enabled administrator is protected (R1).
- **C3.** A user may not be removed through this action from outside the caller's site. A user identifier that belongs to another site is treated as not found; the response must not reveal that the user exists.
- **C4.** A user cannot remove themselves through this action. Self-removal is the separate action in section D.
- **C5.** A user may be removed whatever their status: enabled, disabled, or invited but never activated.
- **C6.** Removing a user leaves the site's accounts, expenses and incomes untouched. They belong to the site and remain visible to its remaining users.
- **C7.** Removal is permanent. There is no archive, recycle bin or restore; disabling remains the reversible alternative.
- **C8.** Once a user is removed, the same username may be used again to sign up. No residual record may prevent that signup or interfere with it.
- **C9.** Before the removal is performed, the administrator types the target's username and enters their own current password.
- **C10.** If the password is wrong, the administrator is told it is invalid, inside the same confirmation, without being signed out.
- **C11.** Repeated wrong-password attempts are not limited by this feature. Existing platform-wide protections still apply.
- **C12.** After a successful removal, the removed user's next request is rejected and they land on the login page.
- **C13.** After a successful removal, email notifications are sent to every enabled administrator of the site — including the administrator who performed the removal — and to the removed user's email address.
- **C14.** Notifications are sent only after the removal has succeeded. A notification that cannot be delivered does not undo, block or fail the removal.

### D. Removing your own account

- **D1.** Any signed-in user, whatever their role, may remove their own account.
- **D2.** The action is offered in the user's settings, as a separate block below the settings sections. It is not part of any editable form, so it never interacts with unsaved changes.
- **D3.** The action is refused when the caller is the site's last enabled administrator (R1) or the last live platform administrator (R8).
- **D4.** Before the removal is performed, the user types their own username and enters their own current password.
- **D5.** A wrong password is reported inside the confirmation, without signing the user out (C10).
- **D6.** After a successful removal the user is signed out on this device and returned to the login page with a brief message.
- **D7.** After a successful removal, email notifications are sent to the remaining enabled administrators of the site and to the departing user's email address, as confirmation.

### E. Deleting a whole site

- **E1.** Only an enabled administrator of the site may delete that site. Deleting a site from outside its own administration is out of scope.
- **E2.** A site may be deleted by its only enabled administrator. The restriction in R4 does not apply here; that administrator's own account is removed as part of the site.
- **E3.** Deleting a site is refused when the site contains the last live platform administrator (R8). The site becomes deletable once that administrator is removed from deployment configuration and redeployed (R9).
- **E4.** Deleting a site removes every user, every session, every account, every expense, every income and every setting belonging to that site. Nothing is retained.
- **E5.** Deletion is immediate. There is no grace period, cancellation window or recovery period.
- **E6.** No backup or export is offered as part of the action, and the confirmation states plainly that no backup is taken.
- **E7.** No record counts or summaries are shown before confirming.
- **E8.** Before the deletion is performed, the administrator types the site name, types their own username and enters their own current password.
- **E9.** A wrong password is reported inside the confirmation, without signing the administrator out (C10).
- **E10.** After a successful deletion, every user of that site has their next request rejected, and the administrator who performed the action is returned to the login page with a brief message.
- **E11.** After a successful deletion, email notifications are sent to every enabled administrator of the deleted site, including the administrator who performed the deletion.

### F. Concurrency and record keeping

- **F1.** If two administrators remove the same user at the same moment, the second action reports that the user was not found. The same applies to a repeat of a completed removal or site deletion.
- **F2.** If a user is signing up or resetting their password at the same moment they are removed, the removal wins. Requests that follow it report that the user was not found.
- **F3.** Each removal and deletion is logged server-side. No durable, queryable audit record is created.
- **F4.** Removal is one user at a time. Bulk removal and "remove all disabled users" are not provided.
- **F5.** No removed data is retained for any period beyond the removal itself.

## Out of Scope

- Cross-tenant operations performed by a platform administrator: deleting a user in another site, deleting another site, and promoting an administrator in another site. These are specified and delivered by PRD-027 (platform administration).
- Operator assistance for a site that has lost its administrators, including inviting a user into that site on the operator's behalf. This is specified and delivered by PRD-027 (platform administration).
- Any override of the last-administrator rule by a platform administrator. This is addressed by PRD-027 (platform administration).
- Bulk removal.
- Soft delete, archiving, anonymisation or a permanent "disabled" state.
- A durable audit history, export or retention period.
- A backup, export or summary step before deleting a site.
- Password re-entry limits specific to these actions.
- Notifying users of a site when another user is removed, beyond the recipients listed in C13.
- **Proactive UI indication of refusal reasons.** It would be preferred if the Users list and the settings blocks showed up front that a user or site cannot be removed, or that an account cannot be disabled, based on the platform administrator and last-administrator rules. This is not required here: the server refuses the action and returns an actionable message, which is sufficient for now. A pre-emptive indicator is separate future work.

The items above that name PRD-027 are specified and delivered there. The rest are separate future work, outside both this document and PRD-027.

## Known Risks and Accepted Limitations

- **Mutual removal.** If two administrators remove each other at the same moment, each check can pass before either removal completes, leaving the site with no users. The likelihood is considered extremely low. No additional complexity is added for it here; a future capability for a platform administrator to add or promote a user in the site is the intended remedy, specified by PRD-027.
- **Notification delivery.** Notifications are best effort. A delivery failure is visible in logs but is not reported to the administrator and does not affect the removal.
- **Stale configuration.** A configured platform administrator identifier that does not belong to an enabled user is ignored when counting live platform administrators (see Terms). Configuration hygiene is an operator responsibility.
- **Orphaned site.** When the only enabled administrator of a site is also the last live platform administrator, that site cannot be deleted until the administrator is removed from deployment configuration and redeployed (E3, R9). Removing the configuration entry restores the administrator's ability to delete the site. Where the administrator is unavailable, the site can only be resolved by the platform administration feature specified by PRD-027, which can add a user to the site or delete it on the operator's behalf.
- **Relaxed platform protection.** R7 allows a platform administrator to be disabled or removed when another live platform administrator exists. This replaces the current behaviour, where a configured platform administrator can never be disabled.
- **Refusals are discovered on submission.** The client does not predict whether an action will be refused. A user who cannot be removed, or a site that cannot be deleted, is only learned when the server refuses the request, and the caller then reads the message. The refusal is authoritative and actionable, so the experience is correct but reactive; a pre-emptive indicator is recorded in Out of Scope.

## Assumptions

- Every site is created with an enabled administrator, and that administrator cannot be disabled or removed. A site with no enabled administrator therefore cannot arise through the product.
- Only enabled users can authenticate, so only enabled users can reach any of these actions.
- The product already delivers email, so the notifications above reuse that mechanism with new message content.

## Decisions Carried Into This Document

| Decision                                          | Resolution                                                           |
| ------------------------------------------------- | -------------------------------------------------------------------- |
| Removal type                                      | Permanent hard delete; disabling remains the reversible alternative  |
| Financial data on user removal                    | Belongs to the site and stays                                        |
| Username after removal                            | Reusable immediately                                                 |
| Removable account states                          | Enabled, disabled and never-activated                                |
| Last remaining user                               | Never removable                                                      |
| Last enabled administrator                        | Protected against removal, disabling and role removal                |
| Administrator removing an administrator           | Allowed; only the last enabled one is protected                      |
| Self-removal                                      | Any signed-in user, subject to the administrator safeguards          |
| Site deletion                                     | In scope; only by the site's own enabled administrator               |
| Lone administrator deleting the site              | Allowed; their own account is removed with the site                  |
| Site holding the last live platform administrator | Not deletable until that administrator is removed from configuration |
| Site deletion recoverability                      | Immediate; no backup, no grace period, no counts                     |
| Confirmation                                      | Target name plus the actor's current password                        |
| Wrong password                                    | Reported in place; the actor stays signed in                         |
| Notification channel                              | Email, with new message content                                      |
| Audit                                             | Server log only                                                      |
| Bulk removal                                      | Not provided                                                         |
| Retention                                         | None                                                                 |

## Manual Test Plan

Use this after all delivery increments are complete, against the built client and server. It complements the automated unit and integration suites; it does not duplicate them, and it is the only place the end-to-end experience is checked by a person.

### Setup

1. Start the stack with the Docker task `docker-start-client-server`, or run the server and client locally.
2. Point the server at a mail sink you can inspect (a local SMTP capture tool is ideal), so the notification scenarios can be checked. If no mail sink is available, the deletion scenarios still run; mark the notification checks as not run.
3. Create throwaway tenants through the signup flow. Do not use an environment whose data matters, and take a database backup first.
   - **Site A** — Admin A1; a second Admin A2; a Viewer A3; a Pending user A4 (invited, never activated); a Disabled user A5. Add an account with one expense and one income, plus a setting.
   - **Site B** — Admin B1 and one user B2, with an account and one expense.
   - **Site C** — a single Admin C1, used for the lone-administrator cases.
   - **Site D** — a single Admin D1, used for the last-platform-administrator site case.
4. Record the user ids needed for the platform-administrator scenarios:

   ```sql
   SELECT "RowId", "Username" FROM "User" ORDER BY "Username";
   ```

5. Nominate platform administrators by setting `PLATFORM_ADMIN_USERIDS` to the comma-separated ids and restarting the API. Record the previous value so it can be restored afterwards.

### A. Remove another user — Site A, signed in as Admin A1

- **A1 — happy path.** Users → A3 → Delete User…; type `A3`'s username and A1's password; confirm.
  Expected: a success toast; A3 disappears from the list; A3 can no longer sign in; every enabled administrator of Site A (including A1) and A3's address receive an email.
- **A2 — wrong password.** Repeat A1 with a wrong password.
  Expected: the dialog stays open with the invalid-password message; A1 stays signed in; A3 remains; no email is sent.
- **A3 — removable states.** Remove the Pending user A4 and the Disabled user A5.
  Expected: both are removed successfully.
- **A4 — self protection.** Look at A1's own row, on desktop and on a phone-width viewport.
  Expected: no Delete action for the signed-in user.
- **A5 — non-administrator.** Sign in as Viewer A3 and open the Users page.
  Expected: no user-management actions are offered.
- **A6 — foreign target (API).** As A1, send `DELETE /api/users/{B2's RowId}` with a bearer token.
  Expected: `404`; B2 is unchanged.
- **A7 — repeat removal.** As A2, try to remove A3 again after A1 has removed them.
  Expected: the row is no longer in the list; a direct request returns `404`.

### B. Remove your own account — Settings

- **B1 — viewer happy path.** Sign in as A3 → Settings → the delete block; type `A3` and the password; confirm.
  Expected: a success toast; the login page with a brief message; A3 cannot sign in; the remaining enabled administrators of Site A receive an email; A3 receives a confirmation email.
- **B2 — administrator with a peer.** Sign in as A2 and delete the account while A1 remains.
  Expected: allowed, with the same outcome.
- **B3 — last enabled administrator.** Sign in as C1 (Site C's only administrator) and attempt self-deletion.
  Expected: refused with the explanation that the site needs an enabled administrator, and the remedy pointing at deleting the site.
- **B4 — wrong password.** Repeat B1 with a wrong password.
  Expected: the dialog stays open; the user stays signed in; nothing is deleted.
- **B5 — unsaved changes.** Change a setting without saving, then open the delete block, cancel it, and return to the setting.
  Expected: no unsaved-changes prompt from the delete block; the edited setting is still there.
- **B6 — visibility.** Check the block as a Viewer and as an administrator.
  Expected: both see the self-delete block.

### C. Delete a site — Settings, signed in as its administrator

- **C1 — happy path.** Sign in as A1 → Settings → delete-site block; type Site A's name, A1's username and password; confirm.
  Expected: a success toast; the login page; A1, A2, A3, A4 and A5 can no longer sign in; Site A no longer exists with its users, accounts, expenses, incomes and settings; the enabled administrators of Site A receive an email.
- **C2 — viewer.** Sign in as a Viewer in a site that still exists.
  Expected: the site-delete block is absent.
- **C3 — no backup or counts.** Open the site-delete confirmation.
  Expected: the copy states plainly that no backup is taken and that the caller is removed too; no record counts are shown.
- **C4 — lone administrator.** Sign in as C1 (Site C's only administrator) and delete Site C.
  Expected: allowed; the account and site are removed. This is the remedy B3 points at.
- **C5 — last live platform administrator.** Nominate only D1 as a platform administrator, restart, and attempt to delete Site D as D1.
  Expected: refused. Remove D1 from `PLATFORM_ADMIN_USERIDS`, restart, and retry: the deletion succeeds.

### D. Platform administrator safeguards

- **D1 — another enabled administrator remains.** Nominate two enabled administrators of the same site, P1 and P2. As P1, disable P2 through the Users page.
  Expected: allowed; P2 is disabled.
- **D2 — the last one.** Nominate only P1, with another administrator in the same site as the caller. Disable P1.
  Expected: refused with the message naming the configuration-and-redeploy remedy; P1 is unchanged.
- **D3 — a disabled peer does not count.** Nominate P1 (enabled) and P2 (disabled). Disable P1 as another administrator of the site.
  Expected: refused with the same message.
- **D4 — the site rule still applies (API).** Nominate P1 and another enabled platform administrator in a different site. P1 is the only enabled administrator of their own site. Send `PUT /api/users/{P1}/status` with `Disabled` as P1.
  Expected: refused with the site's last-enabled-administrator message; P1 is unchanged.

### E. Sessions, notifications and failure handling

- **E1 — removed user's open tab.** With A3 signed in on another device, remove A3 as A1. Perform an action in A3's tab.
  Expected: the next request is rejected and A3 lands on the login page.
- **E2 — deleted site's open tabs.** With a Site A user signed in on another device, delete Site A. Perform an action in that tab.
  Expected: the next request is rejected and the user lands on the login page.
- **E3 — notification failure.** Stop the mail sink, then remove a throwaway user.
  Expected: the removal succeeds; the failure is visible in the server log; no error is shown for the removal itself.
- **E4 — no orphaned sessions.** After removing a user, attempt a token refresh from a client that still holds their refresh cookie.
  Expected: the refresh is rejected; the stale cookie cannot produce a new session.

### F. Confirmation and presentation

- **F1 — dialog behaviour.** In each of the three confirmations: check the initial focus, the disabled destructive button, trimmed and case-insensitive matching, the busy state, a backdrop click, and a short landscape viewport.
  Expected: Cancel is first and focused by default; the destructive action stays disabled until every value matches; values match regardless of case or surrounding spaces; both actions are disabled while the request runs; a backdrop click does not dismiss; the content scrolls in a short viewport.
- **F2 — copy.** Read each confirmation.
  Expected: deleting a user is distinguished from disabling and states that the site's accounts, expenses and incomes remain; self-deletion states that the site's data remains; site deletion states that every user, including the caller, is removed and that no backup is taken.
- **F3 — menus.** Check the Users list on desktop and on phone width.
  Expected: Delete User… is the last action, visually destructive, and shown only where it applies.

### Cleanup

- Restore `PLATFORM_ADMIN_USERIDS` and restart the API.
- Remove any throwaway tenants and users left behind.
- Restore any mail-sink or database state used for the run.

### Results

| ID | Scenario | Result | Notes |
| --- | --- | --- | --- |
| A1–A7 | Remove another user |  |  |
| B1–B6 | Remove your own account |  |  |
| C1–C5 | Delete a site |  |  |
| D1–D4 | Platform administrator safeguards |  |  |
| E1–E4 | Sessions, notifications and failure handling |  |  |
| F1–F3 | Confirmation and presentation |  |  |
