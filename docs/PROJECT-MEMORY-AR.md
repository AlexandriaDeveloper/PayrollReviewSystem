# الذاكرة المعمارية لمشروع مراجعة المرتبات (PROJECT-MEMORY-AR)

> **الحالة:** معتمد كمرجع أساسي للهيكل التقني الأولي لمشروع **PayrollReviewSystem** بموجب المهمة **BOOT-001** (12 نوفمبر 2026).  
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

| المكون / المشروع | المسار الفعلي | نوع المشروع | التبعيات المسموح بها | الوصف والوظيفة |
| :--- | :--- | :--- | :--- | :--- |
| **الحل (Solution)** | `PayrollReviewSystem.sln` و `PayrollReviewSystem.slnx` | Solution File | يشمل كافة المشاريع | الحل المركزي الجامع لكافة مشاريع الواجهة الخلفية والاختبارات |
| **النواة (Core)** | `src/core/core.csproj` | Class Library (`net10.0`) | **لا شيء** (صفر تبعيات خارجية) | تحوي نماذج النطاق وقيم القيمة الخالصة ومفاهيم الحساب المالي المجردة |
| **التطبيق (Application)** | `src/application/application.csproj` | Class Library (`net10.0`) | `core` فقط | تنسق حالات الاستخدام (Use Cases) والعقود والواجهات المستقلة |
| **البنية التحتية (Infrastructure)** | `src/infrastructure/infrastructure.csproj` | Class Library (`net10.0`) | `core`, `application` | تنفيذ المحولات الخارجية، خالية تمامًا في BOOT-001 من أي اتصالات أو ORM |
| **واجهة الويب (API)** | `src/api/api.csproj` | Web API Minimal (`net10.0`) | `application`, `infrastructure` | نقطة الدخول واستضافة الخدمات وتقديم `/health` و `/api/info` |
| **الواجهة الأمامية (Client)** | `client/` | Angular 22 Standalone | مستقلة عبر HTTP / JSON | واجهة مستخدم ويب تدعم اللغة العربية والاتجاه من اليمين لليسار (RTL) |
| **اختبارات الوحدة (Unit Tests)** | `tests/unit-tests/unit-tests.csproj` | xUnit Test (`net10.0`) | `core`, `application`, `api` | اختبارات المكونات الفنية ونقاط الفحص التشغيلي |
| **الاختبارات المعمارية (Architecture Tests)** | `tests/architecture-tests/architecture-tests.csproj` | xUnit Test (`net10.0`) | `core`, `application`, `infrastructure`, `api` | تحقق تلقائي صارم من اتجاه التبعيات وعدم اختراق طبقات النواة |

---

## 3. مخطط اتجاه التبعيات (Dependency Rule)
تتدفق التبعيات بدقة من الخارج نحو الداخل:

```text
[API (src/api)] --------> [Infrastructure (src/infrastructure)]
       |                                   |
       v                                   v
[Application (src/application)] --------> [Core (src/core)]
```

### القواعد الصارمة المفروضة برمجيًا عبر الاختبارات المعمارية:
1. `core` لا تعتمد إطلاقًا على `application` أو `infrastructure` أو `api`.
2. `application` تعتمد حصريًا على `core`؛ وتُحظر تبعيتها لـ `infrastructure` أو `api`.
3. `infrastructure` تعتمد على `core` و `application`؛ وتُحظر تبعيتها لـ `api`.
4. أي محاولة لكسر هذه القواعد تُفشل فورًا الـ Build والـ CI.

---

## 4. القرارات التقنية والبيئية المعتمدة

| الطبقة | التقنية المعتمدة | الإصدار المثبت والمتحقق منه | الملاحظات وقرارات المالك |
| :--- | :--- | :--- | :--- |
| **الخادم (Backend)** | **.NET 10 LTS** | SDK `10.0.404` (`net10.0`) | إعداد `global.json` و `Directory.Build.props` بصرامة |
| **الواجهة (Frontend)** | **Angular 22** | Angular `22.2.6`, TypeScript `6.0.2`, Node `24.15.0` | تطبيق خفيف معزول يدعم RTL عبر Vitest و `@angular/build` |
| **قاعدة البيانات (Database)** | **SQL Server 2014 (12.x)** | مستهدف في مراحل لاحقة | **قرار مالك صريح** في Issue #39 و #41؛ لا تفترض SQL 2022 |
| **الوصول للبيانات (ORM)** | **NOT_SELECTED / NOT_IMPLEMENTED** | غير محدد في BOOT-001 | لا `EF Core` ولا `Dapper` حاليًا؛ توافق SQL 2014 يتطلب اختبارًا منفصلًا |

---

## 5. حالة جداول وعلاقات قاعدة البيانات

```text
STATUS: NOT_DESIGNED / NO_TABLES
```

- **لا توجد أي جداول أو علاقات أو سكريبتات SQL أو ملفات ترحيل (Migrations) في هذه المرحلة.**
- **ممنوع تخمين أو ابتداع أي جداول** أو افتراض Schema حتى يتم إقرار تصميمها بقرار معماري مستقل (ADR) وتفويض تنفيذي منفصل من المالك.

---

## 6. حدود ما لم يُنفذ (خارج نطاق BOOT-001)
- لم يتم بناء أي منطق مالي أو قواعد لحساب المرتبات أو الضرائب أو المعاشات.
- لم يتم تضمين أي أدوات لاستخراج ملفات Oracle PDF أو Excel.
- لم يتم إنشاء شاشات لإدخال أو عرض بيانات الموظفين الحقيقية أو الاصطناعية.
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
