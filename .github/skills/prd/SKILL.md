---
name: prd
description: 'Generate high-quality Product Requirements Documents (PRDs) for software systems and AI-powered features. Includes executive summaries, user stories, technical specifications, spikes, and risk analysis.'
license: MIT
---

# Product Requirements Document (PRD)

## Overview

Design comprehensive, production-grade Product Requirements Documents (PRDs) that bridge the gap between business vision and technical execution. This skill works for modern software systems, ensuring that requirements are clearly defined.

## When to Use

Use this skill when:

- Starting a new product or feature development cycle
- Translating a vague idea into a concrete technical specification
- Defining requirements for AI-powered features
- Stakeholders need a unified "source of truth" for project scope
- User asks to "write a PRD", "document requirements", or "plan a feature"

---

## Operational Workflow

### Phase 1: Source Intake (User-Provided File)

If the user provides an input file (brain dump, bullet list, meeting notes, or draft PRD), you **MUST** treat it as the primary source.

**Intake rules:**

- Accept input from one or more user-provided files.
- Preserve original meaning and wording when moving content into PRD sections.
- Re-home content into the most relevant schema sections instead of rewriting it.
- If a source item fits multiple sections, place it in the most decision-critical section and cross-reference it.
- If content is ambiguous, keep it in-place under a clearly labeled `TBD` or `Open Question` note rather than rewriting.

### Phase 2: Source Maturity Classification (Hard Gate)

Before drafting, classify the source into exactly one mode:

- **Discovery PRD Draft**: Source contains open questions, unknowns, options, assumptions, and unresolved decisions.
- **Delivery PRD Draft**: Source has confirmed scope, confirmed owners, and implementation-ready requirements.

Classification signals:

- Treat as **Discovery PRD Draft** when the source includes phrases like "brain dump", "questions", "TBD", "unknown", "decision pending", "investigate", "options", or comparable uncertainty markers.
- Treat as **Delivery PRD Draft** only when requirements are explicitly confirmed and uncertainties are minor.
- If uncertain, default to **Discovery PRD Draft** and ask the user to confirm.

Hard rules from classification:

- In **Discovery PRD Draft**, do not create committed user stories, owners, team assignments, or final roadmap commitments.
- In **Discovery PRD Draft**, output must prioritize: known facts, assumptions register, open questions register, open decisions backlog, and discovery exit criteria.
- In **Delivery PRD Draft**, user stories and ownership are allowed only when confirmed by source or by user answers.

### Phase 3: Discovery (The Interview)

Before writing a single line of the PRD, you **MUST** interrogate the user to fill knowledge gaps. Do not assume context.

**Interview execution requirement (non-optional):**

- Ask the questions first, then draft.
- Use a blocking interview of at least 4 questions.
- Use the `vscode_askQuestions` tool when available so answers are explicitly collected before drafting.
- Do not skip interview because of confidence, prior examples, or inferred context.
- If the user explicitly says "no questions", proceed with a discovery draft and label unresolved fields as `TBD`.

**Ask about:**

- **Formality**: What tone and level of detail are expected? Is this for a single developer, a cross-functional team, or executive stakeholders?
- **The Core Problem**: Why are we building this now?
- **Success Metrics**: How do we know it worked?
- **Constraints**: Budget, tech stack, or deadline?

**Always include these additional questions:**

- **Maturity Check**: Is this a discovery PRD draft or a delivery-ready PRD?
- **Audience Check**: Who is the primary audience and decision maker?
- **Authority Check**: Are owners/teams known, or should all ownership remain `TBD`?
- **Commitment Check**: Should user stories be deferred until discovery questions are closed?

### Phase 4: Analysis & Scoping

Synthesize the user's input. Identify dependencies and hidden complexities.

- Route the PRD format based on the requested **Formality** and audience.
- Map out the **User Flow**.
- Define **Non-Goals** to protect the timeline.

If mode is **Discovery PRD Draft**, replace "User Stories" with:

- Open Decisions Backlog
- Outstanding Questions Register
- Assumptions and Constraints Register
- Discovery Exit Criteria

### Phase 5: Technical Drafting

Generate the document using the **Core PRD Schema** below.

Drafting safeguards:

- Do not invent teams, roles, owners, or responsibilities.
- Do not label options as decisions unless user or source confirms them.
- For uncertain content, preserve wording and mark as `TBD`, `Open Question`, or `Decision Pending`.

### Phase 6: Fidelity Check (No Silent Edits)

Before finalizing, verify that source content has been transferred without silent modification.

