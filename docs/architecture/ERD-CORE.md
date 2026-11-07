# ERD رقم 1 — الوحدات والأشخاص والعلاقات الوظيفية والقواعد المالية

**مخطط تصوري وليس تعليمات إنشاء SQL.** المجمع الطبي هو الجهة الحسابية التي تخدم كليات الطب وطب الأسنان والصيدلة، ولا نحدد الكادر القانوني تلقائيًا من اسم الكلية.

## العلاقات المرسومة
```mermaid
erDiagram
  ORGANIZATION_UNIT ||--o{ ORGANIZATION_UNIT : يتبع
  PERSON ||--o{ PERSON_IDENTIFIER : يمتلك
  PERSON ||--o{ EMPLOYMENT_RELATIONSHIP : يرتبط_وظيفيا
  PERSON ||--o{ PAYEE_RELATIONSHIP : يستفيد
  ORGANIZATION_UNIT ||--o{ EMPLOYMENT_RELATIONSHIP : يعين
  ORGANIZATION_UNIT ||--o{ PAYEE_RELATIONSHIP : يصرف
  EMPLOYMENT_RELATIONSHIP o|--o{ PAYEE_RELATIONSHIP : يرتبط
  EMPLOYMENT_RELATIONSHIP ||--o{ GRADE_HISTORY : لديه
  EMPLOYMENT_RELATIONSHIP ||--o{ STATUS_HISTORY : لديه
  EMPLOYMENT_RELATIONSHIP ||--o{ WORK_ARRANGEMENT : لديه
  PAYEE_RELATIONSHIP ||--o{ APPROVED_FINANCIAL_FACT : يسجل
  PAYEE_RELATIONSHIP ||--o{ COMPONENT_ASSIGNMENT : يتلقى
  FINANCIAL_COMPONENT ||--o{ COMPONENT_ASSIGNMENT : يسند
  FINANCIAL_COMPONENT ||--o{ COMPONENT_AUDIT_POLICY : يخضع_لسياسة
  FINANCIAL_COMPONENT ||--o{ FINANCIAL_RULE_VERSION : يحسب_بقاعدة
  LEGAL_PROVISION_VERSION o|--o{ FINANCIAL_RULE_VERSION : يسند_قانونيا
  LEGAL_SOURCE ||--o{ LEGAL_PROVISION_VERSION : يتضمن
  ADMINISTRATIVE_DECISION ||--o{ EVENT_EFFECT : ينشئ
  FINANCIAL_COMPONENT o|--o{ EVENT_EFFECT : يؤثر
  PAYEE_RELATIONSHIP o|--o{ ADMINISTRATIVE_DECISION : يخص
  ORGANIZATION_UNIT {
    uuid OrganizationUnitId PK
    uuid ParentUnitId FK
    string InstitutionalCode
    string ArabicName
    string UnitType
    string Status
  }
  PERSON {
    uuid PersonId PK
    string DisplayName
    string VerificationStatus
  }
  PERSON_IDENTIFIER {
    uuid IdentifierId PK
    uuid PersonId FK
    string Kind
    string ProtectedValueRef
    string VerifiedStatus
  }
  EMPLOYMENT_RELATIONSHIP {
    uuid EmploymentId PK
    uuid PersonId FK
    uuid EmployerUnitId FK
    string PersonnelNumber
    date ValidFrom
    date ValidTo
  }
  PAYEE_RELATIONSHIP {
    uuid PayeeRelationshipId PK
    uuid PersonId FK
    uuid PayingUnitId FK
    uuid EmploymentId FK "nullable"
    string ReasonOrRelationshipType
    date ValidFrom
    date ValidTo
  }
  GRADE_HISTORY {
    uuid GradeHistoryId PK
    uuid EmploymentId FK
    string GradeOrRank
    date ValidFrom
    date ValidTo
    uuid DecisionId FK "nullable"
  }
  STATUS_HISTORY {
    uuid StatusHistoryId PK
    uuid EmploymentId FK
    string RegimeAndStatus
    date ValidFrom
    date ValidTo
    uuid DecisionId FK "nullable"
  }
  WORK_ARRANGEMENT {
    uuid ArrangementId PK
    uuid EmploymentId FK
    string WorkPattern
    decimal CashFraction_nullable
    date ValidFrom
    date ValidTo
    uuid DecisionId FK
  }
  APPROVED_FINANCIAL_FACT {
    uuid FactId PK
    uuid PayeeRelationshipId FK
    string FactType
    decimal Amount_nullable
    date ReferenceDate
    date ValidFrom
    date ValidTo
    string EvidenceStatus
    uuid SupersedesFactId FK "nullable"
  }
  COMPONENT_ASSIGNMENT {
    uuid AssignmentId PK
    uuid PayeeRelationshipId FK
    uuid ComponentId FK
    decimal FixedValue_nullable
    date ValidFrom
    date ValidTo
    uuid DecisionId FK "nullable"
  }
  FINANCIAL_COMPONENT {
    uuid ComponentId PK
    string CanonicalName
    string EconomicRole
    string StableInternalCode
  }
  COMPONENT_AUDIT_POLICY {
    uuid PolicyId PK
    uuid ComponentId FK
    uuid OrganizationUnitId FK "nullable"
    string ReviewTreatment
    date ValidFrom
    date ValidTo
    string ApprovalStatus
  }
  FINANCIAL_RULE_VERSION {
    uuid RuleVersionId PK
    uuid ComponentId FK
    uuid ProvisionVersionId FK "nullable"
    string Applicability
    string Version
    date EffectiveFrom
    date EffectiveTo
    string VerificationStatus
  }
  LEGAL_SOURCE {
    uuid LegalSourceId PK
    string SourceNumber
    string VerifiedSourceRef
    string Status
  }
  LEGAL_PROVISION_VERSION {
    uuid ProvisionVersionId PK
    uuid LegalSourceId FK
    string ArticleAnchor
    date EffectiveFrom
    date EffectiveTo
  }
  ADMINISTRATIVE_DECISION {
    uuid DecisionId PK
    uuid PayeeRelationshipId FK "nullable"
    string DecisionKind
    date IssuedDate
    date FunctionalEffectiveDate_nullable
    date FinancialEffectiveDate_nullable
    string EvidenceRef
    string ApprovalStatus
  }
  EVENT_EFFECT {
    uuid EventEffectId PK
    uuid DecisionId FK
    uuid ComponentId FK "nullable"
    date EffectiveFrom
    date EffectiveTo
    string ImpactType
  }
```

