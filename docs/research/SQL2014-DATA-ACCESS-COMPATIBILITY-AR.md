# دراسة توافق طرق الوصول إلى البيانات من .NET 10 مع SQL Server 2014 (DB-COMPAT-001)

```text
STATUS: RESEARCH_ONLY / NO_PROVIDER_SELECTED / NO_SQL_OR_MIGRATIONS
TASK: Issue #50 (DB-COMPAT-001) | BRIDGE-REF: Issue #41 (AI-0019 / AI-0020)
MAIN HEAD VERIFIED: 656097a2e9fe35c14dceb98f7daeb819355c0097
PHASE A (Official Research & Architecture Comparison): COMPLETED
PHASE B (Isolated PoC Execution TC01..TC08): NOT_RUN (لم تُعيَّن بيئة SQL Server 2014 اصطناعية معزولة بعد)
```

> **تنبيه حوكمة إلزامي:** هذه الوثيقة دراسة فنية ومعمارية فقط؛ **ليست** اعتمادًا لمزود بيانات (`NuGet Provider`) ولا تصريحًا بتعديل الكود أو كتابة جداول أو `Migrations` أو الاتصال بقاعدة بيانات الجامعة. يظل الاختيار النهائي لمزود الوصول إلى البيانات قرارًا للمالك بعد مراجعة `ChatGPT` واستكمال اختبارات المختبر المعزول `Phase B`.

---

## 1. السياق والهدف

