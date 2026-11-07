# مثال كامل: رعاية طفل تبدأ وسط الشهر، وبيانات مركزية من صورة أو إدخال يدوي

> جميع الأسماء والمبالغ والمواعيد والرموز في هذا الملف **بيانات اصطناعية للتدريب**، وليست بيانًا لموظفة فعلية أو تحديدًا لقانون أو التزام رسمي. `Oracle` لا يُعدّل من هذا البرنامج. [مواصفة رعاية الطفل](../specs/CHILD-CARE-LEAVE-AND-CENTRAL-SETTLEMENT.md) و[قنوات الإدخال](../specs/MULTI-CHANNEL-CENTRAL-INTAKE.md).

## الحالة: موظفة بالكلية، إجازة رعاية الطفل تبدأ 20 نوفمبر 2026

### المدخل الإداري — ليس تسوية مالية

| الحقل | القيمة الافتراضية |
| --- | --- |
| `PayeeRelationshipId` | `PAY-EX-01` |
| الكلية | الطب |
| نوع الطلب | رعاية طفل لأول مرة |
| قرار الإجازة | `DEC-LEAVE-01` مثبت في المثال |
| سريان الإجازة | 20 نوفمبر إلى 30 نوفمبر شاملًا |
| أيام محتملة قبل الإجازة | 1 إلى 19 نوفمبر |
| مدد سابقة | لا توجد وفق مثال تدريب، لكنها تحتاج مستندًا |
| المستندات | طلب الجامعة + قرار الإجازة + المستندات المؤيدة (محفوظة بالأرشيف الخاص) |

المحرك يُنتج تقسيمًا تاريخيًا **فقط**؛ تفاصيل الأجر المستحق عن أيام العمل والمساهمات الحكومية تعتمد على كل بند والنص والمصدر.

~~~json
{
  "employmentRelationshipId": "EMP-EX-01",
  "payeeRelationshipId": "PAY-EX-01",
  "administrativeDecisionId": "DEC-LEAVE-01",
  "decisionKind": "CHILD_CARE_LEAVE",
  "approvedPeriod": {
    "from": "2026-11-20",
    "toExclusive": "2026-12-01"
  },
  "payrollCycle": "2026-11",
  "sourceEvidenceStatus": "VERIFIED_FOR_SYNTHETIC_EXAMPLE"
}
~~~

### تقسيم الفترة الناتج عن القرار (من دون حساب مرتب)

| المقطع | الفترة | الوضع الإداري |
| --- | --- | --- |
| `SEG-WORK-01` | 1–19 نوفمبر | قبل بدء إجازة رعاية الطفل (العمل الفعلي يحتاج تأكيدًا) |
| `SEG-CARE-01` | 20–30 نوفمبر | إجازة رعاية الطفل المعتمدة في المثال |

**لا** نقول تلقائيًا إن الموظفة تستحق 19/30 من جميع البنود؛ يجب الرجوع لسريان كل بند.

## مدخل أول: استمارة `132 ع ح` بالصورة فقط

نفترض أن المستخدم صور شاشة تقرير 132 أو استلم صورة منه (وليست PDF). الصورة تحتوي قيمًا افتراضية: إجمالي 950 جنيهًا، استقطاع 50 جنيهًا، صافي 900 جنيه.

~~~json
{
  "captureId": "CAP-132-IMAGE-01",
  "captureMethod": "IMAGE_UPLOAD",
  "reportType": "SALARY_PAYMENT_FORM_132",
  "payrollCycle": "2026-11",
  "institutionalUnitCode": "30200105",
  "originalFile": {
    "archiveRef": "PRIVATE-ARCHIVE-REF-132",
    "sha256": "<computed-at-upload>",
    "fileType": "image/png"
  },
  "coverage": "SELECTED_RECIPIENT",
  "sourceRecipient": {
    "rawDisplayName": "موظفة اختبار",
    "matchedPayeeRelationshipId": null
  },
  "sourceFinancialOccurrences": [
    {"sourceLineId": "SL-132-E-1", "side": "Earning", "rawLabel": "استحقاق تجريبي", "rawCode": null, "rawAmountToken": "950.00", "amount": "950.00"},
    {"sourceLineId": "SL-132-D-1", "side": "Deduction", "rawLabel": "استقطاع تجريبي", "rawCode": null, "rawAmountToken": "50.00", "amount": "50.00"}
  ],
  "sourcePrintedTotals": {
    "gross": "950.00",
    "deductions": "50.00",
    "net": "900.00"
  },
  "evidenceAuthenticity": "IMAGE_UNCONFIRMED",
  "transcriptionAccuracy": "SECOND_CHECK_PENDING"
}
~~~

