# مواصفة دورة الملاحظة والرد والحسم ومتابعة الفروق المالية (`D-029 / P-017 / D-030`)

> **المرجع والتكليف:** [`Issue #59 — SPEC-029-001`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/59)، استنادًا إلى [قاعدة العمل الموحدة](../WORKING-BASELINE-AR.md)، [سجل القرارات](../governance/DECISION-REGISTER.md)، [مذكرة قرارات المراجعة](../domain/AUDIT-FINDINGS-AND-DECISIONS.md)، وقرار المالك المؤكد `D-030` في [`Issue #39`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/39).  
> **الحالة:** `PROPOSED_SPECIFICATION / SPEC_DOCS_ONLY / BUSINESS_DECISIONS_PARTLY_OPEN / NO_CODE / NO_DDL / NO_SQL / NO_MERGE`  
> **المرجع المستودعي (`main`):** `913963ef6c28ca5be243cfa96b50bc413a9582ac`

---

## 1. نطاق الشريحة الأولى وحدودها

### 1.1 ما تغطيه المواصفة
تحدد هذه المواصفة السلوك التشغيلي والتصوري لأول شريحة عمل في الوحدة الحسابية المركزية للمجمع الطبي (`30200105`):
1. فتح الملاحظة المالية (`Finding`) ضمن `AuditCase` مرتبط بـ: مستفيد/علاقة صرف (`PayeeRelationshipId` / `EmploymentRelationshipId`)، ودورة استحقاق (`PayrollCycle` / `SalaryEntitlementMonth`)، وصرفية محددة (`PayrollDisbursement` — رئيسية ضمن `MonthlyMainSalarySlot` + `MainSalaryRevision` أو منفصلة `SEPARATE`)، ونسخة مصدر محددة (`SourceVersion`).
2. إرسال الملاحظة أو إبلاغ الاستحقاقات المركزية بها ومتابعة حالتها.
3. استقبال **قناتَي رد المركزية المؤكدتين بقرار المالك `D-030`** في [`Issue #39`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/39):
   - **رد مركزي داخلي (`CentralInternalReply`):** يقدمه حساب مركزي مُصرَّح له داخل النظام بعد اعتماد صلاحياته.
   - **رد مركزي خارجي مسجَّل (`RecordedExternalReply`):** يسجله مستخدم الوحدة المخوّل نيابةً عن استلام رد رسمي/خارجي، مع فصل هوية المسجِّل (`RecordedBy`) عن صاحب الرد المنسوب (`AttributedRespondent`) وإرفاق دليل الإثبات.
