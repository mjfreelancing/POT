---
name: adr
description: "Record a technical decision as an Architecture Decision Record: take the next number, write the front matter and body from the template, cite the evidence for a decision already in the code, update the instruction files and rule documents that should point at it, and run docs-check. Use when a change makes a decision that outlives its feature, or when asked to document why something is the way it is. Never stages or commits."
argument-hint: "[the decision, or the change that made it]"
---

# Architecture Decision Record

Write one ADR for one decision. The record is short, dated and citable; the reasoning that made it is
what a reader six months from now needs, and the code alone will not carry it.

Read [Docs/adr/README.md](../../../Docs/adr/README.md) first: it carries the significance test, the
format, the statuses and the amendment rules. This skill is the procedure that applies them.

## Hard rules

- **One decision per file.** If the draft covers several, write several ADRs.
- **Related decisions may form a family.** Group them with links from an overview or index, but do not
  combine independent decisions in an umbrella ADR.
- **Never rewrite an accepted decision in place.** Supersede it: set the old record to `superseded`,
  point `superseded-by` at the new one, and give the new one `supersedes`.
- **Never reuse a number.** A withdrawn draft still consumes its number.
- **A new record carries no heading suffix and does not call itself backfilled.** Only records up to
  ADR-0052 carry `(legacy)` or `(backfilled)` after the number in their heading, and ADR-0046 does not;
  the ADR guide says why. When the decision is already in
  the code, Context cites the evidence for it: a PRD section, a dated note, a merged PR, or a source
  file.
- **Confirmation must name something mechanical.** A test, an analyzer, an instruction file or a review
  rule. A record with no enforcement is prose, and prose drifts.
- **Never stage or commit.** Offer a commit message and stop; the human stages and commits.
- **Never name a PRD, ADR, task list or requirement ID in code, tests, comments or test names.** An ADR
  may of course cite them; code may not.

## Procedure

### 1. Decide whether it is an ADR at all

Apply the "When to write one" test, including the historical question: before or during implementation,
what consequential alternatives were considered, and would the rationale or trade-offs be lost if only
the current code remained? A stable, long-standing decision can still merit an ADR. Use source material as
evidence; do not present an inference as established history. If the item is a coding convention, it
belongs in [Docs/standards/](../../../Docs/standards/) as a rule document. If it is a product rule, it
belongs in the PRD. If it was an approach tried and dropped without shipping, it belongs in the
`Considered Options` of the ADR that replaced it.

Stop and say so when it is not an ADR, naming where it belongs instead.

### 2. Take the next number

List `Docs/adr/`, read the highest four-digit prefix, and add one. Do not take the next number from the
backlog in `DOCS-REORGANISATION-PLAN.md`: the folder is the authority.

### 3. Write the file

Copy [references/adr.template.md](./references/adr.template.md) to
`Docs/adr/NNNN-kebab-case-title.md` and fill it in:

1. **Front matter** — the five required fields, plus `related-prds` when the decision came from a PRD.
   `date` is when the decision was made, not today.
2. **Summary** — one sentence in the form "In the context of …, facing …, we decided …, to achieve …,
   accepting …".
3. **Context and Problem Statement** — the forces, not the history of the discussion.
4. **Considered Options** — at least two, including the status quo when the decision is already in the code.
5. **Decision Outcome** with **Consequences** (good and bad) and **Confirmation**.
6. **Length** — 40 to 120 lines, ceiling about 150. A longer draft is several decisions, or a design
   document that belongs in reference or explanation.

### 4. Point the rest of the documentation at it

- An instruction file that states the rule gets the ADR number appended in parentheses, for example
  `… a foreign id is a 404 (ADR-0030).` Rules stay imperative; the rationale is not copied in.
- A rule document under `Docs/standards/` that restates the decision links to the ADR instead.
- A guide or reference page that explains the decision links to it rather than repeating it.

### 5. Check at both ends, then stop

`docs-check` runs twice, and the second run is what makes the record trustworthy.

**When the file is written.** The front matter drives the generated index, so regenerate it and validate:

```powershell
pwsh ./.github/scripts/docs-check/docs-check.ps1 -Update
pwsh ./.github/scripts/docs-check/docs-check.ps1
```

**Before the record is treated as final.** Run the same two commands again when you hand the ADR over for
approval, and again when `prd-delivery` moves it from `proposed` to `accepted`. The status change alters
the index, and an instruction file may only cite an `accepted` record, so a citation that was rejected on
the first run becomes valid on the second — and one left behind by a renumbering or a rename surfaces
here.

Both runs must pass. Report the file written, the ADRs or rule documents you pointed at it, the result of
each run, and offer a commit message. Never stage or commit.

## When the decision came from a PRD

- The ADR stays `proposed` while the PRD is `Planning` or `In Progress`, and becomes `accepted` when the
  PRD is `Complete`. `prd-delivery` performs that flip at close-out.
- Name the ADR in the PRD's "Decisions Carried Into This Document" table.
- If the ADR replaces a decision an earlier PRD recorded, the PRD that causes the change lists it under
  "Effect on Earlier Documents", and the earlier PRD receives an `AM-n` entry.
