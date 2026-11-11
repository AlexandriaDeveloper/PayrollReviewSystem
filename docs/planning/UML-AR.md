# مخططات UML لدورة مراجعة المرتبات

**الحالة: نموذج تصميم مقترح v0.3 — تعديلات D-030 إلى D-034 قيد المراجعة، لا كود منفذ.** يكمّل [الخطة](COMPREHENSIVE-WORK-PLAN-AR.md) و[ERD](DATABASE-DIAGRAM-AR.md). الأسماء والعلاقات التالية تحتاج مراجعة وحدود معاملات وعقود قبل التنفيذ.

هذه رسوم Class وSequence وState. أسهم التعاون في Sequence تصف حالة استخدام مقترحة، ولا تعني اتصالًا فعليًا بـOracle أو تنفيذ دفع. القيم الاصطناعية الواردة في الشرح ليست أحكامًا مالية.

## UML-1 — الكيانات والعمليات والعقود

`AuditCase` يضبط نطاق الاعتماد ونسخه؛ `Finding` يضبط الملاحظة ونسخها والردود عليها؛ `FollowUpItem` يتابع أثرًا اتفق على متابعته. الفصل يمنع تحول حسم ملاحظة إلى إثبات تنفيذ. أسماء حدود Aggregates مرشحة وتخضع لاختبار الاتساق.

```mermaid
classDiagram
  direction TB
  class AuditCase {
    +CaseId
    +ScopeKind
    +CaseVersion
    +OpenRevision()
    +ApproveRevision(snapshotHash, expectedVersion)
    +RequestRevalidation(reason)
  }
  class ReviewRevision {
    +RevisionId
    +SourceSetVersion
    +CalculationSetRef
    +DecisionSetRef
    +CoverageSetRef
    +ApprovalCoverageMode
    +KnownAt
    +SnapshotHash
  }
  class Finding {
    +FindingId
    +CaseId
    +RaisedByReviewerRef
    +FindingVersion
    +RecordReply(command, expectedVersion)
    +ResolveByOwner(command, expectedVersion)
  }
  class FindingRevision {
    +FindingRevisionId
    +ReviewRevisionId
    +ComponentRef
    +EntitlementPeriod
    +ProposedDifference
  }
  class FindingResolutionDelegation {
    +ResolutionDelegationId
    +FindingId
    +OriginalFindingVersion
    +AssignedByRef
    +AssignedByRoleRef
    +AuthorizationScopeKind
    +AuthorizationScopeRef
    +AuthorizationScopeVersion
    +AssignedReviewerRef
    +ReasonKind
    +AssignedAt
    +RevokedAt_optional
  }
  class ReviewMessage {
    +MessageId
    +RespondsToFindingRevisionId
    +PartyKind
    +OriginChannel
    +AuthenticatedAuthorRef_optional
    +ClaimedExternalSenderRef_optional
    +ExternalCorrespondenceRef_optional
    +RecordedByRef
    +OccurredAt_optional
    +RecordedAt
    +EvidenceRef_optional
  }
  class ResolutionDecision {
    +DecisionId
    +FindingRevisionId
    +DecidedByReviewerRef
    +DecidedAt
    +ResolutionDelegationId_optional
    +SupportingMessageId_optional
    +DecisionKind
    +Reason
    +EvidenceRef
    +SupersedesDecisionId
  }
  class ReviewApproval {
    +ApprovalId
    +ReviewRevisionId
    +SnapshotHash
    +ApprovedBy
    +ApprovedAt
  }
  class FollowUpItem {
    +FollowUpId
    +ResolutionDecisionId
    +ConfirmedAmount
    +OriginPeriod
    +RecordCarryForward()
    +AllocateEvidence(command, expectedVersion)
    +RequestRecheck(reason)
  }
  class ISourceIntake {
    <<interface>>
    +Capture(input, context) CapturedSource
  }
  class IReviewAuthorization {
    <<interface>>
    +CanPerform(actor, scope, action) PermissionResult
  }
  class IExpectedCalculator {
    <<interface>>
    +Propose(approvedSnapshot) CalculationResult
  }
  class IExecutionEvidenceVerifier {
    <<interface>>
    +Verify(evidence, policy) VerificationResult
  }
  class ReviewUseCase {
    +RecordReply(command)
    +AssignFindingResolutionDelegate(command)
    +ResolveFindingByAuthorizedReviewer(command)
    +ApproveReview(command)
  }
  class PdfSourceAdapter
  class ExcelSourceAdapter
  class DirectFormAdapter
  class Money {
    <<ValueObject>>
    +DecimalAmount
    +Currency
  }
  AuditCase "1" *-- "1..*" ReviewRevision : versions
  AuditCase "1" --> "0..*" Finding : references
  Finding "1" *-- "1..*" FindingRevision : versions
  Finding "1" --> "0..*" FindingResolutionDelegation : resolutionAssignments
  FindingRevision "1" --> "0..*" ReviewMessage : addressedReplies
  FindingRevision "1" --> "0..*" ResolutionDecision : decisions
  ResolutionDecision "0..*" --> "0..1" FindingResolutionDelegation : delegatedAuthority
  ReviewRevision "0..*" --> "0..*" ResolutionDecision : frozenDecisionRefs
  ReviewRevision "1" --> "0..*" ReviewApproval : approvals
  ResolutionDecision "1" --> "0..*" FollowUpItem : authorizedFollowUp
  FollowUpItem --> Money : amount
  ReviewUseCase ..> IReviewAuthorization
  ReviewUseCase ..> IExpectedCalculator
  ReviewUseCase ..> IExecutionEvidenceVerifier
  ReviewUseCase ..> AuditCase
  ReviewUseCase ..> Finding
  PdfSourceAdapter ..|> ISourceIntake
  ExcelSourceAdapter ..|> ISourceIntake
  DirectFormAdapter ..|> ISourceIntake
```

