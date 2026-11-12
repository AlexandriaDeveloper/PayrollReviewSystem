# فهرس التوثيق العربي وحالة اعتماد المعلومات

> **نقطة البدء الحالية:** [قاعدة العمل الموحدة](WORKING-BASELINE-AR.md)، [فهرس 43 مذكرة دراسة](research/README.md)، [مراجعة المستودع](reviews/2026-11-11-repository-review-ar.md)، [حالة EGM المحدثة](egm/REVIEW-STATUS.md)، [سجل تحكيم ملاحظات EGM (`EGM-ADJ-001`)](egm/EGM-ADJUDICATION-REGISTER-AR.md)، و[مواصفة دورة الملاحظة والرد والحسم ومتابعة الفروق (`SPEC-029-001`)](specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md). اعتماد نقل المعرفة لا يفعّل قانونًا أو كودًا.

**مرجع التوثيق بتاريخ 6 نوفمبر 2026.** المستودع يحتوي مخططات وقواعد ومواصفات ولا يعني وجود برنامج منفذ أو مخطط SQL معتمد. جميع ملفات Markdown مشروحة بالعربية؛ تُترك أسماء الكلاسات والمفاتيح والـAPI وأكواد الاختبارات بالإنجليزية لتظل متوافقة مع البرمجة.

## ابدأ بهذه الملفات
| الترتيب | الملف | ماذا ستتعلم؟ |
| --- | --- | --- |
| 1 | [AGENTS.md](../AGENTS.md) و[GEMINI.md](../GEMINI.md) | تعليمات ChatGPT وGemini وحدود الدمج |
| 2 | [التعاون عبر GitHub](governance/COLLABORATION.md) | دورة Issue ثم PR والمراجعة |
| 3 | [بوابات المراجعة](governance/REVIEW-GATES.md) | ما يمنع قبول كود أو بيانات |
| 4 | [سجل القرارات](governance/DECISION-REGISTER.md) | ما اعتمده المالك وما زال مقترحًا |
| 5 | [مفهوم النظام](architecture/ARCHITECTURE.md) | حدود وحدات البرنامج |
| 6 | [مفاهيم قاعدة البيانات](architecture/DOMAIN-AND-DATA-MODEL.md) | هوية الموظف والكيانات |
| 7 | [البنود والوقائع المالية](domain/FINANCIAL-COMPONENTS-AND-FACTS.md) | مرجع 2015 والاستحقاقات والاستقطاعات |
| 8 | [الأثر الرجعي](domain/TEMPORAL-AND-RETROACTIVE.md) | التواريخ والمتجمدات والتسوية |
| 9 | [قرارات المراجعة](domain/AUDIT-FINDINGS-AND-DECISIONS.md) | الاعتماد والفرق والتصحيح |
| 10 | [استيراد Oracle](domain/ORACLE-IMPORT-AND-RECONCILIATION.md) | قراءة PDF وتطابق التقارير |
| 11 | [حوكمة القانون](domain/LEGAL-AND-RULE-GOVERNANCE.md) | مصادر النصوص وسريانها |
| 12 | [المتطلبات والاختبارات](specs/REQUIREMENTS-AND-ACCEPTANCE.md) | الوظائف والحالات المطلوبة |
| 13 | [مواصفة دورة الملاحظة والرد والحسم ومتابعة الفروق (`D-029/P-017/D-030`)](specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md) | آلتا الحالات، قناتا الرد، العقود التصورية، وقبول `SPEC-029-001` |

