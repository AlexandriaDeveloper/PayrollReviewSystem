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
