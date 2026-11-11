# مخطط قاعدة البيانات المنطقي وقاموسه

**الحالة: ERD منطقي مقترح v0.3 — تعديلات D-030 إلى D-039 وتبسيط التسجيل قيد المراجعة — لا DDL أو Migration أو اختيار محرك قاعدة بيانات.** يكمّل المخططات الحالية ويعيد استعمال مفاهيمها؛ اختلاف الاسم يحتاج توحيدًا عند اعتماد التصميم الفيزيائي، ولا يبرر إنشاء جدولين للمعنى نفسه.

PK هوية داخلية مقترحة، وFK علاقة مرشحة. `optional` تعني أن غيابها مقصود في الحالة المحددة. العلاقات بين مجالات DDD تمثل مراجع وعقودًا؛ لا تمنح وحدة حق كتابة جداول وحدة أخرى، ولا تقرر كل FK فيزيائي. `uuid` و`decimal` أسماء أنواع توضيحية، وتُعتمد أنواع التخزين والدقة والقيود مع التقنية.

## ERD-A — الهوية والتاريخ والشهر والصرفيات

```mermaid
erDiagram
  ORGANIZATION_UNIT ||--o{ PAYROLL_CYCLE : owns
  PERSON ||--o{ EMPLOYMENT_RELATIONSHIP : hasEmployment
  PERSON ||--o{ PAYEE_RELATIONSHIP : receivesAs
  EMPLOYMENT_RELATIONSHIP o|--o{ PAYEE_RELATIONSHIP : optionalEmployment
  EMPLOYMENT_RELATIONSHIP ||--o{ GRADE_HISTORY : historicalGrades
  PAYEE_RELATIONSHIP ||--o{ APPROVED_FINANCIAL_FACT : approvedFacts
  PAYROLL_CYCLE ||--o{ PAYROLL_RUN : containsRuns
  PAYROLL_RUN ||--o{ PAYROLL_DISBURSEMENT : containsDisbursements
  PAYROLL_DISBURSEMENT ||--o{ DISBURSEMENT_RECIPIENT : includesRecipients
  PAYEE_RELATIONSHIP ||--o{ DISBURSEMENT_RECIPIENT : appearsIn
  EMPLOYMENT_RELATIONSHIP ||--o{ MONTHLY_MAIN_SALARY_SLOT : logicalMain
  MONTHLY_MAIN_SALARY_SLOT ||--o{ MAIN_SALARY_REVISION : hasVersions
  MAIN_SALARY_REVISION ||--o{ MAIN_REVISION_RECIPIENT_LINK : acceptedParts
  DISBURSEMENT_RECIPIENT ||--o{ MAIN_REVISION_RECIPIENT_LINK : sourceRecipient
  PAYROLL_CYCLE ||--o{ PAYROLL_GROUP : reviewGroups
  PAYROLL_GROUP ||--o{ MONTHLY_GROUP_MEMBERSHIP : historicalMembership
  EMPLOYMENT_RELATIONSHIP ||--o{ MONTHLY_GROUP_MEMBERSHIP : assigned
  PAYROLL_GROUP ||--o{ REVIEWER_ASSIGNMENT : reviewerScope
  ORGANIZATION_UNIT {
    uuid UnitId PK
    string InstitutionalCode
    string UnitType
  }
  PERSON {
    uuid PersonId PK
    string ProtectedIdentityRef
  }
  EMPLOYMENT_RELATIONSHIP {
    uuid EmploymentId PK
    uuid PersonId FK
    string EmployerUnitRef
    date ValidFrom
    date ValidToExclusive
  }
  PAYEE_RELATIONSHIP {
    uuid PayeeId PK
    uuid PersonId FK
    uuid EmploymentId FK "optional"
    string RelationshipKind
  }
  GRADE_HISTORY {
    uuid GradeHistoryId PK
    uuid EmploymentId FK
    string GradeCode
    date ValidFrom
    date ValidToExclusive
    datetime RecordedAt
    uuid SupersedesGradeHistoryId FK "optional"
  }
  APPROVED_FINANCIAL_FACT {
    uuid FactId PK
    uuid PayeeId FK
    string FactKind
    decimal Amount "optional"
    date ReferenceDate "optional"
    date ValidFrom
    date ValidToExclusive
    datetime RecordedAt
    string EvidenceRef
  }
  PAYROLL_CYCLE {
    uuid CycleId PK
    uuid UnitId FK
    string ProcessingPeriod
  }
  PAYROLL_RUN {
    uuid RunId PK
    uuid CycleId FK
    string RunPurpose
  }
  PAYROLL_DISBURSEMENT {
    uuid DisbursementId PK
    uuid RunId FK
    string DisbursementKind
    string OfficialReference "optional"
  }
  DISBURSEMENT_RECIPIENT {
    uuid RecipientId PK
    uuid DisbursementId FK
    uuid PayeeId FK
    string IdentityResolutionRef
  }
  MONTHLY_MAIN_SALARY_SLOT {
    uuid MainSlotId PK
    string AccountingUnitRef
    uuid EmploymentId FK
    string SalaryEntitlementMonth
  }
  MAIN_SALARY_REVISION {
    uuid MainRevisionId PK
    uuid MainSlotId FK
    int RevisionNumber
    uuid SupersedesRevisionId FK "optional"
    string AcceptanceDecisionRef
  }
  MAIN_REVISION_RECIPIENT_LINK {
    uuid MainRecipientLinkId PK
    uuid MainRevisionId FK
    uuid RecipientId FK
    string LinkKind
    string EvidenceRef
  }
  PAYROLL_GROUP {
    uuid GroupId PK
    uuid CycleId FK
    string GroupCode
  }
  MONTHLY_GROUP_MEMBERSHIP {
    uuid MembershipId PK
    uuid GroupId FK
    uuid CycleId FK
    uuid EmploymentId FK
    date ValidFrom
    date ValidToExclusive
    datetime RecordedAt
  }
  REVIEWER_ASSIGNMENT {
    uuid AssignmentId PK
    uuid GroupId FK
    string UserRef
    datetime EffectiveFrom
    datetime EffectiveTo "optional"
    string PermissionScope
  }
```

