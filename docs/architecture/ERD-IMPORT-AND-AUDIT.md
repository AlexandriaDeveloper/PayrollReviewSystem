# ERD رقم 2 — مستندات Oracle والسطور الأصلية وأعمال المراجعين

**كل قيمة مستوردة هي دليل خارجي لا حقيقة موظف معتمدة.** يختلف استيراد صرفية فردية عن الكافر وكشوف الصرف، وقد تكون بعض النماذج إجمالية بلا أسماء مستفيدين.

## العلاقات المرسومة
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

## دورة البيانات
1. `PayrollCycle` تحدد دورة الصرف والفترة، و`PayrollGroup` مجموعات العمل والرقابة.
2. `ImportBatch` مجموعة ملفات مرفوعة؛ قد يحتوي أكثر من ملف من النوع نفسه وفق حالات واقعية معتمدة.
3. `SourceDocument` يحتفظ ببصمة الأصل ومكان تخزينه الآمن ورقم نسخته. النسخة الرسمية المصححة ترتبط بالأقدم ولا تمحوها.
4. `DocumentParseAttempt` يحدد إصدار المحلل وAdapter المستخدم، ويمكن إعادة تحليل المستند دون خلط النتائج السابقة.
5. `SourceRecipientOccurrence` هو ظهور المستفيد داخل مستند محدد، وليس `Person` الداخلي.
6. `SourceFinancialOccurrence` **كل صف مالي مستقل** مع الصفحة والجدول والجانب والكود والاسم الخام والمبلغ وعلامته.
7. `SourceReportedTotal` تحفظ الإجماليات المطبوعة، و`SourceGroupSummary` مجموعات البنك والكافر، و`ParseValidationIssue` الأخطاء.
8. `RecipientMatchDecision` و`ComponentMappingDecision` قرارات ربط المراجع بهوية وبند داخلي، مع سجل الحسم والوثيقة.
9. `ReviewerAssignment` يسجل إسناد مجموعة مرتب لمراجع خلال دورة، مع التغيير والتاريخ.

## قيود مهمة
- **ممنوع مفتاح فريد للكود أو للاسم أو للكود+الاسم** داخل الأسطر الأصلية؛ التكرار قد يكون صحيحًا أو ازدواجًا يحتاج فحصًا.
- اختلاف نتيجة قارئ PDFium وقارئ الخلايا الفيزيائية يجب أن يُعرض ولا يؤخذ أحدهما سرًا.
- احتفظ بالنص العربي الخام حتى لو كان مقلوبًا؛ الاسم الطبيعي تفسير ثانٍ وليس بديلًا يمسح الأصل.
- لا تُساوي بين `ParseStatus` و`MappingStatus` و`ReviewStatus` و`PaymentStatus`.
- مطابقة مجموعات الكافر تستخدم البنك والفرع والمجموعة والشهر، لا `Groups[i]`.
- لمجرد نجاح الصافي لا يجوز تجاهل خطأ الاستحقاقات أو الاستقطاعات أو معلومات جهة العمل.
- لا تنسخ اسمًا أو كودًا غير مؤكد من ملف إلى معلومات موظف معتمد.
- قد تحتاج الصفحة أو الصف أو التكرار لمراجعة بدلاً من اختراع صفر أو إرجاع `Verified`.
- عند اختيار تصدير موظف واحد، لا تُسرب تحذيرات أو أسماء موظفين آخرين.

## مثال للصفوف المخزنة
في [مثال المدخلات والمخرجات](../examples/01-ERD-INPUT-OUTPUT.md) يظهر الكود `21110505` مرتين كعلاوتين لسنتين مختلفتين، مع `SourceLineId` مستقل لكل سطر. هذا **مثال اصطناعي** وليس اعتمادًا لأكواد أو مبالغ قانونية.
