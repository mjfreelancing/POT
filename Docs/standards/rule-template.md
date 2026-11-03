# <Area> rules: <topic>

Owned by <the standards ADR for this area>. Start with a quick-reference list of every rule in this
document, then state each rule once, in the same order.

## Quick reference

| # | Rule | Enforcement | Provenance |
| --- | --- | --- | --- |
| 1 | <rule as a short imperative> | `[error]` | platform convention |
| 2 | <rule> | `[review]` | project decision |

## Rules

### 1. <Rule>

- **Enforcement:** `[error]` — <the analyzer, linter or test that fails when this is broken>, or
  `[review]` — <what a reviewer checks>, or `[test]` — <the fixture that pins it>.
- **Provenance:** <platform convention | project decision | diverges from the platform convention>,
  because <one line>.
- **Where a tool decides it:** <`.editorconfig` key, linter rule or `tsconfig` option>, if one does.

<One or two paragraphs: what the rule requires, and the smallest example that makes it unambiguous.
Link to a worked example beside the code rather than copying one.>

### 2. <Rule>

- **Enforcement:**
- **Provenance:**
- **Where a tool decides it:**

<…>
