# سجل القرارات والمسائل المفتوحة

## قرارات مؤكدة من مالك المشروع

| المعرف | القرار |
| --- | --- |
| D-001 | المستودع المستقل هو `AlexandriaDeveloper/PayrollReviewSystem` ولا يندمج تلقائيًا مع المشاريع القديمة. |
| D-002 | الهدف مراجعة الصرفيات والاستحقاقات المركزية بصورة مستقلة، وليس تنفيذ الدفع. |
| D-003 | ChatGPT مصمم ومراجع، وGemini مطور منفذ، والتواصل الدائم عبر GitHub. |
| D-004 | حفظ المعرفة والمتطلبات وتعليمات أدوات الذكاء الاصطناعي في Markdown داخل المستودع. |
| D-005 | توثيق التصميم أولًا، والموافقة على التوثيق لا تساوي إذن إنشاء كود. |
| D-006 | الوحدة الحسابية للمجمع الطبي، كود `30200105`، وتضم الطب والأسنان والصيدلة. |
| D-007 | يشمل الموظفين وأعضاء هيئة التدريس والمتعاقدين ومستحقي المكافآت. |
| D-008 | المراجعة تسبق اعتماد الصرف غالبًا، وقد تصل أحداث طارئة تستلزم تعديل المراجعة بعد اعتمادها. |
| D-009 | المراجع يوفر البيانات المالية الأصلية المعتمدة؛ Oracle لا يستبدلها تلقائيًا. |
| D-010 | البرنامج **يقترح** أثر القرارات والعلاوات، والمراجع يقبل أو يعدل مع إثبات السبب. |
| D-011 | يمكن للمراجع قبول تجميع بنود تحت نفس باب الصرف **بضوابط ومبررات**، وليس باعتباره تطابقًا تفصيليًا تلقائيًا. |
| D-012 | المعاشات والتأمينات تُقبل عمومًا من المركزية دون إعادة حساب؛ حالة جزء الوقت تحتاج رقابة محددة على الوعاء. |
| D-013 | عدة مراجعين، وتوزيع مجموعات المرتب بحسب الدورة، وحق المخولين في الصرفيات الأخرى مع سجل تعديلات كامل. |
| D-014 | التشغيل على شبكة داخلية محمية، وينوي المالك تحويل المستودع إلى Private؛ لا تُخزن به بيانات الموظفين. |
| D-015 | صندوق دعم المعاقين وصندوق الشهداء من البنود التي تؤخذ قيمتها من المركزية دون إعادة حساب، مع سلامة استخراجها. |
| D-016 | ChatGPT مفوض بدمج ملفات Markdown فقط بعد مراجعتها دون إذن متكرر؛ لا يشمل الكود. |
| D-017 | مالك النظام طلب تصميم دعم إجازة بأثر تاريخي عند اختلاف الدرجة الحالية، وإمكانية تعديل/إضافة/استبعاد بنود المراجعة، ومتابعة فرق استقطاع في شهر لاحق وربطه ببند آخر **إن جاز**. هذا طلب وظيفة وليس تصريحًا بخصم أو مقاصة تلقائيين. |

## توضيحات متطلبات جديدة — رعاية الطفل ومدخلات المصدر
- **D-018:** يريد المالك أن يراعي النظام إجازة رعاية الطفل وكيفية استخراج الاستمارة المرتبطة بها. يلزم الفصل بين طلب الجامعة الإداري وتسوية Oracle المالية وورقة المراجعة الداخلية؛ تفاصيل القالب الرسمي والصلاحيات غير معتمدة بعد.
- **D-019:** يريد المالك إدخال بيانات الاستحقاقات المركزية من صورة أو يدويًا عند تعذر PDF، مع المحافظة على وظائف المراجعة. درجة إثبات المدخلات وموافقة نسخها مستقلة عن مشروعية الاستحقاق.
- **P-009:** نموذج `ChildCareLeaveEpisode` مع `ChildCareSettlement` و`SalaryForm132` للجزء من الشهر، بعد توثيق النص لكل كادر.
- **P-010:** استيراد متعدد القنوات عبر `SourceCaptureSession` و`CaptureVerification` مع فصل `EvidenceAuthenticity` عن `TranscriptionAccuracy`، ومصدر جزئي من دون PDF وهمي.