الصرفية رأس يجمع مستفيدين؛ فرادة المرتب لا توضع عليه كأنه صف موظف. `MainRevisionRecipientLink` يوضح ربط الأجزاء أو المستندات المكملة المقبولة بنفس النسخة المنطقية، مع دليل يمنع اعتماد تكرار كامل كجزء مكمل. الهوية المصدرية غير المحسومة تحفظ في ERD-B؛ لا تُنشأ لها علاقة صرف معتمدة لمجرد نجاح الاستخراج.

### D-039 — إسقاط ملف الموظف التاريخي دون اختراع جدول شامل

- `PERSON` يحتفظ بهوية الشخص، و`EMPLOYMENT_RELATIONSHIP` يميز علاقات عمله، و`PAYEE_RELATIONSHIP` علاقات الصرف. شاشة قائمة الموظفين تفتح ملف شخص/علاقة محددة بدل افتراض أن كل مستفيد مكافأة موظف بدرجة وظيفية.
- تبويب الترقيات والدرجات يستعين بـ`GRADE_HISTORY` وبالوقائع الإدارية المؤرخة، وتبويب المال يعرض الوقائع المالية من `APPROVED_FINANCIAL_FACT` والبنود من `FinancialComponent` والأثر التاريخي للقاعدة/القرار، لا أحدث قيمة لجميع السنوات.
- **تاريخ الأقسام والتنقلات:** يجب توفير علاقة انتماء لقسم/جهة بفترتي السريان وسبب الحركة ومرجع سندها؛ المخطط الحالي يثبت تاريخ الدرجة وعلاقة الجهة لكنه لا يثبت بعد شكل كيان تفاصيل العمل في الأقسام. يُراجع `StatusHistory` و`WorkArrangement` والهيكل التنظيمي قبل اقتراح جدول جديد.
- **الأحكام القضائية:** يجب حفظ مرجع الحكم وتاريخه وتاريخ السريان/الأثر، والقرار أو واقعة التطبيق المرتبطة به، والعلاقة بين كل حكم والبند المالي وفترة تعديله؛ لا يُفترض أن `AdministrativeDecision` وحده يمثل الحكم القضائي. لا تُستنتج مبالغ تنفيذ أو فروق تلقائيًا من وجود حكم.
- `EmployeeProfileReadModel` المقترح في UML قراءة مركبة من المراجع التاريخية مع اختيار «كما في تاريخ»، **وليس جدول بيانات مكررًا أو ترخيصًا لإنشاء أعمدة أو كيانات في SQL**. تراعي القراءة الصلاحيات والهوية والأصل ووقت تسجيل المعرفة.

### قيود ERD-A التي تتطلب تصميمًا فيزيائيًا

- فرادة `AccountingUnitRef × EmploymentId × SalaryEntitlementMonth` مرشح يطابق النطاق المعتاد؛ تعدد التعيين والنقل بين وحدتين سؤال مالك قبل تعميمه. لا تستبدله بفرادة عالمية للشخص من نفسك.
- إصدارات الرئيسي فريدة حسب `MainSlotId × RevisionNumber`، وله نسخة مقبولة فعالة واحدة وفق قرار صريح، لا أحدث ملف وصولًا.
- المنفصلة متعددة بلا قيد عام للشخص والشهر والغرض. تكرار نفس طلب الإدخال يعالج بمفتاح العملية، لا بتشابه المبلغ.
- عضوية المجموعة وتكليف المراجع مؤرخان. يجب تطابق `CycleId` للعضوية مع مجموعة الشهر، ومنع الفترات المتداخلة غير المصرح بها وفق سياسة العضوية؛ فهرس صف نشط وحده لا يثبت عدم تداخل التاريخ.
- لكل صف تاريخي زمن سريان وزمن معرفة ونسخة تصحيح. الحقول المرسومة حد أدنى للشرح؛ `SupersedesId` وحده لا يغني عن تحديد سياسة KnowledgeTime وقراءة اللقطة قبل التنفيذ.

