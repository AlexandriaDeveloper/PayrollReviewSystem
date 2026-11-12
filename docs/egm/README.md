# بوابة Elephant–Goldfish للمراجعة المعمارية المستقلة

> **تحديث الفهرسة في 11 نوفمبر 2026:** راجع [حالة التقارير الحالية](REVIEW-STATUS.md). توجد تقارير فعلية في المستودع؛ استقلال تشغيلها غير متحقق في دراسة التوثيق. عبارات الحالة أدناه تصف مرحلة إعداد سابقة.

**الحالة الحالية للتقارير الموجودة:** `REPORTS_PRESENT_INDEPENDENCE_UNVERIFIED` (كانت حالة الإعداد التاريخية قبل 8 نوفمبر `NOT_RUN`؛ توجد الآن ستة تقارير وتقرير موحد في المستودع لكن استقلال سياق تشغيلها غير متحقق بأدلة تشغيل مستقلة، وتظل مراجعة `ADR-003/004` في [Issue #30](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/30) وسجل الفحص التاريخي في [Issue #27](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/27) بحالة `NOT_RUN` حتى صدور دليل تشغيل مستقل). هذه الوثائق وسجل التحكيم في [EGM-ADJUDICATION-REGISTER-AR.md](EGM-ADJUDICATION-REGISTER-AR.md) ليست شهادة قبول تصميم أو تصريحًا لـGemini بكتابة كود.

## لماذا نستخدم EGM؟
التصميم الحالي صاغه ChatGPT بمساعدة المالك وبسياق المحادثات السابقة (**Elephant**). المطلوب مراجعة مستقلة بواسطة **Goldfish** يبدأ بسياق جديد، ويقرأ حزمة التصميم ومصادرها المكتوبة فقط، ويكشف الافتراضات والتناقضات قبل التنفيذ. **GitHub هو مصدر الحقيقة** لكل الوثائق والنتائج، والمالك صاحب القرار؛ Gemini ينفذ المهمة المعتمدة بعد البوابة.

### حزم المراجعة الثلاث
| ترتيب | ملف الإدخال إلى EGM | ماذا يختبر؟ | الحالة |
| --- | --- | --- | --- |
| 1 | [التصميم الأساسي للصرفيات والمجموعات والعلاقات](01-DOMAIN-AND-PAYROLL-DESIGN-DRAFT.md) | Person/Employment/Payee؛ رئيسية واحدة؛ منفصلة متعددة؛ المجموعات والتصحيحات والتزامن | `REPORTS_PRESENT_INDEPENDENCE_UNVERIFIED` (التاريخ: `NOT_RUN` في #27) |
| 2 | [القواعد المالية التاريخية والمراجعة والتسويات](02-TEMPORAL-FINANCIAL-RULES-DESIGN-DRAFT.md) | درجة تاريخية/إجازات؛ قانون؛ ثلاثة خصومات؛ تعديل المراجع؛ المتجمدات وترحيل خصم جزئي | `REPORTS_PRESENT_INDEPENDENCE_UNVERIFIED` (التاريخ: `NOT_RUN` في #27) |
| 3 | [مصادر Oracle والإدخال البديل ورعاية الطفل](03-ORACLE-INTAKE-AND-EVIDENCE-DESIGN-DRAFT.md) | PDF/Excel/الشاشة؛ هوية السطر؛ البصمة؛ جودة النقل؛ الاستمارات والتسوية | `REPORTS_PRESENT_INDEPENDENCE_UNVERIFIED` (التاريخ: `NOT_RUN` في #27) |

## دورة العمل التي اتفق عليها المالك
1. ChatGPT يكتب أو يحدث ملف الحزمة ومراجعها على GitHub بموافقة توثيق فقط.
2. يتم تثبيت إصدار `main`/SHA عند بدء الجولة.
3. تُشغّل أداة EGM الحقيقية بواسطة بيئة تدعم إنشاء **Fresh Subagents**. لا تسرب المحادثات السابقة أو ذاكرة ChatGPT للمراجع.
4. يكتب الـGoldfish تقريرًا مستقلًا ويشير إلى ملف/جزء/دليل/مثال قابل لإعادة الإنتاج؛ تستخدم [صيغة التقرير](GOLDFISH-FINDINGS-TEMPLATE.md).
5. تُسجل الملاحظات المانعة في GitHub Issue مستقلة، وتُعدّل مواصفات التصميم عبر PR Markdown وتُعاد مراجعة الأجزاء المتأثرة.
6. المالك يحسم تعارضات العمل/القانون ويعتمد التصميم **ونطاق مهمة تنفيذ محددة**؛ لا يكتفي بتقرير «PASS».
7. Gemini ينفذ على فرع وPR بعد إذن التنفيذ، ومع اكتمال الكود يُراجع الفرق بين الكود والمواصفة. `egm-drift-report` يُستخدم **بعد وجود كود فعلي**.

## شرط نزاهة المراجعة
لا يجوز وضع `PASS` أو `GOLDFISH_REVIEWED` بناءً على مجرد إنشاء هذه الملفات أو اختبار روابطها أو قراءة ChatGPT لها. غياب Subagent مستقل = `NOT_RUN / BLOCKED_ENVIRONMENT`، وليس فشلًا أو نجاحًا للمواصفة. حتى مع Goldfish مستقل، يظل فحص الاختبارات والاعتماد القانوني/البشري منفصلين.

## كيفية التشغيل
راجع [البروتوكول والأوامر](REVIEW-PROTOCOL.md) و[بوابات المراجعة المالية](../governance/REVIEW-GATES.md).

**وثائق المشروع الأساسية:** [Blueprint](../architecture/BLUEPRINT-V1.md)، [القرارات](../governance/DECISION-REGISTER.md)، [المتطلبات واختبارات القبول](../specs/REQUIREMENTS-AND-ACCEPTANCE.md).

**المرجع الخارجي:** [EGM الرسمي](https://github.com/google/elephant-goldfish-model)، [شرح بيئات التشغيل](https://github.com/google/elephant-goldfish-model/blob/main/docs/harnesses.md). EGM أداة مساعدة للمراجعة، لا منتج Oracle أو أداة لتطبيق قوانين المرتبات.
