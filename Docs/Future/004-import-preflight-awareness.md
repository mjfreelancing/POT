# Import Preflight Awareness

Feature ID: 004
Date created: 2026-04-05
Scope: Planning and design only. No code changes applied in this session.
Audience: Future implementation agent(s), reviewers, and maintainers.

## Objective

Give users clear awareness before import when a file may not be compatible with the current app version or may cause semantic changes.

## Problem To Solve

Today, compatibility and schema checks happen at import execution time. Users can discover issues only after selecting and attempting import, which is late feedback and can be confusing.

Examples:

1. The file metadata version is older or newer than the version required by the current app.
2. The file is structurally valid ZIP but contains entry shapes not accepted by the current import contract.
3. The user has no clear pre-import guidance on how to remediate (for example: export from current app version first).

## Desired Outcome

Add a preflight capability so users can see compatibility status and remediation guidance before running import.

## Initial Direction

1. Add a lightweight server preflight endpoint that reads metadata and validates compatibility without mutating data.
2. Return a compact result: compatible, blocking issues, warnings, and suggested next action.
3. Integrate preflight check into UI import flow before final confirm action.

## Related Documents

- Future index: [Docs/Future/README.md](README.md)
- Expense accrual policy plan: [Docs/Future/003-expense-accrual-policy-modes.md](003-expense-accrual-policy-modes.md)