- **D-020 (تحديث توضيحي):** المالك حدد البديل المطلوب عندما يتعذر PDF: **Excel فيه بيانات موظف ومجموعة بنود**، أو **شاشة إدخال مباشرة دون Excel أو صورة**. لا يُعتمد OCR أو قراءة الصور كشرط لهذا المسار. ويظل إدخال القيم بديل بيانات للمراجعة لا استمارة Oracle رسمية.
- **P-011:** عقد `ICentralPayrollSourceIntake` المقترح ومسارات `EXCEL_IMPORT / DIRECT_FORM` تتوحد عند `SourceFinancialOccurrence`، بمراجعة نقل ونسخ ومصدر وأكواد مكررة ومطابقة رسمية لاحقة.

- **D-021 (متطلب مؤكد):** يريد المالك إعداد قرار إجازة اعتيادية لعضو هيئة تدريس يخرج 3 استقطاعات منفصلة: 156% من وعاء أول، و91% من وعاء ثانٍ، وثالث 48% أو 60% أو 200% وفق فئة الدرجة، مع نسبة أيام الإجازة إلى أيام الشهر.
- **P-012 (تصميم يحتاج اعتمادًا):** `RuleSet` له `RuleEffect` مستقلة، والثالث `GradeRateTableVersion` بتاريخ الدرجة؛ تحديد كود كل وعاء، فئة 200%، السريان، التقريب وجواز الحساب يحتاج مستندات أصلية.

## اقتراحات تصميم تحتاج اعتمادًا رسميًا

| المعرف | المقترح | السبب |
| --- | --- | --- |
| P-001 | `.NET / ASP.NET Core + Angular RTL + SQL Server` بنظام `Modular Monolith` | مناسب لتطوير محلي قابل للمراجعة؛ الإصدار والتقنية لم يُعتمدا بعد. |
| P-002 | بيانات تاريخية بفترات سريان وحفظ أرشيف المصادر | إعادة مراجعة فترات قديمة دون فقد أصلها. |
| P-003 | محرك قواعد معتمد بالإصدارات وآثار حساب تفصيلية | منع تطبيق قوانين أو تواريخ غير صحيحة. |
| P-004 | قارئ Oracle يعتمد Adapter قابلًا للاستبدال مع فحص صارم | اختلاف أشكال PDF. |
| P-005 | سجل متجمدات وربط المدفوعات ببنود وفترات الاستحقاق | منع الازدواج وتحديد الرصيد الحقيقي. |
| P-006 | فصل صلاحيات المراجع ومعتمد القاعدة المالية | منع تعارض المصالح. |
| P-007 | مخطط Blueprint v0.1 بثلاثة ERD وUML ومخطط تشغيل | مراجعة العلاقات قبل الهجرات البرمجية. |
| P-008 | محرك قواعد زمني يختار النص/الأثر لكل بند وشريحة ونسخة تاريخية مع إدارة تداخل القرارات والدرجات | **مقترح تصميم فقط**؛ ينتظر مصدر كل قانون واعتماد العلاقات الفيزيائية والأولويات قبل التنفيذ. |

## أسئلة لم تحسم بعد
- أشكال تقارير Oracle المعتمدة وأرقام النماذج الرسمية.
- النصوص والقواعد السارية على كل كادر وزمن.
- من يوافق على الاعتماد مع ملاحظة، ومن يعيد فتح اعتماد سابق بعد حدث طارئ؟
- دليل إثبات التسوية البنكية الفعلية مقابل مجرد ظهور المبلغ في كشف الصرف.
- التقريب واحتساب جزء الشهر وفترات الضرائب والتأمينات بأحكامها الموثقة.
- مرجع الهوية الموثوقة، وسياسات تشفير الملفات والاحتفاظ بها.
- تفاصيل التوثيق والصلاحيات على الشبكة الداخلية ومجموعات المراجعين.
- أكواد باب الصرف الرسمية وشروط قبول تجميع البنود.
- كيفية الحصول على الوعاء الذي استخدمته المركزية لحساب الاشتراكات عند العمل جزءًا من الوقت.

**الحسم يتطلب مستندًا أصليًا أو قرارًا من المالك حسب طبيعة السؤال، ولا يجوز افتراض الإجابة في الكود.**

