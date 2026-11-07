---
name: payroll-engineering-quality
description: تطبيق هندسة برمجيات عالية الجودة على تصميم وتنفيذ PayrollReviewSystem، بما يشمل TDD وKISS/DRY/YAGNI وACID والتزامن والحساب المالي الحتمي وIdempotency وAPI Contracts والأمان والمراقبة والأداء. استخدم عند تخطيط أو تعديل كود المرتبات أو مراجعة التصميم وحالات الاستخدام، فقط ضمن Issue معتمدة للتنفيذ.
---

# مهارة هندسة نظام مراجعة المرتبات — Gemini / Antigravity

## متى تستخدمها؟
استخدمها عند: تصميم Use Case أو Aggregate؛ تنفيذ Feature أو Bug Fix مُصرَّح؛ بناء Adapter لـPDF/Excel/DirectForm؛ قواعد الاستحقاق التاريخية؛ تصميم API؛ تخطيط Constraints/Migrations أو تحسين الأداء/الأمان. **التفعيل ليس إذنًا بكتابة كود.**

## 0. تفويض العمل قبل أي أمر
1. اقرأ `AGENTS.md` ثم `GEMINI.md`، و`docs/governance/DECISION-REGISTER.md` و`docs/governance/REVIEW-GATES.md` والـIssue المحددة.
2. تحقق من إذن المالك **الصريح** للكود والمهاجرات/النشر كل على حدة؛ `ADR` أو `SKILL.md` ليسا إذنًا.
3. اقرأ [المعايير التفصيلية](../../../docs/engineering/ENGINEERING-STANDARDS.md) و[تعريف الاكتمال](../../../docs/engineering/DEFINITION-OF-DONE.md)، و`ADR-003` و`ADR-004` و`ADR-005`، وحزمة EGM المعنية.
4. افحص `main HEAD` الحالي و`git status` وملفات المهمة؛ حافظ على الموجود ولا تتجاوز نطاق الفرع/Issue. إذا لم تتوفر صلاحية التنفيذ، قدّم **تصميمًا وخطة اختبارات فقط** ولا تُشغّل كودًا أو تغيّر CI/DB.

## 1. بروتوكول العمل في كل مهمة تنفيذ مصرح بها
- **Define:** اكتب `Bounded Context` و`Aggregate Root` أو Domain Service، الـInvariant، وحالة الاستخدام، المدخلات والمخرجات والأخطاء والعقود الزمنية، والمصدر القانوني الموثق عند الحاجة.
- **Risk-first:** حدّد المخاطر على `SourceAmount/ExpectedAmount/ReviewedAmount/OfficialCorrectionAmount/VerifiedPaidAmount`، والتواريخ، والهوية، وتكرار الرئيسي والتزامن والصلاحيات.
- **Test-first when meaningful:** اكتب سيناريوهات فشل/نجاح وتجارب Regression للقواعد المالية؛ اتبع Red–Green–Refactor متى أمكن فعليًا ولا تدّعِ حدوثه بلا سجل اختبارات.
- **Smallest safe change:** نفّذ أقل تغيير مكتمل؛ اتبع KISS وDRY وYAGNI وOOP/SOLID وComposition حيث يفيد. لا General Repository لكل جدول، ولا واجهة لكل كلاس، ولا Microservices.
- **Data correctness:** `decimal` و`RuleVersion` و`HistoricalSnapshot` و`RuleTrace` وسياسة التقريب؛ `UNKNOWN ≠ 0`. فرادة `MonthlyMainSalarySlot` ذريًا، مع السماح بـ`SEPARATE` متعددة وعدم دمج SourceOccurrences.
- **Safe I/O:** اختبر Retry/Idempotency وحدود الطلبات، تحقق من المستند والهوية والسلطة، وفصل `SourceIntake` عن `Personnel` و`Rules`.
- **Measure:** اختبارات مناسبة على قاعدة مختارة فعلًا، وعقود API، وأمان، وأداء بعينات اصطناعية. وثّق الأوامر والنتائج والخطوات التي لم تتم.
- **Hand-off:** Draft PR وربط Issue/ADR/EGM وDoD؛ لا Merge/Deploy من Gemini.

