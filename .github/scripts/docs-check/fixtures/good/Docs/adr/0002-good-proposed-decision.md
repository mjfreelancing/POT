---
id: ADR-0002
title: Keep a proposed fixture decision
status: proposed
date: 2026-01-02
area: process
---

# ADR-0002: Keep a proposed fixture decision

**Summary:** In the context of a fixture, facing a checker that must accept a record that is not yet
accepted, we decided to keep one, to prove that an accepted record can still be cited, accepting nothing.

## Context and Problem Statement

A proposed record is valid even though no instruction file may cite it yet.

## Considered Options

1. No proposed fixture
2. One proposed record

## Decision Outcome

Chosen option: 2.

### Consequences

- Good, because the status rule is proved.
- Bad, because the fixture must be maintained.

### Confirmation

- Test: the citation check cites ADR-0001, which is accepted.
