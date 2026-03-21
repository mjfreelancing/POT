# Skills: agent-env-tools

## Purpose

Document the skill assets for the `agent-env-tools` pack in this source repository.

## Files

| File                          | Purpose                                                                                |
| ----------------------------- | -------------------------------------------------------------------------------------- |
| `SKILL.md`                    | Workflow guidance for running environment readiness checks in agent-assisted sessions. |
| `scripts/agent-env-tools.ps1` | Diagnostics script used by this skill.                                                 |

## Copy target

Copy this folder to `.github/skills/agent-env-tools/` in the consuming repository.

## Suggested user prompt

Use this prompt in chat when you want the agent to run the full diagnostics check for all tools covered by this skill:

`Run the agent-env-tools skill now. Execute .\.github\skills\agent-env-tools\scripts\agent-env-tools.ps1 in report-only mode, check all required and optional tools, and summarize what is found, missing, unusable, and what to install next.`

## Notes

- Keep this README as a lightweight index.
- Keep executable workflow behavior in `SKILL.md` and `scripts/agent-env-tools.ps1`.