### مدخل ثانٍ: استمارة «تسوية رعاية الطفل» دخلت يدويًا

لنفترض أنه **لا PDF ولا صورة**، لكن المراجع اطلع على كشف رسمي عبر قناة عمل مخولة وسجل معلومة اختبارية عن مساهمة حكومية قدرها 180 جنيهًا في **حقل معلوماتي لا يُصرف نقدًا للموظفة**. المبلغ افتراضي ولا تُشتق منه نسبة تأمين.

~~~json
{
  "captureId": "CAP-CARE-MANUAL-02",
  "captureMethod": "MANUAL_FROM_OFFICIAL_SOURCE",
  "reportType": "CHILD_CARE_SETTLEMENT",
  "payrollCycle": "2026-11",
  "reasonPdfUnavailable": "تعذر الحصول على ملف رسمي قابل للرفع في المثال",
  "originalDocumentReference": "EXTERNAL-REFERENCE-NEEDS-ATTESTATION",
  "institutionalUnitCode": "30200105",
  "sourceRecipient": {
    "rawDisplayName": "موظفة اختبار",
    "matchedPayeeRelationshipId": "PAY-EX-01"
  },
  "sourceFinancialOccurrences": [
    {
      "sourceLineId": "SL-CARE-I-1",
      "side": "EmployerShare",
      "rawCode": null,
      "rawLabel": "حصة جهة مثال - رعاية الطفل",
      "amount": "180.00",
      "componentMappingStatus": "REVIEW_REQUIRED"
    }
  ],
  "sourcePrintedTotals": {
    "gross": null,
    "deductions": null,
    "net": null
  },
  "coverage": "SELECTED_LINES",
  "evidenceAuthenticity": "OFFICIAL_VIEW_ATTESTATION_PENDING",
  "transcriptionAccuracy": "SECOND_CHECK_PENDING"
}
~~~

**ملاحظة:** عدم وجود مستند قابل للرفع لا يسمح بتسجيل `sourcePrintedTotals=180` لأننا لا نعرف الإجمالي المطبوع في التسوية؛ المبلغ المسجل سطر واحد فقط. إذا لم يوجد حتى مرجع رسمي أو شاهد، يختار النظام `MANUAL_WITHOUT_DOCUMENT` وتبقى البيانات مؤقتة غير موثقة.

## مخرجات شاشة المقارنة

| نوع التقرير | القيمة التي يمكن عرضها | حالة الإثبات | ما الذي يمنع الاعتماد الشامل؟ |
| --- | --- | --- | --- |
| استمارة 132 من صورة | إجمالي 950، استقطاع 50، صافي 900 | صورة غير مؤكدة + مراجعة نقل معلقة | التأكد من صحة الصورة والمستفيد وربط البنود |
| تسوية رعاية الطفل من سجل يدوي | ظهور سطر حصة جهة 180 | مرجع رسمي غير مصدق + مراجعة نقل معلقة | أصل أو شهادة معاينة مخولة/تأكيد الكود والجانب |
| قرار رعاية الطفل الإداري | الفترة 20–30 نوفمبر | مثبت في اختبار مصطنع | التأكد من صحة القرار والاختصاص في الإنتاج |
| **نتيجة المراجعة** | *لا يوجد مبلغ إجمالي معتمد بعد* | `REVIEW_REQUIRED` | لا يصح جمع 900 الصافي مع 180 حصة جهة أو إعلان صرف 1080 |

## ERD: قنوات متعددة، سجل مصدر واحد

