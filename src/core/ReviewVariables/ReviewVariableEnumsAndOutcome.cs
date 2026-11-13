namespace PayrollReviewSystem.Core.ReviewVariables;

/// <summary>
/// العمليات المصرّح بنمذجتها على متغير المراجعة في شريحة CODE-001.
/// لا تتضمن أي عمليات اعتماد حسم (D-031) أو إثبات سداد مالي (D-033) أو أوامر تسوية (D-034).
/// </summary>
public enum ReviewVariableOperation
{
    /// <summary>إنشاء متغير مراجعة جديد لأول مرة (Version = 1).</summary>
    RecordReviewVariable = 1,

    /// <summary>تعديل قيمة متغير مراجعة قائم بإصدار نسخة جديدة غير مدمّرة (Revision + 1).</summary>
    ReviseReviewVariable = 2,

    /// <summary>عرض السجل التاريخي للنسخ وتدقيق أفعال المراجعين (قراءة فقط).</summary>
    ViewReviewVariableHistory = 3
}

/// <summary>
/// رموز أخطاء النواة المصنفة داخل طبقة الدومين (Domain).
/// </summary>
public enum DomainFailureCode
{
    /// <summary>قيمة أو مدخل غير صالح (مثل سبب فارغ أو قيمة غير مسموح بها).</summary>
    InvalidInput = 1,

    /// <summary>بيانات مطلوبة غير متوفرة.</summary>
    InputMissing = 2,

    /// <summary>تعارض تزامن: النسخة المتوقعة لا تطابق النسخة الحالية للمتغير.</summary>
    VersionConflict = 3,

    /// <summary>عدم تطابق نطاق العملية مع نطاق متغير المراجعة.</summary>
    ScopeMismatch = 4
}

/// <summary>
/// نتيجة حتمية لعمليات النواة تغني عن استخدام الاستثناءات في مسارات الرفض المتوقعة.
/// </summary>
public sealed record DomainOutcome<T>
{
    /// <summary>تشير إلى نجاح العملية في النواة.</summary>
    public bool IsSuccess { get; }

    /// <summary>القيمة الناتجة عند النجاح فقط.</summary>
    public T? Value { get; }

    /// <summary>رمز الفشل المصنف عند عدم النجاح.</summary>
    public DomainFailureCode? FailureCode { get; }

    /// <summary>وصف واضح لسبب الفشل باللغة العربية.</summary>
    public string? FailureMessage { get; }

    /// <summary>النسخة الحالية الفعلية في حالة حدوث تعارض تزامن.</summary>
    public int? CurrentVersion { get; }

    private DomainOutcome(
        bool isSuccess,
        T? value,
        DomainFailureCode? failureCode,
        string? failureMessage,
        int? currentVersion)
    {
        IsSuccess = isSuccess;
        Value = value;
        FailureCode = failureCode;
        FailureMessage = failureMessage;
        CurrentVersion = currentVersion;
    }

    /// <summary>ينشئ نتيجة نجاح تحمل المخرج المطلوب.</summary>
    public static DomainOutcome<T> Success(T value)
        => new(true, value, null, null, null);

    /// <summary>ينشئ نتيجة رفض مصنفة مع رسالة واضحة ونسخة حالية اختيارية عند تعارض التزامن.</summary>
    public static DomainOutcome<T> Failure(DomainFailureCode code, string message, int? currentVersion = null)
        => new(false, default, code, message, currentVersion);
}
