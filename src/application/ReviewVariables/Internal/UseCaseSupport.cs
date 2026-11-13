using PayrollReviewSystem.Application.ReviewVariables.Contracts;
using PayrollReviewSystem.Application.ReviewVariables.Ports;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.Internal;

/// <summary>
/// أدوات مساعدة داخلية مشتركة لمنع تكرار منطق التحقق من هوية الفاعل، والمصادر، وبناء الإيصالات عبر حالات الاستخدام (DRY).
/// </summary>
internal static class UseCaseSupport
{
    /// <summary>
    /// يستخرج هوية المراجع الموثوقة من الخادم فقط، ويرفض الطلب إذا كانت الجلسة غائبة أو انتحل الطلب هوية مراجع آخر.
    /// </summary>
    public static ApplicationResult<TrustedReviewerContext> ResolveAndVerifyTrustedActor(
        IReviewerContextPort reviewerContextPort,
        string? claimedActorId)
    {
        var context = reviewerContextPort.GetCurrentReviewer();
        if (context is null)
        {
            return ApplicationResult<TrustedReviewerContext>.Failure(
                ApplicationErrorCode.PermissionDenied,
                "لم يتم التحقق من هوية المراجع في سياق الخادم الموثوق.");
        }

        // في حالة إرسال العميل معرّف فاعل لا يطابق هوية الجلسة الموثوقة: رفض صريح لمنع انتحال الفاعل
        if (!string.IsNullOrWhiteSpace(claimedActorId) &&
            !string.Equals(claimedActorId.Trim(), context.ActorId.Value, StringComparison.Ordinal))
        {
            return ApplicationResult<TrustedReviewerContext>.Failure(
                ApplicationErrorCode.AttributionMismatchRejected,
                "هوية المراجع المرسلة في الطلب لا تطابق هوية الجلسة الموثوقة على الخادم؛ تم رفض نسبة الفعل.");
        }

        return ApplicationResult<TrustedReviewerContext>.Success(context);
    }

    /// <summary>
    /// يتحقق من مدخل قيمة متغير المراجعة ويحوله إلى كائن النواة الحتمي أو يرجع خطأ مصنفًا.
    /// </summary>
    public static ApplicationResult<ReviewVariableValue> ValidateValueInput(ReviewVariableValueInput? input)
    {
        if (input is null)
        {
            return ApplicationResult<ReviewVariableValue>.Failure(
                ApplicationErrorCode.InputMissing,
                "مدخل قيمة المتغير مفقود؛ مرّر قيمة عشرية معلومة أو Unknown() صراحةً.");
        }

        if (input.ValidatedCoreValue is null)
        {
            return ApplicationResult<ReviewVariableValue>.Failure(
                ApplicationErrorCode.InvalidVariableValue,
                input.ValidationError ?? "قيمة متغير المراجعة غير صالحة.");
        }

        return ApplicationResult<ReviewVariableValue>.Success(input.ValidatedCoreValue);
    }

    /// <summary>
    /// يتحقق من مدخل المصدر الاختياري ويحوله إلى <see cref="SourceReference"/> مع حفظ نوع القناة بدقة دون ترقيتها إلى Oracle.
    /// </summary>
    public static bool TryConvertSource(
        SourceReferenceInput? input,
        out SourceReference? sourceReference,
        out ApplicationError? error)
    {
        if (input is null)
        {
            sourceReference = null;
            error = null;
            return true;
        }

        if (!SourceReference.TryCreate(input.Kind, input.SourceVersionId, input.SourceCaptureRef, out sourceReference))
        {
            error = new ApplicationError(
                ApplicationErrorCode.InvalidVariableValue,
                "مرجع المصدر غير صالح: يجب تحديد نوع القناة ومعرّف نسخة المصدر.");
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// يبني أو يتحقق من معرّف الارتباط التتبعي للعملية.
    /// </summary>
    public static CorrelationId ResolveCorrelationId(string? rawCorrelationId)
        => CorrelationId.TryCreate(rawCorrelationId, out var parsed) && parsed is not null
            ? parsed
            : CorrelationId.NewId();

    /// <summary>يحوّل قيمة النواة إلى لقطة قراءة آمنة للمخرجات.</summary>
    public static ReviewVariableValueSnapshot ToSnapshot(ReviewVariableValue value)
        => new(value.IsKnown, value.Amount);

    /// <summary>يحوّل مرجع المصدر إلى لقطة قراءة آمنة تظهر هل المصدر كشف أوراكل رسمي أم لا.</summary>
    public static SourceReferenceSnapshot? ToSnapshot(SourceReference? source)
        => source is null
            ? null
            : new SourceReferenceSnapshot(
                source.Kind,
                source.SourceVersionId,
                source.SourceCaptureRef,
                source.IsOfficialOracle);

    /// <summary>
    /// يبني إيصال المخرج القياسي بعد حفظ متغير المراجعة وسجل الفعل ذريًا.
    /// </summary>
    public static ReviewVariableReceipt BuildReceipt(
        ReviewVariable variable,
        ReviewVariableRevision revision,
        ReviewerActionLogEntry actionEntry)
        => new(
            variable.Id.Value,
            variable.AuditCaseId.Value,
            variable.DisbursementRef.Value,
            variable.Key.Value,
            variable.Scope.UnitId,
            variable.Scope.ReviewGroupId,
            variable.Version,
            ToSnapshot(variable.CurrentValue),
            ToSnapshot(variable.CurrentSourceReference),
            actionEntry.PerformedByActorId.Value,
            actionEntry.OccurredAtUtc,
            revision.RevisionId.Value,
            actionEntry.ActionId.Value,
            actionEntry.CorrelationId.Value);

    /// <summary>يحوّل فشل النواة إلى خطأ تطبيق قياسي.</summary>
    public static ApplicationResult<T> MapDomainFailure<T>(
        DomainFailureCode? code,
        string? message,
        int? currentVersion,
        int? expectedVersion = null)
    {
        var errorCode = code switch
        {
            DomainFailureCode.VersionConflict => ApplicationErrorCode.VersionConflict,
            DomainFailureCode.ScopeMismatch => ApplicationErrorCode.PermissionDenied,
            DomainFailureCode.InputMissing => ApplicationErrorCode.InputMissing,
            _ => ApplicationErrorCode.InvalidVariableValue
        };

        var conflictInfo = code == DomainFailureCode.VersionConflict && currentVersion.HasValue
            ? new VersionConflictInfo(expectedVersion ?? 0, currentVersion.Value)
            : null;

        return ApplicationResult<T>.Failure(
            errorCode,
            message ?? "تعذر تنفيذ العملية في النواة.",
            conflictInfo);
    }
}