## ERD-B — المصدر والالتقاط والأسطر

```mermaid
erDiagram
  SOURCE_CAPTURE_SESSION ||--o{ SOURCE_SNAPSHOT : capturedVersions
  SOURCE_DOCUMENT ||--o{ CAPTURE_DOCUMENT_LINK : archivedFile
  SOURCE_CAPTURE_SESSION ||--o{ CAPTURE_DOCUMENT_LINK : optionalFiles
  SOURCE_SNAPSHOT ||--o{ SOURCE_RECIPIENT_OCCURRENCE : observedRecipients
  SOURCE_RECIPIENT_OCCURRENCE ||--o{ SOURCE_FINANCIAL_OCCURRENCE : observedLines
  SOURCE_SNAPSHOT ||--o{ SOURCE_REPORTED_TOTAL : printedTotals
  SOURCE_SNAPSHOT ||--o{ SOURCE_VALIDATION_ISSUE : diagnostics
  SOURCE_RECIPIENT_OCCURRENCE ||--o{ RECIPIENT_MATCH_DECISION : identityDecisions
  SOURCE_FINANCIAL_OCCURRENCE ||--o{ COMPONENT_MAPPING_DECISION : mappingDecisions
  SOURCE_DOCUMENT {
    uuid SourceDocumentId PK
    string Sha256
    string PrivateArchiveRef
    string DocumentKind
    uuid SupersedesDocumentId FK "optional"
  }
  SOURCE_CAPTURE_SESSION {
    uuid CaptureId PK
    string CaptureMethod
    string CycleRef
    string ReportType
    string EnteredByRef
    string ConcurrencyToken
  }
  CAPTURE_DOCUMENT_LINK {
    uuid DocumentLinkId PK
    uuid CaptureId FK
    uuid SourceDocumentId FK
  }
  SOURCE_SNAPSHOT {
    uuid SourceSnapshotId PK
    uuid CaptureId FK
    int SnapshotVersion
    string EvidenceAuthenticity
    string TranscriptionAccuracy
    string SourceCoverage
    string ContentHash
    datetime CapturedAt
  }
  SOURCE_RECIPIENT_OCCURRENCE {
    uuid OccurrenceId PK
    uuid SourceSnapshotId FK
    string RawIdentityRef
    string IdentityStatus
  }
  SOURCE_FINANCIAL_OCCURRENCE {
    uuid SourceLineId PK
    uuid OccurrenceId FK
    string RawCode "optional"
    string RawLabel
    string RawAmountToken "optional"
    decimal ParsedAmount "optional"
    string FinancialSide
    string EvidenceLocator "optional"
  }
  SOURCE_REPORTED_TOTAL {
    uuid TotalId PK
    uuid SourceSnapshotId FK
    uuid OccurrenceId FK "optional"
    string TotalKind
    decimal Amount "optional"
    string EvidenceLocator
  }
  SOURCE_VALIDATION_ISSUE {
    uuid ValidationIssueId PK
    uuid SourceSnapshotId FK
    uuid SourceLineId FK "optional"
    string DiagnosticCode
    string Severity
  }
  RECIPIENT_MATCH_DECISION {
    uuid MatchDecisionId PK
    uuid OccurrenceId FK
    string PayeeRef "optional"
    string DecisionStatus
    string ApprovedByRef
  }
  COMPONENT_MAPPING_DECISION {
    uuid MappingDecisionId PK
    uuid SourceLineId FK
    string ComponentRef "optional"
    string DecisionStatus
    string EvidenceRef
  }
```

قد تكون جلسة شاشة بلا `CaptureDocumentLink` أصلًا؛ لا ينشأ ملف وهمي. تحفظ محاولات قراءة ملف واحد وإصدارات المحلل في سجل الالتقاط، ولا يستبدل Snapshot معتمد بإعادة قراءة أحدث. كل موضع سطر مستقل، ولا فرادة للكود أو للاسم أو للمبلغ. الإجماليات المطبوعة مختلفة عن مجاميع الأسطر المحسوبة، ويمكن أن تبقى غير متاحة.

قبل المخطط الفيزيائي يجب تحديد ربط Snapshot بعين ملفاتها ومحاولتها ومحركها، وربط الإجماليات الفردية بمستفيد من Snapshot نفسها؛ لا يجيز وجود المرجعين منفصلين خلط مصدرين. الصف المصدر بلا هوية داخلية يظل محفوظًا وقابلًا للفحص في نطاق المستند.

## ERD-C — الملاحظة والرد ونسخة الاعتماد

