# Documentation

This file is the map and the rule that keeps the documentation from sprawling: **every kind of
document has exactly one home.** Place a document by this table before writing it. A document that fits
no row is a reason to ask, not to invent a folder.

## The rule

Two things follow from having one home per kind:

- **Never restate** what the code, a package manifest, a project file or `contracts/openapi/v1.json`
  already says. Version numbers belong in the manifests.
- **Rationale lives once**, in an ADR. Every other document links to it.

## Where does X go?

| If you are writing…                                                       | It goes in                                                                                                    |
| ------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| A significant technical decision, and why                                 | `Docs/adr/` — see [adr/README.md](adr/README.md)                                                              |
| What a feature must do, and for whom                                      | `Docs/prd/`                                                                                                   |
| A later change to a Complete PRD                                          | that PRD's `## Amendments`                                                                                    |
| A binding coding convention (naming, structure, validation, …)            | `Docs/standards/` — see [standards/](standards/)                                                              |
| Steps a newcomer follows once, start to finish                            | `Docs/tutorials/`                                                                                             |
| Steps to achieve one task                                                 | `Docs/how-to/`, or `Source/<component>/docs/` when it is internal to one component                            |
| Facts to look up (ports, config keys, permissions, claims)                | `Docs/reference/`                                                                                             |
| How a concept works as a whole                                            | `Docs/explanation/`                                                                                           |
| How to write code in one component (worked examples, recipes, catalogues) | `Source/<component>/docs/`                                                                                    |
| What an end user sees and does                                            | `Docs/user-guide/`                                                                                            |
| A rule an agent must follow                                               | `.github/instructions/` — the condensed rule plus a citation of its ADR or rule document, never the rationale |
| A term with a precise meaning                                             | `Docs/reference/glossary.md`                                                                                  |

## How to write a document

- **A decision** → [adr/README.md](adr/README.md) explains when to write one, the format, and the
  statuses.
- **A requirement** → the `prd-discovery` skill produces a PRD; a later change is an amendment, not an
  edit.
- **A convention** → a rule document under [standards/](standards/), tagged by enforcement and
  provenance; start from [standards/rule-template.md](standards/rule-template.md).

## Checking the set

`.github/scripts/docs-check/docs-check.ps1` validates ADR front matter, regenerates the ADR index,
fails on a broken relative link in the documents the migration owns, checks that instruction files cite
only accepted ADRs, and rejects `PRD-`/`ADR-` identifiers in code under `Source/`. Run it from the
repository root; add `-Update` to regenerate the ADR index.
