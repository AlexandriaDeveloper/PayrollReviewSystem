# ADR-002 — Temporal reference facts versus payable components

- **Status:** PROPOSED — needs Owner confirmation before implementation.
- **Date:** 2026-11-06

## Problem
Frozen base (e.g. 30-06-2015), grades, salary assignments, allowance bases, employer contributions and display totals must coexist without double counting or rewriting historical salary data.

## Proposal
Store canonical immutable/versioned reference facts separately from payable/withheld components and computation snapshots. Use validity dates and knowledge/record dates. Link every calculated component to eligible rule, employee grade and financial input reference. Store external payroll values as source observations. Link arrears to component+entitlement period+event/calc version and verified later settlement allocations.

## Alternatives
One flat employee salary table: simpler but destroys history/role distinction. One row per component per month as mutable truth: quick reports but difficult provenance and incorrect backdated handling.

## Consequences
More entities and calculations; correct historical replay, audit traceability and safe correction. Specific table shapes remain conceptual until scoped schema Issue and reviewed ERD.