## مخططات ERD وUML
- [Blueprint — المخطط الشامل](architecture/BLUEPRINT-V1.md).
- [ERD: الكليات والأشخاص والعلاقات والمبالغ المعتمدة](architecture/ERD-CORE.md).
- [ERD: مستندات Oracle والأسطر المالية المتكررة](architecture/ERD-IMPORT-AND-AUDIT.md).
- [ERD: المقارنة والاعتماد والمتجمدات](architecture/ERD-REVIEW-AND-RETRO.md).
- [UML: الكلاسات والواجهات](architecture/UML-CLASSES.md).
- [UML: الرحلة من الاستيراد للاعتماد وحالات المراجعة](architecture/UML-WORKFLOWS.md).
- [التشغيل الداخلي وعقود البيانات والأمان](architecture/DEPLOYMENT-AND-CONTRACTS.md).
- [مراحل Gemini والقرارات المطلوبة](architecture/IMPLEMENTATION-SLICES.md).

## أمثلة عملية للمدخلات والمخرجات
1. [ERD بالبيانات: من ملف Oracle إلى الجداول ونتيجة المراجع](examples/01-ERD-INPUT-OUTPUT.md).
2. [UML بالبيانات: الكلاسات وطلب JSON والاستجابة وإعادة فتح الصرفية](examples/02-UML-REVIEW-EXAMPLES.md).

كل الأسماء والمبالغ في الأمثلة **افتراضية**، والمخططات في GitHub تُعرض باستخدام Mermaid.

## تصميم محرك القواعد وتداخل القرارات والأمثلة
- [محرك القواعد الزمنية: اختيار الحكم حسب الكادر والدرجة والبند والفترة، وحسم تضارب القرارات](specs/TEMPORAL-RULE-RESOLUTION-AND-CONFLICTS.md).
- [مثال عملي ERD/UML: غياب + ترقية + إجازة وتعديل بأثر رجعي](examples/03-TEMPORAL-RULE-CONFLICT-EXAMPLE.md).
- [سياسة تركيب آثار القواعد ومنع التداخل غير المحسوم، واختبارات TR-021 إلى TR-030](specs/RULE-INTERACTION-AND-VALIDATION.md).
- **مهم:** كل هذه مخططات/اختبارات تصميم وليست قواعد غياب معتمدة أو كودًا موجودًا.

## ثلاث حالات إضافية للمراجعة المالية الدقيقة
- [المواصفة: إجازة بدرجة تاريخية، تغيير/إضافة/استبعاد بند، وترحيل فرق خصم وتسويته تحت بند آخر](specs/HISTORICAL-LEAVE-REVIEW-ADJUSTMENTS-AND-DEDUCTION-CARRY.md).
- [أمثلة ERD وUML ومدخلات ومخرجات JSON بالأرقام الافتراضية](examples/04-HISTORICAL-LEAVE-ADJUSTMENT-DEDUCTION-CARRY.md).
- **تنبيه:** الترجيح القانوني لتاريخ الدرجة، وجواز الترحيل أو التسوية من بند بديل، وقواعد تعديل المراجع تحتاج سندًا وصلاحيات معتمدة. لا تنفيذ خصم آلي.

## رعاية الطفل وإدخال الاستحقاقات المركزية من صورة أو يدويًا
- [إجازة رعاية الطفل: طلب الجامعة، قرار الإجازة، تسوية Oracle، واستمارة 132](specs/CHILD-CARE-LEAVE-AND-CENTRAL-SETTLEMENT.md).
- [مسار PDF أو صورة أو إدخال يدوي مع أدلة المصدر ومراجعة دقة النقل](specs/MULTI-CHANNEL-CENTRAL-INTAKE.md).
- [مثال عملي لقرار رعاية الطفل والاستمارتين وصورة وبيانات يدوية، مع ERD/UML/JSON](examples/05-CHILDCARE-AND-MANUAL-INTAKE.md).
هذه **تصميمات مقترحة** وليست تصريحًا بإصدار استمارة Oracle الرسمية، أو اعتماد نسبة قانونية أو إدخال بيانات مركزية دون إثبات.

## البدائل المطلوبة لإدخال الاستحقاقات المركزية بدون PDF

