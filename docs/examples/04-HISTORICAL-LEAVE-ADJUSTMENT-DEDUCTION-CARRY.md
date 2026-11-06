# أمثلة ERD وUML: إجازة بدرجة تاريخية، تعديل البنود، وترحيل خصم ناقص

> **كل الأمثلة اصطناعية بالكامل** لتوضيح التصميم، وليست قرارات حقيقية أو نسب أبحاث أو أحكام إجازات أو إذنًا بخصم/مقاصة. الكلاسات وحقول JSON عقود مقترحة وليست API موجودة. المرجع: [مواصفة الحالات الثلاث](../specs/HISTORICAL-LEAVE-REVIEW-ADJUSTMENTS-AND-DEDUCTION-CARRY.md).

## 1. الحالة الأولى — إجازة عن فترة مدرس، والدرجة الحالية أستاذ مساعد

### شكل الإدخال
مستفيد افتراضي يعمل في كلية الطب. وصلت في نوفمبر 2026 إجازة تخص فبراير 2025. درجته في فبراير 2025 مدرس، ثم أصبح أستاذًا مساعدًا في تاريخ لاحق. *للتجربة فقط*: نسبة الأبحاث 60% في فترة المدرس، و48% في فترة الأستاذ المساعد، ووعاء الحساب 1000 جنيه إذا افترضنا صحة تلك القواعد.

~~~json
{
  "payeeRelationshipId": "PAY-TEST-01",
  "employmentRelationshipId": "EMP-TEST-01",
  "incomingDecision": {
    "decisionId": "LEAVE-TEST-01",
    "kind": "ANNUAL_LEAVE",
    "eventPeriod": {"from": "2025-02-10", "toExclusive": "2025-02-15"},
    "recordedAt": "2026-11-07T10:00:00+02:00",
    "evidenceStatus": "VERIFIED_IN_TEST"
  },
  "gradeHistory": [
    {"grade": "LECTURER_TEST", "from": "2024-01-01", "toExclusive": "2025-10-01"},
    {"grade": "ASSISTANT_PROFESSOR_TEST", "from": "2025-10-01", "toExclusive": null}
  ],
  "mockResearchRules": [
    {"forGrade": "LECTURER_TEST", "rate": "0.60", "validFrom": "2024-01-01", "validToExclusive": "2025-10-01"},
    {"forGrade": "ASSISTANT_PROFESSOR_TEST", "rate": "0.48", "validFrom": "2025-10-01", "validToExclusive": null}
  ],
  "baseAmountForIllustration": "1000.00",
  "ruleTimeBasis": "EVENT_PERIOD",
  "gradeReferenceDatePolicy": "EVENT_PERIOD",
  "calculationMode": "PROPOSAL_ONLY"
}
~~~

### شكل المخرجات المقترح

~~~json
{
  "decisionId": "LEAVE-TEST-01",
  "eventPeriod": "2025-02-10/2025-02-15",
  "currentGradeForInformation": "ASSISTANT_PROFESSOR_TEST",
  "resolvedHistoricalGrade": "LECTURER_TEST",
  "selectedTestRate": "0.60",
  "illustrativeFullPeriodMathOnly": "1000.00 x 0.60 = 600.00",
  "amountApprovedForPayment": null,
  "auditStatus": "LEGAL_FORMULA_AND_LEAVE_IMPACT_NOT_VERIFIED",
  "requires": [
    "القانون/اللائحة المعتمدة لنوع الإجازة",
    "تاريخ مرجع درجة بند الأبحاث",
    "هل الإجازة تؤثر على البند أصلًا",
    "طريقة احتساب أيام الإجازة"
  ]
}
~~~

**ماذا لو تغيرت الدرجة داخل الإجازة؟** لو كانت الإجازة من 28 سبتمبر إلى 3 أكتوبر 2025، ينشئ المحرك مقطعين عند 1 أكتوبر **إذا نصت القاعدة على الاحتساب اليومي حسب الدرجة**. أما لو المرجع هو درجة تاريخ واحد محدد فلا تقسيم تعسفي.

