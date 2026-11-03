---
id: ADR-0000
title: <the decision in one line, as a statement about the system>
status: proposed        # proposed | accepted | rejected | deprecated | superseded
date: yyyy-mm-dd        # when the decision was made
area: cross-cutting     # frontend | backend | cross-cutting | infrastructure | process
related-prds: []        # optional
supersedes: []          # optional
superseded-by: []       # optional
---

# ADR-0000: <title, matching the front matter>

**Summary:** In the context of <the situation>, facing <the force or problem>, we decided
<the decision>, to achieve <the goal>, accepting <the main cost>.

## Context and Problem Statement

<Three to ten lines: the forces, not the history of the discussion. Link the PRD section or the code
that shows the problem. If this record is backfilled — written after the decision was already in the code
— say so here and name what it was reconstructed from.>

## Decision Drivers            <!-- optional -->

- <driver>

## Considered Options

1. <option, including the status quo for a backfilled record>
2. <option>
3. <option>

## Decision Outcome

Chosen option: <n>, because <at most fifteen lines>.

### Consequences

- Good, because <…>
- Bad, because <…>

### Confirmation

- Test: <the fixture or test that fails if the decision is undone>
- Instruction: `<path>` (<section>)
- Review: <the rule a reviewer applies>

## Pros and Cons of the Options   <!-- optional -->

## More Information               <!-- optional: links and follow-ups; never implementation status -->
