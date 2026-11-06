# Decisions and open questions

## Confirmed by Owner interaction
| ID | Decision |
| --- | --- |
| D-001 | Project repository is **AlexandriaDeveloper/PayrollReviewSystem**, separate from legacy repositories. |
| D-002 | System targets **independent review of government payroll/central entitlements**, not payroll disbursement. |
| D-003 | ChatGPT acts as system architect and code reviewer; Gemini is the coding implementer; GitHub is the communication channel. |
| D-004 | Domain documentation and explicit agent instructions must be durable Markdown in repository. |
| D-005 | Document-only architecture baseline is needed before source-code work; new implementation has not been authorized by this PR. |

## Strong architecture recommendations — pending approval as specific ADRs
| ID | Proposal | Why |
| --- | --- | --- |
| P-001 | .NET/ASP.NET Core, Angular RTL, SQL Server, modular monolith | Consistent existing skill set and auditable modular boundaries; exact versions open. |
| P-002 | Temporal relational schema and immutable source/archive | Repeatable review at old entitlement dates. |
| P-003 | Independent rule engine with approved versions and calculation traces | Legal/rule applicability is not fixed. |
| P-004 | Adapter-based Oracle PDF import with strict verification | Existing extraction tools handle different PDF layouts. |
| P-005 | Per-component/period arrears ledger and paid-allocation ledger | Delayed or partial settlement and no double counting. |
| P-006 | Role-separated reviewer and rule approver | Financial segregation of duties. |

## Business/legal questions — cannot be silently inferred
- Which college/administrative entities and employee categories are in MVP?
- Which Oracle report layouts and official form numbers belong to actual input types?
- Which authoritative laws/decisions/rates apply per employee class and period?
- What accounting/finance criteria allow release with unresolved findings, and who may approve exceptions?
- How to confirm actual bank/payment settlement versus only Payroll sheet inclusion?
- Which official rules govern backdated promotion effects on each item, tax and social-insurance correction?
- What is the rule-specific rounding/proration and prior-year policy?
- Source of employee master data, identity reconciliation procedure and approved encryption/retention controls?
- Which user roles and environment/hosting controls are required?

Do not turn a proposal into an approved business decision through implementation alone. Document Owner approval with Issue/ADR links.

## Owner-validated operational scope (2026-11-06)
| ID | Decision |
| --- | --- |
| D-006 | Accounting unit: Alexandria Medical Complex; institutional code **30200105**; faculties Medicine, Dentistry and Pharmacy. |
| D-007 | Coverage includes staff, university teaching faculty, contracted workers and honorarium/reward payees. |
| D-008 | Review ordinarily occurs before payroll approval; documented urgent events can require post-approval review amendments. |
| D-009 | Reviewers provide approved employee financial reference data; Oracle source never silently substitutes for these. |
| D-010 | Financial-event engine proposes component adjustments; reviewer accepts/modifies/rejects proposals with evidence. |
| D-011 | Aggregating multiple entitlements into one central line can be reviewer-accepted as a documented distribution exception when within one expenditure chapter **subject to other required controls**; do not auto-approve. |
| D-012 | Generic central pension/insurance calculations are outside independent approval scope; a **part-time full-base vs reduced-cash-base** review exception is required. |
| D-013 | Multiple reviewers, salary group assignments per cycle and shared authorization for other payrolls; every change attributed and historic. |
| D-014 | System to run on internal network with protected access; Owner intends repository private. |
| D-015 | Amounts for **صندوق دعم المعاقين** and **صندوق الشهداء** received from central entitlements are accepted as centrally calculated and **not recomputed** by the reviewer; still validate extraction and aggregates. Exact account mappings pending. |

**Source:** owner's direct workflow clarifications; not a determination of what Egyptian law authorizes for every employee class. See ../domain/PART-TIME-INSURANCE-CONTROL.md and OWNER-OPERATING-REQUIREMENTS.md.

### Remaining clarifications (narrowed by Owner input)
- Confirm specific regulatory source/cadre coverage for full insured wage treatment during part-time work.
- Confirm what source reveals the centrally assessed insurance wage base (not simply the contribution amount).
- Confirm who can approve emergency amendment/reopening after review approval.
- Confirm all relevant باب الصرف codes and constraints for accepting aggregated component entries.
- Confirm group assignment authority and concurrent-work locking policy.
