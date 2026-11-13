# الذاكرة المعمارية لمشروع مراجعة المرتبات (PROJECT-MEMORY-AR)

> **الحالة:** تم اعتماد ودمج الهيكل التقني الأولي لمشروع **PayrollReviewSystem** عبر [PR #49](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/49) (`656097a2e9fe35c14dceb98f7daeb819355c0097`)، ثم دمج دراسة المرحلة أ لـ **[Issue #50 — DB-COMPAT-001](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/50)** عبر [PR #51](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/51) و[PR #52](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/52) (`6ad3419c62850fcc7442a61da14726ded6f07963`)، ثم دمج خطة المعمل المستقل **[Issue #54 — LAB-PLAN-001](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/54)** عبر [PR #55](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/55) (`62aa6280006afdb75abcabe5a78aeb4719a55455`). وبموجب قرار المالك **[Issue #56 — OWNER-PRIORITY-001](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/56)** تأجل مسار `VM/SQL Phase B` (`NO_PROVIDER_SELECTED / NO_TABLES / PHASE_B_NOT_RUN` مع إيقاف [Issue #53](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/53) عند `BLOCKED_ISOLATION / BLOCKED_TLS_BUILD`)، ودُمجت مهمة التحكيم التوثيقي **[Issue #57 — EGM-ADJ-001](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/57)** عبر [PR #58](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/58) (`913963ef6c28ca5be243cfa96b50bc413a9582ac`؛ `REPORTS_PRESENT_INDEPENDENCE_UNVERIFIED`)، كما دُمجت مواصفة **[Issue #59 — SPEC-029-001](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/59)** عبر [PR #60](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/60) (`fb0e511e7ba16559e3b0adbf205dfae47a3bf25d`)، ودُمج تحديث **[Issue #61 — SPEC-029-002](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/61)** عبر [PR #62](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/62) (`MERGED` عند `df57f10b2431b60d4c24f4975d8468c42440fbd5`)، ثم دُمج تحديث **[Issue #63 — SPEC-029-003](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/63)** عبر [PR #64](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/64) (`MERGED` عند `b415bb802645b80429c16bdeb35635ab4489bf77`) لتثبيت قرارات `D-034` (`OD-04` في `AI-0075..AI-0083`). وتُحدَّث المواصفة وسجل القرارات حاليًا عبر **[Issue #65 — SPEC-029-004](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/65)** لإدراج قرارات `D-035` (`OD-05`: `AI-0091 → AI-0092 [SUPERSEDED_BY_AI-0093] → AI-0093` — نظام المراجعين المرن وتسجيل التعديلات والتسويات باسم منفذها دون انتظار مراجع أساسي) في `docs/specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md` مع الإبقاء على `STATUS: NOT_DESIGNED / NO_TABLES` (`SPEC_DOCS_ONLY / NO_CODE / NO_DDL / NO_MERGE`).  
> **تنبيه الحوكمة:** هذا الملف هو المرجع الثابت لخريطة المشاريع والتبعيات وحالة قاعدة البيانات، **ويجب تحديثه في كل Pull Request لاحق يغير العلاقات أو يضيف مكونات برمجية**.

---

## 1. النمط المعماري العام
يلتزم النظام بالقرارات المعمارية المعتمدة للمالك:
- **`ADR-003` (D-024):** تطبيق مبادئ `Domain-Driven Design (DDD)` وهيكل `Modular Monolith` وحدود الاعتماد `Clean Architecture`.
- **`ADR-004` (D-025):** الالتزام بمبادئ البرمجة كائنية التوجه (`OOP`) ومعايير `SOLID`.
- **`ADR-005` (D-026):** معايير هندسة البرمجيات (`TDD`, `Deterministic Money`, `KISS/DRY/YAGNI`).

---

## 2. خريطة المشاريع وأسماء الملفات المختصرة
بناءً على التوجيه الصريح للمالك بعدم استخدام الأسماء الطويلة المتكررة، تم اعتماد الأسماء المختصرة التالية:

| المكون / المشروع | المسار الفعلي | نوع المشروع | التبعيات المسموح بها | الوصف الفعلي في `BOOT-001` والدور المستقبلي |
| :--- | :--- | :--- | :--- | :--- |
| **الحل (Solution)** | `PayrollReviewSystem.sln` و `PayrollReviewSystem.slnx` | Solution File | يشمل كافة المشاريع | الحل المركزي الجامع لكافة مشاريع الواجهة الخلفية والاختبارات |
| **النواة (Core)** | `src/core/core.csproj` | Class Library (`net10.0`) | **لا شيء** (صفر تبعيات خارجية) | تحوي حاليًا `CoreMarker` فقط لإثبات الربط المعماري؛ **وستحوي لاحقًا** (بعد اجتياز بوابات التصميم وEGM وتفويض المالك) نماذج النطاق والكائنات القيمية الخالصة |
| **التطبيق (Application)** | `src/application/application.csproj` | Class Library (`net10.0`) | `core` فقط | تحوي حاليًا `ApplicationMarker` فقط؛ **وستنسق لاحقًا** حالات الاستخدام (`Use Cases`) والعقود والواجهات المستقلة |
| **البنية التحتية (Infrastructure)** | `src/infrastructure/infrastructure.csproj` | Class Library (`net10.0`) | `core`, `application` | تحوي حاليًا `InfrastructureMarker` فقط، وهي خالية تمامًا في `BOOT-001` من أي اتصالات أو ORM؛ **وستنفذ لاحقًا** المحولات الخارجية المعتمدة |
| **واجهة الويب (API)** | `src/api/api.csproj` | Web API Minimal (`net10.0`) | `application`, `infrastructure` | نقطة الدخول واستضافة الخدمات وتقديم `/health` و `/api/info` (دون كشف أسماء أنواع CLR الداخلية) |
| **الواجهة الأمامية (Client)** | `client/` | Angular 22 Standalone | مستقلة عبر HTTP / JSON | واجهة مستخدم ويب خفيفة تدعم اللغة العربية والاتجاه من اليمين لليسار (`dir="rtl"`, `lang="ar"`) مع تفعيل `strict` و `strictTemplates` |
| **اختبارات الوحدة (Unit Tests)** | `tests/unit-tests/unit-tests.csproj` | xUnit Test (`net10.0`) | `core`, `application`, `api` | اختبارات المكونات الفنية ونقاط الفحص التشغيلي (`/health` و `/api/info`) |
| **الاختبارات المعمارية (Architecture Tests)** | `tests/architecture-tests/architecture-tests.csproj` | xUnit Test (`net10.0`) | `core`, `application`, `infrastructure`, `api` | حارس معماري مزدوج: يفحص ملفات `*.csproj` ثابتًا (`ProjectReference` و `PackageReference`) مع حالات سلبية اصطناعية، ويفحص التجميعات المترجمة (`Assembly`) وقت التشغيل |

---

## 3. مخطط اتجاه التبعيات (Dependency Rule)
تتدفق التبعيات بدقة من الخارج نحو الداخل:

```text
[API (src/api)] --------> [Infrastructure (src/infrastructure)]
       |                                   |
       v                                   v
[Application (src/application)] --------> [Core (src/core)]
```

### القواعد الصارمة المفروضة برمجيًا عبر الاختبارات المعمارية (`CleanArchitectureDependencyTests`):
1. `core` لا تعتمد إطلاقًا على `application` أو `infrastructure` أو `api`، ولا تقبل أي حزم خارجية (`PackageReference`).
2. `application` تعتمد حصريًا على `core`؛ وتُحظر تبعيتها لـ `infrastructure` أو `api` أو حزم ORM الخارجية.
3. `infrastructure` تعتمد على `core` و `application`؛ وتُحظر تبعيتها لـ `api`، كما يُحظر في `BOOT-001` إضافة أي حزم قواعد بيانات أو ORM (`EntityFrameworkCore`, `Dapper`, `SqlClient`).
4. يتم التحقق من هذه القواعد عبر:
   - **فحص ثابت لملفات XML الخاصة بـ `*.csproj`** يلتقط أي `ProjectReference` أو `PackageReference` محظور حتى لو لم يُستخدم في الكود، مدعومًا بـ **5 حالات اختبار سلبية اصطناعية** تثبت فشل الحارس فورًا عند وجود تبعية ممنوعة.
   - **فحص بيانات التجميع المترجمة (`Assembly.GetReferencedAssemblies()`)** كمكمّل وقت التشغيل.

---

## 4. القرارات التقنية والبيئية المعتمدة

| الطبقة | التقنية المعتمدة | الإصدار المثبت والمتحقق منه | الملاحظات وقرارات المالك |
| :--- | :--- | :--- | :--- |
| **الخادم (Backend)** | **.NET 10 LTS** | SDK `10.0.404` (`net10.0`) | إعداد `global.json` و `Directory.Build.props` بصرامة |
| **الواجهة (Frontend)** | **Angular 22** | Angular `22.2.6`, TypeScript `6.0.2`, Node `24.15.0` | تطبيق خفيف معزول يدعم RTL واللغة العربية مع `strict` و `strictTemplates` |
| **قاعدة البيانات (Database)** | **SQL Server 2014 (12.x)** | مستهدف في مراحل لاحقة | **قرار مالك صريح** في Issue #39 و #41 و #48 (#47)؛ لا تفترض SQL 2022 |
| **الوصول للبيانات (ORM)** | **NOT_SELECTED / NOT_IMPLEMENTED** | غير محدد حتى اعتماد `ADR` | لا `EF Core` ولا `Dapper` ولا `SqlClient` في الإنتاج حاليًا؛ راجع [دراسة التوافق](research/SQL2014-DATA-ACCESS-COMPATIBILITY-AR.md) و[خطة المعمل المستقل LAB-PLAN-001](research/SQL2014-ISOLATED-LAB-PLAN-AR.md) (`Issue #53` متوقف `BLOCKED_ISOLATION / BLOCKED_TLS_BUILD` و`TC01..TC11: NOT_RUN`) |

---

## 5. حالة جداول وعلاقات قاعدة البيانات

```text
STATUS: NOT_DESIGNED / NO_TABLES
```

- **لا توجد أي جداول أو علاقات أو سكريبتات SQL أو ملفات ترحيل (Migrations) في هذه المرحلة.**
- **ممنوع تخمين أو ابتداع أي جداول** أو افتراض Schema حتى يتم إقرار تصميمها بقرار معماري مستقل (ADR) وتفويض تنفيذي منفصل من المالك.

---

## 6. حدود ما لم يُنفذ (خارج نطاق `BOOT-001`)
- لم يتم بناء أي منطق مالي أو قواعد لحساب المرتبات أو الضرائب أو المعاشات.
- لم يتم تضمين أي أدوات لاستخراج ملفات Oracle PDF أو Excel.
- لم يتم إنشاء شاشات لإدخال أو عرض بيانات الموظفين الحقيقية أو الاصطناعية.
- لم يتم بناء نظام المصادقة والصلاحيات (`RBAC/Auth`) أو السجلات المهيكلة المتقدمة (`Structured Logging`) أو اختبارات الحمل (`Performance/Load`) في هذا الهيكل الأولي.
- لا توجد أي أسرار أو مفاتيح API أو سلاسل اتصال مشفرة أو صريحة في المشروع.

---

### خريطة مواصفات ودورة عمل الملاحظات وأوامر التسوية (`SPEC-029-001..004`)

- **الوثيقة المرجعية:** [AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md](specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md) وتُوثّق قرارات المالك المتسلسلة `D-029..D-035` في [DECISION-REGISTER.md](governance/DECISION-REGISTER.md).
- **المسار الكامل للتوثيق:**
  1. `SPEC-029-001` ([Issue #59](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/59) / [PR #60](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/60) — `MERGED`): دورة `Finding` و`FinancialFollowUp` وسبعة الحقول المالية المستقلة وعقود `Core/Application` (`NOT_DESIGNED / NO_TABLES`).
  2. `SPEC-029-002` ([Issue #61](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/61) / [PR #62](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/62) — `MERGED` عند `df57f10b2431b60d4c24f4975d8468c42440fbd5`): `D-031` (الشخص الواحد المخوّل للقرارات الأربع)، `D-032` (اعتماد المرتب مع استمرار التنبيه في الشهر التالي عبر `FindingId`)، `D-033` (الإغلاق بإفادة شفهية موثقة منفصلًا عن التسوية المثبتة بمستند، وفتح `Finding` جديدة عند دليل ناقض دون إعادة فتح المتابعة المغلقة).
  3. `SPEC-029-003` ([Issue #63](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/63) / [PR #64](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/64) — `MERGED` عند `b415bb802645b80429c16bdeb35635ab4489bf77`): `D-034 / OD-04` (`AI-0075..AI-0083`) — استقلال اعتماد مراجعة `MAIN_SALARY` عن `SEPARATE` (`AI-0075`)، ومعاملة استمارة `132 ع.ح` الخاصة برعاية الطفل كمعاملة مراجعة مستقلة عن المرتب الرئيسي دون افتراض تصنيفها الفني (`AI-0076`)، وعدم التأثير الآلي للصرفية المنفصلة على المرتب الرئيسي مع إنشاء أمر تسوية صريح للشهر التالي تنفذه المركزية ويراجعه المراجع في كشف Oracle (`AI-0077`, `AI-0078`)، وإدارة `SettlementOrder` `[PROPOSED]` (الاحتفاظ بنفس الأمر مفتوحًا عند `0` أو رصد جزئي `AI-0079..0080`، الإغلاق الإداري `AdministrativeOrderClosed` عند الاكتمال التراكمي في Oracle دون اعتباره سدادًا بنكيًا `AI-0081`؛ وعند كشف مصحح لاحق: إبقاء الأمر القديم مغلقًا وفتح `Finding` جديدة مرتبطة به `AI-0082`، ثم إنشاء أمر تسوية جديد بقيمة الجزء الملغى للشهر التالي مرتبطًا بالأمر القديم (`RelatedPreviousOrderId` — `PROPOSED`) والملاحظة الجديدة ونسخة التصحيح دون تكرار أصل الالتزام `AI-0083`).
  4. `SPEC-029-004` ([Issue #65](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/65)): `D-035 / OD-05` (`AI-0091 → AI-0092 [SUPERSEDED_BY_AI-0093] → AI-0093`) — تخفيف إلزام مسار رد المركزية والاكتفاء بتقدير المراجع مع حفظ قناتَي `D-030` عند ورود رد (`AI-0091`)، وإلغاء شرط موافقة «المراجع الأساسي» من `AI-0092` بحكم `AI-0093` ليعمل أي مراجع مخوّل ضمن نطاقه دون توقف على مراجع بعينه، مع سجل غير مدمّر للفاعل والتوقيت وقيم قبل/بعد (`VariableChangeAuditEntry` — `PROPOSED`) دون خلطه بإثبات السداد المالي المستقل (`D-033`) ودون توسيع `D-031`.
- **تأكيد الجاهزية:** لا تزال المرحلة التوثيقية دون جداول فعلية (`STATUS: NOT_DESIGNED / NO_TABLES`) ودون توسيع لصلاحيات `D-031` الفردية أو اعتماد DDL/كود تشغيلي.

---

## 7. أوامر التشغيل والبناء والاختبار (Runbook)

### الواجهة الخلفية (.NET 10 LTS):
```bash
# استعادة الحزم
dotnet restore PayrollReviewSystem.sln

# البناء
dotnet build PayrollReviewSystem.sln --no-restore --configuration Release

# تشغيل الاختبارات المعمارية ووحدة الخادم
dotnet test PayrollReviewSystem.sln --no-build --configuration Release --verbosity normal

# تشغيل خادم API محليًا
dotnet run --project src/api/api.csproj
```

### الواجهة الأمامية (Angular 22):
```bash
# تثبيت الحزم
cd client
npm ci

# بناء الواجهة
npm run build

# تشغيل اختبارات الواجهة
npm test -- --watch=false

# تشغيل خادم الواجهة محليًا
npm start
```

---

## 8. خريطة التصميم الموحّد وتسليم الكود — ChatGPT Design Ownership

**المرجعية المعتمدة:** [مواصفة دورة المراجعة §9](specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md) و[Blueprint v1 في Issue #65](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/65#issuecomment-6860157620). **ChatGPT مسؤول عن التصميم والقرارات والمواصفات** طبقًا لتوجيه المالك [هنا](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6860077279)؛ **Gemini منفذ كود فقط** بعد تكليف صريح منفصل. يوجد [Draft PR #66](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/66) لتحديث وثائق `SPEC-029-004`، **لم يُدمج** عند صياغة هذه الإضافة؛ لا تستنتج وجود التصميم في `main` قبل تحقق دمج GitHub فعلي.

### 8.1 العلاقات التصورية بين نموذج الأعمال والكود

| المسار/المفهوم المقترح | موضعه المستقبلي | علاقته بالكيانات الأخرى وحالة الاعتماد |
| :--- | :--- | :--- |
| `Payee / PayeeRelationship / PayrollCycle` | `src/core` | الهوية الوظيفية وفترات الاستحقاق، مع `MonthlyGroupMembership` للنطاق؛ المفاتيح النهائية وتعدد علاقات العمل `OPEN` |
| `PayrollDisbursement / SourceVersion` | `src/core` + منافذ إدخال في `src/application` | صرفية `MAIN_SALARY / SEPARATE` ونسخة مصدر `Oracle / EXCEL_IMPORT / DIRECT_FORM`؛ مستند Oracle الرسمي لا يُستنتج من Excel/إدخال مباشر |
| `AuditCase / ReviewVariableRevision / ReviewerActionLog` | `src/core`, `src/application` | تغير المتغير يولد نسخة وسجل عملية باسم منفذها؛ **لا بوابة مراجع أساسي** (`D-035`)، وسجل الفاعل والنسخ `PROPOSED` |
| `Finding / FindingRevision / ReviewDecision` | `src/core` | ملاحظة وإصدار وقرار مع أدلته؛ بقاء تنبيه `Finding` مفتوحة عند اعتماد البنود السليمة `D-032` |
| `FollowUp / SettlementOrder` | `src/core` | المتابعة وأوامر الشهر التالي منفصلة؛ التصحيح الرسمي بعد الإغلاق الإداري يربط `OldOrder → CorrectedOracle → Finding → NewOrder` دون مديونية ثانية |
| `IAuthorizationPolicyPort / IIdempotencyPort / IRepository` | تعريف في `src/application` وتنفيذ لاحق في `src/infrastructure` | هوية فاعل الخادم، حدود الوصول، `ExpectedVersion` و`IdempotencyKey`، لا ORM ولا DB حاليًا |
| `src/api` و`client` | API وAngular RTL | عرض الإجراءات المسموحة لا يغني عن تفويض الخادم؛ `ActorId` غير مستمد من حقل قابل للتلاعب في Angular |

**مسار التبعيات:** `client → api → application → core`، وتنفيذ منافذ التطبيق في `infrastructure` اعتمادًا على `application/core`. ممنوع `core → infrastructure` أو منطق حساب مالي في Angular/Controller. جميع أسماء Domain هنا **مخطط مفاهيمي لا جداول SQL**.

### 8.2 خريطة علاقة السجلات والأدلة والتصحيح

- **سجل القيمة:** `AuditCase → ReviewVariableRevision → SourceVersion`، وكل تغير `ReviewerActionLog(PerformedByActorId, PerformedAt, OldValue, NewValue, Reason)` بوصفه `PROPOSED`؛ لا محو تاريخ أو نسبة الإجراء لمراجع آخر.
- **سجل الملاحظة:** `AuditCase → Finding → FindingRevision → ReviewDecision`، وقد تنشأ `FollowUp` للفرق الحقيقي وفق القرار، مع استمرار `PersistentFindingNotice` غير المغلقة.
- **قنوات الرد التاريخية:** يحافظ التصميم على `CentralInternalReply` و`RecordedExternalReply` وفق `D-030`؛ تسجيل رد مركزي عند وروده متاح، لكنه ليس خطوة إلزامية لكل مراجعة بقرار `AI-0091`، ولا يُقبل الرد تلقائيًا كإثبات صحة الاستحقاق.
- **سجل التسوية:** `OriginDisbursement → SettlementOrder A → OracleSourceVersion`؛ إذا كان A مغلقًا إداريًا ثم ألغى كشف رسمي مصحح 50 من 200: `A` يظل مغلقًا، `Finding F` جديدة، ثم `Order B=50` للشهر التالي، مرتبط بـ A وF والتصحيح. عدم الظهور أو الظهور الجزئي أثناء فتح الأمر لا ينشئ أمرًا ثانيًا.
- **إثبات المال:** `SourceAmount / ExpectedAmount / ReviewedAmount / OfficialCorrectionAmount / ReportedDisbursementAmount / VerifiedPaidAmount / OutstandingAmount` **سبعة مفاهيم منفصلة**؛ الظهور في Oracle أو التسجيل الإداري أو رأي المراجع **ليس** إثبات `VerifiedPaidAmount`.
- **القوانين:** `RuleSet → RuleEffect → EffectivePeriod → LegalReference` كتصور هندسي `PROPOSED`؛ لا حساب تلقائي لقوانين غير موثقة ولا تقريبات مفترضة.

### 8.3 سجل اختبار وتسليم كل شريحة

| الشريحة المستقبلية | الحد الأول | متطلبات قبول التصميم قبل البرمجة |
| :--- | :--- | :--- |
| `CODE-001` | `core/application` للمتغيرات وسجل المراجعين | غياب مراجع لا يوقف العمل، `PermissionDenied` خارج النطاق، تاريخ before/after، `VersionConflict`، `IdempotencyConflict`، `UNKNOWN != 0` |
| `CODE-002` | `AuditCase/Finding/ReviewDecision` | `D-031` للعمليات الأربع فقط، مراجعة صرفية مستقلة، تنبيه مستمر، رد المركزية اختياري عند عدم وجود إلزام خاص |
| `CODE-003` | `FollowUp/SettlementOrder` | صفر/جزئي/كامل Oracle، إغلاق إداري فقط، `A=200 → F → B=50`، عدم رفع `VerifiedPaidAmount` بلا سند |
| `CODE-004` | `api/client/infrastructure` | API/RBAC من الخادم وRTL، ثم SQL Server 2014 اصطناعي بمزود بيانات يخضع لدراسة توافق مستقلة |

**الاختبارات الحالية لهذه الخريطة:** `DESIGN_ONLY / NOT_RUN`، ولا يوجد دليل على تشغيل اختبار Core جديد أو SQL. لا توجد جداول أو مفاتيح خارجية معتمدة: `STATUS: NOT_DESIGNED / NO_TABLES`. يجب تحديث هذه الخريطة مع **كل PR كود مُدمج فعليًا** وربط أسماء الأصناف والأساليب والجداول ومفاتيحها بالـcommit والاختبارات؛ قبل وجودها لا تُخترع مسارات أو علاقات DB.

### 8.4 حدود القرارات الباقية

`OWNER_DECISION_REQUIRED`: أنواع إثبات السداد المالي المستقل والمخوّل بالتحقق، قواعد الضرائب/التأمينات/المعاشات والإجازات الموثقة زمنيًا، علاقات العمل المتعددة، تفويض اعتماد أو إلغاء `SettlementOrder`، أثر تعديل قيمة سبق اعتماد مراجعتها، والتصحيحات المتعارضة أو الزائدة في Oracle. **اختيار أسماء classes والمنافذ واختبارات التزامن من اختصاص التصميم الهندسي ChatGPT** ولا يحتاج إذنًا ماليًا بحد ذاته، أما تحويل قاعدة حسابية جديدة إلى `OWNER_CONFIRMED` فيتطلب قرار المالك أو سندًا رسميًا مناسبًا.

---

## 9. تحديث نقطة التوقف ونطاق المكونات الجديدة (13 نوفمبر 2026)

**حالة GitHub المؤكدة في هذه الجلسة:**
- دُمجت وثائق `SPEC-029-004` في [PR #66](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/66) عند `837a822e4767c0c0f5d9fcf817e439d1b038c0d8`؛ الجملة التاريخية في §8 عن كون PR #66 Draft تخص وقت كتابتها فقط.
- `CODE-001`: [Issue #67](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/67) مفتوحة، [PR #68](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/68) ما زال Draft غير مدمج عند `8f989136b61490473264b6a84c65c5b74f501715`.
- أنشئ فرع تطوير منفصل `feat/owner-autonomous-20261113` و[Draft PR #69](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/69) المبني على تغييرات PR #68؛ آخر SHA مفحوص `60efc9a4087606183a7aacfd2a5886a21f79c22e`. إصلاح عزل `IdempotencyRecordKey` بحسب `TrustedActorId`، مع Regression لمراجعين مختلفين. [GitHub Actions run 43499684636](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/actions/runs/43499684636) **SUCCESS**: 20 architecture + 35 unit، .NET build بلا أخطاء/تحذيرات وAngular baseline ناجح. ذلك دليل على الاختبارات المكتوبة **وليس** اعتمادًا فنيًا كاملًا أو DB أو Merge.
- PR #68 وPR #69 يتضمنان تغييرات متداخلة؛ **لا يُدمجان معًا من دون خطة توحيد**. `main` لم يتضمن CODE-001 عند آخر تحقق. لا قاعدة بيانات منفذة مثبتة.

**توجيه المالك الجديد — [D-036](governance/DECISION-REGISTER.md):**
- اشتقاق حقول المستفيد والقواعد اللازمة من التشريعات واللوائح بعد التحقق من نطاقها، بدل مطالبة المالك بقائمة يدوية.
- اكتشاف البنود من PDFs وإضافتها آليًا إلى كتالوج `NeedsReview`، وليس إلى GFMIS الرسمي أو FinancialComponent معتمد بغير مراجعة.
- الاستناد إلى شجرة GFMIS المنشورة (9 مقاطع)، مع الفصل عن `SourceAccountCode` و`FinancialComponent` وتاريخ الإصدار.
- بناء `Dynamic Administrative Orders Engine` يعرّف المستخدم المخوّل أنواعه وحقوله وبنوده وقوالبه داخل النظام. التفاصيل التقنية `PROPOSED` ومصفوفات اعتماد/أثر كل أمر `OPEN`.

**المرجع التصميمي:** [مواصفة الحقول، GFMIS، اكتشاف البنود ومحرك الأوامر الديناميكي](specs/OWNER-DYNAMIC-ORDERS-GFMIS-AND-PERSONNEL-AR.md). تمت معاينة بنية ثلاثة مصادر PDF خاصة (70 + 68 + 295 صفحات) **بدون نسخ بيانات الموظفين إلى GitHub**؛ لا استخراج شامل معتمد أو كتالوج بنود نهائي. تم العثور على مراجع منشورة للشجرة والمقطع الاقتصادي لكن ملف `5.xlsx` لم يُتحقق من محتواه بعد.

**نقطة الاستئناف:** إغلاق مراجعة CODE-001 على مسار PR واحد دون دمج أو توسيع، ثم تحويل مواصفات `Personnel`, `FinancialComponent/ChartOfAccounts`, `SourceIntake`, `AdministrativeOrders` إلى وحدات تنفيذية لها Issues مستقلة بعد حسم بوابات السلطة والأدلة. لا تدّعِ اكتمال دراسة جميع القوانين أو نسخة إنتاجية نهائية.