## 2. المعايير العشرة — مختصر يُراجع في كل مهمة
1. **TDD/Regression:** اختبار يُعيد إنتاج Bug قبل إصلاحه متى أمكن، واختبارات Unit/Integration/Contract وعلاقات مالية وسلوك فشل.
2. **KISS/DRY/YAGNI:** لا تكرار يسبب تناقضًا ولا abstraction غير مبرر، مع أولوية وضوح القواعد القانونية.
3. **DB Integrity/ACID/Concurrency:** قيود فريدة وForeign Keys ومعاملات ذرية وrowversion أو بديله؛ لا `check then insert` وحده.
4. **Deterministic Finance:** نفس snapshot والقانون والوعاء والتقريب ينتج نفس القيمة، بلا `float/double` نقدي أو قرارات حسب الوقت الحالي.
5. **Idempotency/Retry:** لا تكرر قرارًا أو حقًا بسبب إعادة استيراد؛ لا تُسقط مكافأتين منفصلتين صحيحتين متشابهتين.
6. **API/Error Contracts:** عقود واضحة وحالات خطأ مستقرة ومقروءة؛ لا استثناءات مُبتلعة أو نجاح وهمي.
7. **Security/Auditability:** Least Privilege، RBAC في الخادم، محفوظات قرار مالية مستقلة، لا بيانات موظفين حقيقية في PR/logs.
8. **Observability:** Structured Logs/Correlation IDs بعد إخفاء PII؛ Audit Trail ≠ Debug Log.
9. **Performance:** Pagination/Streaming/Batch والقياس قبل التحسين؛ حدود الذاكرة والوقت واختبارات الأحجام.
10. **CI/Safe Delivery:** Build/Lint/Tests/Secrets/Dependency scan حسب المخاطر والبيئة، وكل نتيجة مُبلَّغ بها بإثبات.

## 3. أنماط فشل خاصة بـPayrollReviewSystem
- مصدر Oracle ليس حقيقة موظف أو استحقاقًا معتمدًا، ولا يعني ظهور مبلغ في كشف أنه مدفوع.
- راتب MAIN منطقي واحد لكل علاقة عمل/وحدة/شهر، والتصحيح Revision لنفس Slot؛ `SEPARATE` متعددة؛ تكرار الكود في المصدر لا يعني تكرار صرفية.
- أحكام إجازات ودرجات تاريخية تحتاج مستندًا صالحًا وقت الاستحقاق؛ لا تفعّل نسبة أمثلة 156% و91% و48/60/200% دون نص معتمد.
- فرق استقطاع ليس دينًا مثبتًا أو استردادًا مسموحًا. `ReviewAdjustment` لا يمحو نتيجة الحساب أو أصل المستند.
- عند تعارض القانون/الهوية/السريان/الصلاحيات، أخرج `BLOCKED` وبيّن السؤال المطلوب من المالك.

## 4. مخرجات إلزامية قابلة للتحقق
قدم في نهاية المهمة: `Issue/PR/Branch/CommitSHA`؛ قائمة الملفات؛ `Context/Aggregate/Contract/Invariant`؛ جدول المخاطر؛ اختبارات `PASS/FAIL/NOT_RUN/N/A` **بدليل** (أمر + ملخص ناتج)؛ تغييرات قاعدة البيانات والأمن؛ القيود والأسئلة المفتوحة؛ وما يحتاج موافقة المالك. `NOT_RUN` لا تعني `PASS` و`N/A` لها مبرر.

للتقييم النهائي لطلب دمج الكود، استعمل مهارة `payroll-pr-quality-gate` ودليل [Definition of Done](../../../docs/engineering/DEFINITION-OF-DONE.md).
