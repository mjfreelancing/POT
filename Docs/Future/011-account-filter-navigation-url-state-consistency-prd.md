# Account Filter Navigation and URL State Consistency PRD

- Feature ID: 011
- Date created: 2026-05-10
- Status: Complete
- Priority: High
- Audience: Client maintainers, reviewers, QA, and future implementation agents

## Objective

Define a deterministic, scenario-driven contract for account filter behavior across:

1. Expenses and Incomes pages
2. Sidebar navigation between pages
3. Direct URL entry and manual address-bar edits
4. Badge deep links from Accounts
5. Session-storage persistence and fallback behavior

This PRD has been implemented and validated.

Implementation sign-off (2026-05-10):

1. Refactor implemented in client code for Expenses and Incomes account filter behavior.
2. Build and type-check pass.
3. All ten acceptance scenarios (SC-01 through SC-10) validated as functional.

## Business Problem

Recent iterations exposed contradictory expectations between URL behavior and storage persistence:

1. Users expect deep links and manual URL edits to be authoritative and shareable.
2. Users expect cross-page round-trip persistence in many scenarios.
3. Users also expect direct navigation to base routes (for example `/expenses`) to behave intentionally and not force stale prior filters.
4. Current behavior can become inconsistent between what is displayed and what appears in the address bar.

A stable refactor requires explicit scenario-based requirements before selecting architecture.

## Definitions

1. Active filter: the account filter currently applied to page results.
2. Base route: `/expenses` or `/incomes` without `accountId` query parameter.
3. Deep link: URL containing `?accountId=<id>`.
4. Explicit intent action: a user action that intentionally sets account context (badge click, dropdown change, manual URL edit).
5. Address-bar consistency: when a filter is active due to URL intent, the URL and UI must agree.

## Known Scenario Requirements (Authoritative)

### Confirmed Scenarios from Current Discussion

1. SC-01 Sidebar round-trip persistence

- Given Expenses is filtered to Account A
- When user navigates to Incomes via sidebar and then back to Expenses via sidebar
- Then Expenses remains filtered to Account A

2. SC-02 Bookmarked filter takeover

- Given user opens bookmarked Expenses deep link for Account B
- When user navigates to Incomes via sidebar and then back to Expenses via sidebar
- Then Expenses remains filtered to Account B

3. SC-03 Badge override persistence

- Given Expenses is currently filtered to Account C (or unfiltered)
- When user clicks Accounts badge for Expenses Account A
- And user navigates to Incomes via sidebar and then back to Expenses via sidebar
- Then Expenses remains filtered to Account A

4. SC-04 Clear-filter persistence

- Given Expenses is filtered to Account A
- When user clears account filter to unfiltered
- And navigates to Incomes via sidebar and back to Expenses via sidebar
- Then Expenses remains unfiltered

5. SC-05 Invalid/deleted bookmarked account fallback

- Given user opens bookmarked Expenses deep link with accountId that no longer exists
- Then Expenses falls back to unfiltered

6. SC-06 Direct address-bar account change

- Given Expenses is filtered to Account A (or unfiltered)
- When user manually changes URL to `?accountId=AccountB` and loads it
- And user navigates away and back via sidebar
- Then Expenses remains filtered to Account B

7. SC-07 Cross-feature isolation

- Given Expenses is filtered to Account A and Incomes is filtered to Account C
- When user switches between pages via sidebar
- Then each page restores and preserves its own independent last filter

8. SC-08 Edit-route round-trip preservation

- Given Expenses or Incomes is filtered to Account A
- When user opens an edit route or edit dialog from that page
- And then exits the `/edit` route
- Then the previous filtered list view is re-applied
- And the edit route must not clear or replace the active list filter

9. SC-09 Create-route round-trip preservation

- Given Expenses or Incomes is filtered to Account A
- When user opens the create flow from that page
- And then exits the `/create` route or returns back to the list
- Then the previous filtered list view is re-applied
- And the create flow must not clear or replace the active list filter

10. SC-10 Base-route intent and address-bar expectation

- Given user intentionally navigates to base route `/expenses`
- Then the view reflects an unfiltered state
- And that unfiltered state becomes the current state
- And if user navigates away and back again, it remains unfiltered
- And the address bar must remain consistent with that unfiltered state