```mermaid
erDiagram
  AUDIT_CASE ||--o{ REVIEW_REVISION : reviewedVersions
  AUDIT_CASE ||--o{ FINDING : hasFindings
  FINDING ||--o{ FINDING_REVISION : historicalFinding
  FINDING ||--o{ FINDING_RESOLUTION_DELEGATION : overlappingActiveDelegationsAllowed
  FINDING_RESOLUTION_DELEGATION ||--o| FINDING_RESOLUTION_DELEGATION_REVOCATION : cancellationEvent
  FINDING_RESOLUTION_DELEGATION |o--o{ RESOLUTION_DECISION : delegatedDecisionBasis
  FINDING_REVISION ||--o{ REVIEW_MESSAGE : addressedMessages
  FINDING_REVISION ||--o{ RESOLUTION_DECISION : recordedHumanConclusions
  RESOLUTION_DECISION ||--o{ REVIEW_ADJUSTMENT : optionalOverlays
  REVIEW_REVISION ||--o{ REVIEW_SOURCE_LINK : frozenSourceRefs
  SOURCE_SNAPSHOT ||--o{ REVIEW_SOURCE_LINK : usedSource
  REVIEW_REVISION ||--o{ REVIEW_CALCULATION_LINK : frozenCalculationRefs
  EXPECTED_CALCULATION_RUN ||--o{ REVIEW_CALCULATION_LINK : usedCalculation
  REVIEW_REVISION ||--o{ REVIEW_FINDING_LINK : frozenFindingRefs
  FINDING_REVISION ||--o{ REVIEW_FINDING_LINK : reviewedFinding
  REVIEW_REVISION ||--o{ REVIEW_APPROVAL : authorizedSignatures
  REVIEW_REVISION ||--o{ REVIEW_COVERAGE_ITEM : explicitCoverage
  AUDIT_CASE {
    uuid CaseId PK
    string CycleRef
    string ScopeKind
    string RecipientRef "optional"
    string DisbursementRef "optional"
    string ReviewGroupRef "optional"
    string CaseVersion
  }
  REVIEW_REVISION {
    uuid ReviewRevisionId PK
    uuid CaseId FK
    int RevisionNumber
    string SourceSetVersion
    string ApprovalCoverageMode
    datetime KnownAt
    string SnapshotHash "optional until sealed"
    uuid SupersedesRevisionId FK "optional"
  }
  FINDING {
    uuid FindingId PK
    uuid CaseId FK
    string RaisedByReviewerRef
    string FindingVersion "concurrency guard reference"
  }
  FINDING_REVISION {
    uuid FindingRevisionId PK
    uuid FindingId FK
    string OriginReviewRevisionRef
    string SourceLineRef "optional"
    string ExpectedLineRef "optional"
    string ComponentRef "optional"
    string EntitlementPeriod "optional"
    decimal ProposedDifference "optional"
  }
  FINDING_RESOLUTION_DELEGATION {
    uuid ResolutionDelegationId PK
    uuid FindingId FK
    string OriginalFindingVersion
    string AssignedByRef
    string AssignedByRoleRef
    string AuthorizationScopeKind
    string AuthorizationScopeRef
    string AuthorizationScopeVersion
    string AssignedReviewerRef
    string ReasonKind
    datetime AssignedAt
  }
  FINDING_RESOLUTION_DELEGATION_REVOCATION {
    uuid RevocationId PK
    uuid ResolutionDelegationId FK
    string RevokedByRef
    string RevokedByRoleRef
    string RevocationScopeRef
    string RevocationScopeVersion
    datetime RevokedAt
    string Reason
  }
  REVIEW_MESSAGE {
    uuid MessageId PK
    uuid FindingRevisionId FK
    string PartyKind
    string MessageKind
    string OriginChannel
    string AuthenticatedAuthorRef "optional for external"
    string ClaimedExternalSenderRef "optional for in-app"
    string RecordedByRef
    string ExternalCorrespondenceRef "optional for in-app"
    datetime OccurredAt "optional if unknown"
    datetime RecordedAt
    string EvidenceRef "optional"
    string MessageContentRef
  }
  RESOLUTION_DECISION {
    uuid DecisionId PK
    uuid FindingRevisionId FK
    uuid SupportingMessageId FK "optional"
    string BasisFindingVersion
    string DecidedByReviewerRef
    datetime DecidedAt
    uuid ResolutionDelegationId FK "optional for assigned reviewer"
    string DecisionKind
    string Reason
    string AuthorityEvidenceRef
    uuid SupersedesDecisionId FK "optional"
  }
  REVIEW_ADJUSTMENT {
    uuid AdjustmentId PK
    uuid DecisionId FK
    string AdjustmentKind
    string ComponentRef
    string OriginalExpectedLineRef "optional"
    decimal ReviewedAmount "optional for exclusion"
  }
  REVIEW_SOURCE_LINK {
    uuid ReviewSourceLinkId PK
    uuid ReviewRevisionId FK
    uuid SourceSnapshotId FK
  }
  REVIEW_CALCULATION_LINK {
    uuid CalculationLinkId PK
    uuid ReviewRevisionId FK
    uuid CalculationRunId FK
  }
  EXPECTED_CALCULATION_RUN {
    uuid CalculationRunId PK
    string ApprovedFactsSnapshotRef
    string RuleSetVersionRef
    string InputHash
  }
  REVIEW_FINDING_LINK {
    uuid FindingLinkId PK
    uuid ReviewRevisionId FK
    uuid FindingRevisionId FK
    uuid ResolutionDecisionId FK "optional"
    string DecisionRevalidationRef "optional unless reused"
  }
  REVIEW_COVERAGE_ITEM {
    uuid CoverageItemId PK
    uuid ReviewRevisionId FK
    string SubjectKind
    string SubjectRef
    string ComponentRef "optional for group control"
    date From "optional if not period based"
    date ToExclusive "optional if not period based"
    string CoverageOutcome
    string BasisDecisionRef "optional"
    string ExclusionReasonRef "optional"
  }
  REVIEW_APPROVAL {
    uuid ApprovalId PK
    uuid ReviewRevisionId FK
    string SnapshotHash
    string AuthorityRole
    string ApprovedByRef
    datetime ApprovedAt
  }
  SOURCE_SNAPSHOT {
    uuid SourceSnapshotId PK
  }
```