- Do not alter source claims, constraints, or decisions without user approval.
- If a change is strongly recommended, present it as a **suggestion** and ask the user how to proceed.
- Separate drafted content from suggestions using explicit labels such as `Proposed Change` and `Reason`.

---

## Source Mapping Rules (When Starting From Existing Material)

When the user provides source material, map it into the PRD schema as follows:

- Problem statements, business drivers -> `1. Executive Summary`
- User behaviors, workflows, operator interactions -> `2. User Experience & Functionality`
- AI/tooling/evaluation content -> `3. AI System Requirements (If Applicable)`
- APIs, architecture, data, security, integration details -> `4. Technical Specifications`
- Risks, unknowns, dependencies, rollout sequencing -> `5. Risks & Roadmap`

If a source already resembles a PRD section, keep the original structure and wording and only normalize formatting.

---

## PRD Quality Standards

### Requirements Quality

Use concrete, measurable criteria. Avoid "fast", "easy", or "intuitive".

```diff
# Vague (BAD)
- The search should be fast and return relevant results.
- The UI must look modern and be easy to use.

# Concrete (GOOD)
+ The search must return results within 200ms for a 10k record dataset.
+ The search algorithm must achieve >= 85% Precision@10 in benchmark evals.
+ The UI must follow the 'Vercel/Next.js' design system and achieve 100% Lighthouse Accessibility score.
```

---

## Core PRD Schema

You **MUST** include the core sections below. Adapt depth, wording, and supporting artifacts to the audience formality selected in discovery.

### Formality Modes (Required Routing)

- **Lean Solo Mode**: For a single developer, especially when uncertainty is high or stack familiarity is low. Keep the PRD concise, decision-oriented, and learning-focused.
- **Team Delivery Mode**: For cross-functional delivery with owners, acceptance criteria, and trackable handoffs.
- **Executive Readout Mode**: For stakeholder alignment. Prioritize business impact, risks, dependencies, and measurable outcomes over implementation detail.

Routing rules:

- If user signals solo exploration, default to **Lean Solo Mode** unless they request formal governance artifacts.
- If multiple teams or explicit handoffs are involved, use **Team Delivery Mode**.
- If audience is leadership or sponsors, use **Executive Readout Mode** and compress technical depth.
- If uncertain, ask one follow-up to confirm mode before drafting.

### Delivery Maturity Modes (Required Routing)

- **Discovery PRD Draft**: Unknown-heavy artifact for collation and decision shaping.
- **Implementation PRD**: Confirmed artifact for committed delivery planning.

Routing rules:

- If the user says "brain dump", "starting point", or asks to collate unknowns, force **Discovery PRD Draft**.
- If the user says requirements are confirmed and asks for execution planning, use **Implementation PRD**.
- If mixed signals exist, ask a direct maturity question before drafting.

### 1. Executive Summary

- **Problem Statement**: 1-2 sentences on the pain point.
- **Proposed Solution**: 1-2 sentences on the fix.
- **Success Criteria**: 3-5 measurable KPIs.

### 2. User Experience & Functionality

- **User Personas**: Who is this for?
- **User Stories / Outcome Slices**: Use `US-01`, `US-02`, etc. when formal stories are needed. In lean solo mode, concise outcome slices are acceptable when they are easier to execute.
- **Acceptance Criteria**: Bulleted list of "Done" definitions for each story.
- **Non-Goals**: What are we NOT building?

Discovery-mode override:

- If document mode is **Discovery PRD Draft**, this section must not contain committed user stories.
- Replace with candidate workflow notes, unknowns, and decision checkpoints.

### 3. AI System Requirements (If Applicable)

- **Tool Requirements**: What tools and APIs are needed?
- **Evaluation Strategy**: How to measure output quality and accuracy.

### 4. Technical Specifications

- **Architecture Overview**: Data flow and component interaction.
- **Integration Points**: APIs, DBs, and Auth.
- **Security & Privacy**: Data handling and compliance.

### 5. Risks & Roadmap

- **Phased Rollout**: MVP -> v1.1 -> v2.0.
- **Spikes (If Applicable)**: List uncertainty-reduction investigations with sequential IDs such as `SP-01`, `SP-02`.
- **Technical Risks**: Latency, cost, or dependency failures.

---

## Work Item Guidance

Use the following rules to decide when a PRD should include stories, spikes, and related supporting artifacts.

### When to Add PRD Stories

Add PRD stories when the document needs clear, trackable outcome statements that can be reviewed, owned, and accepted independently.