4. حسم صحة الملاحظة (`ReviewDecision`) ضمن إحدى النتائج الخمس في [قاعدة العمل §3](../WORKING-BASELINE-AR.md#L20-L32): تصحيح رسمي وارد من المركزية وإعادة فحص نسخة المصدر؛ أو ثبوت سلامة الملف وخطأ المراجع مع **سحب الملاحظة (`Withdrawn`)**؛ أو اتفاق على **تأجيل فرق صحيح**؛ أو **تسوية كلية/جزئية مثبتة**؛ أو استبقاء **خلاف/نقص مستند** مفتوحًا؛ مع دعم إعادة الفتح بإنشاء نسخة جديدة تحفظ السابقة.
5. **فصل حسم الملاحظة عن متابعة الفرق المالي (`FollowUp`):** عند ثبوت فرق حقيقي مؤجل أو قابل للمتابعة، تُفتح له دورة متابعة مستقلة تراقب ظهوره في كشوف لاحقة (`REPORTED_DEDUCTED` / `Reporting of Deduction`) وتتحقق من تسويته الفعلية بدليل مستقل (`SETTLED_VERIFIED` / `Verified Settlement`) حتى إغلاق المتابعة المالية (`FinancialFollowUpClosed`).

### 1.2 ما يخرج عن نطاق هذه المواصفة
- **ليس نظام صرف أو `ERP` أو مقاصة بنكية:** لا يوقف البرنامج صرفًا في البنك، ولا يرسل استقطاعات إلى Oracle، ولا يعدّل ملفات الموظفين أو كشوف المصدر الأصلية، ولا يدفع أموالًا.
- **البيئة والتخزين مؤجلان:** لا تحدد هذه الوثيقة جداول `SQL Server 2014` أو `ORM` أو `DbContext` أو كود C#/Angular، وتلتزم بترتيب [`Issue #56`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/56) وتأجيل `Phase B`.
- **مسائل حدودية تحتاج قرار مالك أو مراحل لاحقة:** حسم أثر تعليق صرفية `SEPARATE` على اعتماد `AuditCase` للمرتب الرئيسي، وضم استمارة `132 ع.ح` مع تسوية رعاية الطفل، ونقل الموظف بين وحدتين حسابيتين مختلفتين، والقوانين غير الموثقة السند.

---

## 2. قاموس المفاهيم والأدوار التصورية

> **تنبيه تعاقدي:** الأسماء أدناه مفاهيم مجال/تطبيق مرشحة تصوريًا وليست فئات C# منشأة ولا أسماء جداول قاعدة بيانات معتمدة.

### 2.1 المفاهيم الرئيسية
| المفهوم المرشح | الدور المالي والرقابي | ما يُمنع عليه |
| :--- | :--- | :--- |
| `AuditCase` | حاوية مراجعة مستفيد/صرفية في دورة ونسخة مصدر محددة، تجمع الملاحظات وتُصدر حالة فحص المستفيد دون أن تتطابق تلقائيًا مع حالة كل ملاحظة أو الدفع. | محو نسخ سابقة أو فرض `Approved` مع وجود نقص جوهري مانع. |
| `SourceVersion` | إشارة ثابتة (`Immutable`) إلى مستند المصدر ونسخته (`SourceDocument`، `SourceCaptureSession`، كشف رسمي مصحح، أو إدخال بديل Excel/شاشة) وبصمته/قناته. | التعديل المباشر في مكان السجل عند وصول تصحيح رسمي. |
| `Finding` | سجل ملاحظة مالية على بند أو مقارنة مجمّعة أو هوية/وعاء ضمن `AuditCase`، له معرف مستقل ونوع خطورة وقيمة مقارنة مرجعية. | أن ينشئ تلقائيًا مديونية أو استقطاعًا في Oracle. |
| `FindingRevision` | لقطة إصدارية مرقّمة للملاحظة تُحفظ عند فتح الملاحظة أو تعديل نطاقها أو **إعادة فتحها (`Reopened`)**، مع ربطها بالنسخة السابقة وسبب الفتح. | استبدال أو مسح `FindingRevision` السابقة. |
| `CentralResponse` | رد من الاستحقاقات المركزية مرتبط بإصدار ملاحظة محدد (`FindingRevisionId`) عبر إحدى القناتين المؤكدتين بـ `D-030`: `CentralInternalReply` أو `RecordedExternalReply`، مع الفصل بين دليل نسبة/استلام الرد (`ReplyAttributionEvidenceRef`) والدليل الموضوعي للاستحقاق (`SubstantiveEntitlementEvidenceRef`). | القبول الآلي للرد، أو الخلط بين إثبات استلام الرد وبين صحة الاستحقاق المالي أو دليل التوريد البنكي. |
| `ReviewDecision` | قرار الحسم الرقابي المسبب على الملاحظة (إعادة فحص نسخة مصححة، سحب الملاحظة، تأجيل فرق، اعتماد استثناء موثق، أو استبقاء خلاف/نقص دليل) مع المعتمد والوقت والأدلة. | تغيير `SourceAmount` أو `ExpectedAmount` التاريخي أو إغلاق `FollowUp` المالي ضمنيًا. |
| `FollowUp` | سجل متابعة مستقل لفرق مالي ثابت السند ومُرحّل/مؤجّل (`FollowUpObligation` / `FollowUpAllocation` كوصف متابعة غير تنفيذي)، يحفظ أصل الفرق والمخصصات والمتبقي ولا يُلغى إلا بسند رسمي يبطل أصل الفرق. | تنفيذ مقاصة مالية تلقائية، أو إنشاؤه من مجرد فرق مصدر غير مثبت، أو إنشاؤه لملاحظة مسحوبة (`Withdrawn`)، أو إلغاؤه لمجرد رفض ربط بند آخر غير متوافق. |
| `Evidence` | حزمة الإثبات المرجعية (`EvidenceRef`) المميِّزة بين: دليل نسبة/استلام الرد (`ReplyAttributionEvidenceRef`)، والدليل الموضوعي للاستحقاق (`SubstantiveEntitlementEvidenceRef`)، ودليل التسوية المستقل (`SettlementEvidenceRef`) مع بيانات الأصالة ودقة النقل. | ادعاء مستند Oracle رسمي لمدخل بديل، أو الاكتفاء بدليل الاستلام لإثبات الاستحقاق الموضوعي، أو قبول غلاف مجمّع بدون نسبة صريحة كإثبات دفع. |

### 2.2 الأدوار والصلاحيات (`Actor / RBAC`)
| الدور التصوري (`Actor`) | المسؤوليات المسموحة تصوريًا | قيود الحوكمة وحالة الاعتماد |
| :--- | :--- | :--- |
| `UnitReviewer` | فتح `Finding` داخل مجموعته/نطاقه، إرسال طلب توضيح/تصحيح، تسجيل `RecordedExternalReply` مع تعيين `RecordedBy` (هويته) و`AttributedRespondent` (مصدر الرد المركزي المنسوب) ودليله، واقتراح الحسم أو طلب إعادة الفتح. | لا يعتمد قاعدة مالية عدّلها بمفرده؛ صلاحيته على الحسم النهائي والسحب في انتظار الحسم (`OWNER_DECISION_REQUIRED`). |
| `CentralAccountResponder` | إدخال `CentralInternalReply` وإرفاق مستندات أو إشعار تصحيح مركزي على الملاحظات المرسلة لجهته (`D-030`). | وجود القناة مؤكد (`OWNER_CONFIRMED` بـ `#39`)؛ **منح الحساب وصلاحياته ونطاق رؤيته ومهل رده: `OWNER_DECISION_REQUIRED`** (ولا يُعد حسابًا مُنشأً الآن). |
| `AuthorizingReviewer / UnitSupervisor` | مراجعة واعتماد سحب الملاحظة (`Withdrawn`)، إقرار الاستثناء الموثق، الموافقة على تأجيل فرق، واعتماد إعادة فتح نسخة سابقة أو إغلاق المتابعة المالية `FinancialFollowUpClosed`. | فصل المهام (`Separation of Duties`): نطاق الصلاحيات الدقيقة والحدود المالية لكل مستوى: **`OWNER_DECISION_REQUIRED`**. |
| `SourceIntakeOperator` | رفع واستيراد نسخ المصادر والتصحيحات الرسمية دون تعديل قرارات المراجعة أو منح اعتماد مالي لنفسه. | لا يغلق ملاحظة ولا يعتمد صرفية أو متابعة مالية. |

---

## 3. آلتا الحالات المنفصلتان وجداول الانتقالات

**مبدأ الفصل الإلزامي ([مذكرة المراجعة §2](../domain/AUDIT-FINDINGS-AND-DECISIONS.md#L30)):**  
`حالة الملاحظة` ≠ `حالة مراجعة المستفيد (AuditCase)` ≠ `حالة اعتماد الصرفية` ≠ `حالة إصدار المستند المركزي` ≠ `حالة متابعة الفرق وإثبات الدفع الفعلي`.

### 3.1 الآلة (أ): دورة الملاحظة والرد والحسم وإعادة الفتح (`FindingStateMachine`)

```mermaid
stateDiagram-v2
  [*] --> DraftDetected: "رصد فرق أو نقص"
  DraftDetected --> UnderReview: "بدء فحص الملاحظة"
  UnderReview --> WaitingEvidence: "نقص مستند أو دليل محلي"
  WaitingEvidence --> UnderReview: "استيفاء الدليل"
  UnderReview --> SentToCentral: "إبلاغ/إرسال للمركزية"
  SentToCentral --> ResponseReceived: "CentralInternalReply أو RecordedExternalReply موثق النسبة (D-030)"
  SentToCentral --> CorrectionReceivedPendingReAudit: "ورود نسخة مصدر مصححة رسميًا"
  ResponseReceived --> UnderReview: "اكتمال السند الموضوعي للفحص"
  ResponseReceived --> WaitingEvidence: "ثبوت استلام الرد مع نقص الدليل الموضوعي للاستحقاق"
  ResponseReceived --> Disputed: "خلاف موضوعي على القاعدة أو السند"
  Disputed --> SentToCentral: "طلب تصعيد/توضيح إضافي"
  Disputed --> CorrectionReceivedPendingReAudit: "وصول تصحيح رسمي لاحق"
  CorrectionReceivedPendingReAudit --> UnderReview: "إعادة فحص النسخة الجديدة"
  UnderReview --> ResolvedWithCorrection: "قبول النسخة المصححة"
  UnderReview --> Withdrawn: "ثبوت سلامة الملف وسحب الملاحظة"
  UnderReview --> DeferredDifferenceAccepted: "حسم فرق صحيح مع فتح متابعة منفصلة"
  UnderReview --> ExceptionAccepted: "اعتماد استثناء توزيع موثق"
  ResolvedWithCorrection --> Reopened: "حدث/مستند جديد ← FindingRevision جديدة"
  Withdrawn --> Reopened: "دليل ناقض جديد ← FindingRevision جديدة"
  DeferredDifferenceAccepted --> Reopened: "تصحيح/حدث مغيّر للأصل ← FindingRevision جديدة"
  ExceptionAccepted --> Reopened: "مستند جديد يغير الاستثناء ← FindingRevision جديدة"
  Reopened --> UnderReview: "استئناف الفحص على النسخة الجديدة"
```

#### جدول انتقالات الآلة (أ) — الملاحظة والرد والحسم

| From | Event | Guard | Actor | To | Evidence | Failure |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `[*]` | `CreateFinding` | `AuditCase` غير مغلق، بند/موضع محدد، `IdempotencyKey` وفق قاعدة النطاق والحمولة* | `UnitReviewer` | `DraftDetected` | ربط `SourceVersion` ونتيجة مقارنة (`Expected/Source`) | `IdempotencyConflict` / `PermissionDenied` |
| `DraftDetected` | `StartFindingReview` | `ExpectedVersion` مطابق | `UnitReviewer` | `UnderReview` | تسجيل نسخة `FindingRevision = 1` | `VersionConflict` |
| `UnderReview` | `RequestEvidence` | تحديد المستند الموضوعي الناقص صراحةً | `UnitReviewer` | `WaitingEvidence` | وصف الدليل الموضوعي المطلوب | `EvidenceRequired` / `InvalidTransition` |
| `WaitingEvidence` | `AttachEvidence` | إرفاق دليل موضوعي صالح (`SubstantiveEntitlementEvidenceRef`) | `UnitReviewer` | `UnderReview` | `SubstantiveEntitlementEvidenceRef` وبصمة/تاريخ | `EvidenceRequired` |
| `UnderReview` | `SendToCentral` | توصيف الفجوة مسببًا، تحديد جهة/قناة الإبلاغ | `UnitReviewer` | `SentToCentral` | مرجع الإبلاغ والإصدار | `InvalidTransition` / `VersionConflict` |
| `SentToCentral` | `SubmitCentralInternalReply` | `D-030`: حساب مركزي مصرح له + نص رد + ربط `FindingRevisionId` + `IdempotencyKey` وفق قاعدة البصمة* | `CentralAccountResponder` | `ResponseReceived` | `CentralInternalReply` + `SubstantiveEntitlementEvidenceRef` إن وجد | `PermissionDenied` / `IdempotencyConflict` |
| `SentToCentral` | `RecordCentralExternalReply` | `D-030`: فصل `RecordedBy` عن `AttributedRespondent` + توفر **`ReplyAttributionEvidenceRef`** (دليل استلام/نسبة الرد)، و`IdempotencyKey` وفق قاعدة البصمة* (إن غاب الإسناد أو دليل الاستلام يُرفض التسجيل وتبقى الحالة `SentToCentral`) | `UnitReviewer` (المخول) | `ResponseReceived` | `RecordedExternalReply` + `ReplyAttributionEvidenceRef` (+ `SubstantiveEntitlementEvidenceRef?`) | `EvidenceRequired` / `PermissionDenied` / `IdempotencyConflict` |
| `ResponseReceived` | `EvaluateCentralReply` | فحص المراجع لمحتوى الرد والتمييز بين ثبوت استلامه وكفاية دليله الموضوعي: (1) دليل موضوعي مكتمل ← `UnderReview`؛ (2) ثبوت الاستلام مع نقص `SubstantiveEntitlementEvidenceRef` وتحديد المطلوب ← `WaitingEvidence`؛ (3) خلاف على القاعدة/الدرجة ← `Disputed` | `UnitReviewer` | `UnderReview` أو `WaitingEvidence` أو `Disputed` | محضر تقييم الرد (`Rationale`) + توصيف النقص الموضوعي عند الانتقال إلى `WaitingEvidence` | `InvalidTransition` / `VersionConflict` / `EvidenceRequired` |
| `SentToCentral` / `Disputed` | `LinkOfficialCorrectionSource` | وصول `SourceVersion` جديدة مصححة رسميًا ومطابقة للهوية والفترة | `SourceIntakeOperator` / `UnitReviewer` | `CorrectionReceivedPendingReAudit` | `SourceVersionId` الجديد (`OfficialCorrectionAmount`) | `EvidenceRequired` / `InvalidTransition` |
| `CorrectionReceivedPendingReAudit` | `ReauditCorrectedSource` | إتمام إعادة المقارنة على النسخة الجديدة دون محو الأصل | `UnitReviewer` | `UnderReview` | لقطة مقارنة قبل/بعد | `VersionConflict` |
| `UnderReview` | `ResolveWithCorrection` | تطابق النسخة المصححة مع القرار، واستيفاء صلاحية الحسم (`OWNER_DECISION_REQUIRED`) | `AuthorizingReviewer` / `UnitReviewer`** | `ResolvedWithCorrection` | مرجع النسخة المصححة + قرار الحسم | `PermissionDenied` / `EvidenceRequired` / `IdempotencyConflict` |
| `UnderReview` | `WithdrawFinding` | إثبات سلامة الملف الأصلي وخطأ الملاحظة بدليل موضوعي (`SubstantiveEntitlementEvidenceRef`) + سبب مكتوب + صلاحية سحب | `AuthorizingReviewer` (`OWNER_DECISION_REQUIRED`) | `Withdrawn` | `SubstantiveEntitlementEvidenceRef` + سبب السحب + هوية المعتمد (لا ينشأ `FollowUp`) | `EvidenceRequired` / `PermissionDenied` / `IdempotencyConflict` |
| `UnderReview` | `ApproveDeferredDifference` | ثبوت أصل الفرق وسند التأجيل والجهة/الشهر المستهدف | `AuthorizingReviewer` (`OWNER_DECISION_REQUIRED`) | `DeferredDifferenceAccepted` | سند التأجيل (يحفّز فتح سجل `FollowUp` منفصل) | `EvidenceRequired` / `PermissionDenied` / `IdempotencyConflict` |
| `UnderReview` | `AcceptDocumentedException` | في نفس باب الصرف وشروط التجميع المعتمدة وبسند استثناء | `AuthorizingReviewer` (`OWNER_DECISION_REQUIRED`) | `ExceptionAccepted` | سند الاستثناء (لا يصبح «متطابقًا») | `PermissionDenied` / `InvalidTransition` / `IdempotencyConflict` |
| `Resolved*` / `Withdrawn` / `Deferred*` / `Exception*` | `ReopenFinding` | وجود مستند جديد أو تصحيح أو حدث بعد الاعتماد + سبب وإصدار جديد | `AuthorizingReviewer` (`OWNER_DECISION_REQUIRED`) | `Reopened` ← `UnderReview` | إنشاء `FindingRevision + 1` وحفظ الاعتماد السابق كاملًا | `EvidenceRequired` / `PermissionDenied` / `VersionConflict` / `IdempotencyConflict` |

*\* قاعدة الـ`Idempotency` القياسية لجميع أوامر الآلتين: نفس `(OperationType, TargetAggregateId, ActorScope, IdempotencyKey)` مع **نفس بصمة الطلب** يُعيد النتيجة الأصلية دون أثر ثانٍ؛ أما نفس المفتاح مع **حمولة مختلفة** فيُرفض فورًا بـ`IdempotencyConflict` دون تغيير الحالة (انظر القسم 4 والقسم 5.1).*  
*\*\* يُحدَّد نهائيًا في قرار المالك هل يحق للمراجع منفردًا ختم `ResolvedWithCorrection` أم يتطلب اعتمادًا أعلى (انظر القسم 7).*

### 3.2 الآلة (ب): دورة متابعة الفرق والتأجيل والتسوية والإغلاق (`FinancialFollowUpStateMachine`)

> **حاجز التفعيل الحتمي:** لا يُنشأ سجل `FollowUp` عند سحب الملاحظة (`Withdrawn`) ولا عند مجرد فرق مصدر غير مثبت؛ بل يُنشأ فقط عندما يُثبت قرار الحسم فرقًا حقيقيًا مستحق المتابعة (`DeferredDifferenceAccepted` أو فرق تسوية معتمد السند)، وتبقى `Reporting of Deduction` منفصلة تمامًا عن `Verified Settlement`، ولا يتحقق `FinancialFollowUpClosed` إلا بدليل تسوية مستقل وصلاحية إغلاق.

```mermaid
stateDiagram-v2
  [*] --> DeferredScheduled: "إنشاء متابعة لفرق مثبت/مؤجل (Outstanding = الأصل)"
  DeferredScheduled --> MonitoringOracleAppearance: "بدء متابعة الشهور اللاحقة"
  MonitoringOracleAppearance --> ReportedInDisbursement: "ظهور مبلغ في كشف صرف/استقطاع (REPORTED_DEDUCTED)"
  MonitoringOracleAppearance --> PartiallyVerifiedSettled: "إثبات تسوية جزئية بدليل مستقل مباشرة"
  MonitoringOracleAppearance --> VerifiedSettledPendingClose: "إثبات تسوية كاملة بدليل مستقل مباشرة (Outstanding = 0)"
  ReportedInDisbursement --> PartiallyVerifiedSettled: "إثبات تسوية/توريد لجزء فقط (VerifiedPaid < الأصل)"
  ReportedInDisbursement --> VerifiedSettledPendingClose: "إثبات تسوية كاملة (Outstanding = 0)"
  PartiallyVerifiedSettled --> ReportedInDisbursement: "ظهور دفعة إضافية بالكشف"
  PartiallyVerifiedSettled --> VerifiedSettledPendingClose: "إثبات تسوية الجزء المتبقي (Outstanding = 0)"
  MonitoringOracleAppearance --> MonitoringOracleAppearance: "رفض طلب ربط غير متوافق (تبقى المتابعة ورصيدها / UNALLOCATED)"
  ReportedInDisbursement --> ReportedInDisbursement: "رفض طلب ربط غير متوافق (تبقى المتابعة ورصيدها / UNALLOCATED)"
  VerifiedSettledPendingClose --> FinancialFollowUpClosed: "إغلاق المتابعة المالية بصلاحية وسند"
  DeferredScheduled --> FollowUpDisputed: "تنازع موثق على أصل الفرق أو نطاقه"
  MonitoringOracleAppearance --> FollowUpDisputed: "تنازع موثق على أصل الفرق أو نطاقه"
  ReportedInDisbursement --> FollowUpDisputed: "تنازع موثق على أصل الفرق أو نطاقه"
  PartiallyVerifiedSettled --> FollowUpDisputed: "تنازع موثق على أصل الفرق المتبقي"
  FollowUpDisputed --> MonitoringOracleAppearance: "حسم النزاع ببقاء الفرق واستئناف المتابعة"
  DeferredScheduled --> FollowUpCancelledByCorrection: "سند رسمي/تصحيح مركزي يلغي أصل الفرق فقط"
  MonitoringOracleAppearance --> FollowUpCancelledByCorrection: "سند رسمي/تصحيح مركزي يلغي أصل الفرق فقط"
  ReportedInDisbursement --> FollowUpCancelledByCorrection: "سند رسمي/تصحيح مركزي يلغي أصل الفرق فقط"
  FollowUpDisputed --> FollowUpCancelledByCorrection: "ثبوت إلغاء أصل الفرق بسند رسمي"
```

#### جدول انتقالات الآلة (ب) — متابعة الفرق والتسوية المالية

| From | Event | Guard | Actor | To | Evidence | Failure |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `[*]` | `OpenFollowUpForApprovedDifference` | وجود `FindingRevision` محسوم بفرق مثبت أو تأجيل معتمد (محظور مع `Withdrawn` أو `CENTRAL_SOURCE_AMOUNT` بلا التزام ثابت) | `AuthorizingReviewer` / `Application` (بعد قرار الحسم) | `DeferredScheduled` | أصل الفرق + الشهر الأصلي والمستهدف + السند | `InvalidTransition` / `EvidenceRequired` / `IdempotencyConflict` |
| `DeferredScheduled` | `ActivateMonitoring` | تهيئة رصيد متابعة: `VerifiedPaid = 0`, `Outstanding = OriginalAmount` | `UnitReviewer` | `MonitoringOracleAppearance` | مرجع جدول المتابعة الدوري | `VersionConflict` / `IdempotencyConflict` |
| `MonitoringOracleAppearance` / `PartiallyVerifiedSettled` | `RecordReportedAppearance` | ربط `SourceFinancialOccurrence` ظهر في كشف لاحق؛ وإذا اختلف كود البند: تحقق `SettlementCompatibilityPolicy` + `CrossComponentAuthRef`، وعدم تجاوز الرصيد المتاح | `UnitReviewer` | `ReportedInDisbursement` (`REPORTED_DEDUCTED`) | `SourceFinancialOccurrenceId` + تحديث `ReportedDisbursementAmount` فقط (**لا يغلق المتابعة ولا يرفع `VerifiedPaidAmount`**) | `IncompatibleFollowUpAllocation` / `ExceededRemainingBalance` / `IdempotencyConflict` |
| `MonitoringOracleAppearance` / `ReportedInDisbursement` / `PartiallyVerifiedSettled` | `RejectIncompatibleAllocationAttempt` *(رفض طلب ربط غير متوافق)* | محاولة إسناد بند آخر لا يستوفي `SettlementCompatibilityPolicy` أو يفتقر إلى `CrossComponentAuthRef` | `Application` / `UnitReviewer` | **نفس الحالة دون تغيير** *(لا ينتقل `FollowUp` إلى نزاع أو إلغاء ويبقى `OutstandingAmount` ثابتًا)* | تسجيل محاولة الربط المرفوضة بحالة `UNALLOCATED / NOT_AUTHORIZED` مع طلب الإثبات | `IncompatibleFollowUpAllocation` / `EvidenceRequired` |
| `ReportedInDisbursement` / `MonitoringOracleAppearance` | `VerifyPartialSettlement` | دليل توريد/تسوية مستقل (`SettlementEvidenceRef`) يثبت تنفيذ جزء (`0 < Sum(VerifiedPaid) < OriginalAmount` و`Amount <= OutstandingAmount`) | `UnitReviewer` / `AuthorizingReviewer` | `PartiallyVerifiedSettled` (`SETTLED_VERIFIED` جزئي) | `SettlementEvidenceRef` (`DeductionSettlementEvidence`) + بقاء `OutstandingAmount > 0` | `EvidenceRequired` / `ExceededRemainingBalance` / `VersionConflict` / `IdempotencyConflict` |
| `MonitoringOracleAppearance` / `ReportedInDisbursement` / `PartiallyVerifiedSettled` | `VerifyFullSettlement` | أصل الفرق مثبت ومفعّل، ودليل تسوية مستقل صالح (`SettlementEvidenceRef`) يغطي كامل الرصيد المتبقي (`Amount == OutstandingAmount`، أي `Sum(VerifiedPaid) == OriginalAmount` ← `OutstandingAmount = 0`)، مع استيفاء حواجز التحقق والتوافق؛ **ولا يُشترط سبق ظهور في كشف Oracle** (ولا يفترض موافقة قانونية أو تنفيذ إجراء مالي آلي) | `UnitReviewer` / `AuthorizingReviewer` | `VerifiedSettledPendingClose` | حزمة `SettlementEvidenceRef` مكتملة + `OutstandingAmount = 0` (وقد يبقى `ReportedDisbursementAmount = 0` عند التسوية المستقلة المباشرة) | `EvidenceRequired` / `ExceededRemainingBalance` / `VersionConflict` / `IdempotencyConflict` |
| `VerifiedSettledPendingClose` | `CloseFinancialFollowUp` | `OutstandingAmount == 0` + عدم وجود مراجعة تصحيح مفتوحة + صلاحية الإغلاق المعتمدة (`OWNER_DECISION_REQUIRED`) | `AuthorizingReviewer` (`OWNER_DECISION_REQUIRED`) | `FinancialFollowUpClosed` | اعتماد الإغلاق المالي وهوية المعتمد والتوقيت | `PermissionDenied` / `InvalidTransition` / `EvidenceRequired` / `IdempotencyConflict` |
| `DeferredScheduled` / `MonitoringOracleAppearance` / `ReportedInDisbursement` / `PartiallyVerifiedSettled` | `DisputeFollowUpOrigin` | وجود سند/اعتراض موثق يطعن في أصل الفرق نفسه أو نطاق متابعته (وليس مجرد رفض ربط بند آخر) | `AuthorizingReviewer` / `UnitReviewer` | `FollowUpDisputed` | محضر النزاع ومرجع الاعتراض مع حفظ الأرصدة الحالية | `EvidenceRequired` / `PermissionDenied` / `VersionConflict` / `IdempotencyConflict` |
| `FollowUpDisputed` | `ResolveFollowUpDispute` | حسم النزاع ببقاء أصل الفرق مستحق المتابعة | `AuthorizingReviewer` (`OWNER_DECISION_REQUIRED`) | `MonitoringOracleAppearance` | سند حسم النزاع واستئناف الرصيد المتبقي | `EvidenceRequired` / `PermissionDenied` / `VersionConflict` |
| `DeferredScheduled` / `MonitoringOracleAppearance` / `ReportedInDisbursement` / `FollowUpDisputed` | `CancelFollowUpByOfficialCorrection` | حصرًا عند وصول **تصحيح مركزي رسمي أو قرار إعادة فتح/سحب** يُلغي أصل الفرق ذاته | `AuthorizingReviewer` (`OWNER_DECISION_REQUIRED`) | `FollowUpCancelledByCorrection` | مستند الإلغاء/التصحيح الرسمي لأصل الفرق (+ فتح نسخة متابعة جديدة عند إعادة التقدير بمبلغ مختلف) | `EvidenceRequired` / `PermissionDenied` / `IdempotencyConflict` |

---

## 4. القواعد الثابتة غير قابلة للتجاوز (`Invariants`)

1. **عدم قابلية تغيير الأصل (`Immutability`):** سطور المصدر (`SourceFinancialOccurrence`)، وملفاته (`SourceDocument`)، ونسخ الملاحظات والقرارات المعتمدة السابقة (`FindingRevision`, `ReviewDecision`) لا تُعدّل أو تُمحى عند التعديل أو إعادة الفتح أو الاستيراد التصحيحي.
2. **فصل القيم السبع إلزاميًا:** لا يجوز دمج أو اختصار أي من الحقول المستقلة: `SourceAmount`، `ExpectedAmount`، `ReviewedAmount`، `OfficialCorrectionAmount`، `ReportedDisbursementAmount`، `VerifiedPaidAmount`، و`OutstandingAmount`.
3. **منع التسوية بتطابق المبلغ أو باب الصرف وحدهما (وحفظ المتابعة عند الرفض):** وجود 200 جنيه ببند آخر في شهر لاحق داخل نفس باب الصرف **لا يكفي** لربطها بفرق 200 جنيه أصلي؛ يشترط تحقق `SettlementCompatibilityPolicy` وسند رسمي ينسب المبلغ للالتزام الأصلي. وعند رفض الربط يبقى التخصيص المرفوض بحالة `UNALLOCATED / NOT_AUTHORIZED` **وتستمر `FollowUp` الأصلية في حالتها ورصيدها (`OutstandingAmount`) دون إلغاء أو تعطيل**.
4. **منع `UNKNOWN = 0` والاحتساب التقديري:** نقص مستند أو غياب قاعدة/درجة يؤدي لحفظ `UNKNOWN / INPUT_MISSING` على البند المتأثر فقط دون تصفيره أو اختراع مبلغ تقديري.
5. **التمييز الصارم بين استلام الرد وصحة الاستحقاق والظهور والتسوية المستقلة:** يُفصل دليل نسبة/استلام الرد الخارجي (`ReplyAttributionEvidenceRef`) عن الدليل الموضوعي للاستحقاق (`SubstantiveEntitlementEvidenceRef`) وعن دليل التسوية الفعلية (`SettlementEvidenceRef`)؛ فلا يُعتبر رد المركزية ولا طباعة المبلغ بكشف الصرف/الاستقطاع دليلًا على التسوية الفعلية، كما يمكن إثبات التسوية الجزئية أو الكاملة مباشرة بدليل مستقل صالح حتى لو لم يسبق ظهور المبلغ في كشف Oracle.
6. **عدم التنفيذ المالي الآلي:** لا ينفذ البرنامج خصمًا في Oracle ولا يصدر إيقافًا بنكيًا ولا يحوّل فروقًا تلقائيًا إلى مديونية قانونية واجبة النفاذ.
7. **حماية التزامن والـ`Idempotency` الحتمية (`Concurrency & Idempotency`):** كل تعديل حالة أو تخصيص متابعة يفرض `ExpectedVersion`، ويمنع تجاوز مجموع التخصيصات لأصل الفرق، ويعالج تعارض التزامن على مستوى `Finding` أو `FollowUp` حتى لا يصطدم مراجعان يعملان على بنود مختلفة بلا داعٍ (تصوريًا وفق `MIN-04`). ويُقيَّد `IdempotencyKey` بنطاق `(OperationType, TargetAggregateId, ActorScope)` مع بصمة الحمولة المعيارية (`RequestPayloadHash`) وفق قاعدتين حتميتين: (أ) **نفس المفتاح والنطاق + نفس بصمة الطلب** = استعادة نتيجة العملية الأصلية (`Idempotent Replay`) دون أثر ثانٍ أو تعديل رصيد؛ (ب) **نفس المفتاح والنطاق + حمولة مختلفة** = رفض فوري بخطأ `IdempotencyConflict` دون تغيير أي حالة أو رصيد.

---

## 5. حدود عقود `Core / Application` التصورية (دون كود أو DDL)

تتسق العقود المقترحة مع `ADR-003` و`ADR-004` و`ADR-005` بحيث يظل **`Core`** نقيًا وخاليًا تمامًا من `SQL / ORM / PDFium / HTTP`:
- **`Core` (Domain Layer):** يمتلك الـ`Aggregates` (مثل `Finding` + `FindingRevision`, `FollowUp`), والـ`Value Objects` (مثل `MonetaryAmount` بـ`decimal`, `AuditActorsAttribution`, `DifferenceBalances`), وآلتَي الحالات (`FindingStateMachine`, `FinancialFollowUpStateMachine`) والحواجز النقية (`Guards`) والأحداث النطاقية (`DomainEvents`).
- **`Application` (Use Case Orchestration):** ينسق استرجاع الكيانات والتحقق من `ActorId` عبر `IAuthorizationPolicyPort`، والتأكد من `IdempotencyKey` عبر `IIdempotencyPort`، والتحقق من وجود الأدلة والمصادر عبر `IEvidenceRegistryPort` / `ISourceReadPort`، وحفظ التعديلات في معاملة منضبطة عبر `IFindingRepository` / `IFollowUpRepository` / `IUnitOfWorkPort`.
- **`Infrastructure` (مؤجلة):** محولات قواعد البيانات (`SQL Server 2014` لاحقًا بعد `Issue #30` وتفويض المالك) وقراءة PDF/Excel.

### 5.1 أمثلة عقود تصورية (`Commands / Queries / Ports`)

| النوع / الاسم المقترح | مدخلات العقد التصورية | المخرجات المتوقعة (`Result`) | الأحداث / الآثار (`Domain Events`) | حالات الخطأ القياسية |
| :--- | :--- | :--- | :--- | :--- |
| `Command: RegisterAuditFinding` | `AuditCaseId`, `PayrollDisbursementId`, `SourceVersionId`, `ComponentRef`, `DiscrepancyKind`, `ExpectedAmount?`, `SourceAmount?`, `Rationale`, `ActorId`, `IdempotencyKey` | `Result<FindingRevisionRef, ContractError>` | `FindingRegisteredEvent` | `PermissionDenied`, `IdempotencyConflict`, `EvidenceRequired` |
| `Command: RecordCentralResponse` | `FindingId`, `FindingRevisionId`, `ExpectedVersion`, `Channel` (`CentralInternalReply` \| `RecordedExternalReply`), `ActorId` (`RecordedBy`), `AttributedRespondent` (إلزامي للخارجي), `SummaryText`, `ReplyAttributionEvidenceRef` (إلزامي لإثبات استلام/نسبة الخارجي), `SubstantiveEntitlementEvidenceRef?`, `OccurredAt`, `IdempotencyKey` | `Result<CentralResponseRef, ContractError>` | `CentralResponseRecordedEvent` | `PermissionDenied`, `VersionConflict`, `EvidenceRequired`, `InvalidTransition`, `IdempotencyConflict` |
| `Command: EvaluateCentralReply` | `FindingId`, `FindingRevisionId`, `ExpectedVersion`, `Outcome` (`ProceedToReview` \| `RequestSubstantiveEvidence` \| `MarkDisputed`), `Rationale`, `MissingEvidenceSpec?`, `ActorId`, `IdempotencyKey` | `Result<FindingRevisionRef, ContractError>` | `CentralReplyEvaluatedEvent` | `PermissionDenied`, `VersionConflict`, `EvidenceRequired`, `InvalidTransition`, `IdempotencyConflict` |
| `Command: ResolveFinding` | `FindingId`, `FindingRevisionId`, `ExpectedVersion`, `ResolutionKind` (`WithCorrection` \| `WithdrawFinding` \| `DeferDifference` \| `AcceptException`), `Rationale`, `SubstantiveEntitlementEvidenceRef`, `OfficialCorrectionSourceVersionId?`, `FollowUpTargetPeriod?`, `ActorId`, `IdempotencyKey` | `Result<ReviewDecisionRef, ContractError>` | `FindingResolvedEvent` (+ `FollowUpRequiredEvent` عند `DeferDifference` فقط) | `PermissionDenied`, `VersionConflict`, `EvidenceRequired`, `InvalidTransition`, `IdempotencyConflict` |
| `Command: ReopenFinding` | `FindingId`, `ExpectedVersion`, `ReopenReason`, `TriggeringEvidenceRef`, `ActorId`, `IdempotencyKey` | `Result<FindingRevisionRef, ContractError>` (نسخة جديدة تزيد بـ`+1`) | `FindingReopenedEvent` | `PermissionDenied`, `VersionConflict`, `EvidenceRequired`, `InvalidTransition`, `IdempotencyConflict` |
| `Command: RecordFollowUpAllocation` | `FollowUpId`, `ExpectedVersion`, `AllocationKind` (`ReportedAppearance` \| `VerifiedSettlement`), `SourceOccurrenceRef?`, `SettlementEvidenceRef?`, `Amount` (`decimal > 0`), `CrossComponentAuthRef?`, `ActorId`, `IdempotencyKey` | `Result<FollowUpBalancesSnapshot, ContractError>` | `FollowUpReportedEvent` أو `FollowUpVerifiedSettlementEvent` (وعند الرفض: لا يتغير `FollowUp` ورصيده ويبقى الطلب `UNALLOCATED / NOT_AUTHORIZED`) | `PermissionDenied`, `VersionConflict`, `EvidenceRequired`, `InvalidTransition`, `IdempotencyConflict`, `IncompatibleFollowUpAllocation`, `ExceededRemainingBalance` |
| `Command: CloseFinancialFollowUp` | `FollowUpId`, `ExpectedVersion`, `ClosingRationale`, `ActorId`, `IdempotencyKey` | `Result<FollowUpClosedSnapshot, ContractError>` | `FinancialFollowUpClosedEvent` | `PermissionDenied`, `VersionConflict`, `InvalidTransition`, `EvidenceRequired`, `IdempotencyConflict` |
| `Query: GetAuditCaseFindingsAndFollowUps` | `AuditCaseId`, `ActorId`, `IncludeSupersededRevisions` | قائمة `Findings` مع آخر `ReviewDecision` والـ`FollowUp` المرتبط دون مزج القيم السبع | — *(قراءة محايدة لا تعدل الحالة)* | `PermissionDenied` |
| `Ports` (`Application` -> `Infra` لاحقًا) | `IFindingRepository`, `IFollowUpRepository`, `ISourceReadPort`, `IEvidenceRegistryPort`, `IAuthorizationPolicyPort`, `IIdempotencyPort` | حفظ ذري واسترجاع بالمعرف والنسخة (`ExpectedVersion`)؛ `IIdempotencyPort` يحفظ `(OperationType, TargetAggregateId, ActorScope, IdempotencyKey, RequestPayloadHash, StoredResult)` | نفس المفتاح + نفس البصمة = `Idempotent Replay` للنتيجة الأصلية دون أثر ثانٍ | نفس المفتاح + حمولة مختلفة = `IdempotencyConflict`؛ تغيّر النسخة = `VersionConflict` |

---

## 6. حالات القبول الاصطناعية الخمس الأساسية وحالات الرفض (`Given / When / Then`)

> **ملاحظة حوكمة:** هذه سيناريوهات قبول تصميمية على مستوى المواصفة (**لم تُنفذ كاختبارات كود بعد**: `NOT_RUN`)؛ البيانات والأرقام الواردة اصطناعية تمامًا.

### 6.1 السيناريوهات الخمسة الأساسية (`SPEC-ACC-01..05`)

1. **`SPEC-ACC-01` — تصحيح رسمي وارد من المركزية مع إعادة فحص نسخة المصدر:**
   - **Given:** موظف في `AuditCase`، مصدر النسخة الأولى `SourceVersion = v1` يعرض حافزًا `SourceAmount = 1200` مقابل `ExpectedAmount = 1000`، وفتحت ملاحظة `Finding` في حالة `SentToCentral`.
   - **When:** يصل مستند مركزي مصحح رسميًا `SourceVersion = v2` (`OfficialCorrectionAmount = 1000`) ويُربط بالملاحظة فتُعاد مقارنتها ثم يصدر `ResolveFinding(WithCorrection)`.
   - **Then:** يبقى `SourceAmount = 1200` محفوظًا في `v1` دون تعديل؛ تُربط `v2` بـ`OfficialCorrectionAmount = 1000`؛ تنتقل الملاحظة إلى `ResolvedWithCorrection` مع حفظ `ReviewedAmount` وسبب الحسم ومن اعتمده؛ ولا يُنشأ خصم أو دفع بنكي آلي.

2. **`SPEC-ACC-02` — رد سليم يثبت خطأ المراجع وسحب الملاحظة:**
   - **Given:** ملاحظة مفتوحة في `SentToCentral` تزعم زيادة `SourceAmount = 1200` عن `ExpectedAmount = 1000`.
   - **When:** يرد الطرف المركزي (عبر `CentralInternalReply` أو `RecordedExternalReply` مستوفي `ReplyAttributionEvidenceRef` و`AttributedRespondent` وفق `D-030`) مرفقًا دليلًا موضوعيًا (`SubstantiveEntitlementEvidenceRef`: قرار استحقاق رسمي) يثبت أن المستحق الصحيح `1200` وليس `1000`؛ ثم يُصدر المعتمد قرار `WithdrawFinding`.
   - **Then:** تنتقل الملاحظة إلى `Withdrawn` مع حفظ نص الملاحظة الأصلي والرد والأدلة وسبب السحب وهوية المعتمد؛ **ولا يُنشأ أي `FollowUp` أو فرق مرحل**؛ ويظل سجل المراجعة التاريخي محتفظًا بأثر السحب.

3. **`SPEC-ACC-03` — فرق مؤكد متفق على تأجيله بقرار معتمد:**
   - **Given:** ملاحظة بفرق حقيقي مثبت `200` (مثل استقطاع معتمد `1000` مقابل ظهور `800` أو استحقاق مؤجل `200`) في حالة `UnderReview` / `ResponseReceived` مع وجود سند إقرار/تأجيل للوفاء في الشهر المستهدف التالي.
   - **When:** يصدر قرار `ApproveDeferredDifference` من ذي الصلاحية، ثم يُفتح سجل متابعة `OpenFollowUpForApprovedDifference`.
   - **Then:** تُحسم الملاحظة إلى `DeferredDifferenceAccepted`؛ وتُنشأ دورة `FollowUp` مستقلة في `DeferredScheduled` ← `MonitoringOracleAppearance` برصيد `OriginalAmount = 200`, `VerifiedPaidAmount = 0`, `OutstandingAmount = 200`؛ دون إرسال أي استقطاع إلى Oracle.

4. **`SPEC-ACC-04` — تسوية جزئية أو كاملة موثقة لفرق `200` دون اعتبار الظهور صرفًا ودون اشتراط سبق ظهور Oracle:**
   - **Given:** سجل `FollowUp` مفتوح برصيد أصلي `200` في `MonitoringOracleAppearance`.
   - **When (الخطوة 1 — مسار الظهور ثم التسوية الجزئية):** يظهر مبلغ `100` في كشف صرف/استقطاع لاحق (`RecordReportedAppearance`).  
     **Then (الخطوة 1):** يتحدث `ReportedDisbursementAmount = 100` وتنتقل الحالة إلى `ReportedInDisbursement` (`REPORTED_DEDUCTED`)، **ويبقى `VerifiedPaidAmount = 0` و`OutstandingAmount = 200`** ولا تُغلق المتابعة.  
     **When (الخطوة 2):** يُرفق دليل تسوية/توريد مستقل صالح (`SettlementEvidenceRef`) للمبلغ `100` (`VerifyPartialSettlement`).  
     **Then (الخطوة 2):** يصبح `VerifiedPaidAmount = 100` و`OutstandingAmount = 100` وتنتقل المتابعة إلى `PartiallyVerifiedSettled` (`SETTLED_VERIFIED` جزئي)، وتظل مفتوحةً للـ`100` المتبقية.
   - **المسار البديل المباشر (تسوية كاملة دون سبق ظهور Oracle):** إذا قُدّم من `MonitoringOracleAppearance` مباشرةً دليل تسوية مستقل مكتمل (`SettlementEvidenceRef`) يغطي كامل الـ`200` (`VerifyFullSettlement`) مع تحقق أصل الفرق وكامل الحواجز، تنتقل المتابعة مباشرةً إلى `VerifiedSettledPendingClose` برصيد `VerifiedPaidAmount = 200` و`OutstandingAmount = 0` (مع بقاء `ReportedDisbursementAmount = 0` دون افتراض موافقة قانونية أو إجراء مالي).

5. **`SPEC-ACC-05` — رد خارجي مثبت الاستلام والنسبة مع نقص الدليل الموضوعي أو خلاف يبقي الملاحظة مفتوحة:**
   - **Given:** ملاحظة مفتوحة في `SentToCentral`، ويستلم المراجع المخوّل ردًا خارجيًا من `AttributedRespondent` موثقًا بدليل استلام/نسبة رسمي (`ReplyAttributionEvidenceRef` مثل مذكرة وارد أو محضر إحالة رسمي، استيفاءً لـ`D-030` و`SPEC-NEG-03`)، لكن مضمون الرد يكتفي بإشارة عامة/شفهية للاستحقاق **دون إرفاق الدليل الموضوعي (`SubstantiveEntitlementEvidenceRef`: رقم القرار وصورته)**، أو يظهر خلاف على انطباق الدرجة/السند.
   - **When:** يُسجَّل الرد بنجاح (`RecordCentralExternalReply` ← `ResponseReceived`) بفضل توفر `ReplyAttributionEvidenceRef`، ثم يقيّم المراجع الرد عبر `EvaluateCentralReply`.
   - **Then:** تنتقل الملاحظة مباشرةً إلى `WaitingEvidence` (لنقص `SubstantiveEntitlementEvidenceRef`) أو `Disputed` (للخلاف على السند)، **ويُرفض إغلاق الملاحظة أو سحبها أو اعتبار القيمة المجهولة صفرًا (`UNKNOWN ≠ 0`)**، وتبقى مفتوحةً مع توثيق المستند الموضوعي المطلوب والطرف المسؤول *(أما الادعاء الشفهي الخالي من `ReplyAttributionEvidenceRef` أصلًا فيُرفض تسجيله كـ`RecordedExternalReply` فورًا عند `SentToCentral` وفق `SPEC-NEG-03`)*.

### 6.2 سيناريوهات الرفض والحماية (`SPEC-NEG-01..08`)

| معرف الاختبار | سيناريو الرفض (`Given / When`) | النتيجة المتوقعة (`Then`) |
| :--- | :--- | :--- |
| `SPEC-NEG-01` | **(أ) تكرار متطابق:** إعادة إرسال نفس الأمر (مثل `RecordCentralResponse` أو `RecordFollowUpAllocation`) بنفس `(OperationType, TargetAggregateId, ActorScope, IdempotencyKey)` **ونفس بصمة الحمولة**. <br>**(ب) تعارض مفتاح:** إعادة استخدام نفس `IdempotencyKey` ضمن نفس النطاق **بحمولة مختلفة** (مبلغ أو قناة أو مستند مختلف). | **(أ)** يُعاد نفس `Result` المحفوظ للعملية الأصلية (`Idempotent Replay`) دون إنشاء سجل ثانٍ أو تعديل رصيد أو إطلاق حدث إضافي. <br>**(ب)** يُرفض الطلب فورًا بخطأ `IdempotencyConflict` مع بقاء الحالة المالية والرصيد دون أي تغيير. |
| `SPEC-NEG-02` | تنفيذ `ResolveFinding` أو `VerifyPartialSettlement` بنسخة `ExpectedVersion` قديمة بعد تعديل متزامن لآخرين. | فشل الطلب فورًا بخطأ `VersionConflict` دون الكتابة فوق التعديل الآخر أو تجاوز الرصيد المتبقي. |
| `SPEC-NEG-03` | محاولة تسجيل `RecordedExternalReply` دون `AttributedRespondent` أو دون `ReplyAttributionEvidenceRef` (دليل نسبة/استلام الرد)، أو محاولة `WithdrawFinding`/`ResolveFinding` دون `SubstantiveEntitlementEvidenceRef` وسبب. | رفض الطلب فورًا بخطأ `EvidenceRequired` وعدم تغير الحالة (`SentToCentral` أو `UnderReview` كما هي). |
| `SPEC-NEG-04` | محاولة ربط `200` من كود بند مختلف في `FollowUp` مفتوحة (مثل `MonitoringOracleAppearance` أو `ReportedInDisbursement`) لمجرد تطابق المبلغ أو باب الصرف، مع غياب `SettlementCompatibilityPolicy` و`CrossComponentAuthRef`، أو تقديم مستند إثبات مزيف/غير كافٍ. | رفض طلب التخصيص بخطأ `IncompatibleFollowUpAllocation` أو `EvidenceRequired` وبقاء التخصيص `UNALLOCATED / NOT_AUTHORIZED`؛ **وتبقى `FollowUp` الأصلية في حالتها السليمة وبنفس رصيدها (`OutstandingAmount = 200`) دون نقلها إلى `FollowUpDisputed` أو `FollowUpCancelledByCorrection`**. |
| `SPEC-NEG-05` | محاولة ربط رد مركزي أو `SourceFinancialOccurrence` تخص موظفًا/علاقة عمل أخرى (`PayeeRelationship` / `EmploymentRelationship` مختلف). | رفض الربط لحماية حدود علاقة العمل والمستفيد وعدم تلويث `AuditCase`. |
| `SPEC-NEG-06` | محاولة تعديل قرار حسم معتمد مباشرًا دون `ReopenFinding` أو إخفاء الاعتماد السابق عند إعادة الفتح. | رفض التعديل المباشر بـ`InvalidTransition`؛ واشتراط `ReopenFinding` لتوليد `FindingRevision + 1` مع بقاء النسخة السابقة محفوظة. |
| `SPEC-NEG-07` | محاولة تنفيذ قرار حسم/سحب/إغلاق متابعة بواسطة مستخدم خارج المجموعة أو يجمع بين تعديل المصدر/الرد واعتماده النهائي المخالف لصلاحياته. | رفض فوري بخطأ `PermissionDenied` مع تسجيل محاولة الوصول دون تغيير الحالة. |
| `SPEC-NEG-08` | محاولة إغلاق `FinancialFollowUpClosed` من حالة `ReportedInDisbursement` مباشرة، أو فتح `FollowUp` لملاحظة في حالة `Withdrawn`. | رفض حتمي بخطأ `InvalidTransition` / `EvidenceRequired`. |

---

## 7. مصفوفة قرارات المالك المفتوحة (مرتبطة بـ [`Issue #39`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/39))

> **قرار مؤكد لا يُعاد سؤاله:** دعم قناتَي رد المركزية (`CentralInternalReply` داخل النظام و`RecordedExternalReply` المسجل بواسطة مستخدم الوحدة) **مؤكد بالفعل بالقرار `D-030` في `Issue #39`**. تتركز الأسئلة المتبقية في الضوابط الآتية:

| المعرف | الموضوع والسؤال الموجز للمالك | التصنيف | الأثر العملي على الشريحة الأولى (`Core/Application`) |
| :--- | :--- | :--- | :--- |
| `OD-01` | **صلاحيات الحسم وسحب الملاحظة وإغلاق المتابعة المالية:** من يملك سلطة اعتماد `Withdrawn`؟ وهل تكفي موافقة المراجع المختص لحسم `ResolvedWithCorrection` أم يلزم اعتماد مشرف/معتمد ثانٍ عند السحب وتأجيل الفرق وإغلاق `FinancialFollowUpClosed`؟ | `MUST_DECIDE_BEFORE_CODE` | يحدد قواعد `IAuthorizationPolicyPort` وأدوار `Separation of Duties` في شريحة التطبيق. |
| `OD-02` | **اعتماد حالة `AuditCase` مع بقاء `UNKNOWN` أو `FollowUpPending`:** هل يُسمح باعتماد مراجعة البنود السليمة للموظف بالشهر بينما تبقى متابعة فرق مالي أو بند استقطاع ثانوي بحالة `UNKNOWN` / `FollowUpPending`؟ | `MUST_DECIDE_BEFORE_CODE` | يحسم شروط قفل `AuditCase` مقابل استقلال `Finding` و`FollowUp` (يعالج `MAJ-02`/`DES-01-011` في EGM). |
| `OD-03` | **مستندات إثبات التسوية الفعلية (`VerifiedPaidAmount`):** ما المستند أو الشهادة الرسمية المعتمدة داخل الوحدة الحسابية لإثبات التوريد أو التسوية الفعلية للانتقال من `ReportedInDisbursement` إلى `PartiallyVerifiedSettled` / `VerifiedSettledPendingClose`؟ | `MUST_DECIDE_BEFORE_CODE` | يحدد أنواع `EvidenceRef` المقبولة لإغلاق `FollowUp` (يعالج `Q-04` دون ربط آلي بـ`GPS`). |
| `OD-04` | **نطاق الصرفيات المنفصلة ورعاية الطفل وعلاقتَي التعيين:** هل يظل تعليق بند في صرفية `SEPARATE` مستقلًا عن اعتماد `MAIN_SALARY`؟ وكيف تُضم استمارة `132 ع.ح` وتسوية رعاية الطفل في الشهر نفسه؟ وما القيد عند النقل أو علاقتي التعيين؟ | `MUST_DECIDE_BEFORE_CODE` *(للنطاق بين الصرفيات)* / يمكن بدء شريحة الملاحظة داخل صرفية مفردة أولًا ثم توسيعها | يعالج `BLK-02`, `BLK-04`, `BLK-12`, `MAJ-21` في سجل تحكيم EGM قبل نموذج الصرفيات الكامل. |
| `OD-05` | **حوكمة وتوقيتات قنوات رد المركزية (`D-030`):** ما سياسة إنشاء حسابات `CentralAccountResponder`، وما الحد الأدنى من الإثبات المطلوب لتسجيل `RecordedExternalReply`، وما هي مهل التصعيد الزمنية؟ | `DEFERRED_NONBLOCKING` *(على أصل القناتين)* / `MUST_DECIDE_BEFORE_CODE` *(لإنتاج RBAC الفعلي)* | نموذج `D-030` في المواصفة يدعم القناتين من الآن، بينما تُضبط القوائم والمهل كإعدادات صلاحيات عند التفعيل. |
| `OD-06` | **مصفوفة توافق التسوية بين بندين (`SettlementCompatibilityPolicy`):** ما البنود/الجهات الدائنة المسموح بترحيل فرق استقطاع إليها أو تسويته منها، ومن يعتمدها قانونيًا؟ | `DEFERRED_NONBLOCKING` *(للشريحة الأولى)* / `MUST_DECIDE_BEFORE_CODE` *(قبل تفعيل تسوية عبر البنود)* | تظل ميزة التخصيص بين بندين محظورة تلقائيًا (`UNALLOCATED`) حتى إيداع السياسة المعتمدة. |

---

## 8. مصفوفة التتبع (`D-029 / P-017 / D-030` وEGM والشريحة الأولى)

| المرجع / المطلب | الوصف المختصر | الحالة | مرجع EGM / المواصفة | سيناريو القبول | يدخل الشريحة الأولى (`Core/Application` عند التصريح)؟ |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `D-029` + [قاعدة العمل §3](../WORKING-BASELINE-AR.md#L20-L32) | دورة الملاحظة ← الرد ← الحسم (تصحيح/سحب/تأجيل/تسوية/خلاف) وحفظ الأصل والرد والمعتمد | `OWNER_CONFIRMED` *(التفاصيل التشغيلية في المواصفة `PROPOSED`)* | [EGM Register §4.2](../egm/EGM-ADJUDICATION-REGISTER-AR.md) (`BLK-05`, `BLK-06`, `MIN-04`) | `SPEC-ACC-01..05`, `SPEC-NEG-01..08` | **نعم** — في حدود `Core/Application` `in-memory` بعد بوابة `EGM` واعتماد قرارات `OD-01..03` |
| `P-017` | إعطاء الأولوية لدورة الملاحظات والردود والحسم ومتابعة الفروق | `PROPOSED` *(مرتبط بأولوية `#56`)* | [DECISION-REGISTER](../governance/DECISION-REGISTER.md#L92) | `SPEC-ACC-01..05` | **نعم** — هي نطاق الشريحة الأولى المقترحة |
| `D-030` (`Issue #39`) | دعم `CentralInternalReply` و`RecordedExternalReply` مع فصل `RecordedBy` عن `AttributedRespondent` | `OWNER_CONFIRMED` *(تفاصيل الصلاحيات والمهل: `OWNER_DECISION_REQUIRED`)* | [`Issue #39`](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/39) + `OD-05` | `SPEC-ACC-02`, `SPEC-ACC-05`, `SPEC-NEG-03` | **نعم** — تمثيل القناتين وعقود الإسناد داخل `Core/Application` (بدون حسابات إنتاج فعلية الآن) |
| فصل القيم السبع وفصل `Reporting` عن `Verified Settlement` | منع الخلط بين الظهور في Oracle والتسوية/الدفع الفعلي، وعدم إغلاق المتابعة عند سحب الملاحظة | `OWNER_CONFIRMED` (ثوابت `AGENTS.md` + `D-028`) | `BLK-05` / `Q-04` (`EGM`) + [HISTORICAL-LEAVE](HISTORICAL-LEAVE-REVIEW-ADJUSTMENTS-AND-DEDUCTION-CARRY.md) | `SPEC-ACC-02`, `SPEC-ACC-04`, `SPEC-NEG-04`, `SPEC-NEG-08` | **نعم** — حواجز نقية إجبارية داخل `Core` |
| التزامن المتفائل و`IdempotencyKey` | حماية تعديل الملاحظات والردود وتخصيص الفروق ضد السباق والتكرار | `PROPOSED` *(انبثق عن معايير `ADR-005`)* | `BLK-06`, `MIN-04` (`EGM`) | `SPEC-NEG-01`, `SPEC-NEG-02` | **نعم** — حقول وعقود نتائج `Core/Application` |
| صلاحيات السحب/الإغلاق، سياسة `UNKNOWN`، مستند `VerifiedPaidAmount` | تحديد الأدوار المخولة وشروط قفل `AuditCase` ومستند التسوية المعتمد | `OWNER_DECISION_REQUIRED` | `OD-01..04`, `MAJ-02`, `MAJ-21` | `SPEC-ACC-04`, `SPEC-NEG-07` | **بوابة شرطية:** يلزم حسمها قبل تجميد منطق القفل والاعتماد النهائي |
| ربط `SQL Server 2014`، `ORM`، قراءة PDF/Excel، حساب قوانين غير موثقة، وربط `GPS` | الطبقة الفيزيائية للتخزين والاستخراج والمحركات القانونية والبنوك | مؤجل / خارج الشريحة | [Issue #56](https://github.com/AlexandriaDeveloper/PayrollReviewSystem/issues/56), [ADR-003/005](../decisions/ADR-003-ddd-modular-monolith-clean-architecture.md) | `NOT_IN_SLICE_1` | **لا** — خارج الشريحة الأولى تمامًا |

