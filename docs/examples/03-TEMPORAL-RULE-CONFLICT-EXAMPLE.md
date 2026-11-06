# مثال عملي: غياب وترقية وإجازة متداخلة وتعديل يصل بعد الاعتماد

> **كل الأسماء والأرقام والمعرفات والقواعد والأحداث في المثال اصطناعية بالكامل، وليست وقائع موظف أو حكم قانوني.**
> الهدف توضيح **كيف يحدد البرنامج القاعدة السارية لكل بند وفترة** وكيف يتوقف عند تضارب القرارات. لا يحدد المثال قيمة خصم يوم الغياب أو أي نسبة قانونية.

## السيناريو
مستفيد افتراضي له علاقة عمل واحدة ومرتبات أكتوبر 2026:
- درجته الاختبارية `G-B` حتى **12 أكتوبر شاملًا**؛ وقرار ترقية يغيّرها إلى `G-A` من **13 أكتوبر** بأثر مالي. القرار وصل للنظام **5 نوفمبر**، بعد اعتماد مراجعة أكتوبر.
- قرار إداري `DEC-ABS` يثبت واقعة غياب عن **12–14 أكتوبر شاملًا**، وتاريخ تسجيله يختلف عن أيام الواقعة.
- قرار إجازة `DEC-LEAVE` يخص **13 أكتوبر فقط**، وصل متأخرًا، ولم يحدد بعد هل يُلغي يوم الغياب المذكور أو كيف يتفاعل معه.
- بندان للاختبار: `COMP-BASE` للأجر و`COMP-ALLOW` لعلاوة اختبارية قد تختلف قواعد تأثرها؛ لا نفترض أن الغياب يؤثر على الاثنين بالضرورة.
- نص القاعدة القانونية، طريقة حساب اليوم وباب الصرف **غير محددة عمدًا**؛ أي نتيجة مبلغ نهائية ستكون غير قانونية لو اخترعنا القاعدة.

## Input رقم 1 — حقائق زمنية وقرارات

~~~json
{
  "cycle": "2026-10",
  "asKnownAt": "2026-11-11T10:00:00+02:00",
  "institutionalUnitCode": "30200105",
  "payeeRelationshipId": "PAY-TEST-01",
  "employmentRelationshipId": "EMP-TEST-01",
  "employeeCategory": "ADMINISTRATIVE_TEST_CATEGORY",
  "gradeHistory": [
    {
      "grade": "G-B",
      "validFrom": "2026-10-01",
      "validToExclusive": "2026-10-13",
      "evidenceId": "DEC-OLD-GRADE"
    },
    {
      "grade": "G-A",
      "validFrom": "2026-10-13",
      "validToExclusive": null,
      "evidenceId": "DEC-PROMOTION",
      "recordedAt": "2026-11-05T09:00:00+02:00"
    }
  ],
  "events": [
    {
      "eventId": "EV-ABS-01",
      "kind": "UNEXCUSED_ABSENCE",
      "validFrom": "2026-10-12",
      "validToExclusive": "2026-10-15",
      "evidenceId": "DEC-ABS",
      "evidenceStatus": "DOCUMENT_VERIFIED"
    },
    {
      "eventId": "EV-LEAVE-01",
      "kind": "APPROVED_LEAVE",
      "validFrom": "2026-10-13",
      "validToExclusive": "2026-10-14",
      "evidenceId": "DEC-LEAVE",
      "evidenceStatus": "DOCUMENT_VERIFIED",
      "supersedesEventId": null
    }
  ],
  "components": ["COMP-BASE", "COMP-ALLOW"],
  "calculationMode": "PROPOSAL_ONLY"
}
~~~

**تنبيه:** `G-A` و`G-B` ليستا درجات وظيفية رسمية؛ مجرد رمزين لاختبار تغيّر الدرجة. والتحقق من وثيقة الإجازة لا يعني وحده أنها **ألغت** وثيقة الغياب.

## ERD مصغّر — ما الجداول التي توصل إلى القرار؟

