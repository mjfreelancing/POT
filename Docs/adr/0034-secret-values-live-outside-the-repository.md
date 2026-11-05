---
id: ADR-0034
title: Secret values live outside the repository, in a tracked template and a platform store
status: accepted
date: 2025-09-12
area: infrastructure
---

# ADR-0034 (backfilled): Secret values live outside the repository, in a tracked template and a platform store

**Summary:** In the context of an application that needs database credentials, a signing key and mail
credentials to start, facing the choice between committing usable values and committing only their
names, we decided that the repository holds a tracked template with no secrets and that the working file
and the deployed values live outside it, in an ignored file locally and in the platform's secret store in
production, to achieve a clone that contains no credential, accepting that a new environment must be
supplied with values before it can start.

## Context and Problem Statement

A committed value is permanent in a way a rotated value is not: it is in the history of every clone,
including the ones that were already taken. A configuration that cannot start without a value makes that
pressure real, which is why the tempting answer is a working default with a real credential in it.

The application already validates required settings at startup, so a missing value is a failed start
rather than a silent misconfiguration.

## Decision Drivers

- Nothing that grants access should be in the repository, including its history.
- A new developer should know which values are needed without being given them.
- A deployed environment should take its values from a store that can rotate them, not from the image.

## Considered Options

1. A tracked template of keys with placeholders, an ignored working file, and the platform's secret store
   in production.
2. Commit working development values, with production overridden at deployment.
3. No template; document the keys in prose.
4. Encrypt the values in the repository with a shared key.

## Decision Outcome

Chosen option: 1. The repository holds `Source/Docker/.env.development.template`: every per-developer
key the compose file substitutes, with startup-safe values where a local default is harmless and marked
placeholders where a value must be supplied. Copying it produces a working local stack; the copy is
ignored (`.gitignore` ignores `*.development*` and `*.production*`, with the template as the one
exception). The two non-secret keys, `COMPOSE_PROJECT_NAME` and `POSTGRES_DB`, are in the tracked `.env`.
The API run outside Docker reads its local values from `appsettings.Development.json`, which the same
rule ignores; no template is tracked for it.

Production values come from the platform's secret store (Key Vault, read through a managed identity, in
`Docs/Azure/POT-Azure-Diagram.mmd`) and reach the container as environment settings, so the image
contains no credential and a rotated value does not need a rebuild. The API has no Key Vault client of its
own; how each value is mapped is container-app configuration that the repository does not record.

### Consequences

- Good, because a clone contains no credential, and a leaked repository is not a leaked environment.
- Good, because the key list is versioned, so adding a required setting is visible in review and a
  developer learns what exists without being handed the values.
- Good, because production values can be rotated without rebuilding an image.
- Bad, because a fresh environment cannot start until its values are supplied, which is a step somebody
  must perform.
- Bad, because the template is one more file to keep in step with the settings the application reads.
- Bad, because placeholders that satisfy validation can make a stack that starts and then fails at the
  first mail, which is correct but easy to mistake for working.

### Confirmation

- Review: a real credential in a tracked file, and a template that contains one, are both defects.
- Mechanism: `.gitignore` ignores `*.development*` and `*.production*` and re-includes only
  `Source/Docker/.env.development.template`, so an ordinary `git add` does not pick up a working copy.
- Test: `OptionsStartupValidationFixture` asserts that missing or malformed required settings fail startup,
  which is what makes an unsupplied value a failed start rather than a silent default.
- Gap: nothing scans tracked files for credentials, and no test checks that a copy of the template starts
  a stack; both are kept by review.

## Pros and Cons of the Options

### Template, ignored file, secret store (chosen)

- Good: nothing sensitive tracked, and values rotatable.
- Bad: a setup step and a file to maintain.

### Committed development values

- Good: a clone simply runs.
- Bad: the values are in history forever, and the habit spreads to production values.

### Documented keys only

- Good: no file to maintain.
- Bad: the documentation drifts from the settings the code reads, and a typo is discovered at runtime.

### Encrypted values in the repository

- Good: versioned and secret.
- Bad: a second secret to distribute, and re-encryption on every rotation.

## More Information

- Which settings are required at startup is decided by each settings type's validation, not by this
  record.
- The deployment that consumes the secret store is
  [ADR-0036](0036-production-runs-two-container-apps.md).
