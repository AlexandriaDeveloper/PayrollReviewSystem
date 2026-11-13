namespace PayrollReviewSystem.Application.ReviewVariables.Contracts;

/// <summary>
/// رموز الأخطاء القياسية التي تعيدها حالات الاستخدام في طبقة التطبيق دون ابتلاع الاستثناءات أو إرجاع نجاح وهمي.
/// </summary>
public enum ApplicationErrorCode
{
    /// <summary>رفض الوصول: المراجع غير موثق في الجلسة أو خارج نطاق وحدته/مجموعته.</summary>
    PermissionDenied = 1,

    /// <summary>تضارب نسبة الفاعل: محاولة تمرير هوية مراجع مختلفة عن هوية جلسة الخادم الموثوقة (Anti-Spoofing).</summary>
    AttributionMismatchRejected = 2,

    /// <summary>تعارض تزامن: النسخة المتوقعة (<c>ExpectedVersion</c>) قديمة مقارنةً بالنسخة المحفوظة.</summary>
    VersionConflict = 3,

    /// <summary>تعارض عدم التكرار: إعادة استخدام <c>IdempotencyKey</c> نفسه بحمولة طلب مختلفة.</summary>
    IdempotencyConflict = 4,

    /// <summary>قيمة أو مدخل غير صالح (مثل رقم عائم غير حتمي أو سبب/مفتاح فارغ أو صيغة رقمية مشوهة).</summary>
    InvalidVariableValue = 5,

    /// <summary>مدخل أو مرجع مطلوب غير متوفر.</summary>
    InputMissing = 6,

    /// <summary>متغير المراجعة المطلوب غير موجود.</summary>
    NotFound = 7,

    /// <summary>محاولة إنشاء متغير مكرر لنفس الحالة والصرفية والمفتاح؛ استخدم التعديل برقم النسخة بدلًا منه.</summary>
    DuplicateVariable = 8,

    /// <summary>فشل ذري في طبقة التخزين؛ تم التراجع لضمان عدم بقاء سجل ناقص.</summary>
    PersistenceFailure = 9,

    /// <summary>عملية غير مدعومة ضمن نطاق هذه الشريحة (مثل تعديل مبالغ السداد البنكي أو Oracle).</summary>
    OperationNotSupported = 10
}

/// <summary>
/// تفاصيل تعارض النسخ عند رفض التعديل بسبب تحديث متزامن لمراجع آخر.
/// قيمة <see cref="ActualVersion"/> تكون <c>null</c> إذا تعذر على طبقة التخزين تحديدها بيقين (Q-06).
/// </summary>
public sealed record VersionConflictInfo(int ExpectedVersion, int? ActualVersion);

/// <summary>
/// كائن خطأ مصنف يحمل الرمز القياسي والرسالة التفسيرية باللغة العربية وتفاصيل التعارض إن وجدت.
/// </summary>
public sealed record ApplicationError(
    ApplicationErrorCode Code,
    string Message,
    VersionConflictInfo? ConflictInfo = null);

/// <summary>
/// غلاف نتيجة موحد لحالات الاستخدام في طبقة التطبيق (<c>Result&lt;T&gt;</c>).
/// يميّز بوضوح بين التنفيذ الجديد، والإعادة المتطابقة (<see cref="IsReplayed"/>)، وحالات الفشل المصنفة.
/// </summary>
public sealed class ApplicationResult<T>
{
    /// <summary>تشير إلى نجاح العملية أو إعادة نتيجتها المحفوظة.</summary>
    public bool IsSuccess { get; }

    /// <summary>تشير إلى أن النتيجة معادة عبر <c>Idempotent Replay</c> دون تنفيذ كتابة أو تاريخ جديد.</summary>
    public bool IsReplayed { get; }

    /// <summary>القيمة الناتجة عند النجاح فقط.</summary>
    public T? Value { get; }

    /// <summary>تفاصيل الخطأ المصنف عند الفشل فقط.</summary>
    public ApplicationError? Error { get; }

    private ApplicationResult(bool isSuccess, bool isReplayed, T? value, ApplicationError? error)
    {
        IsSuccess = isSuccess;
        IsReplayed = isReplayed;
        Value = value;
        Error = error;
    }

    /// <summary>ينشئ نتيجة نجاح لتنفيذ فعلي جديد.</summary>
    public static ApplicationResult<T> Success(T value)
        => new(isSuccess: true, isReplayed: false, value: value, error: null);

    /// <summary>ينشئ نتيجة نجاح معادة من سجل عدم التكرار (Idempotent Replay).</summary>
    public static ApplicationResult<T> Replayed(T value)
        => new(isSuccess: true, isReplayed: true, value: value, error: null);

    /// <summary>ينشئ نتيجة فشل مصنفة غير قابلة للابتلاع.</summary>
    public static ApplicationResult<T> Failure(
        ApplicationErrorCode code,
        string message,
        VersionConflictInfo? conflictInfo = null)
        => new(isSuccess: false, isReplayed: false, value: default, error: new ApplicationError(code, message, conflictInfo));
}

