# Definition of Done — بوابة استلام مهام Gemini

**الحالة:** معيار تسليم معتمد في [ADR-005](../decisions/ADR-005-engineering-quality-and-agent-skills.md). ليس إذن تنفيذ للكود أو CI أو SQL أو Deploy. يجب تطبيقه بحسب طبيعة المهمة وعدم الادعاء بمرور اختبارات غير منفذة.

## 1. تعريف حالات الأدلة
| الحالة | تعريفها | حكم الاستلام |
| --- | --- | --- |
| `PASS` | فحص محدد أجري فعليًا ونتيجته موثقة على `Commit SHA` أو Artifact | مقبول لهذه البوابة فقط |
| `FAIL` | فحص أجري وأثبت خللًا | يمنع دمج تغيير حساس حتى الإصلاح وإعادة الفحص |
| `NOT_RUN` | لم يجرِ الفحص أو لم نستطع التحقق من نتيجته | لا ينقلب إلى PASS؛ في بوابة جوهرية يمنع وصف PR بأنه جاهز |
| `N/A` | غير ذي صلة، مع شرح واضح لسبب عدم التطبيق | مقبول إذا كان التبرير صحيحًا |

**`Green CI` لا يغطي تلقائيًا كل البوابات المالية والهوية والقانون.**

## 2. قبل بدء البرمجة
- [ ] Issue صريحة **مصرح بها للتنفيذ**، والمالك وافق على أي SQL/CI/Deploy يتطلب قرارًا منفصلًا.
- [ ] مراجعة `main HEAD`, الفرع، `AGENTS.md`, `GEMINI.md`, ADRs، و`docs/README.md`.
- [ ] Goldfish مستقل مُوثَّق للحزمة الجوهرية عند انطباق شرط المراجعة؛ `NOT_RUN` ليست اجتيازًا.
- [ ] `Bounded Context`, `Aggregate`, `Invariant`, `Use Case`, `Contract`, `Transaction Boundary` محددة أو مُعلَّق قرارها.
- [ ] أنماط الـ`Failure` والـ`Test Cases` ومصادر القانون المتحققة محددة قبل تغيير الحساب.

## 3. وقت إعداد PR
| البوابة | دليل مطلوب عند انطباقها | مثال على سبب N/A مقبول |
| --- | --- | --- |
| Scope/Review | Issue وADR وحالة التفويض وملفات Diff الفعلية و`SHA` | لا N/A لطلب مراجعة كود |
| Design Quality | DDD/SOLID/KISS/DRY/YAGNI، ملكية Contexts وحدود Aggregates | مجرد تعديل تهجئة في Markdown |
| Financial Invariants | Unit tests للـMoney/RuleTrace/Rate/DateRange و`UNKNOWN`، بلا تغيير القيم التاريخية | تعديل CSS حصري |
| Data Integrity | Integration tests على DB Provider الحقيقي + Concurrent writes + Unique/Foreign Keys وIdempotency | لا مكونات تخزين تغيرت ولا أثر عليها |
| API/Adapters | Contract tests، حالات خطأ متميزة، PDF/Excel/DirectForm مع حفظ تكرار السطور | لا API/Adapter في النطاق ولا أثر عليها |
| Security/Privacy | فحص RBAC/IDOR/PII والأسرار وتصدير Excel الخطِر حسب الأثر | تعديل غير قابل للتنفيذ لوثائق عامة |
| Observability | Correlation IDs وStructured Logs بلا PII واختبار Failure path | لا تغييرات في I/O أو تسجيل ولا أثر |
| Performance | قياس وتفسير بالأحجام الاصطناعية ومقارنة قبل/بعد عند تغيير ساخن | تعديل غير مؤثر في مسار الحمل |
| CI/Build/Lint | أسماء الأوامر والنتائج و`SHA` ومبررات أي `NOT_RUN` | Markdown فقط لا يتطلب Build |
| Migration/Recovery | أثر schema وفقد البيانات وخطة backup/restore/rollback أو forward-fix المجرَّبة حسب القرار | لا Migration |
| Usability | RTL وعرض الأرقام/الأكواد والأصفار وأخطاء المستخدم وإتاحة الوصول حسب الشاشة | Backend أو docs فقط |
| Documentation | تحديث العقد أو المخطط والقرارات، وأمثلة اصطناعية صحيحة وروابط حية | لا N/A عندما يتغير Contract |
| Owner Gate | كل الاعتمادات المطلوبة موجودة، ولا سريان قانون/صرف/نشر تلقائي | لا N/A لتغيير قانوني حساس |

## 4. حد أدنى لتقرير PR
يجب على Gemini إرفاق جدول:

| Gate | Status | Evidence | Note/Follow-up |
| --- | --- | --- | --- |
| Unit Tests | `PASS/FAIL/NOT_RUN/N/A` | اسم الأمر/نتيجة/Commit SHA | أين نرى النتائج؟ |
| Integration DB | `PASS/FAIL/NOT_RUN/N/A` | Provider ونسخة اختبار بدون بيانات حقيقية | التزامن/فهرس |
| Security | `PASS/FAIL/NOT_RUN/N/A` | فحص RBAC/Privacy مناسب | مخاطر متبقية |
| CI/Build | `PASS/FAIL/NOT_RUN/N/A` | Run link أو command output | فشل/عدم تنفيذ |
| Data/Migration | `PASS/FAIL/NOT_RUN/N/A` | خطة/تجربة أو N/A مسبب | مالك القرار |

الصفوف السابقة **قالب وليس نتائج تشغيل**. أضف اختبارات `AC/PG/TR/FL/DDD/SOLID` ذات الصلة؛ لا تضع `PASS` إلا لنتيجة موثقة.

## 5. شروط اعتبار المهمة مكتملة
1. **Design-only:** توثيق متسق ومراجع، قرارات صريحة ومقترحات مميزة؛ لا يدعي نجاح Build أو EGM غير مثبت.
2. **Code-ready:** جميع البوابات الجوهرية `PASS` أو `N/A` مسبب؛ لا `FAIL` مانع أو `NOT_RUN` جوهري؛ الفروقات مغطاة باختبارات قابلة للتكرار.
3. **Merge-ready:** مراجعة مستقلة للـDiff الفعلي والـPR، حل الموانع، نتائج CI مرتبطة بنفس SHA، وإذن الدمج وفق تفويض المالك؛ **Gemini لا يدمج كوده بنفسه**.
4. **Deploy-ready:** تفويض نشر مستقل، خطة رجوع/Forward-Fix/Backup/Restore ومراقبة، واختبار على بيئة مخولة؛ لم يُمنح بهذا القرار.

## 6. توقف وتصعيد
إذا فشل تزامن فرادة المرتب، أو ظهر مصدر بلا نسب ثابتة، أو لم يُوثَّق حكم قانوني، أو انكشف رقم قومي أو سر، أو فُقد سجل مراجعة/مستند أصلي، أوقف **المسار المتأثر** وأبلغ المالك بالمشكلة وخطوات إعادة الإنتاج بدون بيانات حقيقية. لا تنشئ ترقيعًا أخفى الفشل أو يسوِّي أرقامًا بالتقريب الشامل.
