# Documentation map and current authority

**Baseline date: 2026-11-06. Documentation-only repository.**

| Read order | File | Purpose |
| --- | --- | --- |
| 1 | ../AGENTS.md, ../GEMINI.md | Mandatory agent conduct |
| 2 | governance/COLLABORATION.md | GitHub Issue/PR handoff protocol |
| 3 | governance/REVIEW-GATES.md | What blocks code/merge |
| 4 | governance/DECISION-REGISTER.md | Approved versus proposed choices |
| 5 | architecture/ARCHITECTURE.md | System and integration boundaries |
| 6 | architecture/DOMAIN-AND-DATA-MODEL.md | Canonical conceptual schema |
| 7 | domain/FINANCIAL-COMPONENTS-AND-FACTS.md | Paid, reference and non-cash data |
| 8 | domain/TEMPORAL-AND-RETROACTIVE.md | Effective date, promotions, arrears |
| 9 | domain/AUDIT-FINDINGS-AND-DECISIONS.md | Review options and closure |
| 10 | domain/ORACLE-IMPORT-AND-RECONCILIATION.md | PDF quality, matching, source evidence |
| 11 | domain/LEGAL-AND-RULE-GOVERNANCE.md | How to verify legal rules |
| 12 | specs/REQUIREMENTS-AND-ACCEPTANCE.md | MVP and testable requirements |
| 13 | process/ROADMAP.md | Proposed implementation sequence |
| 14 | reference/RESEARCH-INVENTORY-STATUS.md | What is / is not evidenced |
| 15 | security/SECURITY-AND-PRIVACY.md | Personal data and audit security |
| 16 | ui/REVIEWER-UX.md | Arabic RTL reviewer workflow |

## Document states
- **REQUIREMENT CONFIRMED** — explicitly agreed goal or invariant in Owner conversation.
- **DESIGN PROPOSAL** — architect recommendation awaiting Owner approval to become binding detail.
- **LEGAL SOURCE UNVERIFIED** — facts inferred from secondary notes or incomplete primary review; cannot power a live rule.
- **IMPLEMENTATION AUTHORIZED** — explicit Issue scope/Owner authorization only.
- **VERIFIED IN CODE** — real PR code inspected with passing test evidence.

Historical reference repos: Payroll-Auditor, Py-Ai-Moratabat, OraclePdfParser, PDF-EXTRACTOR, IProgram2. They do not set this project's architecture or legal policy.

**Caution:** Prior research included user-supplied Markdown summaries of regulations and a larger source inventory. Summaries and source counts are not substitutes for original text, page-level evidence and up-to-date applicability. No confidential source files are copied into this public repository.

## Owner-confirmed operational clarifications — 2026-11-06
- [Owner workflow and 30200105 unit scope](governance/OWNER-OPERATING-REQUIREMENTS.md)
- [Part-time cash entitlement versus pension assessment base control](domain/PART-TIME-INSURANCE-CONTROL.md)
These clarify workflow and bounded insurance exception. They are not authority to implement general insurance recalculation or activate an unverified legal rule.
