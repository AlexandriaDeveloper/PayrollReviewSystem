# Suggested delivery roadmap — NOT automatic authorization

## Phase 0 — design/governance (this PR)
- Agent and GitHub handoff rules, domain glossary, requirements/acceptance, reference status.
- Owner reviews key open questions in decision register and accepts or requests changes.
- No application code or database changes.

## Phase 1 — data & evidence correctness
- Define canonical identity and temporal schema via ADR.
- Define source import contracts and financial component classifications.
- Read-only Oracle adapter(s) and staging, page evidence, report classification.
- Synthetic tests; privately validate diverse actual report layouts (never commit private originals).
- Arabic reviewer import preview and blocking issue list.

## Phase 2 — review workbench
- Expected versus source line comparison, document requests, reviewer overlays and immutable audit trail.
- Approval gate at employee and case levels; no central source modifications.
- Cross-file reconciliation including groups/periods/bank evidence.

## Phase 3 — rule-based expected payroll
- Approved legal rule catalogue and explicit effective dates; calculation input snapshots.
- Grade, periodic allowance, fixed/historical reference cases and dependency graph.
- No active rule until legal basis and tests verified.

## Phase 4 — promotion/retroactive
- Promotion event dates, re-run of affected historical component periods.
- Retro-claim ledger, partial/unallocated future reports, independently evidenced settlement.
- Tax/insurance impacts where rules and documents support them.

## Phase 5 — controls and analytics
- Review risk flags, trends, reconciliation to remittances/budget lines, reports and audit dashboards.
- Broader legal categories, integrations only when authorized.

## Every phase: delivery requirements
Issue scope -> tests/acceptance -> documented PR -> independent architect review -> Owner approval -> merge. No silent refactor of earlier phases. Nothing is 'production ready' without representative and authorized UAT.