## معنى العلاقات
- `Person` كيان للشخص، وقد يمتلك أكثر من `EmploymentRelationship` عبر الزمن وأكثر من `PayeeRelationship`، بما فيها مكافأة لمستفيد غير موظف.
- `OrganizationUnit` تنظم الجهة الحسابية والكليات، و`EmploymentRelationship` تحفظ جهة التعيين وتواريخها.
- `GradeHistory` و`StatusHistory` و`WorkArrangement` تحفظ الدرجة والكادر والانتداب/جزء الوقت **بحسب الفترة**.
- `ApprovedFinancialFact` تحفظ قيم المراجع المعتمدة: أساسي مرجعي تاريخي، وعاء معاشات موثق، ثابت مالي وغير ذلك. ليس كل Fact بندًا نقديًا.
- `FinancialComponent` اسم البند الداخلي، و`ComponentAssignment` إسناده إلى علاقة صرف. `ComponentAuditPolicy` تحدد هل يحسب مستقلاً أو يأخذ مبلغ المركزية.
- `FinancialRuleVersion` يربط القاعدة بالبند والفئات وفترة السريان والمادة القانونية الأصلية؛ لا يُفعل قبل اعتماد المصدر.
- `AdministrativeDecision` يحتفظ بتاريخ القرار والسريان الإداري والأثر المالي، ويحدد `EventEffect` الأثر لكل بند وفترة.

