# <Feature name> PRD

**Status:** Planning
**Priority:** <High | Medium | Low>
**Last Updated:** <yyyy-mm-dd>
<!-- **Amended:** <yyyy-mm-dd> — <count> amendments, see [Amendments](#amendments). Uncomment only once the document is Complete and a later change has overtaken it. -->
**Feature ID:** NNN
**Discovery:** complete
**Scope:** <One paragraph. What the feature does, which actors it serves, and — explicitly — what it
does not cover and where those parts live instead. This paragraph is the contract the delivery plan is
checked against.>

## Problem Statement

<What is wrong today, who it affects, and why it matters. State the current behaviour with evidence:
what the product does now, and what it forces people to do instead. Do not describe the solution here.>

## Actors and Authority

<Include when more than one actor has different powers. Omit when there is only one.>

| Actor   | Definition                      |
| ------- | ------------------------------- |
| <actor> | <what makes someone this actor> |

| Action   | Who may perform it | What it affects |
| -------- | ------------------ | --------------- |
| <action> | <actor>            | <blast radius>  |

<Any authority deliberately withheld, and from whom.>

## Terms

<Only words used repeatedly with a specific meaning. Omit the section if there are none.>

- **<term>** — <definition, precise enough that two readers would draw the same line>.

## Functional Requirements

<Group by behaviour area. Number within each group using a stable prefix — `A1`, `A2`, `B1` — so any
requirement can be referenced in review and in discussion. State behaviour, not implementation: no
endpoint paths, class names or component names unless the contract genuinely requires them.>

### A. <behaviour area>

- **A1.** <requirement>
- **A2.** <requirement>
- **A3.** <requirement>

### B. <behaviour area>

- **B1.** <requirement>
- **B2.** <requirement>

## Requirement Coverage

<Every requirement, and what proves it. Name the test, suite or manual scenario that fails if the
requirement stops holding; a requirement with no row is either untested or not really a requirement.
Fill this in as delivery proceeds, and keep it accurate at completion.>

| Requirement | Verified by |
| ----------- | ----------- |
| A1          | <suite and fixture, or manual scenario ID> |

## Out of Scope

<Each excluded item, and where it belongs instead — another feature, another document, or future work.
An exclusion without an owner is how scope quietly returns.>

- <item> — <where it belongs instead>

## Known Risks and Accepted Limitations

<Behaviours that are understood, accepted and not being fixed. Each with enough context that a reader
can judge whether the risk is still acceptable.>

- **<risk>** — <what happens, why it is accepted, and what would change the decision>.

## Assumptions

<Things taken as true that, if false, invalidate requirements above.>

- <assumption>

## Decisions Carried Into This Document

<The decisions reached during discovery, as a table, so a reader can see what was settled without
reading the discovery transcript. A decision that meets the significance test becomes an ADR, and the
`ADR` column names it; a product rule stays here with no ADR.>

| Decision   | Resolution   | ADR |
| ---------- | ------------ | --- |
| <decision> | <resolution> | <ADR-NNNN, or "—"> |

## Effect on Earlier Documents

<Filled in during discovery. Every Complete document this feature makes wrong, and the amendment entry
that will record it. The delivery skill writes the entries into those documents at close-out; leave a
row empty only when the change needs no amendment.>

| Document | What changes | Amendment |
| -------- | ------------ | --------- |
| <PRD-NNN, ADR-NNNN or reference page> | <what it now says wrongly> | <AM-n, or "(rewrite)"> |

## Open Questions

<Only what genuinely remains open. Each phrased so it makes sense without the discovery conversation,
and each with what is needed to close it. Delete the section when there are none.>

- **<question>** — <why it is still open and what would resolve it>.

## Manual Test Plan

<Executed by a person against the built product at the end of delivery. Complements the automated
suites; do not restate what they already assert. If the automated suites cover every behaviour this
plan would check, replace the setup, scenarios, cleanup and results table with a single line —
`**Not required** — <the suites and tests that cover it>` — and say why no manual check adds value.>

### Setup

1. <How to start the stack or environment.>
2. <Data to create before testing, named so scenarios can refer to it.>
3. <Anything to record before starting, so it can be restored afterwards.>

### A. <scenario group>

- **A1 — <scenario>.** <Steps.>
  Expected: <the observable result.>
- **A2 — <scenario>.** <Steps.>
  Expected: <the observable result.>

### B. <scenario group>

- **B1 — <scenario>.** <Steps.>
  Expected: <the observable result.>

### Cleanup

- <How to restore the environment and remove test data.>

### Results

| ID    | Scenario         | Result | Notes |
| ----- | ---------------- | ------ | ----- |
| A1–A2 | <scenario group> |        |       |
| B1    | <scenario group> |        |       |

## Amendments

Append-only, oldest first. The text above is this document as approved; read it through these entries.
None.

## Verification Evidence

<Filled in by the delivery skill at close-out, from the stage plan. This is the part of the plan that
cannot be reconstructed once it is deleted.>

### Deliberately broken, one at a time

<One row per fault deliberately introduced to prove an assertion fires: what was broken, what the suite
answered, and the verdict.>

| Fault introduced | What the suite answered | Verdict |
| ---------------- | ----------------------- | ------- |
| <fault>          | <failure observed>      | <proved> |

### Measured baselines

<Numbers a later reader would compare against to spot drift — suite durations, test counts, build
times — each with the date it was taken and the machine it was taken on.>

| Measure | Value | Date | Conditions |
| ------- | ----- | ---- | ---------- |
| <measure> | <value> | <yyyy-mm-dd> | <idle machine, cold build, …> |

### What the evidence cannot show

<The limits: a tool that cannot instrument the code, a suite that was not run, a result that holds only
on an idle machine. Say it here so the numbers above are not read as covering more than they do.>

- <limit>