Note: SC-10 is now resolved as explicit unfiltered base-route behavior.

## Current Behavior Observed in Code

1. The current list pages special-case `/edit/` paths and preserve the list filter while the user is in the edit flow.
2. The product requirement is broader than the current code: exiting either `/edit` or `/create` must re-apply the previous filtered list state.
3. Create flows should be treated the same as edit flows for filter restoration, even if the implementation uses a different route guard or return path.
4. Edit/create round-trips should be considered first-class requirements rather than incidental side effects.

## Non-Negotiable Design Rules

1. No timing-based correctness assumptions (no relying on race windows between render/effects for correctness).
2. One clear source-of-truth policy must be documented for each state transition path.
3. URL semantics must be stable enough for bookmarking and sharing.
4. Invalid account IDs must fail safely to unfiltered.
5. Expenses and Incomes persistence stores must remain isolated.

## Design Recommendation

### Recommended Approach: URL Authority with Dynamic Sidebar Links

**Resolve Q4 as: Sidebar links carry the stored filter in their `href` when one is active.**

The sidebar link for Expenses (or Incomes) is built dynamically at render time:

- If the feature's sessionStorage entry has a valid `selectedAccountId` → link href is `/expenses?accountId=<stored-id>`
- If storage has null or no entry → link href is `/expenses` (base route)

Alongside this, the page's filter resolution rule changes to:

> **URL is the sole source of truth for what the page renders.** The page never falls back to storage at render time. Storage is only written — never read — by the page.

These two changes together resolve all ten scenarios without contradictions.

**Why not the alternative (sidebar always uses base route, page restores from storage on mount):**

SC-01 and SC-10 become mutually exclusive under that approach. SC-01 requires that returning via sidebar restores Account A. SC-10 requires that arriving via sidebar at the base route produces an unfiltered view. Since both flows use the exact same sidebar → Expenses navigation path, no static rule can satisfy both simultaneously. Introducing a secondary signal (e.g., a "cleared" sentinel in storage) would reintroduce timing assumptions and hidden coupling. This direction is eliminated.

### Before State — Current Behaviour Per Scenario

| Scenario | Current behaviour                                                                                                                                                                  | Status                    |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------- |
| SC-01    | Navigate away → back via sidebar → URL is base route → `urlAccountId = null` → falls back to `storedSelectedAccountId = A` → filtered. Works by accident through storage fallback. | Passes (for wrong reason) |
| SC-02    | Bookmark `/expenses?accountId=B` → Sync Rule 3 stores B → round-trip via sidebar → storage fallback restores B. Works by accident.                                                 | Passes (for wrong reason) |
| SC-03    | Badge click stores A and navigates to URL with A → Sync Rule 3 → storage has A → sidebar round-trip restores A via fallback.                                                       | Passes (for wrong reason) |
| SC-04    | Clear sets storage=null, URL=base → sidebar → base route → `null \|\| null = null` → unfiltered.                                                                                   | Passes                    |
| SC-05    | Invalid URL account → fallback effect calls `updateSelectedAccount(null)` → clears storage and URL → unfiltered.                                                                   | Passes                    |
| SC-06    | Manual URL edit → Sync Rule 3 → storage=B → sidebar round-trip via fallback restores B.                                                                                            | Passes (for wrong reason) |
| SC-07    | Storage keys are independent per feature. Both pages restore independently via storage fallback.                                                                                   | Passes                    |
| SC-08    | `isEditing` guard prevents URL updates while on `/edit/` path. Storage retains A. Return restores filter.                                                                          | Passes                    |
| SC-09    | `/create` paths are not guarded. Filter changes from within a create flow could overwrite stored filter or break on return.                                                        | Fails (gap)               |
| SC-10    | Navigate to `/expenses` (base route) → `urlAccountId = null` → falls back to `storedSelectedAccountId = A` → page is **filtered** even though intent was unfiltered.               | Fails (confirmed bug)     |

Additional code defects in the current implementation:

