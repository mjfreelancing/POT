# CI/CD Pipeline and Delivery Policies

Feature ID: 0034
Status: Proposed
Date created: 2026-11-06
Scope: Placeholder for discovery. This document records why continuous integration and delivery are now worth deciding, and the questions discovery must settle. It proposes no workflow, deployment change or policy; a full `prd-discovery` cycle produces the requirements when this is picked up.
Audience: Future discovery and implementation agent(s), reviewers, and maintainers.

## Objective

Decide what POT's build, test and release process should be now that the project has grown past what a
single developer can check by habit. That includes what runs automatically, what blocks a merge or a
deployment, and how a deployed version is identified and rolled back.

## Problem To Solve

Builds, tests and deployments are run by hand
([ADR-0027](../adr/0027-builds-and-deployments-are-run-by-hand.md)). Two VS Code tasks build the server
and client images from the local working tree and push them as `:latest`. No test, lint or contract check
runs first, and the running revision is switched by hand in the Azure portal. So:

- uncommitted or failing code can reach production;
- no record shows which commit is deployed;
- `:latest` leaves no earlier image to roll back to;
- every check that should guard `main` depends on someone remembering to run it.

The project has grown more complex: about 50 ADRs, a server integration suite on Testcontainers, a
four-project browser matrix with a production-build profile, a committed OpenAPI contract, and a
documentation check. Several open items assume a pipeline exists to run their checks.

The decision brief that raised this PRD (DD-05, answered 2026-11-06) chose option B as a starting
direction. The owner has no fixed view beyond that. Whether delivery is automated too, and which policies
apply, are for discovery. The brief's options were:

- **A. Keep as-is.** Not chosen.
- **B. A build-and-test workflow only.** It runs on pull requests and pushes to `main`:
  - `dotnet build` and `dotnet test`;
  - client `lint`, `type:check` and Vitest;
  - a check that `contracts/openapi/v1.json` is unchanged after the build;
  - `docs-check`.

  Browser suites stay local and deployment is unchanged. Integration tests use Testcontainers, which
  work on GitHub's Ubuntu runners.
- **C. B, plus image build and deployment from `main`.** Images are tagged with the commit SHA and
  deployed with `az containerapp update`, using OIDC federated credentials.
- **Interim suggestion:** tag images with the commit SHA as well as `:latest`, which gives a rollback
  point before any pipeline exists.

## What Is Known

| Area                 | Fact                                                                                                                                       | Source                                                   |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------- |
| Pipelines            | None. `.github/workflows/` exists locally but is empty and untracked                                                                       | Repository                                               |
| Image build and push | `azure-server-build-and-deploy` and `azure-client-build-and-deploy`: `docker build --no-cache` from the working tree, push `:latest` to GHCR | `.vscode/tasks.json`                                     |
| Client build inputs  | The API base URL, timeout and nginx config are baked in as build arguments                                                                 | `.vscode/tasks.json`; `Source/Docker/Client/Dockerfile`  |
| Deployment           | The Container Apps revision is switched by hand in the portal                                                                              | ADR-0027; ADR-0028                                       |
| Migrations           | The server image runs the migration runner at start-up, then the API                                                                       | `Source/Docker/Server/entrypoint.sh`; ADR-0008           |
| Server checks        | `dotnet build` and `dotnet test pot.sln`; integration tests need Docker (Testcontainers)                                                   | `.github/copilot-instructions.md`                        |
| Client checks        | `lint`, `type:check`, `test` (Vitest) and `build`                                                                                          | `Source/Client/pot-react/package.json`                   |
| Browser suites       | Dev matrix and production-build matrix, both run by hand; the production-build run gates client-changing PRDs                              | ADR-0038                                                 |
| Contract             | The OpenAPI document is generated at build time and committed; nothing checks that it is current                                           | ADR-0045                                                 |
| Documentation        | `docs-check` validates ADRs, links and citations; it is run by hand                                                                        | `.github/scripts/docs-check/`                            |
| Secrets              | Secret values live outside the repository, in a platform store                                                                             | ADR-0022                                                 |

## Questions Discovery Must Answer

**Integration**

1. Which events trigger a run: pull requests, pushes to `main`, other branches, tags, a schedule?
2. Which checks run, and which of them block a merge? Candidates include server build and tests, client
   lint, type check, unit tests and build, the OpenAPI drift check, `docs-check`, and the dev or
   production-build browser suites.
3. Does the integration suite run on hosted runners with Docker, and what run time is acceptable?
4. Is `main` protected, and is a pull request with passing checks required to change it?

**Delivery**

5. Is delivery automated, and if so, how far: image build only, publish, or deploy too?
6. How is an image identified (commit SHA, version, tag), and what replaces `:latest`?
7. What is a release, and how is it triggered: every merge to `main`, a tag, or a manual approval?
8. How is a deployment rolled back, and how does that interact with migrations that run at start-up?
9. Does deployment need a staging or pre-production environment?
10. How does the pipeline authenticate to Azure and GHCR (for example OIDC federated credentials, or
    repository secrets)?

**Policies**

11. Which dependency and security checks are wanted (dependency updates, vulnerability scanning, secret
    scanning)?
12. Which existing decisions or open items should be revisited once a pipeline exists? Candidates:
    ADR-0027, ADR-0038's production-build gate, DD-06 (database version alignment) and DD-07 (generated
    client types).
13. What is the budget for hosted-runner minutes and any paid services?

## Out Of Scope

- Infrastructure as code for the Azure environment, unless discovery decides it is a prerequisite.
- Changing what the test suites assert. This PRD decides where and when they run.

## Related Documents

- Feature index: [Docs/Future/README.md](README.md)
- Current process: [ADR-0027](../adr/0027-builds-and-deployments-are-run-by-hand.md); production topology:
  [ADR-0028](../adr/0028-production-runs-two-container-apps.md)
- Build-time code-style enforcement, which a pipeline would run:
  [PRD-0029](PRD-0029-build-time-code-style-enforcement.md)
- Liveness and readiness probes, which an automated deployment depends on:
  [PRD-0033](PRD-0033-liveness-readiness-probe-audit.md)
