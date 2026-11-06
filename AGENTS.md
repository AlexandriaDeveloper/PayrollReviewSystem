# Mandatory instructions for every AI agent and developer

Read this document before analyzing, designing or changing this repository. These rules are repository governance, not a substitute for the Owner's latest explicit decisions.

## Authority and source order
1. Owner's latest explicit, recorded business decision.
2. Reviewed ADRs and approved GitHub Issue acceptance criteria.
3. Domain/architecture specifications under docs.
4. Source documents **after** their authenticity, applicability and version are confirmed.
5. Legacy repositories: discovery only; no implicit inheritance.

If documents conflict, identify the conflict in the assigned Issue. Do not silently invent a rule, overwrite an existing decision, or implement a disputed policy. Proposed changes to approved boundaries require an ADR and Owner approval.

## Agent roles and GitHub communication
- **ChatGPT:** owns system/domain architecture, proposes tasks, reviews PR diffs and risks, records review comments. May not claim a PR was reviewed unless the actual changes were inspected.
- **Gemini:** implements **only** the scope of the explicitly assigned Issue; documents tests, source changes, assumptions, risks and security impact in its PR.
- **Owner:** approves business/legal assumptions and scope, explicitly authorizes migrations/production changes and merges.
- **GitHub Issues/PRs** are the asynchronous handoff surface, not a hidden autonomous link between AI models. ChatGPT must be explicitly invoked or separately scheduled to check PRs; an @mention alone does not guarantee execution.

## Hard invariants
1. This is a **review** system, not a salary disbursement or Oracle modification system.
2. Treat source PDFs/extracted rows as immutable evidence with file hash, provenance and page/row references; corrections form separate records.
3. Person != employment relationship != payee/financial relationship.
4. Historical entitlement uses the correct legal status, grade, source amounts and rule versions **at the entitlement date**, not current profile values.
5. Keep financial reference/base figures separate from cash earnings, deductions, employer costs and display totals.
6. Gross payroll matching does not prove entitlement; payment-sheet inclusion does not by itself prove bank settlement.
7. Missing != 0; null/unknown statuses must be explicit. Never mark ambiguous extraction Verified.
8. No automatic change to employee master data from an external payroll file, even during finance-only imports.
9. No hidden default grade, salary, frozen 2015 base, rate, amount, due date or law classification.
10. No retroactive deduction, repayment, payment approval or legal violation determination made automatically.
11. Each item calculation must be explainable: inputs, dates, rules, source references, rounding and dependent items.
12. Reconciliation differences and reviewer overrides never mutate historical Oracle source facts or approved snapshots.
13. Financial math uses decimal-compatible fixed precision and explicit rounding and proration policies per rule.
14. Approval of a full employee review must be blocked where required evidence or unresolved material differences remain; a reviewer can finish examination without closing a correction or payment.
15. Store both entitlement-period and disbursement-period concepts. Post-period adjustments must be allocated to underlying periods/components when proven; otherwise mark allocation unresolved.
16. No real national IDs, payslips, bank account numbers, connection strings, API keys, unredacted logs or private legal records in public GitHub.

## Oracle PDF importer gate
For any importer/parser work, first read docs/reference/ORACLEPDFPARSER-CODE-STUDY.md and docs/specs/ORACLE-PDF-ADAPTER-DESIGN.md. OraclePdfParser is a high-value proven *starting point for code discovery*, not an automatically accepted library or proof that extracted money is correct. Treat source types as payslip, cover and payment sheet with report-specific validation. Do not implement or copy parser code until Owner explicitly authorizes the scoped issue.

## Markdown-only merge delegation
The Owner authorized the ChatGPT architect to merge **reviewed Markdown-only changes** without seeking separate merge approval; see docs/governance/COLLABORATION.md. This does **not** authorize Gemini to self-merge, or any application code/configuration/schema/deployment changes.

## Before doing any change
- Read docs/README.md and only the relevant domain specs; inspect main HEAD, current branch and authorized Issue.
- State what exists, what is planned, exact change scope, acceptance cases and any open legal/business questions.
- No implementation while the assigned Issue is design-only or lacks authorization.
- Prefer incremental reversible changes; do not break previously working functionality to implement a new feature.
- Keep modules decoupled, migrations reversible/assessed, and schema changes explicitly described.
- Do not modify unrelated repositories, GitHub settings, deployment resources or production data.

## Evidence required in every implementation PR
- Issue/decision references; behavior summary and affected modules.
- Passing commands and exact test results, or explicit reason tests could not run.
- Synthetic regression fixtures with boundary dates, negative amounts, missing data, Arabic RTL and exceptional cases.
- Security/privacy impact and migration impact.
- Before/after examples with source evidence, no sensitive identifiers.
- Outstanding questions or architectural deviations, clearly marked.

## Stop / escalate
Stop affected work and open a question for: unverifiable legal formula; inconsistent source identities; unreconciled monetary totals; uncertain payee or reference basis; unapproved architecture change; potentially destructive migration; unexpected sensitive data; or attempts to bypass reviewer approval.
