using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.Contracts;

/// <summary>
/// مدخل بيانات المصدر الوصفية المرفقة بأمر إنشاء أو تعديل متغير المراجعة (Metadata فقط).
/// </summary>
public sealed record SourceReferenceInput(
    SourceKind Kind,
    string SourceVersionId,
    string? SourceCaptureRef = null);

/// <summary>
/// أمر تسجيل متغير مراجعة جديد (النسخة 1).
/// ملاحظة أمنية: يتم استخراج هوية المراجع الفعلي من <c>IReviewerContextPort</c> على الخادم،
/// وحقل <see cref="ClaimedActorId"/> اختياري للتحقق من عدم انتحال الهوية فقط وليس مصدر ثقة أبدًا.
/// </summary>
public sealed record RecordReviewVariableCommand(
    string AuditCaseId,
    string PayrollDisbursementId,
    string VariableKey,
    string UnitId,
    string ReviewGroupId,
    ReviewVariableValueInput? Value,
    string Reason,
    SourceReferenceInput? Source,
    string IdempotencyKey,
    string? CorrelationId = null,
    string? ClaimedActorId = null);

/// <summary>
/// أمر تعديل قيمة متغير مراجعة قائم برقم نسخة متوقعة (<see cref="ExpectedVersion"/>).
/// يعمل لأي مراجع مخوّل في النطاق دون اشتراط موافقة مراجع أساسي (AI-0093 / D-035).
/// </summary>
public sealed record ReviseReviewVariableCommand(
    Guid VariableId,
    int ExpectedVersion,
    ReviewVariableValueInput? NewValue,
    string ChangeReason,
    SourceReferenceInput? Source,
    string IdempotencyKey,
    string? CorrelationId = null,
    string? ClaimedActorId = null);

/// <summary>
/// استعلام قراءة السجل التاريخي الكامل لمتغير المراجعة وسجل تدقيق أفعال المراجعين (قراءة فقط).
/// </summary>
public sealed record GetReviewVariableHistoryQuery(
    Guid VariableId,
    string? ClaimedActorId = null);

/// <summary>
/// تمثيل قراءة غير قابل للتعديل لقيمة متغير المراجعة يظهر بوضوح الفرق بين المعلوم (<c>IsKnown = true</c>) والمجهول.
/// </summary>
public sealed record ReviewVariableValueSnapshot(bool IsKnown, decimal? Amount);

/// <summary>
/// تمثيل قراءة غير قابل للتعديل لمرجع المصدر مع إظهار عدم رسمية Excel/DirectForm مقارنةً بـ Oracle.
/// </summary>
public sealed record SourceReferenceSnapshot(
    SourceKind Kind,
    string SourceVersionId,
    string? SourceCaptureRef,
    bool IsOfficialOracle);

/// <summary>
/// إيصال مخرج موحد لعمليتَي التسجيل والتعديل يعكس الحالة الفعلية بعد الحفظ الذري ويُخزن لإعادات Idempotency.
/// </summary>
public sealed record ReviewVariableReceipt(
    Guid VariableId,
    string AuditCaseId,
    string PayrollDisbursementId,
    string VariableKey,
    string UnitId,
    string ReviewGroupId,
    int Version,
    ReviewVariableValueSnapshot CurrentValue,
    SourceReferenceSnapshot? SourceReference,
    string PerformedByActorId,
    DateTimeOffset OccurredAtUtc,
    Guid RevisionId,
    Guid ActionId,
    string CorrelationId);

/// <summary>
/// عنصر نسخة تاريخية في استعلام السجل التاريخي للمتغير.
/// </summary>
public sealed record ReviewVariableRevisionDto(
    Guid RevisionId,
    int RevisionNumber,
    ReviewVariableValueSnapshot? PreviousValue,
    ReviewVariableValueSnapshot NewValue,
    string PerformedByActorId,
    DateTimeOffset OccurredAtUtc,
    string Reason,
    SourceReferenceSnapshot? SourceReference,
    string CorrelationId);

/// <summary>
/// عنصر سجل فعل في <c>ReviewerActionLog</c> يعرض من نفّذ كل خطوة ومتى ولماذا دون أي بيانات شخصية حساسة.
/// </summary>
public sealed record ReviewerActionLogDto(
    Guid ActionId,
    int TargetVersion,
    ReviewVariableOperation Operation,
    string PerformedByActorId,
    string UnitId,
    string ReviewGroupId,
    DateTimeOffset OccurredAtUtc,
    ReviewVariableValueSnapshot? OldValue,
    ReviewVariableValueSnapshot NewValue,
    string Reason,
    SourceReferenceSnapshot? SourceReference,
    string CorrelationId);

/// <summary>
/// النتيجة الكاملة لاستعلام تاريخ متغير المراجعة وسجل أفعال المراجعين.
/// </summary>
public sealed record ReviewVariableHistoryDto(
    Guid VariableId,
    string AuditCaseId,
    string PayrollDisbursementId,
    string VariableKey,
    string UnitId,
    string ReviewGroupId,
    int CurrentVersion,
    ReviewVariableValueSnapshot CurrentValue,
    SourceReferenceSnapshot? CurrentSourceReference,
    IReadOnlyList<ReviewVariableRevisionDto> Revisions,
    IReadOnlyList<ReviewerActionLogDto> ActionLogs);