### معنى النطاق واللقطة

- **D-030 — مصدر الرد:** `OriginChannel` يميز `IN_APP` (كتبه مستخدم المركزية بحسابه الموثق، بعد فحص سلطته) من `EXTERNAL_RECORDED` (وصل خارج التطبيق وسجله مستخدم الوحدة المخوّل). `RecordedByRef` هو حساب مَن أدخل السجل بالفعل، و`AuthenticatedAuthorRef` يشير إلى كاتب الرد الموثق داخل النظام فقط، بينما `ClaimedExternalSenderRef` و`ExternalCorrespondenceRef` يحفظان بيانات جهة الرد ومرجع مراسلته كما وردا دون ادعاء تحقق حساب مركزي. لا تستنتج قوة الإثبات من القناة وحدها.
- **قيود منطقية مقترحة:** في رد المركزية الداخلي لا يُقبل سجل بلا مرجع مستخدم موثق وصلاحية بالنطاق. في تسجيل الرد الخارجي لا يحل المرسل المنسوب محل `RecordedByRef` ولا يتحول إلى `AuthenticatedAuthorRef`. تظل متطلبات المرفق والتوقيع واعتماد التسجيل والصلاحيات التفصيلية قرارات مفتوحة؛ هذه ليست قيود DB منفذة.
- **D-031/D-032 — سلطة حسم الملاحظة والتكليف:** `FINDING.RaisedByReviewerRef` يبقى مرجع صاحب الملاحظة الأصلي. `RESOLUTION_DECISION.DecidedByReviewerRef` و`DecidedAt` يثبتان الشخص الذي حسمها وتوقيته: في المسار المعتاد يطابق الحاسم صاحبها؛ وعند غيابه أو نقله يجب أن يشير `ResolutionDelegationId` إلى تكليف صريح صالح يحدد `AssignedReviewerRef` و`AssignedByRef` و`ReasonKind` ومرجع الملاحظة الأصلي ونسختها. تُفحص هوية المسؤول المخوّل عند التكليف، وهوية البديل ونطاقه والإصدار عند الحسم. سجلات التكليف والقرارات تاريخية لا تستبدل بعضها، ولا تُنقل ملكية أصل الملاحظة إلى البديل. لا يلزم توقيع رئيس المراجعة **لحسم الملاحظة ذاته**؛ اعتماد `REVIEW_REVISION` وإثبات التسوية مستقلان. **D-033 حدد صفتَي المُصدر**: رئيس المراجعة أو رئيس الوحدة الحسابية، كلٌّ حسب اختصاصه. تحفظ `AssignedByRoleRef` الصفة التي مارس بها `AssignedByRef` سلطة التكليف، ويُحفظ `AuthorizationScopeRef` مرجع النطاق الذي أجاز التكليف وقت إصداره؛ يتحقق التطبيق من عضوية الملاحظة في هذا النطاق دون افتراض أن صلاحيات الوظيفتين متطابقة. **D-034 حسم حدود الإصدار:** `REVIEW_HEAD` يصدر التكليف فقط إن كانت الملاحظة داخل `PayrollGroup` تخضع لإشرافه في `PayrollCycle` المعنية، ويرفض التكليف خارج مجموعاته وللصرفيات المنفصلة التي لا تندرج في نطاق مجموعة مراجعة رئيسية؛ لا تُمنح سلطة على صرفية منفصلة بسبب عضوية مستفيدها في مجموعته. `ACCOUNTING_UNIT_HEAD` يصدر التكليف لملاحظات جميع مجموعات المراجعة والصرفيات التابعة للوحدة التي يرأسها، ويرفض ملاحظات أي وحدة أخرى. لتجنب اختيار صلاحية من لحظة غير صحيحة، تحفظ مسودة `FINDING_RESOLUTION_DELEGATION` نوع النطاق `AuthorizationScopeKind` ومرجعه `AuthorizationScopeRef` وإصداره `AuthorizationScopeVersion`، ويُحسم الربط المنطقي بمراجع `PayrollCycle` و`PayrollGroup` و`ReviewerAssignment` والكيان الفعلي للصرفية مع مراعاة سجل الإسناد الزمني. لا يغيّر هذا ملكية الملاحظة أو اعتماد `REVIEW_REVISION`. **D-035 حسم سلطة الإلغاء:** رئيس المراجعة يلغي أي تكليف داخل مجموعاته حتى لو أصدره رئيس الوحدة، ورئيس الوحدة يلغي أي تكليف داخل وحدته حتى لو أصدره رئيس المراجعة؛ لا يصح تجاوز هذه الحدود. `FINDING_RESOLUTION_DELEGATION_REVOCATION` سجل إلغاء مستقل يحفظ رابط التكليف، وهوية مُصدِر الإلغاء وصفته ونطاقه ووقت الإلغاء وسببه. حالة التكليف تُستنتج من وجود سجل الإلغاء الصالح لا من محو أو تحديث سجل التكليف التاريخي؛ يلزم ضمان عدم قبول حسم جديد بواسطة بديل بتكليف ألغي بالفعل، بما يشمل سباق طلبَي الحسم والإلغاء، دون إبطال `RESOLUTION_DECISION` صحيحة سابقة بأثر رجعي. **D-036/D-037 — التسجيل لا التحكيم:** يجوز تعدد التكليفات والآراء المختلفة للملاحظة/نسختها؛ لا قيد فريد على `FINDING_RESOLUTION_DELEGATION.FindingId` يمنعها. `REVIEW_MESSAGE.MessageKind` يميز رد المركزية عن رأي المراجع أو إفادة بالإجراء، ويحتفظ `RecordedByRef` وهوية قائله/كاتبه ووقت التسجيل ومرجع الدليل. `RESOLUTION_DECISION` يوثق خلاصة أو قرارًا بشريًا إذا ورد، مع حفظ الإصدارات السابقة؛ لا يرفض البرنامج تسجيل رأي مغاير لأن هناك رأيًا سابقًا. **التصرف الفعلي يُذكر فقط بقدر مصدره:** يظهر البلاغ أو كشف الصرف بوصفه `Reported`، وتُثبت التسوية أو الدفع فقط بدليل مستقل من طبقة `ExecutionEvidence/AllocationVerification` القائمة في ERD-D. لا يُنشأ محرك للتحكيم أو اختيار الرأي المفضل، ولا يُستنتج التنفيذ من اتفاق المراجعين. المتبقي من التزامن هو حماية سلامة التدوين من الكتابة فوق سجل آخر، لا فرض قرار نهائي آلي. لا DDL أو اختبار تشغيل حقيقي.
- `ScopeKind` يحدد مستفيدًا في صرفية أو صرفية كاملة أو مجموعة مراجعة. يجب وجود المرجع الموافق فقط، وانتماء المصادر والمستفيد والمجموعة للدورة والوحدة المصرح بهما. الحالة الجماعية لا تمنح مراجع مجموعة إذن الاطلاع على الجميع.
- `ReviewSourceLink` و`ReviewCalculationLink` و`ReviewFindingLink` تجمع النسخ التي يشملها اعتماد بعينه. تربط قرارات الحسم بنسخة الملاحظة الصحيحة؛ لا تستخدم أحدث رد بالوقت فقط.
- `ReviewCoverageItem` تحفظ مكونات النطاق التي فحصت والمعتمدة وغير المعتمدة والمستثناة وأسبابها، بما فيها البنود والمقاطع الزمنية. `ApprovalCoverageMode` يميز `WHOLE_SCOPE` عن `EXPLICIT_SUBSET` المسموح بسياسة مخولة. تدخل التغطية وأسبابها في البصمة وتظهر في التصدير، ولا يعرض مجموع أجزاء مقبولة باعتباره صافيًا معتمدًا كاملًا.
- كل SubjectRef وملاحظة وقرار في روابط اللقطة يجب أن ينتمي إلى الحالة ونطاقها. عند استمرار قرار قديم صالح في لقطة لاحقة، يبقى القرار القديم ثابتًا ويسجل `DecisionRevalidationRef` لفحص ملاءمته؛ لا ينقل من اللقطة السابقة ولا ينسخ تلقائيًا.
- الملاحظة قد تكون نقص دليل أو تصنيفًا أو فرقًا جماعيًا بلا مبلغ؛ الحقول المالية اختيارية بحسب نوعها، ولا تتحول `null` إلى صفر.
- عند اعتماد Revision تقفل مجموعة المراجع ومحتواها وتحفظ البصمة. الرسائل والقرارات والتعديلات المحفوظة ضمنها لا تتغير؛ تضاف نسخ أو عكس مسند. صلاحيات التوقيع الأول والثاني — إن تطلبتها سياسة الحالة — تحتاج مصفوفة معتمدة.
- يجب تنسيق تغييرات Finding وCaseVersion التي تؤثر في الاعتماد داخل حد اتساق معتمد. رد جديد لا يعاد استخدامه على مصدر لاحق بلا فحص صلاحية سنده والنطاق. لا يكفي فحص نسخة في الواجهة.
- عند مراجعة تعتمد كليًا على فحص يدوي مسند، قد لا يوجد CalculationRun؛ يجب إعلان ذلك وعدم تسميتها نتيجة حساب آلي. القبول الجزئي لا يختلق Expected للبنود المركزية أو المجهولة.

