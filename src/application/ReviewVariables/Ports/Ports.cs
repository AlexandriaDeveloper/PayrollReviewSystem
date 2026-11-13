using PayrollReviewSystem.Application.ReviewVariables.Contracts;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.Ports;

/// <summary>
/// سياق المراجع الموثق المستخرج من جلسة الخادم فقط (وليس من جسم الطلب القادم من العميل).
/// لا يشترط وجود صفة «المراجع الأساسي» للسماح بالتعديل (AI-0093 / D-035).
/// </summary>
public sealed record TrustedReviewerContext(
    ActorId ActorId,
    string RoleName,
    IReadOnlySet<ActorScope> AuthorizedScopes,
    bool CanModifyReviewVariables);

/// <summary>
/// نتيجة فحص الصلاحية لعملية محددة ضمن نطاق محدد.
/// </summary>
public sealed record AuthorizationDecision(bool IsAllowed, string? DenialReason = null)
{
    /// <summary>قرار تفويض بالسماح.</summary>
    public static AuthorizationDecision Allow() => new(true, null);

    /// <summary>قرار تفويض بالرفض مع السبب.</summary>
    public static AuthorizationDecision Deny(string reason) => new(false, reason);
}

/// <summary>
/// منفذ الحصول على هوية المراجع الموثوقة من بيئة التشغيل/الخادم.
/// يرجع <c>null</c> إذا كانت الجلسة غير موثقة أو مفقودة.
/// </summary>
public interface IReviewerContextPort
{
    /// <summary>يرجع سياق المراجع الموثق الحالي أو <c>null</c> عند غياب المصادقة.</summary>
    TrustedReviewerContext? GetCurrentReviewer();
}

/// <summary>
/// منفذ سياسة التحقق من صلاحية المراجع لتنفيذ عملية على نطاق وحدة/مجموعة محدد.
/// </summary>
public interface IAuthorizationPolicyPort
{
    /// <summary>يقيم ما إذا كان المراجع مخوّلًا لتنفيذ العملية المطلوبة داخل النطاق المستهدف.</summary>
    AuthorizationDecision Evaluate(
        TrustedReviewerContext context,
        ReviewVariableOperation operation,
        ActorScope targetScope);
}

/// <summary>
/// منفذ الساعة الحتمية لتوحيد توقيت UTC دون الاعتماد المباشر على ساعة النظام داخل الاختبارات.
/// </summary>
public interface IClockPort
{
    /// <summary>التوقيت الحالي بصيغة UTC.</summary>
    DateTimeOffset UtcNow { get; }
}

/// <summary>
/// نتيجة محاولة حفظ نسخة جديدة من متغير المراجعة في المستودع.
/// </summary>
public enum RepositorySaveStatus
{
    /// <summary>تم الحفظ بنجاح.</summary>
    Saved = 1,

    /// <summary>تعارض تزامن: تم تعديل النسخة في التخزين قبل إتمام الحفظ.</summary>
    VersionConflict = 2
}

/// <summary>
/// منفذ مستودع متغيرات المراجعة (مجرد عن أي تقنية قاعدة بيانات أو ORM).
/// </summary>
public interface IReviewVariableRepository
{
    /// <summary>يجلب متغير المراجعة بالمعرّف الفريد مع نسخه التاريخية أو <c>null</c> إن لم يوجد.</summary>
    Task<ReviewVariable?> GetByIdAsync(ReviewVariableId id, CancellationToken cancellationToken = default);

    /// <summary>يبحث عن المتغير بالمفتاح الطبيعي (الحالة + الصرفية + مفتاح المتغير) لمنع الازدواج عند الإنشاء.</summary>
    Task<ReviewVariable?> GetByNaturalKeyAsync(
        AuditCaseId auditCaseId,
        PayrollDisbursementRef disbursementRef,
        VariableKey key,
        CancellationToken cancellationToken = default);

    /// <summary>يحفظ متغير مراجعة جديدًا بالنسخة 1.</summary>
    Task SaveNewAsync(ReviewVariable variable, CancellationToken cancellationToken = default);

