namespace PayrollReviewSystem.Core.ReviewVariables;

/// <summary>
/// معرّف فريد لمتغير المراجعة داخل النواة.
/// يغلّف <see cref="Guid"/> لمنع الخلط بين المعرّفات المختلفة أثناء الترجمة والتشغيل.
/// </summary>
public readonly record struct ReviewVariableId
{
    /// <summary>القيمة الداخلية للمعرّف.</summary>
    public Guid Value { get; }

    private ReviewVariableId(Guid value) => Value = value;

    /// <summary>ينشئ معرّفًا جديدًا لمتغير مراجعة.</summary>
    public static ReviewVariableId NewId() => new(Guid.NewGuid());

    /// <summary>يحاول إنشاء المعرّف من قيمة Guid صالحة غير فارغة.</summary>
    public static bool TryCreate(Guid value, out ReviewVariableId id)
    {
        if (value == Guid.Empty)
        {
            id = default;
            return false;
        }

        id = new ReviewVariableId(value);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}

/// <summary>
/// معرّف حالة المراجعة (AuditCase) المرتبطة بالمتغير دون حمل منطق اعتماد الحالة نفسه في هذه الشريحة.
/// </summary>
public sealed record AuditCaseId
{
    /// <summary>النص المعياري للمعرّف الاصطناعي لحالة المراجعة.</summary>
    public string Value { get; }

    private AuditCaseId(string value) => Value = value;

    /// <summary>يتحقق من أن المعرّف غير فارغ وينشئه بصيغة منقّحة.</summary>
    public static bool TryCreate(string? rawValue, out AuditCaseId? result)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            result = null;
            return false;
        }

        result = new AuditCaseId(rawValue.Trim());
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// مرجع الصرفية (رئيسية أو منفصلة) التي يرتبط بها متغير المراجعة.
/// لا يحمل هذا المرجع أي بيانات بنكية أو أرقام قومية.
/// </summary>
public sealed record PayrollDisbursementRef
{
    /// <summary>المعرّف المرجعي للصرفية.</summary>
    public string Value { get; }

    private PayrollDisbursementRef(string value) => Value = value;

    /// <summary>ينشئ مرجع الصرفية بعد التحقق من أنه نص غير فارغ.</summary>
    public static bool TryCreate(string? rawValue, out PayrollDisbursementRef? result)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            result = null;
            return false;
        }

        result = new PayrollDisbursementRef(rawValue.Trim());
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// المفتاح التعريفي لمتغير المراجعة (مثل اسم بند أو معامل مراجعة داخل الصرفية).
/// </summary>
public sealed record VariableKey
{
    /// <summary>النص المعياري لمفتاح المتغير.</summary>
    public string Value { get; }

    private VariableKey(string value) => Value = value;

    /// <summary>يتحقق من صلاحية مفتاح المتغير (غير فارغ ومطبّع بدون مسافات طرفية).</summary>
    public static bool TryCreate(string? rawValue, out VariableKey? result)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            result = null;
            return false;
        }

        result = new VariableKey(rawValue.Trim());
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// هوية الفاعل (المراجع) المشتقة من سياق الخادم الموثوق فقط.
/// </summary>
public sealed record ActorId
{
    /// <summary>المعرّف النصي لهوية المراجع.</summary>
    public string Value { get; }

    private ActorId(string value) => Value = value;

    /// <summary>ينشئ هوية فاعل صالحة غير فارغة.</summary>
    public static bool TryCreate(string? rawValue, out ActorId? result)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            result = null;
            return false;
        }

        result = new ActorId(rawValue.Trim());
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// معرّف الارتباط (CorrelationId) لتتبع تسلسل العملية عبر السجلات دون كشف بيانات شخصية حساسة.
/// </summary>
public sealed record CorrelationId
{
    /// <summary>قيمة معرّف الارتباط.</summary>
    public string Value { get; }

    private CorrelationId(string value) => Value = value;

    /// <summary>ينشئ معرّف ارتباط جديدًا فريدًا.</summary>
    public static CorrelationId NewId() => new(Guid.NewGuid().ToString("N"));

    /// <summary>يحاول إنشاء معرّف ارتباط من نص غير فارغ.</summary>
    public static bool TryCreate(string? rawValue, out CorrelationId? result)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            result = null;
            return false;
        }

        result = new CorrelationId(rawValue.Trim());
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>معرّف فريد لكل نسخة غير قابلة للتعديل من متغير المراجعة.</summary>
public readonly record struct ReviewVariableRevisionId(Guid Value)
{
    /// <summary>يولّد معرّف نسخة جديدًا.</summary>
    public static ReviewVariableRevisionId NewId() => new(Guid.NewGuid());
}

/// <summary>معرّف فريد لكل سجل فعل في سجل تدقيق المراجعين (ReviewerActionLog).</summary>
public readonly record struct ReviewerActionId(Guid Value)
{
    /// <summary>يولّد معرّف سجل فعل جديدًا.</summary>
    public static ReviewerActionId NewId() => new(Guid.NewGuid());
}