## قواعد مهمّة
1. لا تجعل `PersonId` مساويًا للرقم القومي أو كود الجهة أو رقم الصرف.
2. قد يكون ارتباط `PayeeRelationship` بعلاقة العمل اختياريًا؛ فصاحب المكافأة قد لا يكون موظفًا.
3. انخفاض استحقاق جزء الوقت إلى 65% في مثال معين لا يعني انخفاض **وعاء المعاشات المعتمد**؛ الوعاء Fact مستقل عن الأجر النقدي.
4. المرجع المؤرخ في 30 يونيو 2015 ليس استحقاقًا جديدًا ولا يضاف للإجمالي.
5. بعض الصناديق والتأمينات مصدرها المركزية ولا تُحسب مجددًا؛ الرقابة الاستثنائية تحتاج إثباتًا.
6. يمكن أن يحتوي `FinancialComponent` على تصنيف محاسبي وباب صرف مختلفين عن كود Oracle الخارجي.
7. احتفظ بتواريخ السريان ووقت التسجيل وأسباب التصحيح، وأسماء من يعتمدون التعديلات؛ أضف الحقول في التصميم الفيزيائي بعد ADR.
8. لا تفرض قاعدة منع التداخل الزمني على كل العلاقات دون دراسة نوعها؛ لبعض الوقائع أكثر من أثر متزامن مشروع.

## توسيع مقترح لدعم القواعد المتداخلة
توسعة `FinancialRuleVersion` و`AdministrativeDecision` في [مخطط تداخل القواعد والدرجات](../examples/03-TEMPORAL-RULE-CONFLICT-EXAMPLE.md) تعرض `RuleApplicability` و`RuleEffect` و`RuleInteraction` و`RuleResolutionSegment` دون تكرار كيان أساسي أو تنفيذ SQL. شاهد [خوارزمية اختيار الحكم](../specs/TEMPORAL-RULE-RESOLUTION-AND-CONFLICTS.md).

## ERD — قاعدة واحدة بثلاثة تأثيرات استقطاع
ربط `FinancialRuleVersion` بعدة `RuleEffect`، لكل منها `BaseComponentId` و`TargetDeductionComponentId` ونسبة ثابتة أو `GradeRateTableVersion`، مع تاريخ مرجع الدرجة والوعاء وسياسة أيام الشهر والتقريب. هذه كيانات منطقية يُفضّل إعادة استخدام الموجود منها بدل إنشاء جدول مكرر.

```mermaid
erDiagram
 FINANCIAL_RULE_VERSION ||--o{ RULE_EFFECT : "ثلاثة آثار"
 RULE_EFFECT o|--o{ GRADE_RATE_TABLE_VERSION : "جدول نسب اختياري"
 GRADE_RATE_TABLE_VERSION ||--o{ GRADE_RATE_ROW : "ربط فئات الدرجة"
 FINANCIAL_RULE_VERSION ||--o{ CALCULATION_RUN : "إصدارات حساب"
 CALCULATION_RUN ||--o{ CALCULATION_EFFECT_RESULT : "نتيجة كل بند"
 RULE_EFFECT ||--o{ CALCULATION_EFFECT_RESULT : "تفاصيل الأثر"
```

[مثال ثلاثة استقطاعات 156% و91% وثالث حسب الدرجة](../specs/FACULTY-ANNUAL-LEAVE-MULTI-DEDUCTION-RULE.md).

## مثال بيانات ومخرجات
راجع [مثال ERD للبيانات الفعلية الافتراضية](../examples/01-ERD-INPUT-OUTPUT.md) الذي يوضح Person وعلاقة العمل والصرف والمبالغ المعتمدة والأسطر المستخرجة.
