---
id: ADR-0035
title: Builds, tests and deployments are run by hand; there is no continuous integration
status: accepted
date: 2026-11-04
area: process
---

# ADR-0035: Builds, tests and deployments are run by hand; there is no continuous integration

**Summary:** In the context of a personal project with one developer and a small hosted deployment,
facing the choice between a pipeline and a set of editor tasks, we decided that builds, tests, image
publication and deployment are run by hand from tasks defined with the project, to achieve no
infrastructure cost and no pipeline to maintain, accepting that nothing enforces the tests before a
deployment and that the deployed version is whatever the last local build produced.

## Context and Problem Statement

A pipeline's value is that it runs the checks a person might skip, on a machine that is not theirs, and
produces an artefact that is traceable to a commit. Its cost is a service to configure, keep authorised
and pay for, and a class of failures that only exist in the pipeline.

For a single developer with a small deployment, the checks can be the same commands the pipeline would
run, and the temptation to skip them is a personal discipline problem rather than a process guarantee.

Cost is the deciding factor at this stage: a hosted pipeline and its minutes are a recurring cost for a
project whose deployment is a container app on a consumption plan.

## Decision Drivers

- No recurring cost for build infrastructure on a personal project.
- One developer, so no coordination problem a pipeline would solve.
- The same commands must be runnable locally exactly as they are run for a deployment.

## Considered Options

1. Editor tasks for build, test, image publication and deployment, run by hand.
2. A hosted pipeline that builds, tests and publishes on every push.
3. A pipeline that only publishes on a tag, with tests run locally.
4. A self-hosted runner.

## Decision Outcome

Chosen option: 1. Tasks defined with the project build the images, publish them to the registry and start
the stacks; tests are the same commands a pipeline would invoke. A deployment publishes the `latest` tag
built from the working tree.

Nothing gates a deployment on the tests passing. A published image is what the developer's machine
produced, from whatever the working tree contained.

### Consequences

- Good, because there is no build infrastructure to pay for, authorise or maintain for a small deployment.
- Good, because the commands are the same ones used locally, so a deployment failure is reproducible on a
  developer machine.
- Good, because a deployment takes one task rather than a commit and a wait.
- Bad, because nothing enforces that the tests ran, or passed, before an image was published.
- Bad, because an image cannot be traced to a commit: `latest` is whatever was built last, and the working
  tree may have contained anything.
- Bad, because a rollback means rebuilding from a remembered state rather than deploying an earlier
  artefact.

### Confirmation

- Review: a deployment is preceded by the test suites being run, and that remains a personal discipline
  rather than a check.
- Known gap: no artefact identifies the commit it was built from, so provenance depends on the developer's
  record.

## Pros and Cons of the Options

### Tasks by hand (chosen)

- Good: no cost and nothing to maintain.
- Bad: no enforcement, and no traceable artefact.

### Pipeline on every push

- Good: tests run whether or not anyone remembers, and artefacts carry provenance.
- Bad: a service to configure and pay for, and a second place where the build can fail.

### Publish on a tag only

- Good: cheaper than every push, and tagged artefacts are traceable.
- Bad: still a pipeline to maintain, and the tests that matter are the ones run before the tag.

### Self-hosted runner

- Good: no per-minute cost.
- Bad: a machine to keep patched and available, which is more work than the pipeline it replaces.

## More Information

- This decision is expected to be revisited as the product grows; a pipeline would then be a change to
  this record rather than an addition beside it.
- The deployment target and its topology are separate decisions.
