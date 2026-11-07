# UML — تتابع العمليات وحالات المراجعة

**مخططات مقترحة لا تعني وجود كود منفذ.** تُظهر الحركة بين المراجع والتطبيق وطبقة الدومين ومصدر Oracle، دون تنفيذ صرف أو استرداد.

## مسار بديل لاستيراد استحقاقات موظف من Excel أو إدخال مباشر

```mermaid
sequenceDiagram
  actor Reviewer as "المراجع"
  participant UI as "واجهة الاستحقاقات"
  participant Intake as "ICentralPayrollSourceIntake"
  participant Validate as "SourceValidation"
  participant Audit as "AuditCase"
  alt Excel
    Reviewer->>UI: رفع XLSX لموظف وبنوده
    UI->>Intake: ImportEmployeeExcel(file, context)
  else شاشة مباشرة
    Reviewer->>UI: فتح الموظف وإضافة سطور البنود
    UI->>Intake: CreateManualEmployeeDraft(context)
    UI->>Intake: SaveManualLines(draftId, lines, version)
  end
  Intake-->>UI: CaptureDraft مع سطور مصدرية مستقلة
  UI->>Validate: ValidateDraft(draftId)
  Validate-->>UI: أخطاء الصفوف والمجاميع وحالة الإثبات
  Reviewer->>UI: تصحيح المسودة ثم إرسالها للمراجعة
  UI->>Intake: SubmitForSourceReview(draftId)
  Intake-->>Audit: CapturedSourceSnapshot
  Audit-->>Reviewer: مقارنة الاستحقاق المتوقع بالمصدر البديل
```

ملحوظة: لا يتطلب المسار صورة أو OCR، ولا يؤدي إدخال Excel أو الشاشة إلى اعتماد صرف أو تعديل بيانات Oracle. [تفاصيل ERD واختبارات EX وDF](../specs/EXCEL-AND-DIRECT-EMPLOYEE-PAYROLL-INTAKE.md).

## 1. تسلسل استيراد PDF والتحقق من البنود
```mermaid
sequenceDiagram
  autonumber
  actor Reviewer as المراجع
  participant UI as الواجهة العربية
  participant API as خدمة الاستيراد
  participant Archive as أرشيف المصادر
  participant Parser as محرك قراءة PDF
  participant Check as بوابة الجودة
  participant Store as السطور المصدرية
  Reviewer->>UI: يرفع PDF لدورة مرتبات
  UI->>API: بدء الاستيراد
  API->>Archive: حفظ الملف الأصلي والبصمة
  Archive-->>API: DocumentId
  API->>Parser: قراءة النص والخلايا والصفحات
  Parser-->>API: أسطر ومبالغ وأدلة وتحذيرات
  API->>Check: فحص الهوية والمجاميع والإشارات والجوانب
  Check-->>API: حالات مستقلة لكل سطر وتقرير
  API->>Store: تخزين كل ظهور مستقل دون دمج الأكواد
  API-->>UI: عرض الملف والسطور والمشكلات
  alt الاسم العربي أو الربط غير مؤكد
    Reviewer->>UI: يختار الربط المدعوم بدليل
    UI->>Store: إضافة قرار جديد وسجل مراجعة
  else المستند غير معروف
    API-->>UI: Rejected وسبب الرفض
  end
  Note over Store: لا يغيّر الاستيراد بيانات الموظف المعتمدة
```

## UML — قرار إجازة يخرج 3 استقطاعات

```mermaid
sequenceDiagram
 actor Reviewer as المراجع
 participant Event as قرار الإجازة
 participant Rules as IFinancialRuleResolver
 participant Grade as GradeHistory
 participant Facts as ApprovedFinancialFacts
 participant Engine as IExpectedPayrollCalculator
 Reviewer->>Event: إدخال فترة الإجازة المعتمدة
 Event->>Rules: حل القواعد والفترات
 Rules->>Grade: اختيار فئة الدرجة التاريخية
 Grade-->>Rules: النسبة الخاصة بالفئة أو تعارض
 Rules-->>Engine: ثلاثة RuleEffect
 loop لكل شهر ولكل أثر
  Engine->>Facts: قراءة وعاء البند بتاريخ القاعدة
  Facts-->>Engine: مبلغ أو INPUT_MISSING
  Engine->>Engine: نسبة ثابتة أو جدول درجة × أيام الإجازة/أيام الشهر
 end
 Engine-->>Reviewer: ثلاثة مقترحات مستقلة وسجل شرح حساب
```

