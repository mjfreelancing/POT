# PWA Update Change Awareness

Feature ID: 005
Date created: 2026-04-05
Scope: Planning and design only. No code changes applied in this session.
Audience: Future implementation agent(s), reviewers, and maintainers.

## Objective

Inform users when a new deployed app version is detected in the PWA, including important feature changes and breaking behavior changes.

## Problem To Solve

Users can continue using stale assumptions after an update, especially when behavior changes are intentional and important (for example import/export compatibility changes).

Without a version-aware notification pattern:

1. Breaking changes can feel unexpected.
2. Users may perform workflows in the wrong order.
3. Support burden increases because context is missing at the moment of change.

## Desired Outcome

When a new version is detected, show targeted notifications explaining what changed, impact level, and recommended actions.

## Initial Direction

1. Detect app version changes in the PWA update lifecycle.
2. Store last-seen version per user/client context.
3. Show release note cards/toasts for high-impact changes, with acknowledgement state.
4. Allow linking to a fuller change log or migration guidance.

## Related Documents

- Future index: [Docs/Future/README.md](README.md)
- Import preflight awareness: [Docs/Future/PRD-004-import-preflight-awareness.md](PRD-004-import-preflight-awareness.md)
