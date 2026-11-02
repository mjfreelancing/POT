# Documentation Reorganisation Plan

**Status:** Agreed. Phase 0b is complete; Phase 1 starts on the owner's OK.
**Baseline:** commit `9024e1d2`, the tip of `v0.1-doc-update-plan` when Phase 0b ran. Every line number, count, quote and `path:line` reference in this plan and in `DECISIONS-TO-DISCUSS.md` was taken at `ceb57419`; Phase 0b re-checked every reference into a file that changed since, so all of them hold at this commit too.
**What this file holds:** only the work still to do and the rules it follows. Earlier drafts, the review that produced them, and the options weighed for each settled decision are in git history.

Agents never stage or commit. Each phase ends with a stop for the owner's review and an offered commit message.

## Before you start: re-baseline (Phase 0b)

> **For the agent executing this plan:** the source documents will change again before the plan is applied. Your first job is Phase 0b, not Phase 1.

- **Treat every reference here as a pointer** for finding the content, not as a guaranteed address.
- **Do Phase 0b immediately before Phase 1,** on the branch the plan will change:
  1. List what changed since the baseline:
     - `git diff --stat ceb57419 <target> -- '*.md' .github Docs`;
     - plus any source file cited as evidence in section 8, Appendix E or `DECISIONS-TO-DISCUSS.md`.
  2. Read every changed or new document. Look for:
     - new decisions (backlog candidates, section 8);
     - new amendment notes, or PRDs newly overtaken by a later change (Appendices C and D);
     - new or changed rules (D13, Appendix E);
     - items in section 10 that have since been fixed;
     - code changes that alter the evidence or recommendation in a brief in `DECISIONS-TO-DISCUSS.md`.
  3. Check `Docs/Future/README.md` (or `Docs/prd/README.md`) for PRDs or ADRs added since the baseline, and add them to the inventory.
  4. Re-derive line references before using them.
  5. **Run the availability check** in [Files reported as missing or unavailable](#files-reported-as-missing-or-unavailable) (below). Do it last, after the other steps, so it covers anything they add.
  6. **Update this plan and `DECISIONS-TO-DISCUSS.md` in place so they still list only outstanding work:**
     - remove what is now done;
     - add what is new;
     - move the baseline commit forward.

     Then stop for the owner's review before starting Phase 1.

### Phase 0b record (completed 2026-11-02 at `9024e1d2`)

Phase 0b ran on a full checkout, on `v0.1-doc-update-plan`. Repeat the procedure above if the plan waits again before Phase 1 starts.

**What moved since `ceb57419`:** 12 commits, 13 files.

| Change | Effect on this plan |
| --- | --- |
| PRD-031 moved In Progress → Complete (2026-11-02), in `Docs/Future/README.md` and its own header | Section 8 row 0083 is now accepted, and the PRD-031 rows in Appendix D are due now rather than at closure |
| PRD-031 gained `## Requirement Coverage` and `## Verification Evidence` | The PRD template change in section 12 must carry both sections, or the delivery skill writes sections the template lacks |
| `Docs/Future/TASKS-PRD-031.md` deleted | The target `prd/` layout and the 6.3 mapping treat a task list as a resident file; the delivery skill now keeps the stage plan out and deletes the file at close (6.1, 6.3) |
| `.github/skills/prd-delivery/SKILL.md`: close-out now harvests deliberately-broken checks, measured baselines and the limits of the evidence into the PRD, then deletes the task list; the close-out steps renumbered 1–5 | Section 12 records that the evidence half of the close-out already exists, so only the ADR flip and the amendments are left to add |
| `Source/Server/DEVELOPER.md`: two CORS samples now use `HeaderNames` constants | No section 10 item covers those lines, and the file's line count is unchanged, so every reference into it still holds |
| `Pot.AspNetCore/Concerns/Cors/Configuration/CorsOptionsSetup.cs` and its test, `Invariants/IsolationFixture.cs`, `e2e/scripts/e2e-logged-run.ps1`, `playwright.prod.config.ts` | Reviewed: none is cited as evidence in section 8, Appendix E or a brief, and none fixes a section 10 item |

**No section 10 item has been fixed since the baseline.** The only file section 10 names that changed is the server guide, and the change touches none of the listed lines. The four "code that does not exist" claims in section 10 were re-checked and all still hold at this commit: no `@/hooks/useToast` import, no `ValidationError` type in the client, "Site Owner" only in the stale `_DOCUMENTATION_PLAN.md`, and `import-export:manage` only at `AUTHENTICATION.md:1919`.

**Decisions weighed and not raised as backlog rows.** A narrowed E2E run now keeps its script's config (`e2e-logged-run.ps1`). The prodlike config now sets `timeout: 60_000`, which removes a drift against ADR-012 §13 rather than creating one, and a 600s client start budget that only the config comment records. Both are tooling and measured-budget facts for the E2E reference, which section 9 already places outside the ADR series; the start budget is content for that reference when Phase 5 splits the E2E docs.

**Availability check** (run last, as the procedure asks). Five items differ from what the planning machine reported:

| Item | Found here | Follow-up |
| --- | --- | --- |
| `Docs/Azure/Azure-Deployment-Notes.md` | Present, but untracked and ignored (`.gitignore:408`) | **Owner decision.** Track it, track a redacted template, or remove the three links. Until then `how-to/deploy-to-azure.md` replaces it rather than renames it |
| `Source/Docker/.env.development.template` | Absent: not on disk, never tracked | **Owner decision.** Add a tracked template (row 0089) or correct "copy the template" to "create from scratch" (§10.12, Appendix E.3) |
| `Source/Docker/.env` | **Tracked**, not ignored; holds `COMPOSE_PROJECT_NAME` and `POSTGRES_DB` only, no secrets | Row 0089's `Ver` narrowed; Appendix E.3 corrected |
| `.github/workflows/` | No such directory | DD-05's evidence is "there is no CI", not "empty and untracked"; `docs-check` stays a local task |
| `.github/instructions/playwright-e2e.instructions.md` | Still absent; the live file is `e2e.instructions.md` | **Owner decision at Phase 2.** Create the file, or repoint `e2e/README.md:10`, `e2e/AUTHORING.md:5` and `ADR-012:20` |

Confirmed as reported, so no claim changes: `appsettings.Development.json`, `Data/README.md`, `Data/Scripts`, `Data/**/postgres-data` and both client `.env` files are present and ignored; `Data/Exports` is empty; there is still no infrastructure-as-code for Azure.

### Files reported as missing or unavailable

> **For the agent executing this plan:** this plan and `DECISIONS-TO-DISCUSS.md` were written on a machine that **did not have the full repository**. Everything below was reported as missing, absent, empty or "not in the repo". Some of it may exist on the executing machine but be git-ignored, often deliberately, because it holds secrets, connection strings or machine-specific data. Before Phase 1, check each item on the machine that will do the work, and record the result in the re-baseline addendum.

**How to check:**

- `Test-Path '<path>'` shows whether the item is on disk.
- `git check-ignore -v '<path>'` shows whether it is ignored, and by which `.gitignore` rule.
- `git ls-files --others --ignored --exclude-standard` lists every ignored file that is present. Skim it for anything else the plan should know about.

**Rules for anything you find:**

- **Never copy the contents of an ignored file into a tracked document, ADR or instruction file.** That includes values, secrets, connection strings, host names and passwords. Record only that the file exists, what it is for, and its shape (for example, which keys it defines) where that is safe.
- **If a file looks ignored for security reasons, ask the owner** before citing it at all, even by path.
- **A link to a file that exists only locally is still broken** for every other reader. Do not mark such a link as fixed. Ask the owner whether to track the file, track a redacted template, or remove the link.
- **If an item exists, re-check every claim that depends on it.** Update the plan, the affected backlog row or brief, and section 10.

| Item | Reported as | Claims that depend on it |
| --- | --- | --- |
| `Docs/Azure/Azure-Deployment-Notes.md` | Linked three times, does not exist. **Phase 0b:** present, untracked and ignored | §10.12; §6.1 (`how-to/deploy-to-azure.md` "replaces" it); §6.3 Docker guide row; Appendix E.3 (Ports, Production Deployment, Additional Resources) |
| `.github/instructions/playwright-e2e.instructions.md` | Linked from `e2e/README.md:10`, `e2e/AUTHORING.md:5` and `ADR-012:20`, does not exist. **Phase 0b:** still absent | §3.1 (ADR-012 broken link); §10.11; Phase 2 ("fix the broken instruction-file link") |
| Infrastructure-as-code for Azure (Container App replica count, ingress, health probes, PostgreSQL server version) | Not in the repo. **Phase 0b:** still none | DD-04 (replica count), DD-06 (Azure runs 17.6, from a diagram label only), DD-16 option C (probes), the forwarded-headers follow-up at the end of `DECISIONS-TO-DISCUSS.md` (the ingress sets `X-Forwarded-For`); rows 0051, 0054, 0084 |
| `.github/workflows/` | Exists but empty and untracked. **Phase 0b:** no such directory | DD-05; row 0090; Appendix E.1 Migrations row ("there is no CI") |
| `Source/Client/pot-react/.env.development`, `.env.production` | Git-ignored (`.gitignore:405–406`), absent from a clean checkout | Appendix E.2 Environment Configuration; Appendix E.3 Images and Dockerfiles (the `.env.production` fallback); row 0073/0085 |
| `Source/Docker/.env`, `Source/Docker/.env.development` | Git-ignored; created from `.env.development.template`. **Phase 0b:** `.env` is tracked (two non-secret keys), `.env.development` is ignored, and no template exists | Row 0089 (`Ver`); §10.12 ("create from scratch" vs "copy the template"); Appendix E.3 Environment Variables |
| `Source/Server/Pot.AspNetCore/appsettings.Development.json` | Not found on the planning machine, but `Data/Local Docker/README.md:5` says it points the API at the prodlike stack on `5444` | Row 0088 ("the prodlike stack is also the dev-mode API's database") |
| `Data/README.md` | Git-ignored machine-local index of this machine's data folders | Row 0088 (local environments) |
| `Data/Scripts/`, `Data/**/Backups/` | Git-ignored (backup tooling; the Azure target needs a password) | Row 0089 (backups are kept outside the repository) |
| `Data/Exports` | Reported as empty by commit `a7cbded7` | The restore instructions in the `Data/` and E2E READMEs |
| `Source/Docker/postgres-data`, `Data/*/postgres-data` | Local PostgreSQL 13 clusters (bind mounts) | DD-06 option B (the manual dump and restore); agents never touch these folders |

The items in section 10 that describe code that does not exist (the `@/hooks/useToast` import, the `ValidationError (422)` type, the "Site Owner" role, the `import-export:manage` permission) are claims about tracked source. Re-check them as part of step 2, not this list.

---

## Contents

1. [Summary](#1-summary)
2. [Agreed decisions](#2-agreed-decisions)
3. [Starting state](#3-starting-state)
4. [PRD amendments](#4-prd-amendments)
5. [The new ADR series](#5-the-new-adr-series)
6. [Target documentation structure](#6-target-documentation-structure)
7. [What happens to ARCHITECTURE.md](#7-what-happens-to-architecturemd)
8. [ADR backlog](#8-adr-backlog)
9. [Things that are not ADRs](#9-things-that-are-not-adrs)
10. [Content to correct during the migration](#10-content-to-correct-during-the-migration)
11. [Code risks: decided in `DECISIONS-TO-DISCUSS.md`](#11-code-risks-decided-in-decisions-to-discussmd)
12. [Changes to skills, templates and agent instructions](#12-changes-to-skills-templates-and-agent-instructions)
13. [Migration phases](#13-migration-phases)
14. [Governance: keeping it from sprawling again](#14-governance-keeping-it-from-sprawling-again)
- [Appendix A — ADR template](#appendix-a--adr-template)
- [Appendix B — PRD amendment format](#appendix-b--prd-amendment-format)
- [Appendix C — Existing amendment notes to retrofit](#appendix-c--existing-amendment-notes-to-retrofit)
- [Appendix D — PRDs affected by later decisions that carry no note](#appendix-d--prds-affected-by-later-decisions-that-carry-no-note)
- [Appendix E — Developer guide classification](#appendix-e--developer-guide-classification)

---

## 1. Summary

1. **PRD amendments.** Every PRD overtaken by a later change gets:
   - an append-only `## Amendments` section at the end;
   - a one-line `**Amended:**` header field.

   The PRD that *causes* a change lists its impacts in an `Effect on Earlier Documents` table, so the link runs both ways. PRD body text is never edited. See [section 4](#4-prd-amendments).
2. **A new ADR series:**
   - MADR-style ADRs with YAML front matter, in `Docs/adr/`, numbered `ADR-0001` upwards, one file each.
   - The frontend/backend split is an `area` field, and the generated index groups by it.
3. **The two legacy ADRs are re-homed.**
   - ADR 012 becomes four focused ADRs, and its reference material moves to `e2e/README.md`.
   - ADR 014 becomes one Proposed ADR.
   - Tombstone stubs stay at the old paths, so links in frozen PRDs keep working.
4. **About 80 ADRs** are written from the backlog in [section 8](#8-adr-backlog), in themed batches. Rows marked `Dec` wait for their brief in `DECISIONS-TO-DISCUSS.md` to be decided. Nothing else waits.
5. **Coding conventions become rule documents** in `Docs/standards/`, owned by three standards ADRs ([D13](#d13--rule-documents-for-conventions)).
6. **One home per document type.**
   - Developer docs follow the Diátaxis types (tutorials, how-to, reference, explanation), with `adr/`, `prd/` and `user-guide/` as their own trees.
   - The long `DEVELOPER.md` guides split into decisions (ADRs), rules (rule documents) and examples (which stay beside the code), per [Appendix E](#appendix-e--developer-guide-classification).
   - Definitions repeated in seven places get one glossary.
7. **`ARCHITECTURE.md` is slimmed, not deleted,** into a 150–250 line map of the system.
8. **Stale content is corrected** as each file is reworked ([section 10](#10-content-to-correct-during-the-migration)), not in a big bang.
9. **Sequencing:**
   1. Conventions and tooling first.
   2. Re-home the two legacy ADRs.
   3. Write ADRs in themed batches.
   4. Retrofit the PRD amendments.
   5. Move and split files last, so each file is touched about once.

   A local PowerShell `docs-check` script catches broken links and stale citations, because there is no CI.

---

## 2. Agreed decisions

These are settled with the owner and binding on the work. The options weighed for each are in git history.

### D1 — ADR numbering and areas

- One number sequence, `ADR-0001` upwards. The 4-digit IDs keep the series distinct from the legacy "ADR 012/014" names that frozen PRDs still mention.
- One ADR per file in `Docs/adr/`. There are no area folders.
- An `area` front-matter field:
  - `frontend` covers `Source/Client`;
  - `backend` covers `Source/Server`;
  - `cross-cutting`, `infrastructure` and `process` cover the rest.

  A decision both sides must honour, such as the API error contract, is one `cross-cutting` ADR. It is never split into a frontend file and a backend file.
- The generated index groups ADRs by area.

### D2 — Location and folder naming

- ADRs live in `Docs/adr/`, not `Docs/Future/`.
- New folders are lowercase kebab-case (`adr/`, `how-to/`, `user-guide/`).
- The existing `Future`, `USER-GUIDE` and `Azure` folders are renamed in Phase 5. A case-only rename (`USER-GUIDE` → `user-guide`) needs a two-step git rename; the owner does the git side.

### D3 — Retrofitting the existing PRD notes

- Each note in [Appendix C](#appendix-c--existing-amendment-notes-to-retrofit) moves verbatim into an `AM-n` entry in the new Amendments section.
- A note that warned about a specific paragraph is replaced in place by a one-line pointer (`> Amended — see AM-2`).
- Top-of-file banners go, and the `**Amended:**` header line replaces them.
- **Exceptions:**
  - Sites 13 and 16–22 stay in place. They are a PRD amending itself during or straight after delivery, so they are part of its own record.
  - Site 25 (PRD-027, not delivered) is folded into PRD-027's rewrite.

### D4 — Missing amendments

Add an `AM-n` entry for every row of [Appendix D](#appendix-d--prds-affected-by-later-decisions-that-carry-no-note) in Phase 4, each citing its cause.

### D5 — `Docs/Future/` becomes `Docs/prd/`

- Done in Phase 5, at the same depth, so the `../../Source/...` links in frozen PRDs keep working.
- The two PRD skills, the stage-plan template and `copilot-instructions.md` hard-code `Docs/Future`. They change in the same commit.

### D6 — Developer guides

Every guide section mixes three layers, and each layer gets its own home:

| Layer | Goes to |
| --- | --- |
| **Decision:** why we do it | An ADR (most are already backlog rows) |
| **Rule:** the current binding convention | A rule document ([D13](#d13--rule-documents-for-conventions)). The instruction files keep a condensed copy that cites it. |
| **Example:** worked code, recipes, catalogues, commands | `Source/<component>/docs/`, each file ≤ ~800 lines, linking to the ADR and the rule |

- Each `DEVELOPER.md` becomes an index of at most 150 lines.
- Anything a tool enforces (`.editorconfig`, ESLint, Prettier, `tsconfig`) treats the config file as the source of truth; prose only points at it.
- `Docs/` holds product-wide and cross-cutting documentation. `Source/<component>/` holds internal developer reference for that component.
- [Appendix E](#appendix-e--developer-guide-classification) applies this to every section of the three guides.

### D7 — `ARCHITECTURE.md`

It stays at its path, slimmed to a 150–250 line overview ([section 7](#7-what-happens-to-architecturemd)). Its decisions go to ADRs, and its domain explanation goes to `explanation/cashflow-model.md`.

### D8 — Superseded ADRs, and how-to guides for ADRs and PRDs

- **Superseded decisions get their own ADR only if they shipped** and left traces a reader will meet. There are three: 0009 (persisted accrual with a dirty flag), 0024 (refusals answer 422) and 0086 (cache-first Docker builds).
- Approaches tried and dropped during development go into the "Considered Options" of the ADR that replaced them.
- **One "How this works" README per document type.** `Docs/adr/README.md` and `Docs/prd/README.md` each start with a hand-written section covering:
  - when to write one;
  - how to create it;
  - the format;
  - the statuses and lifecycle;
  - amendments and supersession.

  The generated index sits below that section. ADR-0001 and ADR-0003 record *why* the rules exist.
- **One canonical template per type:** `Docs/adr/template.md` and `Docs/prd/template.md`. The PRD template moves out of `.github/skills/prd-discovery/references/`, and the skills link to the new path and follow the READMEs.

### D9 — ADR status

- An ADR raised by a PRD stays `proposed`, and freely editable, while the PRD is Planning or In Progress.
- It becomes `accepted` when the PRD is marked Complete, and `rejected` if the PRD is abandoned.
- A standalone ADR is `accepted` when committed.
- **ADR status never tracks implementation progress.** The PRD status does that.

### D10 — Items that need a decision

- Each `Dec` item in the backlog, and each code risk, has a brief in `DECISIONS-TO-DISCUSS.md` and is decided on its own.
- **While a brief is undecided,** its ADR is not written and its backlog row stays `Dec`. Nothing else waits for it.
- **Once decided:**
  - **The decision keeps the code as it is:** the ADR is `accepted` and records the owner's reasoning.
  - **The decision changes code:** a PRD is raised through `prd-discovery`, and the ADR stays `proposed` until that PRD is Complete (D9). The owner generates any EF migration; agents never do.

### D11 — `docs-check`

- **The script:** PowerShell 7, at `.github/scripts/docs-check/docs-check.ps1`, with a `README.md`, laid out like `agent-env-tools/`. It:
  - validates ADR front matter;
  - regenerates `Docs/adr/README.md`;
  - fails on broken relative links;
  - checks that instruction files cite only accepted ADRs;
  - greps `Source/**` for `PRD-`/`ADR-` references, which are banned there.
- **Front matter is limited to flat `key: value` pairs and simple lists.** The script parses those itself, with no YAML module, and fails on anything else.
- **Tests:** Pester 5, against a fixtures folder of good and bad ADRs and links. The E2E scripts already use Pester 5 (`npm run e2e:scripts`).
- **Running it:** a VS Code task, and optionally a pre-commit hook.

### D12 — `AGENTS.md`

A short root file that points at `.github/copilot-instructions.md`, the instruction files, `Docs/README.md`, `Docs/adr/README.md` and the rule documents. It restates none of their rules. It is created in Phase 1, together with the files it points to.

### D13 — Rule documents for conventions

- **Single-decision ADRs stay as agreed:** one file each, unchangeable once accepted.
- **Three standards ADRs,** one each for server code, client code and the API contract, record the decision to keep a rule set. Each owns its rule documents in `Docs/standards/server/`, `client/` or `api/`.
- **Every rule carries an enforcement tag** (`[error]`, `[warning]`, `[test]` or `[review]`). `[test]` covers rules pinned by fixture tests (0058).
- **Every rule carries a provenance marker:** "platform convention", "project decision" or "diverges from the platform convention". The platform is .NET on the server, and React, TanStack and shadcn on the client.
- **Precedence:** `.editorconfig` and the other tool configs win for anything a tool can check; the rule document wins for everything else.
- **Brownfield rule:** existing working behaviour outranks a style rule.
- **Changing a rule:** rules change by edit, with the reason stated. A change that reverses a significant decision also needs its own ADR. Instruction files carry the condensed rule and cite its rule document.
- **Size:** a rule document stays within about 800 lines, and starts with a quick-reference list of its rules and their tags.
- **Effect on the backlog:** rows that are mostly convention (0042, 0046, 0048, 0060, 0076) shrink to the decision behind them, or fold into a standards ADR. The backlog is renumbered into a clean sequence in Phase 1.

### Backlog folding and Appendix E

- **Ten small backlog rows fold into a neighbour,** listed in [section 8](#8-adr-backlog). Their numbers are not used.
- **Appendix E is accepted as written:** its classifications, and its delete list (E.4.4), which is applied when each guide is split in Phase 5.

---

## 3. Starting state

### 3.1 The document estate

| Family | Files | Lines | What needs attention |
| --- | --- | --- | --- |
| PRDs (`Docs/Future/PRD-*.md`) | 32 | ~8,800 | PRD-001 to 011 are planning documents in a pre-template shape. Four header statuses disagree with the index (007, 009, 012, 027). PRD-032 is still a discovery document, linked from the index as if it were the PRD. |
| Legacy ADRs | 2 | 757 | Numbered after their PRD. They mix decision, current-state description, config tables, how-to and design history. ADR-012 has a broken link to a non-existent instruction file. |
| `Docs/*.md` (architecture, auth, setup, features) | 7 | ~3,660 | Heavy duplication (3.2). `AUTHENTICATION.md` (2,217 lines) is partly current and partly describes the pre-AuthSession design. |
| User guide | 13 | ~1,560 | Accounts, Dashboard, Projections and Security are real. Seven pages are stubs. A 745-line `_DOCUMENTATION_PLAN.md` is a stale work plan. |
| Developer guides | 6 | ~10,600 | Server `DEVELOPER.md` 5,347; client `DEVELOPER.md` 3,228; `IMPLEMENTATION.md` 825 (with encoding corruption); Docker `DEVELOPER.md` 847; `Data/Local Docker/README.md` 80; `Data/Seed Docker/README.md` 81. |
| Agent guidance (`.github/`) | 8 instructions + 13 skills | ~1,050 | Current and concise. It carries many decisions as rules, with no "why". |

### 3.2 Duplication to consolidate

| Topic | Number of places | Where |
| --- | --- | --- |
| Docker env-file setup | 7 | GETTING-STARTED, DOCKER-SETUP, Docker DEVELOPER, ARCHITECTURE, FIRST-TIME-SETUP, AUTHENTICATION (twice), LOCAL-SETUP |
| Ports | 10 | ARCHITECTURE, DOCKER-SETUP, Docker DEVELOPER (three places), copilot-instructions, docker.instructions, the two `Data/*/README.md` files, LOCAL-SETUP, e2e.instructions, e2e/README |
| Accrued / Arrears / Committed / Available definitions | 7 | ARCHITECTURE, server DEVELOPER, IMPLEMENTATION, FEATURES, three user-guide pages |
| Tenancy model | 4 in server DEVELOPER alone | lines 157/183, 237, 1878, 4678 |
| CORS configuration | 2 full copies | server DEVELOPER 3450–3538 and 4989–5062 |
| Platform-admin bootstrap | 5 | AUTHENTICATION, FIRST-TIME-SETUP, GETTING-STARTED, LOCAL-SETUP, Docker DEVELOPER |
| Health-check verification blocks | 2 verbatim copies | DOCKER-SETUP 54–134 = 171–251 |
| Income vs expense renewal explanation | ~90% copy | IMPLEMENTATION 189–399 vs 402–604 |

### 3.3 Decisions with no PRD

Two decisions were made with no PRD, so their ADRs (0025 and 0038) are the first thing able to cite them:

- the switch from 422 to 400, in commit `bec69a48` (2026-10-16);
- binding access tokens to their session, in commit `31c96313` (2026-10-17).

ADR-012:362 wrongly attributes the 422 change to PRD-030 (Appendix C, site 9).

---

## 4. PRD amendments

### 4.1 The format

**In every PRD that has been overtaken:**

1. A header line under the existing fields: `**Amended:** 2026-10-26 — 2 amendments, see [Amendments](#amendments)`. `Last Updated` keeps meaning "last approved".
2. A final `## Amendments` section, **append-only, oldest first**. Each entry is an `### AM-n — <title> (<date>)` subsection, not a table row: these notes run to several sentences, and prose in table cells is unreadable in raw Markdown. Each entry has:
   - **Type:**
     - `Superseded`: a rule was replaced.
     - `Obsoleted`: the thing described no longer exists.
     - `Corrected`: the document was wrong when written.
     - `Clarified`: no behaviour change.
   - **Affects:** requirement IDs (A1, B3) for template PRDs, and section anchors for older PRDs. This is what makes inline markers in the body unnecessary.
   - **Source:** the PRD, ADR or commit that made the change.
3. The original text is **never edited**. A pointer line (`> Amended — see AM-2`) goes in the body only where a reader would otherwise act on a paragraph that is now wrong.

**In the PRD that causes a change:**

4. A new template section, `## Effect on Earlier Documents`, is filled in during discovery. It is a table: Document | What changes | Amendment. The causing PRD therefore lists its impacts *before* delivery, and `prd-delivery` writes the matching `AM-n` entries when the PRD closes. That gives a link in each direction.

**Elsewhere:**

5. **Architectural supersessions** are also recorded as an ADR with `supersedes`/`amends`. The PRD amendment then cites the ADR. Product-rule changes (for example "a platform admin may now be removed while another exists") need only the amendment.
6. **The index** (`Docs/prd/README.md`) gains a narrow `Amended` column holding the latest amendment date. Summary cells go back to one line describing what the feature *is*.
7. **Scope:** amendments apply to **Complete** (or abandoned) PRDs. A PRD that has not been delivered absorbs changes directly when it is next revised. PRD-027 ("Blocked, needs rewriting") is the example: its existing note is folded in during its rewrite.

The format is shown in [Appendix B](#appendix-b--prd-amendment-format).

### 4.2 Retrofitting the existing PRDs

- **No original PRD content changes.** The work is:
  - adding the Amendments section and the header line;
  - moving the existing notes into it ([D3](#d3--retrofitting-the-existing-prd-notes));
  - adding entries for the gaps ([D4](#d4--missing-amendments)).
- **Header disagreements** are recorded as `Corrected` amendments, not edited: PRD-007 "Planned", PRD-009 "Implemented", and PRD-028:55 "implementation has not started".
- **PRD-001 to 021 are not reshaped** into the new template. Their structure is historical record; they only gain the Amendments section where needed.

---

## 5. The new ADR series

### 5.1 Template

The template is **MADR 4, customised**: Nygard's format is too thin for backfilled records, and MADR's "Confirmation" section is the natural hook for tests and instruction files. The full skeleton is in [Appendix A](#appendix-a--adr-template).

**Required sections:**

| Section | Rule |
| --- | --- |
| Summary | One sentence: "In the context of…, facing…, we decided…, to achieve…, accepting…" |
| Context and Problem Statement | — |
| Considered Options | At least two, including the status quo for backfills |
| Decision Outcome | — |
| Consequences | Good and bad |
| Confirmation | The test, analyzer, instruction file or review rule that enforces the decision |

**Optional sections:** Decision Drivers, Pros and Cons of the Options, More Information.

**Front matter:**

| Field | Values |
| --- | --- |
| `id`, `title` | — |
| `status` | `proposed`, `accepted`, `rejected`, `deprecated` or `superseded` |
| `date` | When the decision was made |
| `recorded` | When the file was written |
| `provenance` | `contemporaneous` or `retroactive` |
| `evidence` | Required when retroactive |
| `area` | — |
| `tags` | — |
| `supersedes`, `superseded-by`, `amends`, `amended-by` | — |
| `related-prds`, `related-adrs` | — |

**Length:** target 40–120 lines, with a ceiling of about 150. A longer draft is several decisions (split it) or a design document (move the content to reference or explanation).

### 5.2 Lifecycle and immutability

- **Lifecycle:** `proposed` → `accepted` → `deprecated` | `superseded`, or `proposed` → `rejected`.
- **When it becomes `accepted`:** see [D9](#d9--adr-status).
- **Edits allowed once accepted:**
  - typo and link fixes;
  - the `status`, `superseded-by` and `amended-by` fields;
  - a dated clarification line that doesn't change the decision.
- **Amend or supersede:**
  - *Amend*, via a new ADR with `amends:`, when the decision mostly still holds and something narrower changes.
  - *Supersede* when the decision statement is no longer true.
  - Never rewrite an accepted decision in place.
- **Backfilled ADRs:**
  - `provenance: retroactive`.
  - `date` is the best evidence of when the decision was made: the PRD completion date, a dated note, or the commit date.
  - `recorded` is the day the file is written.
  - `evidence` links PRD sections and source files. Because PRDs are frozen, deep links into them stay stable.

### 5.3 Relationship to PRDs

| Document | Records |
| --- | --- |
| PRD | What and why for one feature: requirements, scope, actors, product rules, the test plan. Frozen at Complete. |
| ADR | One significant technical decision that outlives the feature, with the alternatives and consequences. |

**Significance test.** Raise an ADR if any of these is true:

- it constrains features beyond this PRD;
- it is costly to reverse;
- it crosses components or defines the client–server contract;
- someone will later ask "why is it like this?".

**What does not get an ADR:**

- Product rules stay in PRDs.
- Coding conventions are rules, not decisions. They live in `Docs/standards/` ([D13](#d13--rule-documents-for-conventions)). See also [section 9](#9-things-that-are-not-adrs).

**Linking:**

- **ADR → PRD:** the ADR's front matter lists `related-prds`.
- **PRD → ADR (new PRDs):** the PRD's "Decisions Carried Into This Document" table gains an `ADR` column.
- **PRD → ADR (existing PRDs, which are not edited):** the generated ADR index includes an "ADRs by PRD" view.

### 5.4 The two existing ADRs

| Legacy | Becomes | Non-decision content goes to |
| --- | --- | --- |
| `Docs/Future/ADR-012-playwright-e2e-testing-infrastructure.md` (633 lines) | ADR-0079 to 0082 (shared stack and seeded database; test isolation classes; fresh login per test; execution policy). The rejected designs in Appendix A become their Considered Options. | Config values, ports, seeded identities and directory tree go to `e2e/README.md` (with no copied config tables that drift). Operational notes in §13 go to a troubleshooting section there. How-to content goes to `e2e/AUTHORING.md`. |
| `Docs/Future/ADR-014-retry-after-client-cooldown-handling.md` (124 lines) | ADR-0072, `proposed`, decisions D-1 to D-5 | The open questions stay in PRD-014. "Where changes would land" goes nowhere durable (it belongs in the TASKS plan). |

At each old path a **tombstone stub** of about five lines says "moved to ADR-00xx…". PRD-012, PRD-014 and the index keep working links without editing frozen PRDs.

---

## 6. Target documentation structure

### 6.1 Layout

```text
README.md                          repo overview; links to Docs/README.md
Docs/
  README.md                        docs map + "where does X go?" table (the anti-sprawl contract)
  ARCHITECTURE.md                  slim overview, links to ADRs (section 7)
  adr/
    README.md                      "How this works" (D8) + generated index: by area, by status, by PRD
    template.md
    0001-record-architecture-decisions.md …
  prd/                             renamed from Future/ (same depth, D5)
    README.md                      "How this works" (D8) + PRD index (one-line summaries + Amended column)
    template.md                    moved from the prd-discovery skill (D8)
    PRD-NNN-*.md
    TASKS-PRD-NNN.md               only while a delivery runs; deleted at close, never a resident
  standards/                       rule documents (D13)
    server/  client/  api/
  tutorials/
    getting-started.md             single entry tutorial (absorbs DOCKER-SETUP's quick start)
    first-time-setup.md
    images/first-time-setup/…
  how-to/
    run-locally-without-docker.md
    run-the-docker-stack.md
    configure-platform-administrators.md
    troubleshoot-auth-cookies-and-cors.md
    deploy-to-azure.md             replaces the referenced-but-missing Azure-Deployment-Notes.md
  reference/
    configuration.md               every env var / options section, once
    ports-and-environments.md      every port and stack, once
    permissions.md                 the real permission list and roles
    auth-and-sessions.md           token lifetimes, claims, cookie attributes, auth endpoints
    glossary.md                    Accrued, Arrears, Committed, Available, Daily Need, Reserved …
    diagrams/                      pot_erd.d2/.png, Azure .mmd/.png + generate script
  explanation/
    cashflow-model.md              the obligation-first model with worked examples
    authentication.md              signup, login, refresh, logout, reset flows (concepts, no code dumps)
    multi-tenancy.md
  user-guide/                      renamed from USER-GUIDE/
Source/Server/DEVELOPER.md         ≤150-line index (D6)
Source/Server/docs/*.md            worked examples and recipes only (D6), each ≤ ~800 lines
Source/Client/pot-react/DEVELOPER.md   index
Source/Client/pot-react/docs/*.md      worked examples and component catalogue only (D6)
Source/Client/pot-react/e2e/README.md, AUTHORING.md   stay (reference + how-to)
Source/Docker/DEVELOPER.md         stays as the Docker reference
Data/Local Docker/README.md, Data/Seed Docker/README.md   stay (each describes its own stack)
```

### 6.2 Where does X go?

This table becomes the core of `Docs/README.md`.

| If you are writing… | It goes in |
| --- | --- |
| A significant technical decision, and why | `Docs/adr/` |
| What a feature must do, and for whom | `Docs/prd/` |
| A later change to a Complete PRD | that PRD's `## Amendments` |
| Steps a newcomer follows once, start to finish | `Docs/tutorials/` |
| Steps to achieve one task | `Docs/how-to/` (product-wide) or `Source/<component>/docs/` (component-internal) |
| Facts to look up (ports, config keys, permissions, claims) | `Docs/reference/` |
| How a concept works as a whole | `Docs/explanation/` |
| A binding coding convention (naming, structure, validation, …) | `Docs/standards/` (D13) |
| How to write code in one component (worked examples, recipes, catalogues) | `Source/<component>/docs/` |
| What an end user sees and does | `Docs/user-guide/` |
| A rule an agent must follow | `.github/instructions/` (the condensed rule plus a citation of its ADR or rule document, no rationale) |
| A term with a precise meaning | `Docs/reference/glossary.md` |

Two rules come with it:

- **Never restate** what the code, `package.json`, the csproj files or `contracts/openapi/v1.json` already say. Version numbers belong in package manifests, not prose.
- **Rationale lives once**, in the ADR. Everything else links to it.

### 6.3 Mapping of existing files

| Current | Action |
| --- | --- |
| `README.md` | Keep. Add links to the docs map and the ADR index; the setup docs are currently missing from its list. |
| `Docs/ARCHITECTURE.md` | Slim (section 7). Cashflow sections go to `explanation/cashflow-model.md` and ADRs 0008–0015. The test split goes to ADR-0058. Setup steps are deleted (they duplicate the tutorial). |
| `Docs/AUTHENTICATION.md` | Split four ways (the content is listed below the table). Drop the large code snapshots: several describe the removed `User.RefreshToken` design. |
| `Docs/GETTING-STARTED.md` | Becomes `tutorials/getting-started.md`, the single entry point. |
| `Docs/DOCKER-SETUP.md` | Its quick start merges into the tutorial; the rest goes to `how-to/run-the-docker-stack.md`. The duplicated verification blocks are removed. |
| `Docs/LOCAL-SETUP.md` | Becomes `how-to/run-locally-without-docker.md`. Option B is replaced with the `Data/Local Docker` stack. |
| `Docs/FIRST-TIME-SETUP.md` (+ `Docs/images/first-time-setup/`) | Becomes `tutorials/first-time-setup.md`. It becomes the only place for bootstrapping the first platform admin. |
| `Docs/FEATURES.md` | Merges into `user-guide/README.md` (a third copy of the feature list). |
| `Docs/USER-GUIDE/*` | Moves to `user-guide/`. Projections.md's client persistence and fetch-window spec moves to ADRs and reference. |
| `Docs/USER-GUIDE/_DOCUMENTATION_PLAN.md` | Retire after harvesting anything still valid. It is a stale work plan, wrong in at least six places. |
| `Docs/Future/PRD-*.md`, `README.md` | Move to `Docs/prd/` (D5). Contents unchanged apart from the amendments. A `TASKS-PRD-NNN.md` is a working document deleted at close, so it is never a resident of `prd/`; `TASKS-PRD-031.md` has already been removed. |
| `Docs/Future/ADR-012…`, `ADR-014…` | Re-homed (5.4). Tombstones are left behind. |
| `Docs/pot_erd.d2` / `.png` | Move to `reference/diagrams/`. **Caveat:** `Pot.Data.Migrations/App.cs:61` hard-codes `..\..\..\..\..\..\Docs\pot_erd.d2`, so moving it is a one-line code change (the owner's approval), or the file stays where it is. |
| `Docs/Azure/*` | Moves to `reference/diagrams/`. `Docs/pot-azure-diagram.png` is a *different, older* render of the same diagram; delete it. |
| `Source/Server/DEVELOPER.md` | Split (D6). "Key Architectural Principles" (148–197) becomes links to ADRs. The second CORS copy is deleted. |
| `Source/Server/IMPLEMENTATION.md` | Its domain explanation goes to `explanation/cashflow-model.md`, and its calculator detail to `Source/Server/docs/calculations.md`. Fix the encoding. Income and expense renewal collapse into one section with a differences table. |
| `Source/Client/pot-react/DEVELOPER.md` | Split (D6). Regenerate the stale tree and table of contents. |
| `Source/Docker/DEVELOPER.md` | Keep. Fix the three dead links to `Azure-Deployment-Notes.md`. |
| `Data/Local Docker/README.md`, `Data/Seed Docker/README.md` | Keep. Each describes its own stack. Their ports and purposes feed `reference/ports-and-environments.md`. |

`Docs/AUTHENTICATION.md` splits into:

- about ten ADRs (0030–0039, 0071);
- `explanation/authentication.md` (flows);
- `reference/auth-and-sessions.md`;
- two how-tos (platform admins; cookies and CORS);
- the user-facing parts, which move to `user-guide/Security.md`.

---

## 7. What happens to ARCHITECTURE.md

It stays at `Docs/ARCHITECTURE.md`, rewritten as a 150–250 line arc42/C4-lite overview:

1. **Purpose and quality goals.** Projection-first, obligation-first, deterministic server-side computation, tenant isolation.
2. **Context and container diagram.** Mermaid, rendered with the existing `mmd2png` skill: browser/PWA → nginx → API → PostgreSQL, SMTP, Azure.
3. **Building blocks.** Server layers, client feature modules, test projects, one paragraph each.
4. **Cross-cutting concepts.** One paragraph each with ADR links: tenancy, authentication and sessions, the error contract, concurrency, rate limiting, health, time and money.
5. **Deployment view.** Local stacks and Azure, linking `reference/ports-and-environments.md`.
6. **A link to the ADR index.**

It must not contain:

- **Version numbers.** They already drift: ADR-012 says Vite 6 while the code is on Vite 8.
- **Rationale.** That lives in the ADRs.
- **Setup steps.** Those live in the tutorial.
- **Domain formulae.** Those live in `explanation/cashflow-model.md` and the glossary.

---

## 8. ADR backlog

**Reading the table:**

- **Numbers** are provisional and grouped by area. The backlog is renumbered into a clean sequence in Phase 1 (D13), after the folded rows below are removed. A dropped row consumes no number.
- **Status values:**
  - `Acc`: accepted, decided and current.
  - `Sup`: superseded (written for history, per D8).
  - `Prop`: proposed, its PRD not complete.
  - `Dec`: waits for its brief in `DECISIONS-TO-DISCUSS.md` (D10, section 11).
  - `Ver`: verify against the code before writing, because the docs disagree.
- **Tier values:**
  - `1`: write first.
  - `2`: write in the main pass.
  - `3`: fold into a neighbour or drop.

### Process

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0001 | Record architecture decisions in one numbered ADR series | Acc (new) | this plan | 1 |
| 0002 | Give every kind of document exactly one home | Acc (new) | this plan §6 | 1 |
| 0003 | Freeze PRDs at Complete; record later change as amendments | Acc (new) | PRD-023 OD-05, PRD-030 N2 (implicit today) | 1 |
| 0004 | Deliver PRDs through the discovery/delivery skills; agents never stage or commit; no PRD/ADR IDs in code | Acc | prd skills; copilot-instructions:7–18 | 1 |
| 0005 | Layer agent guidance (always-on, glob-scoped, skills); instruction files cite ADRs | Acc | .github/README; copilot-instructions | 2 |
| 0006 | CRLF line endings everywhere except shell scripts | Acc | .gitattributes; .editorconfig; .prettierrc; OpenAPI normaliser | 3 |

### Product and domain (cross-cutting)

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0007 | POT forecasts from schedules; it is not a transaction register (manual balances, no bank feeds) | Acc | README:9–50; copilot-instructions:3; PRD-024:119 | 1 |
| 0008 | Obligation-first, cash-basis model with rolling forecast windows | Acc | ARCHITECTURE:146–189 | 1 |
| 0009 | Persist account accrual and recalculate on a dirty flag | Sup by 0010 | PRD-003; PRD-009 | 2 |
| 0010 | Derive accrual on read from the schedule; never persist a computed value | Acc | PRD-023 | 1 |
| 0011 | Two accrual metrics (stable daily need; dynamic operational), gated by per-expense policy `Automatic`/`None` | Acc | PRD-003; ARCHITECTURE:120–132; IMPLEMENTATION:123–135 | 1 |
| 0012 | Obligation components: Accrued + Arrears = Committed; Available = Balance − Reserved − Committed | Acc | PRD-023 OD-17; ARCHITECTURE:138–144 | 1 |
| 0013 | `NextDue` is the user-acknowledged schedule cursor; due-day timing, renewal modes and calendar arithmetic (incl. `EndOfMonth`) | Acc | PRD-023:32, 228; IMPLEMENTATION:65–398; FrequencyExtensions.cs | 1 |
| 0014 | Projections are a day-by-day in-memory simulation over immutable cursors; nothing is persisted | Acc | PRD-023:186–204; IMPLEMENTATION:608–823 | 2 |
| 0015 | The projection API publishes components; the client composes balances and totals | Acc | PRD-025; PRD-028 | 1 |
| 0016 | One business time zone (Australia/Sydney) through `ITimeProvider`; `DateOnly` schedules, UTC timestamps | Acc (interim; PRD-018 proposes superseding) | Pot.App/AppContext.cs; ITimeProvider | 1 |
| 0017 | Money is `double`, rounded to 2 dp away from zero; AUD and en-AU assumed | Dec | pot_erd.d2; ExpenseEntity.cs:25; moneyUtils.ts | 1 |
| 0018 | Export packages are versioned; import accepts only the current version and never deletes | Acc | PRD-021:91–173; PRD-023 OD-02; MetadataV1–V4 readers | 2 |
| 0019 | A settlement ledger, if built, is history only and never feeds a calculation | Prop | PRD-024 | 3 |

### API contract (cross-cutting)

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0020 | Client and server release together; breaking API changes need no compatibility window | Acc | PRD-021:169; PRD-025:261; PRD-028:73; PRD-030:157 | 1 |
| 0021 | Expose only GUID `RowId`s; integer keys stay internal | Acc | server.instructions:17; EntityBase.cs | 1 |
| 0022 | Optimistic concurrency via an app-managed `Etag` in the body; a stale tag answers 409 | Acc + Ver | server DEVELOPER:4783–4862; PRD-030:137; CheckHasSameEtag (not a database concurrency token, see §11) | 1 |
| 0023 | Every failure is RFC 9457 Problem Details, written regardless of `Accept` | Acc | PRD-030; server DEVELOPER:3862–3945 | 1 |
| 0024 | Refusals answer 422 Unprocessable Entity | Sup by 0025 | PRD-026:428–433 | 2 |
| 0025 | Validation failures and refused business rules answer 400 with an `errors` map | Acc | commit `bec69a48` (2026-10-16); PRD-030 | 1 |
| 0026 | 409 conflicts are distinguished by problem `type` URN | Acc | PRD-030; server DEVELOPER:957–967 | 2 |
| 0027 | A correlation ID flows from the client header into logs and every problem body | Acc | server DEVELOPER:3745–3768; CorrelationIdMiddleware; apiHelpers.ts | 2 |
| 0028 | The OpenAPI document is generated at build time and committed as the contract | Acc | server.instructions:21–28; Pot.AspNetCore.csproj | 1 |
| 0029 | Client API types are hand-written; not generated from, or validated against, the contract | Dec | source scan (no codegen, no `.parse` on API data) | 2 |

### Security and identity (cross-cutting)

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0030 | Tenants are isolated by site (query filters for financial data, site-scoped repositories for users and sites, named cross-tenant reads); a foreign target reads as missing (404) | Acc | PRD-026; server DEVELOPER:4678–4780 | 1 |
| 0031 | Authorisation fails closed: authentication by default, an explicit anonymous allow-list, pinned by test | Acc | PRD-026:437 | 1 |
| 0032 | `resource:action` permissions, resolved from the database on every request | Acc | AUTHENTICATION:1902–2017; AddRolesAndPermissions migration | 1 |
| 0033 | Platform administration is granted by deployment configuration, not a role | Acc | AUTHENTICATION:2021–2155; PRD-026:518; PRD-027:75 | 1 |
| 0034 | A site's last enabled Admin and the platform's last live platform administrator are protected | Acc | PRD-026:439–441 as amended by PRD-022:43–57 | 1 |
| 0035 | Sign-up requires email verification and platform-admin approval; rejected usernames stay locked | Acc | AUTHENTICATION:24–256 | 2 |
| 0036 | Credentials: PBKDF2 hashing via the Identity hasher (without Identity), single-use one-time codes, enumeration-safe reset with temporary password | Acc + Dec (plaintext codes) | AUTHENTICATION:568–944; UserPasswordHasher.cs; OneTimePasswordEntity.cs; PRD-032 may amend | 1 |
| 0037 | Short-lived access tokens with minimal claims plus server-side AuthSessions with a rotating HttpOnly refresh cookie | Acc | PRD-001; PRD-002; AUTHENTICATION:1258–1313, 1844–1898 | 1 |
| 0038 | Revocation per session (`session_id` checked on every request) and globally (`TokenVersion`) | Acc | PRD-002:55; commit `31c96313` (2026-10-17) | 1 |
| 0039 | One server-side chained rate-limit policy, partitioned by the forwarded caller address or the signed-in user; accounts are never locked | Acc | server DEVELOPER:3539–3669, 3414–3449; commit `ceb57419`; PRD-014:27–37; user-guide Security; ForwardedHeadersOptionsSetup.cs | 1 |
| 0040 | Credentialed CORS only from configured origins, exposing only the headers a cross-origin caller needs (`content-disposition`, `location`, `retry-after`, correlation id) | Acc | server DEVELOPER:3450–3538; commit `dcdf0744` (exposes `location`) | 2 |

### Backend

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0041 | Layered server (API → App → Data, plus Shared) with feature slices mirrored across layers; auth lives in the host | Acc | ARCHITECTURE:95–118; server.instructions:10–15 | 1 |
| 0042 | Endpoints are Minimal API slices: static handler, typed results, separate request/response models, hand-written mapping (no AutoMapper) | Acc | server DEVELOPER:440–489, 2920–3086 | 1 |
| 0043 | Build on the AllOverIt library family | Acc | Directory.Packages.props; allsoverit-patterns.instructions | 1 |
| 0044 | Services are registered by marker-interface scanning | Dec (conflicts with server.instructions:61–65) | server DEVELOPER:5065–5262 | 2 |
| 0045 | Business failures are `EnrichedResult` values, not exceptions | Acc | server DEVELOPER:2228–2305; PRD-030:138 | 1 |
| 0046 | Request shape is validated in the handler; data-dependent rules are entity-check chains | Acc | server DEVELOPER:340–368, 2041–2226, 3268–3412 | 1 |
| 0047 | Split read/persist repositories, no-tracking by default, explicit transactions | Acc | server DEVELOPER:2369–2803 | 1 |
| 0048 | Persistence conventions: `Entity` suffix, enriched enums stored by name, `citext` with check-plus-index uniqueness | Acc | server DEVELOPER:4118–4674 | 2 |
| 0049 | Cascade delete is restricted; deletions are ordered hard-delete sweeps; no soft delete | Acc | server DEVELOPER:4570–4589; PRD-022 | 1 |
| 0050 | A separate runner applies migrations before the API starts; destructive changes ship in two append-only steps; humans generate migrations | Acc | PRD-023:313–324; PRD-013; entrypoint.sh | 1 |
| 0051 | PostgreSQL version target | Dec (13.6 pinned, EOL; Azure runs 17.6) | DbContextOptionsBuilderExtensions.cs:11; Postgres Dockerfile; Azure .mmd | 1 |
| 0052 | Configuration binds to validated options sections that fail at startup | Acc | server DEVELOPER:5061; server.instructions:27 | 2 |
| 0053 | Liveness is separate from readiness; background workers wait for readiness | Acc | PRD-013; PRD-030:67 | 2 |
| 0054 | Background work runs in the API process (single-replica assumption) | Acc + Dec | Features/Workers; PRD-019 proposes an outbox | 2 |
| 0055 | Email is queued in process and rendered with Razor components | Acc (PRD-019 proposes superseding) | Pot.EmailSender; Pot.RazorComponents | 2 |
| 0056 | Log request metadata only, never submitted values; structured exception logging; JSON console in production | Acc | PRD-030 A1–A6; allsoverit-patterns:64–68 | 1 |
| 0057 | Middleware order is a contract (forwarded headers first); security headers on every response | Acc | Program.cs:51–132; server DEVELOPER:3671–3861 | 2 |
| 0058 | Server tests: per-layer unit projects plus one hosted integration project on Testcontainers; architecture rules pinned by fixture tests | Acc | ARCHITECTURE:191–211; dotnet-tests.instructions; aspnetcore-integration-tests.instructions (the authorization matrix and isolation invariants belong to 0083) | 1 |
| 0059 | Server tests stay on VSTest (`xunit.v3.mtp-off`) until MTP mode works | Acc | PRD-020; dotnet-tests:27 | 3 |
| 0060 | Build quality gates: nullable warnings are errors, `.editorconfig` is the style source, central package versions with major-version ranges | Acc + Prop (PRD-029) | PRD-029; Directory.Build.props / Directory.Packages.props | 2 |

### Frontend

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0061 | Feature modules over shared `api/`, `concerns/` and `lib/` layers, with one-way dependencies | Acc + Ver (existing feature→feature imports must be sanctioned or scheduled) | client DEVELOPER:106–261; client.instructions:12–18 | 1 |
| 0062 | shadcn/ui primitives are vendored and never hand-edited | Acc | PRD-015:33; client.instructions:16 | 1 |
| 0063 | Server state lives in TanStack Query over one axios instance; invalidation follows a dependency graph | Acc | client DEVELOPER:2339–2705; cacheInvalidation.ts | 1 |
| 0064 | API hooks return `Result` values; one interceptor normalises every failure and never fails on a body | Acc | client DEVELOPER:2612–2723; PRD-030 E1–E8 | 1 |
| 0065 | One blocking ErrorSheet, transient toasts and a root error boundary | Acc | client DEVELOPER:1870–2322; App.tsx | 2 |
| 0066 | Client state split by kind (Zustand persisted user, Query, Context, React Hook Form) | Acc | ARCHITECTURE:269–274; client DEVELOPER:2323 | 2 |
| 0067 | Browser storage is scoped by environment and user, with a persistence policy per setting | Acc | PRD-001 (amended by PRD-011, PRD-025) | 1 |
| 0068 | The URL is the only render-time source of truth for list filters | Acc | PRD-011 | 1 |
| 0069 | Lazy declarative routes behind `ProtectedRoute`; create/edit forms are route-driven non-modal sheets | Acc | AppRoutes.tsx; client DEVELOPER:1354–1595, 2545–2611 | 2 |
| 0070 | Forms use React Hook Form with Zod; API types are derived with `z.infer` | Acc | client DEVELOPER; feature schemas | 2 |
| 0071 | Client token handling: access token in memory only, proactive and reactive refresh, one logout path | Acc + Ver (the docs disagree on the refresh model) | AUTHENTICATION:948–1488; authInterceptor.ts; logoutManager.ts | 1 |
| 0072 | Honour `Retry-After` and never auto-retry a 429 (re-homes legacy ADR 014) | Prop | ADR-014; PRD-014 | 2 |
| 0073 | Client configuration is baked in at build time | Acc | Docker DEVELOPER:368–388, 492–504 | 2 |
| 0074 | PWA precaches only the shell, never `/api`, and enforces updates | Acc | client DEVELOPER:2939–3125; vite.config.ts | 1 |
| 0075 | Login alone gets a longer timeout with progress feedback | Acc | PRD-006 | 3 |
| 0076 | Strict TypeScript and lint policy; no React Compiler | Acc | eslint.config.js; tsconfig.app.json; PRD-015 DQ-7 | 2 |
| 0077 | Client dependencies upgrade in gated micro-batches; TypeScript 7 waits for typescript-eslint | Acc | PRD-015; PRD-016; PRD-017 | 2 |
| 0078 | Client unit tests use Vitest with module mocks; MSW is deferred behind stated triggers | Acc | client DEVELOPER:2837–2915 | 2 |

### End-to-end testing (cross-cutting; replaces legacy ADR 012)

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0079 | E2E runs one shared stack with Testcontainers Postgres and a single seeded database | Acc | ADR-012 §3–4, §8–9, App. A | 1 |
| 0080 | Every E2E test is classified as parallel-safe or fixture-managed | Acc | ADR-012 §7 | 1 |
| 0081 | Every E2E test logs in fresh; storage state is never shared | Acc | ADR-012 §6 | 2 |
| 0082 | E2E execution policy: dev matrix by default, prodlike gate, fixed timeouts, config-owned reporters, four-browser matrix | Acc + Dec (prodlike: per-PRD gate or on demand?) | ADR-012 §10, §13; e2e README:150–153; e2e.instructions:13 | 2 |
| 0083 | Scenario-driven tenant testing: test-only message capture and mail sink, product-route provisioning (server scenarios and a third, **destructive** E2E class that provisions its own tenant), canary-tenant isolation invariants, an authorization matrix generated from the contract, and opt-in scenario and browser tiers (amends 0058, 0079 and 0080) | Acc (implementation landed; PRD-031 completed 2026-11-02) | PRD-031; aspnetcore-integration-tests.instructions; e2e.instructions; run-tests SKILL "Tiers"; prd-delivery step 4.8 | 2 |

### Infrastructure

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0084 | Production runs on Azure Container Apps from GHCR images; nginx serves the SPA; the API is on its own subdomain; the API trusts the ingress's `X-Forwarded-For`/`-Proto` from any sender because only the ingress can reach it | Ver (docs disagree on whether ingress routes `/api`) | Azure .mmd; Docker DEVELOPER:667–723; server DEVELOPER:3414–3449 (commit `ceb57419`); AUTHENTICATION:1612–1662 | 1 |
| 0085 | Multi-stage, non-root Alpine images containing runtime projects only | Acc | PRD-007:61–114; Dockerfiles | 2 |
| 0086 | Images build with the cache by default | Sup by 0087 | PRD-007:56–76 | 3 |
| 0087 | Images always build with `--no-cache` | Acc | PRD-007:246–264 | 2 |
| 0088 | Local environments: prodlike compose stack (also the dev-mode API's database), a scratch database and a seed-authoring database on bind mounts, E2E on its own Testcontainers database, fixed ports, one stack at a time | Acc | Data/Local Docker/README; Data/Seed Docker/README; Docker DEVELOPER:48–61 | 2 |
| 0089 | Secrets stay out of the repo: a tracked template, an ignored env file, the platform secret store in production | Acc + Ver (Phase 0b: no tracked template exists; `Source/Docker/.env` is tracked and holds only `COMPOSE_PROJECT_NAME` and `POSTGRES_DB`) | GETTING-STARTED:103–126; Docker DEVELOPER:417–456 | 2 |
| 0090 | Build and deploy by hand from VS Code tasks; no CI | Dec | .vscode/tasks.json; no `.github/workflows` directory | 2 |

**Folded rows (agreed).** These rows are written as part of their host ADR, and their numbers are not used:

| Folded row | Host |
| --- | --- |
| 0005 | 0004 |
| 0026, 0027 | 0023 |
| 0040 | 0037 |
| 0052 | 0060 |
| 0057 | 0056 |
| 0065 | 0064 |
| 0066 | 0063 |
| 0073 | 0085 |
| 0081 | 0080 |

**Smaller decisions to fold in, not number:**

- The dependency-advisory handling (the `serialize-javascript` override; the icon generator removed, with the icons committed) folds into 0077. Evidence: client DEVELOPER 2975, 2979–3011.
- "The OpenAPI document declares only the failures an endpoint's own rules can produce" widens 0028. Evidence: server DEVELOPER 519–521, 965–967, 1698–1707, plus the per-slice schema names at 325–332.
- "No test tier is time-limited; every documented command reports its duration" folds into 0083 (run-tests "Tiers", e2e README).
- "Database backups are kept outside the repository" folds into 0089 (`.gitignore`; Docker DEVELOPER 589–590).
- The compose stack's missing server health check is decided in DD-16, then recorded in 0053 or 0088.

---

## 9. Things that are not ADRs

These stay as rules, examples or reference, with no ADR of their own:

- **C# style:** sealed by default, no primary constructors, one class per file, member ordering, `CancellationToken` last, no `ConfigureAwait(false)`, XML docs. These become rules in `Docs/standards/server/` (D13), with `.editorconfig` taking precedence for anything it checks. ADR-0060 covers *how* style is enforced.
- **TypeScript style:** function declarations, exports at the bottom, barrel files, braces. These become rules in `Docs/standards/client/`.
- **Component catalogues:** tables, cards, badges, pickers, theme, chart sizing. These go in the client reference.
- **Single-place reference facts:** ports, the env-var list, the permission list, the token claim list, the seeded E2E identities.
- **Small API conventions:** 201 for create and 200 for delete, action-style POST for bulk operations, the login `status` discriminator. These go in the API reference, or are folded into ADR-0023 or 0035.
- **Build details:** manual vendor chunks, the logger singleton, the accessibility-first locator order in E2E.

---

## 10. Content to correct during the migration

Each item is fixed when its file is reworked, not in a separate pass.

**Fix early: wrong in a security-relevant way, or misleading enough to cause bad code**

1. **Server DEVELOPER 4022–4084:** the `DatabaseExceptionHandler` sample echoes `PostgresException.MessageText` and maps everything to 500. The real handler returns 409 or 500 with fixed messages and never echoes database text.
2. **AUTHENTICATION 923 and 2163 say BCrypt.** The code uses the Identity `PasswordHasher` (PBKDF2).
3. **AUTHENTICATION 461, 469, 836, 1335 and 1417–1460 describe the removed `User.RefreshToken` design.** Lines 1460 and 2166 say there is "no server-side session store", which contradicts AuthSession.
4. **Access-token storage contradicts itself.** AUTHENTICATION 1181 and 1246 say sessionStorage, while 1269 and 1300 say memory only. The refresh model at 1826–1832 contradicts 952–971.
5. **The "Site Owner" role doesn't exist.** The user guide (README 77–79 and every page) and `_DOCUMENTATION_PLAN.md` use it; the real roles are Admin and Viewer. AUTHENTICATION 1919 lists a non-existent `import-export:manage` permission.
6. **ARCHITECTURE 284–291 says production is Docker Compose with Docker PostgreSQL.** It is Azure Container Apps with a separate PostgreSQL 17.6.

**Fix when the file is reworked**

7. **ARCHITECTURE.md:**
   - The tree (38–79) shows `store/` and omits `Pot.Shared.Tests`, `Pot.RazorComponents` and `contracts/`.
   - The stack list omits AllOverIt, TanStack Table, the PWA, Scalar and Tailwind v4.
   - Line 272 says Zustand holds authentication; the access token is in React context.
8. **Server DEVELOPER:**
   - JWT claims (240, 3251–3258) omit `session_id`. Line 240 also still says `tokenVersion` "and nothing else"; the claim is `token_version`.
   - The enriched-enum sample (4362) omits `EndOfMonth`.
   - Line 207 lists 204, but deletes return 200.
   - Line 1875 (`WhenNotNull` on every dependency) contradicts allsoverit-patterns:18 (DD-15).
   - The table of contents is wrong.
9. **IMPLEMENTATION.md:**
   - Encoding corruption at about 20 lines.
   - `AccruedIsDirty` at 442 and 603 no longer exists.
   - The projection mechanism text (374–381, 732–740, 767–777, 802, 804–813) predates the fold or cursor design.
   - `EndOfMonth` is never mentioned.
10. **Client DEVELOPER:**
    - The source tree (46–101) is stale.
    - It claims ESLint "strict" (262–323), but the config is `recommended` + `stylistic`.
    - Filter persistence is described as localStorage (1854–1866); the code uses the URL plus sessionStorage.
    - Auth and logout paths are wrong (2391–2427).
    - The cache invalidation path is wrong (2678).
    - It lists a `ValidationError (422)` that doesn't exist (2707–2726).
    - It imports from `react-router-dom` (2556).
    - Chart heights are stale (1689–1759).
    - The toast import points at a non-existent hook (765).
11. **E2E docs:**
    - `e2e/README.md:10`, `e2e/AUTHORING.md:5` and `ADR-012:20` link to a non-existent `playwright-e2e.instructions.md`.
    - The fixture lists in `AUTHORING.md:31` and `e2e.instructions:42` are stale: they list three fixtures, but `e2e/fixtures/auth.ts` exports four (`quickActionsTest` is missing).
    - `e2e/README.md:121–128` lists three canonical users; the seed has four (`e2e_quickactions` is missing).
    - `e2e/README.md:398` refers to a "see 8.7" that matches nothing.
    - Playwright and Vite versions are stale (`e2e/README.md:88` says Playwright 1.59.1; `package.json` has `^1.62.1`).
    - e2e.instructions:13 calls prodlike a per-PRD completion gate, while the README (150–153) calls it on-demand (DD-11).
    - e2e.instructions:67 says react-day-picker v9; the project uses v10.
12. **Setup docs:**
    - Node minimum: 22.22.2 vs `>=22.22.0` vs "Node 24 is the minimum".
    - `.env.development`: "create from scratch" vs "copy the tracked template". **Phase 0b:** no template exists in the repository or on disk, so the "copy" instruction has nothing to copy; add a tracked template (row 0089) or correct the instruction.
    - `Docs/Azure/Azure-Deployment-Notes.md` is linked three times (Docker DEVELOPER 107, 720 and 843) but is untracked and ignored, so those links are dead for every other reader. **Phase 0b:** the file exists on this machine only; the owner decides whether to track it, track a redacted template, or remove the links.
    - The GETTING-STARTED anchor at 228 is broken.
    - DOCKER-SETUP's "curl" output is actually PowerShell output.
    - LOCAL-SETUP Option B collides with the prodlike container name.
13. **FIRST-TIME-SETUP:**
    - Status names are inconsistent: `PendingApproval` vs `Approval`.
    - It says only platform admins see the Users menu; the real gate is `user:manage`.
14. **Other docs:**
    - FEATURES 145 claims automatic logout on inactivity, which doesn't exist.
    - USER-GUIDE/Security 7 says a password change ends "every other session"; it ends every session, including the current one.
15. **Compose file comment** (`docker-compose-client-server.yml:430`): it refers to `/swagger`, but the API UI is Scalar and is Development-only.

---

## 11. Code risks: decided in `DECISIONS-TO-DISCUSS.md`

The code risks and the `Dec` backlog rows are decided one at a time in `DECISIONS-TO-DISCUSS.md` (D10). This is not documentation work. The ADR for each row is written only once its brief is decided.

| Backlog row or rule | Brief |
| --- | --- |
| 0017 money as `double` | DD-03 |
| 0020 no compatibility window | DD-14 |
| 0022 Etag (`Ver`) | DD-02 |
| 0029 hand-written client types | DD-07 |
| 0032 permission policy strings | DD-09 |
| 0036 plaintext one-time codes | DD-08 |
| 0044 DI by marker scanning | DD-10 |
| 0051 PostgreSQL version | DD-06 |
| 0053 / 0088 compose health check | DD-16 |
| 0054 single replica, in-process email | DD-04 |
| 0058 EF InMemory in unit tests | DD-12d |
| 0061 feature-to-feature imports (`Ver`) | DD-13 |
| 0082 prodlike E2E gate | DD-11 |
| 0090 no CI | DD-05 |
| `WhenNotNull` rule (server guide vs instructions) | DD-15 |
| Housekeeping: Cronos, sonner theme, animate plugin, ERD path | DD-12a, b, c, e |

---

## 12. Changes to skills, templates and agent instructions

| File | Change |
| --- | --- |
| `.github/skills/prd-discovery/references/prd.template.md` | **Moves to `Docs/prd/template.md`** (D8); the skill links to the new path. Add a commented `**Amended:**` header line, used only once amended. Add an `ADR` column to "Decisions Carried Into This Document". Add an `## Effect on Earlier Documents` section (Document, What changes, Amendment). Add a closing `## Amendments` section containing "None." Add `## Requirement Coverage` and `## Verification Evidence`, which the delivery skill already writes at close. |
| `.github/skills/prd-discovery/SKILL.md` | **Stage 5:** classify each carried decision as product or architectural, apply the significance test (5.3), and draft a `proposed` ADR for each architectural decision. **Before deleting the discovery document**, harvest the rejected alternatives into those ADRs; today that rationale is lost when the scratch file is deleted. Fill in "Effect on Earlier Documents". Update the paths if D5 is approved. |
| `.github/skills/prd-delivery/SKILL.md` | **Step 4 (verify):** add a check listing decisions taken during delivery that are not covered by an ADR, and raise or justify each. **Step 5 (close):** flip the PRD's ADRs to `accepted`; write an `AM-n` entry into every document named in "Effect on Earlier Documents", plus any other contradicted Complete PRD; keep the index row to one line. Replace "per `Docs/Future` conventions" with a link to ADR-0003. Its close-out already harvests deliberately-broken checks, measured baselines and the evidence's limits into the PRD before deleting the task list, so only the ADR flip and the amendments need adding. |
| `.github/skills/prd-delivery/references/stage-plan.template.md` | Path update (D5). |
| `.github/skills/adr/SKILL.md` (new) | Create an ADR from `Docs/adr/template.md` following `Docs/adr/README.md`, take the next number from the folder, fill in the front matter (with evidence for retroactive records), update the related instruction-file citations, and run `docs-check`. Never stages or commits, like the other skills. |
| `Docs/adr/README.md` (new) | A hand-written "How this works" section (D8) above the generated index. |
| `.github/scripts/docs-check/` (new) | `docs-check.ps1`, `README.md`, Pester tests and fixtures (D11). Add a row for it to `.github/scripts/README.md`. A VS Code task runs it. |
| `Docs/standards/` (new, D13) | Rule documents, each starting with a quick-reference list of its rules and their enforcement tags. |
| `Docs/Future/README.md` (becomes `Docs/prd/README.md`) | Add a hand-written "How this works" section (D8). Remove the "next free ADR number" text. Define the amendment convention by linking ADR-0003. Add the `Amended` column. Trim the summaries that carry supersession history (003, 009, 015, 023, 026). |
| `.github/copilot-instructions.md` | **Line 14:** keep the ban on PRD and ADR IDs in code, but change its reason, which currently calls these documents "temporary". New wording: "PRDs and task lists are temporary; ADRs are permanent but are cited from docs and instruction files only — code states the rule itself." **New rule:** "Before an architectural change, read `Docs/adr/README.md`; do not contradict an accepted ADR without asking; a change that makes a new architectural decision adds a proposed ADR in the same change." **References section:** point at the new paths. |
| `.github/instructions/*.instructions.md` | Append the ADR number to each rule cluster, for example `… a foreign id is a 404 (ADR-0030).` Rules stay imperative; the rationale is not copied in. |
| `AGENTS.md` (D12, approved) | A short root file that points at `.github/copilot-instructions.md`, the instruction files, `Docs/README.md`, `Docs/adr/README.md` and the rule documents. It does not restate their rules. Created in Phase 1. |

---

## 13. Migration phases

Each phase ends with a stop for the owner’s review and an offered commit message. Agents never stage or commit.

| Phase | Work | Output |
| --- | --- | --- |
| **0b. Re-baseline** — done 2026-11-02 | Ran at `9024e1d2`; the outcome is the [Phase 0b record](#phase-0b-record-completed-2026-11-02-at-9024e1d2). | This plan and `DECISIONS-TO-DISCUSS.md` updated in place. **Waiting only on the owner's OK to start Phase 1.** The three owner decisions the record lists are needed at Phase 2 (the instruction-file link) and Phase 5 (the Azure notes and the env template), so they gate nothing earlier. |
| **1. Foundations** | Create `Docs/adr/` with `template.md`, the "How this works" README, ADR-0001 to 0004 and the generated index. Move the PRD template to `Docs/prd/template.md` (still under `Docs/Future/` until Phase 5) and write its "How this works" section. Write `Docs/README.md` (docs map plus the "where does X go" table). Build `docs-check.ps1` with its Pester tests and VS Code task. Make the skill and copilot-instructions changes from section 12. Create the `adr` skill and `AGENTS.md`. Create `Docs/standards/` (`server/`, `client/`, `api/`) with an empty rule-document template and the three standards ADRs. Renumber the section 8 backlog into a clean sequence, without the folded rows. | Conventions in place before any content moves. |
| **2. Re-home the legacy ADRs** | Write ADR-0079 to 0082 from ADR-012 and ADR-0072 from ADR-014. Move ADR-012's reference content into `e2e/README.md`. Leave tombstones at the old paths. Fix the broken instruction-file link. | ADR-012 and ADR-014 retired. |
| **3. Write the ADRs in themed batches** | Batches of 5–10 ADRs per commit, in this order: domain, then the API contract, security, backend, frontend, infrastructure. Each batch also adds the ADR citations to the instruction files. The `Ver` rows are checked against the code first. Each `Dec` row is written once its item in `DECISIONS-TO-DISCUSS.md` is decided. | The ADR series. |
| **4. PRD amendments** | Retrofit the 29 existing notes (D3) and add the missing amendments (D4), one PRD per change. The new ADRs give the 422→400 and session-binding amendments something to cite. Clean up the index summaries. | Every overtaken PRD says so in one predictable place. |
| **5. Restructure** | Move and split files into the section 6 layout: slim ARCHITECTURE.md, split AUTHENTICATION.md, split the developer guides per Appendix E (rules into `Docs/standards/`, examples beside the code), build the glossary and the reference pages, apply the section 10 corrections as each file is touched, and rename the folders (D2, D5). The ERD path in `App.cs` changes only with the owner's approval. `docs-check` must pass before each commit. | The target structure. |
| **6. Steady state** | The governance rules in section 14 apply from here on. | — |

**Why restructuring comes last:** Phases 3 and 4 only *add* files and sections. Phase 5 rewrites each existing file once, replacing duplicated rationale with links to ADRs that by then exist. Doing the moves first would mean editing most files twice.

---

## 14. Governance: keeping it from sprawling again

1. **A decision becomes an ADR in the same change.** Agents add it as `proposed`; the skills enforce this for PRD work.
2. **A change that contradicts a Complete PRD adds an `AM-n` entry to it in the same change.**
3. **Every document type has exactly one home**, set by the `Docs/README.md` table. Prose never restates code, manifests or the OpenAPI document.
4. **Size budgets:**
   - ADRs: about 150 lines or fewer.
   - Developer-guide topic files: about 800 lines or fewer.
   - `ARCHITECTURE.md`: about 250 lines or fewer.
   - Instruction files: "concise".
5. **`docs-check` passes before commit.** It checks:
   - the front matter;
   - that the generated index is current;
   - that relative links resolve;
   - that instruction-file citations point to accepted ADRs;
   - that no `PRD-`/`ADR-` IDs appear in `Source/**`.
6. **Review checklist** for any change:
   - Does it make a decision, and is there an ADR?
   - Does it contradict an ADR or a Complete PRD?
   - Were generated files (index, OpenAPI) regenerated?
   - Does `docs-check` pass?

---

## Appendix A — ADR template

```markdown
---
id: ADR-0030
title: Isolate tenants by site; a foreign target reads as missing
status: accepted              # proposed | accepted | rejected | deprecated | superseded
date: 2026-10-14              # when the decision was made (backfill: best evidence)
recorded: 2026-11-04          # when this record was written
provenance: retroactive       # contemporaneous | retroactive
evidence:                     # required when retroactive
  - ../prd/PRD-026-cross-tenant-authorization-hardening.md#4-design
  - ../../Source/Server/Pot.AspNetCore.Integration.Tests/Security/CrossTenantAuthorizationFixture.cs
area: cross-cutting           # frontend | backend | cross-cutting | infrastructure | process
tags: [security, multi-tenancy, authorization]
supersedes: []
superseded-by: []
amends: []
amended-by: []
related-prds: [PRD-026, PRD-022, PRD-027, PRD-031]
related-adrs: [ADR-0031, ADR-0034]
---

# ADR-0030: Isolate tenants by site; a foreign target reads as missing

**Summary:** In the context of a multi-tenant API where users and sites cannot carry a global query
filter, facing cross-tenant reads and writes through caller-supplied ids, we decided to remove raw
user/site queryables and resolve every target through site-scoped repository methods that report a
foreign id exactly as a missing one (404), to make isolation structural rather than per-route,
accepting a narrower repository API and a closed list of named cross-tenant reads.

## Context and Problem Statement
<3–10 lines: the forces, not the history of the discussion. Link the PRD section for detail.>

## Decision Drivers            <!-- optional -->
- <driver>

## Considered Options
1. Per-route site predicates (status quo before PRD-026)
2. Global query filters on `User` and `Site`
3. Site-scoped repository surface with no raw queryables

## Decision Outcome
Chosen option: 3, because <≤15 lines>.

### Consequences
- Good, because <…>
- Bad, because <…>

### Confirmation
- Test: `Pot.AspNetCore.Integration.Tests/Security/CrossTenantAuthorizationFixture.cs`
- Instruction: `.github/instructions/server.instructions.md` (Architecture)
- Review: any new id-taking endpoint resolves its target through a site-scoped method.

## Pros and Cons of the Options   <!-- optional -->

## More Information               <!-- optional: links and follow-ups; never implementation status -->
```

---

## Appendix B — PRD amendment format

**In the PRD that was overtaken.** The header gains one line; the body is untouched.

````markdown
**Status:** Complete
**Priority:** High
**Last Updated:** 2026-10-14
**Amended:** 2026-10-26 — 2 amendments, see [Amendments](#amendments)
**Feature ID:** 026

…(the PRD as approved — never edited)…

## Amendments

Append-only, oldest first. The text above is this document as approved; read it through these
entries. Each entry names what it affects and the document or change that caused it.

### AM-1 — Refusals answer 400, not 422 (2026-10-17)

- **Type:** Superseded
- **Affects:** every `422` in this document (§4.3 refusals; AC5)
- **Source:** [ADR-0025](../adr/0025-validation-and-refusals-answer-400.md) (commit `bec69a48`)

This document records `422 Unprocessable Entity` for the refusals it introduced. That status code
has since been replaced: every invalid or refused request now returns `400 Bad Request` with the same
ProblemDetails body … *(moved verbatim from the original top-of-file note)*

### AM-2 — A refusal's message is read from the `errors` map (2026-10-26)

- **Type:** Superseded
- **Affects:** how a refusal's message is read (the `400` responses of AM-1)
- **Source:** [PRD-030](PRD-030-problem-details-consolidation.md), [ADR-0023](../adr/0023-problem-details-contract.md)

A `400` now carries the validation problem's `errors` map … *(moved verbatim)*

### AM-3 — A platform administrator may be removed while another remains (2026-10-27)

- **Type:** Superseded
- **Affects:** §4.4, §4.8, D2, AC5
- **Source:** [PRD-022](PRD-022-user-and-site-deletion.md) R7–R9; [ADR-0034](../adr/0034-protected-identities.md)

*(new entry, D4)*
````

An in-body pointer is used only where a reader would otherwise act on a now-wrong paragraph:

```markdown
> Amended — see [AM-1](#am-1--refusals-answer-400-not-422-2026-10-17).
```

**In the PRD that causes the change.** This is a new template section, filled in during discovery:

```markdown
## Effect on Earlier Documents

| Document | What changes                                           | Amendment |
| -------- | ------------------------------------------------------ | --------- |
| PRD-026  | A platform administrator is no longer always protected | AM-3      |
| PRD-027  | Notification and protection rules it reuses change     | (rewrite) |
```

---

## Appendix C — Existing amendment notes to retrofit

Each of these 29 sites moves verbatim into an `AM-n` entry in its own document (D3), apart from the exceptions below.

| # | Location | Current format | Cause | Date |
| --- | --- | --- | --- | --- |
| 1 | PRD-001:357–359 | unlabelled paragraph in a list | session binding (commit `31c96313`; belongs to PRD-002) | 17 Oct 2026 |
| 2 | PRD-003:34–38 | status bullets + "Supersession note" | PRD-023 | 2026-09-27 |
| 3 | PRD-003:70 | section blockquote "Snapshot note" | PRD-023 | 2026-09-27 |
| 4 | PRD-007:246–264 | appended H2 "Supplementary Implementation Note" | own incident (cache-first reversed) | 2026-05-03 |
| 5 | PRD-009:5–10 | header bullets + "Supersession note" | PRD-023 | 2026-09-27 |
| 6–7 | PRD-012:104, :195 | blockquotes under R2, R10 | PRD-023 | 2026-09-27 |
| 8 | ADR-012:325 | blockquote in a numbered step (also stands for :112, :178) | PRD-023 | 2026-09-27 |
| 9 | ADR-012:361–363 | undated parenthetical; **wrongly credits PRD-030** | 422→400 (`bec69a48`) | — |
| 10 | PRD-014:7 | Scope header rewritten | CORS exposes `Retry-After` | 2026-10-19 |
| 11 | PRD-014:35 | undated parenthetical | PRD-030 | — |
| 12 | PRD-014:36, ADR-014:37–39 | trailing "Added …" sentence | CORS exposure | 2026-10-19 |
| 13 | PRD-015:14–99 | dated H3 decision log, "RESOLVED" markers | own living log | 2026-09-03 to 09-21 |
| 14 | PRD-018:32 | blockquote in a list | PRD-023 | 2026-09-27 |
| 15 | PRD-019:43 | blockquote in a bullet | PRD-023 | 2026-09-27 |
| 16–18 | PRD-023:144, :146, :324, OD table cells | bold inline "Amended …", table-cell markers | own post-implementation changes | 2026-09-27 |
| 19 | PRD-023:220, :385 | italic parenthetical | PRD-025 | 2026-10-03 |
| 20 | PRD-024:40 | blockquote after a list | PRD-023 | 2026-09-27 |
| 21 | PRD-025:7 | italic parenthetical inside Scope | PRD-028 (no back-link) | 2026-10-04 |
| 22 | PRD-025 (several rows) | "updated / dropped on …" in tables | own rework | 2026-10-03 |
| 23 | PRD-026:3–7 | top blockquote "Superseded contract note" | 422→400 (no PRD) | 2026-10-17 |
| 24 | PRD-026:9–13 | top blockquote "Problem Details note" | PRD-030 | 2026-10-26 |
| 25 | PRD-027:3–17 | top blockquote "Needs rewriting" | several | 2026-10-17+ |
| 26 | PRD-028:7 | italic parenthetical inside Scope | own post-completion rename | 2026-10-04 |
| 27 | PRD-022:172 | undated inline sentence | own test-plan rewrite | — |
| 28–29 | `Docs/Future/README.md` rows 003, 009, 015, 023 | clauses in the Summary column | PRD-023 and own changes | various |

**Exceptions (D3):**

- **Sites 13 and 16–22** stay in place. They are a PRD amending itself during or straight after delivery, so they are part of its own record.
- **Site 25** (PRD-027, not delivered) is folded into PRD-027’s rewrite.

---

## Appendix D — PRDs affected by later decisions that carry no note

Under D4, each of these gets a new `AM-n` entry.

| Document | What is now wrong | Cause |
| --- | --- | --- |
| PRD-026 §4.4:441, §4.8:493/504, D2:603, AC5:618 | A configured platform administrator can never be disabled | PRD-022 R7–R9 |
| PRD-026 §4.6:465 | Token claims "…and nothing else" | `session_id` (commit `31c96313`) |
| PRD-026:625–626 | "Blocked pending this document" | PRD-022 and PRD-026 delivered |
| PRD-002 | No note on session binding (it was put in PRD-001); open decisions unanswered although Complete; title still says "Placeholder" | commit `31c96313` |
| PRD-001:94 | Legacy keys are purged | PRD-025 R11 removed the purge |
| PRD-001:78–80 | Storage read on render (S004–S006) | PRD-011 (URL is authoritative) |
| PRD-009:155–157 | Package "current version is v3" | PRD-021 (v4) |
| PRD-021:91–97 | Any column change bumps the version | PRD-023 OD-02 changed v4 in place |
| PRD-012 NG5, D5, C2–C3, R3 / ADR-012 §1.3, §2, §14.2 | No email E2E; committed seeds only | PRD-031 mail sink and route provisioning (landed in `ee148852` and `5d9d9fad`; PRD-031 completed 2026-11-02, so the entry is due) |
| ADR-012 §5:185–203 | Three seeded users | Four now: `e2e_quickactions` was added, and PRD-022's two deletion identities were removed again by the PRD-031 work (`5d9d9fad`) |
| ADR-012 §7:254–293 | "Every test belongs to one of two classes" | PRD-031 adds a third class, **destructive**, which provisions the tenant it destroys (`5d9d9fad`; e2e.instructions, e2e/README "Test isolation model") |
| ADR-012 §6.2:227–243 | Auth fixtures include the deletion identities | Those fixtures were removed (`5d9d9fad`); `e2e/fixtures/auth.ts` exports four |
| PRD-012 C7:245 / ADR-012:107, :133 | Vite 6, Playwright 1.59 | PRD-015 |
| ADR-014:34–36 | Error code `TooManyRequests` in the body | PRD-030 removed `errorCode` |
| PRD-014 OQ-6:76 | Can the server omit `Retry-After`? | Answered by PRD-030 B8 |
| PRD-013 | Probes not described as short-circuited | PRD-030:67, :143 |
| PRD-018 Q5:113, :149 | Points at PRD-009 dirty-flag behaviour | PRD-023 |
| PRD-019:30, R9, OD-04 / :41 | "Seven" email writers; old cross-tenant method name | PRD-022 notifications; PRD-026:457 |
| PRD-025 R5, R6, R8, payload table, Tests | `global` / "Total (All Accounts)" | PRD-028 |
| PRD-028:55 | "Implementation has not started" (Status is Complete) | Corrected |
| PRD-007:3, :54–76 | Header "Planned"; cache-first decisions | Corrected / own incident |
| `Docs/Future/README.md` rows 007, 026, 027 | Cache-first; "refuse (422)"; "Blocked" with no rewrite flag | as above |

---

## Appendix E — Developer guide classification

This appendix sorts every section of the three developer guides into the layers agreed in D6 (accepted as written):

- **Decision:** goes to an ADR.
- **Rule:** goes to a rule document (D13).
- **How-to:** worked examples, which stay beside the code.

It also flags sections that are **Tool-enforced**, **Duplicate** or **Stale**. Every code check behind a note is named in the Notes column.

- **Guides covered:**
  - `Source/Server/DEVELOPER.md` (5,347 lines)
  - `Source/Client/pot-react/DEVELOPER.md` (3,228 lines)
  - `Source/Docker/DEVELOPER.md` (847 lines)
- **Reading the Mix column:** it estimates how each section's lines split between Decision, Rule and How-to, in that order. Code blocks count as How-to, so example-heavy sections score high on How-to even when their prose is mostly rule.
- **Folded backlog rows** are cited through their host row (section 8).
- **Naming conventions:**
  - Rule-document topics are written as `server/<topic>` and `client/<topic>`.
  - How-to files are written as `docs/<name>.md`, relative to the component folder (`Source/Server/` or `Source/Client/pot-react/`).


### E.1 Server guide: `Source/Server/DEVELOPER.md`

| Section | Lines | Mix (D/R/H) | Primary category | Target | Notes |
| --- | --- | --- | --- | --- | --- |
| Title + Table of Contents | 1–61 | – | Delete | The new ≤150-line index | The TOC is wrong (plan §10.8). Regenerate it as part of the split. |
| Architecture Overview / Project Structure | 62–148 | 5/15/80 | How-to | 0041; rule `server/project-structure`; `docs/project-structure.md` | The tree is nearly current but misses `Pot.App/Mappings`, `Pot.AspNetCore/Concerns/OpenApi` and `Pot.Shared/Models` (checked with `ls`). The test-project list duplicates dotnet-tests.instructions:8. Replace it with one paragraph per layer plus a link. |
| Key Architectural Principles | 149–199 | 70/30/0 | Decision (+Duplicate) | One bullet group per ADR: 0032/0031 (1), 0041 (2), 0046/0023/0025 (3), 0045 (4), 0042 (5), 0030 (6), 0053 (7), 0047 (8); rate numbers → 0039 | This duplicates Security (233–249) and ARCHITECTURE 95–118. Line 153 says "role-based authorization", but permissions are resolved from the database (0032). The plan already says to turn this section into ADR links. |
| Design Conventions | 200–232 | 20/70/10 | Rule | 0021, 0022, 0042, 0045, 0048; rules `server/project-structure` (naming), `server/endpoints`, `server/error-handling` | Good rule material (naming patterns, RowId only). Line 207 lists 204, but deletes return 200 (plan §10.8). |
| Security | 233–249 | 65/30/5 | Decision | 0030, 0031, 0032, 0033, 0034, 0036, 0037, 0038, 0039, 0056 (absorbs 0057: headers); rule `server/security` | This is the best decision prose in the guide. Line 246 (the checklist for a new endpoint that takes an id) is pure rule. **Stale:** line 240 says the token holds "`jti`, `sub` and `tokenVersion` and nothing else", but `PotClaimTypes.cs` adds `session_id` (and the claim is `token_version`). Overlaps AUTHENTICATION.md. |
| Feature Implementation Recipe: Step-by-Step Guide | 250–268 | 0/30/70 | How-to | `docs/add-a-feature.md` | – |
| Example Walkthrough, Steps 1–13 | 269–890 | 10/20/70 | How-to | `docs/add-a-feature.md`; rules `server/endpoints`, `server/services` | All ten file paths checked exist (AccountsEndpoints.cs, Create/RequestMapping.cs, EntityChecks/Checks/*, Concerns/OpenApi/SchemaReferenceId.cs …). The embedded "why" decisions go to: OpenAPI schema naming (325–332) → 0028; Request vs Input (434–438) → 0042; validator vs entity check (361–362) and Chain of Responsibility (763–768) → 0046. Its code is repeated almost verbatim in Endpoint Patterns and Service Layer: keep this as the one worked example. |
| Example Walkthrough, Step 14 (test + error bodies) | 891–968 | 45/35/20 | Decision | 0023 (absorbs 0026), 0025, 0022; rule `server/error-handling` | The 409 `type` URN table and the "which endpoints declare 409" policy (957–967) are contract decisions buried in a how-to step. They duplicate Problem Details 3890–3964. Move them out and leave a link. Port 5242 and `/scalar` are current. |
| Feature Organization (API/App/Data/Shared) | 969–1242 | 10/30/60 | Duplicate | 0041; rule `server/project-structure` | The four trees repeat Project Structure (66–148) and the walkthrough. The App "Service Pattern" code (1088–1124) repeats the Service Layer. Keep only the "Critical" WithTracking note (1205) as a `server/data-access` rule. Collapse the rest into one structure page. |
| Endpoint Patterns: Handler Pattern | 1243–1432 | 10/25/65 | How-to | 0042; rule `server/endpoints`; `docs/endpoints.md` | The step order (validate → map → call → typed result) is the rule. The six handler variants are a good catalogue. |
| Request Validation | 1433–1550 | 10/25/65 | How-to | 0046; rule `server/endpoints`; `docs/endpoints.md` | "Common Validation Rules" (1516–1549) is generic FluentValidation: trim it. The validator base-class rules overlap allsoverit-patterns. |
| Response Construction | 1551–1645 | 10/30/60 | How-to | 0042, 0021/0022 (ResponseBase); rule `server/endpoints` | Current, including the note that there is no zero-argument `Response.Ok()`. |
| Endpoint Registration | 1646–1763 | 25/40/35 | Rule | 0028 (response metadata), 0039 (group rate limit); rule `server/endpoints` | Lines 1698–1707 (Produces vs ProducesProblem, 400/500 on the group, 408/413 per route) are binding and pinned by `EndpointResponseMetadataFixture`. |
| Service Layer: Service Pattern | 1764–2040 | 5/25/70 | How-to | 0045, 0030; rule `server/services`; `docs/services-and-entity-checks.md` | **Rule conflict:** line 1875 says to guard every DI dependency with `WhenNotNull()` (as `CreateAccountService.cs:27` still does), but allsoverit-patterns:18 says not to. Lines 2011 (empty list, never 404) and 2038 (foreign reads as 404) are rules that duplicate Security. The ConfigureAwait note (1883) is C# style (plan §9). |
| Entity Checks | 2041–2227 | 15/20/65 | How-to | 0046; rule `server/services` | The code duplicates walkthrough Step 10 (624–768): keep one copy. The guidelines (2219–2226) are rule. |
| Result Pattern | 2228–2368 | 30/30/40 | Decision | 0045, 0025 ("no 422", 2357–2359), 0028 (2361–2366); rule `server/error-handling` | The rationale (2232–2238) is ADR material. The contract paragraph (2362–2367) duplicates the OpenAPI section of server.instructions. |
| Transaction Management | 2369–2474 | 15/35/50 | How-to | 0047; rule `server/data-access` | The guidelines (2439–2445) are rule. Merge in the "query, act, query, act" rule from server.instructions, which is missing here. |
| Repository Layer: Repository Pattern | 2475–2722 | 20/30/50 | How-to | 0047, 0030 (no `IQueryable` for User/Site); rule `server/data-access`; `docs/data-access.md` | The method-naming list (2707–2721) is rule. |
| Tracking vs No-Tracking | 2723–2804 | 30/30/40 | Duplicate | 0047; rule `server/data-access` | Repeats 2582–2606, and Performance Considerations repeats it again (4276–4294). Keep one copy. |
| Query Specifications | 2805–2919 | 10/20/70 | How-to | 0043 (AllOverIt) / 0047; `docs/data-access.md` | No ADR of its own. |
| Mapping Patterns (Request→Input, Entity→Output) | 2920–3087 | 20/35/45 | Rule | 0042 (no AutoMapper); rules `server/project-structure` (placement), `server/endpoints` | The `RequestMapping` placement duplicates server.instructions ("Feature-specific request mapping …"). |
| Concerns (intro) | 3088–3110 | 0/20/80 | How-to | `docs/cross-cutting.md` | A folder tree only. |
| Authentication & Authorization | 3111–3267 | 30/20/50 | Stale | 0031, 0032, 0033, 0037, 0038; rule `server/security`; `Docs/reference/auth-and-sessions.md` | **Stale:** the claims block (3249–3259) omits `session_id` (plan §10.8). "Refresh Token Pattern" (3261–3266) is thin and duplicates AUTHENTICATION.md, so replace it with a link. The permission list (3242–3247) belongs in `reference/permissions.md`. |
| Validation (Concerns) | 3268–3413 | 15/25/60 | How-to | 0046; `docs/cross-cutting.md` | Internals of `ProblemDetailsInspector`, for maintainers. Overlaps Request Validation (1433–1550). |
| Forwarded Headers | 3414–3449 | 50/30/20 | Decision | 0084 (trust model), 0039 (partition key), 0057 (first in the pipeline); rule `server/security` | The three things that broke without it, and the trust model (clear the known networks, `ForwardLimit` 1, ignore `X-Forwarded-Host`), are ADR text. "Never expose the container directly" and "revisit if a CDN is placed in front" are rules. Matches `ForwardedHeadersOptionsSetup.cs` (checked). |
| CORS Configuration (first copy) | 3450–3538 | 40/30/30 | Decision | 0037 (absorbs 0040); `Docs/reference/configuration.md` | Keep this copy. The exposed-headers rationale (including `location`) and the 10-minute preflight cache (3506) are ADR text. Line 3518 (the `AllowedOrigins` format) is reference. |
| Rate Limiting | 3539–3670 | 40/20/40 | Decision | 0039, 0072 (`Retry-After`); rule `server/security` | Defaults 15 and 50 match `RateLimiterDefaults.cs`. Per-IP partitioning (`RateLimiterPolicy.cs:32`) reads the forwarded caller address. The numbers belong in reference, not prose. |
| Middleware | 3671–3861 | 50/25/25 | Decision | 0056 (absorbs 0057; request logging metadata only), 0023 (absorbs 0027; correlation ID); rules `server/security`, `server/error-handling` | The documented order matches `Program.cs:55–117` (checked). The rationale prose (3729–3768) is ADR material. The UserContext/ICurrentUserContext code (3770–3861) is How-to. |
| Problem Details | 3862–4087 | 45/30/25 | Decision | 0023 (absorbs 0026/0027), 0025, 0056; rule `server/error-handling`; default-`detail` table → `Docs/reference/` | **Stale:** the `DatabaseExceptionHandler` sample (4022–4084) differs from the real handler, which maps to fixed 409/500 messages (`DatabaseExceptionHandler.cs:26–33`; plan §10.1, fix early). The "always write" rationale (3940–3945) is ADR material. |
| Entity Framework Core (intro: EntityBase) | 4088–4117 | 50/40/10 | Decision | 0021, 0022 | "Why three identifiers" is ADR text. Duplicated at 4324–4349. |
| Entity / Table Naming Convention | 4118–4195 | 30/50/20 | Rule | 0048; rule `server/project-structure` (naming) | The debug-only suffix check exists (`DbContextBase.cs:16, 90`). |
| Entity Conventions + Entity Configuration | 4196–4273 | 10/70/20 | Rule | 0048; rule `server/data-access` | The data annotations claim is true (`AccountEntity.cs:10`). "Seal entity classes" overlaps server.instructions. |
| Performance Considerations | 4274–4323 | 20/50/30 | Duplicate | 0047; rule `server/data-access` | Repeats Tracking (2723–2804) and server.instructions (N+1, no-tracking). |
| API Design Guidelines | 4324–4349 | 30/60/10 | Duplicate | 0021, 0022 | The third statement of RowId/Etag (after 204–206 and 4105–4111). Delete. |
| Enriched Enums | 4350–4465 | 35/25/40 | Decision | 0048; rule `server/data-access`; `docs/entities-and-schema.md` | **Stale:** the `Frequency` sample (4362) omits `EndOfMonth` (`Frequency.cs:14`). "Why strings" (4391–4396) is ADR text. The enum list (4451–4462) is reference, and its permission list matches `Permission.cs`. |
| Entity Relationships | 4466–4592 | 20/30/50 | How-to | 0049 (cascade restrict); `docs/entities-and-schema.md` | The cascade rationale (4587–4589) belongs in the ADR. |
| Indexes | 4593–4677 | 15/40/45 | How-to | 0048 (citext + check-plus-index, 4570–4572); rule `server/data-access` | Current. |
| Multi-Tenancy & Query Filters | 4678–4782 | 45/30/25 | Decision | 0030; rules `server/security`, `server/data-access` | The third statement of the two-mechanism model (after 157/183–187 and 237). Keep the mechanics here and move the "why" to 0030. |
| Optimistic Concurrency (ETags) | 4783–4865 | 40/20/40 | Decision | 0022 (Ver) | "Why" (4857–4862) is ADR text. The plan §11 caveat applies: this is not a database concurrency token. |
| Migrations | 4866–4988 | 15/25/60 | How-to | 0050; rule `server/data-access`; `docs/migrations.md` | **Stale:** line 4937 says the runner is used by "CI/CD pipelines", but there is no CI (0090). The commands are repeated at 5316–5324 and in Docker 191–195. The "humans generate migrations" rule (server.instructions) is missing here. |
| CORS Configuration (second copy) | 4989–5064 | 40/30/30 | Duplicate | Delete (plan §6.3); move 5022 (startup validation) → 0060 (absorbs 0052) | Keep only line 5061. |
| Dependency Injection | 5065–5265 | 30/30/40 | Decision | 0044 (Dec); rule `server/di-and-configuration`; `docs/dependency-injection.md` | **Conflict:** the guide and the code agree (91 Pot.App files implement `IPotScopedDependency`; `AutoRegisterScoped` in WebApplicationBuilderExtensions.cs:457), while server.instructions (DI section) says "register concrete classes … interface only when …". The instruction file is the outlier. |
| Accrual Metrics Semantics | 5266–5305 | 90/10/0 | Decision (domain) | 0010, 0011, 0012; `Docs/explanation/cashflow-model.md` | Domain explanation, not developer guidance. Duplicates ARCHITECTURE 120–133 and IMPLEMENTATION. Remove from this guide. |
| Available Commands | 5306–5336 | 0/10/90 | How-to | The index, or `docs/commands.md` | Migration rows repeat 4888–4978. |
| Additional Resources | 5337–5347 | – | How-to | The index | – |

### E.2 Client guide: `Source/Client/pot-react/DEVELOPER.md`

| Section | Lines | Mix (D/R/H) | Primary category | Target | Notes |
| --- | --- | --- | --- | --- | --- |
| Title + Table of Contents | 1–45 | – | Delete | The new index | It lists headings that do not exist (Accounts/Expenses/Incomes Page, Reusable Layout Components) and omits Badge Styling System (plan §10.10). |
| Architecture Overview (stack + tree) | 46–101 | 10/10/80 | Stale | 0061; rule `client/project-structure` | Stale tree (plan §10.10). The "Vite 8" version belongs in package.json (plan §6.2). Duplicates ARCHITECTURE 81–93. |
| Project Organization | 102–261 | 35/45/20 | Rule | 0061; rule `client/project-structure` | "Why this pattern" (201–207) and lib vs concerns (191–199) are ADR text. **Stale:** line 260 says "Features never import from other features", but accounts, expenses, incomes, users and userSettings import `@/features/auth`, and dashboard imports expenses and incomes (grep). That is 0061's "Ver". The feature tree (114–124) omits the real `create/ edit/ delete/ schemas/ utils/` sub-folders. |
| TypeScript Standards | 262–323 | 25/25/50 | Tool-enforced (Stale) | 0076; config `eslint.config.js`, `tsconfig.app.json` | **Stale:** it cites `tseslint.configs.strict` in `eslint.config.mjs`. The real file is `eslint.config.js`, using `recommended` + `stylistic` (lines 14–16). `no-non-null-assertion` and `no-floating-promises` are not enabled. Reduce to "see the config" plus the "no `any`" rule. |
| Code Formatting (exports, barrels, spacing, braces) | 324–477 | 10/60/30 | Rule | rule `client/code-style`; config `.prettierrc` | None of these is lint-enforced (no `curly` or padding rules in eslint.config.js), so they really are rules. They duplicate client.instructions "TypeScript style". The import sort is enforced by `simple-import-sort` and should only be pointed at. |
| Function Declarations | 478–518 | 30/50/20 | Rule | rule `client/code-style` | Duplicates client.instructions:38–39. |
| Custom Components (intro) | 519–528 | 30/70/0 | Rule | 0062 | Duplicates client.instructions:16. |
| Table, Cards, Dialog components | 529–719 | 5/10/85 | How-to | `docs/components.md` | A catalogue (plan §9). |
| Feedback components | 720–793 | 5/25/70 | How-to | `docs/components.md`; 0064 (absorbs 0065) | **Stale:** line 765 imports `@/hooks/useToast`, which does not exist. The guidelines (782–788) duplicate 2223–2240 and client.instructions:30. |
| Badge Styling System | 794–1023 | 10/40/50 | Rule | rule `client/components-and-styling`; `docs/styling-and-layout.md` | The colour-semantics table and "always use the helpers" are rules. Duplicates client.instructions:31. `src/lib/badgeStyles.ts` exists. |
| Filter, Input, Layout, Navigation, Picker, Theme, User components | 1024–1353 | 5/10/85 | How-to | `docs/components.md` | Theme persistence (1284–1285) is a storage policy → 0067. |
| Sheet Components | 1354–1595 | 20/45/35 | Rule | 0069; rule `client/components-and-styling` | The binding conventions are `modal={false}`, hidden default X, the two widths and route-driven forms. The Separator + `opacity-80` rule (client.instructions:31) is missing here. |
| Projections Page | 1596–1785 | 5/25/70 | Stale | `docs/styling-and-layout.md` | **Stale:** `ProjectionsPage.tsx:247` is `h-screen overflow-hidden`, not `md:h-screen`, and the chart is `h-[500px]`/`min-h-[500px]` with a short-viewport branch (`ProjectionChart.tsx:176–181`), not 400px. The "Pure CSS responsive" learning (1749–1753) is contradicted by that JS branch. Keep the learnings and drop the class-by-class breakdown. |
| Dashboard + Table-Based Pages | 1786–1869 | 5/40/55 | Stale | 0068, 0067; `docs/styling-and-layout.md` | **Stale:** 1854–1866 says filters persist in localStorage, but `useExpenseStorage.ts:34` uses sessionStorage and the URL (plan §10.10). |
| Error Handling: core, standard pattern, examples, mutation, storage, early return | 1870–2222 | 5/25/70 | How-to | 0064 (absorbs 0065); `docs/error-handling.md` | The root error boundary (0065) is not documented anywhere in the guide. |
| When to Use ErrorSheet vs Toasts + Guidelines | 2223–2254 | 20/80/0 | Rule | 0064; rule `client/error-handling` | The core rule list. Duplicates 782–788 and client.instructions:30. |
| Common Patterns | 2255–2322 | 0/10/90 | Duplicate | – | Repeats 1954–2221. Delete. |
| State Management | 2323–2384 | 30/50/20 | Duplicate | 0063 (absorbs 0066), 0067; rule `client/data-and-state` | Duplicates client.instructions "State and data flow" and ARCHITECTURE 269–274. The `useLocalStorage('theme')` example ignores the env/user-scoped keys (0067). React Hook Form + Zod (0070) is not covered anywhere in the guide. |
| Authentication & Authorization | 2385–2544 | 15/35/50 | Stale | 0071, 0032; rule `client/auth-and-routing` | **Stale paths:** AuthContext is at `src/features/auth/contexts/AuthContext.tsx`, and logoutManager is at `src/concerns/auth/logoutManager.ts` (2391, 2422, 2427). "PermissionGuard vs WithPermission" (2506–2509) is a rule. The permission list belongs in `reference/permissions.md`. |
| Routing | 2545–2611 | 30/40/30 | Rule | 0069; rule `client/auth-and-routing` | **Stale:** a `react-router-dom` import (2556). The `/logout` and catch-all rules duplicate client.instructions:27. |
| API Integration: Result Pattern | 2612–2653 | 40/30/30 | Decision | 0064 | – |
| Available API Hooks | 2654–2673 | 0/20/80 | How-to | `docs/data-fetching.md` | The names match the `useApi.ts` exports (checked). |
| Cache Invalidation | 2674–2706 | 20/40/40 | Stale | 0063; rule `client/data-and-state` | **Stale:** the path `src/lib/cacheInvalidation.ts` should be `src/concerns/cache/cacheInvalidation.ts`. |
| Error Types | 2707–2726 | 10/10/80 | Stale | 0064, 0025 | Lists `ValidationError (422)`, which does not exist. `apiErrors.ts` has `BadRequestError` and `ServerError`, and the list omits both. Replace the list with a pointer to the file. |
| Import/Export Features | 2727–2766 | 50/20/30 | Decision (local) | 0063; `docs/data-fetching.md` | A small design note, not ADR-worthy on its own. |
| Environment Configuration | 2767–2836 | 20/30/50 | How-to | 0085 (absorbs 0073); `Docs/reference/configuration.md` | **Stale:** `.env.development` and `.env.production` are git-ignored (`.gitignore:405–406`) and absent from a clean checkout, yet they are shown as if tracked. Duplicates Docker 492–504. |
| Testing Mocking Strategy | 2837–2915 | 60/25/15 | Decision | 0078; rule `client/testing` | Already written almost as an ADR (options, trade-offs, triggers). The rule side is in client-tests.instructions. |
| Available Commands | 2916–2938 | 0/10/90 | How-to | The index | **Stale:** "ESLint with strict TypeScript rules". It omits the `e2e:*` scripts; point at package.json instead. |
| PWA: What is a PWA? + Why PWA | 2939–2964 | 60/0/40 | Decision | 0074 | "What is a PWA" is generic: delete it. "Why" is ADR context. |
| PWA: Setup, `serialize-javascript` override, Icon Generation | 2965–3028 | 30/10/60 | How-to | 0077 or the new candidate N1; `docs/pwa.md` | The `vite-plugin-pwa@^1.3.0` pin duplicates package.json. The CVE override and the removal of the generator are decision records. |
| `vite.config.ts` Changes | 3029–3081 | 50/20/30 | Decision | 0074 | Restates config: point at `vite.config.ts`. The "Key configuration decisions" table is ADR content. |
| Service Worker Registration and Update UX | 3082–3125 | 55/20/25 | Decision | 0074 | The strongest decision content in this guide: enforced update, quiet window, `version.json` channel (the files exist under `src/concerns/pwa/`). **Conflict:** line 3122 relies on a "backward-compatibility policy", but 0020 says breaking changes need no compatibility window. |
| `index.html` / TypeScript support / Build Output | 3126–3177 | 5/10/85 | Delete | – | A changelog ("was updated"). The precache count "~64 entries" drifts. The files themselves are the source. |
| Validating PWA + Local Sanity Check | 3178–3215 | 0/10/90 | How-to | `docs/pwa.md` | – |
| Additional Resources | 3216–3228 | – | How-to | The index | – |

### E.3 Docker guide: `Source/Docker/DEVELOPER.md`

| Section | Lines | Mix (D/R/H) | Primary category | Target | Notes |
| --- | --- | --- | --- | --- | --- |
| Title + TOC | 1–26 | – | How-to | Keep | The `Docs/DOCKER-SETUP.md` link must follow the plan's rename. |
| Overview | 27–47 | 10/20/70 | How-to (reference) | 0088; `Docs/reference/ports-and-environments.md` | The port table duplicates docker.instructions:9 and the two `Data/*/README.md` files. |
| Other Docker stacks | 48–61 | 30/40/30 | Rule | 0088; rule `docker` | "One stack at a time" is repeated at 819. |
| Key terms | 62–74 | – | How-to | Keep (or the glossary) | – |
| Dependencies and startup order | 75–93 | 30/20/50 | Decision | 0050, 0053 | Matches the compose file (checked). Only postgres and client have health checks, so 0053's readiness has no compose expression (see N2). |
| Ports | 94–110 | 10/20/70 | How-to (reference) | `Docs/reference/ports-and-environments.md`; 0084 (Ver) | **Contradiction:** "ingress routes `/api/*` to the API container" against `VITE_API_BASE_URL=https://api.payontime.com.au/api` (499, 689). That is exactly 0084's "Ver". The `Azure-Deployment-Notes.md` link (107) is dead. |
| Development Workflow | 111–202 | 5/25/70 | How-to | `Docs/how-to/run-the-docker-stack.md`; rule `docker` | **Stale:** 116–117 say "each developer creates their own" `.env.development`, but `Source/Docker/.env.development.template` exists (plan §10.12). The rules are "Do not add `-p pot`" (173) and the env-file pair (155–158). The migration command duplicates the server guide. |
| Docker Compose Configuration | 203–315 | 20/10/70 | How-to | 0087 (`no_cache`), 0088; keep | The excerpts match the compose file (checked). The client `environment: NODE_ENV` is omitted (minor). The network YAML is repeated at 512–516. |
| Images and Dockerfiles | 316–408 | 35/10/55 | How-to | 0085 (absorbs 0073: build-time config), 0050 (migrations in entrypoint, `set -e`), 0087; keep | `entrypoint.sh` matches. "Node 24 is the minimum" (366) conflicts with the setup docs (plan §10.12), and versions belong in the Dockerfile. **Risk:** the `.env.production` fallback (379–381) relies on a git-ignored local file, and `.dockerignore` also drops the client `.env`, so a clean clone has neither fallback. The nginx notes omit the `/version.json` no-cache location (`nginx.conf:25`). |
| Environment Variables: `.env`, `.env.development` | 409–466 | 30/20/50 | How-to | 0089; `Docs/reference/configuration.md` | Should say "copy `.env.development.template`". |
| Variables compose passes / Client build arguments | 467–507 | 10/10/80 | How-to (reference) | `Docs/reference/configuration.md` | Matches the compose file (checked). Duplicates client guide 2767–2836. |
| Networking | 508–544 | 10/40/50 | How-to | 0088; rule `docker` (use service names, not localhost) | – |
| Data Persistence + Backup | 545–593 | 20/20/60 | How-to | 0088 (bind mounts); `Docs/how-to/run-the-docker-stack.md` | The reset steps are repeated at 777–780. It says backups live outside the repository (0089). |
| Health Checks | 594–666 | 25/15/60 | How-to | 0053; see N2 | 635–649 propose adding a server health check. That is an open design choice, not documentation. |
| Production Deployment | 667–723 | 40/10/50 | Decision | 0084 (Ver), 0087, 0090; `Docs/how-to/deploy-to-azure.md` | The link at 720 is dead. It contains the same `/api` ingress contradiction. The forwarded-headers bullet (713–716) is 0084 decision text. |
| Troubleshooting | 724–791 | 0/10/90 | How-to | Keep | "Rebuild after changing code" (782–791) duplicates 185–189. |
| Best Practices | 792–830 | 40/50/10 | Duplicate | 0085, 0089, 0088, 0053; rule `docker` | Generic advice that restates the earlier sections and the ADR rationale. "Pin base image versions" while using `nginx:alpine` and `postgres:13` is inconsistent. Delete once the ADRs exist; move the few real rules to `docker`. |
| Additional Resources | 831–847 | – | How-to | Keep | The link at 843 is dead. |


### E.4 Summary

#### E.4.1 Lines per category (approximate)

These totals are mix-weighted, so a section that is 30% rule adds 30% of its lines to Rule. Code counts as How-to.

| Guide | Total | Decision | Rule | How-to / example | Delete outright | Duplicate (primary) | Stale (primary) |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Server | 5,347 | ~1,220 (23%) | ~1,480 (28%) | ~2,590 (49%) | ~60 | ~510 | ~160 |
| Client | 3,228 | ~500 (16%) | ~920 (28%) | ~1,760 (55%) | ~100 | ~130 | ~540 |
| Docker | 847 | ~160 (19%) | ~140 (17%) | ~530 (64%) | 0 | ~40 | 0 |

**Tool-enforced:**

- Server: no section is tool-enforced. `.editorconfig` style lives only in server.instructions.
- Client: only TypeScript Standards (~60 lines), plus parts of Code Formatting.

**Duplicated or stale sections that are not primary:**

- The "Duplicate" and "Stale" columns count only sections where that is the primary category. Many more sections carry a duplicate or stale paragraph (see the Notes columns).
- Most of the server's decision lines are already stated two or three times inside the guide itself: tenancy three times, RowId/Etag three times, CORS twice, problem details twice.

#### E.4.2 Proposed rule documents

**Server (8 topics):**

| Topic | Fed by (lines) | Also merge |
| --- | --- | --- |
| `server/project-structure` (layers, slices, naming) | 62–148, 200–232, 969–1242 (trimmed), 2920–3087 (placement), 2707–2721, 4118–4195, 4904–4908 | server.instructions "Architecture" |
| `server/endpoints` (handler, validation call, response, registration, OpenAPI metadata) | 250–890 (rules only), 1243–1763, 2924–2994 | server.instructions "OpenAPI contract" |
| `server/services` (service shape, entity checks, results, transactions) | 1764–2227, 2228–2368 (rules), 2369–2474 | allsoverit-patterns |
| `server/error-handling` (status map, problem body, 409 types, logging of failures) | 200–232 (errors), 891–968, 2327–2367, 3671–3861 (correlation), 3862–4087 | – |
| `server/data-access` (repos, tracking, specs, entities, enums, relationships, indexes, ETags, migration guidelines) | 2475–2919, 4088–4677, 4783–4865, 4979–4988 | server.instructions "Persistence" |
| `server/security` (tenancy checklist, authz, forwarded headers, CORS, rate limits, headers) | 233–249 (esp. 246), 3111–3267, 3414–3670, 3729–3743, 4678–4782 | – |
| `server/di-and-configuration` (registration, lifetimes, validated options) | 5065–5265, 5061, 3518 | Resolve 0044 first. |
| `server/testing` (fixtures that pin architecture rules) | Barely fed by the guide: only the fixture names it cites (`EndpointResponseMetadataFixture`, `OpenApiSecurityFixture`, `SchemaReferenceIdFixture`, `EnrichedErrorExtensionsFixture`) | dotnet-tests and aspnetcore-integration-tests instructions |

**Client (8 topics):**

| Topic | Fed by (lines) | Also merge |
| --- | --- | --- |
| `client/project-structure` | 46–261 | client.instructions "Architecture" |
| `client/code-style` (with pointers to eslint, prettier and tsconfig for the enforced parts) | 262–518 | client.instructions "TypeScript style" |
| `client/components-and-styling` (shadcn rule, sheets, badges, page-layout learnings) | 519–528, 794–1023, 1354–1595, 1747–1784, 1803–1858 | – |
| `client/error-handling` | 782–788, 2223–2254, rule lines of 1870–2222 | – |
| `client/data-and-state` (state split, Result, cache invalidation, storage scope, URL filters) | 2323–2384, 2612–2766, 1860–1866 (corrected) | – |
| `client/auth-and-routing` | 2385–2611 | – |
| `client/testing` | 2837–2915 | client-tests.instructions |
| `client/build-and-pwa` (env vars baked at build, PWA never caches `/api`, enforced update) | 2767–2836, 2939–3125 | – |

**Docker (1 topic):** `docker`, fed by lines 48–61, 155–158, 173, 508–544 and the rule lines of 792–830. Merge docker.instructions, which already holds most of it.

#### E.4.3 Smaller decisions and contradictions

- The smaller decisions found in the guides are folded into existing rows; they are listed under section 8, "Smaller decisions to fold in".
- The contradictions the classification found are decided in `DECISIONS-TO-DISCUSS.md`. Write the affected ADR or rule only once its brief is decided:
  - feature-to-feature imports, client line 260 (row 0061, DD-13);
  - DI registration, server guide vs `server.instructions.md` (row 0044, DD-10);
  - the backward-compatibility policy assumed at client line 3122 (row 0020, DD-14);
  - `WhenNotNull` on injected dependencies, server line 1875 vs `allsoverit-patterns.instructions.md:18` (DD-15);
  - the missing server health check in the compose stack (rows 0053 and 0088, DD-16).

#### E.4.4 Sections to delete outright

| Guide | Lines | Section | Why |
| --- | --- | --- | --- |
| Server | 1–61 | TOC | Wrong; replaced by the split index. |
| Server | 969–1242 | Feature Organization | Repeats Project Structure and the walkthrough. Keep the one WithTracking warning. |
| Server | 4274–4323 | Performance Considerations | Repeats Tracking and server.instructions. |
| Server | 4324–4349 | API Design Guidelines | The third copy of RowId/Etag. |
| Server | 4989–5064 | CORS Configuration, second copy | The first copy is kept (section 6.3). Keep line 5061 (to 0060). |
| Server | 5266–5305 | Accrual Metrics Semantics | Move to `explanation/cashflow-model.md` and 0010–0012; it is domain content, not developer guidance. |
| Server | 3261–3266 | Refresh Token Pattern | Thin and partly stale. Replace with a link to `reference/auth-and-sessions.md`. |
| Client | 1–45 | TOC | Stale. |
| Client | 2255–2322 | Common Patterns | Repeats the error examples above it. |
| Client | 2941–2951 | What is a PWA? | Generic web content. |
| Client | 3126–3177 | `index.html` / TS support / Build Output | A changelog of a past change, not guidance. |
| Client | 1606–1745 | Class-by-class Projections layout breakdown | Stale. Keep only the learnings. |
| Client | 2707–2726 | Error Types | Stale. Replace with a pointer to `src/api/errors/apiErrors.ts`. |
| Docker | 792–830 | Best Practices | Restates the guide and the future ADR rationale. Move its few rules to `docker`. |
| Docker | 782–791 | Rebuild after changing code | A verbatim duplicate of 185–189. |