تهدف هذه الدراسة — استجابةً لـ [Issue #50 (DB-COMPAT-001)](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/50) ومرجع التنسيق `AI-0019` في [Issue #41](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41) — إلى الإجابة الموثّقة على السؤال المعماري التالي:

**ما طرق الوصول إلى البيانات من `.NET 10` (`net10.0`, SDK `10.0.404`) إلى قاعدة بيانات `SQL Server 2014 (12.x)` المتوافقة تقنيًا ومعماريًا وأمنيًا، وما الفارق بين «الدعم الرسمي من Microsoft» و«التوافق التقني على البروتوكول (TDS 7.4)»؟**

يرتبط القرار التقني هنا بثوابت **PayrollReviewSystem**:
- التزام **Clean Architecture + DDD + OOP/SOLID** ([ADR-003](../decisions/ADR-003-ddd-modular-monolith-clean-architecture.md) و[ADR-004](../decisions/ADR-004-object-oriented-and-solid-principles.md)) بعزل الوصول للبيانات داخل طبقة `Infrastructure` دون تسريب `SqlConnection` أو أي ORM إلى `Core/Domain` أو `Application`.
- الحساب المالي الحتمي باستخدام `decimal` وتجنب تقريب الفواصل العائمة أو الأخطاء الضمنية في المعاملات المالية.
- احترام قيود التكامل والتزامن والاعتماد الذري (`ACID` و`Idempotency` وفرادة `MAIN_SALARY` منطقيًا لكل موظف وشهر مرتب).
- دعم الترحيل المستقبلي من `SQL Server 2014 (12.x)` إلى إصدارات أحدث (`SQL Server 2019/2022/2025`) دون إعادة كتابة منطق المجال أو طبقة التطبيق.

---

## 2. حقيقة دورة حياة SQL Server 2014 ومستوى التوافق (Lifecycle & Compatibility Level)

قبل تقييم مكتبات `.NET 10`، يجب توثيق وضع محرك `SQL Server 2014 (12.x)` نفسه وفق صفحة دورة حياة منتجات Microsoft الرسمية:

| البند | القيمة الموثقة | الأثر على النظام |
| :--- | :--- | :--- |
| **إصدار المحرك** | `SQL Server 2014 (12.x)` | يدعم مستويات التوافق (`Compatibility Level`): `120` و`110` و`100` فقط |
| **بداية التوفر العام (GA)** | `2014-06-05` | — |
| **نهاية الدعم الرئيسي (Mainstream End Date)** | `2019-07-10` | توقف التطوير والميزات الجديدة |
| **نهاية الدعم الممتد (Extended Support End Date)** | `2024-07-10` | **انتهى الدعم القياسي لـ SQL Server 2014** |
| **تحديثات الأمان الممتدة (ESU)** | السنة 1: `2024-07-10` إلى `2025-07-09`<br>السنة 2: `2025-07-09` إلى `2026-07-15`<br>السنة 3: `2026-07-15` إلى `2027-07-13` | يتطلب تغطية وترخيص ESU صريحًا للبقاء ضمن نطاق التحديثات الأمنية الرسمية حتى `2027-07-13` |

- **المرجع الرسمي:** [Microsoft Lifecycle — SQL Server 2014](https://learn.microsoft.com/en-us/lifecycle/products/sql-server-2014)

### أثر `Compatibility Level` على T-SQL

يجب التمييز الصارم بين **إصدار الخادم (`12.x`)** و**مستوى توافق قاعدة البيانات (`database_id` -> `compatibility_level`)**؛ فقد يعمل خادم `SQL Server 2014` لكن قاعدة بيانات معينة مضبوطة على مستوى `100` أو `110` (مثل قواعد مُستعادة من SQL Server 2008/2012) أو `120` (الحد الأقصى لـ SQL Server 2014).

وفق [وثيقة `ALTER DATABASE Compatibility Level` الرسمية](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-database-transact-sql-compatibility-level)، فإن الميزات التالية **غير متوفرة** عند `Compatibility Level <= 120`:
- `OPENJSON` (قدّمت في `130` / SQL Server 2016) — وهو السبب الجوهري لفشل استعلامات LINQ التي تولد `OPENJSON` في EF Core 8+ ما لم تُقيَّد.
- `STRING_SPLIT` و`JSON` built-in functions و`DROP ... IF EXISTS` و`AT TIME ZONE` (أغلبها `130+`).
- `STRING_AGG` و`TRIM(characters)` (قدّمت في `140` / SQL Server 2017).
- نوع البيانات الأصلي `json` (`170` / SQL Server 2025).
- بروتوكول `TDS 8.0` وخيار `Encrypt=Strict` (يتطلب SQL Server 2022+).

---

## 3. مصفوفة التوافق والدعم الرسمي لمكتبات `.NET 10`

توضح المصفوفة التالية الحالة الفعلية لكل حزمة اعتمادًا على بيانات `NuGet (.nuspec)` ووثائق `Microsoft Learn` الرسمية التي جرى التحقق منها في **12 نوفمبر 2026**:

| المسار / الحزمة | الإصدار المرجعي (NuGet) | إطار الاستهداف في `.nuspec` | حالة دعم المكتبة على `.NET 10` | حالة التشغيل مع `SQL Server 2014 (12.x)` | التقييم العام |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`Microsoft.Data.SqlClient` (LTS)** | `6.1.8` (و`6.1.7` في جدول MS Learn بتاريخ `2026-10-23`) | `net8.0`, `net9.0` (متوافق مع `net10.0`), `netstandard2.0`, `net462` | **`SUPPORTED`** (خط LTS مدعوم حتى `2028-08-14`) | **متوافق تقنيًا عبر TDS 7.4 — `UNVERIFIED` عمليًا حتى اختبار المختبر (`Phase B`)**؛ وخارج جدول المحركات النشط في MS Learn (الذي يبدأ من `2017`) لانتهاء دعم 2014 القياسي ما لم يكن تحت `ESU` | **المسار الأساسي المرشح (ADO.NET صريح)** |
| **`Microsoft.Data.SqlClient` (STS)** | `7.1.1` (خط `7.0.x` ينتهي `2026-12-17`) | `net8.0`, `net9.0`, `net10.0`, `netstandard2.0`, `net462` | **`SUPPORTED` (STS قصير الأمد)** | نفس الملاحظة عبر TDS 7.4، لكنه `STS` (ينتهي بعد 3 أشهر من الإصدار التالي) وفصل امتدادات Azure | غير مفضل على `6.1.8 LTS` لنظام طويل الأمد |
| **`Dapper` + `Microsoft.Data.SqlClient`** | `Dapper 2.1.89` + `6.1.8 LTS` | `net10.0` صريح في `Dapper.nuspec` (دون تبعيات إضافية، ترخيص `Apache-2.0`) | **`SUPPORTED`** | يعتمد كليًا على `SqlConnection` وعلى SQL يدوي صريح متوافق مع `Compatibility Level 120` — **`UNVERIFIED` عمليًا حتى `Phase B`** | **المسار المرشح الاختياري فوق ADO.NET** |
| **`Microsoft.EntityFrameworkCore.SqlServer` (`EF Core 10`)** | `10.0.15` (يعتمد على `Microsoft.Data.SqlClient 6.1.7`) | `net10.0` (مدعوم حتى `2028-11-10`) | **`SUPPORTED` على `.NET 10`** | **`UNSUPPORTED` رسميًا من Microsoft**؛ الوثيقة الرسمية (محدثة `2026-07-13`) تحصر المحركات المدعومة في **SQL Server 2019 فما فوق** | **غير موصى به كخيار أساسي لـ SQL 2014** |
| **`EntityFramework` (`EF 6.5.2`) / `Microsoft.EntityFramework.SqlServer`** | `6.5.2` (`Microsoft.EntityFramework.SqlServer` يعتمد على `Microsoft.Data.SqlClient 6.1.4`) | `netstandard2.1` | وضع صيانة فقط (`Modern Lifecycle Policy`، دون تطوير ميزات) | `EntityFramework 6.5.2` الأصلي يعتمد على `System.Data.SqlClient 4.8.6` المهجورة؛ حزمة `Microsoft.EntityFramework.SqlServer 6.5.2` مزود مجتمعي/صيانة فقط | **غير موصى به لنظام جديد بـ `.NET 10` وClean Architecture** |
| **`System.Data.SqlClient`** | `4.9.1` | `.NETCoreApp`, `.NETStandard`, `.NETFramework` | **`UNSUPPORTED` (Deprecated)** | يصفها ملف `.nuspec` صراحةً: `DEPRECATED - Use Microsoft.Data.SqlClient.` | **مرفوض نهائيًا** |

### المصادر الرسمية المعتمدة في التقييم
1. **دعم مزود EF Core SQL Server:** [EF Core SQL Server Database Provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/) (محدّث `2026-07-13`: يذكر `Microsoft SQL Server (2019 onwards)` صراحةً).
2. **دورة حياة `Microsoft.Data.SqlClient`:** [SqlClient Driver Support Lifecycle](https://learn.microsoft.com/en-us/sql/connect/ado-net/sqlclient-driver-support-lifecycle) (محدّث `2026-10-23`: خط `6.1` LTS حتى `August 14, 2028`؛ وجدول التوافق النشط يذكر `SQL Server 2017+`).
3. **دورة حياة `SQL Server 2014`:** [SQL Server 2014 Lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/sql-server-2014).
4. **دعم TLS 1.2 لـ SQL Server:** [KB3135244 — TLS 1.2 support for Microsoft SQL Server](https://learn.microsoft.com/en-us/troubleshoot/sql/database-engine/connect/tls-1-2-support-microsoft-sql-server).
5. **سلاسل الاتصال والـ Encryption:** [Connection strings with Microsoft.Data.SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/connection-strings) (محدّث `2026-10-28`).
6. **مستويات التوافق T-SQL:** [ALTER DATABASE compatibility level](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-database-transact-sql-compatibility-level).
7. **تغييرات `EF Core 8` و`OPENJSON`:** [EF Core 8 Breaking Changes — Contains uses OPENJSON](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/breaking-changes) و[EF Core Releases](https://learn.microsoft.com/en-us/ef/core/what-is-new/release-planning).
8. **بيانات حزم NuGet الفعلية (`.nuspec`):** `Microsoft.Data.SqlClient` (`6.1.8`, `7.1.1`), `Dapper` (`2.1.89`), `Microsoft.EntityFrameworkCore.SqlServer` (`10.0.15`), `EntityFramework` (`6.5.2`), `Microsoft.EntityFramework.SqlServer` (`6.5.2`), `System.Data.SqlClient` (`4.9.1`).

---

## 4. التحليل المقارن المفصل لطرق الوصول للبيانات

### المسار الأول: `Microsoft.Data.SqlClient` (`6.1.8 LTS`) — ADO.NET الصريح (المعياري)

- **آلية العمل:** فتح اتصال `SqlConnection` واستخدام `SqlCommand` و`SqlParameter` و`SqlTransaction` و`SqlDataReader` مباشرة داخل `Infrastructure` Repository Adapters.
- **نقاط القوة:**
  1. توافق `LTS` حتى `2028-08-14`، متناغم مع دعم `.NET 10 LTS` (حتى `2028-11-10`).
  2. تحكم كامل 100% في T-SQL المكتوب ليتوافق تمامًا مع `Compatibility Level 120` (أو `110`/`100` إن لزم)، دون أي SQL مولد تلقائيًا قد يفشل على محرك 2014.
  3. دقة كاملة في أنواع البيانات المالية (`SqlDbType.Decimal` مع ضبط `Precision` و`Scale` صراحةً وتجنب تحويلات `float`/`double`).
  4. وضوح تام في حدود المعاملات (`BeginTransactionAsync`)، مستويات العزل (`Serializable` أو `UPDLOCK, HOLDLOCK`) لحماية فرادة `MAIN_SALARY` ومنع سباق التزامن.
  5. أداء عالٍ واستخدام مباشر لـ `SqlBulkCopy` عند استيراد ملفات كبيرة مستقبلًا إذا لزم.
- **نقاط الضعف / التكلفة:**
  - كتابة كود تحويل الصفوف (`Reader -> Domain/ReadModel DTO`) يدويًا، وزيادة الحاجة لاختبارات التكامل للتحقق من أسماء الأعمدة والـ Mapping.

### المسار الثاني: `Microsoft.Data.SqlClient` (`6.1.8 LTS`) + `Dapper` (`2.1.89`)

- **آلية العمل:** استخدام `Microsoft.Data.SqlClient` لإدارة الاتصال والمعاملات والـ `SqlParameter` الدقيقة عند الحاجة، مع الاستعانة بـ `Dapper` لتمرير معاملات الكائنات ورسم صفوف القراءة (`QueryAsync<TRow>`) وتقليل التكرار في `Infrastructure`.
- **نقاط القوة:**
  1. يستهدف `Dapper 2.1.89` إطار `net10.0` صراحةً ودون أي تبعيات خارجية إضافية (`Apache-2.0`).
  2. يحتفظ بكل مزايا المسار الأول من حيث التحكم الكامل في T-SQL الخاص بـ `Compatibility Level 120` و`ACID` والأداء.
  3. يقلل الكود الميكانيكي (`boilerplate`) في قراءة الجداول والتقارير (`Read Models`) مقارنة بـ `SqlDataReader` اليدوي.
- **نقاط الضعف / الضوابط المطلوبة:**
  1. الافتراضي في `Dapper` لمعاملات `string` هو `NVARCHAR(4000)` عند عدم تحديد الطول أو `DbString`؛ يجب الحذر في استعلامات الفهارس والأنواع النصية (`VARCHAR` مقابل `NVARCHAR`) وتجنب تمديد الخطة (`plan cache bloat`).
  2. يجب ضبط الدقة المالية (`decimal`) بوضوح وتمرير `SqlTransaction` الصريحة دائمًا، وعدم استخدام `Dapper` لبناء SQL ديناميكي بالدمج النصي (منع `SQL Injection`).
  3. وجود تبعية ثانية خفيفة يجب فحصها في حوكمة التبعيات.

### المسار الثالث: `Microsoft.EntityFrameworkCore.SqlServer` (`10.0.15`)

- **آلية العمل:** ORM متكامل مع `DbContext`، تتبع التغييرات (`Change Tracking`)، `LINQ to SQL`، و`EF Core Migrations`.
- **لماذا هو `UNSUPPORTED` هنا؟**
  1. تنص وثيقة Microsoft Learn الرسمية صراحةً على أن المحركات المدعومة تبدأ من **SQL Server 2019**.
  2. بدءًا من EF Core 8، يولّد المترجم عبارات `OPENJSON` بشكل افتراضي عند استخدام `.Contains()`، وهو ما ينهار بخطأ صياغة (`Incorrect syntax near 'OPENJSON'`) على `SQL Server 2014` (`Compatibility Level <= 120`).
  3. ورغم وجود الخيار البرمجي `UseCompatibilityLevel(120)` في مزود SQL Server لتقييد توليد SQL المتقدم (مثل تجنب `OPENJSON` أو `json` type)، فإن **استخدام `UseCompatibilityLevel(120)` لا يخلق دعمًا رسميًا من Microsoft لمحرك 2014**؛ بل قد تبقى مزايا أو استعلامات/Migrations أو سلوكيات metadata غير مختبرة من Microsoft على محرك خرج من الدعم الرئيسي والقياسي.
  4. إذا اعتمد المشروع `EF Core 10`، فلن تغطي اختبارات Microsoft أي خلل يظهر حصرًا على `SQL Server 2014`.

### المسار الرابع: البدائل المستبعدة (`EF 6.5.2` و`System.Data.SqlClient`)

- **`System.Data.SqlClient 4.9.1`:** معلنة رسميًا كحزمة مهجورة (`DEPRECATED`) في NuGet؛ يمنع استخدامها.
- **`EntityFramework 6.5.2` / `Microsoft.EntityFramework.SqlServer 6.5.2`:** مكتبة إرثية في وضع الصيانة (`maintenance-only`) لنقل الأنظمة القديمة من `.NET Framework`، ولا تدعم أنماط EF Core الحديثة أو تصميمًا نظيفًا لمشروع `.NET 10` جديد، كما أن حزمتها الأصلية تعتمد على `System.Data.SqlClient` المهجورة.

---

## 5. العزل المعماري في Clean Architecture وحدود الترقية المستقبلية

بموجب [خريطة Bounded Contexts وAggregates](../architecture/DDD-BOUNDED-CONTEXTS-AND-AGGREGATES.md) و[معايير OOP وSOLID](../architecture/OOP-AND-SOLID-DESIGN-STANDARDS.md) و[DEPLOYMENT-AND-CONTRACTS.md](../architecture/DEPLOYMENT-AND-CONTRACTS.md):

1. **التبعيات المسموح بها:**
   - `PayrollReviewSystem.Core`: **صفر تبعيات** على SQL Server أو `Microsoft.Data.SqlClient` أو `Dapper` أو `EF Core`.
   - `PayrollReviewSystem.Application`: تعرّف واجهات العقود (`Ports`) بحجم الاستخدام (`ISP`) مثل `IPayrollCycleRepository` أو `IUnitOfWork` أو قارئات الاستعلام (`IPayrollReadStore`)، ولا تستورد أي نوع من `System.Data` أو `Microsoft.Data.SqlClient`.
   - `PayrollReviewSystem.Infrastructure`: الطبقة **الوحيدة** المسموح لها باستيراد `Microsoft.Data.SqlClient` (و`Dapper` إذا اعتُمد لاحقًا) وتنفيذ `SqlConnectionFactory` و`Adapters`.
   - `PayrollReviewSystem.Api`: يربط `Dependency Injection` بتكوين `Infrastructure` دون كتابة استعلامات SQL داخل الـ Controllers.
2. **سهولة الترقية إلى SQL Server 2019/2022+ لاحقًا:**
   - عندما ترقّي الجامعة الخادم مستقبلًا من `SQL Server 2014` إلى `SQL Server 2022`، لن يتغير أي سطر في `Core` أو `Application` أو عقود الـ API.
   - يمكن حينها الاستمرار على `Microsoft.Data.SqlClient` (نفس البروتوكول مع رفع مستوى TLS 1.2/1.3 وتفعيل ميزات أحدث عند رفع `Compatibility Level`)، أو حتى إضافة `EF Core` لـ Write/Read Context محدد خلف نفس الـ Ports إذا قرر المالك ذلك عبر ADR جديد.
3. **تنويه مهم حول فصل قواعد البيانات (مستقبلًا):**
   - هذه الدراسة تخص قاعدة بيانات **نظام المراجعة نفسه** المستهدف على `SQL Server 2014`، ولا تغيّر قاعدة الحوكمة القاضية بعدم التعديل على بيانات Oracle أو قاعدة الجامعة التشغيلية أو القراءة المباشرة منها دون تفويض صريح وعقد تكامل معتمد.

---

## 6. متطلبات الأمان والتشفير وTLS 1.2 والمصادقة

### أ) مشكلة كسر الاتصال الافتراضي في `Microsoft.Data.SqlClient` (>= 4.0)

بدءًا من `Microsoft.Data.SqlClient 4.0` (ويشمل `6.1.x` و`7.x`)، أصبحت القيمة الافتراضية في سلسلة الاتصال هي **`Encrypt=Mandatory`** (`true`) بدل `Optional` (`false`). يعني ذلك ما يلي عند الاتصال بـ `SQL Server 2014`:
1. إذا كان خادم `SQL Server 2014` قديمًا بلا تحديثات `TLS 1.2` (إصدار RTM أو SP1 قبل التحديث التراكمي)، فقد يفشل التفاوض الأمني (`Handshake`) مع `.NET 10` الحديث الذي يعطل `TLS 1.0/1.1` افتراضيًا على أنظمة التشغيل الحديثة.
2. حتى مع وجود `TLS 1.2` على الخادم، إذا كان `SQL Server 2014` يستخدم **شهادة موقعة ذاتيًا (Self-Signed Certificate)** مولدة تلقائيًا عند بدء الخدمة، فسيرفض `Microsoft.Data.SqlClient` الاتصال افتراضيًا بالخطأ المعروف:
   `The certificate chain was issued by an authority that is not trusted.`

### ب) الحد الأدنى المطلوب على خادم SQL Server 2014 لـ TLS 1.2 (وفق `KB3135244`)

| مسار التحديث على SQL Server 2014 | الحد الأدنى للإصدار (`Build`) | المرجع |
| :--- | :--- | :--- |
| **SQL Server 2014 RTM** | `CU12` (`12.0.2564.0`) فما فوق | `KB3135244` |
| **SQL Server 2014 SP1** | `CU5` (`12.0.4439.1`, `KB3130926`) فما فوق | `KB3135244` |
| **SQL Server 2014 SP2 / SP3** | مدعوم ضمن حزمة الخدمة (`SP2` / `SP3` وآخر CU/ESU) | `KB3135244` |

### ج) سياسة إعدادات الاتصال (`Encrypt`, `TrustServerCertificate`, Authentication)

- **`Encrypt=Strict` (TDS 8.0):** **غير مدعوم على SQL Server 2014** (يخص SQL Server 2022+ فقط). لا تفعله على بيئات 2014.
- **الإنتاج / البيئة شبه الإنتاجية (Recommended Security Posture):**
  - الإبقاء على `Encrypt=True` (`Mandatory`)، مع تثبيت شهادة TLS صادرة عن جهة تصديق داخلية موثوقة (`Internal Enterprise CA`) على خادم SQL Server، أو تحديد `HostNameInCertificate` / `ServerCertificate` (متاح في `Microsoft.Data.SqlClient 5.1+`) عند الحاجة.
  - **يُحظر** اعتبار `TrustServerCertificate=True` أو `Encrypt=False` إعدادًا افتراضيًا للإنتاج، لما يسببه من قابلية لهجمات الوسيط (`MITM`) وكشف بيانات المصادقة والبيانات المالية الحساسة.
  - يُسمح بـ `TrustServerCertificate=True` **فقط** داخل بيئة مختبر محلية معزولة خالية من البيانات الحقيقية أثناء الاختبار المؤقت، وبموافقة صريحة وموثقة.
- **المصادقة (`Authentication`):**
  - داخل شبكة Domain (`Active Directory`) في البيئة المحلية: يُفضَّل `Integrated Security=True` (`Windows Authentication` / حساب خدمة بأقل صلاحيات `Least Privilege`) لتجنب تخزين كلمات مرور SQL نصية في ملفات الإعدادات.
  - عند تعذر مصادقة Windows: استخدام `SQL Authentication` بحساب مخصص لصلاحيات جداول المراجعة فقط، وتخزين سر الاتصال في متغيرات بيئة/مخزن أسرار آمن لا يرفع إلى GitHub مطلقًا.

---

## 7. خطة ومصفوفة اختبارات التحقق في المختبر المعزول (`Phase B — NOT_RUN`)

> **حالة التنفيذ الحالية:** **جميع اختبارات `TC01..TC08` بحالة `NOT_RUN`**.
> تم التحقق من وجود خدمة `MSSQLSERVER` وأداة `SQLCMD.EXE` محليًا دون الاتصال بأي مثيل، التزامًا بعدم لمس أي قاعدة بيانات غير مخصصة أو قاعدة بيانات الجامعة، وبانتظار توفير وتوثيق حاوية أو مثيل `SQL Server 2014 (12.x)` اصطناعي ومعزول بنسبة 100%.

### شروط تشغيل المختبر المعزول مستقبلًا
1. قاعدة بيانات اصطناعية جديدة ومفرغة من أي بيانات موظفين حقيقية (مثال: `PayrollReview_CompatLab_Synthetic`).
2. التحقق من رقم البناء (`@@VERSION` و`SERVERPROPERTY('ProductVersion')` و`SERVERPROPERTY('ProductLevel')`) ومستوى التوافق (`sys.databases.compatibility_level = 120` واختبار `110` إن لزم).
3. مشروع اختبار تكامل معزول أو PoC مؤقت لا يُدمج في الإنتاج إلا بعد ADR واختيار المزود.

### مصفوفة الاختبارات المطلوبة (`TC01..TC08`)

| المعرّف | سيناريو الاختبار | ما يجب التحقق منه فعليًا | شرط النجاح المتوقع (`Pass Criteria`) | الحالة الحالية |
| :--- | :--- | :--- | :--- | :--- |
| **`TC01`** | التحقق من الإصدار و`Compatibility Level` | تنفيذ `SELECT @@VERSION, SERVERPROPERTY('ProductVersion'), compatibility_level` من `.NET 10` | قراءة نسخة `12.x` و`compatibility_level = 120` بدقة دون أخطاء اتصال | **`NOT_RUN`** |
| **`TC02`** | تفاوض `TLS 1.2` والشهادات | اختبار الاتصال في الحالات الثلاث: (أ) الافتراضي `Encrypt=Mandatory` مع شهادة غير موثوقة، (ب) `Encrypt=Mandatory` مع شهادة CA موثوقة / `ServerCertificate`، (ج) توثيق فشل `Encrypt=Strict` المتوقع على 2014 | إثبات سلوك الفشل الآمن عند غياب الثقة، ونجاح الاتصال المشفر بـ TLS 1.2 عند وجود شهادة موثوقة | **`NOT_RUN`** |
| **`TC03`** | النصوص العربية (`RTL` و`NVARCHAR`) | إدراج واسترجاع نصوص عربية مركبة، مسافات، أرقام، وعلامات تشكيل في أعمدة `NVARCHAR` باستخدام `SqlParameter` صريح | مطابقة ثنائية (`ordinal equality`) للنص العربي دون تحويل إلى `????` | **`NOT_RUN`** |
| **`TC04`** | الدقة المالية (`DECIMAL`) والقيم السالبة والصفرية و`NULL` | كتابة وقراءة مبالغ بـ `decimal(18,2)` و`decimal(18,4)` تشمل قيمًا سالبة وصفرًا و`DBNull` | الحفاظ الكامل على `decimal` دون تقريب `double`؛ التمييز التام بين `NULL` و`0.00m` | **`NOT_RUN`** |
| **`TC05`** | توافق استعلامات `Compatibility Level 120` | فحص استعلامات القراءة والانضمام والتجميع والترحيل (`OFFSET/FETCH` المتوفرة منذ `2012 / 110+`) وتجربة كشف رفض `OPENJSON` أو `STRING_SPLIT` | نجاح جميع استعلامات الخطة `120` بدون اعتماد على دوال `130+`؛ وتأكيد فشل `OPENJSON` على مستوى `120` | **`NOT_RUN`** |
| **`TC06`** | المعاملات والتزامن و`Rollback` (`ACID`) | محاكاة تنفيذين متزامنين (`Task.WhenAll`) لمحاولة حجز نفس `MAIN_SALARY` لنفس العلاقة والشهر، واختبار التراجع عند الفشل الوسطي | نجاح عملية واحدة فقط وفشل/رفض الثانية عبر قيد فريد أو قفل معاملات، وعدم بقاء بيانات جزئية عند `Rollback` | **`NOT_RUN`** |
| **`TC07`** | الأداء المبدئي للدفعات (اصطناعي) | إدراج/قراءة 1,000 سطر استحقاق اصطناعي متكرر الكود والاسم مع قياس الزمن والذاكرة | إكمال التنفيذ بزمن مقبول ومستقر دون استنزاف Pool الاتصالات | **`NOT_RUN`** |
| **`TC08`** | اختبار `EF Core 10` (للمقارنة والتوثيق) | تشغيل استعلام يحتوي `.Contains(ids)` على جدول اصطناعي بدون ثم مع `UseCompatibilityLevel(120)` | إثبات سقوط الاستعلام الافتراضي بسبب `OPENJSON`، وتوثيق أثر `UseCompatibilityLevel(120)` مع تأكيد بقاء التصنيف `UNSUPPORTED` رسميًا | **`NOT_RUN`** |

---

## 8. التوصية المعمارية المشروطة وقرار المالك المطلوب

بناءً على الحقائق الموثقة في القسمين (3) و(4):

1. **استبعاد تام لـ `System.Data.SqlClient` (`DEPRECATED`) و`EF Core 10` كخيار أساسي لـ `SQL Server 2014` (`UNSUPPORTED` رسميًا من Microsoft لمحرك 2014).**
2. **التوصية المبدئية المشروطة للبحث (`Phase A`):**
   - الاعتماد على **`Microsoft.Data.SqlClient 6.1.8` (`LTS` حتى `2028-08-14`)** داخل طبقة `Infrastructure` كمحرك اتصال أساسي على `.NET 10 LTS`.
   - **المفاضلة التنفيذية المفتوحة للمالك وChatGPT عند اختيار التصميم:**
     - **الخيار (أ): `Microsoft.Data.SqlClient 6.1.8 LTS` (ADO.NET صريح بالكامل)** — أقل تبعيات، أقصى شفافية، صفر مفاجآت في Mapping أو Types المالية.
     - **الخيار (ب): `Microsoft.Data.SqlClient 6.1.8 LTS` + `Dapper 2.1.89` (اختياري للربط فقط)** — يختصر تكرار Mapping الصفوف مع الحفاظ على T-SQL الصريح المتوافق مع `120` والمعاملات الصريحة.
3. **شروط الانتقال إلى أي اعتماد فعلي (`Provider Selection ADR`):**
   - التحقق ميدانيًا من رقم تحديث خادم `SQL Server 2014` الفعلي لدى الجهة المالكة (هل هو على `SP1 CU5+` / `SP2` / `SP3` وهل يدعم `TLS 1.2` وهل توجد تغطية `ESU`؟).
   - تشغيل اختبارات **`TC01..TC08` (`Phase B`)** على بيئة `SQL Server 2014 (12.x)` اصطناعية معزولة تمامًا والحصول على نتائج `PASS` فعلية.
   - مراجعة `ChatGPT` المعمارية واعتماد **ADR مستقل** من المالك يثبت اختيار المزود قبل إضافة أي حزمة `NuGet` إلى ملفات `.csproj` الإنتاجية أو إنشاء أي `Schema` أو `Migrations`.
