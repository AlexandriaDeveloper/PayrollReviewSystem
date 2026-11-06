# Architecture baseline (proposal, no code yet)

**Diagrammed design:** [Architecture Blueprint v0.1](BLUEPRINT-V1.md) with ERD, UML class/sequence/state and deployment views. The blueprint is conceptual until the Owner approves specific physical schema and tech-stack ADRs.

## Mission and boundaries
The system independently reconstructs lawful expected payroll for each person/employment/entitlement period from approved personnel, reference, event and legal evidence, then compares with central payroll source, records review decisions and monitors whether corrections/arrears are subsequently shown and paid.

**Out of scope:** replacing GFMIS/Oracle, posting disbursements, altering central entitlement records, automatically recovering overpayments, and declaring legal violations without human review.

## Logical modules
1. **Source Intake & Evidence**: archive original files and versions, extract, validate, page/row evidence; import intent and quality.
2. **People & Employment**: person identities, multiple employment relations, grades, departments, appointments, historical status.
3. **Financial Catalog & Facts**: canonical components, account mapping, 30-Jun-2015 and other historical reference bases, approved amounts.
4. **Legal Rules & Administrative Events**: versioned eligibility/calculation rules, orders, promotions, leave, disciplinary, court rulings, effective date.
5. **Expected Payroll Engine**: dependency-aware per-period calculations, snapshots, detailed traces.
6. **Reconciliation & Review**: comparison, ambiguity management, reviewer adjustments, documents, findings, decisions.
7. **Arrears & Follow-up**: retro entitlements, allocation of partial disbursements, over/under-payment tracking, no payment execution.
8. **Centrally Calculated Items / Narrow Insurance Exception**: preserve pension and Owner-named funds as source-trusted amounts, not independently recalculated; review only evidenced part-time insured-wage base errors under verified applicable rules. Tax/remittance controls remain separate future scoping decisions.
9. **Reports & Administration**: dashboard, audit workpapers, RBAC, immutable audit trail, exports.

These are **logical boundaries**, not a mandatory microservice count or DB schema list.

## Ports / anti-corruption layers
Import adapters for Oracle payslips, cover sheets, payment sheets, spreadsheets and future authorized sources. Translators produce canonical **source facts** with provenance, never internal approved facts. A rules engine may consume approved source facts only after identity and evidence gates.

## Processing flow
Source file -> hash/archive -> classify -> parse -> validate report type/month/identity/gross/deductions/net -> reviewer intake decisions -> canonical source observations -> independently calculate expected snapshot -> compare -> finding -> reviewer decision -> official correction or follow-up -> evidence-based settlement/closure.

At each step preserve originals and trace transformations; do not make one-time import the canonical truth for legal grade or salary.

## Storage guidance
Proposed SQL Server relational transactional core with separate object/archive storage for originals. Append-only source and approval journals, versioned financial facts, employer/payment evidence and reproducible calculation snapshots. No cross-source update to originals.

## Calculation engine boundary
No arbitrary executable expressions from users or AI. Typed, validated operators with explicit eligibility, reference base, date basis, rounding and safe ordering. Detect dependency cycles and missing inputs.

## Quality attributes
Auditability > convenience; correctness > silent fallback; Arabic RTL first; availability without external Oracle runtime; deterministic financial arithmetic; secure on-prem/private deployment preference; privacy by default; testable before any salary data is processed.

## Candidate technologies (NOT approved stack)
.NET/ASP.NET Core + Angular RTL + SQL Server, modular monolith. Keep final selection and version in ADR and a scoped Issue before coding. Existing legacy implementation is not a dependency.
