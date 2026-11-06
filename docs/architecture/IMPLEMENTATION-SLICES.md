# Architecture delivery plan for Gemini — proposed sequence

**Status: PLAN ONLY.** The Owner granted **automatic merge permission for reviewed Markdown-only PRs**, not application code or database schema migrations. Code implementation needs specific authorized GitHub Issues and independent ChatGPT review.

## Stage 0 — documentation baseline (this PR)
- Review ERD, class diagram, sequence diagrams, state machines and deployment proposal.
- Reconcile synonyms used in earlier specs: `Employees` = `EmploymentRelationship`; `FinancialRelationship/Payee` = `PayeeRelationship`; `EmployeeFinancialFacts` = `ApprovedFinancialFact` on a payee relationship.
- Diagram names express **domain ownership** and candidate tables, not a commitment that every box must be a table/class in generated code.
- Record unresolved architecture/business decisions and freeze a versioned blueprint. **No migrations.**

## Stage 1 — source-oriented minimum vertical slice
Implementation tasks only after Owner authorizes Issue scope and finalizes adapter contracts:
1. Secure original source PDF archive and immutable document metadata.
2. Parser adapter and **source financial occurrences**, repeated-code preservation and Arabic normalization.
3. Per-type report validators for payslip / cover / payment sheet; source group, gross/deduction/net, identifiers and signs.
4. Reviewer preview / fail-close / mapping gates and synthetic regression fixtures.
5. Review against `OraclePdfParser` best-existing code, plus `PDF-EXTRACTOR` physical table techniques. Refer to [Issue #6](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/6) and its 25-task plan.
6. Owner-authorized private verification on diverse genuine PDF layouts. Do not claim completed after green CI alone.

## Stage 2 — canonical data and reviewer accounts
- Minimal organization, person/identity, employment/payee relationship, group reviewer assignment and RBAC.
- Approved financial fact import with reviewer evidence; historical grade, status and 30-06-2015 reference facts.
- Prevent importing Oracle data into canonical employment profiles.
- API audit logs, case versioning, concurrent review and review status per employee/group/report.

## Stage 3 — audit case and correction workflow
- Independent expected-vs-source workbench, finding status, reviewed amount overlays, source-trusted items and same-chapter aggregation exceptions.
- Completed examination versus correction pending, before-approval and post-approval amendments.
- Compare source PDF revised versions without overwriting prior approval.

## Stage 4 — verified rule proposals and temporal events
- Legal source catalog/source verification and effective-dated rule versions.
- Grade/allowance/promotion/part-month changes proposed by engine; reviewer approves/modifies with evidence.
- **No automatic general insurance recalculation**. Narrow part-time insured-base exception after original legal applicability verified for category.
- Fiscal/tax capabilities only after Owner separately scopes them; do not generalize initial old docs.

## Stage 5 — retroactive and payment proof tracking
- Retro claim by original component and period; official reported movement, reviewer allocation and independent settlement evidence.
- No automatic assumption about payroll month, duplicate prior payment or bank completion.

## Stage 6 — reporting, hardening and acceptance
- RTL Excel and source-side exports; performance with synthetic large files, replay determinism, encrypted restore tests, SOD/RBAC checks and audit reports.
- Authoritative migrations and class/ERD synchronization once schema approved and application code exists.

## Design acceptance gates
- Every canonical entity has responsible context, keys, cardinality, lifecycle/immutable rules and privacy tier.
- Every financial amount is **source/expected/reviewed/corrected/reported/paid** with clear identity and status.
- Every row in original Oracle document has independent provenance and one or more proposed financial mappings; never merge by code.
- Dates have correct effective and recorded semantics; new decisions do not retroactively rewrite historical approvals.
- Each reviewer action has logged actor, evidence, permission and concurrency guard.
- Each API workflow has at least one positive, one missing-evidence, one repeated-code/Arabic and one stale-update test where relevant.
- SQL design review resolves same person multiple contracts, multiple payroll cycles, ambiguous source recipients and legitimate duplicated Oracle rows.

## Decisions that must precede specific code
1. **Tech stack**: approve .NET/Angular/SQL Server or alternative, and supported versions.
2. **Physical ERD**: schema ownership, FK nullability, delete semantics, transaction boundaries, concurrency fields, performance constraints.
3. **Person/payee matching policy**: what exactly constitutes authoritative evidence across payroll sources and campus institutions.
4. **Official Oracle report variants**: complete sample types and source field identity per document.
5. **Financial review permissions**: who approves review, permitted exception classes, who may reopen after approval and how request to stop payment is handled externally.
6. **Partial-work pension base**: primary legal source, workforce categories and official evidence of centrally applied insured wage base.
7. **Approved legal calculation scope**, source versions and controls (do not infer from secondary summaries).
8. **Source group mapping**: reporting categories, bank/branch/group and permitted one-to-many case matching.
9. **Deployment security**: internal auth model, private original document archive, retention, secrets and encryption, monitoring, backups.
10. **Volume and environment**: number of payees/cycles/documents, annual history size, network and authorized test dataset.

## No-rework rule for Gemini
Before creating any entity class or migration: quote the exact blueprint/ADR, write table/class ownership and dependencies, list invariants/test cases, and wait for the specific authorized implementation Issue. Unverified diagrams must not be treated as approved database requirements.