التكوين `*--` يعني ملكية نسخة داخل الكيان، والسهم `-->` مرجع لا تصريح كتابة في سياق آخر، والسهم المنقط تبعية أو تحقيق عقد. لا تعني أي علاقة تحميل كل السجل في الذاكرة. يجوز أن تتضمن عدة لقطات قرار حسم ثابتًا من الحالة نفسها إذا أعيد التحقق من صلاحيته وسجل مرجع ذلك الفحص؛ يختلف ذلك عن نقل القرار من لقطة قديمة أو إعادة استعماله بصمت. تحوي CoverageSetRef البنود والمقاطع المعتمدة وغير المعتمدة وأسبابها ضمن البصمة؛ الاعتماد الجزئي يحتاج سياسة مخولة ولا يسمى اعتمادًا شاملًا. `Money` يحفظ القيمة والعملة؛ سياسة التقريب مؤرخة في عقد الحساب، ولا تُختزل في تقريب عام لكل المبالغ.

## UML-2 — الاستلام والملاحظة والرد والحسم

```mermaid
sequenceDiagram
  autonumber
  actor Reviewer as المراجع الأصلي
  actor ReviewHead as رئيس المراجعة
  actor UnitHead as رئيس الوحدة الحسابية
  actor DelegateReviewer as المراجع البديل
  actor CentralUser as مستخدم المركزية
  participant UI as واجهة المراجعة
  participant Intake as استقبال المصدر
  participant Audit as حالة استخدام المراجعة
  participant Central as مراسلات المركزية الخارجية
  participant Follow as متابعة الفروق
  Reviewer->>UI: استقبال ملف أو إدخال بديل
  UI->>Intake: Capture مع هوية الطلب والسياق
  Intake-->>UI: نسخة مصدر محفوظة وحالات الجودة والتغطية
  Reviewer->>Audit: تثبيت النطاق والملاحظة وأدلتها
  Audit-->>Reviewer: ملاحظة بإصدار ومرجع نسخة المصدر
  Reviewer->>Central: إبلاغ وفق قناة العمل المخولة
  Reviewer->>Audit: تسجيل الإبلاغ ومرجعه
  alt رد مباشر داخل النظام
    CentralUser->>UI: رد مرتبط بإصدار الملاحظة ومستنداته
    UI->>Audit: تسجيل رد مع هوية الحساب المركزي الموثقة
    Audit->>Audit: فحص صلاحية الكاتب ونطاق الملاحظة
  else تسجيل رد وصل خارج النظام
    Central-->>Reviewer: رد عبر مراسلة أو مستند رسمي مصحح
    Reviewer->>UI: إدخال الرد ومرجع المراسلة ودليله المتاح
    UI->>Audit: تسجيل الرد مع هوية مُدخله والمرسل المنسوب
    Audit->>Audit: منع نسبة الإدخال لحساب مركزي لم يدخل النظام
  end
  Audit->>Audit: فحص النسخة والأدلة دون حسم تلقائي
  alt صاحب الملاحظة متاح
    Reviewer->>Audit: طلب حسم بصفته صاحب الملاحظة مع دليل وسبب وإصدار
    Audit->>Audit: تحقق من RaisedByReviewerRef وهوية الطالب وصلاحيته والنطاق والنسخة
  else صاحب الملاحظة غائب أو منقول
    alt رئيس المراجعة يصدر التكليف
      ReviewHead->>Audit: تكليف بديل لملاحظة ضمن مجموعة تحت إشرافه
      Audit->>Audit: التحقق من إشرافه على مجموعة الملاحظة في الدورة المناسبة
    else رئيس الوحدة الحسابية يصدر التكليف
      UnitHead->>Audit: تكليف بديل لمجموعة أو صرفية منفصلة داخل وحدته
      Audit->>Audit: التحقق من وحدة الملاحظة وحدود وحدة المسؤول
    end
    Audit->>Audit: رفض تكليف رئيس مراجعة خارج مجموعاته أو لصرفية منفصلة
    Audit->>Audit: رفض تكليف رئيس وحدة لملاحظة من وحدة أخرى
    Audit->>Audit: حفظ هوية المُصدِر وصفته ونطاقه وتوقيت التكليف دون تغيير الأصل
    DelegateReviewer->>Audit: حسم باسم البديل مع مرجع التكليف والدليل وإصدار الملاحظة
    Audit->>Audit: تحقق من صلاحية تكليف البديل وحالته والنطاق والإصدار
  end
  alt ثبتت سلامة الملف وخطأ الملاحظة
    Audit-->>Reviewer: حفظ سحب الملاحظة بقرار صاحبها دون فرق مرحل
  else اتفاق على فرق وتصحيح مصدر
    Reviewer->>Intake: حفظ التصحيح الرسمي كنسخة جديدة
    Intake-->>Audit: مرجع النسخة والأجزاء المتغيرة
    Audit-->>Reviewer: إعادة فحص الأثر قبل اعتماد نسخة المراجعة
  else فرق مثبت مع تأجيل أو تسوية لاحقة مسموحة
    Reviewer->>Audit: توثيق أصل الفرق وقراره بصفته صاحب الملاحظة
    Audit->>Follow: طلب إنشاء متابعة وفق سند وسلطة منفصلين دون تنفيذ خصم
    Follow-->>Reviewer: أصل الفترة والهدف والمتبقي وحالة الدليل
  else الرد ناقص أو الخلاف مستمر
    Audit-->>Reviewer: انتظار دليل أو تصعيد للنطاق المتأثر
  end
  Note over Audit,Follow: يحسم صاحب الملاحظة أو بديل مكلف رسميًا باسمه دون محو الأصل؛ اعتماد الصرفية والتنفيذ مستقلان
```

