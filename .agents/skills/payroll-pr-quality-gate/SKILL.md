---
name: payroll-pr-quality-gate
description: تدقيق Pull Requests وDefinition of Done لنظام PayrollReviewSystem، مع فحص جودة الاختبارات وSOLID وDDD وسلامة المرتبات وقاعدة البيانات وIdempotency والأمان والـCI. استخدم عند طلب مراجعة PR أو التحقق من جاهزية الدمج أو اكتمال مهمة Gemini؛ لا تدمج كودًا أو تنشر دون تفويض مستقل.
---

# مهارة فحص جاهزية PR — مراجعة مستقلة قائمة على الدليل

## قبل المراجعة
- اقرأ `AGENTS.md`, `GEMINI.md`, [معايير الهندسة](../../../docs/engineering/ENGINEERING-STANDARDS.md), [تعريف اكتمال المهمة](../../../docs/engineering/DEFINITION-OF-DONE.md)، والـIssue وأحدث `ADR` المعني.
- اعرف `base SHA`, `head SHA`, قائمة الملفات المتغيرة والـDiff الحقيقي؛ لا تعتمد على وصف PR وحده، ولا تفترض أن نتيجة CI تساوي صحة المالية.
- حدّد حالة تفويض التنفيذ والـGoldfish المرتبط؛ عدم وجود تقرير Goldfish مستقل عند اللزوم يمنع وصف التصميم بأنه `APPROVED`.
- هذه المهارة للمراجعة والتوثيق، **وليست إذنًا لتعديل قاعدة بيانات/تنفيذ كود/تشغيل CI أو دمج PR** من غير تصريح. Gemini لا يدمج PRs البرمجية.

## بروتوكول فحص PR
1. **Scope:** كل ملف متغير مرتبط بالـIssue؟ هل تغيرت قواعد أو جداول لم تُعتمد؟ هل هناك Migration خطرة؟
2. **Architecture:** DDD Context Ownership وAggregate Invariants، Clean Architecture Dependency Rule، OOP/SOLID وKISS/DRY/YAGNI؛ اختبر حدود الوحدات لا شكل الكلاسات فقط.
3. **Financial Safety:** `decimal` والتقريب والـSnapshots وتواريخ valid/known، الفرق بين مصدر/متوقع/مُراجع/مدفوع، MAIN واحد وSEPARATE متعددة، source occurrences مستقلة.
4. **Failure/Concurrency:** retry/idempotency، طلبان متوازيان، انقطاع أثناء حفظ ملف، duplicate input، معاملات وتسويات لا تتجاوز المستند؛ لا `last-write-wins` بلا كشف.
5. **Security/Privacy:** تحقق صلاحيات الخادم، عدم وصول مراجع لمجموعة خارج نطاقه، File Upload/Excel Formula Injection، إخفاء PII والأسرار، Audit Event غير قابل للمحو الصامت.
6. **Tests:** راجع اختبارات Unit/Integration/Contract/Security/Performance حسب أثر التغيير، وشغّل فقط ما يسمح به سياق المهمة؛ قارن النتائج المرفقة بالأوامر الفعلية. اختبارات DB على provider حقيقي لا InMemory وحده.
7. **Delivery:** Check build/lint/CI، أثر API/Schema، خطة احتفاظ/نسخ احتياطي/رجوع أو Forward-Fix، مراقبة وتشخيص، التوافق مع واجهة عربية RTL.

## نتيجة إلزامية
اكتب تقريرًا بجدول صغير `Gate | PASS/FAIL/NOT_RUN/N/A | Evidence | Risk` مع سطور محددة (`file:line`) لكل مشكلة و`BLOCKER/MAJOR/MINOR/QUESTION`. افصل بين:
- **ملاحظات مثبتة** من Diff/Tests/Logs؛
- **مقترحات تحسين** تحتاج قياسًا؛
- **أسئلة مالك/قانون** لا يجوز للمراجع حسمها.

**لا تصف المهمة `DONE` إذا فشل شرط مانع أو كانت أدلة جوهرية `NOT_RUN`.** أعطِ ردًا محددًا لـGemini بالاختبار المطلوب وإصلاح النطاق فقط. لا Merge/Deploy ذاتيًا.
