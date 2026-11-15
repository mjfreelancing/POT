# PRD-0036 Client feature import boundaries — Discovery

> Scratch document. Not a PRD and not a durable record. Delete this file once
> `PRD-0036-client-feature-import-boundaries.md` exists and has been approved.

**Feature ID:** 0036
**Last Updated:** 2026-11-15
**Status:** Planning

## Captured input

On 2026-11-15 the owner decided how to reconcile the client's documented one-way dependency rule with the
code, and asked for this discovery to start:

> yes, start the /prd-discovery now - even if we only capture the initial basics and follow up later.
> I don't want to forget to action this

The decision, in the owner's words:

> Move the cross-cutting pieces into layers, then enforce "no feature→feature" with lint.

What that means concretely: the permission components and the auth and access-token contexts,
`sessionNotice` and `useClearSession` move to `concerns/auth`; `features/shared/sheets` moves to
`components/`; the renew helpers move to `api/` or `concerns/`; and the reverse imports from `api/`,
`components/` and `concerns/` are fixed at the same time. Effort M, client only, import moves with no
behaviour change.

Two alternatives were weighed on the day and dropped. They are recorded here because nothing else keeps
them:

- **Delete the rule and record the reality.** Features may import each other; documentation only.
  Cheapest, but it leaves the dependency graph unconstrained.
- **Sanction a narrow set instead.** Any feature may import `auth`'s public exports and
  `features/shared`, and nothing else, with a lint rule. Cheaper than what was chosen, but it keeps
  `auth` as a feature that everything depends on.

## Draft problem statement

The client documents a one-way dependency rule: features may use the shared layers (`api/`, `concerns/`,
`components/`, `lib/`), and those layers must never depend on a feature. It is stated in
`.github/instructions/client.instructions.md` and, in the same words, in `Source/Client/pot-react/DEVELOPER.md`
("Features never import from other features (use shared utilities instead)").

The code does not follow it, in both directions. Seven feature files import another feature — five in
`dashboard` (`RenewExpensesAction`, `RenewIncomesAction`, `ExpenseCard`, `IncomeCard`, `QuickActions`) and
two in `userSettings` (`DeleteAccountAction`, `DeleteSiteAction`) — almost all of them through
`@/features/auth`. Five files in the shared layers import a feature: `src/api/hooks/useMe.ts`,
`src/api/hooks/useSignup.ts`, `src/components/nav/AppSidebarMenus.tsx`,
`src/components/user/UserMenu.tsx` and `src/concerns/auth/logoutManager.ts`.

The reverse imports are the ones that matter. When a component or a hook in a shared layer imports
`@/features/auth`, `auth` is being used as a layer, not as a feature: the boundary the documents describe
does not exist, and a reader cannot tell which directories are features and which are infrastructure.
Nothing enforces the rule either — `eslint.config.js` has no import-boundary or `no-restricted-imports`
setting — so the drift continues with each new feature rather than being a one-off.

The consequence today is small and structural: no user-visible behaviour depends on it. The cost is paid
by every later change, because the dependency graph gives no signal about where code belongs.

## Scope sketch

**Appears in scope**

- Move `src/features/shared/sheets/asyncSheetStates` to `components/`.
- Move the permission components (`src/features/auth/components/PermissionGuard.tsx`, `WithPermission.tsx`)
  and the auth and access-token contexts, `sessionNotice` and `useClearSession` to `concerns/auth`.
- Move the `dashboard` renew helpers — currently reached through `src/features/expenses/bulkActions/renew.ts`
  and the incomes equivalent — to `api/` or `concerns/`.
- Fix the reverse imports so that `api/`, `components/`, `concerns/` and `lib/` import nothing from
  `features/`.
- Add a lint rule that fails a feature-to-feature import and a shared-layer-to-feature import.
- Update any document that names a path which moves, including the folder list in
  `Source/Client/pot-react/DEVELOPER.md` and the anchors listed in
  `.github/instructions/e2e.instructions.md` if a path there changes.

**Appears out of scope**

- Any behaviour change. This is import moves and a lint rule; if a test changes, that is a signal the
  refactor went wrong.
- The server, the API contract and the database — none is affected.
- Other client improvements that touch the same files but are separate decisions: replacing
  permission-policy string literals with named constants, and generating the client's API types from the
  committed contract.
- Splitting `features/auth` itself into smaller concerns beyond what the move requires.

## Open Questions

- **Where the renew helpers live.** `api/` or `concerns/` — the decision allows both. They are
  mutation-plus-cache-invalidation helpers that `dashboard` composes; `api/` matches their transport
  shape, `concerns/` matches their cross-feature purpose. Decide before the move, because it decides who
  else may import them.
- **Whether `sessionNotice` and `useClearSession` move as-is.** Both are named in the agreed moves but
  neither was inventoried by import site; the move may reveal other consumers.
- **Which lint mechanism.** `no-restricted-imports` with per-directory patterns, or a boundary plugin
  such as `eslint-plugin-boundaries` or `eslint-plugin-import`'s `no-restricted-paths`. The repository
  already uses `simple-import-sort` but no boundary plugin.
- **Whether `userSettings` keeps any of its imports.** Once the contexts live in `concerns/auth`, its two
  import sites should be satisfied by the shared layer — worth confirming rather than assuming.
- **Whether the E2E anchor list needs updating.** The instruction lists accessible names and
  `data-slot` anchors with the files that define them; a move changes the paths, not the anchors.
