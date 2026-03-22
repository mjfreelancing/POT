---
name: docker_workflow
description: Run Docker lifecycle workflows with task-first defaults.
---

Handle Docker workflows using the safest, most repeatable repository path.

## Scope

- Handle build, start, stop, status, and health-check workflows for the existing stack.

## Workflow

- Prefer repository task or command wrappers over ad-hoc shell sequences.
- Validate health and status after lifecycle changes.

## Commands and Rules

- Prefer VS Code tasks or documented wrapper commands first.
- Use the repository's compose file or orchestration manifests when direct commands are required.
- Include required environment files or flags when repository docs specify them.
- Never modify persisted data folders or volumes unless the user explicitly asks.

## Validation

- Confirm expected services or containers are running.
- Confirm documented health or status endpoints respond when available.

If a step fails, summarize the root error and provide the smallest next corrective step.

## Expansion Notes

- Add repository-specific task names, compose paths, endpoints, and safety boundaries in consuming copies.
- Keep service-specific safety rules aligned with any Docker instruction file used by the repository.
  - POT: Start task is `docker-start-pot-client-server`; stop task is `docker-stop-pot-client-server`.
  - POT: Compose file is `Source/Docker/docker-compose-client-server.yml`.
  - POT: Direct compose commands should include `--env-file .env --env-file .env.development`.
  - POT: Protected data path is `Source/Docker/postgres-data/**`.
  - POT: Expected services are `pot-react`, `pot-aspnet`, and `pot-postgres`.
  - POT: Health endpoints are `http://localhost:5241/_health` and `http://localhost:5175/health`.