    /// <summary>يحفظ تعديلًا جديدًا على المتغير مع التحقق من تطابق <paramref name="expectedVersion"/>.</summary>
    Task<RepositorySaveStatus> SaveRevisionAsync(
        ReviewVariable variable,
        ReviewVariableRevision newRevision,
        int expectedVersion,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// منفذ سجل تدقيق أفعال المراجعين (Append-only؛ لا يدعم الحذف أو التعديل الصامت).
/// </summary>
public interface IAuditTrailPort
{
    /// <summary>يلحق سجل فعل جديدًا غير قابل للتعديل.</summary>
    Task AppendAsync(ReviewerActionLogEntry entry, CancellationToken cancellationToken = default);

    /// <summary>يرجع جميع أفعال التدقيق المرتبطة بمتغير مراجعة محدد مرتبة زمنيًا.</summary>
    Task<IReadOnlyList<ReviewerActionLogEntry>> GetByVariableIdAsync(
        ReviewVariableId variableId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// مفتاح تحديد نطاق عدم التكرار (Idempotency) طبقًا للمواصفة:
/// نوع العملية + معرّف الهدف + نطاق العمل + مفتاح الطلب.
/// </summary>
public sealed record IdempotencyRecordKey(
    ReviewVariableOperation OperationType,
    string TargetAggregateId,
    ActorScope ActorScope,
    string IdempotencyKey);

/// <summary>حالة فحص مفتاح عدم التكرار مقابل البصمة المعيارية للطلب.</summary>
public enum IdempotencyCheckStatus
{
    /// <summary>طلب جديد لم يُنفذ من قبل بهذا المفتاح والنطاق.</summary>
    NotFound = 1,

    /// <summary>إعادة مطابقة: نفس المفتاح والنطاق ونفس بصمة الحمولة؛ تُعاد النتيجة الأصلية دون أثر ثانٍ.</summary>
    MatchedReplay = 2,

    /// <summary>تعارض: نفس المفتاح والنطاق مع بصمة حمولة مختلفة.</summary>
    PayloadMismatchConflict = 3
}

/// <summary>نتيجة فحص سجل عدم التكرار شاملةً الإيصال المحفوظ عند التطابق.</summary>
public sealed record IdempotencyCheckResult(
    IdempotencyCheckStatus Status,
    ReviewVariableReceipt? StoredReceipt = null);

/// <summary>
/// منفذ التحقق من عدم التكرار وحفظ إيصالات العمليات الناجحة.
/// </summary>
public interface IIdempotencyPort
{
    /// <summary>يفحص ما إذا كان الطلب معادًا أم متعارضًا أم جديدًا.</summary>
    Task<IdempotencyCheckResult> CheckAsync(
        IdempotencyRecordKey key,
        string requestPayloadHash,
        CancellationToken cancellationToken = default);

    /// <summary>يحفظ بصمة الطلب ونتيجته النهائية داخل نفس حدود الذرية للعملية الناجحة.</summary>
    Task SaveAsync(
        IdempotencyRecordKey key,
        string requestPayloadHash,
        ReviewVariableReceipt receipt,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// عقد وحدة الذرية (Atomic Transaction Boundary) لضمان عدم نجاح حفظ تعديل المتغير
/// إذا فشل حفظ سجل التدقيق (<see cref="IAuditTrailPort"/>) أو سجل عدم التكرار (<see cref="IIdempotencyPort"/>).
/// يتم تنفيذه لاحقًا عبر قاعدة البيانات في Infrastructure عند التفويض، ويُحاكي التراجع الكامل في اختبارات الوحدة.
/// </summary>
public interface IReviewAtomicTransactionPort
{
    /// <summary>ينفذ كتلة الكتابة ذريًا بحيث تُحفظ كل الأجزاء معًا أو يُتراجع عنها بالكامل عند أي فشل.</summary>
    Task<TResult> ExecuteAtomicallyAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default);
}