~~~mermaid
erDiagram
  PAYROLL_CYCLE ||--o{ SOURCE_CAPTURE_SESSION : "يخص دورة"
  SOURCE_CAPTURE_SESSION ||--o{ EVIDENCE_ATTACHMENT : "يضم دليلا"
  SOURCE_CAPTURE_SESSION ||--o{ SOURCE_RECIPIENT_OCCURRENCE : "يرصد مستفيدين"
  SOURCE_RECIPIENT_OCCURRENCE ||--o{ SOURCE_FINANCIAL_OCCURRENCE : "يرصد سطورا"
  SOURCE_CAPTURE_SESSION ||--o{ CAPTURE_VERIFICATION : "يمر بمراجعة"
  SOURCE_CAPTURE_SESSION ||--o{ SOURCE_REPORTED_TOTAL : "له إجماليات أصلية"
  PAYEE_RELATIONSHIP ||--o{ CHILD_CARE_LEAVE_EPISODE : "له فترات رعاية"
  CHILD_CARE_LEAVE_EPISODE ||--o{ CHILD_CARE_SETTLEMENT_AUDIT : "تراجع التسوية"
  SOURCE_CAPTURE_SESSION ||--o{ CHILD_CARE_SETTLEMENT_AUDIT : "توثق المصدر"
  PAYROLL_CYCLE {
    string CycleId PK
    string Period
  }
  SOURCE_CAPTURE_SESSION {
    string CaptureId PK
    string CaptureMethod
    string ReportType
    string SourceCoverage
    string EvidenceAuthenticity
  }
  EVIDENCE_ATTACHMENT {
    string AttachmentId PK
    string ArchiveRef
    string Sha256
    string MediaType
  }
  SOURCE_RECIPIENT_OCCURRENCE {
    string RecipientOccurrenceId PK
    string RawName
  }
  SOURCE_FINANCIAL_OCCURRENCE {
    string SourceLineId PK
    string RawCode
    string RawLabel
    decimal Amount
    string FinancialSide
  }
  CAPTURE_VERIFICATION {
    string VerificationId PK
    string Status
    string VerifiedBy
  }
  SOURCE_REPORTED_TOTAL {
    string TotalId PK
    string TotalKind
    decimal PrintedAmount
  }
  PAYEE_RELATIONSHIP {
    string PayeeId PK
  }
  CHILD_CARE_LEAVE_EPISODE {
    string EpisodeId PK
    string DecisionId
    date EffectiveFrom
    date EffectiveTo
  }
  CHILD_CARE_SETTLEMENT_AUDIT {
    string AuditId PK
    string PayrollCycleId
    string ReviewStatus
  }
~~~

> `SourceCaptureSession` امتداد استيراد المصدر الموجود، و`EVIDENCE_ATTACHMENT` لا يخلق بايتات أصل عند الإدخال اليدوي بلا ملف. بعض الروابط والجداول تبقى تصميمًا منطقيًا حتى ADR؛ لا إنشاء FK أو اعتماد تكرار لا داعي له.

## UML: من صورة إلى حالة مراجعة دون التحايل على المصدر

~~~mermaid
sequenceDiagram
  autonumber
  actor User as "مراجع مخول"
  participant UI as "واجهة عربية"
  participant Ingest as "استيراد متعدد القنوات"
  participant Archive as "أرشيف الأدلة"
  participant Gate as "بوابة جودة المصدر"
  participant Reviewer as "مراجع النقل المستقل"
  participant Audit as "ملف المراجعة"
  User->>UI: صورة تقرير 132 + اختيار نوعه والشهر
  UI->>Ingest: ImageCapture
  Ingest->>Archive: حفظ الصورة والبصمة
  Ingest-->>UI: سطور مقترحة ومواضع الصورة
  User->>UI: تصحيح السطور غير المؤكدة
  UI->>Gate: فحص المجاميع والجانب والهوية والتغطية
  Gate-->>UI: TranscriptionReviewRequired
  Reviewer->>Audit: التحقق من نقل القيم وفق الدليل
  Audit-->>UI: قبول لقطة مصدر مع درجة إثبات واضحة
  User->>UI: إدخال يدوي لتسوية رعاية الطفل بلا PDF
  UI->>Ingest: ManualCapture + مرجع وسند
  Ingest->>Gate: فحص مصدر/شاهد/نوع ومكونات
  Gate-->>UI: EvidencePending أو TranscribedAndAttested
  Note over Gate,Audit: لا يُحوّل سجل الإدخال اليدوي إلى PDF رسمي ولا يُصدر أمر دفع
~~~

## نموذج ورقة المراجعة الداخلية المقترحة (وليس استمارة Oracle رسمية)
| خانة | قيمة المثال |
| --- | --- |
| الوحدة / دورة المرتبات | `30200105` / نوفمبر 2026 |
| الموظفة / علاقة الصرف | موظفة اختبار / `PAY-EX-01` |
| مستند قرار الرعاية | `DEC-LEAVE-01` |
| فترة الرعاية | 20–30 نوفمبر |
| فترة العمل المحتملة | 1–19 نوفمبر |
| ملف 132 | صورة مقدمة، تحتاج إثبات المصدر |
| تسوية رعاية الطفل | مدخل يدوي جزئي، يحتاج شهادة |
| بيانات حصة الجهة | سطر 180 معلوماتي، غير نقدي |
| التوقيع / القرار | «قيد المراجعة» إلى أن تكتمل الأدلة |

## حالات التصميم المعروضة في المواصفتين
`CC-001..CC-012` لرعاية الطفل، و`MI-001..MI-016` لقنوات الاستيراد، بما في ذلك منع استقطاع/مساهمة الجهة من التحول إلى صافي موظف، وعدم إخفاء صورة لم تُراجع، والمقارنة إذا وصل PDF رسمي بعد الإدخال اليدوي.
