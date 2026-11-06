# PayrollReviewSystem — Independent Government Payroll Audit

**Status: DESIGN BASELINE / NO APPLICATION CODE AUTHORIZED.**
**Created for Alexandria University payroll audit use cases.**
**Repository may be public: never commit real employee, financial, identity or legal-source files without permission and sanitization.**

## Mission
An independent, evidence-led review system. It reconstructs expected amounts from verified personnel history, financial facts, administrative decisions and effective-dated rules; compares them with immutable Oracle/GFMIS outputs; manages findings, reviewer decisions, corrections and retroactive balances. It is **not** a replacement for central entitlements or a payment execution system.

## Roles
- **Owner:** business-policy authority, priorities, scope, approvals and release decisions.
- **ChatGPT:** software architect, financial-audit/domain designer and code/quality reviewer.
- **Gemini (coding agent):** implementer working only on explicitly authorized GitHub issues.
- **GitHub:** durable collaboration source via Issues, PRs, code and Markdown decisions.

## Start here
1. [AGENTS.md](AGENTS.md) — mandatory agent rules.
2. [GEMINI.md](GEMINI.md) — coding agent workflow.
3. [Documentation map](docs/README.md) — specifications and source status.
4. [Collaboration protocol](docs/governance/COLLABORATION.md) — how agents exchange work via GitHub.
5. [Architecture](docs/architecture/ARCHITECTURE.md) and [domain model](docs/architecture/DOMAIN-AND-DATA-MODEL.md).
6. [Audit rules](docs/domain/AUDIT-FINDINGS-AND-DECISIONS.md) and [retroactive payments](docs/domain/TEMPORAL-AND-RETROACTIVE.md).

## Firm principles
- Imported Oracle records are external evidence, never automatically trusted as the lawful entitlement or allowed to overwrite the canonical employee profile.
- Distinguish **expected**, **appearing in payroll**, **reviewed**, **corrected by central entitlements**, and **actually paid**.
- Distinguish financial **reference bases** (e.g., 30 June 2015 frozen base), calculated aggregates and employee cash entitlements. Never double-pay a reference base.
- A promotion does not prove its arrears will be paid in the current month. Calculate each component and entitlement period, allocate independently proven payments and preserve unsettled balances.
- Unknown, ambiguous, missing or unverified facts must remain review-required, not become fabricated zero or silent approval.
- No legal rate/entitlement is accepted without authoritative source, effective date, covered employee class and verification.

## Scope of this baseline
**Documentation only.** No chosen implementation framework, executable code, database migration, deployment or merge to main is approved by this documentation set. Prior repositories are **references, not dependencies**.

## Traceability
All new work: GitHub Issue -> approved acceptance criteria -> isolated branch -> PR with tests/evidence -> architect review -> owner merge/release decision.