1. `isEditing` uses `window.location.pathname.includes('/edit/')` — bypasses React Router and will misfire on any route containing the literal string `/edit/` for an unrelated reason.
2. Two effects (invalid-account fallback and Sync Rule 3 URL→storage sync) perform the same guard logic over overlapping state. They should be one consolidated effect.
3. `storedSelectedAccountId` is read outside any effect and used in render, making the storage-fallback behaviour implicit and difficult to audit.

### After State — Behavioural Contracts

#### Sidebar Navigation Rule

The sidebar builds each feature's href at render time by reading from sessionStorage:

- If the stored `selectedAccountId` is non-null and non-empty → href is `/expenses?accountId=<stored-id>`
- Otherwise → href is `/expenses`

The sidebar never writes to storage. It reads storage once per render, refreshed on every route transition.

#### Badge Click Rule

`persistLinkedAccountFilter` (unchanged) writes `selectedAccountId` to sessionStorage before the navigation call. The navigation target is `/expenses?accountId=<id>`. This ensures:

1. The target page's URL param drives its initial filter.
2. Storage is updated immediately so the sidebar link reflects the new filter on next render.

#### URL Param Rule

The page derives `selectedAccountId` from the URL parameter only:

- If `?accountId=<id>` is present → use that value (subject to account validity check)
- If no `?accountId` param → `selectedAccountId` is null → page renders unfiltered

There is no storage fallback at render time. Storage is invisible to the page's rendering logic.

#### Storage Write Rule

Storage is written in exactly three situations:

1. Badge click (via `persistLinkedAccountFilter`, before navigation): writes the target account ID.
2. Explicit filter action (dropdown or clear via `updateSelectedAccount`): writes the selected account ID (or null if cleared).
3. Consolidated sync effect: when the URL contains a valid account ID that differs from storage, promotes it to storage so future sidebar links reflect it.

Storage is cleared (written to null) in two situations:

- `updateSelectedAccount(null)` is called (user clears filter via dropdown).
- The consolidated effect detects the URL account ID is invalid (deleted/missing) and calls `updateSelectedAccount(null)`.

Storage is **never read by the page at render time.**

#### Edit and Create Route Guard Rule

A single `isSubRoute` flag replaces the current fragile `isEditing` check. It is derived from the React Router `useLocation()` hook and is true when the current pathname is a recognised child path of the list route:

- `/expenses/edit/<id>` or `/expenses/edit` → `isSubRoute = true`
- `/expenses/create` → `isSubRoute = true`
- `/expenses` or `/expenses?accountId=X` → `isSubRoute = false`

When `isSubRoute` is true:

- `updateSelectedAccount` writes storage but does **not** update the URL.
- The consolidated effect skips all URL→storage sync and invalid-account handling.

This ensures neither edit nor create flows interfere with the list filter URL or storage state.

#### Clear Filter Rule

When the user clears the account filter:

1. `updateSelectedAccount(null)` is called.
2. Storage is written to null.
3. URL params are replaced with empty `URLSearchParams` → URL becomes base route.
4. Page re-renders with `selectedAccountId = null` → unfiltered.
5. Sidebar link for this feature becomes base route on next render.

#### Base-Route Rule

When the page mounts with URL `/expenses` (no `?accountId` param):

- `urlAccountId = null`
- `selectedAccountId = null`
- Page renders unfiltered
- A mount effect writes null to storage so the sidebar link reverts to the plain base route
- Any subsequent return navigation (via sidebar or otherwise) arrives at the base route and remains unfiltered

### Files That Would Need to Change

