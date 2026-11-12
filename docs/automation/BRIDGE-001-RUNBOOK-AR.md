# تشغيل Agent Bridge للرسائل إلى Gemini — المرحلة الأولى

**الحالة:** تصميم تنفيذي داخل [Issue #43](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/43) وDraft PR؛ **لا تُعد القناة آلية نشطة قبل دمج الـWorkflow على `main`، وتسجيل السر، وظهور اختبار Run حقيقي ناجح.**

## ماذا ينفذ؟

- في [قناة التواصل Issue #41](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/41) يكتب **حساب `AlexandriaDeveloper` فقط** تعليقًا مستقلاً بصيغة:
  ```text
  /run-gemini AI-0005
  ```
- يقرأ `ai-bridge-gemini.yml` تعليق الأمر على `issue_comment.created`، ويتحقق أن رقم Issue هو 41 وليس PR، وصاحبه هو الحساب المخول، وأن الصيغة **مطابقة** ومرتبطة بتعليق سابق يحمل `[BRIDGE-ID: AI-0005]` وصادرًا من `[FROM: CHATGPT]` وموجهًا إلى `GEMINI`.
- Gemini CLI يعمل **بلا أدوات** (`tools.core: []`) ويستقبل محتوى الرسالة كمدخل تحليل، دون صلاحية تعديل ملفات، أو تنفيذ أوامر، أو كتابة تعليقات بنفسه. يظهر رد Gemini في ملف نتائج مؤقت لا يحتوي Secret.
- تنشر وظيفة أخرى منفصلة الرد في Issue #41 ومعه `TRIGGER-COMMENT-ID` ومرجع `Actions Run`. الردود المنشورة من البوت أو التعليقات العادية لا تحفز التنفيذ مجددًا.
- **يعمل عند أوامر جديدة فقط**؛ الرسائل السابقة لا تُعاد معالجتها تلقائيًا بعد الدمج. محادثة ChatGPT الحالية ليست طرفًا آليًا؛ تستلزم مراجعة عند استدعائها.

## سبب فصل الصلاحيات

| الخطوة | صلاحيات GitHub | ما تملكه |
| --- | --- | --- |
| `gate` | `contents:read` و`issues:read` | تأكيد صاحب الأمر ورقم Issue ورسالة المصدر؛ دون Secret |
| `run` | `contents:read` و`issues:read` | مفتاح Gemini محجوب عن بقية الخطوات، لا `issues:write` أو `contents:write`، ولا أدوات Gemini |
| `publish` | `issues:write` فقط | ملف الرد المؤقت؛ **لا يحمل `GEMINI_API_KEY`** ولا تشغيل Gemini |

**مهم:** هذه آلية ردود تلقائية **للتصميم والتقرير فقط**؛ ليست Agent مخولًا بالبرمجة، وليست Goldfish مستقلًا، ولا تمنح ChatGPT القدرة على سماع التعليقات دون تشغيل جلسة مراجعة.

## ما يلزم من المالك قبل التفعيل

1. **راجع [PR الخاص بالتنفيذ](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/pull/44)** وقائمة الملفات والفروق والاختبارات. لا تدمج قبل التحقق من عدم وجود أوامر غير موثوقة أو صلاحيات زائدة.
2. افتح مستودع GitHub ← `Settings` ← `Secrets and variables` ← `Actions` ← `New repository secret`، وأنشئ `GEMINI_API_KEY` من Google AI Studio. **لا تكتب قيمة المفتاح في Issue أو PR أو المحادثة.**
3. تحقق من تفعيل GitHub Actions للمستودع ومن السماح للتنفيذ بقراءة Issue ونشر تعليق بواسطة `GITHUB_TOKEN`. صلاحيات كل وظيفة مضبوطة داخل YAML؛ قد تمنع سياسة المؤسسة صلاحيات الكتابة رغم طلبها.
4. بعد نجاح مراجعة PR والأمان، والاطمئنان إلى وجود المفتاح، يصدر المالك إذنًا بتفعيل Workflow على `main`. **الدمج لا ينشر تطبيق المرتبات ولا يسمح بتعديل Oracle.**
5. في Issue #41، ومن حساب `AlexandriaDeveloper`، اكتب `/run-gemini AI-0005` كتعليق مستقل. راقب تبويب `Actions` ونتيجة التعليق الجديد `AUTO-...`. نجاح البوابة وحده ليس نجاح Gemini؛ يجب رؤية `STATUS: ANSWERED` مع جواب صالح ورابط Run.
6. لو ظهر `BLOCKED_CONFIG`: المفتاح غير متاح؛ أضفه أو راجع صلاحية السر وأعد الأمر **بتعليق جديد**. لو ظهر `ERROR`: راجع Run logs مع عدم نسخ أي أسرار.
7. لا تختبر بإدخال بيانات موظفين حقيقية أو ملفات Oracle خاصة؛ الرسالة `AI-0005` توثيق اصطناعي.

## سيناريوهات اختبار القبول

| السيناريو | النتيجة الصحيحة | حالة الدليل الحالي |
| --- | --- | --- |
| أمر صحيح من المالك في #41 | `accepted` بعد العثور على مصدر مصرح | اختبار وحدة معرف في `.github/scripts/bridge-policy.test.cjs`؛ **تشغيل GitHub Actions لم يُثبت بعد** |
| أمر من حساب آخر | `UNAUTHORIZED_OR_UNRELATED`، لا استخدام سر | اختبار وحدة |
| أمر على Issue أخرى أو PR | رفض وعدم تشغيل Gemini | اختبار وحدة |
| رد بوت أو تعليق غير مطابق | تجاهل، منع حلقة ردود | اختبار وحدة |
| رسالة المصدر غير موثقة أو غير موجهة إلى Gemini | لا تشغيل | اختبار وحدة |
| تكرار تعليق تشغيل سبق نشر نتيجته | تجاهل | اختبار وحدة |
| غياب `GEMINI_API_KEY` | `BLOCKED_CONFIG` دون تنفيذ Gemini | **يستلزم اختبار Run فعلي** |
| طلب صحيح وسر صحيح | رد `ANSWERED` في #41 مع رابط Run | **NOT_RUN حتى اختبار حقيقي** |
| حقن أوامر في التعليق | رفض التنسيق أو مرور النص كمدخل بيانات فقط بلا أدوات | اختبار وحدة لرفض النص الزائد؛ مراقبة السلوك الحقيقي لازمة |

## حدود الأمان والتنفيذ

- تعليق Issue **مدخل غير موثوق**؛ لذلك البوابة تستخدم صيغة أمر محددة ويفحص المصدر، ولا تدخل أي قيمة تعليق في سكربتات Bash أو JavaScript بوصفها كودًا. المحتوى الوحيد الذي يُعطى لـGemini هو نص المصدر من حساب مخول، دون أدوات تنفيذ.
- جلسة `Gemini CLI` تحت `tools.core: []` و`disableYoloMode`، ولا يصل إليها `GITHUB_TOKEN` ذي صلاحية كتابة. لا تستخدم `pull_request_target` أو `workflow_run` لتشغيل كود PR.
- في بيئة CI غير التفاعلية (headless)، تفرض أداة Gemini CLI التحقق من موثوقية المجلد؛ لذا يُمرر `--skip-trust` مع متغير `GEMINI_CLI_TRUST_WORKSPACE: "true"` لمنع الخروج برمز الخطأ 55 (`FatalUntrustedWorkspaceError`).
- عند حدوث أي خطأ في تشغيل Gemini أو في تحليل الرد، يتم تنقيح رسائل الخطأ من الأسرار ومفاتيح API وBearer tokens تلقائيًا قبل كتابتها إلى ملف النتيجة أو نشرها في تعليق Issue #41 أو سجلات Actions.
- يُعلن GitHub Actions حالة فشل حقيقية (Red / Failure) إذا انتهى التشغيل بغير `ANSWERED`، مع ضمان نشر تعليق الخطأ أولاً عبر وظيفة النشر المنفصلة.
- الـWorkflow يعتمد على `actions/checkout@v7` و`actions/github-script@v8` و`setup-node@v7` و`upload/download-artifact@v7` ونسخة `@google/gemini-cli@0.64.0`. **يلزم مراجعة تحديثات واعتماديات هذه الأدوات وفحص توفر إصدار CLI فعليًا ضمن اختبارات تفعيل PR**؛ المقترح لا يعني أن نسخها اختُبرت.
- رد Gemini لا يستطيع تعديل مصدر Markdown أو برنامج الموظفين؛ النشر في تعليق منفصل. أي طلب كود أو SQL أو ترحيل يظل بحاجة Issue معتمدة وإذن المالك وEGM عند انطباقه.
- GitHub لا يثبت استقلال Goldfish ولا يثبت قانونًا أو يغير صلاحية اعتماد المراجع.
- مصدر الاستخدام الرسمي: [Gemini CLI headless](https://geminicli.com/docs/cli/headless/) و[التحكم بالأدوات](https://geminicli.com/docs/reference/configuration/) و[GitHub Actions Security](https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions).

## قناة استكمال المراجعة

اربط عند ظهور أول تشغيل حقيقي: `Run URL` و`SHA` و`Comment ID` ونتيجة `PASS/FAIL/NOT_RUN` في [Issue #43](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/43)، ثم أبلغ ChatGPT عبر Issue #41. لا تنشئ أتمتة رد OpenAI في هذه المرحلة.