- [مواصفة Excel لبيانات موظف ومجموعة البنود أو إدخال مباشر من الشاشة، مع دوال وERD وUML](specs/EXCEL-AND-DIRECT-EMPLOYEE-PAYROLL-INTAKE.md).
- [ERD الاستيراد](architecture/ERD-IMPORT-AND-AUDIT.md) و[UML الكلاسات](architecture/UML-CLASSES.md) و[UML سير البيانات](architecture/UML-WORKFLOWS.md).
- الأولوية هي **Excel / الشاشة المباشرة**، وليست الصور أو OCR؛ الملفات السابقة التي عرضت مثال الصورة تبقى للتوثيق التاريخي ولا تُعتبر شرطًا للتنفيذ.

## نموذج عملي: ثلاثة استقطاعات لإجازة اعتيادية لعضو هيئة تدريس
- [تصميم قاعدة متعددة الآثار: 156% و91% وثالث 48%/60%/200% حسب الدرجة، وأيام الإجازة/أيام الشهر، ERD وUML وJSON](specs/FACULTY-ANNUAL-LEAVE-MULTI-DEDUCTION-RULE.md).
- 18 اختبارًا تصميميًا `FL-001..FL-018`. النسب وأسماء الأوعية وفئة 200% تحتاج توثيقًا واعتمادًا قبل تفعيل قاعدة حقيقية.

## ملفات دراسة Oracle
- [دراسة النسخة الحالية من OraclePdfParser](reference/ORACLEPDFPARSER-CODE-STUDY.md).
- [تقرير 25 مشكلة ومخاطرة](reference/ORACLE-PARSER-FAILURE-AUDIT.md).
- [عقد استيراد PDF](specs/ORACLE-PDF-ADAPTER-DESIGN.md).
- [هوية السطر المالي والعربية وتكرار الكود](specs/ORACLE-FINANCIAL-LINE-IDENTITY.md).

## تعليمات تشغيل أكدها المالك
- [نطاق وحدة 30200105 والمراجعين](governance/OWNER-OPERATING-REQUIREMENTS.md).
- [البنود التي تؤخذ من المركزية دون إعادة احتساب](domain/CENTRAL-CALCULATED-ITEMS.md).
- [رقابة جزء الوقت ووعاء المعاش](domain/PART-TIME-INSURANCE-CONTROL.md).

## مراجع وحماية وأدوات واجهة
- [خارطة الطريق](process/ROADMAP.md).
- [فهرس الدراسات الأصلية وحدود التحقق](reference/RESEARCH-INVENTORY-STATUS.md).
- [حماية البيانات](security/SECURITY-AND-PRIVACY.md).
- [تصميم شاشة المراجع العربي](ui/REVIEWER-UX.md).
- [قرارات ADR](decisions/ADR-001-separate-independent-review.md) و[الفصل المالي الزمني](decisions/ADR-002-separate-temporal-cash-and-reference-facts.md).

## حالات توثيق المعلومات
- `REQUIREMENT CONFIRMED`: أكدها المالك صراحة.
- `DESIGN PROPOSAL`: اقتراح هندسي لم يعتمد تفاصيله بعد.
- `LEGAL SOURCE UNVERIFIED`: لم يُراجع النص الأصلي والصفحة أو السريان بعد.
- `IMPLEMENTATION AUTHORIZED`: مهمة تنفيذ مرخصة مستقلة.
- `VERIFIED IN CODE`: كود واختبارات فعلية راجعناها وثبتت.

المشاريع السابقة `Payroll-Auditor` و`Py-Ai-Moratabat` و`OraclePdfParser` و`PDF-EXTRACTOR` و`IProgram2` مصادر دراسة لا تنقل قوانين أو جداول إلى هنا تلقائيًا. والمذكرات المرفوعة سابقًا ليست بديلًا عن أصول القوانين.

**كل قرار برمجي أو قاعدة مالية جديدة يجب أن تحافظ على السجلات المصدرية والاعتماد التاريخي، وأن تعود للمراجع والمالك عند وجود غموض.**

