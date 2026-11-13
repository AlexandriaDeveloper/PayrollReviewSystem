namespace PayrollReviewSystem.Core.ReviewVariables;

/// <summary>
/// سجل تدقيق غير مدمّر (Append-only) يوثّق كل فعل فعلي قام به مراجع مخوّل على متغير مراجعة.
/// يتوافق مع متطلب المساءلة الفردية في D-035 / AI-0093: كل تعديل يُنسب لمنفذه الفعلي وتوقيته دون انتظار مراجع أساسي
/// ودون احتواء أي بيانات موظفين حساسة (No PII).
/// </summary>
public sealed record ReviewerActionLogEntry
{
    /// <summary>المعرّف الفريد لسجل الفعل.</summary>
    public ReviewerActionId ActionId { get; }

    /// <summary>معرّف متغير المراجعة المستهدف بهذه العملية.</summary>
    public ReviewVariableId TargetVariableId { get; }

    /// <summary>رقم النسخة الناتج بعد العملية.</summary>
    public int TargetVersion { get; }

    /// <summary>نوع العملية المنفذة (<see cref="ReviewVariableOperation"/>).</summary>
    public ReviewVariableOperation Operation { get; }

    /// <summary>هوية المراجع الفعلي المشتقة من سياق الخادم الموثوق.</summary>
    public ActorId PerformedByActorId { get; }

    /// <summary>نطاق الوحدة والمجموعة الذي جرت داخله العملية.</summary>
    public ActorScope Scope { get; }

    /// <summary>توقيت تنفيذ العملية بصيغة UTC.</summary>
    public DateTimeOffset OccurredAtUtc { get; }

    /// <summary>القيمة قبل التنفيذ (<c>null</c> عند الإنشاء الأول).</summary>
    public ReviewVariableValue? OldValue { get; }

    /// <summary>القيمة بعد التنفيذ.</summary>
    public ReviewVariableValue NewValue { get; }

    /// <summary>السبب الموثق للإجراء.</summary>
    public string Reason { get; }

    /// <summary>بيانات المصدر الوصفية المرفقة بالعملية إن وُجدت (Metadata فقط).</summary>
    public SourceReference? SourceReference { get; }

    /// <summary>معرّف الارتباط التتبعي للعملية.</summary>
    public CorrelationId CorrelationId { get; }

    private ReviewerActionLogEntry(
        ReviewerActionId actionId,
        ReviewVariableId targetVariableId,
        int targetVersion,
        ReviewVariableOperation operation,
        ActorId performedByActorId,
        ActorScope scope,
        DateTimeOffset occurredAtUtc,
        ReviewVariableValue? oldValue,
        ReviewVariableValue newValue,
        string reason,
        SourceReference? sourceReference,
        CorrelationId correlationId)
    {
        ActionId = actionId;
        TargetVariableId = targetVariableId;
        TargetVersion = targetVersion;
        Operation = operation;
        PerformedByActorId = performedByActorId;
        Scope = scope;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
        OldValue = oldValue;
        NewValue = newValue;
        Reason = reason;
        SourceReference = sourceReference;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// ينشئ سجل فعل تدقيقيًا جديدًا لعملية إنشاء أو تعديل ناجحة، بعد التحقق من وجود السبب وهوية الفاعل.
    /// </summary>
    public static ReviewerActionLogEntry Create(
        ReviewVariableId targetVariableId,
        int targetVersion,
        ReviewVariableOperation operation,
        ActorId performedByActorId,
        ActorScope scope,
        DateTimeOffset occurredAtUtc,
        ReviewVariableValue? oldValue,
        ReviewVariableValue newValue,
        string reason,
        SourceReference? sourceReference,
        CorrelationId correlationId,
        ReviewerActionId? explicitActionId = null)
    {
        if (targetVersion < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(targetVersion), "رقم النسخة المستهدفة يجب أن يكون 1 على الأقل.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("سبب الإجراء مطلوب لتسجيل سجل التدقيق.", nameof(reason));
        }

        return new ReviewerActionLogEntry(
            explicitActionId ?? ReviewerActionId.NewId(),
            targetVariableId,
            targetVersion,
            operation,
            performedByActorId,
            scope,
            occurredAtUtc,
            oldValue,
            newValue,
            reason.Trim(),
            sourceReference,
            correlationId);
    }
}
