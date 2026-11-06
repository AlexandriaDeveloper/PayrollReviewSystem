# فهرس التوثيق العربي وحالة اعتماد المعلومات

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
