# ADR-001 — Separate review system, non-mutating central source

- **Status:** DESIGN RECORD — Owner confirmed independent repo and review-only goal; implementation details pending.
- **Date:** 2026-11-06

## Context
Government central entitlements and Oracle/GFMIS provide observed payroll records. Review must independently determine expected amounts and preserve external evidence, rather than recalculate from a payroll system's own potentially erroneous figures.

## Decision boundary
This repository builds a separate independent payroll review application. It must not alter central payroll, perform disbursement, issue automatic employee recovery or replace payroll provider decisions. Imported source is read-only and archived with provenance; reviewer adjustment is separate from original. Financial truth is not inferred merely because a document reconciles arithmetically.

## Consequences
Requires independent employee histories, legal/rule catalogue and evidence for actual payment. Complex date and audit needs are intentional. Existing legacy repositories serve as evidence and engineering research only; no direct migration or reuse is authorized by this ADR.

## Implementation
Only following approved Issues/stack decisions. Not a code instruction by itself.
