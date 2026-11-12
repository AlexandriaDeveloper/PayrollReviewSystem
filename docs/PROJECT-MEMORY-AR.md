# الذاكرة المعمارية لمشروع مراجعة المرتبات (PROJECT-MEMORY-AR)

> **الحالة:** تم اعتماد ودمج الهيكل التقني الأولي لمشروع **PayrollReviewSystem** عبر [PR #49](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/49) (`656097a2e9fe35c14dceb98f7daeb819355c0097`)، ثم دمج دراسة المرحلة أ لـ **[Issue #50 — DB-COMPAT-001](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/50)** عبر [PR #51](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/51) و[PR #52](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/52) (`6ad3419c62850fcc7442a61da14726ded6f07963`). وتوقف فحص البيئة الأولي **[Issue #53 — DB-COMPAT-002](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/53)** عند `BLOCKED_ISOLATION / BLOCKED_TLS_BUILD`، وأُعدّت خطة المعمل المستقل **[Issue #54 — LAB-PLAN-001](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/54)** للتوثيق فقط (`NO_PROVIDER_SELECTED / NO_TABLES / PHASE_B_NOT_RUN`) بتاريخ 13 نوفمبر 2026.  
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