| File                                                                             | Change intent                                                                                                                                                                                                                                                                                             |
| -------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `src/features/expenses/ExpensesPage.tsx`                                         | Remove `storedSelectedAccountId` from render-time filter derivation. Derive `selectedAccountId` from URL only. Replace `isEditing` (window.location) with a `useLocation`-derived `isSubRoute` guard covering both `/edit` and `/create`. Merge the two URL→storage effects into one consolidated effect. |
| `src/features/incomes/IncomesPage.tsx`                                           | Identical changes to `ExpensesPage.tsx` — the two files mirror each other.                                                                                                                                                                                                                                |
| `src/components/nav/AppSidebarMenus.tsx`                                         | Make the Expenses and Incomes href values dynamic, reading from each feature's sessionStorage entry to decide whether to append `?accountId=<id>` or use the plain base route.                                                                                                                            |
| New utility `src/hooks/useFeatureFilterHref.ts` (or similar shared hook)         | Encapsulates storage key construction and href-building logic for sidebar link use. Keeps the storage key contract in one place and prevents the sidebar from knowing internal storage schema details directly.                                                                                           |
| `src/features/accounts/utils/persistLinkedAccountFilter.ts`                      | No changes required.                                                                                                                                                                                                                                                                                      |
| `src/features/accounts/components/AccountsTable.tsx` and `AccountMobileCard.tsx` | No changes required.                                                                                                                                                                                                                                                                                      |
| `src/features/expenses/hooks/useExpenseStorage.ts` and Income equivalent         | No functional changes required. Hook interface is unchanged.                                                                                                                                                                                                                                              |

### Risks and Open Questions

1. **SC-10 scope boundary (resolved):** Manually navigating to the base route (`/expenses` or `/incomes` with no `?accountId` param) is an explicit clear-filter intent. The page must write null to storage on mount when no `urlAccountId` is present. This ensures the sidebar link reverts to the plain base route so that any subsequent return navigation (via sidebar or otherwise) also arrives unfiltered. An additional mount effect is required in the refactor to implement this write.

2. **Exit navigation from edit/create routes (resolved):** Current behavior already correctly restores the list with the account filter that was present prior to entering the edit/create flow. This is guaranteed by the `isSubRoute` guard preventing storage writes during edit/create, and by using `navigate(-1)` or return navigation that preserves the filter state. SC-08/SC-09 acceptance criteria capture this requirement explicitly. No additional mitigation required.

3. **`not-assigned` special token (resolved):** Treat `not-assigned` the same as null/cleared — it represents an unfiltered state. When the URL contains `?accountId=not-assigned`, it should be promoted to storage via Sync Rule 3 just as any other valid account ID would be, so sidebar links reflect the unfiltered state and round-trip navigation preserves it.

4. **Sidebar storage reactivity and UX mitigation (noted):** The sidebar reads storage at render time. If the user changes the filter via dropdown while remaining on the Expenses page, the sidebar Expenses link does not update visually until the next navigation causes a sidebar re-render. Additionally, a user could accidentally click the Expenses sidebar link while already on the Expenses page, causing an unnecessary reload/navigation. The implementation phase should add visual or behavioral mitigation — for example, disable the sidebar link for the current page, or show a visual indicator that the item is already active. This is a UX concern, not a correctness issue.

5. **Effect dependency arrays on consolidation (implementation note):** The merged effect must carry the union of both current effects' dependency arrays. During implementation, verify the dependency array exactly, particularly for any storage-comparison values that are no longer needed in render but may still be referenced inside the consolidated effect, to prevent stale closures.

## Acceptance Criteria for Final Refactor PR

1. Final implementation must pass SC-01 through SC-10 exactly.
2. SC-10 must be documented in both code comments and release notes as explicit unfiltered base-route behavior.
3. Lint, type-check, and production build must pass.
4. Manual QA checklist must include all ten scenarios.
5. Any behavior change from current production must be called out before merge.

## Out of Scope

1. Server-side filtering semantics.
2. API contract changes.
3. Account entity model changes.

## References

- [Docs/Future/README.md](README.md)
- [Source/Client/pot-react/src/features/expenses/ExpensesPage.tsx](../../Source/Client/pot-react/src/features/expenses/ExpensesPage.tsx)
- [Source/Client/pot-react/src/features/incomes/IncomesPage.tsx](../../Source/Client/pot-react/src/features/incomes/IncomesPage.tsx)
- [Source/Client/pot-react/src/features/accounts/components/AccountsTable.tsx](../../Source/Client/pot-react/src/features/accounts/components/AccountsTable.tsx)
- [Source/Client/pot-react/src/features/accounts/components/AccountMobileCard.tsx](../../Source/Client/pot-react/src/features/accounts/components/AccountMobileCard.tsx)
- [Source/Client/pot-react/src/components/nav/AppSidebarMenus.tsx](../../Source/Client/pot-react/src/components/nav/AppSidebarMenus.tsx)