## 2. الحالة الثانية — شاشة المراجع قبل وبعد تغيير ثلاثة بنود

### نتيجة حساب البرنامج الأصلية

| البند | `Expected` | `Source` | الحالة |
| --- | ---: | ---: | --- |
| أبحاث اختبارية `COMP-RES` | 600 | 600 | محسوب للمراجعة |
| بدل اختباري `COMP-ALLOW` | 300 | 300 | محسوب للمراجعة |
| حافز اختباري `COMP-INC` | — | — | غير موجود |
| **إجمالي المتوقع الأصلي** | **900** | | |

### طلبات المراجع — تختلف عن تعديل PDF

~~~json
{
  "reviewCaseId": "CASE-TEST-01",
  "expectedRunId": "CALC-TEST-01",
  "reviewRevision": 3,
  "requestedBy": "REVIEWER-TEST-01",
  "adjustments": [
    {
      "adjustmentType": "AMOUNT_OVERRIDE",
      "originalExpectedLineId": "EXP-RES-01",
      "componentId": "COMP-RES",
      "newReviewedAmount": "550.00",
      "reason": "مثال: قرار مالي مستقل تم التحقق منه",
      "evidenceRef": "TEST-EVIDENCE-A"
    },
    {
      "adjustmentType": "EXCLUDE_COMPONENT",
      "originalExpectedLineId": "EXP-ALLOW-01",
      "componentId": "COMP-ALLOW",
      "reason": "مثال: انطباق البند على الفئة لم يثبت",
      "evidenceRef": "TEST-EVIDENCE-B"
    },
    {
      "adjustmentType": "ADD_EXISTING_COMPONENT",
      "componentId": "COMP-INC",
      "newReviewedAmount": "200.00",
      "reason": "مثال: بند معروف له قرار مالي خاص",
      "evidenceRef": "TEST-EVIDENCE-C"
    }
  ],
  "expectedCaseVersion": "v3"
}
~~~

### شاشة عرض التغييرات

| البند | حساب البرنامج | قرار المراجع | قيمة المراجعة المقترحة |
| --- | ---: | --- | ---: |
| الأبحاث | 600 | تعديل مع إثبات | 550 |
| البدل | 300 | استبعاد من الاستحقاق | 0 في **تجميع المراجعة فقط** |
| الحافز | غير موجود | إضافة بند معروف | 200 |
| **الإجمالي** | **900** | | **750** |

في قاعدة البيانات: **لا نضع `SourceAmount=0` ولا نحذف `ExpectedLine`**. عملية الاستبعاد تحمل `Status=Excluded` والمبلغ الأصلي 300 محفوظ. الإجمالي 750 **مقترح من قرارات المراجع حتى اعتمادها**، وليس نسخة من بيانات Oracle أو كشف دفع.

### بعد إعادة حساب لاحقة
لو وصل تصحيح درجة أو قانون أثره على `EXP-RES-01`، يصبح تعديل 550 مرتبطًا بلقطة `CALC-TEST-01` القديمة. النسخة الجديدة تعرض `REVALIDATION_REQUIRED` للمراجع بدل تكرار 550 أو إلغائه بلا علم.

## 3. الحالة الثالثة — 1000 جنيه خصم مطلوب، 800 ظاهر، وترحيل 200

### أكتوبر: فرق خصم مرشح

~~~json
{
  "obligationId": "OBL-TEST-01",
  "payeeRelationshipId": "PAY-TEST-01",
  "deductionType": "TEST_AUTHORIZED_DEDUCTION",
  "originalEntitlementPeriod": "2026-10",
  "authorizedRequiredAmount": "1000.00",
  "evidenceRef": "TEST-OFFICIAL-OBLIGATION",
  "oracleOccurrence": {
    "sourceLineId": "SRC-DEDUCT-OCT-01",
    "reportPeriod": "2026-10",
    "reportedDeduction": "800.00",
    "sourceComponentId": "COMP-ORIGINAL"
  },
  "candidateDifference": "200.00",
  "carryForward": {
    "targetPeriod": "2026-11",
    "status": "PENDING_LEGAL_AND_REVIEW_APPROVAL"
  }
}
~~~

