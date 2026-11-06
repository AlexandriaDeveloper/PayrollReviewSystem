# مثال عملي: شكل المدخلات والجداول والمخرجات في نظام مراجعة المرتبات

> **جميع أسماء الموظفين والأرقام والمبالغ في هذا المثال افتراضية للتوضيح وليست بيانات فعلية أو قاعدة قانونية.** كود الوحدة \`30200105\` يعبّر عن نطاق العمل المتفق عليه، أما الرموز \`P-001\` و\`DOC-001\` فهي مفاتيح توضيحية وليست قيم UUID نهائية.

## السيناريو
الوحدة الحسابية للمجمع الطبي تراجع صرفية أكتوبر 2026. موظف افتراضي في كلية الطب لديه مرتّب أساسي، وعلاوتان مختلفتان **بنفس كود Oracle**، واستقطاع لصندوق دعم متحدي الإعاقة. المفروض أن تظهر البنود الثلاثة منفصلة حتى لو تكرر الكود أو تشوّه الاسم العربي في PDF.

## 1) شكل المدخلات قبل التخزين

### أ. بيانات الموظف المعتمدة لدى المراجع (لا تؤخذ من Oracle تلقائيًا)

| الحقل | قيمة افتراضية |
| --- | --- |
| PersonId | \`P-001\` |
| الاسم | أحمد محمود (اسم افتراضي) |
| الكلية | الطب |
| العلاقة الوظيفية | \`EMP-001\` |
| علاقة الصرف | \`PAY-001\` |
| شهر المراجعة | \`2026-10\` |
| المجموعة | \`GRP-01\` |
| الأجر الأساسي المعتمد لهذا المثال | 1000.00 |
| علاوة خاصة لسنة 2014 | 40.00 |
| علاوة خاصة لسنة 2015 | 60.00 |

### ب. الصفوف كما تظهر افتراضيًا في Oracle PDF

| السطر الأصلي | جانب الصرفية | كود Oracle | اسم البند المقروء من PDF | المبلغ |
| --- | --- | --- | --- | ---: |
| \`ROW-01\` | استحقاق | \`21110101\` | المرتب الأساسي | 1000.00 |
| \`ROW-02\` | استحقاق | \`21110505\` | \`ةصاخلا ةوالعلا 2014\` | 40.00 |
| \`ROW-03\` | استحقاق | \`21110505\` | \`ةصاخلا ةوالعلا 2015\` | 60.00 |
| \`ROW-04\` | استقطاع | \`41110104\` | صندوق دعم متحدي الإعاقة | 10.00 |

**المهم:** \`ROW-02\` و\`ROW-03\` حسابهما واحد، لكنهما **سطران ماليان مستقلان**. برنامج القراءة قد يقترح تفسيرًا للاسم المعكوس، لكن **لن يغيّر النص الأصلي** أو يدمج السطرين.

### ج. شكل المدخل البرمجي المقترح للاستيراد

\`\`\`json
{
  "payrollCycle": "2026-10",
  "institutionalUnitCode": "30200105",
  "documentKind": "EmployeePayslip",
  "sourceDocumentId": "DOC-001",
  "sourceFileSha256": "<sha256-of-original-private-pdf>",
  "parseAttemptId": "PARSE-001",
  "recipient": {
    "sourceRecipientOccurrenceId": "SRC-EMP-001",
    "rawName": "أحمد محمود",
    "matchedPayeeRelationshipId": null
  },
  "lines": [
    {"sourceLineId":"L-001","page":1,"row":1,"side":"Earning","rawCode":"21110101","rawLabel":"المرتب الأساسي","rawAmount":"1000.00","amount":1000.00},
    {"sourceLineId":"L-002","page":1,"row":2,"side":"Earning","rawCode":"21110505","rawLabel":"ةصاخلا ةوالعلا 2014","rawAmount":"40.00","amount":40.00},
    {"sourceLineId":"L-003","page":1,"row":3,"side":"Earning","rawCode":"21110505","rawLabel":"ةصاخلا ةوالعلا 2015","rawAmount":"60.00","amount":60.00},
    {"sourceLineId":"L-004","page":1,"row":4,"side":"Deduction","rawCode":"41110104","rawLabel":"صندوق دعم متحدي الإعاقة","rawAmount":"10.00","amount":10.00}
  ]
}
\`\`\`

> هذا **عقد توضيحي مقترح** وليس API منفذًا. قيمة \`sourceFileSha256\` الفعلية تنتج من الملف الأصلي داخل التخزين الآمن، ولا تُخترع. الأرقام JSON افتراضية، بينما التطبيق المالي يستخدم \`decimal\` وليس \`float\`.

## 2) ERD مصغّر لهذا المثال

\`\`\`mermaid
erDiagram
    PERSON ||--o{ EMPLOYMENT_RELATIONSHIP : "له علاقة وظيفية"
    PERSON ||--o{ PAYEE_RELATIONSHIP : "له علاقة صرف"
    PAYEE_RELATIONSHIP ||--o{ APPROVED_FINANCIAL_FACT : "تستند إلى بيانات معتمدة"
    PAYROLL_CYCLE ||--o{ SOURCE_DOCUMENT : "يستقبل المستند"
    SOURCE_DOCUMENT ||--o{ SOURCE_RECIPIENT : "يتضمن المستفيد"
    SOURCE_RECIPIENT ||--o{ SOURCE_LINE : "يتضمن الأسطر"
    SOURCE_LINE ||--o{ MAPPING_DECISION : "له قرارات ربط"
    FINANCIAL_COMPONENT ||--o{ MAPPING_DECISION : "البند المعتمد"
    PAYEE_RELATIONSHIP ||--o{ REVIEW_CASE : "تتم مراجعته"
    REVIEW_CASE ||--o{ REVIEW_FINDING : "تُسجّل فيه الفروق"
    PERSON {
      string PersonId PK
      string DisplayName
    }
    EMPLOYMENT_RELATIONSHIP {
      string EmploymentId PK
      string PersonId FK
    }
    PAYEE_RELATIONSHIP {
      string PayeeId PK
      string PersonId FK
    }
    APPROVED_FINANCIAL_FACT {
      string FactId PK
      string PayeeId FK
      decimal Amount
      string EffectivePeriod
    }
    PAYROLL_CYCLE {
      string CycleId PK
      string Period
    }
    SOURCE_DOCUMENT {
      string DocumentId PK
      string CycleId FK
      string Sha256
    }
    SOURCE_RECIPIENT {
      string OccurrenceId PK
      string DocumentId FK
    }
    SOURCE_LINE {
      string SourceLineId PK
      string OccurrenceId FK
      string RawCode
      string RawLabel
      decimal Amount
    }
    FINANCIAL_COMPONENT {
      string ComponentId PK
      string CanonicalName
    }
    MAPPING_DECISION {
      string DecisionId PK
      string SourceLineId FK
      string ComponentId FK
    }
    REVIEW_CASE {
      string ReviewCaseId PK
      string PayeeId FK
    }
    REVIEW_FINDING {
      string FindingId PK
      string ReviewCaseId FK
    }
\`\`\`

## 3) ما الذي سيظهر في جداول قاعدة البيانات؟

### \`Person\` — الأشخاص

| PersonId | الاسم |
| --- | --- |
| \`P-001\` | أحمد محمود (افتراضي) |

### \`EmploymentRelationship\` و\`PayeeRelationship\`

| EmploymentId | PersonId | College |
| --- | --- | --- |
| \`EMP-001\` | \`P-001\` | الطب |

| PayeeId | PersonId | EmploymentId | PayingUnit |
| --- | --- | --- | --- |
| \`PAY-001\` | \`P-001\` | \`EMP-001\` | \`30200105\` |

### \`SourceFinancialOccurrence\` — **كل سطر يُخزّن وحده**

| SourceLineId | SourceDocumentId | RawCode | RawLabel | Amount |
| --- | --- | --- | --- | ---: |
| \`L-001\` | \`DOC-001\` | \`21110101\` | المرتب الأساسي | 1000.00 |
| \`L-002\` | \`DOC-001\` | \`21110505\` | ةصاخلا ةوالعلا 2014 | 40.00 |
| \`L-003\` | \`DOC-001\` | \`21110505\` | ةصاخلا ةوالعلا 2015 | 60.00 |
| \`L-004\` | \`DOC-001\` | \`41110104\` | صندوق دعم متحدي الإعاقة | 10.00 |

### \`ComponentMappingDecision\` — تفسير البنود دون تغيير الأصل

| SourceLineId | ComponentId المعتمد | الاسم الطبيعي | حالة الربط |
| --- | --- | --- | --- |
| \`L-001\` | \`COMP-BASIC\` | المرتب الأساسي | مقترح للتأكيد |
| \`L-002\` | \`COMP-ALLOW-2014\` | العلاوة الخاصة 2014 | يحتاج مراجعة الاسم |
| \`L-003\` | \`COMP-ALLOW-2015\` | العلاوة الخاصة 2015 | يحتاج مراجعة الاسم |
| \`L-004\` | \`COMP-DISABILITY-FUND\` | صندوق دعم متحدي الإعاقة | يُقبل مبلغ المركزية بعد التحقق من الربط |

الأسماء الطبيعية وسياسات المراجعة لا تُشتق من الكود وحده؛ لازم يكون في ربط معتمد وفق المستند والتصنيف.

## 4) شكل المخرجات المتوقعة في شاشة المراجع

| بند المراجعة | من Oracle | المستحق المتوقع | النتيجة |
| --- | ---: | ---: | --- |
| المرتب الأساسي | 1000.00 | 1000.00 | متطابق حسابيًا |
| العلاوة الخاصة 2014 | 40.00 | 40.00 | المبلغ متطابق، الاسم يحتاج تأكيدًا |
| العلاوة الخاصة 2015 | 60.00 | 60.00 | المبلغ متطابق، الاسم يحتاج تأكيدًا |
| صندوق دعم متحدي الإعاقة | 10.00 | — | المبلغ يُؤخذ من المركزية دون إعادة احتساب |
| **إجمالي الاستحقاقات في المثال** | **1100.00** | **1100.00** | تطابق الإجمالي لا يلغي تحذير الاسم |
| **إجمالي الاستقطاعات في المثال** | **10.00** | — | يُفحص مصدره وموقعه مرة واحدة |
| **الصافي الحسابي المبسط** | **1090.00** | — | ليس إثباتًا لتطابق نماذج Oracle الرسمية |

> معادلة الصافي أعلاه توضيحية فقط، لا تمثل بالضرورة طريقة عرض حصة جهة العمل أو معادلة Oracle الرسمية لأي تقرير. مطابقة مستند حقيقي تستخدم الإجماليات المطبوعة والسياق المحاسبي الصحيح.

## 5) لماذا هذا التصميم مهم؟
- الكود المتكرر لا يُلغي أو يدمج أي سطر.
- اسم البند المعكوس يُعرض **بجوار النص الأصلي** مع اقتراح للتصحيح.
- البنود المحسوبة في المركزية لا يُختلق لها \`ExpectedAmount=0\`.
- يمكن إعادة قراءة نفس PDF بمحلل أحدث دون تدمير النسخة السابقة.
- المراجع يستطيع الوصول للصفحة والخلية التي أنتجت كل مبلغ، وتسجيل قراره ومن اعتمده ومتى.
