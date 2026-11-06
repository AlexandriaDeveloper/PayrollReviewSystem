# UML — الكلاسات والواجهات البرمجية

**هذا نموذج علاقات مبدئي وليس مشروع كود.** أسماء الكلاسات والـInterfaces بالإنجليزية للحفاظ على معانيها في C# أو أي تقنية نعتمدها لاحقًا، ولا يعني وجود Class ضرورة إنشاء جدول مماثل له.

## الرسم الأول: علاقات الدومين
```mermaid
classDiagram
  direction LR

  class PayrollCycle {
    +CycleId
    +Period
    +OrganizationUnitId
    +CycleType
  }
  class Person {
    +PersonId
    +DisplayName
  }
  class EmploymentRelationship {
    +EmploymentId
    +ValidFrom
    +ValidTo
  }
  class PayeeRelationship {
    +PayeeRelationshipId
    +RelationType
  }
  class SourceDocument {
    +DocumentId
    +Sha256
    +ArchiveReference
  }
  class SourceFinancialOccurrence {
    +SourceLineId
    +RawAccountCode
    +RawArabicLabel
    +RawAmountToken
    +ParsedAmount
    +PageRowPaneReference
  }
  class FinancialComponent {
    +ComponentId
    +EconomicRole
  }
  class ComponentAuditPolicy {
    +ReviewTreatment
    +EffectiveFrom
    +EffectiveTo
  }
  class FinancialRuleVersion {
    +RuleVersionId
    +Applicability
    +EffectiveFrom
  }
  class ExpectedCalculation {
    +CalculationRunId
    +InputSnapshot
    +Status
  }
  class ExpectedLine {
    +ComponentId
    +EntitlementPeriod
    +ExpectedAmount
    +EvidenceTrace
  }
  class AuditCase {
    +ReviewRevision
    +ReviewStatus
    +RecordAction()
    +RequestReopen()
  }
  class AuditFinding {
    +FindingKind
    +EvidenceStatus
  }
  class ReviewApproval {
    +ApprovalRevision
    +SnapshotHash
  }
  class RetroEntitlement {
    +EntitlementPeriod
    +DifferenceAmount
  }
  class RetroPaymentAllocation {
    +VerifiedAllocationAmount
  }
  Person "1" --> "0..*" EmploymentRelationship
  Person "1" --> "0..*" PayeeRelationship
  EmploymentRelationship "0..1" <-- "0..*" PayeeRelationship : optional
  PayrollCycle "1" --> "0..*" AuditCase
  PayeeRelationship "1" --> "0..*" AuditCase
  SourceDocument "1" --> "0..*" SourceFinancialOccurrence : parser outputs
  FinancialComponent "1" --> "0..*" ComponentAuditPolicy
  FinancialComponent "1" --> "0..*" FinancialRuleVersion
  FinancialComponent "1" --> "0..*" ExpectedLine
  ExpectedCalculation "1" *-- "0..*" ExpectedLine
  AuditCase "1" *-- "0..*" AuditFinding
  AuditCase "1" --> "0..*" ReviewApproval
  RetroEntitlement "1" --> "0..*" RetroPaymentAllocation
```

### تفسير الرسم
- \`Person\` شخص قد يكون له صفر أو أكثر من العلاقات الوظيفية \`EmploymentRelationship\` وعلاقات استحقاق \`PayeeRelationship\`.
- \`PayrollCycle\` دورة شهرية أو صرفية أخرى، وتشمل ملفات \`AuditCase\`.
- \`SourceDocument\` ينشئ استخراجُه عدة \`SourceFinancialOccurrence\` مستقلة؛ تكرار الكود لا يلغي وقوع أي سطر.
- \`FinancialComponent\` هو تعريف البند المعتمد؛ وله \`ComponentAuditPolicy\` متغيرة بالسياق وإصدارات \`FinancialRuleVersion\`.
- \`ExpectedCalculation\` يحتوي أسطر \`ExpectedLine\` مستقلة عن المصدر.
- \`AuditCase\` يحتفظ بالملاحظات والاعتمادات بإصدارات، و\`RetroEntitlement\` يتتبع التسويات المخصصة إذا ثبت الدفع.

## الرسم الثاني: واجهات الخدمات وفصل المسؤوليات
```mermaid
classDiagram
  direction TB

  class IPdfExtractionAdapter {
    <<interface>>
    +Extract(SourceDocument) ParsedDocument
  }
  class ISourceArchive {
    <<interface>>
    +StoreImmutable(bytes, metadata) SourceDocument
    +ReadByReference(id) Stream
  }
  class IImportQualityGate {
    <<interface>>
    +Validate(ParsedDocument) QualityReport
  }
  class IRecipientMatcher {
    <<interface>>
    +ProposeMatches(SourceRecipientOccurrence) MatchCandidates
  }
  class IComponentMapper {
    <<interface>>
    +ProposeMappings(SourceFinancialOccurrence) MappingCandidates
  }
  class IFinancialRuleResolver {
    <<interface>>
    +Resolve(PayeeRelationship, component, period) RuleEvidence
  }
  class IExpectedPayrollCalculator {
    <<interface>>
    +Propose(ApprovedFacts, Rules, Period) CalculationSnapshot
  }
  class IReconciliationService {
    <<interface>>
    +Compare(SourceSnapshot, ExpectedSnapshot) FindingCandidates
  }
  class IReviewWorkflowService {
    <<interface>>
    +RecordDecision(case, actor, evidence) ReviewRevision
    +RequestAmendment(approvedCase, cause) PendingAmendment
  }
  class IRetroEntitlementService {
    <<interface>>
    +RecalculateAffectedPeriods(approvedEvent) RetroProposal
  }
  class IPaymentEvidenceService {
    <<interface>>
    +VerifyPayment(proof) PaymentVerification
  }
  class IAuditTrail {
    <<interface>>
    +Append(actor, kind, refs, reason) AuditEvent
  }
  class ImportUseCase
  class ReviewUseCase
  class FinancialProposalUseCase
  class RetroReviewUseCase
  class PdfiumTextAdapter
  class PhysicalTableAdapter

  ImportUseCase ..> IPdfExtractionAdapter
  ImportUseCase ..> ISourceArchive
  ImportUseCase ..> IImportQualityGate
  ReviewUseCase ..> IRecipientMatcher
  ReviewUseCase ..> IComponentMapper
  ReviewUseCase ..> IReconciliationService
  ReviewUseCase ..> IReviewWorkflowService
  ReviewUseCase ..> IAuditTrail
  FinancialProposalUseCase ..> IFinancialRuleResolver
  FinancialProposalUseCase ..> IExpectedPayrollCalculator
  RetroReviewUseCase ..> IRetroEntitlementService
  RetroReviewUseCase ..> IPaymentEvidenceService
  PdfiumTextAdapter ..|> IPdfExtractionAdapter
  PhysicalTableAdapter ..|> IPdfExtractionAdapter