**ضبط D-030:** يُميز `OriginChannel=IN_APP` عن `EXTERNAL_RECORDED`. في الرد الداخلي تُحفظ هوية الحساب الذي كتب الرد بالفعل، وفي الرد الخارجي تُحفظ هوية مستخدم الوحدة الذي نقله وبيانات مرسله المنسوبة دون انتحال حسابه. لا تفرض هذه المسودة إتاحة كل ملاحظة لجميع مستخدمي المركزية؛ صلاحية الاطلاع والرد وشروط إثبات المراسلة مسائل مفتوحة. لا يغيّر هذا القرار حالات UML-4 أو UML-5: الحسم والتنفيذ لا يحدثان عند تسجيل رد.

**ضبط D-031/D-032:** الأصل أن يحسم صاحب الملاحظة برد مسند؛ عند الغياب أو النقل يختار المسؤول المخوّل مراجعًا بديلًا لملاحظة محددة، فيحسم البديل باسمه مستخدمًا تكليفًا صالحًا مع حفظ اسم صاحب الملاحظة. يفحص التطبيق `RaisedByReviewerRef` أو `FindingResolutionDelegation`، وهوية الحاسم، وسلطة مصدر التكليف، والنسخة والنطاق والدليل. التكليف لا يغير `RaisedByReviewerRef` ولا يمنح `ReviewApproval`. **D-033/D-034:** مُصدر التكليف هو رئيس المراجعة **في مجموعات إشرافه فقط** أو رئيس الوحدة الحسابية **في مجموعات المراجعة والصرفيات داخل وحدته**. يسجل التكليف الصفة و`AuthorizationScopeKind` و`AuthorizationScopeRef` و`AuthorizationScopeVersion` للتحقق من العلاقة المعتمدة عند الإصدار والاستعمال؛ لا يُسمح للرئيس الأول بتكليف صرفية منفصلة خارج نطاق المجموعات ولا للرئيس الثاني بتكليف وحدة أخرى. حدود إلغاء التكليف وإعادة إصداره ما زالت مفتوحة.