[مثال 6 أيام/30 يومًا وأربعة فحوص للتداخل والدرجة والوعاء](../specs/FACULTY-ANNUAL-LEAVE-MULTI-DEDUCTION-RULE.md). الحساب مقترح ولا ينفذ خصمًا.

## 2. الحساب المستقل وقرار المراجع
```mermaid
sequenceDiagram
  autonumber
  actor Reviewer as المراجع المختص
  participant UI as شاشة المراجعة
  participant Case as ملف المراجعة
  participant Facts as الوقائع المعتمدة
  participant Rules as القواعد القانونية المتحققة
  participant Engine as حساب المستحق المقترح
  participant Source as أسطر Oracle الأصلية
  participant Compare as محرك المطابقة
  Reviewer->>UI: فتح موظف من مجموعته
  UI->>Case: فحص الصلاحية ونسخة الحالة
  Case->>Facts: بيانات الموظف في شهر الاستحقاق
  Case->>Rules: اختيار إصدار القاعدة الساري
  Case->>Engine: حساب البنود غير المستثناة
  Engine-->>Case: مقترحات مع مدخلات وأثر حساب
  Case->>Source: تحميل السطور المقبولة للقراءة
  Case->>Compare: مقارنة كل بند وإجمالياته
  Compare-->>UI: فروق وملاحظات وتوزيع البنود
  alt فرق أو نقص مستند
    Reviewer->>Case: تسجيل تصحيح أو مطالبة مستند أو اعتراض
  else مراجعة مكتملة ومصرح بالاعتماد
    Reviewer->>Case: اعتماد نسخة المراجعة بعد الضوابط
  end
  Note over Engine: التأمينات والصناديق المحددة مصدرية دون إعادة حساب
```

## 3. تعديل طارئ بعد اعتماد المراجعة
```mermaid
sequenceDiagram
  autonumber
  actor Reviewer as المراجع
  participant Case as نظام المراجعة
  participant Evidence as المستندات والقرارات
  participant Engine as إعادة الحساب حسب الفترة
  participant Approval as الأرشيف المعتمد
  participant Central as الجهة المركزية الخارجية
  Reviewer->>Case: وصول وفاة أو إعارة أو جزء شهر
  Case->>Evidence: فحص التاريخ والمستند والصلاحية
  Case->>Approval: قراءة الاعتماد القديم دون تعديله
  Case->>Engine: اقتراح أثر الحدث على الشهور والبنود
  Engine-->>Case: فرق مقترح ومراجعة جديدة
  Case-->>Reviewer: طلب تعديل واعتماد جديد
  opt ضرورة تصحيح خارجي
    Reviewer->>Central: طلب إيقاف أو تصحيح عبر الإجراءات المخولة
    Central-->>Reviewer: مستند رسمي جديد إن ورد
    Reviewer->>Case: إرفاق المصدر الجديد وتتبعه
  end
  Note over Central: لا يصدر النظام أمر صرف أو إيقاف تلقائي
```

