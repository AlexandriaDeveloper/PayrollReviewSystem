# حالة مراجعات EGM

تاريخ التحديث: 13 نوفمبر 2026 (ضمن مهمة التحكيم التوثيقية [`Issue #57 — EGM-ADJ-001`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/57) على أساس المراجعة الأولية المؤرخة 11 نوفمبر 2026).

- **الحالة الحالية للتقارير الستة والتقرير الموحّد:** `REPORTS_PRESENT_INDEPENDENCE_UNVERIFIED / PARTIAL_CORE_FIRST_ADJUDICATION`
- **تقارير على القرص (`docs/egm/reports/`):** `egm-comprehension.md`، `egm-design.md`، `egm-implementation-readiness.md`، `egm-structural.md`، `egm-style.md`، `egm-zoom-out.md`؛ والتقرير الموحّد في [`../../consolidated_review_report.md`](../../consolidated_review_report.md) ونسخته في [`consolidated_review_report.md`](consolidated_review_report.md).
- **سجل التحكيم والتتبع الكامل (74 صفًا خامًا):** [سجل تحكيم ملاحظات EGM وتوحيد تتبعها (`EGM-ADJUDICATION-REGISTER-AR.md`)](EGM-ADJUDICATION-REGISTER-AR.md).

## التمييز بين وجود التقارير واستقلال تشغيلها
1. **عدم مسح السجل التاريخي لـ`NOT_RUN`:** لا تلغي تقارير 8 نوفمبر حقيقة الفحص التاريخي المسجل في [Issue #27](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/27) (أفاد التشخيص يوم 8 نوفمبر بعدم العثور على استدعاءات `manage_subagents` أو Artifacts وقتها) ولا حالة `NOT_RUN` لجولة `ADR-003/004` في [Issue #30](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/30).
2. **عدم الاكتفاء بالـ`Self-Report`:** إدراج `FreshContextVerified: YES` داخل نص التقارير نفسها لا يكفي وحده لإثبات استقلال التشغيل (`FreshContextVerified`) دون سجل تشغيل وArtifacts يمكن التحقق منهما مستقلاً؛ كما تحوي التقارير معرفات هدف غير دقيقة مثل `CURRENT_HEAD` (`egm-structural.md`) و`Current Working Tree` مع `EGMPluginVersion: N/A` (`egm-implementation-readiness.md`)، إضافةً إلى `RepoMainSha=1d68936639a01044e1c74a6cb9a78b5694dae752`.
3. **جرد الملاحظات وتضارب الأعداد:** أظهر حصر الجداول الفعلية **74 صفًا خامًا** (`20 BLOCKER, 31 MAJOR, 14 MINOR, 9 QUESTION`) متطابقًا مع [سجل أدلة 11 نوفمبر](../reviews/2026-11-11-study-evidence.md#L13-L23)، بينما أعلن التقرير الموحّد خطأً `64` غير مكرر (`16 BLOCKER`) مع سرد **`63` بندًا فقط (`BLK-01..15`, `MAJ-01..26`, `MIN-01..13`, `Q-01..09`)** وإسقاط أو دمج غير معلن لبعض الصفوف.
4. **أحكام التحكيم التوثيقي (`Core-First`):** جرى تحكيم الملاحظات المؤثرة على أولوية `Core-First` ودورة `D-029 / P-017` في [سجل التحكيم](EGM-ADJUDICATION-REGISTER-AR.md) مع التمييز الصريح بين **صحة النقد** و**رفض الحل المقترح المخالف للمتطلب**، ووسم الملاحظات غير المفحوصة بـ`NOT_ADJUDICATED_YET`. لا يزال البدء بأي كود أو SQL محظورًا دون تفويض مستقل من المالك بعد استكمال البوابات.