## قرار المالك: مجموعات الشهر وصرفية المرتب الرئيسية
- **D-022 (متطلب مؤكد):** الموظفون مقسمون إلى مجموعات لكل شهر، و**لا يزيد عدد الصرفيات الرئيسية عن واحدة للموظف عن شهر مرتبه**، بينما يمكن أن يكون له أكثر من صرفية منفصلة في الشهر نفسه. التصحيحات وتشغيلات الشهر ليست مرتبًا ثانيًا.
- **P-013 (تصميم مقترح):** `MonthlyMainSalarySlot` فريدة على الوحدة/علاقة العمل/شهر المرتب، `MainSalaryRevision` للإصدارات، `PayrollDisbursement.Kind` و`MonthlyGroupMembership` للإسناد. سياسة تعدد علاقات العمل/جهات الصرف والتعيين تحتاج حسمًا قبل SQL.

## اعتماد المالك لمنهجية EGM — توثيق لا اعتماد تصميم نهائي
- **D-023 (مؤكد):** وافق المالك على منهجية `Elephant` للتصميم لدى ChatGPT، و`Goldfish` لمراجعة مستقلة بسياق جديد، وGitHub لحفظ التصميم والنتائج، وGemini لتنفيذ المهام المعتمدة.
- **P-014 (مسار تطبيق):** ثلاث حزم Goldfish داخل `docs/egm/` لمجالات الهوية والصرفيات، القواعد المالية الزمنية، ومصادر Oracle/Excel/الإدخال. تاريخيًا كان التجهيز قبل التشغيل في حالة `NOT_RUN` (كما يوثق سجل الفحص في [Issue #27](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/27) وتظل مراجعة `ADR-003/004` في [Issue #30](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/30) بحالة `NOT_RUN`)؛ أما التقارير الستة الموجودة حاليًا فحالتها المحدثة `REPORTS_PRESENT_INDEPENDENCE_UNVERIFIED` مع سجل التحكيم الأولي في [REVIEW-STATUS](../egm/REVIEW-STATUS.md) و[EGM-ADJUDICATION-REGISTER-AR](../egm/EGM-ADJUDICATION-REGISTER-AR.md).
- **حدود التفويض:** اعتماد التعاون لا يفوض تثبيت Plugin في بيئة المستخدم أو تشغيل محرك كود أو اعتماد قانون أو دمج ملفات غير Markdown. لا يكفي وجود تقرير «لا مشاكل» وحده لترخيص التنفيذ.

## القرار المعماري الجديد — DDD (7 نوفمبر 2026)
- **D-024 (قرار مالك معتمد):** تطوير `PayrollReviewSystem` وفق `Domain-Driven Design (DDD)` مع `Modular Monolith` و`Clean Architecture` لحماية قواعد المرتبات وفصل المنطق عن الاستيراد والتخزين والعرض. [ADR-003](../decisions/ADR-003-ddd-modular-monolith-clean-architecture.md).
- **P-015 (تفصيل يحتاج مراجعة):** [خريطة Bounded Contexts والـAggregate Roots](../architecture/DDD-BOUNDED-CONTEXTS-AND-AGGREGATES.md) بما فيها `Personnel/Payroll/FinancialFacts/Rules/SourceIntake/Audit/Retro/AccessGovernance` **مقترحة** إلى أن تُراجع Goldfish وتُحسم حدودها. ليست إنشاء جداول.
- **توضيح P-001:** `Modular Monolith` أصبح معتمدًا كأسلوب **تنظيم منطقي وتشغيل مبدئي**، لكن اعتماد `ASP.NET Core / Angular / SQL Server` أو نسخ التقنيات وSQL Schema وملفات النشر لا يزال منفصلًا ومعلّقًا.
- **توثيق مسار EGM:** مراجعة الحزمة الأولى الجارية بدأت من ملفات سابقة؛ لا يجوز إسناد نتيجتها إلى هذا الـADR الجديد دون مراجعة إضافية. لا صلاحية كود/SQL/نشر ناشئة عن D-024.

## اعتماد المالك لمبادئ OOP وSOLID (7 نوفمبر 2026)
- **D-025 (قرار مالك معتمد):** تُطبق مبادئ `Object-Oriented Programming (OOP)` و`SOLID (SRP/OCP/LSP/ISP/DIP)` في تصميم وتنفيذ `PayrollReviewSystem`، **مكملةً** لقرار `DDD + Modular Monolith + Clean Architecture`. [ADR-004](../decisions/ADR-004-object-oriented-and-solid-principles.md).
- **P-016 (تفصيل قابل للمراجعة):** [دليل المعايير والأمثلة والاختبارات المقترحة](../architecture/OOP-AND-SOLID-DESIGN-STANDARDS.md)، مع تجنب `God Classes` والاعتماد المباشر على SQL/PDF في الـDomain أو خلق `interfaces` بلا داعٍ. اختيار تفاصيل كود وPorts وAggregates قيد المراجعة قبل التنفيذ، لا أوامر تنفيذ مستقلة.
- **فصل الحالة:** منهجية OOP/SOLID **معتمدة**، لكن مرور Goldfish على حزمة DDD/الـOOP التفصيلية **لم يحدث بعد**؛ ملف Goldfish الأول قيد التشغيل من جانب المستخدم ولا ينطبق على القرار الجديد بأثر رجعي. لا تغيير في حالات التقارير حتى وصول Artifact وتثبته.

## قرار معايير الجودة وAgent Skills (7 نوفمبر 2026)
- **D-026 (مالك معتمد):** إضافة معايير `TDD/Regression` و`KISS/DRY/YAGNI` و`Database Integrity/ACID` و`Deterministic Financial Calculations` و`Idempotency` و`API Contracts` و`Security/Auditability` و`Observability` و`Performance` و`CI/Safe Delivery`، مع `Definition of Done` قائمة على أدلة. [ADR-005](../decisions/ADR-005-engineering-quality-and-agent-skills.md).
- **D-027 (مالك معتمد):** تقديم المعايير إلى Gemini/Antigravity بمهارات مشروع `SKILL.md` فعلية في [مهارة هندسة المرتبات](../../.agents/skills/payroll-engineering-quality/SKILL.md) و[مهارة تدقيق PR](../../.agents/skills/payroll-pr-quality-gate/SKILL.md)، ومراجع [Engineering Standards](../engineering/ENGINEERING-STANDARDS.md) و[Definition of Done](../engineering/DEFINITION-OF-DONE.md).
- **تفويض التنفيذ ثابت:** `Skill` و`ADR` لا يمنحان تصريح Code/SQL/CI/Deploy، ومرور Goldfish مستقلة وحسم القانون والـBounded Contexts لا يزال بوابة مستقلة. `PASS` تُدعم بنتيجة فعلية فقط.


## توحيد قاعدة العمل وضم الدراسات (11 نوفمبر 2026)
- **D-028 (توجيه مالك معتمد):** إضافة التوثيق المتاح من الدراسة إلى GitHub واتخاذه قاعدة العمل. [ADR-006](../decisions/ADR-006-consolidated-working-baseline.md) و[نقطة البدء](../WORKING-BASELINE-AR.md) و[فهرس المذكرات](../research/README.md). يعتمد هذا مرجعية المعرفة وإتاحتها، ولا يحول المقترحات أو دراسات القوانين التاريخية إلى قواعد تشغيل معتمدة.
- **D-029 (تثبيت وصف المستخدم لدورة العمل):** استلام أوراكل من المركزية ومراجعته بالوحدة، وإبلاغ الملاحظات ثم تصحيحها أو تأجيل فرق أو تسويته، أو قبول رد يثبت سلامة الملف وخطأ المراجع؛ مع حفظ الملاحظة والرد وسبب الحسم ومتابعة التنفيذ عند اللزوم. التفاصيل التقنية والصلاحيات والمواعيد ليست محسومة بهذا الوصف.
- **P-017 (أولوية تحليل مقترحة):** بدء الجزء التشغيلي بدورة الملاحظات والردود والحسم ومتابعة الفروق، ثم توسيع الحساب الآلي للبنود المكتملة السند. لا يعد ذلك تصريحًا بكود أو تغييرًا في مخطط البيانات.
- **تحديث حالة الأدلة:** توجد تقارير EGM الستة منذ الإصدار 844624d؛ استقلال تشغيلها لم يتحقق في الدراسة الحالية. الحالة الحالية وقيودها في [REVIEW-STATUS](../egm/REVIEW-STATUS.md)، وتُقرأ عبارات NOT_RUN السابقة كتاريخ للإعداد وسجلات [Issue #27](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/27) و[Issue #30](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/30)، لا نفي وجود ملفات التقارير.

## إعادة ترتيب الأولويات إلى Core-First وتحكيم EGM التوثيقي (13 نوفمبر 2026)
- **OWNER-PRIORITY-001 ([Issue #56](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/56)):** اعتمد المالك ترتيبًا تخطيطيًا يضع تحكيم EGM وتصحيح حالة تقاريره ثم تثبيت مواصفة دورة الملاحظة والرد والحسم ومتابعة الفروق (`D-029 / P-017`) قبل النظر لاحقًا في أول شريحة `Core/Application` مستقلة عن قاعدة البيانات، مع تأجيل مسار معمل `VM / SQL Server 2014 Phase B` كمسار جانبي دون مساس بـ`MSSQLSERVER`.
- **EGM-ADJ-001 ([Issue #57](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/57)):** تحكيم توثيقي لأولويات `Core-First` و`D-029/P-017` في [EGM-ADJUDICATION-REGISTER-AR](../egm/EGM-ADJUDICATION-REGISTER-AR.md) مع إبقاء باقي الملاحظات بحالة `NOT_ADJUDICATED_YET`؛ ولا يفتح هذا التحكيم إذن كود أو SQL أو دمج.

## قنوات رد المركزية ومواصفة دورة الملاحظة (`Issue #39` و`Issue #59`)
- **D-030 (قرار مالك مؤكد — [Issue #39](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/39)):** نص قرار المالك الصريح: *«البرنامج يدعم ردود المركزية داخليًا وتسجيل الردود الخارجية»*. وعليه يمثل التصميم قناتين منفصلتين: (1) `CentralInternalReply` من حساب مركزي مُصرّح له عند اعتماد الصلاحيات، و(2) `RecordedExternalReply` يسجله مستخدم الوحدة المخوّل مع فصل `RecordedBy` عن `AttributedRespondent` وأدلة نسبة الرد؛ مع منع القبول الآلي أو المساواة بين قناة الرد وقوة إثباته. ولا يُعد هذا القرار تصريحًا بإنشاء حسابات فعلية أو تحديد صلاحيات الحسم أو مواعيد التصعيد التي تظل في انتظار حسم المالك.
- **SPEC-029-001 ([Issue #59](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/59)):** دُمجت النسخة الأولى لمواصفة دورة الملاحظة والرد والحسم ومتابعة الفروق المالية في [AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR](../specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md) عبر [PR #60](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/60) (`fb0e511e7ba16559e3b0adbf205dfae47a3bf25d`).

## قرارات المالك المؤكدة لدورة الملاحظة والحسم والمتابعة (`OD-01..OD-03` — `Issue #61 / SPEC-029-002`)
- **D-031 (`OD-01` — حسم جزئي مؤكد من المالك في [`AI-0057..AI-0061`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41)):** يحدد هذا القرار **صلاحية من يعتمد** (وليس كل شروط وأنواع الإغلاق التي يحكمها `D-033`)؛ حيث يكفي اعتماد **شخص واحد مخوّل** من أحد الأدوار الثلاثة (`UnitReviewer` المراجع المختص | `ChiefReviewer` رئيس المراجعين | `AccountingUnitHead` رئيس الوحدة الحسابية) ضمن نطاق مجموعته **دون اشتراط توقيع ثانٍ** لتنفيذ **العمليات الأربع المعتمدة فقط**: `WithdrawFinding` (سحب ملاحظة ثبت خطؤها)، `ResolveWithCorrection` (حسم ملاحظة بعد وصول كشف مركزي مصحح والتحقق من صحته)، `ApproveDeferredDifference` (اعتماد تأجيل فرق مثبت وتوثيق اتفاق التأجيل والشهر المستهدف)، و`CloseFinancialFollowUp` (اعتماد إغلاق المتابعة المالية وفق أحد مسارَي الإغلاق المحددين في `D-033`)، مع حفظ المستندات/التوثيق والسبب وهوية المعتمد والتاريخ. **لا يتوسع هذا التفويض وتظل بحالة `OWNER_DECISION_REQUIRED`:** عمليات `AcceptDocumentedException` و`ReopenFinding` و`VerifyPartialSettlement` و`VerifyFullSettlement` (التي تختلف عن اعتماد إغلاق المتابعة) و`DisputeFollowUpOrigin` و`ResolveFollowUpDispute` و`CancelFollowUpByOfficialCorrection`، إضافةً إلى حوكمة حساب `CentralAccountResponder`.
- **D-032 (`OD-02` — حسم مؤكد من المالك في [`AI-0062..AI-0063`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41)):** يجوز اعتماد البنود السليمة واعتماد مراجعة مرتب الموظف (`AuditCase`) حتى مع وجود بند عليه ملاحظة لم تُحل، بشرط: (1) بقاء الملاحظة مفتوحة، (2) ظهورها مجددًا كتنبيه في الشهر التالي أو الصرفية التالية إذا لم تُحل، (3) استمرار التنبيه في الظهور حتى حل المشكلة، و(4) الاحتفاظ بتاريخ الملاحظة والأدلة والقرارات بعد حلها. فنيًا يُقترح (`PROPOSED`) ربط التنبيهات المتكررة (`PersistentFindingNotice`) بمعرّف الملاحظة الأصلية `FindingId` دون إنشاء فروق مالية مكررة، مع الفصل الصارم بين حالة اعتماد `AuditCase` وحالة `Finding`/`FollowUp` وعدم اعتبار اعتماد المراجعة إذنًا بالصرف البنكي أو تسوية مالية تلقائية.
- **D-033 (`OD-03` — حسم مؤكد من المالك في [`AI-0064..AI-0066`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41)):**
  1. توثيق أسباب حسم الملاحظات يكون إما بإرفاق ملف/مستند (`AttachmentEvidence`) أو بكتابة ملاحظة من المراجع تشرح سببًا شفهيًا اقتنع به (`ReviewerRecordedOralExplanation`) مع نوع الدليل ونصه وهوية المراجع والتاريخ، ولا يُعتبر التوضيح الشفهي تلقائيًا إثباتًا لسداد مالي مستقل.
  2. تحكم `D-033` **شروط ومسارَي إغلاق المتابعة المالية** (ويُمارَس اعتماد الإغلاق بواسطة المعتمد الفردي المحدد في `D-031`):
     - **(أ) الإغلاق بتسوية مثبتة بمستند مستقل (`FinancialFollowUpClosedVerified` — اسم مقترح `PROPOSED`):** يشترط إثبات التسوية الفعلية بمستند مالي مستقل (`SettlementEvidenceRef`) وتحقق `OutstandingAmount = 0` وفق `AI-0061`.
     - **(ب) الإغلاق التشغيلي بإفادة شفهية موثقة (`ClosedByReviewerAttestation` — اسم مقترح `PROPOSED`):** يجوز تشغيليًا عند غياب مستند السداد بناءً على إفادة شفهية يقتنع بها المراجع (`AI-0065`)، مع توثيق نص الإفادة وسبب الاقتناع وهوية المراجع وتاريخ القرار والمبلغ المرتبط، و**حظر رفع `VerifiedPaidAmount`** أو تصفير الرصيد كسداد مثبت بمستند أو تعديل البيانات المالية تلقائيًا.
  3. إذا أُغلقت متابعة مالية تشغيليًا بناءً على إفادة شفهية ثم ظهر لاحقًا مستند ينفي السداد، **لا يُعاد فتح المتابعة القديمة**؛ بل تبقى المتابعة القديمة مغلقة بتاريخ إغلاقها الأصلي، وتُنشأ **ملاحظة رقابية جديدة (`Finding`)** بتاريخ اكتشاف الدليل الناقض مرتبطة بالمتابعة والملاحظة السابقتين (`OriginatingFollowUpId`, `RelatedPreviousFindingId` — أسماء حقول مقترحة `PROPOSED`)، ويستمر ظهور تنبيهها حتى الحل، مع **منع تكرار المبلغ أو إنشاء خصم مالي جديد تلقائيًا** وعدم اعتبار الإغلاق الشفهي السابق دليلًا على توريد مالي مثبت.
- **SPEC-029-002 ([Issue #61](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/61)):** تحديث مواصفة [AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR](../specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md) وسجل القرارات والذاكرة المعمارية بما يعكس `D-031..D-033` (دُمج عبر [PR #62](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/62) عند `df57f10b2431b60d4c24f4975d8468c42440fbd5`).

---

## D-034 — قرارات المالك المؤكدة لـ `OD-04` (استقلال مراجعة الصرفيات و`132 ع.ح`، وأوامر التسوية للشهر التالي، وفصل الرصد في Oracle عن السداد البنكي)

- **الحالة:** `OWNER_CONFIRMED` *(ضوابط العمل المعتمدة)* | مع مسميات وعقود مقترحة `PROPOSED` + عناصر مفتوحة `OWNER_DECISION_REQUIRED`
- **المراجع المصدرية الدقيقة في [Issue #41](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41):**
  - [`AI-0075`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6849469300) | [`AI-0076`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6849509843) | [`AI-0077`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6849613721) | [`AI-0078`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6850675366) | [`AI-0079`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6851324982) | [`AI-0080`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6851486678) | [`AI-0081`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6851579865) | [`AI-0082`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6851694319) | [`AI-0083`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6853570038)
  - تكليف ChatGPT [`AI-0084`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6854047321) + [Issue #63 (`SPEC-029-003`)](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/63)
- **المواصفة المحدثة:** [`docs/specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md`](../specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md) (`§1`, `§2.1`, `§3.3`, `§4`, `§5.1`, `§6`, `§7`, `§8`)

### ما تم تأكيده من المالك نصًا (`OWNER_CONFIRMED` — `AI-0075..AI-0083`)
1. **استقلال اعتماد مراجعة الصرفيات ([`AI-0075`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6849469300)):** كل صرفية مستقلة في اعتماد مراجعتها؛ يجوز اعتماد مراجعة المرتب الرئيسي (`MAIN_SALARY`) السليم رغم تعليق صرفية منفصلة (`SEPARATE`) للمستفيد نفسه، دون اعتماد المنفصلة ضمنيًا ودون اعتبار ذلك إذنًا بالصرف البنكي.
2. **استمارة `132 ع.ح` الخاصة برعاية الطفل معاملة مراجعة مستقلة عن المرتب الرئيسي ([`AI-0076`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6849509843)):** تُعامل استمارة `132 ع.ح` الخاصة برعاية الطفل بوصفها **معاملة مراجعة مستقلة عن مراجعة المرتب الرئيسي**، دون إلزام بدمج قرارَي المراجعة ودون افتراض تصنيفها الفني ضمن `PayrollDisbursement.Kind` أو افتراض آثار مالية/أيام عمل لم يقررها المالك.
3. **منع التأثير الآلي للصرفية المنفصلة على المرتب الرئيسي وإنشاء أمر تسوية صريح للشهر التالي ([`AI-0077`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6849613721)):** لا تؤثر الصرفية المنفصلة أو خطؤها آليًا على المرتب الرئيسي؛ وإذا صُرف مبلغ فعلًا ويحتاج تسوية في المرتب، ينشئ المراجع صراحةً من «الأوامر» **أمر تسوية خاصًا للشهر التالي**.
4. **تنفيذ المركزية والمراجعة في كشف Oracle للشهر التالي ([`AI-0078`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6850675366)):** الأمر يُسجَّل، و**المركزية هي التي تنفذه**، والمراجع يراجع ظهوره في **كشف Oracle للشهر التالي**؛ البرنامج لا يُرسل ولا يُنفّذ في Oracle آليًا.
5. **استمرار نفس الأمر مفتوحًا عند عدم الظهور (`0`) ([`AI-0079`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6851324982)):** إذا لم يظهر مبلغ التسوية في كشف Oracle للشهر المستهدف، يظل **الأمر الأصلي نفسه مفتوحًا** وتستمر متابعته في الشهر اللاحق دون إنشاء أمر ثانٍ مكرر، مع حفظ تاريخ فحص كل شهر.
6. **استمرار نفس الأمر مفتوحًا عند الظهور الجزئي ([`AI-0080`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6851486678)):** إذا ظهر جزء فقط من المبلغ (مثل 100 من 200) في كشف Oracle، يظل **الأمر الأصلي نفسه مفتوحًا على المتبقي للظهور** للشهر اللاحق، مع توثيق الظهور الجزئي ومنع تكرار احتساب الكشف ذاته، ودون استنتاج سداد مالي محقق (`VerifiedPaidAmount`).
7. **الإغلاق الإداري عند اكتمال الظهور التراكمي في Oracle ([`AI-0081`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6851579865)):** عند اكتمال **الظهور التراكمي** لمبلغ الأمر في كشوف Oracle (مثل 100 سبتمبر + 100 أكتوبر لأمر 200)، **يُغلق الأمر إداريًا** (`AdministrativeOrderClosed` — مقترح `PROPOSED`) دون انتظار مستند مستقل لإغلاق الأمر الإداري، **لكن الإغلاق الإداري يثبت الظهور في كشوف Oracle فقط ولا يساوي تسوية مالية مثبتة بمستند مستقل (`VerifiedPaidAmount`) ولا يصفّر `OutstandingAmount` المالي المحقق**.
8. **كشف Oracle مصحح بعد الإغلاق الإداري: إبقاء القديم مغلقًا وفتح `Finding` جديدة ([`AI-0082`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6851694319)):** إذا وصل بعد الإغلاق الإداري **كشف رسمي مصحح** يلغي جزءًا مما سبق ظهوره، **يبقى أمر التسوية القديم مغلقًا إداريًا بتاريخ إغلاقه الأصلي ولا يُعاد فتحه**، وتُنشأ **ملاحظة رقابية جديدة (`Finding`)** بتاريخ اكتشاف التصحيح مرتبطة بالأمر القديم وبنسختَي Oracle (الأصلية والمصححة)، مع منع تكرار المديونية أو تعديل `VerifiedPaidAmount` تلقائيًا.
9. **إنشاء أمر تسوية جديد للجزء الملغى للشهر التالي بعد الملاحظة الجديدة ([`AI-0083`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41#issuecomment-6853570038)):** بعد فتح الملاحظة الجديدة للجزء الملغى (مثال: 50 من أصل 200 أُلغي بعد إغلاق أمر A)، ينشئ المراجع صراحةً **أمر تسوية جديدًا بقيمة الجزء الملغى للشهر التالي** (`Order-B = 50`)، **مرتبطًا بالأمر القديم المغلق (`RelatedPreviousOrderId`) وبالملاحظة الجديدة (`TriggeringFindingId`) وبنسخة التصحيح الرسمي**، ويُتابع وفق قواعد الأمر المفتوح والإغلاق الإداري دون مضاعفة الالتزام المالي الأصلي ودون رفع `VerifiedPaidAmount`.

### ما يظل مقترحًا (`PROPOSED`) لا قرارًا نهائيًا للجداول أو الـAPI
- الكيان `SettlementOrder` ومسميات حالاته (`OrderRecordedOpen`, `OpenZeroObservedInCycle`, `PartiallyObservedInOracleOpen`, `AdministrativeOrderClosed`) وحقول ربطه (`OriginDisbursementRef`, `RelatedPreviousOrderId`, `TriggeringFindingId`, `CumulativeReportedInOracle`, `RemainingToBeObservedInOracle`) وعقوده التصورية (`CreateNextMonthSettlementOrder`, `RecordSettlementOrderOracleObservation`, `RegisterOfficialOracleReversalAfterAdminClose`) في `§3.3` و`§5.1`.

### ما يظل مفتوحًا (`OWNER_DECISION_REQUIRED`)
- صلاحيات اعتماد مراجعة الصرفيات وإصدار وإلغاء واعتماد أوامر التسوية `SettlementOrder` (مع عدم توسيع صلاحيات `D-031` الفردية المحصورة في العمليات الأربع إليها تلقائيًا).
- قناة إيصال الأمر للمركزية، المهل الزمنية (`SLA`)، ظهور مبلغ أكبر من أصل الأمر، التصحيحات المتعارضة/المتكررة، وغياب كشف الشهر.
- الشكل النهائي لـ`132 ع.ح` والآثار القانونية على وعاء الاستحقاق إن وجدت.
- أنواع مستندات التحويل البنكي/الخزانة المقبولة لإثبات `VerifiedPaidAmount` بعد `AdministrativeOrderClosed`.
- تعدد علاقات العمل أو النقل بين الوحدات (`BLK-02`, `BLK-04`, `BLK-12`, `MAJ-21`)، والمعالجة المحاسبية/الختامية عبر السنوات المالية.

### ملاحظات التنفيذ والإبلاغ
- **SPEC-029-003 ([Issue #63](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/63)):** تحديث مواصفة [AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR](../specs/AUDIT-FINDING-RESPONSE-RESOLUTION-WORKFLOW-AR.md) وسجل القرارات والذاكرة المعمارية بما يعكس `D-034` (`OD-04`: `AI-0075..AI-0083`) وتثبيت إتمام دمج `PR #62` — **توثيق فقط دون كود أو SQL أو DDL أو تعديل قواعد البيانات أو دمج ذاتي**.
