# دراسة توافق طرق الوصول إلى البيانات من .NET 10 مع SQL Server 2014 (DB-COMPAT-001)

```text
STATUS: RESEARCH_ONLY / NO_PROVIDER_SELECTED / NO_SQL_OR_MIGRATIONS
TASK: Issue #50 (DB-COMPAT-001) | BRIDGE-REF: Issue #41 (AI-0019, AI-0022, AI-0023)
PR REVIEW FOLLOW-UP: PR #51 (DBREV-01..DBREV-04) | MAIN HEAD VERIFIED: c995074e1046dc820cbeee390ae32d632fa6db7f
PHASE A (Official Research & Architecture Comparison): COMPLETED (Revised per PR #51 review)
PHASE B (Isolated PoC Execution TC01..TC11): NOT_RUN (لم تُعيَّن بيئة SQL Server 2014 اصطناعية معزولة بعد)
```

> **تنبيه حوكمة إلزامي:** هذه الوثيقة دراسة فنية ومعمارية فقط؛ **ليست** اعتمادًا لمزود بيانات (`NuGet Provider`) ولا تصريحًا بتعديل الكود أو كتابة جداول أو `Migrations` أو الاتصال بقاعدة بيانات الجامعة. تُعد قاعدة بيانات `SQL Server 2014 (12.x)` بيئة **التطوير والإنتاج الفعلية** حاليًا بقرار المالك، ويظل الاختيار النهائي لمزود الوصول إلى البيانات قرارًا للمالك بعد مراجعة `ChatGPT` واستكمال اختبارات المختبر المعزول `Phase B`.

---

## 1. السياق والهدف

