---
id: ADR-0001
title: Keep the good fixture decision
status: accepted
date: 2026-01-02
area: backend
related-prds: [PRD-001]
---

# ADR-0001: Keep the good fixture decision

**Summary:** In the context of a fixture, facing a checker that must accept a well-formed record, we
decided to keep one, to prove the passing path, accepting nothing.

## Context and Problem Statement

A checker is only trustworthy if it accepts what is correct.

## Considered Options

1. No fixture
2. One valid record

## Decision Outcome

Chosen option: 2.

### Consequences

- Good, because the passing path is proved.
- Bad, because the fixture must be maintained.

### Confirmation

- Test: `docs-check.Tests.ps1` asserts this set produces no errors.