## UML-3 — اعتماد نسخة وتصحيح لاحق

```mermaid
sequenceDiagram
  autonumber
  actor Approver as المعتمد المخول
  participant Audit as مراجعة الصرفية
  participant Registry as عقد نسخ المصدر المقبولة
  participant Store as تخزين القرار واللقطة
  participant Intake as استقبال التصحيح
  Approver->>Audit: معاينة الاعتماد لنطاق محدد
  Audit->>Registry: قراءة SourceSetVersion ومراجع النسخ المقبولة
  Registry-->>Audit: المراجع وإصدار المجموعة
  Audit-->>Approver: SnapshotHash وCaseVersion ونواقص النطاق
  Approver->>Audit: طلب اعتماد بنفس البصمة والإصدارات
  Audit->>Audit: تحقق السلطة والشروط ونفس مجموعة القرارات
  Audit->>Registry: تحقق أن المصدر المقبول لم يتغير
  alt تغير مصدر مقبول أو قرار مؤثر أو نسخة الحالة
    Audit-->>Approver: إعادة معاينة، دون نجاح اعتماد قديم
  else اللقطة مطابقة وشروط الاعتماد مستوفاة
    Audit->>Store: حفظ اعتماد ولقطة ثابتة بمعاملة معتمدة
    Store-->>Approver: اعتماد مراجعة هذا النطاق والإصدار
  end
  Intake->>Audit: وصول تصحيح مرشح متعلق بنطاق سبق اعتماده
  Audit->>Store: سجل أثر ووضع الحاجة للفحص، دون محو الاعتماد
  Approver->>Audit: فحص التصحيح وقبول ربطه وفق الصلاحية
  Audit->>Store: نسخة مراجعة جديدة ومراجع مصدر جديدة
  Audit-->>Approver: إعادة تحقق من القرارات والردود والتعديلات المتأثرة
  Note over Registry,Store: التحقق والحفظ يحتاجان ضمان اتساق، لا فحصًا منفصلًا يعقبه سباق
```

إجراء المعاملة واتساق عقد المصدر والاعتماد لم يُحسم تقنيًا. قبل التنفيذ يلزم تصميم يضمن أن قبول مصدر جديد أو قرار مؤثر لا ينجح بين فحص النسخة وحفظ الاعتماد دون اكتشافه؛ يمكن أن يحتاج إجراء Application ومعاملة مشتركة معتمدة داخل التطبيق أو بروتوكول حجز/تحقق مكافئ. لا يفسر الرسم استدعاء `Check` ثم `Save` مستقلين بوصفه ضمانًا ذريًا.

## UML-4 — حالة حسم الملاحظة