## ERD-D — الفروق والتأجيل ودليل التنفيذ

```mermaid
erDiagram
  RESOLUTION_DECISION ||--o{ FOLLOW_UP_ITEM : initiatesFollowUp
  FOLLOW_UP_ITEM ||--o{ CARRY_FORWARD_DECISION : targetPeriods
  FOLLOW_UP_ITEM ||--o{ EXECUTION_ALLOCATION : evidenceCoverage
  EXECUTION_EVIDENCE ||--o{ EXECUTION_EVIDENCE_LINE : sourceCapacity
  EXECUTION_EVIDENCE_LINE ||--o{ EXECUTION_ALLOCATION : partialAssignments
  EXECUTION_ALLOCATION ||--o{ ALLOCATION_VERIFICATION : verificationVersions
  EXECUTION_ALLOCATION o|--o{ EXECUTION_ALLOCATION : reversesPrior
  FOLLOW_UP_ITEM {
    uuid FollowUpId PK
    uuid ResolutionDecisionId FK
    string FinancialCaseKind
    string ObligationRef "optional until verified"
    decimal ConfirmedRequiredAmount "optional until verified"
    string Currency
    string OriginPeriod
    string ComponentRef "optional for group case"
    string AuthorizationEvidenceRef "optional while pending"
    string RecordVersion
  }
  CARRY_FORWARD_DECISION {
    uuid CarryDecisionId PK
    uuid FollowUpId FK
    decimal ProposedCarryAmount
    string TargetProcessingPeriod
    string Reason
    string AuthorizationEvidenceRef
    string ApprovedByRef
  }
  EXECUTION_EVIDENCE {
    uuid ExecutionEvidenceId PK
    string EvidenceKind
    string OriginalSourceRef
    string EvidencePolicyVersion
    string VerificationStatus
  }
  EXECUTION_EVIDENCE_LINE {
    uuid EvidenceLineId PK
    uuid ExecutionEvidenceId FK
    string PayeeRef
    string ComponentRef "optional"
    string OriginPeriod "optional if unallocated"
    decimal AvailableAmount
    string Currency
  }
  EXECUTION_ALLOCATION {
    uuid AllocationId PK
    uuid FollowUpId FK
    uuid EvidenceLineId FK
    decimal AllocationAmount
    string Currency
    string AllocationKind
    uuid ReversesAllocationId FK "optional"
    string CrossComponentAuthorizationRef "optional"
    string IdempotencyKey
  }
  ALLOCATION_VERIFICATION {
    uuid VerificationId PK
    uuid AllocationId FK
    string VerificationLevel
    string EvidenceRef
    string VerifiedByRef
    datetime RecordedAt
  }
  RESOLUTION_DECISION {
    uuid DecisionId PK
  }
```