Include stories when:

- The PRD covers multiple distinct deliverables or workstreams.
- Different stakeholders need separate acceptance criteria.
- The initiative includes readiness gates, rollout gates, or explicit sign-off criteria.
- The work is investigative but still needs concrete outputs such as matrices, reports, plans, or recommendations.
- A solo developer still needs explicit sequencing for decisions or deliverables.
- A solo developer has a significant-effort item that benefits from a separate, trackable execution document.

Do not create filler stories just to mirror every section heading. Stories should represent reviewable outcome slices, not document structure.

For lean solo mode, stories are optional for small items. For significant-effort items, stories are recommended so planning, tasking, and progress can be tracked independently.

### PRD Story Format

Prefer the following pattern when stories are needed:

```markdown
## 2. User Experience & Functionality

- **User Stories**:

| Story ID | Summary | Owner | Status |
|---|---|---|---|
| US-01 | Short outcome-oriented summary | TBD | Defined |

  - US-01:
    - As a [role], I want [outcome] so that [decision/value].
    - Acceptance Criteria:
      - Concrete, measurable requirement.
      - Concrete, measurable requirement.
      - Explicit completion or sign-off condition.
```

Guidance:

- Use stable, sequential IDs such as `US-01`, `US-02`.
- Use summaries that describe the deliverable, not the activity.
- Acceptance criteria should be testable or reviewable, not aspirational.
- In lean solo mode, use stories for significant efforts and keep each story outcome-focused, then track implementation tasks in a separate story document.

Lean solo alternative (acceptable when formality is low):

```markdown
- US-01: Validate approach for [feature]
  - Done when: A working path is documented, known blockers are listed, and the next implementation step is clear.
```

Solo story tracking pattern (recommended for significant effort):

```markdown
## Story Tracking Index

| Story ID | Title | Document | Status |
|---|---|---|---|
| US-01 | Implement feature foundation | [US-01 - Implement feature foundation.md](Stories/US-01%20-%20Implement%20feature%20foundation.md) | In Progress |
```

Example separate story document template:

```markdown
# US-01 - Implement feature foundation

| Field | Value |
|---|---|
| **Status** | In Progress |
| **Owner** | Solo Developer |
| **Related PRD** | [Example PRD.md](../Example%20PRD.md) |
| **Last Updated** | YYYY-MM-DD |

## Outcome

As a developer, I want [outcome] so that [value].

## Acceptance Criteria

- Concrete, measurable requirement.
- Concrete, measurable requirement.

## Task Checklist

- [ ] Task 1
- [ ] Task 2
- [ ] Task 3

## Notes / Decisions

- Decision or discovery log.
```

### When to Add Spikes

Add a spike when there is material technical uncertainty that must be reduced before implementation or rollout decisions can be made.

Create a spike when one or more of the following is true:

- A third-party dependency may block the initiative.
- Architecture viability is unknown and requires prototype or vendor validation.
- A compatibility question depends on runtime behavior, environment state, or deployment topology.
- The team needs an explicit investigation artifact with findings and recommendation.
- The result may change rollout policy, platform support, or mitigation strategy.
- Delivery risk is driven by a developer or team knowledge gap in the chosen stack, and a time-boxed investigation is needed before committing implementation scope.

Do not create a spike for routine implementation work, ordinary refactoring, or items that are already known and only need execution.

### When Not to Use a Spike

Use a validation task or tracked dependency item instead of a spike when:

- The answer is already substantially known and only needs confirmation.
- The work is owned by another initiative but should be monitored in the PRD.
- The outcome is simple status validation rather than open-ended investigation.

Examples:

- Managed dependency appears x64-safe and only needs representative validation.
- A shared module owned by another team must be rebuilt or version-aligned, but the current product team is not the primary investigation owner.

### PRD Roadmap Formatting for Spikes and Validation Tasks

When spikes exist, list them explicitly in the roadmap phase where uncertainty is being reduced. If some items are narrower than spikes, label them as validation tasks instead of forcing them into spike form.

```markdown
- Phase 1.1: Dependency qualification spikes
  - SP-01: [dependency or architecture uncertainty].
  - SP-02: [dependency or architecture uncertainty].
  - Validation task: [known item that still needs confirmation].
```

If spikes are used, also add an appendix table that links to the detailed spike documents.

```markdown
## Appendix B: Investigation Spike Documents

| Spike | Title | Document | Status |
|---|---|---|---|
| SP-01 | Short title | [SP-01 - Example.md](Spikes/SP-01%20-%20Example.md) | Not Started |
```

