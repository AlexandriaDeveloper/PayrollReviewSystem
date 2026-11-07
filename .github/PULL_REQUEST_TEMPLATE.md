# وصف طلب المراجعة

## المهمة
رقم GitHub Issue والقرار أو ADR المعتمد.

## لماذا التعديل؟
المشكلة الفعلية والسلوك السابق والأثر في مراجعة الصرفيات.

## ما الذي تغيّر؟
الملفات والكلاسات والجداول والواجهات ومخططات ERD/UML المتأثرة.

## بيانات الإدخال والمخرجات
أمثلة اصطناعية قبل التعديل وبعده، بما فيها حالات الفشل.

## الاختبارات الفعلية
- الأمر المستخدم:
- النتيجة:
- عدد الاختبارات الناجحة/الفاشلة:
- حدود الاختبار والمصادر غير المتاحة:

## سلامة البيانات
هل التغيير يحافظ على PDF الأصلي والسجل التاريخي؟ وهل يمنع تكرار أو دمج البنود خطأً؟

## الأمان والخصوصية
الصلاحيات، التسريب المحتمل، الأسرار، وإخفاء البيانات الحساسة.

## تغييرات قاعدة البيانات
هل توجد Migration؟ وهل تحتاج اعتمادًا منفصلًا؟ وهل الرجوع للإصدار السابق آمن؟

## أمور لم تُحسم
كل خلل أو مصدر قانوني أو اعتماد يحتاج تدخلًا.

## حالة المراجعة
لا تدمج التغييرات البرمجية بنفسك. ملفات Markdown فقط تخضع لتفويض ChatGPT بالمراجعة والدمج.

## Engineering Quality Evidence Matrix — ADR-005
املأ الجدول عند PR برمجي. **لا تكتب PASS دون أمر تشغيل/رابط نتيجة وSHA**؛ `N/A` يحتاج سببًا. في PR توثيق، ضع `N/A` للفحوص غير المناسبة.

| Gate | Status: PASS/FAIL/NOT_RUN/N/A | Evidence: Command/Run/SHA | Risk/Reason |
| --- | --- | --- | --- |
| Scope + Owner Authorization | | | |
| DDD/OOP/SOLID + KISS/DRY/YAGNI | | | |
| Financial Unit/Regression/Determinism | | | |
| DB Integrity/Concurrency/Idempotency | | | |
| API/Adapter Contract + Error Handling | | | |
| Security/RBAC/IDOR/PII | | | |
| Structured Logging/Observability | | | |
| Performance/Load (if relevant) | | | |
| Build/Lint/CI/Dependency/Secrets | | | |
| Migration/Backup/Restore (if relevant) | | | |
| Goldfish evidence for major design change | | | |

راجع `docs/engineering/DEFINITION-OF-DONE.md`، واستخدم `.agents/skills/payroll-pr-quality-gate/SKILL.md` لتجهيز الفحص. Gemini لا يدمج كوده ولا ينشره.