`FollowUpItem` سجل متابعة لا ينشئ دينًا. إذا كان أصل استقطاع مثبتًا يشار إلى `DeductionObligation` الموجودة في التصميم، وإذا كان حقًا للمستفيد يشار إلى `RetroEntitlement`؛ نوع المرجع والتحقق من صاحبه يثبتان قبل القبول. عند عدم ثبوت الأصل يبقى المبلغ غير محسوم ولا يعرض رصيدًا قانونيًا مؤكدًا.

### ضبط المبلغ والازدواج

1. الظهور في Oracle والتخصيص إلى فرق والتحقق من التنفيذ مستويات مختلفة. لا تجمع قيمة التخصيص مرة باعتبارها ظهورًا ومرة باعتبارها تحققًا في إجمالي واحد.
2. عند ثبوت أصل مطلوب، يعرض المتبقي المبلّغ والمتبقي المتحقق كل بحسب مستواه. لا يفترض أن طرح كل ما ظهر في الكشف ينتج متبقيًا ثبت تنفيذه.
3. تمنع معاملة قاعدة البيانات المعتمدة تخصيص أكثر من سعة سطر الدليل أو أصل الالتزام. يلزم حارس على الجانبين، واختبار عمليتين متزامنتين، لا فحص `sum` ثم حفظ منفصل.
4. المقارنة على المستفيد والعملة والفترة والغرض والبند وسياسة الدليل. اختلاف البند يحتاج سند توافق للفترة والنوع، ولا يحله تطابق المبلغ أو باب الصرف.
5. عكس تخصيص يربط بالأصل والسبب والجهة المخولة ويمنع عكس أكثر مما خصص. تبقى عمليات العكس تاريخية، ولا تحذف اعتمادًا أو دليلًا. المبالغ السالبة الأصلية لا تُحذف أو تُحوّل إلى موجبة أثناء القراءة.
6. يجب أن تشير `AllocationVerification` إلى تخصيص صالح ودليل مناسب لنوع التنفيذ، مع نسخة سياسة وهوية المعتمد. القبول بتأكيد تسوية يتبع سياسة الجهة؛ لا يفرض دليل بنك واحدًا على كل نوع عمل دون قرار.

## ERD-E — الشجرة الحكومية ودليل بنود المرتبات والربط (D-038)

