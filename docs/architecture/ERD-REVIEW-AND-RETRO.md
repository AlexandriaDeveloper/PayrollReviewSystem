# Conceptual ERD 3 — Expected payroll, audit workflow, retroactive entitlement and verified settlement

> Draft, non-executable. Keep historical revisions. `SourceFinancialOccurrence`, `PayeeRelationship` and `AdministrativeDecision` are logical references to [ERD 1](ERD-CORE.md) and [ERD 2](ERD-IMPORT-AND-AUDIT.md).

```mermaid
erDiagram
  PAYROLL_CYCLE ||--o{ EXPECTED_CALCULATION_RUN : receives
  PAYEE_RELATIONSHIP ||--o{ EXPECTED_CALCULATION_RUN : subject
  EXPECTED_CALCULATION_RUN ||--o{ EXPECTED_CALCULATION_LINE : computes
  FINANCIAL_COMPONENT ||--o{ EXPECTED_CALCULATION_LINE : component
  PAYROLL_CYCLE ||--o{ AUDIT_CASE : reviewed_in
  PAYEE_RELATIONSHIP ||--o{ AUDIT_CASE : subject
  AUDIT_CASE ||--o{ AUDIT_FINDING : has
  AUDIT_CASE ||--o{ REVIEW_ACTION : records
  AUDIT_CASE ||--o{ REVIEW_APPROVAL : approved_as
  AUDIT_CASE ||--o{ POST_APPROVAL_AMENDMENT : reopened_by
  REVIEW_APPROVAL o|--o{ POST_APPROVAL_AMENDMENT : prior_version
  AUDIT_FINDING ||--o{ REVIEWED_AMOUNT_OVERLAY : resolved_by
  EXPECTED_CALCULATION_LINE ||--o{ LINE_ALLOCATION : allocation_candidate
  SOURCE_FINANCIAL_OCCURRENCE ||--o{ LINE_ALLOCATION : reported_candidate
  AUDIT_CASE ||--o{ LINE_ALLOCATION : judged_in
  EXPECTED_CALCULATION_LINE o|--o{ RETRO_ENTITLEMENT : changes
  ADMINISTRATIVE_DECISION o|--o{ RETRO_ENTITLEMENT : triggers
  RETRO_ENTITLEMENT ||--o{ RETRO_PAYMENT_ALLOCATION : may_receive
  PAYMENT_EVIDENCE ||--o{ RETRO_PAYMENT_ALLOCATION : supports
  SOURCE_FINANCIAL_OCCURRENCE o|--o{ REPORTED_RETRO_MOVEMENT : represents
  REPORTED_RETRO_MOVEMENT o|--o{ RETRO_ENTITLEMENT : may_identify
  SYSTEM_USER ||--o{ REVIEW_ACTION : performed
  SYSTEM_USER ||--o{ REVIEW_APPROVAL : signs
  SYSTEM_USER ||--o{ AUDIT_EVENT : is_actor
  AUDIT_CASE ||--o{ AUDIT_EVENT : logged_against
  EXPECTED_CALCULATION_RUN {
    uuid CalculationRunId PK
    uuid PayrollCycleId FK
    uuid PayeeRelationshipId FK
    string CalculationVersion
    string SnapshotStatus
    datetime CreatedAt
  }
  EXPECTED_CALCULATION_LINE {
    uuid ExpectedLineId PK
    uuid CalculationRunId FK
    uuid ComponentId FK
    string EntitlementPeriod
    decimal ExpectedAmount_nullable
    string CalculationDisposition
    string RuleTraceReference_nullable
  }
  AUDIT_CASE {
    uuid AuditCaseId PK
    uuid PayrollCycleId FK
    uuid PayeeRelationshipId FK
    string ReviewStatus
    int ReviewRevision
    string SourceScopeRef
  }
  AUDIT_FINDING {
    uuid FindingId PK
    uuid AuditCaseId FK
    string FindingKind
    string Severity
    decimal Difference_nullable
    string EvidenceStatus
  }
  REVIEW_ACTION {
    uuid ActionId PK
    uuid AuditCaseId FK
    uuid PerformedBy FK
    string ActionType
    string Reason
    datetime OccurredAt
  }
  REVIEWED_AMOUNT_OVERLAY {
    uuid OverlayId PK
    uuid FindingId FK
    decimal ReviewedAmount
    string Rationale
    uuid EvidenceId FK
    uuid ReviewerActionId FK
  }
  REVIEW_APPROVAL {
    uuid ApprovalId PK
    uuid AuditCaseId FK
    uuid ApprovedBy FK
    int ReviewRevision
    string ApprovalType
    datetime ApprovedAt
    string SnapshotHash
  }
  POST_APPROVAL_AMENDMENT {
    uuid AmendmentId PK
    uuid AuditCaseId FK
    uuid EarlierApprovalId FK
    string TriggerKind
    uuid NewDecisionId FK "nullable"
    string OutcomeStatus
  }
  LINE_ALLOCATION {
    uuid AllocationId PK
    uuid AuditCaseId FK
    uuid SourceLineId FK
    uuid ExpectedLineId FK
    decimal MatchedAmount
    string AllocationBasis
    string ReviewerDecisionStatus
  }
  RETRO_ENTITLEMENT {
    uuid RetroEntitlementId PK
    uuid ExpectedLineId FK "nullable"
    uuid DecisionId FK "nullable"
    uuid PayeeRelationshipId FK
    uuid ComponentId FK
    string EntitlementPeriod
    decimal DifferenceAmount
    string ClaimRevision
  }
  REPORTED_RETRO_MOVEMENT {
    uuid ReportedMovementId PK
    uuid SourceLineId FK "nullable"
    string ReportedPeriod
    decimal ReportedAmount
    string AttributionStatus
  }
  PAYMENT_EVIDENCE {
    uuid PaymentEvidenceId PK
    string ProofKind
    string SettlementPeriod
    decimal VerifiedAmount_nullable
    string VerificationStatus
    string SourceReference
  }
  RETRO_PAYMENT_ALLOCATION {
    uuid RetroAllocationId PK
    uuid RetroEntitlementId FK
    uuid PaymentEvidenceId FK
    decimal AllocatedAmount
    string AllocationStatus
  }
  SYSTEM_USER {
    uuid UserId PK
  }
  AUDIT_EVENT {
    uuid EventId PK
    uuid UserId FK
    uuid AuditCaseId FK "nullable"
    string EventKind
    string BeforeHash_nullable
    string AfterHash_nullable
    datetime OccurredAt
  }
  PAYROLL_CYCLE {
    uuid PayrollCycleId PK
  }
  PAYEE_RELATIONSHIP {
    uuid PayeeRelationshipId PK
  }
  FINANCIAL_COMPONENT {
    uuid ComponentId PK
  }
  SOURCE_FINANCIAL_OCCURRENCE {
    uuid SourceLineId PK
  }
  ADMINISTRATIVE_DECISION {
    uuid DecisionId PK
  }
```