```mermaid
stateDiagram-v2
  [*] --> Detected
  Detected --> UnderReview: تثبيت النطاق والتحقق
  UnderReview --> WaitingEvidence: مستند أو مدخل ناقص
  WaitingEvidence --> UnderReview: استيفاء البيانات
  UnderReview --> WaitingReply: إبلاغ ملاحظة وتسجيل المرجع
  WaitingReply --> ReplyReview: وصول رد مرتبط بإصدار
  ReplyReview --> WaitingEvidence: دليل الرد غير كاف
  ReplyReview --> Escalated: استمرار خلاف
  Escalated --> ReplyReview: قرار أو توضيح مختص
  ReplyReview --> SourceCorrect: حسم مخول مع دليل وإصدار
  ReplyReview --> DifferenceConfirmed: حسم فرق بواسطة مراجع مخول بسند
  UnderReview --> Withdrawn: سحب بسبب مسند بواسطة مراجع مخول
  SourceCorrect --> [*]
  Withdrawn --> [*]
  DifferenceConfirmed --> [*]
  Note right of DifferenceConfirmed: قد تبقى متابعة تصحيح أو تنفيذ مفتوحة
```

تجتاز انتقالات الحسم فحص هوية صاحب الملاحظة `RaisedByReviewerRef` أو مرجع تكليف البديل وسلطة مُصدره عند الغياب/النقل، مع نطاق الصلاحية ونسخة الملاحظة؛ لا يحتاج **حسم الملاحظة ذاته** إلى توقيع رئيس إضافي وفق D-031/D-032. النتائج تخص الحسم الموضوعي للملاحظة، وليست تصريح إغلاق كل مراجعة الصرفية. إذا وصل دليل يغير حسمًا قديمًا، تحفظ نسخة القرار السابقة وتفتح نسخة فحص جديدة مرتبطة بها؛ لا تُمسح النهاية التاريخية.

## UML-5 — حالة متابعة الفرق مستقلة

```mermaid
stateDiagram-v2
  [*] --> Candidate
  Candidate --> WaitingAuthorization: أصل المبلغ أو الإجراء يحتاج سندا
  WaitingAuthorization --> AuthorizedOpen: ثبت الأصل وجواز المتابعة
  AuthorizedOpen --> Deferred: قرار هدف لشهر لاحق
  Deferred --> Reported: ظهر مبلغ مصدر مرشح
  AuthorizedOpen --> Reported: ظهر مبلغ مصدر مرشح
  Reported --> WaitingProof: الظهور لا يثبت التنفيذ
  Reported --> RecheckRequired: قرار يصحح الأصل أو الدليل
  WaitingProof --> RecheckRequired: إلغاء أو تصحيح قبل تحقق التنفيذ
  WaitingProof --> PartlyVerified: تخصيص جزئي بدليل مقبول
  AuthorizedOpen --> PartlyVerified: دليل مستقل يثبت جزءا
  PartlyVerified --> PartlyVerified: تخصيص إضافي ضمن الحدود
  WaitingProof --> FullyVerified: الدليل يغطي كامل المطلوب
  PartlyVerified --> FullyVerified: اكتمل الباقي المثبت
  AuthorizedOpen --> FullyVerified: دليل مستقل يغطي الكامل
  AuthorizedOpen --> CancelledByDecision: إلغاء الأصل بقرار مختص
  Deferred --> CancelledByDecision: إلغاء الأصل بقرار مختص
  FullyVerified --> RecheckRequired: تصحيح دليل أو قرار لاحق
  PartlyVerified --> RecheckRequired: تصحيح دليل أو قرار لاحق
  RecheckRequired --> AuthorizedOpen: نسخة متابعة مصححة تبين الرصيد
  RecheckRequired --> CancelledByDecision: إلغاء مسند مع حفظ كل الأثر السابق
  FullyVerified --> [*]
  CancelledByDecision --> [*]
  Note right of RecheckRequired: يحفظ القديم وتضاف نسخة أو عكس مسند
```

عند إثبات أصل مطلوب 200، وظهور 100 بلا دليل تنفيذ مقبول، لا يصبح المتبقي المثبت 100 تلقائيًا. يعرض البرنامج ما ظهر وما ثبت تنفيذه كلًا على حدة. الإلغاء أو عكس تخصيص بعد تحقق جزئي يحتاج معالجة نسخ وأثر المبالغ السابقة، ولا يستخدم لطمس ما ثبت وقوعه.