**مثال المجموعة:** لو أصل الالتزام 1000 يتكون من بند أ = 600، وبند ب = 400، بينما PDF يذكر إجمالي تحصيل 800 دون تفصيل، يمكن اكتشاف فرق إجمالي 200 **ولا يجوز الجزم أنه يخص البند أ أو ب**. واجهة المراجع تظهر `COMPONENT_ATTRIBUTION_PENDING`.

### نوفمبر: ظهر 200 في بند مختلف

| رقم السطر | شهر Oracle | كود البند | القيمة | دلالة المصدر |
| --- | --- | --- | ---: | --- |
| `SRC-DEDUCT-OCT-01` | أكتوبر | `COMP-ORIGINAL` | 800 | خصم وارد |
| `SRC-DEDUCT-NOV-07` | نوفمبر | `COMP-OTHER` | 200 | خصم وارد **في بند مختلف** |

#### مخرجات قبل وجود سند يسمح بالتسوية

~~~json
{
  "obligationId": "OBL-TEST-01",
  "reportedAllocated": "800.00",
  "candidateUnallocatedAmount": "200.00",
  "proposedMatch": {
    "sourceLineId": "SRC-DEDUCT-NOV-07",
    "amount": "200.00",
    "sourceComponentId": "COMP-OTHER",
    "targetObligationComponentId": "COMP-ORIGINAL",
    "policyStatus": "CROSS_COMPONENT_AUTHORIZATION_MISSING",
    "decision": "REVIEW_REQUIRED"
  },
  "settledVerifiedAmount": null
}
~~~

#### مخرجات **مشروطة** بعد وجود سياسة وسند يسمحان بربط المبلغ
في مثال اختباري مختلف عن الواقع، لو قدمت الجهة المختصة مستندًا يربط سطر نوفمبر بالالتزام نفسه، وكانت سياسة نقل/تخصيص المبلغ بين البندين **مسموحة ومعتمدة للفترة والجهة**:

~~~json
{
  "allocationId": "ALLOC-TEST-NOV-01",
  "obligationId": "OBL-TEST-01",
  "sourceLineId": "SRC-DEDUCT-NOV-07",
  "reportedAllocationAmount": "200.00",
  "sourceComponentId": "COMP-OTHER",
  "originalObligationComponentId": "COMP-ORIGINAL",
  "crossComponentAuthorizationRef": "TEST-COMPETENT-AUTHORITY-EVIDENCE",
  "approvedBy": "AUTHORIZED-REVIEWER-TEST",
  "reportedDeductionCoverage": "1000.00",
  "reportedDifferenceRemaining": "0.00",
  "settlementStatus": "REPORTED_DEDUCTED_AWAITING_SETTLEMENT_PROOF"
}
~~~

**مهم:** وصول «200 في بند آخر» بنفسه لا يثبت جواز المقاصة. حتى استكمال التخصيص الورقي قد لا يثبت التوريد أو الدفع وفق مستندات الجهة. لو السياسة لا تسمح، يكون القرار الصحيح **رفض اعتماد التسوية وطلب تصحيح رسمي**، لا تعديل كود Oracle القديم.

## 4. ERD مصغّر للعلاقات الجديدة دون تكرار الموجود

