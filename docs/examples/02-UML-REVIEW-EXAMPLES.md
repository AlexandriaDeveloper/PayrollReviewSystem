# مثال UML: من إدخال صرفية Oracle إلى قرار المراجع والمخرجات

> سيناريوهات تدريبية افتراضية؛ لا تتضمن بيانات موظفين حقيقية أو موافقة على قاعدة قانونية. أسماء الكلاسات والعقود بالإنجليزية للحفاظ على دقة التنفيذ، والشرح بالعربية.

## السيناريو الأول: ملف صحيح إجمالًا لكن بندان لهما نفس الكود

### UML Class Diagram: من المسؤول عن أي عملية؟

\`\`\`mermaid
classDiagram
  direction LR
  class SourceDocument {
    +Id
    +Sha256
    +DocumentType
  }
  class SourceFinancialOccurrence {
    +Id
    +RawCode
    +RawArabicLabel
    +PageNumber
    +ParsedAmount
    +Side
  }
  class IPdfExtractionAdapter {
    <<interface>>
    +Extract(document) SourceObservations
  }
  class FinancialComponent {
    +ComponentId
    +CanonicalName
    +AuditPolicy
  }
  class IComponentMapper {
    <<interface>>
    +Propose(sourceLine) MappingCandidates
  }
  class ReconciliationService {
    +Compare(source, expected) Differences
  }
  class AuditCase {
    +CaseId
    +ReviewRevision
    +RecordDecision()
  }
  SourceDocument "1" --> "*" SourceFinancialOccurrence : "يحتوي سطورًا مستقلة"
  IPdfExtractionAdapter ..> SourceDocument : "يقرأ"
  IComponentMapper ..> SourceFinancialOccurrence : "يفسّر"
  IComponentMapper ..> FinancialComponent : "يقترح ربطًا"
  ReconciliationService ..> SourceFinancialOccurrence : "يقارن"
  AuditCase ..> ReconciliationService : "يعرض الفروق"
\`\`\`

### UML Sequence Diagram: طلب الإدخال والرد

\`\`\`mermaid
sequenceDiagram
  autonumber
  actor Reviewer as "المراجع"
  participant UI as "واجهة المراجعة"
  participant Import as "خدمة الاستيراد"
  participant PDF as "محلل PDF"
  participant DB as "قاعدة البيانات"
  participant Mapper as "خدمة ربط البنود"
  participant Audit as "ملف المراجعة"
  Reviewer->>UI: يختار صرفية أكتوبر ويرفع PDF
  UI->>Import: ImportPdf(file,cycle)
  Import->>DB: حفظ أصل المستند وSHA256
  Import->>PDF: استخراج البنود مع الصفحة/الصف
  PDF-->>Import: L-001, L-002, L-003, L-004
  Note over PDF,Import: الكود 21110505 ظهر مرتين: لا ندمج السطرين
  Import->>DB: تخزين الأسطر الأربعة كملاحظات مصدرية
  Import->>Mapper: اقتراح تعيين كل سطر إلى بند معتمد
  Mapper-->>UI: اقتراحان يحتاجان تأكيد الاسم العربي
  Reviewer->>Audit: يعتمد ربط السطرين أو يطلب مراجعة
  Audit->>DB: حفظ القرار والوقت وهوية المراجع
  Audit-->>UI: نتيجة محدثة، مع بقاء PDF الأصلي كما هو
\`\`\`

## شكل الـInput البرمجي

\`\`\`json
{
  "command": "ImportPdf",
  "cycleId": "CYCLE-2026-10",
  "sourceDocumentId": "DOC-001",
  "expectedDocumentKind": "EmployeePayslip",
  "actingReviewerId": "USER-01"
}
\`\`\`

## شكل الـOutput البرمجي المقترح

\`\`\`json
{
  "documentId": "DOC-001",
  "parseStatus": "ReviewRequired",
  "financialOccurrences": [
    {"id":"L-001","code":"21110101","amount":"1000.00","mappingStatus":"Proposed"},
    {"id":"L-002","code":"21110505","rawLabel":"ةصاخلا ةوالعلا 2014","amount":"40.00","mappingStatus":"NeedsReview"},
    {"id":"L-003","code":"21110505","rawLabel":"ةصاخلا ةوالعلا 2015","amount":"60.00","mappingStatus":"NeedsReview"},
    {"id":"L-004","code":"41110104","amount":"10.00","auditTreatment":"CentralSourceAmount"}
  ],
  "issues": [
    {"code":"ARABIC_NAME_UNCERTAIN","sourceLineId":"L-002","severity":"Review"},
    {"code":"ARABIC_NAME_UNCERTAIN","sourceLineId":"L-003","severity":"Review"}
  ]
}
\`\`\`

> أسماء الأوامر والحقول والـDTOs أعلاه **تصميم مبدئي** وليست Routes منفذة. القيم المالية في الإنتاج تُمثل بنوع \`decimal\` ثابت الدقة، وتُنقل في بعض العقود كسلاسل نصية لمنع فقد الدقة.

## السيناريو الثاني: صرفية اعتُمدت ثم صدر قرار إعارة

\`\`\`mermaid
stateDiagram-v2
  [*] --> UnderReview
  UnderReview --> Approved: "اعتماد المراجعة"
  Approved --> ReopenRequested: "ظهور قرار إعارة لاحق"
  ReopenRequested --> AmendmentReview: "التحقق من المستند والتاريخ"
  AmendmentReview --> AwaitingEvidence: "بيانات ناقصة"
  AwaitingEvidence --> AmendmentReview: "استيفاء البيانات"
  AmendmentReview --> NewApproval: "اعتماد نسخة مراجعة جديدة"
  NewApproval --> [*]
\`\`\`

**المدخل:** مستند إداري صادر بعد الاعتماد، يحدد تاريخ سريان الإعارة والجزء المستحق من الشهر.

**المخرجات:** طلب تعديل مرتبط بالنسخة القديمة، أثر مالي مقترح يحتاج مراجعة، ونسخة اعتماد جديدة عند استكمال الإجراءات. لا تُعدّل الصرفية الأصلية داخل Oracle، ولا يُوقف البنك تلقائيًا.

## السيناريو الثالث: العمل جزءًا من الوقت — قاعدة رقابة استثنائية

| الحقل | القيمة الافتراضية |
| --- | ---: |
| الأجر النقدي قبل القرار | 1000 |
| النسبة الافتراضية في القرار | 65% |
| الأجر النقدي بعد القرار | 650 |
| وعاء الاشتراك المعتمد **لهذا المثال** | 1000 |
| الوعاء الذي استخدمته المركزية بحسب دليل متاح | 650 |

**المخرج:** \`PensionBaseMismatch\` — ملاحظة تطلب اعتراضًا وتصحيحًا من الجهة المختصة عند تحقق شروط القاعدة ومستنداتها. **لا يحسب البرنامج قيمة الاشتراك نفسه**، ولا يفترض أن 1000 هي الوعاء المعتمد لكل موظف. إذا لم يظهر وعاء المركزية في المستند، تكون النتيجة \`EvidenceRequired\` وليس إثبات خطأ.

## السيناريو الرابع: عدة بنود مجمّعة في بند واحد

| البنود المعتمدة | المتوقع |
| --- | ---: |
| مكافأة افتراضية أ | 200 |
| مكافأة افتراضية ب | 300 |
| مكافأة افتراضية ج | 500 |
| الإجمالي | 1000 |

**مدخل Oracle الافتراضي:** بند واحد بمبلغ 1000.

**المخرج:** \`DistributionMismatch\` مع تفاصيل ثلاثة أسطر متوقعة مقابل سطر مصدر واحد؛ لا يسوّيها تلقائيًا حتى لو الإجمالي صحيح. قد يختار المراجع \`AcceptedWithDocumentedException\` بعد التحقق من باب الصرف والضوابط والمستندات والصلاحية.

## الفرق بين نتيجة المحلل ونتيجة المراجع

| الطبقة | مثال الحالة | ماذا تعني؟ |
| --- | --- | --- |
| \`ParseStatus\` | \`ReviewRequired\` | يوجد اسم غير مؤكد رغم قراءة المبالغ |
| \`MappingStatus\` | \`NeedsReview\` | لم يحدد المراجع البند المعتمد بعد |
| \`ReconciliationStatus\` | \`DistributionMismatch\` | إجمالي صحيح لكن توزيع البنود مختلف |
| \`ReviewStatus\` | \`ReviewerCompleted\` | انتهى فحص المراجع؛ قد تبقى مطالبة تصحيح |
| \`PaymentStatus\` | \`ReportedInPayroll\` | ظهر في كشف الصرف، ولم يثبت بعد دخوله البنك |

## طريقة استخدام المثال مع Gemini
يراجع Gemini هذا السيناريو مع ERD المفصل وUML الرئيسي. يُطلب منه إنشاء **اختبارات اصطناعية أولًا**، ولا يبدأ تطبيق Schema أو واجهة/API حقيقية إلا بIssue تنفيذ منفصل معتمد. هذه الأمثلة توضح الشكل المراد الوصول إليه فقط.