```

## من يفعل ماذا؟

| الخدمة | مسؤوليتها | ما لا يجوز أن تفعله |
| --- | --- | --- |
| \`ImportUseCase\` | حفظ PDF وبصمته وتحويله إلى أسطر مصدرية قابلة للفحص | تغيير المرتب المعتمد أو درجة الموظف |
| \`IRecipientMatcher\` | اقتراح ربط مستفيد مصدر بهوية داخلية مع الأدلة | اعتماد الشخص لمجرد تشابه الأسماء |
| \`IComponentMapper\` | اقتراح بند داخلي من الكود والاسم والجانب والسياق والتاريخ | دمج كود متكرر أو تغيير الاسم الخام دون سجل |
| \`IFinancialRuleResolver\` | تحديد القاعدة الأصلية المتحقق منها والفترة والكادر | اختراع نسبة أو قيمة الأساسي المرجعي |
| \`IExpectedPayrollCalculator\` | حساب مستقل **مقترح** لكل بند وفترة مع أثر حساب | إعادة حساب البنود التي قبلها المالك من المركزية |
| \`IReconciliationService\` | اكتشاف الفروق والجمع والمجموعات البنكية والتصنيف | اعتماد موظف من تطابق الصافي فقط |
| \`IReviewWorkflowService\` | اتخاذ قرارات المراجعين واعتماد نسخ مستقلة وإعادة فتحها | تعديل Oracle أو حذف الاعتماد السابق |
| \`IRetroEntitlementService\` | حساب الفروق بأثر رجعي حسب أصل الشهر والبند | افتراض أن متجمدات الموظف صُرفت |
| \`IPaymentEvidenceService\` | التحقق من دليل التسوية وتخصيص المدفوعات | الخلط بين كشف الصرف والدفع المؤكد |
| \`IAuditTrail\` | تسجيل الفاعل والوقت والسبب وقيمة الحالة قبل وبعد | السماح بمحو تاريخ القرار |

## أنواع القيمة المقترحة
\`Money(decimal Amount,Currency)\`، \`Period(Year,Month)\`، \`EffectiveInterval\`، \`EvidenceRef(DocumentId,Page,Row,Cell)\`، \`ExternalAccountCode\`، \`SourceLineId\`، \`ComponentId\`، \`RuleTraceRef\`.

سياسة البند \`IndependentCalculation\` أو \`CentralSourceAmount\` أو \`ReferenceOnly\` أو \`ExceptionalCheck\`، مع تاريخ ونطاق اعتماد. افصل حالات **جودة القراءة** عن **ربط البند** عن **المراجعة** عن **إثبات التسوية** ولا تختصرها في متغير \`IsApproved\` واحد.

## اتجاه الاعتمادات
\`Domain\` لا يعتمد على PDFium أو SQL أو Angular. توضع العقود في \`Application\`، وتنفيذ قارئ PDF/التخزين في \`Infrastructure\`. المبلغ \`decimal\` مع تقريب صريح؛ المدخلات التاريخية والحسابات محفوظة بإصدارات.

## مثال قابل للفهم
[الطلب والاستجابة والمخطط المبسط في مثال UML](../examples/02-UML-REVIEW-EXAMPLES.md) يعرض الكلاسات أثناء استيراد صرفية فيها نفس الكود لبدلين مختلفين.
