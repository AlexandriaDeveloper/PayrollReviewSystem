# Conceptual ERD 1 — Organization, identities, approved facts and legal rules

**Draft, non-executable**. The three colleges are organizational units below a parent unit carrying code `30200105`. No employee legal status follows merely from organizational membership.

```mermaid
erDiagram
  ORGANIZATION_UNIT ||--o{ ORGANIZATION_UNIT : parent_of
  PERSON ||--o{ PERSON_IDENTIFIER : holds
  PERSON ||--o{ EMPLOYMENT_RELATIONSHIP : enters
  PERSON ||--o{ PAYEE_RELATIONSHIP : receives_as
  ORGANIZATION_UNIT ||--o{ EMPLOYMENT_RELATIONSHIP : appoints
  ORGANIZATION_UNIT ||--o{ PAYEE_RELATIONSHIP : pays
  EMPLOYMENT_RELATIONSHIP o|--o{ PAYEE_RELATIONSHIP : may_underlie
  EMPLOYMENT_RELATIONSHIP ||--o{ GRADE_HISTORY : has
  EMPLOYMENT_RELATIONSHIP ||--o{ STATUS_HISTORY : has
  EMPLOYMENT_RELATIONSHIP ||--o{ WORK_ARRANGEMENT : has
  PAYEE_RELATIONSHIP ||--o{ APPROVED_FINANCIAL_FACT : records
  PAYEE_RELATIONSHIP ||--o{ COMPONENT_ASSIGNMENT : receives
  FINANCIAL_COMPONENT ||--o{ COMPONENT_ASSIGNMENT : assigned_as
  FINANCIAL_COMPONENT ||--o{ COMPONENT_AUDIT_POLICY : controlled_by
  FINANCIAL_COMPONENT ||--o{ FINANCIAL_RULE_VERSION : calculated_by
  LEGAL_PROVISION_VERSION o|--o{ FINANCIAL_RULE_VERSION : authorizes
  LEGAL_SOURCE ||--o{ LEGAL_PROVISION_VERSION : contains
  ADMINISTRATIVE_DECISION ||--o{ EVENT_EFFECT : creates
  FINANCIAL_COMPONENT o|--o{ EVENT_EFFECT : affects
  PAYEE_RELATIONSHIP o|--o{ ADMINISTRATIVE_DECISION : concerns
  ORGANIZATION_UNIT {
    uuid OrganizationUnitId PK
    uuid ParentUnitId FK
    string InstitutionalCode
    string ArabicName
    string UnitType
    string Status
  }
  PERSON {
    uuid PersonId PK
    string DisplayName
    string VerificationStatus
  }
  PERSON_IDENTIFIER {
    uuid IdentifierId PK
    uuid PersonId FK
    string Kind
    string ProtectedValueRef
    string VerifiedStatus
  }
  EMPLOYMENT_RELATIONSHIP {
    uuid EmploymentId PK
    uuid PersonId FK
    uuid EmployerUnitId FK
    string PersonnelNumber
    date ValidFrom
    date ValidTo
  }
  PAYEE_RELATIONSHIP {
    uuid PayeeRelationshipId PK
    uuid PersonId FK
    uuid PayingUnitId FK
    uuid EmploymentId FK_nullable
    string ReasonOrRelationshipType
    date ValidFrom
    date ValidTo
  }
  GRADE_HISTORY {
    uuid GradeHistoryId PK
    uuid EmploymentId FK
    string GradeOrRank
    date ValidFrom
    date ValidTo
    uuid DecisionId FK_nullable
  }
  STATUS_HISTORY {
    uuid StatusHistoryId PK
    uuid EmploymentId FK
    string RegimeAndStatus
    date ValidFrom
    date ValidTo
    uuid DecisionId FK_nullable
  }
  WORK_ARRANGEMENT {
    uuid ArrangementId PK
    uuid EmploymentId FK
    string WorkPattern
    decimal CashFraction_nullable
    date ValidFrom
    date ValidTo
    uuid DecisionId FK
  }
  APPROVED_FINANCIAL_FACT {
    uuid FactId PK
    uuid PayeeRelationshipId FK
    string FactType
    decimal Amount_nullable
    date ReferenceDate
    date ValidFrom
    date ValidTo
    string EvidenceStatus
    uuid SupersedesFactId FK_nullable
  }
  COMPONENT_ASSIGNMENT {
    uuid AssignmentId PK
    uuid PayeeRelationshipId FK
    uuid ComponentId FK
    decimal FixedValue_nullable
    date ValidFrom
    date ValidTo
    uuid DecisionId FK_nullable
  }
  FINANCIAL_COMPONENT {
    uuid ComponentId PK
    string CanonicalName
    string EconomicRole
    string StableInternalCode
  }
  COMPONENT_AUDIT_POLICY {
    uuid PolicyId PK
    uuid ComponentId FK
    uuid OrganizationUnitId FK_nullable
    string ReviewTreatment
    date ValidFrom
    date ValidTo
    string ApprovalStatus
  }
  FINANCIAL_RULE_VERSION {
    uuid RuleVersionId PK
    uuid ComponentId FK
    uuid ProvisionVersionId FK_nullable
    string Applicability
    string Version
    date EffectiveFrom
    date EffectiveTo
    string VerificationStatus
  }
  LEGAL_SOURCE {
    uuid LegalSourceId PK
    string SourceNumber
    string VerifiedSourceRef
    string Status
  }
  LEGAL_PROVISION_VERSION {
    uuid ProvisionVersionId PK
    uuid LegalSourceId FK
    string ArticleAnchor
    date EffectiveFrom
    date EffectiveTo
  }
  ADMINISTRATIVE_DECISION {
    uuid DecisionId PK
    uuid PayeeRelationshipId FK_nullable
    string DecisionKind
    date IssuedDate
    date FunctionalEffectiveDate_nullable
    date FinancialEffectiveDate_nullable
    string EvidenceRef
    string ApprovalStatus
  }
  EVENT_EFFECT {
    uuid EventEffectId PK
    uuid DecisionId FK
    uuid ComponentId FK_nullable
    date EffectiveFrom
    date EffectiveTo
    string ImpactType
  }
```

