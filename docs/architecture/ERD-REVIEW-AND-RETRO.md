# ERD رقم 3 — محرك الحساب والمراجعة والاعتماد والمتجمدات

**مخطط تصوري.** يحفظ الفرق بين المبلغ الوارد من Oracle، والحساب المقترح، وقرار المراجع، والتصحيح الرسمي، وما ظهر في كشف الصرف، وما ثبت صرفه.

## العلاقات المرسومة
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

## مسؤولية كل مجموعة
- `ExpectedCalculationRun` و`ExpectedCalculationLine`: نتيجة حساب مستقلة **مقترحة** لكل بند وشهر ونسخة قاعدة، مع الأثر الحسابي.
- `AuditCase` يجمع مراجعة علاقة صرف في دورة محددة، و`AuditFinding` يسجل الفرق أو نقص الهوية/المستند حتى لو لم نستطع وضع مبلغ له.
- `ReviewAction` يسجل ما فعله المراجع وسببه. `ReviewedAmountOverlay` نتيجة مراجعة منفصلة لا تعدّل `SourceFinancialOccurrence`.
- `ReviewApproval` يعتمد **نسخة ثابتة** من البيانات والقواعد والحسابات، و`PostApprovalAmendment` يفتح إصدارًا جديدًا لقرار لاحق مثل وفاة أو إعارة أو عمل بجزء الشهر.
- `LineAllocation` جسر ربط محتمل **متعدد لمتعدد** بين السطور المستوردة والمتوقعة؛ يحتاج مراجعة قبل اعتماد التجميع.
- `RetroEntitlement` فرق عن شهر أصلي وبند وسبب وإصدار. `ReportedRetroMovement` ظهور مبلغ لاحقًا في كشف الصرف، لا يعني تسوية.
- `PaymentEvidence` إثبات فعلي مستقل؛ `RetroPaymentAllocation` يوزع مبلغًا مثبتًا على مستحقات أصلية إن تسمح الأدلة.
- `AuditEvent` يربط المستخدم والوقت والمصدر والقرار والقيمة السابقة واللاحقة.

## قواعد عدم التلاعب والازدواج
1. بند يُؤخذ من المركزية وفق `CentralSourceAmount` قد لا يكون له `ExpectedAmount` مستقل. لا تملأه بصفر ولا تدّع حسابه.
2. إذا جمعت المركزية عدة بنود في سطر واحد، اعرض كل المتوقع وكل المصدر، واسمح بقبول استثنائي موثق فقط إذا تحققت شروط باب الصرف والصلاحية.
3. المبلغ المتجمد يرتبط بالمستفيد والبند والشهر الأصلي والقرار وإصدار الحساب؛ لا تكرر المطالبة عند تشغيل الحساب مرة أخرى.
4. صرف مبلغ متجمد مجمع لا يعني معرفة توزيعه على الأشهر تلقائيًا؛ يبقى `Unallocated` حتى يثبت الربط.
5. اعتماد المراجعة ليس إثباتًا للسداد، وانتهاء الفحص لا يغلق طلب التصحيح أو متابعة التسوية.
6. بعد الاعتماد، يحتفظ النظام بالنسخة السابقة، ولا يغيرها القرار الجديد.
7. لا يسمح بإجمالي تخصيص أكبر من المبلغ المثبت، مع مراعاة إشارة الفروق السالبة وضوابطها.
8. الفصل بين من يقترح قاعدة ومن يعتمد أثرها يتم بالخادم وليس واجهة المستخدم فقط.

## مثال لمقارنة المصدر والمتوقع
راجع [المثال العملي للـERD](../examples/01-ERD-INPUT-OUTPUT.md) و[سيناريوهات المراجعة في UML](../examples/02-UML-REVIEW-EXAMPLES.md).
