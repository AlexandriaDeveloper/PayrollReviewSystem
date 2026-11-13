namespace PayrollReviewSystem.Core.ReviewVariables;

/// <summary>
/// نسخة تاريخية غير قابلة للتعديل (Immutable) من متغير المراجعة.
/// تحفظ القيمة السابقة والجديدة ومن نفّذ التغيير ومتى وبأي سبب أو مصدر، دون محو أي نسخة سابقة.
/// </summary>
public sealed record ReviewVariableRevision
{
    /// <summary>المعرّف الفريد لهذه النسخة.</summary>
    public ReviewVariableRevisionId RevisionId { get; }

    /// <summary>معرّف متغير المراجعة المالك لهذه النسخة.</summary>
    public ReviewVariableId VariableId { get; }

    /// <summary>رقم النسخة التسلسلي (يبدأ من 1 عند الإنشاء ويزيد بمقدار 1 مع كل تعديل).</summary>
    public int RevisionNumber { get; }

    /// <summary>القيمة قبل هذا التغيير؛ وتكون <c>null</c> عند الإنشاء الأول للمتغير فقط.</summary>
    public ReviewVariableValue? PreviousValue { get; }

    /// <summary>القيمة الجديدة الناتجة عن هذا الإدخال أو التعديل.</summary>
    public ReviewVariableValue NewValue { get; }

    /// <summary>هوية المراجع الفعلي الذي نفّذ هذا الإدخال أو التعديل (وفق سياق الخادم الموثوق).</summary>
    public ActorId PerformedByActorId { get; }

    /// <summary>التاريخ والوقت الموحد بصيغة UTC عند حدوث العملية.</summary>
    public DateTimeOffset OccurredAtUtc { get; }

    /// <summary>السبب الموثق للإدخال أو التعديل.</summary>
    public string Reason { get; }

    /// <summary>بيانات المصدر الوصفية المرفقة بهذه النسخة إن وُجدت (Metadata فقط).</summary>
    public SourceReference? SourceReference { get; }

    /// <summary>معرّف الارتباط لتتبع العملية دون كشف أي بيانات موظفين حساسة.</summary>
    public CorrelationId CorrelationId { get; }

    /// <summary>
    /// منشئ داخلي مضبوط يضمن استيفاء جميع ثوابت النسخة قبل إنشائها.
    /// </summary>
    internal ReviewVariableRevision(
        ReviewVariableRevisionId revisionId,
        ReviewVariableId variableId,
        int revisionNumber,
        ReviewVariableValue? previousValue,
        ReviewVariableValue newValue,
        ActorId performedByActorId,
        DateTimeOffset occurredAtUtc,
        string reason,
        SourceReference? sourceReference,
        CorrelationId correlationId)
    {
        RevisionId = revisionId;
        VariableId = variableId;
        RevisionNumber = revisionNumber;
        PreviousValue = previousValue;
        NewValue = newValue;
        PerformedByActorId = performedByActorId;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
        Reason = reason;
        SourceReference = sourceReference;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// يعيد بناء كائن نسخة تاريخية موجودة مسبقًا (مثل القراءة من مستودع تخزين مستقبلي).
    /// </summary>
    public static ReviewVariableRevision Reconstitute(
        ReviewVariableRevisionId revisionId,
        ReviewVariableId variableId,
        int revisionNumber,
        ReviewVariableValue? previousValue,
        ReviewVariableValue newValue,
        ActorId performedByActorId,
        DateTimeOffset occurredAtUtc,
        string reason,
        SourceReference? sourceReference,
        CorrelationId correlationId)
    {
        if (revisionId.Value == Guid.Empty)
        {
            throw new ArgumentException("معرّف النسخة التاريخية لا يجوز أن يكون فارغًا.", nameof(revisionId));
        }

        if (variableId.Value == Guid.Empty)
        {
            throw new ArgumentException("معرّف متغير المراجعة للنسخة التاريخية لا يجوز أن يكون فارغًا.", nameof(variableId));
        }

        if (revisionNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(revisionNumber), "رقم النسخة التاريخية يجب أن يكون 1 على الأقل.");
        }

        ArgumentNullException.ThrowIfNull(newValue);
        ArgumentNullException.ThrowIfNull(performedByActorId);
        ArgumentNullException.ThrowIfNull(correlationId);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("سبب النسخة التاريخية لا يجوز أن يكون فارغًا.", nameof(reason));
        }

        return new(
            revisionId,
            variableId,
            revisionNumber,
            previousValue,
            newValue,
            performedByActorId,
            occurredAtUtc,
            reason.Trim(),
            sourceReference,
            correlationId);
    }
}