## Critical financial and procedural semantics
- One reported line can represent **several expected financial lines**, and one expected component can be represented by several reported occurrences. `LINE_ALLOCATION` is an **explicit, reviewer-reviewed many-to-many bridge**, never an implicit code join. The same-expenditure-chapter exception is not automatic matching.
- Expected lines are **proposals** when the underlying formula is independent and verified. If the audit policy is `CENTRAL_SOURCE_AMOUNT`, `ExpectedAmount` is absent **by design**; it must not be defaulted to zero or falsely declared a separately verified match.
- Not every `AuditFinding` needs one numeric difference: source ambiguity, wrong identity, missing legal decision or wrong funding chapter are valid findings without precise `Difference`.
- A `REVIEWED_AMOUNT_OVERLAY` is a decision **about** a reported value and never modifies the imported source line.
- `REVIEW_APPROVAL` signs a fixed review revision/snapshot. A later event creates `POST_APPROVAL_AMENDMENT` and a new review revision instead of editing earlier approval in place.
- `RETRO_ENTITLEMENT`: one logical claim per payee relationship + component + original period + cause + approved calculation revision. Subsequent recalculations use new revisions with supersession links; do not duplicate active logical claims.
- `REPORTED_RETRO_MOVEMENT` means the amount appeared in central payroll. It does **not** prove bank settlement. `RETRO_PAYMENT_ALLOCATION` requires separately acceptable `PAYMENT_EVIDENCE`. A lump sum with uncertain month/item mapping remains unallocated and ReviewRequired.
- Overpayment does not authorize automatic recovery; a separate human-controlled correction/follow-up is needed.
- External payment evidence may be absent in the first MVP. Record `ReportedInPayroll` without claiming `PaidVerified`.
- Audit log captures import, calculations, review changes, rule changes, assignment changes and access decisions; the full authoritative audit ledger must be append-only or otherwise tamper-evident under chosen infrastructure.

## Referential integrity and unresolved physical choices
1. An `AUDIT_CASE` represents a reviewed **payee relationship in a specific payroll cycle**, not necessarily a single PDF recipient row. Case details can include several source recipient occurrences/official files.
2. `LINE_ALLOCATION` needs explicit cap checks: allocations by source/expected side must not exceed the confirmed available amount and must avoid double allocation across report reimports. Signed negative movements require an approved treatment rather than an unsigned total.
3. `REVIEW_APPROVAL` must bind all approved inputs: import versions, mappings, facts, rules, financial run and findings, rather than a mutable screen or current employee profile.
4. User permission to create a review action is not proof of permission to approve the same action. Define segregation of duties with the Owner.
5. Timing and identity of real disbursement evidence require a separate source study before final schema/matching.
6. Put immutable originals, immutable calculation versions and audit actions into separate storage/transaction boundaries from mutable work-in-progress projections.
