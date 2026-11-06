# Conceptual ERD 2 — Immutable Oracle evidence, import quality, repeated financial lines and reviewer assignments

**Draft, non-executable**. This view is source-focused: import values and labels are **observations**, not approved entitlements or canonical employee facts.

```mermaid
erDiagram
  ORGANIZATION_UNIT ||--o{ PAYROLL_CYCLE : owns
  PAYROLL_CYCLE ||--o{ PAYROLL_GROUP : includes
  PAYROLL_CYCLE ||--o{ IMPORT_BATCH : receives
  IMPORT_BATCH ||--o{ SOURCE_DOCUMENT : includes
  SOURCE_DOCUMENT ||--o{ DOCUMENT_PARSE_ATTEMPT : parsed_as
  DOCUMENT_PARSE_ATTEMPT ||--o{ SOURCE_RECIPIENT_OCCURRENCE : shows
  DOCUMENT_PARSE_ATTEMPT ||--o{ SOURCE_GROUP_SUMMARY : includes
  DOCUMENT_PARSE_ATTEMPT ||--o{ PARSE_VALIDATION_ISSUE : raises
  SOURCE_RECIPIENT_OCCURRENCE ||--o{ SOURCE_FINANCIAL_OCCURRENCE : lists
  SOURCE_RECIPIENT_OCCURRENCE ||--o{ SOURCE_REPORTED_TOTAL : prints
  SOURCE_RECIPIENT_OCCURRENCE ||--o{ RECIPIENT_MATCH_DECISION : matched_by
  SOURCE_FINANCIAL_OCCURRENCE ||--o{ COMPONENT_MAPPING_DECISION : mapped_by
  PAYEE_RELATIONSHIP ||--o{ RECIPIENT_MATCH_DECISION : can_match
  FINANCIAL_COMPONENT ||--o{ COMPONENT_MAPPING_DECISION : can_map
  PAYROLL_GROUP ||--o{ REVIEWER_ASSIGNMENT : assigned
  SYSTEM_USER ||--o{ REVIEWER_ASSIGNMENT : performs
  SOURCE_DOCUMENT {
    uuid SourceDocumentId PK
    uuid ImportBatchId FK
    uuid SupersedesDocumentId FK "nullable"
    string DocumentKind
    string ReportPeriod_nullable
    string OriginalSha256
    string SecureArchivePointer
    string StorageStatus
  }
  IMPORT_BATCH {
    uuid ImportBatchId PK
    uuid CycleId FK
    datetime ReceivedAt
    string IntakeStatus
  }
  DOCUMENT_PARSE_ATTEMPT {
    uuid ParseAttemptId PK
    uuid SourceDocumentId FK
    string AdapterName
    string ParserVersion
    string ParseStatus
    datetime ParsedAt
  }
  SOURCE_RECIPIENT_OCCURRENCE {
    uuid SourceRecipientOccurrenceId PK
    uuid ParseAttemptId FK
    string SourceNameRaw
    string SourceIdTokens
    string SourcePaymentNumber
    int FirstPage
    int LastPage
    string RecognitionStatus
  }
  SOURCE_FINANCIAL_OCCURRENCE {
    uuid SourceLineId PK
    uuid SourceRecipientOccurrenceId FK
    int PageNumber
    int TableIndex_nullable
    int RowIndex_nullable
    int PaneIndex_nullable
    string RawCode_nullable
    string RawArabicLabel
    string RawAmountToken_nullable
    decimal ParsedAmount_nullable
    string SourceSide
    string QualityStatus
  }
  SOURCE_REPORTED_TOTAL {
    uuid ReportedTotalId PK
    uuid SourceRecipientOccurrenceId FK
    string TotalKind
    decimal ReportedAmount_nullable
    string SourceCellRef
  }
  SOURCE_GROUP_SUMMARY {
    uuid SourceGroupId PK
    uuid ParseAttemptId FK
    string BankAndBranchRaw
    string GroupKey_nullable
    decimal ReportedNet_nullable
    int EmployeeCount_nullable
    string MatchStatus
  }
  PARSE_VALIDATION_ISSUE {
    uuid ParseIssueId PK
    uuid ParseAttemptId FK
    uuid SourceLineId FK "nullable"
    string Severity
    string DiagnosticCode
    string EvidenceLocator
  }
  RECIPIENT_MATCH_DECISION {
    uuid MatchDecisionId PK
    uuid SourceRecipientOccurrenceId FK
    uuid PayeeRelationshipId FK "nullable"
    uuid ReviewerId FK
    string Status
    string EvidenceRef
  }
  COMPONENT_MAPPING_DECISION {
    uuid ComponentMappingId PK
    uuid SourceLineId FK
    uuid ComponentId FK "nullable"
    uuid ReviewerId FK
    string MappingStatus
    string AliasVersion_nullable
  }
  PAYROLL_CYCLE {
    uuid CycleId PK
    uuid OrganizationUnitId FK
    string CycleType
    string PayrollPeriod
    string Status
  }
  PAYROLL_GROUP {
    uuid PayrollGroupId PK
    uuid CycleId FK
    string GroupReference
    string ResponsibilityStatus
  }
  REVIEWER_ASSIGNMENT {
    uuid AssignmentId PK
    uuid PayrollGroupId FK
    uuid UserId FK
    datetime AssignedAt
    datetime UnassignedAt_nullable
    string PermissionScope
  }
  SYSTEM_USER {
    uuid UserId PK
    string InternalUserName
    string AccountStatus
  }
  ORGANIZATION_UNIT {
    uuid OrganizationUnitId PK
    string InstitutionalCode
  }
  PAYEE_RELATIONSHIP {
    uuid PayeeRelationshipId PK
  }
  FINANCIAL_COMPONENT {
    uuid ComponentId PK
  }
```

