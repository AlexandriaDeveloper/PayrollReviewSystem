# تقرير مراجعة Goldfish المستقل — التدقيق الهيكلي وتماثل الوثائق (Structural Layout Validator)

> **حالة المراجعة:** COMPLETE (تم الفحص الهيكلي المستقل عبر Fresh Context).  
> **تنبيه حوكمة:** بصفتي وكيل مراجعة فرعي للقراءة فقط (Read-Only Subagent)، أُعيد نص التقرير الكامل أدناه لاعتماده وتخزينه في المسار: `docs/egm/reports/egm-structural.md`.

---

## بيانات التشغيل
| الحقل | القيمة عند التنفيذ |
| --- | --- |
| `ReviewRunId` | `RUN-20261108-STRUCTURAL` |
| `TargetDocumentPath` | `docs/egm/01-DOMAIN-AND-PAYROLL-DESIGN-DRAFT.md`, `docs/egm/02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md`, `docs/egm/03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md` |
| `TargetDocumentSha` | `CURRENT_HEAD` |
| `RepoMainSha` | `1d68936639a01044e1c74a6cb9a78b5694dae752` |
| `EGMPluginVersion` | `1.0.0 (Elephant-Goldfish Protocol)` |
| `Harness` | `Antigravity Subagent (egm-structural)` |
| `FreshContextVerified` | `YES` |
| `ReviewerPersonas` | `egm-structural (Structural Layout Validator and Document Architect)` |
| `ReadFiles` | `docs/egm/GOLDFISH-FINDINGS-TEMPLATE.md`, `docs/egm/REVIEW-PROTOCOL.md`, `docs/egm/01-DOMAIN-AND-PAYROLL-DESIGN-DRAFT.md`, `docs/egm/02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md`, `docs/egm/03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md`, `docs/governance/OWNER-OPERATING-REQUIREMENTS.md`, `docs/architecture/ERD-CORE.md`, `docs/architecture/ERD-IMPORT-AND-AUDIT.md`, `docs/specs/MONTHLY-PAYROLL-GROUPS-AND-DISBURSEMENT-TYPES.md`, `docs/architecture/DDD-BOUNDED-CONTEXTS-AND-AGGREGATES.md`, `docs/domain/LEGAL-AND-RULE-GOVERNANCE.md`, `docs/security/SECURITY-AND-PRIVACY.md` |
| `RunStatus` | `COMPLETE` |
| `ReportArtifact` | `docs/egm/reports/egm-structural.md` |

---

## الملاحظات الهيكلية والتنظيمية

