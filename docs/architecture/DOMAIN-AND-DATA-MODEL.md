# Conceptual data model — source of truth and time

**Design proposal; not an executable migration.** A canonical identity is distinct from employment, payee, and one or more payment relationships.

## Core entities
| Concept | Minimum durable fields / relationships | Important invariant |
| --- | --- | --- |
| Person | PersonId, names, separately protected identities (national ID/passport), verification | National ID is not PK; foreign recipients supported. |
| EmploymentRelationship (Employees) | EmployeeId, PersonId, employer/appointing entity, employee number, appointment/end dates, provenance | One person may have multiple relationships across periods. |
| FinancialRelationship/Payee | PayeeId, PersonId, paying entity, entitlement reason, validity | A recipient of fees is not automatically an employee. |
| OrganizationalAssignmentHistory | EmployeeId, unit, from/to, evidence | Do not overwrite historical unit. |
| EmploymentStatusHistory | EmployeeId, legal regime, status, from/to, decision | Grade, emeritus, contract, leave etc are dated facts. |
| EmployeeGradeHistory | EmployeeId, grade/rank, from/to, decision, verification | Past grade drives past entitlement. |
| FinancialComponent | ComponentId, stable code, name, classification | Separate cash earning/deduction/reference base/employer cost/display total. |
| FinancialRuleVersion | RuleVersionId, component, coverage, rule type, date range, verified legal anchor | Never silently change a used rule version. |
| EmployeeFinancialFact | FactId, EmployeeId, fact type, amount, reference date, validity, recorded date, source, verification | A frozen historical base is not a payable item. |
| EmployeeComponentAssignment | EmployeeId, component, parameters/value, from/to, decision | Use for genuine assignments; not repeated calculated monthly values. |
| AdministrativeDecision | DecisionId, type, issuing authority, issue/effect dates, evidence, approval | Promotion's functional and financial dates may differ. |
| FinancialEventEffect | EventId/DecisionId, affected component, scope, period, rule basis | One order can affect multiple components differently. |
| SourceDocument | DocumentId, SHA-256 hash, source, version, received time, secure archive pointer | Source unchanged even after later correction. |
| ImportBatch / SourceObservation | BatchId, file, type, report period, person identifiers, exact observed item/amount/page | Imported data is external evidence, not canonical entitlement. |
| ComponentMapping | External system/code/label/context -> internal component, date, reviewer decision | Account code alone may be ambiguous. |
| ExpectedCalculationRun/Line | RunId, employee, entitlement period, component, expected amount, rule/input snapshot, trace | Repeatable explainable calculation. |
| ReconciliationDifference | Source line, calculation line, amount, classification, evidence quality | Missing data is not zero. |
| AuditFinding / ReviewerAction | FindingId, opinion, classification, reason, evidence, actor/time | Finishing review != settling payment. |
| Correction / ReviewedOverlay | Original value, review value, official correction source and actor | Do not edit Oracle original. |
| RetroEntitlement | Employee, component, entitlement period, cause, calculation version, expected differential | Unique logical claim; avoid duplicate issue. |
| RetroDisbursement / Allocation | Payment evidence, settlement period, amount, allocation to specific claims | Unallocated partial payment remains unresolved. |
| RemittanceEvidence | Tax/insurance liability period, remitted amount/date, proof | Withheld != remitted. |
| AuditEvent | Actor, action, entity, before/after refs, timestamp, reason, evidence | Append-only, no silent deletion. |

## Historical semantics
Every effective-dated entity needs a domain-relevant interval [ValidFrom, ValidTo) and when-the-system-knew-it timestamps, including supersession/correction linkage. Dates alone do not establish employer obligations; legal RuleVersion identifies which temporal dimension governs each effect.

## Constraints
- Strong foreign keys within logical ownership, uniqueness/identity policies with temporal checks.
- No duplicate active overlapping values for an exclusive employee+fact-type+validity dimension unless law explicitly permits it.
- Do not convert an Oracle employee code or institution prefix to PersonId/EmployeeId.
- Imported unknown fields must not null existing canonical fields.
- Source file rows and calculation snapshots immutable; review corrections are new revisions.
- Indexed lookups by employee, component, entitlement month, import batch, external code, source hash and review state.
- Private data access masked by role and audit scope.

## Employees table itself
Keep it narrow: EmployeeId (opaque PK), PersonId, employer organization relation, optional employee/personnel codes, start/end relation dates, source and record status/audit metadata. Dates/grades/legal category/bank/earnings/2015 base belong in their own historical tables, not repeating employee columns.
