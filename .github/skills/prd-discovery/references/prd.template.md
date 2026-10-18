# <Feature name> PRD

**Status:** Planning
**Priority:** <High | Medium | Low>
**Last Updated:** <yyyy-mm-dd>
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

| Actor | Definition |
| --- | --- |
| <actor> | <what makes someone this actor> |

| Action | Who may perform it | What it affects |
| --- | --- | --- |
| <action> | <actor> | <blast radius> |

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
reading the discovery transcript.>

| Decision | Resolution |
| --- | --- |
| <decision> | <resolution> |

## Open Questions

<Only what genuinely remains open. Each phrased so it makes sense without the discovery conversation,
and each with what is needed to close it. Delete the section when there are none.>

- **<question>** — <why it is still open and what would resolve it>.

## Manual Test Plan

<Executed by a person against the built product at the end of delivery. Complements the automated
suites; do not restate what they already assert.>

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

| ID | Scenario | Result | Notes |
| --- | --- | --- | --- |
| A1–A2 | <scenario group> |  |  |
| B1 | <scenario group> |  |  |