## Non-negotiable parser and source rules
- **No unique constraint on `RawCode`, `RawArabicLabel`, their concatenation, or amount**. Even identical code/label/amount can denote two physical occurrences, or double extraction; validate with source coordinates and official totals before deciding.
- `SourceLineId` identifies a **specific extraction occurrence**. Re-parsing the same original with another adapter/version generates separately traceable candidates, then comparison/staging decides accepted interpretation. Do not double-post their amounts.
- Persist the **original immutable PDF** securely with hash; never commit real files to GitHub. The parsed text, glyph order, normalized display candidates, coordinates and source page form a chain of evidence.
- Some report types display no employee detail and only source bank groups/summary. The `SOURCE_RECIPIENT_OCCURRENCE` relation is optional.
- No mapping decision can silently mutate PERSON/EMPLOYMENT/APPROVED_FINANCIAL_FACT or the imported original string.
- Work assignment is by **payroll cycle and group**, with recorded reassignment dates. Other disbursement types use authorized access rules; shared access does not imply concurrent writes without optimistic concurrency.
- Name conflicts and duplicate employee identity are review cases; a payslip with `ReviewRequired` cannot overwrite a verified source's name.
- Group matching depends on period, institutional scope, bank and branch and verified group identifier, **not array index**.
- Partial reports (missing cover/payment sheet) cannot be called fully cross-reconciled. A `Rejected` classification cannot be downgraded by subsequent review.
- `COMPONENT_MAPPING_DECISION` may be one-to-many over time as proposed/approved/rejected **decisions**; only one currently authoritative mapping may be active for an occurrence at a given decision revision.

## Trust statuses are separate
`ParseStatus` = structural readable/rejected/review; `SourceLine.QualityStatus` = item extraction confidence; `MatchDecision` = verified person/component match; `AuditReviewStatus` = financial review by a human; `PaidStatus` = proof of financial settlement.

## Interfaces to detailed audit ERD
`PayeeRelationshipId` and `SourceLineId` are referenced from reconciliation and follow-up design. The diagrams are separated to remain readable.