~~~mermaid
erDiagram
  EMPLOYMENT_RELATIONSHIP ||--o{ GRADE_HISTORY : "درجات تاريخية"
  FINANCIAL_RULE_VERSION ||--o{ RULE_TIME_BASIS : "مرجع الزمن"
  PAYEE_RELATIONSHIP ||--o{ REVIEW_CASE : "حالات مراجعة"
  REVIEW_CASE ||--o{ EXPECTED_CALCULATION_LINE : "خطوط محسوبة"
  REVIEW_CASE ||--o{ REVIEW_ADJUSTMENT : "تعديلات"
  EXPECTED_CALCULATION_LINE o|--o{ REVIEW_ADJUSTMENT : "قد يتغير"
  FINANCIAL_COMPONENT ||--o{ REVIEW_ADJUSTMENT : "بند مستهدف"
  PAYEE_RELATIONSHIP ||--o{ DEDUCTION_OBLIGATION : "التزامات"
  DEDUCTION_OBLIGATION ||--o{ DEDUCTION_OBLIGATION_COMPONENT : "بنود الأصل"
  DEDUCTION_OBLIGATION ||--o{ DEDUCTION_CARRY_DECISION : "قرارات ترحيل"
  DEDUCTION_OBLIGATION ||--o{ DEDUCTION_ALLOCATION : "تخصيصات"
  SOURCE_FINANCIAL_OCCURRENCE ||--o{ DEDUCTION_ALLOCATION : "سطر الخصم"
  SETTLEMENT_POLICY o|--o{ DEDUCTION_ALLOCATION : "سند الربط"
  DEDUCTION_ALLOCATION ||--o{ DEDUCTION_SETTLEMENT_EVIDENCE : "دليل التوريد"
  EMPLOYMENT_RELATIONSHIP {
    string EmploymentId PK
  }
  GRADE_HISTORY {
    string GradeHistoryId PK
    date ValidFrom
    date ValidTo
    string Grade
  }
  FINANCIAL_RULE_VERSION {
    string RuleVersionId PK
    string Status
  }
  RULE_TIME_BASIS {
    string RuleTimeBasisId PK
    string GradeReferenceDatePolicy
    string RateReferenceDatePolicy
    string BaseAmountReferenceDatePolicy
  }
  REVIEW_CASE {
    string ReviewCaseId PK
  }
  REVIEW_ADJUSTMENT {
    string AdjustmentId PK
    string AdjustmentType
    decimal ReviewedAmount
    string EvidenceRef
    int ReviewRevision
  }
  EXPECTED_CALCULATION_LINE {
    string ExpectedLineId PK
    decimal ProposedAmount
  }
  FINANCIAL_COMPONENT {
    string ComponentId PK
  }
  PAYEE_RELATIONSHIP {
    string PayeeId PK
  }
  DEDUCTION_OBLIGATION {
    string ObligationId PK
    string PayeeId FK
    decimal AuthorizedAmount
    string OriginPeriod
    string Status
  }
  DEDUCTION_OBLIGATION_COMPONENT {
    string ComponentShareId PK
    string ObligationId FK
    string OriginalComponentId FK
    decimal EvidenceBasedAmount
  }
  DEDUCTION_CARRY_DECISION {
    string CarryDecisionId PK
    string ObligationId FK
    decimal CarriedAmount
    string TargetPeriod
    string AuthorizationRef
  }
  SOURCE_FINANCIAL_OCCURRENCE {
    string SourceLineId PK
    decimal RawAmount
  }
  DEDUCTION_ALLOCATION {
    string AllocationId PK
    string ObligationId FK
    string SourceLineId FK
    decimal AllocatedAmount
    string ReviewStatus
  }
  SETTLEMENT_POLICY {
    string PolicyId PK
    string ValidPeriod
    string LegalAuthorization
  }
  DEDUCTION_SETTLEMENT_EVIDENCE {
    string EvidenceId PK
    string VerificationStatus
  }
~~~

> الربط بـ`ExpectedCalculationLine` في الرسم توضيحي؛ المخطط النهائي لا يفترض أن `ReviewCase` يملك سطرًا ماليًا مباشرًا بدل علاقته بـ`ExpectedCalculationRun`. `RULE_TIME_BASIS` قد يكون كائن إعداد ضمن `FinancialRuleVersion` وليس جدولًا مستقلًا. `SETTLEMENT_POLICY` تحفظ موافقات النطاق؛ لا تخلق ترخيصًا قانونيًا بمفردها.

## 5. UML — تتابع طلب تسوية من بند مختلف

~~~mermaid
sequenceDiagram
  autonumber
  actor Reviewer as "المراجع المالي"
  participant UI as "واجهة الترحيل"
  participant Ledger as "سجل الاستقطاعات"
  participant Source as "أسطر Oracle الأصلية"
  participant Policy as "سياسة السماح بين البنود"
  participant Audit as "السجل الرقابي"
  Reviewer->>UI: فتح فرق خصم 200 عن أكتوبر
  UI->>Ledger: قراءة أصل الالتزام والمبالغ المخصصة
  Ledger-->>UI: فرق مرشح 200 وحالة الأدلة
  Reviewer->>UI: اقتراح سطر نوفمبر بمبلغ 200 من بند آخر
  UI->>Source: تحقق من المستفيد والشهر والسطر والإشارة
  Source-->>UI: أصل سطر مختلف وبصمة PDF
  UI->>Policy: هل هذا البند يحقق التزام الأصل؟
  alt لا يوجد سند أو العلاقة محظورة
    Policy-->>UI: NOT_AUTHORIZED
    UI-->>Reviewer: تسجيل ملاحظة وطلب تصحيح
  else توجد قاعدة وموافقة ومستند تخصيص
    Policy-->>UI: ELIGIBLE_FOR_REVIEW
    Reviewer->>UI: تأكيد تخصيص المبلغ بدليل
    UI->>Ledger: تخصيص ذري بفحص المبلغ المتاح والتزام الأصل
    Ledger->>Audit: سجل المراجع والإصدار والدليل
    Ledger-->>UI: مبلغ خصم مغطى بالمصدر، بانتظار إثبات التسوية
    UI-->>Reviewer: 0 فرق ظاهر ومرحلة التسوية لم تغلق بعد
  end
~~~

## 6. خريطة حالات خصم ناقص

~~~mermaid
stateDiagram-v2
  [*] --> DifferenceDetected
  DifferenceDetected --> ObligationNotVerified: "الالتزام الأصلي غير مثبت"
  DifferenceDetected --> ObligationVerified: "إثبات الالتزام والخصم"
  ObligationVerified --> CarryPending: "طلب ترحيل"
  CarryPending --> CarryApproved: "موافقة وسند"
  CarryPending --> CarryRejected: "لا يجوز الترحيل"
  CarryApproved --> ReportedNextPeriod: "ظهر مبلغ بشهر لاحق"
  ReportedNextPeriod --> AllocationReview: "تحديد الأصل والبند"
  AllocationReview --> RejectedMatch: "الربط غير جائز"
  AllocationReview --> ReportedAllocated: "ربط مصرح به"
  ReportedAllocated --> SettledVerified: "دليل تسوية مقبول"
  ReportedAllocated --> AwaitingSettlementProof: "الدليل غير متاح"
  AwaitingSettlementProof --> SettledVerified: "وصول دليل"
  Note right of ReportedAllocated: "ظهور الخصم ليس إثبات توريد"
~~~

## 7. اختبارات مهمة مرتبطة بالأمثلة
مراجعة كامل الحالات `HR-001..HR-005` و`OV-001..OV-007` و`DD-001..DD-015` في [المواصفة](../specs/HISTORICAL-LEAVE-REVIEW-ADJUSTMENTS-AND-DEDUCTION-CARRY.md). يجب أن تفشل الاختبارات عند اعتماد نسبة أبحاث غير موثقة، أو تغيير مصدر Oracle، أو تكرار تخصيص 200، أو إجازة قديمة تستعمل الدرجة الحالية دون تصريح، أو مقاصة بين بندين دون سند. لا يُنشأ SQL أو Code من هذا الملف إلا بقرار تنفيذ منفصل.