~~~mermaid
erDiagram
  EMPLOYMENT_RELATIONSHIP ||--o{ GRADE_HISTORY : "تاريخ درجات"
  EMPLOYMENT_RELATIONSHIP ||--o{ EFFECTIVE_EVENT : "وقائع معتمدة"
  ADMINISTRATIVE_DECISION ||--o{ EFFECTIVE_EVENT : "ينشئ نطاقات"
  ADMINISTRATIVE_DECISION ||--o{ DECISION_SUPERSESSION_SCOPE : "تصحيح محدد"
  EFFECTIVE_EVENT ||--o{ DECISION_SUPERSESSION_SCOPE : "يراجع نطاقه"
  LEGAL_SOURCE ||--o{ LEGAL_PROVISION_VERSION : "نصوص وإصدارات"
  LEGAL_PROVISION_VERSION ||--o{ FINANCIAL_RULE_VERSION : "يسند القاعدة"
  FINANCIAL_RULE_VERSION ||--o{ RULE_APPLICABILITY : "شروط الانطباق"
  FINANCIAL_RULE_VERSION ||--o{ RULE_EFFECT : "أثر البنود"
  FINANCIAL_RULE_VERSION ||--o{ RULE_INTERACTION : "علاقات الأحكام"
  FINANCIAL_COMPONENT ||--o{ RULE_EFFECT : "يعالج بندا"
  RULE_RESOLUTION_RUN ||--o{ RULE_SEGMENT : "يقسم الفترة"
  RULE_SEGMENT ||--o{ RULE_CANDIDATE_EVALUATION : "يوثق المرشحين"
  RULE_SEGMENT ||--o{ EXPECTED_CALCULATION_LINE : "ينتج مقترحا"
  EMPLOYMENT_RELATIONSHIP {
    string EmploymentId PK
    string PersonId FK
  }
  GRADE_HISTORY {
    string GradeHistoryId PK
    string EmploymentId FK
    string Grade
    date ValidFrom
    date ValidTo
    datetime RecordedAt
  }
  ADMINISTRATIVE_DECISION {
    string DecisionId PK
    date IssuedOn
    string DocumentEvidenceId
  }
  EFFECTIVE_EVENT {
    string EventId PK
    string DecisionId FK
    string EventKind
    date ValidFrom
    date ValidTo
  }
  DECISION_SUPERSESSION_SCOPE {
    string ScopeId PK
    string EventId FK
    date ValidFrom
    date ValidTo
    string Reason
  }
  LEGAL_SOURCE {
    string LegalSourceId PK
    string VerifiedSourceRef
  }
  LEGAL_PROVISION_VERSION {
    string ProvisionId PK
    string LegalSourceId FK
    date ValidFrom
  }
  FINANCIAL_RULE_VERSION {
    string RuleVersionId PK
    string ProvisionId FK
    string ApprovalStatus
  }
  RULE_APPLICABILITY {
    string ConditionId PK
    string RuleVersionId FK
    string PredicateDefinition
  }
  RULE_EFFECT {
    string EffectId PK
    string RuleVersionId FK
    string ComponentId FK
    string EffectType
  }
  RULE_INTERACTION {
    string InteractionId PK
    string RuleVersionId FK
    string OtherRuleVersionId FK
    string DocumentedRelationship
  }
  FINANCIAL_COMPONENT {
    string ComponentId PK
    string Name
  }
  RULE_RESOLUTION_RUN {
    string RunId PK
    string SnapshotHash
  }
  RULE_SEGMENT {
    string SegmentId PK
    string RunId FK
    date From
    date To
    string ResolutionStatus
  }
  RULE_CANDIDATE_EVALUATION {
    string CandidateId PK
    string SegmentId FK
    string RuleVersionId FK
    string EvaluationReason
  }
  EXPECTED_CALCULATION_LINE {
    string LineId PK
    string SegmentId FK
    decimal ProposedAmount
  }
~~~

## 1 — كيف تُقسَّم فترة أكتوبر؟

نقسم التواريخ عند بداية الغياب وتغيير الدرجة وبداية الإجازة ونهايتها ونهاية الغياب. تُنشئ هذه الخطوط **فترات غير متداخلة**:

| الفترة الداخلية `[من، إلى)` | الدرجة | وقائع نشطة | نتيجة اختيار القاعدة |
| --- | --- | --- | --- |
| 1–12 أكتوبر | `G-B` | لا غياب من الوقائع المقدمة | حساب البنود العادية فقط إن كانت قواعدها معتمدة |
| **12–13 أكتوبر** | `G-B` | يوم من الغياب المثبت | حل أثر الغياب على الدرجة `G-B` لهذا البند |
| **13–14 أكتوبر** | `G-A` | غياب + إجازة معتمدان متداخلان | **`EVENT_CONFLICT`**؛ لا نخصم أو نلغي بالحدس |
| **14–15 أكتوبر** | `G-A` | يوم من الغياب المثبت | حل أثر الغياب على الدرجة `G-A` لهذا البند |
| 15 أكتوبر–1 نوفمبر | `G-A` | لا غياب من الوقائع المقدمة | حساب البنود العادية وفق القواعد السارية |

**ملحوظة:** هذه **فترات تقويمية**، وليست إثباتًا أن كل يوم فيها يوم عمل مدفوع. أيام العمل والفترات الجزئية والأجر عن كل يوم تحتاج قواعد وتقويم دوام معتمدين.

## 2 — كيف تُختار القاعدة داخل المقطع؟

لكل بند على حدة يُجلب مجموعة القواعد المستوفية للشروط. لتوضيح الاختيار فقط، افترض وجود نسختين **مختلقتين للبرمجة والاختبارات**:

| RuleVersion | المرشح لأي بند/فئة؟ | شرط الدرجة والفترة | ما ينتج |
| --- | --- | --- | --- |
| `TEST-R-BASE-G-B` | `COMP-BASE` / فئة اختبارية إدارية | `G-B` في أيام الواقعة | ترشيح أثر الغياب بلا مبلغ قبل اعتماد الصيغة |
| `TEST-R-BASE-G-A` | `COMP-BASE` / فئة اختبارية إدارية | `G-A` في أيام الواقعة | ترشيح أثر مختلف بحسب سياق هذه الدرجة |
| `TEST-R-ALLOW` | `COMP-ALLOW` | يلزم بحث سند استقلال العلاوة | لا تقرر تأثرها تلقائيًا |

لا تعتبر قاعدة اختبارية `ACTIVE` في الإنتاج. في البيئة الحقيقية يجب إثبات كل حكم ومصدره ونسخته ومدة سريانه والكادر والبند والصيغة والاعتماد.

## Output رقم 1 — نتيجة حل القواعد دون اختراع مبلغ

~~~json
{
  "resolutionRunId": "RUN-02",
  "financialPeriod": "2026-10",
  "overallStatus": "PARTIAL_RESULT",
  "segments": [
    {
      "from": "2026-10-12",
      "toExclusive": "2026-10-13",
      "grade": "G-B",
      "eventIds": ["EV-ABS-01"],
      "component": "COMP-BASE",
      "candidateRuleIds": ["TEST-R-BASE-G-B"],
      "status": "RULE_REQUIRES_APPROVAL",
      "proposedAmount": null
    },
    {
      "from": "2026-10-13",
      "toExclusive": "2026-10-14",
      "grade": "G-A",
      "eventIds": ["EV-ABS-01", "EV-LEAVE-01"],
      "component": "COMP-BASE",
      "status": "EVENT_CONFLICT",
      "proposedAmount": null,
      "reason": "الغياب والإجازة متداخلان دون سند إلغاء محدد لهذا اليوم"
    },
    {
      "from": "2026-10-14",
      "toExclusive": "2026-10-15",
      "grade": "G-A",
      "eventIds": ["EV-ABS-01"],
      "component": "COMP-BASE",
      "candidateRuleIds": ["TEST-R-BASE-G-A"],
      "status": "RULE_REQUIRES_APPROVAL",
      "proposedAmount": null
    }
  ],
  "reviewAction": "REQUEST_OFFICIAL_CONFLICT_RESOLUTION",
  "oldApprovalPreserved": true
}
~~~

الـOutput ده يستعرض المقاطع المتأثرة بالغياب **فقط** للاختصار؛ المقاطع الأخرى تظهر في ملف `RuleTrace` الكامل. لا رقم مالي هنا لأن صيغة القانون وأيام العمل لم تُعتمد.

## 3 — وصول قرار تصحيح يلغي الغياب يوم 13 فقط

إذا ورد **قرار مختص واضح** يقرر أن يوم 13 كان إجازة معتمدة و**يلغي واقعة الغياب لهذا اليوم وحده**، نضيف:

~~~json
{
  "decisionId": "DEC-CORRECTION-13",
  "targetEventId": "EV-ABS-01",
  "supersessionType": "PARTIAL_REVOCATION",
  "scope": {
    "from": "2026-10-13",
    "toExclusive": "2026-10-14"
  },
  "legalAndAdministrativeEvidence": "VERIFIED_PRIVATE_DOCUMENT_REFERENCE",
  "recordedAt": "2026-11-12T11:00:00+02:00"
}
~~~

### الفرق في المخرجات بعد التصحيح
| اليوم | قبل قرار التصحيح | بعد قرار التصحيح |
| --- | --- | --- |
| 12 أكتوبر | غياب ودرجة `G-B` | **كما هو**، ولا يُلغى |
| 13 أكتوبر | `EVENT_CONFLICT` | إجازة وفق مستند الإلغاء، **لا يُعامل كغياب بدون إذن** |
| 14 أكتوبر | غياب ودرجة `G-A` | **كما هو**، ولا يُلغى |

ثم يعاد اختيار القواعد والحساب **للبنود والمقاطع التي مسها التصحيح فقط**، وتنشأ نسخة مراجعة جديدة، وتظل نتيجة أكتوبر الأولى محفوظة كما كانت وقت الاعتماد.

## UML — كيف يتعاون محرك الأحداث والدرجات والقواعد؟

~~~mermaid
sequenceDiagram
  autonumber
  actor Reviewer as "المراجع المالي"
  participant Events as "قرارات ووقائع"
  participant History as "تاريخ الدرجة والوظيفة"
  participant Catalog as "كتالوج قواعد معتمد"
  participant Resolver as "محرك تحديد القواعد"
  participant Calc as "حساب مقترح"
  participant Case as "سجل المراجعة"
  Reviewer->>Resolver: حل أثر الغياب في أكتوبر على بند محدد
  Resolver->>Events: جلب الوقائع والتصحيحات وتواريخها
  Resolver->>History: جلب الدرجات الحقيقية في كل يوم
  Resolver->>Catalog: جلب قواعد الكادر والبند السارية
  Resolver->>Resolver: تقسيم الفترة عند كل تغيير مؤثر
  alt يوم فيه غياب وإجازة دون إلغاء مثبت
    Resolver-->>Case: EVENT_CONFLICT + الأدلة والمقطع
  else الحكم والمدخلات مكتملان
    Resolver->>Calc: حساب الأثر بالمصدر المعتمد
    Calc-->>Case: مقترح وRuleTrace لكل بند ومقطع
  end
  Case-->>Reviewer: عرض الأسباب والأيام والمصادر والنتيجة
  Note over Case: لا تعديل Oracle ولا إلغاء الاعتماد القديم
~~~

## التمييز بين ثلاثة أنواع من التعارض
- **`EVENT_CONFLICT`**: القرارات أو الوقائع نفسها غير محسومة (غياب وإجازة على اليوم نفسه).
- **`HISTORY_CONFLICT`**: بيانات الدرجة أو الحالة المعتمدة متضاربة في العلاقة نفسها والفترة نفسها.
- **`RULE_CONFLICT`**: حكمين أو أكثر مرشحين على نفس بند/فترة، ولا يوجد سند معتمد يحدد طريقة اجتماعهما أو أسبقيتهما.

حل واحد منها لا يعني حل الباقي. ويجب أن يستطيع المراجع رؤية النص الأصلي والأدلة ومكان التعارض ومن حسمه ومتى.

## اختبار صحة السيناريو
المخرجات المطلوبة من الاختبارات `TR-002`, `TR-003`, `TR-004`, `TR-005`, `TR-010` و`TR-018` معرفة في [مواصفات محرك القواعد](../specs/TEMPORAL-RULE-RESOLUTION-AND-CONFLICTS.md). **لا تنفذ أو تفعّل هذه القواعد في النظام لمجرد وجود المثال.**