## Important business invariants
1. **Person != Employment != PayeeRelationship**. Someone receiving a one-off reward does not need an `EmploymentRelationship`; a faculty member may have more than one valid payment relationship.
2. `PayeeRelationship.EmploymentId` is optional but **never inferred** merely because a person shares the same name or national ID.
3. A `WORK_ARRANGEMENT.CashFraction=0.65` (when proved by decision) changes the **cash entitlement proposal**. It does not automatically change `APPROVED_FINANCIAL_FACT` for the insured wage base.
4. `APPROVED_FINANCIAL_FACT` includes reference-only facts like `BasicAsOf2015_06_30` and a separately evidenced `ApprovedInsuredWageBase`. These are **not** automatically cash payable.
5. Generic social insurance and the two Owner-specified funds are source-trusted under a **contextual component policy**; they are not subject to generic local calculation. The narrow work-time insurance-base exception is a separate check.
6. A rule version is not active merely because its source has a title, number or secondary summary. Store original provision, effective dates, population and human approval.
7. `ValidFrom/ValidTo` and `RecordedAt`/revision timestamps distinguish entitlement effect from time the auditor learned of changes. Record independent *functional* and *financial* dates.
8. Use separately authorized referential consistency checks for optional `DecisionId` links and contradictory event/period history. The ERD is a sketch of cardinalities, not a license to hardcode a global exclusive history rule.
9. The finance source chapter/باب الصرف is associated with the component's **versioned accounting classification** (to be defined in approved physical schema), not confused with its Oracle account code; more than one component can share an Oracle code.

## Additional fields intentionally omitted
Record author/approver, `RecordedAt`, `SupersededAt`, organization scope, original documentary evidence, privacy classification, row version, and temporal uniqueness constraints are mandatory for physical implementation design, but not repeated on every Mermaid box.
