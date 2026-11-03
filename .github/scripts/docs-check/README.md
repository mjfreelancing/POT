# Scripts: docs-check

Validates the documentation set and regenerates the ADR index. It exists because the repository has no
CI: the checks a pipeline would run on a pull request run here instead, before a change is offered for
review.

## What it checks

- **ADR front matter** — every file in `Docs/adr/` except `README.md` must carry `id`, `title`, `status`,
  `date` and `area`, with a four-digit file number matching its `id` and a `status` and `area` from the
  allowed sets. Front matter is limited to flat `key: value` pairs and simple lists; anything else is an
  error. The template itself lives with the `adr` skill.
- **The generated index** — the block between the markers in `Docs/adr/README.md` must match what the
  script would generate from the front matter.
- **Relative links** — every Markdown link in `Docs/adr/`, `Docs/standards/`, `Docs/prd/`, `.github/`,
  `Docs/README.md`, `AGENTS.md`, `README.md` and the two planning documents must resolve. Links inside
  fenced code blocks, absolute URLs and bare anchors are ignored, and a `fixtures/` folder is skipped
  because its documents are broken on purpose.
- **Instruction citations** — a `ADR-NNNN` reference in `.github/instructions/` must name an ADR that
  exists and is `accepted`.
- **Document identifiers in code** — a `PRD-NNN` or `ADR-NNNN` reference in code or configuration under
  `Source/` is an error. Markdown under `Source/` is exempt, because a developer guide may cite an ADR.

With `-ReportFacts` it also prints, for a small list of reference facts (ports, the health endpoints, the
two roles, a build-time variable), how many documents currently mention each. That report is
informational and never fails: it measures the duplication the migration is removing, so the count
should fall to one per fact and stay there.

Broken links in `Docs/Future/` (frozen PRDs), the legacy guides and the user guide are not checked yet:
they carry historical links to files that have since moved, and each is corrected when it is reworked.

## Running it

Run from the repository root. The `adr` skill runs it when a record is written and again before the
record is treated as final, and `prd-delivery` runs it when it accepts a PRD's records at close-out, so
you rarely need to run it by hand:

```powershell
pwsh ./.github/scripts/docs-check/docs-check.ps1
```

| Mode | Flags | Effect |
| --- | --- | --- |
| Check | none | Reports errors; changes nothing. Exit code 1 when anything fails. |
| Regenerate the index | `-Update` | Rewrites the generated block in `Docs/adr/README.md`, then checks everything else. |
| Duplication report | `-ReportFacts` | Prints where each reference fact appears and how many documents repeat it. |

## Tests

[Pester](https://pester.dev/) 5, against the fixtures in `fixtures/good` and `fixtures/bad`:

```powershell
Invoke-Pester -Path ./.github/scripts/docs-check/docs-check.Tests.ps1 -Output Detailed
```

The good fixture is copied to a temporary folder before each run, so the index regeneration never
touches a tracked file.

Keep usage guidance in this README and behaviour rules in `docs-check.ps1`.