### Spike Document Template

When a PRD introduces spikes (if applicable), prefer separate spike documents using a consistent template like this:

```markdown
# SP-01 - Example Title

| Field | Value |
|---|---|
| **Status** | Not Started |
| **Owner** | TBD |
| **Related PRD** | [Example PRD.md](../Example%20PRD.md) |
| **Last Updated** | YYYY-MM-DD |

## Objective

One clear statement of the uncertainty to resolve and the decision this spike must support.

## Acceptance Criteria

- Known baseline evidence is documented.
- Questions to answer are explicit.
- Findings/recommendation format is clear.
- Open dependencies and follow-up actions are identified.

## Background

Known baseline facts and evidence.

## Questions to Answer

- Key uncertainty.
- Key uncertainty.

## Findings

Working findings or finalized findings as evidence is gathered.

## Recommendation

To be completed at spike conclusion.

## Open Issues

Record unresolved blockers here.

## References

- Related PRD
- Relevant code/docs/vendor references
```

Guidance:

- Treat each spike as the investigation unit; do not add a second nested story layer unless there is a very specific reason to subdivide the spike document.
- Use `SP-01`, `SP-02`, etc. consistently between the PRD and spike documents.
- Use the spike document to capture investigation detail; keep the PRD itself at decision-summary level.
- If a spike becomes mostly known up front, document the known baseline clearly and leave only true uncertainties in the questions/findings sections.

### Other Supporting Artifacts

Consider adding these when the initiative is architecture-heavy or investigative:

- Assumptions register when key decisions rely on unvalidated beliefs.
- Discovery question backlog when the PRD is being iterated and unknowns are still being harvested.
- Workstream ownership table when multiple teams or roles contribute different outputs.
- Decision gates when rollout depends on passing explicit readiness thresholds.

These should be added only when they materially improve decision-making or execution clarity.

---

## Implementation Guidelines

### DO (Always)

- **Define Testing**: For AI systems, specify how to test and validate output quality.
- **Iterate**: Present a draft and ask for feedback on specific sections.
- **Match Audience Formality**: Confirm whether output is for solo build execution, team delivery, or executive alignment, then tune depth and artifact rigor accordingly.
- **Preserve Source Fidelity**: Keep user-provided source wording intact when migrating content into PRD sections.
- **Ask Before Changing Meaning**: If a high-value edit changes intent, ask the user whether to apply it.
- **Confirm Maturity Mode**: Explicitly confirm discovery vs implementation before producing delivery artifacts.
- **Keep Ownership Honest**: Use `TBD` for owner/team when not provided by user or source.

### DON'T (Avoid)

- **Skip Discovery**: Never write a PRD without asking at least 4 clarifying questions first, including formality/audience and unknowns.
- **Hallucinate Constraints**: If the user didn't specify a tech stack, ask or label it as `TBD`.
- **Silently Rewrite Source Material**: Do not paraphrase away important details from user-provided files.
- **Invent Organization Design**: Do not fabricate teams, owners, or responsibilities.
- **Force Stories in Discovery**: Do not generate committed user stories when the source is uncertainty-heavy.

---

## Example: Intelligent Search System

### Example A: Lean Solo Mode (Unknown-Heavy)

**Audience**: Single developer exploring an unfamiliar stack.

- **Story Tracking**: Track story progress in a separate index, e.g. [Story Tracking Index.md](Stories/Story%20Tracking%20Index.md).

- **US-01**: Validate retrieval strategy for repository docs.
  - Done when: Candidate approach is selected and tradeoffs are documented.
- **SP-01**: Time-box indexing feasibility spike (4 hours).
  - Output: Feasibility note, benchmark snapshot, recommendation.

### Example B: Team Delivery Mode

### 1. Executive Summary

**Problem**: Users struggle to find specific documentation snippets in massive repositories.
**Solution**: An intelligent search system that provides direct answers with source citations.
**Success**:

- Reduce search time by 50%.
- Citation accuracy >= 95%.

### 2. User Stories

- **Story**: As a developer, I want to ask natural language questions so I don't have to guess keywords.
- **AC**:
  - Supports multi-turn clarification.
  - Returns code blocks with "Copy" button.

### 3. AI System Architecture

- **Tools Required**: `codesearch`, `grep`, `webfetch`.

### 4. Evaluation

- **Benchmark**: Test with 50 common developer questions.
- **Pass Rate**: 90% must match expected citations.