## مجموعات الموظفين وتشغيلات الشهر ونوع الصرفية
- [صرفية المرتب الرئيسية الواحدة والصرفيات المنفصلة المتعددة: ERD وسياسة فرادة السجل](specs/MONTHLY-PAYROLL-GROUPS-AND-DISBURSEMENT-TYPES.md).
- [أمثلة ERD/UML وInput/Output لمرتب واحد و3 صرفيات منفصلة وتصحيح](examples/06-MONTHLY-PAYROLL-GROUPS-AND-DISBURSEMENTS.md).
- [ERD الاستيراد والمجموعات](architecture/ERD-IMPORT-AND-AUDIT.md)، [UML الكلاسات](architecture/UML-CLASSES.md)، [UML سير العمل](architecture/UML-WORKFLOWS.md). الصرفيات المنفصلة ليست خاضعة لقيد `MainSalarySlot`.

## بوابة المراجعة المستقلة EGM / Goldfish
اعتمد المالك مسار مراجعة معماري مستقل **قبل تنفيذ التغييرات الجوهرية**: ChatGPT يصمم، وGoldfish يراجع في سياق جديد، والمالك يحسم ويعتمد، وGemini ينفذ. **الحالة الآن للتقارير الستة الموجودة على القرص: `REPORTS_PRESENT_INDEPENDENCE_UNVERIFIED`** (توجد تقارير على القرص لكن استقلال تشغيلها غير متحقق بأدلة تشغيل مستقلة؛ بينما تبقى المراجعة الموثقة تاريخيًا في [Issue #27](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/27) ومراجعة `ADR-003/004` في [Issue #30](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/30) بحالة `NOT_RUN` حتى توفر دليل تشغيل مستقل).
- [حالة تقارير EGM وقيود إثبات الاستقلال](egm/REVIEW-STATUS.md)
- [سجل تحكيم ملاحظات EGM وتوحيد تتبعها (`EGM-ADJ-001`)](egm/EGM-ADJUDICATION-REGISTER-AR.md)
- [فهرس الحزم الثلاث وحالتها](egm/README.md)
- [بروتوكول التثبيت والتحقق من Fresh Context والتشغيل](egm/REVIEW-PROTOCOL.md)
- [حزمة نطاق الموظفين والصرفيات الرئيسية والمنفصلة](egm/01-DOMAIN-AND-PAYROLL-DESIGN-DRAFT.md)
- [حزمة القواعد الزمنية والتسويات](egm/02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md)
- [حزمة Oracle وExcel والشاشة ورعاية الطفل](egm/03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md)
- [قالب توثيق نتائج المراجع المستقل](egm/GOLDFISH-FINDINGS-TEMPLATE.md)
تُعتمد القواعد القانونية والقرارات التجارية لدى أصحاب صلاحياتها؛ ولا يحوّل EGM مقترحات التصميم إلى SQL أو كود معتمد تلقائيًا.

## القرار المعماري DDD / Modular Monolith / Clean Architecture (7 نوفمبر 2026)
**اعتمد المالك المنهجية؛ التفاصيل الفيزيائية والـBounded Contexts المقترحة ما زالت في انتظار مراجعة Goldfish.**
- [ADR-003 — اعتماد DDD + Modular Monolith + Clean Architecture](decisions/ADR-003-ddd-modular-monolith-clean-architecture.md).
- [خريطة مبدئية لـBounded Contexts وAggregate Roots وحدود التكامل واختبارات التصميم](architecture/DDD-BOUNDED-CONTEXTS-AND-AGGREGATES.md).
- [Issue #28](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/28) لتتبع إعداد قرار التصميم، وليست إذن برمجة.
- **مهم:** ملف `docs/egm/01-DOMAIN-AND-PAYROLL-DESIGN-DRAFT.md` ومراجعه الثمانية **لم تتغير** أثناء الجولة الحالية؛ نتيجة المراجعة الحالية لا تصادق تلقائيًا على هذا التفصيل الجديد.

## اعتماد OOP وSOLID كمعايير جودة التصميم والتنفيذ (7 نوفمبر 2026)
وافق المالك على **OOP + SOLID Principles** لتكملة `DDD + Modular Monolith + Clean Architecture`؛ اعتماد المبادئ لا يعني إقرار كود أو SQL أو جميع الكلاسات المقترحة.
- [ADR-004 — قرار اعتماد OOP وSOLID](decisions/ADR-004-object-oriented-and-solid-principles.md).
- [الدليل التطبيقي لـEncapsulation/Polymorphism وSRP/OCP/LSP/ISP/DIP في نظام المرتبات](architecture/OOP-AND-SOLID-DESIGN-STANDARDS.md).
- [Issue #31](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/31) لتوثيق هذا القرار فقط.
- اختبارات تصميم `SOLID-001..018` **لم تنفذ بعد**؛ تُستخدم عند مراجعة أكواد Gemini المصرح بها لاحقًا. لم تتغير ملفات ومراجع Goldfish الجاري في هذا PR.

## المهارات الهندسية الموجهة إلى Gemini/Antigravity
اعتمد المالك المعايير العشرة للهندسة البرمجية و`Definition of Done`، وأضفنا **مهارتين فعليتين `SKILL.md`** على مستوى المشروع في `.agents/skills/` لبيئات Gemini/Antigravity المتوافقة. **لا يُفهم من وجود الملفات أن الوكيل الحالي فعّلها؛ على المستخدم سحب تحديث المستودع ثم إعادة تحميل/بدء جلسة مهارات.**
- [ADR-005 — قرار معايير جودة البرمجيات وAgent Skills](decisions/ADR-005-engineering-quality-and-agent-skills.md).
- [Engineering Standards — التفاصيل العشرة والحالات الاصطناعية](engineering/ENGINEERING-STANDARDS.md).
- [Definition of Done — معايير مراجعة الأدلة لكل PR](engineering/DEFINITION-OF-DONE.md).
- [Skill التطوير المالي: payroll-engineering-quality](../.agents/skills/payroll-engineering-quality/SKILL.md).
- [Skill التحقق: payroll-pr-quality-gate](../.agents/skills/payroll-pr-quality-gate/SKILL.md).
- [Issue #33](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/33) لتتبع التوثيق، وليس تصريح Gemini بالبرمجة.


## حزمة التخطيط لـGemini / Antigravity
[البرومبت والخطة العربية وUML ومخطط البيانات](planning/README.md) — مسودات مبنية على قاعدة العمل، مع مصفوفة قبول ودليل مراجعة مستقلة ونتائج فحص موثقة. لا تعد اعتمادًا لكود أو SQL.

## دراسات التوافق التقني والبيئي
- [دراسة توافق الوصول للبيانات من .NET 10 مع SQL Server 2014 (DB-COMPAT-001)](research/SQL2014-DATA-ACCESS-COMPATIBILITY-AR.md) — دراسة `RESEARCH_ONLY / NO_PROVIDER_SELECTED` وفق `Issue #50` و`AI-0019` وتصحيحات المراجعة `AI-0022` (`Phase B / TC01..TC11: NOT_RUN`).
- [خطة تجهيز معمل VM مستقل لاختبار SQL Server 2014 (LAB-PLAN-001)](research/SQL2014-ISOLATED-LAB-PLAN-AR.md) — خطة توثيقية فقط (`PLAN_ONLY / LAB_NOT_CREATED`) وفق `Issue #54` و`AI-0030` ومحدّثة وفق `AI-0034` لمعالجة `BLOCKED_ISOLATION` و`BLOCKED_TLS_BUILD` قبل بدء `Phase B`.