## 4. المتجمدات وظهور الصرف لاحقًا
```mermaid
sequenceDiagram
  autonumber
  actor Reviewer as المراجع
  participant Order as قرار إداري معتمد
  participant Engine as المحرك التاريخي
  participant Retro as سجل المتجمدات
  participant Report as كشوف الصرف اللاحقة
  participant Proof as أدلة التسوية
  Reviewer->>Order: إثبات قرار ترقية وتاريخ الأثر
  Order->>Engine: حساب فرق كل بند وكل شهر سابق
  Engine-->>Retro: مطالبة مقترحة بفترة وبند وسبب
  Retro-->>Reviewer: فحص واعتماد أو تعديل المطالبة
  Report->>Retro: ظهور مبلغ متجمد في كشف لاحق
  alt ثبتت هوية الشهر والبند والتسوية
    Retro->>Proof: فحص دليل الصرف الفعلي
    Proof-->>Retro: مبلغ مثبت وتاريخ مستقل
    Retro-->>Reviewer: تخصيص المبلغ للمطالبة الأصلية
  else ظهر مبلغ مجمع غير موضح
    Retro-->>Reviewer: يبقى غير موزع ويحتاج إثباتًا
  end
  Note over Retro: ظهور المبلغ بكشف الصرف لا يعني أنه وصل للبنك
```

## 5. مخطط حالات مراجعة المستفيد
```mermaid
stateDiagram-v2
  [*] --> NotStarted
  NotStarted --> SourcePending: "بدء الفحص"
  SourcePending --> UnderReview: "قبول مصدر للفحص"
  SourcePending --> SourceRejected: "رفض المستند"
  SourceRejected --> SourcePending: "مصدر رسمي بديل"
  UnderReview --> WaitingEvidence: "نقص مستند"
  WaitingEvidence --> UnderReview: "استيفاء البيانات"
  UnderReview --> Proposed: "اكتمال الفحص"
  Proposed --> UnderReview: "إرجاع للمراجع"
  Proposed --> Approved: "اعتماد نسخة ثابتة"
  Approved --> ReopenRequested: "حدث جديد بعد الاعتماد"
  ReopenRequested --> AmendmentReview: "فحص سبب التعديل"
  AmendmentReview --> UnderReview: "إعادة الحساب بنسخة جديدة"
  AmendmentReview --> WaitingEvidence: "بيانات غير مكتملة"
  Approved --> [*]
  Note right of Approved: "اعتماد المراجعة ليس إثبات دفع"
```

## 6. مخطط حالات الملاحظة والمتابعة
```mermaid
stateDiagram-v2
  [*] --> Detected
  Detected --> UnderReview
  UnderReview --> WaitingEvidence: "طلب إثبات"
  WaitingEvidence --> UnderReview: "وصول الإثبات"
  UnderReview --> CorrectionRequested: "طلب تصحيح مركزي"
  UnderReview --> ExceptionAccepted: "قبول استثناء موثق"
  UnderReview --> Resolved: "ثبوت عدم وجود فرق"
  CorrectionRequested --> OfficialCorrectionReceived: "وصول مستند مصحح"
  OfficialCorrectionReceived --> UnderReview: "إعادة الفحص"
  Resolved --> [*]
  ExceptionAccepted --> [*]
  Note right of CorrectionRequested: "يمكن اكتمال فحص الموظف مع بقاء طلب التصحيح"
```

## اختبارات السلوك المطلوبة
- تكرار كود بند داخل صفحة أو عبر صفحات لا يؤدي لدمج السطور.
- عدم وضوح الاسم أو فقد السالب يعطي مراجعة لازمة بدل اعتماد تلقائي.
- مراجعة يفتحها أكثر من شخص لا تمسح تغيرات أحدهما؛ هناك `ConcurrencyConflict`.
- اعتماد سابق لا يختفي بعد قرار وفاة/إعارة متأخر.
- المتجمد الظاهر في ديسمبر عن يوليو لا يسجل مدفوعًا بلا دليل تسوية.
- لا تُنشئ قيمة متوقعة مستقلة لبند صندوق يقبل من المركزية.
- في حالة جزء الوقت يكون وعاء المعاش مستقلًا عن الاستحقاق النقدي وتُتحقق قاعدة الاستثناء أولًا.

## مدخلات ومخرجات ملموسة
راجع [مثال UML التدريبي](../examples/02-UML-REVIEW-EXAMPLES.md) الذي يحتوي على طلب JSON ونتيجة تحليل PDF ومراجعة وتعديل بعد اعتماد.