| ID | الدرجة | ملف/سطر/إثبات | سيناريو يكشف الخلل | الأثر المالي/التقني | الإصلاح والاختبار المقترح | قرار المالك | Issue/PR |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `EGM-STR-001` | `BLOCKER` | [03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md](file:///f:/Prog-Projects/PayrollReviewSystem/docs/egm/03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md#L78-L94) | غياب قسم إلزامي مستقل ومُرقّم لـ «الأسئلة المفتوحة» (`Open Questions`) في حزمة 03 بخلاف حزمتي 01 و02. وردت أسئلة متفرقة تحت السطر 78 دون ترقيم كعقد صريح للمالك. | يخاطر بدفع المطور (Gemini) إلى تخمين سياسات معالجة غياب المستندات أو حالات تضارب الإدخال المباشر مع ملفات PDF دون قرار مالك صريح. | إنشاء قسم رئيسي `## 8. أسئلة مفتوحة يجب ألا يخمن Goldfish إجاباتها` مع ترقيم محدد للأسئلة المفتوحة، وفصلها عن قسم معايير القبول. | معلّق للمالك | لم يُنشأ بعد |
| `EGM-STR-002` | `BLOCKER` | [02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md](file:///f:/Prog-Projects/PayrollReviewSystem/docs/egm/02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md#L20-L36) | انعدام قسم مستقل لعقد المدخلات والمخرجات الاصطناعية (`## Input/Output اصطناعي`) بصيغة JSON مقننة في حزمة 02، والاكتفاء بنصوص حسابية غير مهيكلة بالسطور 25-28. | غياب عقد بيانات (Data Contract) مهيكل لحالات `BLOCKED_CONFLICT` و`INPUT_MISSING` و`ExpectedCalculationLine`، مما يمنع التحقق الآلي من الـ Snapshots. | إضافة قسم `## 5. Input/Output اصطناعي` بنماذج JSON كاملة للمدخلات التاريخية والمخرجات المتوقعة وحالات الفشل الزمني. | معلّق للمالك | لم يُنشأ بعد |
| `EGM-STR-003` | `MAJOR` | [02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md](file:///f:/Prog-Projects/PayrollReviewSystem/docs/egm/02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md#L37-L51) | تناقض دلالي بين عنوان القسم ونوع المخطط البياني؛ العنوان يَنص على `## 4. رسم التسلسل للمراجعة` (Sequence Diagram) بينما المخطط الفعلي كُتب كـ `flowchart TD`. | تضليل المطورين والمهندسين المعماريين؛ مخطط التدفق يصف انسياب القرار الإجرائي ولا يوضح الرسائل والحدود الزمنية بين الكيانات كما يقتضي رسم التسلسل. | تصحيح العنوان إلى `## 4. مخطط تدفق المراجعة الزمنية` أو استبدال الكتلة بمخطط تسلسل حقيقي `sequenceDiagram` يوضح التفاعل بين `RuleResolver`, `SnapshotStore`, و`AuditCase`. | معلّق | لم يُنشأ بعد |
| `EGM-STR-004` | `MAJOR` | [03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md](file:///f:/Prog-Projects/PayrollReviewSystem/docs/egm/03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md#L60-L76) | تناقض دلالي بين العنوان ونوع المخطط؛ العنوان يَنص على `## 5. رسم UML لتحديد مصادر الإدخال`، بينما الكتلة البرمجية هي `flowchart TD` (Process Flowchart) وليست رسم UML معتمد (Class/Component/Sequence). | عدم وضوح العقود الصنفية؛ حزمة إدخال المصادر تحتاج توضيح علاقات الـ Interfaces والـ Adapters (`ICentralPayrollSourceIntake`) وليس مجرد خطوات إجرائية عامة. | تغيير العنوان إلى `مخطط مسار قنوات الإدخال` أو إدراج `classDiagram` معتمد يوضح مشتقات `ICentralPayrollSourceIntake` وعلاقتها بـ `SourceCaptureSession`. | معلّق | لم يُنشأ بعد |
| `EGM-STR-005` | `MAJOR` | [01-DOMAIN-AND-PAYROLL-DESIGN-DRAFT.md](file:///f:/Prog-Projects/PayrollReviewSystem/docs/egm/01-DOMAIN-AND-PAYROLL-DESIGN-DRAFT.md#L35-L38) مقابل [MONTHLY-PAYROLL-GROUPS-AND-DISBURSEMENT-TYPES.md](file:///f:/Prog-Projects/PayrollReviewSystem/docs/specs/MONTHLY-PAYROLL-GROUPS-AND-DISBURSEMENTS-TYPES.md#L55-L62) | تباين مسميات الكيانات بين ERD حزمة 01 والوثيقة المرجعية المعتمدة؛ حزمة 01 تستخدم `DISBURSEMENT_RECIPIENT` و`SOURCE_DOCUMENT_LINK`، بينما المواصفة المرجعية تستخدم `PAYROLL_DISBURSEMENT_RECIPIENT` و`DISBURSEMENT_SOURCE_LINK`. | تفكك سلامة النماذج (Schema Drift) بين وثيقة المراجعة والوثائق المعمارية التفصيلية، مما يعطل إنشاء جداول قاعدة البيانات والـ DTOs تلقائيًا. | توحيد التسمية في حزمة 01 لتطابق المواصفة المرجعية تمامًا (`PAYROLL_DISBURSEMENT_RECIPIENT` و`DISBURSEMENT_SOURCE_LINK`). | معلّق | لم يُنشأ بعد |
| `EGM-STR-006` | `MAJOR` | عبر كافة حزم التصميم [01, 02, 03] | غياب التناظر الهيكلي (Structural Asymmetry) في عدد وترتيب العناوين الرئيسية عبر الحزم؛ حزمة 01 تضم 9 أقسام متسلسلة تشمل الواجهات، بينما حزمة 02 تضم 7 أقسام، وحزمة 03 تضم 8 أقسام وتفتقر كلتاهما لقسم الواجهات البرمجية المنفصل (`واجهات التصميم والمراجعة`). | يجعل هيكل الوثائق غير قابل للتحقق القياسي المؤتمت (Linting/Validation Gate)، ويخفي العقود البرمجية في حزمتي 02 و03. | اعتماد القالب المعياري ذي الأقسام التسعة (9-Section Blueprint) وإعادة ضبط هيكلة حزمتي 02 و03 لتطابق حزمة 01 تمامًا. | معلّق للمالك | لم يُنشأ بعد |
| `EGM-STR-007` | `MINOR` | [02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md](file:///f:/Prog-Projects/PayrollReviewSystem/docs/egm/02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md#L20-L34) | استخدام الترقيم الهجائي العربي (`### أ.`, `### ب.`, `### ج.`, `### د.`) تحت القسم 3، بينما تلتزم حزم النظام ومواصفاته بالترقيم العددي العشري (`3.1`, `3.2`, `3.3`). | تباين أسلوبي يضعف التوافق مع المعايير القياسية للتوثيق الهندسي (Documentation Formatting Standards). | توحيد العناوين الفرعية لتصبح `### 3.1` حتى `### 3.4`. | للتسوية | لا يوجد |
| `EGM-STR-008` | `MINOR` | [03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md](file:///f:/Prog-Projects/PayrollReviewSystem/docs/egm/03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md#L25) | إدراج المثال الاصطناعي كعنوان فرعي H3 (`### مثال اصطناعي`) داخل قسم `3. Excel + الشاشة` بدلًا من فرزه في قسم رئيسي H2 متناظر كما في حزمة 01. | تفاوت في مستوى الأهمية والبروز البصري لعقود المدخلات والمخرجات عبر الحزم. | رفع المثال إلى قسم رئيسي H2 مستقل متناظر مع حزمة 01: `## 5. Input/Output اصطناعي`. | للتسوية | لا يوجد |
| `EGM-STR-009` | `QUESTION` | عام عبر كافة ملفات `docs/egm/` | هل يعتمد المالك هيكلًا إلزاميًا موحدًا (Rigid Standard Blueprint) يتكون دائمًا من 9 أقسام رئيسية ثابتة لجميع مسودات EGM الحالية والمستقبلية؟ | يضمن انضباط الجولات اللاحقة ويمنع تفاوت الصياغات بين المراجعين والمصممين. | توثيق القالب الهيكلي الموحد في `docs/egm/BLUEPRINT-STANDARD.md`. | معلّق للمالك | لا يوجد |

---

## مقارنة التناظر الهيكلي بين حزم التصميم (Structural Symmetry Matrix)

| القسم القياسي المطلوب | حزمة 01 (الهوية والصرفيات) | حزمة 02 (القواعد الزمنية) | حزمة 03 (قنوات الإدخال) | حالة الاتساق |
| --- | --- | --- | --- | --- |
| **1. النطاق / المشكلة** | `## 1. المشكلة التي ينبغي فحصها` | `## 1. نطاق المراجعة المستقلة` | `## 1. نطاق التصميم` | `MINOR` تباين في اللفظ فقط |
| **2. الكيانات وحدود الهوية** | `## 2. الكيانات وحدود الهوية` | `## 2. محرك القواعد وقواعد السلامة` | `## 2. هوية السطر والمصدر` | `MAJOR` عدم تجانس تصنيف الكيانات |
| **3. القيود الحاكمة / الحالات** | `## 3. القيد الحاكم الذي نطلب نقده` | `## 3. حالات تكشف قصور تصميم...` | `## 3. Excel + الشاشة` | `MAJOR` تباين وظيفي في محتوى القسم 3 |
| **4. المخطط البنيوي (Diagram)** | `## 4. رسم ERD للمراجعة النقدية` (`erDiagram`) | `## 4. رسم التسلسل للمراجعة` (`flowchart TD` ❌) | (مرحل للقسم 5) | `MAJOR` عدم اتساق الترتيب وتناقض نوع الرسم |
| **5. نماذج Input/Output** | `## 5. Input/Output اصطناعي` (JSON مقنن) | **مفقود تمامًا كقسم مستقل** ❌ | مدرج كـ H3 فرعي داخل قسم 3 ⚠️ | `BLOCKER` غياب العقد في 02 وتباين موضعه في 03 |
| **6. واجهات التصميم والعقود** | `## 6. واجهات التصميم والمراجعة` | **مفقود تمامًا** ❌ | **مفقود تمامًا كقسم مستقل** ❌ | `MAJOR` غياب تعريفات الواجهات الموحدة |
| **7. معايير وحالات القبول** | `## 7. حالات قبول أساسية` | `## 5. حالات قبول تلزم قبل البرمجة` | `## 7. معايير القبول` | `MAJOR` اختلاف رقم القسم وترتيبه |
| **8. الأسئلة المفتوحة** | `## 8. أسئلة مفتوحة...` (مرقمة 1-5) | `## 6. أسئلة لا يجيب عنها Goldfish...` (مرقمة 1-5) | **مفقود كقسم مرقم صريح** ❌ | `BLOCKER` غياب قسم الأسئلة المرقمة من 03 |
| **9. المراجع التصميمية** | `## 9. المراجع التصميمية المحددة` | `## 7. المراجع التصميمية` | `## 8. الملفات المرجعية المحددة` | `MINOR` تفاوت أرقام الأقسام |

---

## قرار الجولة
- **نتائج التدقيق الهيكلي المستقل:** كشف التدقيق عن وجود **خللين مانعين (2 Blockers)** يتمثلان في غياب قسم الأسئلة المفتوحة الإلزامي في حزمة 03 وغياب قسم نماذج المدخلات والمخرجات المهيكلة في حزمة 02، بالإضافة إلى **أربعة اختلالات جسيمة (4 Majors)** في دلالات المخططات البيانية وتسميات الكيانات والتناظر البنيوي بين الوثائق.
- **المشاكل المانعة المفتوحة:** `EGM-STR-001`, `EGM-STR-002`.
- **قرارات المالك المطلوبة:**
  1. اعتماد القالب المعياري الموحد (9-Section Blueprint) وتكليف المعماري بإعادة تنظيم الحزمتين 02 و03 وفقًا له.
  2. اعتماد المسميات الموحدة لكيانات الصرف والربط في حزمة 01 (`PAYROLL_DISBURSEMENT_RECIPIENT` و`DISBURSEMENT_SOURCE_LINK`).
  3. حسم نوع المخططات البيانية في الحزمتين 02 و03 (إما بتصحيح العناوين لتعكس أنها Flowcharts أو ترقيتها لمخططات تسلسل وطبقات UML فعلية).
- **التصميم مُعتمد للبرمجة:** **لا** (حتى تصحيح الهياكل وحل المشاكل المانعة).
- **دليل تفعيل Fresh Subagent:** تم التشغيل في جلسة مستقلة بالكامل تحت المعرف `RUN-20261108-STRUCTURAL` على بيئة Antigravity Subagent مع فحص مباشر ومطابقة دقيقة لملفات المستودع.

