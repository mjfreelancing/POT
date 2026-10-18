---
name: prd-discovery
description: "Turn a feature idea into a reviewed PRD by discovery: claim the next PRD number, write a discovery document with a draft problem statement, clarify unknowns one at a time, gather functional requirements in numbered rounds, then produce the PRD with a manual test plan. Use when a new PRD is needed or an idea needs requirements gathering. Never stages or commits."
argument-hint: "[feature idea, problem statement, or path to notes]"
---

# PRD Discovery

Discovery first, documentation second. The output is a PRD that was argued over in writing, not a document generated from a single prompt.

Two files are involved:

- **Discovery document** — `Docs/Future/PRD-NNN-<slug>-discovery.md`. Holds the captured input, the draft problem statement, the scope sketch, every question round and any open questions. It is scratch: delete it once the PRD is written.
- **PRD** — `Docs/Future/PRD-NNN-<slug>.md`. The durable document that `prd-delivery` consumes.

The number comes from `Docs/Future/README.md`, never from guessing: read the `Next free PRD number` line and bump it when the number is claimed.

## Project conventions this skill assumes

The procedure below hard-codes the layout this repository uses. When this skill is copied into another
project, adjust these four things and nothing else:

- PRDs live in `Docs/Future/` — change the paths in Stages 1, 5 and 6.
- The next number comes from a `Next free PRD number:` line in `Docs/Future/README.md` — change the
  number source if the project keeps one elsewhere, or ask the human outright when it keeps none.
- The index is a Markdown table in that same README — change the row format if the project's index differs.
- Delivery plans live beside the PRD as `TASKS-PRD-NNN.md` — keep that convention, because the
  `prd-delivery` skill looks for it.

## Hard rules

- **Never stage or commit.** Make working-tree edits only. Offer a commit message instead; the human stages and commits.
- **Ask, do not assume.** Where a requirement is not stated, ask. A recommended answer is a proposal, not a decision, and the human may always supply their own.
- **Clarify one question at a time.** During clarification, ask a single question per `ask_user` call and wait. This is not a requirement round.
- **No cap on requirement questions.** Cover the whole feature in ROUND 1 so the human reviews one complete round rather than a drip feed.
- **Number questions continuously across rounds** (1, 2, 3, …). A ROUND 2 follow-up continues the sequence so any item can be referenced later ("answer 27").
- **`unknown` is a valid answer.** Record it under Open Questions in the discovery document and carry on. When an answer is `unknown` because the human does not understand the question, explain it plainly and offer a rephrasing instead of recording a meaningless unknown.
- **Do not invent requirements.** Anything the human has not answered belongs in Open Questions, not in the requirements list.
- **The discovery document is scratch.** Delete it once the PRD exists and has been approved.
- **Offer a commit message at the end of every stage.** The index row and the PRD are the durable artefacts; whether a commit includes the scratch discovery document is the human's call.

## Procedure

### Stage 1 — Claim the number and open the discovery document

1. Read `Docs/Future/README.md` and take the number from the `Next free PRD number` line.
2. Derive a short kebab-case slug from the feature, matching the existing file names.
3. Create `Docs/Future/PRD-NNN-<slug>-discovery.md` from the [discovery document template](./references/discovery-document.template.md), filling in:
   - the input exactly as the human supplied it, or the notes from the interview;
   - a draft problem statement — what is wrong today and why it matters — written for the human to correct;
   - a scope sketch listing what appears to be in scope and out of scope.
4. Add a `Docs/Future/README.md` row with status `Planning`, the current date, a link to the discovery document, and a one-line summary. Bump `Next free PRD number` and the index-update date on the last line of that file.
5. Summarize what you understood in a short paragraph and **stop**. Offer a commit message for the index change.

### Stage 2 — Clarify unknowns (chat, one at a time)

Ask only about things that lack clarity and that would change what you ask later — not feature decisions, which belong in the rounds.

1. Ask one question per `ask_user` call, with a single field.
2. On `unknown`, append it to Open Questions in the discovery document.
3. Stop when nothing is left that would change the requirement questions, and say so.

### Stage 3 — ROUND 1: functional requirements

Append `## ROUND 1 — Functional requirements` to the discovery document.

- Place each question under a short heading, numbered continuously.
- Offer options `(a)`, `(b)`, `(c)` with an `Other:` escape, and mark a recommended option where you have one, stating briefly why.
- Give every question an `Answer:` line for inline responses.
- Cover the feature from every angle that changes behaviour: who may act and under what authority; what data is created, changed or destroyed; lifecycle and reversibility; what happens on failure; notifications and their recipients; every client surface involved; concurrency; retention and audit; and what is deliberately excluded.
- Say plainly at the top of the round that the human may answer `unknown`, `n/a`, or `your call`.

Then **ask the human to review the discovery document and answer inline**, and stop. Do not answer on their behalf.

### Stage 4 — Review the answers and run follow-up rounds

Read every answer and check for:

- contradictions between answers;
- answers that contradict the draft problem statement or scope sketch;
- answers that imply a scenario nobody has decided yet;
- answers that over-specify (for example a question expecting dozens of checks where a behaviour statement would do);
- `unknown` answers, which may need rephrasing rather than recording.

Append `## ROUND N — Clarifications` with follow-ups, continuing the numbering, and each with its own `Answer:` line. Then ask again and stop.

Repeat until nothing material is unanswered.

### Stage 5 — Produce the PRD

When the rounds are done, **ask the human whether to produce the PRD now or wait**, because some open questions become answerable as the work progresses. Then:

1. Create `Docs/Future/PRD-NNN-<slug>.md` from the [PRD template](./references/prd.template.md), which defines the whole shape: header metadata, problem statement, actors and authority, terms, grouped functional requirements, out of scope, known risks, assumptions, decisions, open questions and the manual test plan. Keep every section the template defines; delete a section only when the template says it may be omitted.
2. Rewrite the `Docs/Future/README.md` row to link to the PRD, keep status `Planning`, refresh the date and the one-line summary.
3. Complete the **Manual Test Plan** section of that same file: a setup section, scenario groups with referenceable IDs (`A1`, `B2`, …) and expected results, a cleanup section, and a results table. It must be executable by a person against the built product and must not duplicate the automated suites.
4. Delete the discovery document when the PRD is approved.
5. Report what was produced and offer a commit message.

### Stage 6 — Refine, then hand off

1. Iterate on the PRD as the human reviews it. Keep every requirement numbered and stable so discussion can reference them.
2. When the human is satisfied, tell them the PRD is `Planning` and that `prd-delivery` needs it at `Planned`; set it to `Planned` only when they say so.
3. Point them at the next step: ask for a TASKS plan to start delivery with the `prd-delivery` skill.

## Notes

- The PRD records decisions; the discovery document records how they were reached. Once the PRD exists, the second is noise.
- Never reference the PRD number, requirement IDs or this process in code, tests, comments or test names. Those identifiers live only in `Docs/Future/`.
- The Manual Test Plan is executed at the end of delivery — `prd-delivery` pauses for it before completing a PRD.
