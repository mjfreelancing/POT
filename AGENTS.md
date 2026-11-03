# AGENTS.md

This file is a pointer, not a rule book. The rules live in the files listed here so that each has one
home and none of them is restated.

| For                                                            | Read                                                               |
| -------------------------------------------------------------- | ------------------------------------------------------------------ |
| The working agreement, code style and commands                 | [.github/copilot-instructions.md](.github/copilot-instructions.md) |
| Rules for one area of the code, applied by path                | [.github/instructions/](.github/instructions/)                     |
| Multi-step workflows (PRDs, ADRs, tests, documentation)        | [.github/skills/](.github/skills/)                                 |
| Utility scripts, including the documentation check             | [.github/scripts/README.md](.github/scripts/README.md)             |
| Where any kind of document belongs                             | [Docs/README.md](Docs/README.md)                                   |
| Why the system is shaped as it is, and what is already decided | [Docs/adr/README.md](Docs/adr/README.md)                           |
| What a feature must do, and why (PRDs)                         | [Docs/Future/README.md](Docs/Future/README.md)                     |
| Binding coding conventions                                     | [Docs/standards/](Docs/standards/)                                 |

Before an architectural change, read the ADR guide and do not contradict an accepted decision without
asking. Run `docs-check` before offering a documentation change for review:

```powershell
pwsh ./.github/scripts/docs-check/docs-check.ps1
```