```mermaid
erDiagram
  GOVERNMENT_ACCOUNT_NODE o|--o{ GOVERNMENT_ACCOUNT_NODE : parentChild
  GOVERNMENT_ACCOUNT_NODE ||--o{ FINANCIAL_COMPONENT_ACCOUNT_MAPPING : mappedAccount
  FINANCIAL_COMPONENT ||--o{ FINANCIAL_COMPONENT_ACCOUNT_MAPPING : mappedComponent
  GOVERNMENT_ACCOUNT_NODE {
    string GovernmentAccountNodeId PK
    string OfficialCode
    string OfficialName
    string ParentNodeId FK "optional"
    date ValidFrom "optional"
    date ValidTo "optional"
  }
  FINANCIAL_COMPONENT {
    string FinancialComponentId PK
    string ComponentCode
    string ComponentName
    string ComponentKind
  }
  FINANCIAL_COMPONENT_ACCOUNT_MAPPING {
    string MappingId PK
    string FinancialComponentId FK
    string GovernmentAccountNodeId FK
    date ValidFrom "optional"
    date ValidTo "optional"
    string EvidenceRef "optional"
    string ApprovalStatus
  }
```

هذا رسم منطقي **غير منفذ وغير معتمد كجدول جديد**. `FINANCIAL_COMPONENT` يعيد استعمال مفهوم `FinancialComponent` الموجود في المواصفات؛ `GOVERNMENT_ACCOUNT_NODE` و`FINANCIAL_COMPONENT_ACCOUNT_MAPPING` اقتراحان مبدئيان لحفظ تصنيف حكومي مستقل عن بنود المرتبات وروابطها. لا يقرر هذا الرسم عدد الروابط المسموح لكل بند أو نطاق استعمالها (جهة/فترة/نوع صرف) ولا تفاصيل مستويات دليل الحسابات الرسمي؛ تُثبت من مصدر محاسبي معتمد. `SourceLine` أو كود Oracle الخام يظل سجل مصدر مستقلًا، وقد يلزم جدول ربط له بالبند طبقًا لدليل البيانات القائم، **لا يُدمج آليًا مع عقدة الحساب**. مثال `PAY-ITEM-01` ⇄ `GOV-ACC-01` رموز تجريبية وليست أكوادًا حكومية حقيقية. عرض الرابط لا يعني صلاحية إنشاء أو تنفيذ قيد محاسبي.

## مرجع الحساب والتاريخ خارج الرسوم التفصيلية

يستعمل هذا النموذج كيانات `FinancialComponent`, `ComponentAuditPolicy`, `LegalProvisionVersion`, `FinancialRuleVersion`, `RuleEffect`, `ExpectedCalculationRun/Line`, `StatusHistory`, `WorkArrangement`, `AdministrativeDecision` الموجودة في ERD والمواصفات الأصلية. لا يعيد إنشاء بدائل لها. قائمة البنود ونتائج الحساب تحتاج مفتاح علاقة صرف وبند وفترة ومراجع قاعدة ووقائع وتقريب، وتبقى البنود المصدرية دون Expected مستقل.

## قاموس حقول مشترك وقرارات قبل SQL

| المفهوم | ما يحفظ | ما يجب حسمه قبل التنفيذ |
| --- | --- | --- |
| المبلغ | Decimal وعملة، والسالب الخام بجانبه ونوعه | الدقة ومقياس التخزين والتقريب لكل قاعدة وحدودها |
| التاريخ | السريان وفترة الاستحقاق والعلم والتسجيل والمعالجة والتنفيذ مستقلة | معنى الحدود والمنطقة الزمنية والدقة والفترات المفتوحة |
| الدليل | مرجع أصل مؤمن وبصمة وموضع، أو شهادة معاينة مخولة بمستواها | أنواع الأدلة المقبولة والاحتفاظ والمرفقات والسلطة |
| النسخة | رقم/رمز تزامن ومراجع منسوخة بالمعنى وبصمة لقطة | متى تقفل وكيف تقرأ كما كانت معلومة وتتعقب التصحيح |
| السلطة | مستخدم وفعل ونطاق ووقت وتفويض عند اللزوم | الأدوار والتوقيعات والفصل بين محرر ومعتمد |
| الهوية | معرف داخلي ومرجع هوية محمي وقرارات ربط | معيار المطابقة وعلاقات الصرف المشروعة والاستثناءات |
| العملية المتكررة | مفتاح طلب ونطاقه ونتيجة التنفيذ | مدة المفتاح وقواعد إعادة المحاولة واستئناف الفشل |

يلزم في التصميم الفيزيائي: عدم حذف أدلة واعتمادات بالتسلسل، والتحقق من FK المركبة التي تمنع خلط دورة أو مستفيد أو Snapshot، وفهارس للاستعلام والتفويض، وسياسة تشفير ونسخ احتياطي واستعادة، وحدود معاملات واختبارات تزامن. ترسم هذه الوثيقة العلاقات المطلوبة؛ لا تعلن أن القيود نافذة قبل تنفيذها واختبارها.