تهدف هذه الدراسة — استجابةً لـ [Issue #50 (DB-COMPAT-001)](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/50) ومرجع التنسيق `AI-0019` ثم تصحيحات المراجعة `AI-0022` (`DBREV-01..DBREV-04`) على `PR #51` في [Issue #41](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41) — إلى الإجابة الموثّقة على السؤال المعماري التالي:

**ما طرق الوصول إلى البيانات من `.NET 10` (`net10.0`, SDK `10.0.404`) إلى قاعدة بيانات `SQL Server 2014 (12.x)` المتوافقة تقنيًا ومعماريًا وأمنيًا، وما الحد الفاصل بين «الدعم المعلن رسميًا من Microsoft» و«التوثيق الرسمي المتعارض» و«التوافق التقني على البروتوكول (`TDS 7.4`)»؟**

يرتبط القرار التقني هنا بثوابت **PayrollReviewSystem**:
- التزام **Clean Architecture + DDD + OOP/SOLID** ([ADR-003](../decisions/ADR-003-ddd-modular-monolith-clean-architecture.md) و[ADR-004](../decisions/ADR-004-object-oriented-and-solid-principles.md)) بعزل الوصول للبيانات داخل طبقة `Infrastructure` دون تسريب `SqlConnection` أو أي ORM إلى `Core/Domain` أو `Application`.
- الحساب المالي الحتمي باستخدام `decimal` وتجنب تقريب الفواصل العائمة أو الأخطاء الضمنية في المعاملات المالية.
- احترام قيود التكامل والتزامن والاعتماد الذري (`ACID` و`Idempotency` وفرادة `MAIN_SALARY` منطقيًا لكل موظف وشهر مرتب).
- استهداف قابلية الترحيل المستقبلي من `SQL Server 2014 (12.x)` إلى إصدارات أحدث (`SQL Server 2019/2022+`) **كهدف هندسي اختياري** عند قرار المالك، دون افتراض ترقية إلزامية الآن ودون ضمان مطلق بمعزل عن اختبارات التراجع (`TC08`).

---

## 2. حقيقة دورة حياة SQL Server 2014 وESU ومستوى التوافق (`Lifecycle & Compatibility Level`)

قبل تقييم مكتبات `.NET 10`، يجب توثيق وضع محرك `SQL Server 2014 (12.x)` وفق صفحات دورة حياة Microsoft الرسمية مع بيان **الفارق الموثّق في التواريخ والمنطقة الزمنية بين صفحتين رسميتين**:

| البند | وفق صفحة المنتج (`Lifecycle Product`) | وفق صفحة أسئلة ESU (`SQL Server ESU FAQ`) | الملاحظات والأثر على النظام |
| :--- | :--- | :--- | :--- |
| **إصدار المحرك ومستوى التوافق** | `SQL Server 2014 (12.x)` (`120`, `110`, `100`) | — | الحد الأقصى لمستوى التوافق على محرك 2014 هو `120` |
| **بداية التوفر العام (GA)** | `2014-06-05` (`8:00:00 AM PT`) | — | — |
| **نهاية الدعم الرئيسي (Mainstream End)** | `2019-07-10` (`6:59:59 AM PT`) | — | توقف التطوير والميزات الجديدة |
| **نهاية الدعم الممتد القياسي (Extended End)** | `2024-07-10` (`6:59:59 AM PT`) | `July 9, 2024` (`2024-07-09`) | **انتهى الدعم القياسي لـ SQL Server 2014** |
| **ESU — السنة 1 (Critical Only)** | `2024-07-10` إلى `2025-07-09` (`PT`) | حتى `July 8, 2025` (`2025-07-08`) | تغطية تحديثات أمنية حرجة فقط |
| **ESU — السنة 2 (Critical Only)** | `2025-07-09` إلى `2026-07-15` (`PT`) | حتى `July 14, 2026` (`2026-07-14`) | تغطية تحديثات أمنية حرجة فقط |
| **ESU — السنة 3 (Critical Only)** | `2026-07-15` إلى `2027-07-13` (`6:59:59 AM PT`) | حتى `July 12, 2027` (`2027-07-12`) | **فارق يوم واحد بين المرجعين بسبب تدوين `Pacific Time Zone (PT)` في صفحة المنتج مقابل التاريخ المعلن في ESU FAQ**؛ لذا لا يُجزم بيوم موحد دون ذكر المصدرين |

- **المراجع الرسمية (تاريخ الاطلاع: `2026-11-12`):**
  - صفحة المنتج الرسمية: [Microsoft Lifecycle — SQL Server 2014](https://learn.microsoft.com/en-us/lifecycle/products/sql-server-2014) (تُظهر التواريخ بتوقيت `Pacific Time Zone (PT) - Redmond, WA, USA`).
  - صفحة الأسئلة الشائعة: [Microsoft Lifecycle — SQL Server Extended Security Updates FAQ](https://learn.microsoft.com/en-us/lifecycle/faq/sql-server-extended-security-updates) (محدّثة `2026-07-01`).

> **ضوابط مهمة حول برنامج `ESU` وفق وثيقة Microsoft الرسمية:**
> 1. برنامج `ESU` خيار مدفوع أخير يوفّر **تحديثات الأمان المصنفة `Critical` فقط (`Critical security updates only`)**، **ولا يمدّد دورة حياة المنتج ولا يشمل الدعم التقني العام** (`The ESU program does not extend the product's lifecycle and does not provide technical support beyond the original support timeline`)؛ أي لا يشمل استكشاف الأعطال العامة أو إصلاح الأخطاء غير الأمنية أو ميزات جديدة.
> 2. لا يشمل شراء `ESU` نسخ `Express` أو `Developer` أو `Web` منفردةً، وإنما نسخ `Standard` و`Enterprise` المؤهلة (مع إمكانية تطبيق التحديث على نفس الجهاز في حالات محددة موصوفة في FAQ).
> 3. التحقق من أهلية الجهة المالكة وإصدار الخادم الفعلي وترخيص `ESU` أمر تشغيلي/إداري منفصل عن هذه الدراسة، ولا يبرر افتراض تغطية `ESU` تلقائيًا.

### أثر `Compatibility Level` على T-SQL

يجب التمييز الصارم بين **إصدار المحرك (`12.x`)** و**مستوى توافق قاعدة البيانات (`sys.databases.compatibility_level`)**؛ فقد يعمل خادم `SQL Server 2014` لكن قاعدة بيانات معينة مضبوطة على مستوى `100` أو `110` (مثل قواعد مُستعادة من SQL Server 2008/2012) أو `120` (الحد الأقصى لـ SQL Server 2014).

وفق [وثيقة `ALTER DATABASE Compatibility Level` الرسمية](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-database-transact-sql-compatibility-level)، فإن الميزات التالية **غير متوفرة** عند `Compatibility Level <= 120`:
- `OPENJSON` (قدّمت في `130` / SQL Server 2016) — وهو السبب العملي الشائع لفشل بعض استعلامات `EF Core 8+` (مثل `.Contains()` الافتراضية) إذا لم تُقيَّد بمستوى التوافق.
- `STRING_SPLIT` و`JSON` built-in functions و`DROP ... IF EXISTS` و`AT TIME ZONE` (`130+`).
- `STRING_AGG` و`TRIM(characters)` (`140` / SQL Server 2017).
- نوع البيانات الأصلي `json` (`170` / SQL Server 2025).
- بروتوكول `TDS 8.0` وخيار `Encrypt=Strict` (يتطلب SQL Server 2022+).

---

## 3. مصفوفة التوافق والدعم الرسمي لمكتبات `.NET 10`

توضح المصفوفة التالية الحالة الفعلية لكل حزمة اعتمادًا على بيانات `NuGet (.nuspec)` ووثائق `Microsoft Learn` الرسمية (تاريخ الاطلاع: **12 نوفمبر 2026**)، مع استخدام الحالات الموحدة: **`SUPPORTED` / `UNVERIFIED` / `OUTSIDE_CURRENT_SUPPORT_MATRIX` / `CONFLICTING_OFFICIAL_DOCUMENTATION` / `DEPRECATED`**:

| المسار / الحزمة | الإصدار المرجعي (NuGet) | إطار الاستهداف في `.nuspec` | حالة دعم المكتبة على `.NET 10` | حالة الدعم والتوافق مع `SQL Server 2014 (12.x)` | التقييم العام في الدراسة |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`Microsoft.Data.SqlClient` (LTS)** | `6.1.8` (محدث في `NuGet` بتاريخ `2026-10-28`؛ صفحة MS Learn بتاريخ `2026-10-23` ما زالت تعرض `6.1.7` في جدول الباتشات) | `net8.0`, `net9.0` (متوافق مع `net10.0`), `netstandard2.0`, `net462` | **`SUPPORTED`** (خط `6.1` LTS مدعوم حتى `2028-08-14`) | **`UNVERIFIED / OUTSIDE_CURRENT_SUPPORT_MATRIX`** — جدول *Database compatibility* النشط على MS Learn يبدأ من `SQL Server 2017` ولا يدرج `2014`؛ ولا يكفي بروتوكول `TDS 7.4` وحده لإعلان دعم رسمي من Microsoft قبل تشغيل `Phase B` | **المسار المرشح الأساسي للدراسة والتجربة المعزولة (ADO.NET صريح)** |
| **`Microsoft.Data.SqlClient` (STS)** | `7.1.1` (خط `7.0.x` ينتهي `2026-12-17`) | `net8.0`, `net9.0`, `net10.0`, `netstandard2.0`, `net462` | **`SUPPORTED` (`STS` قصير الأمد)** | **`UNVERIFIED / OUTSIDE_CURRENT_SUPPORT_MATRIX`** — نفس وضع جدول التوافق النشط (`2017+`)، مع مدة دعم أقصر (`STS`) وفصل امتدادات Azure | غير مفضل على `6.1.8 LTS` لنظام طويل الأمد |
| **`Dapper` + `Microsoft.Data.SqlClient`** | `Dapper 2.1.89` + `6.1.8 LTS` | `net10.0` صريح في `Dapper.nuspec` (دون تبعيات إضافية، ترخيص `Apache-2.0`) | **`SUPPORTED`** | **`UNVERIFIED / OUTSIDE_CURRENT_SUPPORT_MATRIX`** — يرث توافقية الاتصال من `Microsoft.Data.SqlClient` ويعتمد على T-SQL يدوي صريح لمستوى `120` حتى إثبات `Phase B` | **مسار مرشح اختياري فوق `SqlClient` عند الحاجة لتقليل تكرار Mapping** |
| **`Microsoft.EntityFrameworkCore.SqlServer` (`EF Core 10`)** | `10.0.15` (يعتمد على `Microsoft.Data.SqlClient 6.1.7`) | `net10.0` (مدعوم حتى `2028-11-10`) | **`SUPPORTED` على `.NET 10`** | **`CONFLICTING_OFFICIAL_DOCUMENTATION / NOT_ESTABLISHED`** — صفحة المزود التفصيلية تذكر **`SQL Server (2019 onwards)`** بينما صفحة فهرس المزودين العامة تذكر **`SQL Server 2012 onwards`** لـ `EF Core 8, 9, 10`؛ لم يختبر عبر PoC ولا يكفي `UseCompatibilityLevel(120)` كضمان دعم | **عدم الاعتماد حاليًا كقرار تحفظ هندسي** (وليس جزمًا بإجماع الوثائق على عدم الدعم) |
| **`EntityFramework` (`EF 6.5.2`) / `Microsoft.EntityFramework.SqlServer`** | `6.5.2` (`Microsoft.EntityFramework.SqlServer` يعتمد على `Microsoft.Data.SqlClient 6.1.4`) | `netstandard2.1` | وضع صيانة فقط (`Modern Lifecycle Policy`، دون تطوير ميزات جديدة) | `EntityFramework 6.5.2` الأصلي يعتمد على `System.Data.SqlClient 4.8.6` المهجورة؛ حزمة `Microsoft.EntityFramework.SqlServer 6.5.2` مسار صيانة إرثي دون دعم ميزات `.NET 10` الحديثة | **غير موصى به لنظام جديد بـ `.NET 10` وClean Architecture** |
| **`System.Data.SqlClient`** | `4.9.1` | `.NETCoreApp`, `.NETStandard`, `.NETFramework` | **`DEPRECATED`** | يصفه ملف `.nuspec` صراحةً: `DEPRECATED - Use Microsoft.Data.SqlClient.` (**استبعاد كخيار تطوير حديث** بسبب حالة الإهمال الرسمية، لا ادعاء فشل اتصال تقني على كل خادم) | **مستبعد كاختيار حديث** |

### المصادر الرسمية المعتمدة في التقييم (تاريخ الاطلاع: `2026-11-12`)
1. **تعارض وثائق EF Core الرسمية:**
   - صفحة مزود SQL Server التفصيلية (محدّثة `2026-07-13`): [EF Core SQL Server Database Provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/) — قسم *Supported database engines* ينص على: **`Microsoft SQL Server (2019 onwards)`**.
   - صفحة فهرس مزودي EF Core العامة (محدّثة `2026-10-20`): [EF Core Database Providers Index](https://learn.microsoft.com/en-us/ef/core/providers/) — صف `Microsoft.EntityFrameworkCore.SqlServer` ينص على: **`Azure SQL, SQL Server 2012 onwards, Azure Synapse Analytics`** لإصدارات EF Core **`8, 9, 10`**.
2. **دورة حياة `Microsoft.Data.SqlClient`:** [SqlClient Driver Support Lifecycle](https://learn.microsoft.com/en-us/sql/connect/ado-net/sqlclient-driver-support-lifecycle) (محدّثة `2026-10-23`: خط `6.1` LTS حتى `August 14, 2028`؛ جدول التوافق النشط يبدأ من `SQL Server 2017`).
3. **دورة حياة `SQL Server 2014` وتعارض تواريخ ESU:**
   - [SQL Server 2014 Lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/sql-server-2014) (توقيت `PT` — السنة 3 تنتهي `2027-07-13`).
   - [SQL Server Extended Security Updates FAQ](https://learn.microsoft.com/en-us/lifecycle/faq/sql-server-extended-security-updates) (محدّثة `2026-07-01` — السنة 3 تنتهي `July 12, 2027`).
4. **دعم TLS 1.2 لـ SQL Server:** [KB3135244 — TLS 1.2 support for Microsoft SQL Server](https://learn.microsoft.com/en-us/troubleshoot/sql/database-engine/connect/tls-1-2-support-microsoft-sql-server).
5. **سلاسل الاتصال والـ Encryption:** [Connection strings with Microsoft.Data.SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/connection-strings) (محدّثة `2026-10-28`).
6. **مستويات التوافق T-SQL:** [ALTER DATABASE compatibility level](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-database-transact-sql-compatibility-level).
7. **تغييرات `EF Core 8` و`OPENJSON`:** [EF Core 8 Breaking Changes — Contains uses OPENJSON](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/breaking-changes) و[EF Core Releases](https://learn.microsoft.com/en-us/ef/core/what-is-new/release-planning).
8. **بيانات حزم NuGet الفعلية (`.nuspec`):** `Microsoft.Data.SqlClient` (`6.1.8`, `7.1.1`), `Dapper` (`2.1.89`), `Microsoft.EntityFrameworkCore.SqlServer` (`10.0.15`), `EntityFramework` (`6.5.2`), `Microsoft.EntityFramework.SqlServer` (`6.5.2`), `System.Data.SqlClient` (`4.9.1`).

---

## 4. التحليل المقارن المفصل لطرق الوصول للبيانات

### المسار الأول: `Microsoft.Data.SqlClient` (`6.1.8 LTS`) — ADO.NET الصريح

- **آلية العمل:** فتح اتصال `SqlConnection` واستخدام `SqlCommand` و`SqlParameter` و`SqlTransaction` و`SqlDataReader` مباشرة داخل `Infrastructure` Repository Adapters.
- **نقاط القوة:**
  1. توافق `LTS` للمحرك العميل على `.NET 10` حتى `2028-08-14`، متناغم مع دعم `.NET 10 LTS` (حتى `2028-11-10`).
  2. تحكم صريح ومباشر في استعلامات `T-SQL` لتتوافق مع `Compatibility Level 120` (أو `110`/`100` إن لزم)، دون توليد SQL تلقائي مفاجئ.
  3. ضبط صريح للأنواع المالية (`SqlDbType.Decimal` مع `Precision` و`Scale`) وتجنب تحويلات `float`/`double`.
  4. وضوح حدود المعاملات (`BeginTransactionAsync`)، ومستويات العزل (`Serializable` أو `UPDLOCK, HOLDLOCK`) لحماية فرادة `MAIN_SALARY` ومنع سباق التزامن.
  5. إتاحة `SqlBulkCopy` لسيناريوهات الاستيراد الجماعي إن احتاجها النظام مستقبلًا.
- **نقاط الضعف / القيود:**
  - يتطلب كتابة كود تحويل الصفوف (`Reader -> DTO/Record`) يدويًا، وتظل صلاحيته التشغيلية على بيئة الجامعة `SQL Server 2014` مصنفة **`UNVERIFIED / OUTSIDE_CURRENT_SUPPORT_MATRIX`** حتى تنفيذ `Phase B` والتحقق من `SP/CU` و`TLS 1.2`.

### المسار الثاني: `Microsoft.Data.SqlClient` (`6.1.8 LTS`) + `Dapper` (`2.1.89`)

- **آلية العمل:** إدارة الاتصال والمعاملات عبر `Microsoft.Data.SqlClient`، مع الاستعانة بـ `Dapper` لتمرير المعاملات ورسم صفوف القراءة (`QueryAsync<TRow>`) وتقليل التكرار في `Infrastructure`.
- **نقاط القوة:**
  1. يستهدف `Dapper 2.1.89` إطار `net10.0` صراحةً دون تبعيات خارجية إضافية (`Apache-2.0`).
  2. يحافظ على كتابة `T-SQL` الصريح المتوافق مع `Compatibility Level 120` ويمرر المعاملات بالربط المعلَّم (`Parameterized`).
  3. يقلل الكود الميكانيكي (`boilerplate`) في قراءة الجداول والتقارير (`Read Models`).
- **نقاط الضعف / الضوابط المطلوبة:**
  1. الافتراضي في `Dapper` لمعاملات `string` هو `NVARCHAR(4000)` عند عدم استخدام `DbString`؛ يجب الحذر مع الأعمدة من نوع `VARCHAR` وفهارس البحث لتجنب بطء الفهرسة أو تضخم خطط التنفيذ (`plan cache bloat`).
  2. يجب ضبط أنواع ودقة `decimal` وتمرير `SqlTransaction` الصريحة دائمًا، ومنع الدمج النصي في SQL (`SQL Injection`).
  3. وجود تبعية ثانية خفيفة تضاف إلى حوكمة التبعيات، ويبقى الاتصال الفعلي بـ 2014 **`UNVERIFIED`** حتى `Phase B`.

### المسار الثالث: `Microsoft.EntityFrameworkCore.SqlServer` (`10.0.15`) — التوثيق المتعارض وتحفظ الاعتماد

- **آلية العمل:** ORM متكامل مع `DbContext`، تتبع التغييرات (`Change Tracking`)، `LINQ to SQL`، و`EF Core Migrations`.
- **توصيف التعارض الرسمي (`DBREV-01`):**
  - صفحة **مزود SQL Server التفصيلية** (محدّثة `2026-07-13`) تذكر أن المحركات المدعومة تبدأ من **`SQL Server (2019 onwards)`**، وهو ما يستبعد `SQL Server 2014`.
  - بالمقابل، صفحة **فهرس مزودي EF Core العام** (محدّثة `2026-10-20`) تذكر أن `Microsoft.EntityFrameworkCore.SqlServer` يدعم **`SQL Server 2012 onwards`** لـ EF Core `8, 9, 10`.
  - إذن يوجد **تعارض رسمي صريح داخل وثائق Microsoft نفسها (`CONFLICTING_OFFICIAL_DOCUMENTATION / NOT_ESTABLISHED`)**.
- **سبب عدم اعتماد `EF Core 10` حاليًا (تحفظ هندسي، لا جزم بغياب كل نص داعم):**
  1. بدءًا من EF Core 8، يولّد المترجم عبارات `OPENJSON` افتراضيًا عند `.Contains()`، مما ينكسر على `Compatibility Level <= 120`.
  2. الخيار `UseCompatibilityLevel(120)` يقيّد توليد بعض الميزات البرمجية (مثل تجنب `OPENJSON` أو نوع `json`)، لكنه **ليس ضمانًا رسميًا موثقًا** ولا يُغني عن اختبار PoC فعلي على SQL Server 2014 لكل من الاستعلامات والـ Migrations وmetadata.
  3. التزامًا بالتحفظ الهندسي والمالي في نظام مرتبات، يُفضَّل **عدم اعتماد `EF Core` حاليًا** ما لم يثبت دعمه بوضوح أو تُستوفى اختبارات PoC معزولة واعتماد المالك عبر `ADR` مستقل.

### المسار الرابع: البدائل المستبعدة (`System.Data.SqlClient` و`EF 6.5.2`)

- **`System.Data.SqlClient 4.9.1`:** معلنة رسميًا كحزمة مهجورة (`DEPRECATED - Use Microsoft.Data.SqlClient.`) في NuGet؛ تستبعد كاختيار تطوير حديث لحرمانها من التطوير والميزات والأمان المستمر (مع الإقرار بأن `DEPRECATED` تصنيف دعم وحزمة لا ادعاء بأنها تعجز عن فتح أي اتصال تقنيًا على الإطلاق).
- **`EntityFramework 6.5.2` / `Microsoft.EntityFramework.SqlServer 6.5.2`:** مكتبة إرثية في وضع الصيانة فقط (`maintenance-only`) لنقل أنظمة `.NET Framework` القديمة، وتعتمد نسختها الأصلية على `System.Data.SqlClient 4.8.6` المهجورة؛ لذلك لا تناسب مشروع `.NET 10` جديدًا يلتزم بـ Clean Architecture.

---

## 5. العزل المعماري في Clean Architecture وحدود الترقية المستقبلية

بموجب [خريطة Bounded Contexts وAggregates](../architecture/DDD-BOUNDED-CONTEXTS-AND-AGGREGATES.md) و[معايير OOP وSOLID](../architecture/OOP-AND-SOLID-DESIGN-STANDARDS.md) و[DEPLOYMENT-AND-CONTRACTS.md](../architecture/DEPLOYMENT-AND-CONTRACTS.md):

1. **حدود التبعيات المسموح بها:**
   - `PayrollReviewSystem.Core`: **صفر تبعيات** على SQL Server أو `Microsoft.Data.SqlClient` أو `Dapper` أو `EF Core`.
   - `PayrollReviewSystem.Application`: تعرّف واجهات العقود (`Ports`) بحجم الاستخدام (`ISP`) مثل `IPayrollCycleRepository` أو `IUnitOfWork` أو `IPayrollReadStore`، ولا تستورد أي نوع من `System.Data` أو `Microsoft.Data.SqlClient`.
   - `PayrollReviewSystem.Infrastructure`: الطبقة **الوحيدة** المسموح لها باستيراد مزود البيانات المختار مستقبلًا وتنفيذ `SqlConnectionFactory` و`Adapters`.
   - `PayrollReviewSystem.Api`: يربط `Dependency Injection` بتكوين `Infrastructure` دون كتابة استعلامات SQL داخل الـ Controllers.
2. **قابلية الترقية المستقبلية إلى SQL Server 2019/2022+ (هدف هندسي مشروط):**
   - **الهدف الهندسي** من عزل العقود (`Ports/Adapters`) هو إمكانية ترقية خادم قاعدة البيانات مستقبلًا من `SQL Server 2014` إلى `SQL Server 2019/2022+` (إن قرر المالك ذلك لاحقًا) دون المساس بمنطق `Core` أو عقود `Application`، **بشرط** الحفاظ على ثبات واجهات `Application/Infrastructure` وتمرير اختبارات التراجع الكاملة وإعادة التحقق **`TC08`** عند الترقية.
3. **تنويه مهم حول فصل قواعد البيانات:**
   - هذه الدراسة تخص قاعدة بيانات **نظام المراجعة نفسه** المستهدف على `SQL Server 2014` (بيئة التطوير والإنتاج الحالية)، ولا تغيّر قاعدة الحوكمة القاضية بعدم التعديل على بيانات Oracle أو قاعدة الجامعة التشغيلية أو القراءة المباشرة منها دون تفويض صريح وعقد تكامل معتمد.

---

## 6. متطلبات الأمان والتشفير وTLS 1.2 والمصادقة

### أ) مشكلة كسر الاتصال الافتراضي في `Microsoft.Data.SqlClient` (>= 4.0)

بدءًا من `Microsoft.Data.SqlClient 4.0` (ويشمل `6.1.x` و`7.x`)، أصبحت القيمة الافتراضية في سلسلة الاتصال هي **`Encrypt=Mandatory`** (`true`) بدل `Optional` (`false`). يعني ذلك ما يلي عند الاتصال بـ `SQL Server 2014`:
1. إذا كان خادم `SQL Server 2014` قديمًا بلا تحديثات `TLS 1.2`، فقد يفشل التفاوض الأمني (`Handshake`) مع `.NET 10` الحديث الذي يعطل `TLS 1.0/1.1` افتراضيًا على أنظمة التشغيل الحديثة.
2. حتى مع وجود `TLS 1.2` على الخادم، إذا كان `SQL Server 2014` يستخدم **شهادة موقعة ذاتيًا (Self-Signed Certificate)**، فسيرفض `Microsoft.Data.SqlClient` الاتصال افتراضيًا بالخطأ المعروف:
   `The certificate chain was issued by an authority that is not trusted.`

### ب) الحد الأدنى المطلوب على خادم SQL Server 2014 لـ TLS 1.2 (وفق `KB3135244`)

| مسار التحديث على SQL Server 2014 | الحد الأدنى للإصدار (`Build`) | المرجع |
| :--- | :--- | :--- |
| **SQL Server 2014 RTM** | `CU12` (`12.0.2564.0`) فما فوق | `KB3135244` |
| **SQL Server 2014 SP1** | `CU5` (`12.0.4439.1`, `KB3130926`) فما فوق | `KB3135244` |
| **SQL Server 2014 SP2 / SP3** | مدعوم ضمن حزمة الخدمة (`SP2` / `SP3` وآخر CU/ESU) | `KB3135244` |

### ج) سياسة إعدادات الاتصال (`Encrypt`, `TrustServerCertificate`, Authentication)

- **`Encrypt=Strict` (TDS 8.0):** **غير مدعوم على SQL Server 2014** (يخص SQL Server 2022+ فقط). لا يفعّل على بيئات 2014.
- **الإنتاج / البيئة شبه الإنتاجية (Recommended Security Posture):**
  - الإبقاء على `Encrypt=True` (`Mandatory`)، مع تثبيت شهادة TLS صادرة عن جهة تصديق داخلية موثوقة (`Internal Enterprise CA`) على خادم SQL Server، أو تحديد `HostNameInCertificate` / `ServerCertificate` (متاح في `Microsoft.Data.SqlClient 5.1+`) عند الحاجة.
  - **يُحظر** استخدام `TrustServerCertificate=True` أو `Encrypt=False` كسلوك افتراضي أو Fallback صامت في الإنتاج، لما يسببه من قابلية لهجمات الوسيط (`MITM`) وكشف بيانات المصادقة والبيانات المالية الحساسة.
  - يُقاس الامتثال لهذه القاعدة صراحةً في اختبار **`TC07` (منع Fallback التشفير غير الآمن)**.
- **المصادقة (`Authentication`):**
  - داخل شبكة Domain (`Active Directory`) في البيئة المحلية: يُفضَّل `Integrated Security=True` (`Windows Authentication` / حساب خدمة بأقل صلاحيات `Least Privilege`) لتجنب تخزين كلمات مرور SQL نصية في ملفات الإعدادات.
  - عند تعذر مصادقة Windows: استخدام `SQL Authentication` بحساب مخصص لصلاحيات جداول المراجعة فقط، وتخزين سر الاتصال في متغيرات بيئة/مخزن أسرار آمن لا يرفع إلى GitHub مطلقًا.

---

## 7. مصفوفة التتبع واختبارات التحقق في المختبر المعزول (`Phase B — TC01..TC11: NOT_RUN`)

> **حالة التنفيذ الحالية:** **جميع اختبارات `TC01..TC11` بحالة `NOT_RUN`**.
> تم التحقق من وجود خدمة `MSSQLSERVER` وأداة `SQLCMD.EXE` محليًا دون الاتصال بأي مثيل، التزامًا بعدم لمس أي قاعدة بيانات غير مخصصة أو قاعدة بيانات الجامعة، وبانتظار توفير وتوثيق حاوية أو مثيل `SQL Server 2014 (12.x)` اصطناعي ومعزول.

### شروط تشغيل المختبر المعزول مستقبلًا
1. قاعدة بيانات اصطناعية جديدة ومفرغة من أي بيانات موظفين حقيقية (مثال: `PayrollReview_CompatLab_Synthetic`) مع سكريبت إنشاء وإزالة مؤقت موثق لا يُدمج في فرع التطبيق.
2. التحقق من رقم البناء (`@@VERSION` و`SERVERPROPERTY('ProductVersion')` و`SERVERPROPERTY('ProductLevel')`) ومستوى التوافق الفعلي (`sys.databases.compatibility_level`: اختبار `120` وفحص سلوك `110` إن لزم).
3. عدم استخدام GitHub-hosted CI للاتصال بالشبكة الداخلية أو افتراض وجود SQL Server 2014 في CI؛ اختبارات التكوين الصورية (`Mock/Unit`) في CI لا تغني عن `Phase B`.

### أ) مصفوفة التتبع الكاملة لمتطلبات Issue #50 (`TC01..TC08`) + اختبارات البحث المكملة (`TC09..TC11`)

| المعرّف الموحد | المتطلب الأصلي في Issue #50 / نطاق البحث | ما يجب التحقق منه فعليًا في المختبر المعزول | شرط النجاح المتوقع (`Pass Criteria`) | الحالة الحالية |
| :--- | :--- | :--- | :--- | :--- |
| **`TC01`** | **Issue #50 (`TC01`):** فتح اتصال آمن | فتح اتصال `SqlConnection` بـ `Encrypt=Mandatory` وشهادة موثوقة وتنفيذ `SELECT 1, @@VERSION, SERVERPROPERTY('ProductVersion'), compatibility_level` | نجاح فتح الاتصال المشفر عبر `TLS 1.2` وقراءة نسخة `12.x` و`compatibility_level` الفعلي بدقة دون أخطاء مصادقة أو TLS | **`NOT_RUN`** |
| **`TC02`** | **Issue #50 (`TC02`):** استعلام معلمة (`Parameterized Query`) | تنفيذ `SELECT` و`INSERT` بحقول اصطناعية عبر `SqlParameter` صريح (وتجربة محاولة حقن نصي مثل `' OR 1=1 --` كمدخل بيانات عادي) | معاملة المدخل كقيمة حرفية آمنة تمامًا؛ عدم تنفيذ SQL محقون، وربط صحيح للأنواع (`DbType`/`Size`) | **`NOT_RUN`** |
| **`TC03`** | **Issue #50 (`TC03`):** `decimal` دقيق | كتابة وقراءة مبالغ بـ `decimal(18,2)` و`decimal(18,4)` تشمل قيمًا موجبة وسالبة وصفرًا و`DBNull` | تطابق رقمي تام لنوع `decimal` في .NET دون أي تقريب `double/float`، والتمييز التام بين `NULL` و`0.00m` | **`NOT_RUN`** |
| **`TC04`** | **Issue #50 (`TC04`):** `Unicode` عربي (`RTL`) | إدراج واسترجاع نصوص عربية مركبة، مسافات، أرقام عربية/هندية، وعلامات تشكيل في أعمدة `NVARCHAR` باستخدام معلمات صريحة | مطابقة ثنائية (`ordinal equality`) للنص العربي دون تشويه أو تحول إلى `????` | **`NOT_RUN`** |
| **`TC05`** | **Issue #50 (`TC05`):** `Rollback` وتكامل المعاملات (`ACID`) | تشغيل معاملة متعددة الخطوات تفشل في منتصفها مع `RollbackAsync`، بالإضافة لمحاكاة تزامن (`Task.WhenAll`) لحجز نفس `MAIN_SALARY` | تراجع كامل عند الفشل دون أي أثر جزئي في القاعدة، ونجاح عملية واحدة فقط في التزامن عبر القيد الفريد أو أقفال `ACID` | **`NOT_RUN`** |
| **`TC06`** | **Issue #50 (`TC06`):** سلوك `Timeout / Retry / Idempotency` | محاكاة انتهاء مهلة (`CommandTimeout`) أو خطأ عابر متقطع وإعادة إرسال نفس العملية بنفس `IdempotencyKey` | إيقاف آمن عند Timeout دون اتصال معلّق، وعدم تكرار السطر المالي أو الأثر المحاسبي عند إعادة المحاولة بنفس المفتاح | **`NOT_RUN`** |
| **`TC07`** | **Issue #50 (`TC07`):** عدم وجود `Fallback` غير آمن للتشفير | محاولة اتصال بسيرفر يحمل شهادة غير موثوقة مع إعداد التطبيق الافتراضي، والتحقق من عدم التراجع التلقائي إلى `TrustServerCertificate=True` أو `Encrypt=False` (وكذلك توثيق رفض `Encrypt=Strict` على 2014) | فشل الاتصال بصورة آمنة ومعلنة (`fail-closed`) عند انعدام ثقة الشهادة، وعدم السماح بإضعاف التشفير صمتًا في إعدادات الإنتاج | **`NOT_RUN`** |
| **`TC08`** | **Issue #50 (`TC08`):** إعادة الاختبار على إصدار SQL أحدث مستقبلًا | تشغيل نفس مجموعة اختبارات `TC01..TC07` + اختبارات التراجع (`Regression`) على مثيل اصطناعي أحدث (`SQL Server 2019/2022+` بمستوى توافق أعلى) عبر نفس واجهات `Infrastructure` | اجتياز جميع العقود والاختبارات دون تعديل منطق `Core` أو `Application`، وتوثيق أي تغيير مطلوب في سلسلة الاتصال (`TLS 1.3` / `Encrypt=Strict` إن فُعّل) | **`NOT_RUN`** |
| **`TC09`** *(إضافي بحثي)* | **فحص قيود `Compatibility Level 120` و`110`** | فحص استعلامات التصفح (`OFFSET/FETCH`) والتجميع، والتحقق من رفض دوال `130+` مثل `OPENJSON` و`STRING_SPLIT` على قاعدة بمستوى `120` | خلو كافة استعلامات المشروع المصرح بها مستقبلًا من أي دالة `130+`؛ وتأكيد عدم تمرير `OPENJSON` على مستوى `120` | **`NOT_RUN`** |
| **`TC10`** *(إضافي بحثي)* | **قياس الأداء المبدئي للدفعات (اصطناعي)** | إدراج وقراءة 1,000 سطر استحقاق اصطناعي متكرر الكود والاسم مع قياس الزمن والذاكرة وثبات `Connection Pool` | إتمام الدفعة بزمن مستقر دون استنزاف الاتصالات أو تسريب موارد (`DisposeAsync`) | **`NOT_RUN`** |
| **`TC11`** *(إضافي بحثي)* | **تجربة مقارنة `EF Core 10` (ليست اعتمادًا)** | تشغيل استعلام يحتوي `.Contains(ids)` على جدول اصطناعي بدون ثم مع `UseCompatibilityLevel(120)` على مثيل `2014` | توثيق فشل الاستعلام الافتراضي بسبب `OPENJSON`، وقياس سلوك `UseCompatibilityLevel(120)` مع إبقاء الحالة `CONFLICTING_OFFICIAL_DOCUMENTATION` وعدم الاعتماد | **`NOT_RUN`** |

---

## 8. التوصية المعمارية المشروطة وقرار المالك المطلوب

بناءً على الحقائق الموثقة في الأقسام (2) و(3) و(4):

1. **استبعاد `System.Data.SqlClient` (`DEPRECATED`) كخيار حديث، والتحفظ الهندسي عن اعتماد `EF Core 10` حاليًا** بسبب تعارض وثائق Microsoft الرسمية (`CONFLICTING_OFFICIAL_DOCUMENTATION / NOT_ESTABLISHED` بين `2019 onwards` و`2012 onwards`) وبانتظار اختبار فعلي لا تحلّه `UseCompatibilityLevel(120)` وحدها.
2. **التوصية المبدئية المشروطة للبحث (`Phase A`):**
   - الاعتماد المبدئي المرشح للدراسة على **`Microsoft.Data.SqlClient 6.1.8` (`LTS` حتى `2028-08-14`)** داخل طبقة `Infrastructure` كعميل اتصال أساسي على `.NET 10 LTS`، مع الإقرار الصريح بأن تشغيله على `SQL Server 2014` يظل **`UNVERIFIED / OUTSIDE_CURRENT_SUPPORT_MATRIX`** حتى إثبات `Phase B`.
   - **المفاضلة التنفيذية المفتوحة للمالك و`ChatGPT` عند اختيار التصميم لاحقًا:**
     - **الخيار (أ): `Microsoft.Data.SqlClient 6.1.8 LTS` (ADO.NET صريح بالكامل)** — أقل تبعيات، أقصى وضوح في المعاملات والأنواع المالية.
     - **الخيار (ب): `Microsoft.Data.SqlClient 6.1.8 LTS` + `Dapper 2.1.89` (اختياري للربط فقط)** — يختصر تكرار Mapping الصفوف مع الحفاظ على `T-SQL` الصريح المتوافق مع `120` والمعاملات الصريحة.
3. **شروط الانتقال إلى أي اعتماد فعلي (`Provider Selection ADR`):**
   - التحقق الميداني من رقم التحديث الفعلي لخادم `SQL Server 2014` لدى الجهة المالكة (`Build / SP / CU` لدعم `TLS 1.2`) ومسألة أهلية وترخيص `ESU`.
   - تشغيل كامل اختبارات **`TC01..TC11` (`Phase B`)** على بيئة `SQL Server 2014 (12.x)` اصطناعية معزولة تمامًا والحصول على نتائج `PASS` موثقة (خاصة `TC06` و`TC07` و`TC08` عند توفر بيئة الترقية الاختبارية).
   - مراجعة `ChatGPT` المعمارية واعتماد **ADR مستقل** من المالك يثبت اختيار المزود قبل إضافة أي حزمة `NuGet` إلى ملفات `.csproj` الإنتاجية أو إنشاء أي `Schema` أو `Migrations`.
