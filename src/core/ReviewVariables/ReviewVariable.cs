namespace PayrollReviewSystem.Core.ReviewVariables;

/// <summary>
/// الجذر التجميعي (Aggregate Root) لمتغير المراجعة في طبقة النواة النقية.
/// يحمي الثوابت التالية:
/// 1. التغليف الكامل: لا توجد محددات عامة (public setters)، ولا تتغير القيمة أو النسخة إلا عبر عمليات المجال المنضبطة.
/// 2. عدم وجود بوابة انتظار لمراجع أساسي (AI-0093 / D-035): أي مراجع مخوّل في النطاق يعدّل فورًا دون حجب.
/// 3. التاريخ غير المدمّر: كل تعديل يولّد نسخة جديدة <see cref="ReviewVariableRevision"/> ويزيد <see cref="Version"/>.
/// 4. التزامن المتفائل: مطابقة <c>ExpectedVersion</c> إلزامية عند التعديل لتجنب الكتابة الخفية فوق عمل مراجع آخر.
/// 5. الفصل الصارم: هذا الكيان لا يملك أي قدرة على تعديل VerifiedPaidAmount أو OutstandingAmount أو SettlementOrder أو Oracle أو الدفع البنكي.
/// </summary>
public sealed class ReviewVariable
{
    private readonly List<ReviewVariableRevision> _revisions;

    /// <summary>المعرّف الفريد لمتغير المراجعة.</summary>
    public ReviewVariableId Id { get; }

    /// <summary>معرّف حالة المراجعة التي ينتمي إليها هذا المتغير.</summary>
    public AuditCaseId AuditCaseId { get; }

    /// <summary>مرجع الصرفية المرتبط بها المتغير (رئيسية أو منفصلة).</summary>
    public PayrollDisbursementRef DisbursementRef { get; }

    /// <summary>المفتاح التعريفي للمتغير داخل الصرفية.</summary>
    public VariableKey Key { get; }

    /// <summary>نطاق الوحدة والمجموعة المالك لهذا المتغير لحماية حدود الوصول.</summary>
    public ActorScope Scope { get; }

    /// <summary>القيمة الحالية الحتمية للمتغير (تميّز صراحةً بين المعلوم والمجهول).</summary>
    public ReviewVariableValue CurrentValue { get; private set; }

    /// <summary>مرجع المصدر الوصفي الأحدث (Metadata فقط)؛ قد يكون <c>null</c> عند التعديل المباشر بلا مرفق مصدر.</summary>
    public SourceReference? CurrentSourceReference { get; private set; }

    /// <summary>رقم النسخة الحالي للمتغير لغرض حماية التزامن المتفائل.</summary>
    public int Version { get; private set; }

    /// <summary>السجل التاريخي الكامل لجميع النسخ غير القابلة للتعديل مرتبًا تصاعديًا.</summary>
    public IReadOnlyList<ReviewVariableRevision> Revisions => _revisions.AsReadOnly();

    private ReviewVariable(
        ReviewVariableId id,
        AuditCaseId auditCaseId,
        PayrollDisbursementRef disbursementRef,
        VariableKey key,
        ActorScope scope,
        ReviewVariableValue currentValue,
        SourceReference? currentSourceReference,
        int version,
        List<ReviewVariableRevision> revisions)
    {
        Id = id;
        AuditCaseId = auditCaseId;
        DisbursementRef = disbursementRef;
        Key = key;
        Scope = scope;
        CurrentValue = currentValue;
        CurrentSourceReference = currentSourceReference;
        Version = version;
        _revisions = revisions;
    }

    /// <summary>
    /// ينشئ متغير مراجعة جديدًا بالنسخة الأولى (<c>Version = 1</c>) ويولّد أول نسخة تاريخية غير قابلة للتعديل.
    /// </summary>
    public static DomainOutcome<ReviewVariable> Create(
        AuditCaseId auditCaseId,
        PayrollDisbursementRef disbursementRef,
        VariableKey key,
        ActorScope scope,
        ReviewVariableValue initialValue,
        ActorId actorId,
        DateTimeOffset occurredAtUtc,
        string? reason,
        SourceReference? sourceReference,
        CorrelationId correlationId,
        ReviewVariableId? explicitId = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return DomainOutcome<ReviewVariable>.Failure(
                DomainFailureCode.InvalidInput,
                "سبب إنشاء متغير المراجعة مطلوب ولا يجوز أن يكون فارغًا.");
        }

        var variableId = explicitId ?? ReviewVariableId.NewId();
        var normalizedReason = reason.Trim();

        // أول نسخة تاريخية تكون قيمتها السابقة null لأنها نقطة البداية
        var initialRevision = new ReviewVariableRevision(
            ReviewVariableRevisionId.NewId(),
            variableId,
            revisionNumber: 1,
            previousValue: null,
            newValue: initialValue,
            performedByActorId: actorId,
            occurredAtUtc: occurredAtUtc,
            reason: normalizedReason,
            sourceReference: sourceReference,
            correlationId: correlationId);

        var variable = new ReviewVariable(
            variableId,
            auditCaseId,
            disbursementRef,
            key,
            scope,
            initialValue,
            sourceReference,
            version: 1,
            revisions: [initialRevision]);

        return DomainOutcome<ReviewVariable>.Success(variable);
    }

    /// <summary>
    /// يطبّق تعديلًا جديدًا على قيمة متغير المراجعة بعد التحقق من النطاق و<paramref name="expectedVersion"/>.
    /// إذا كانت النسخة قديمة يرفض التعديل برمز <see cref="DomainFailureCode.VersionConflict"/> دون أي تغيير في الحالة.
    /// يعمل مباشرةً لأي مراجع مخوّل ضمن النطاق دون اشتراط موافقة مراجع أساسي (AI-0093).
    /// </summary>
    public DomainOutcome<ReviewVariableRevision> Revise(
        int expectedVersion,
        ReviewVariableValue newValue,
        ActorId actorId,
        ActorScope actorScope,
        DateTimeOffset occurredAtUtc,
        string? changeReason,
        SourceReference? sourceReference,
        CorrelationId correlationId)
    {
        if (!Scope.Equals(actorScope))
        {
            return DomainOutcome<ReviewVariableRevision>.Failure(
                DomainFailureCode.ScopeMismatch,
                "نطاق العملية لا يطابق نطاق متغير المراجعة.");
        }

        if (expectedVersion != Version)
        {
            return DomainOutcome<ReviewVariableRevision>.Failure(
                DomainFailureCode.VersionConflict,
                $"تعارض تزامن: النسخة المتوقعة ({expectedVersion}) تختلف عن النسخة الحالية للمتغير ({Version}).",
                currentVersion: Version);
        }

        if (string.IsNullOrWhiteSpace(changeReason))
        {
            return DomainOutcome<ReviewVariableRevision>.Failure(
                DomainFailureCode.InvalidInput,
                "سبب تعديل متغير المراجعة مطلوب لتوثيق التغيير.");
        }

        var previousValue = CurrentValue;
        var nextVersion = Version + 1;
        var normalizedReason = changeReason.Trim();

        var revision = new ReviewVariableRevision(
            ReviewVariableRevisionId.NewId(),
            Id,
            revisionNumber: nextVersion,
            previousValue: previousValue,
            newValue: newValue,
            performedByActorId: actorId,
            occurredAtUtc: occurredAtUtc,
            reason: normalizedReason,
            sourceReference: sourceReference,
            correlationId: correlationId);

        // تحديث الحالة يتم فقط بعد نجاح بناء النسخة الجديدة كاملة
        _revisions.Add(revision);
        CurrentValue = newValue;
        CurrentSourceReference = sourceReference;
        Version = nextVersion;

        return DomainOutcome<ReviewVariableRevision>.Success(revision);
    }

    /// <summary>
    /// يعيد بناء الكيان من نسخ تاريخية محفوظة مسبقًا مع التحقق الصارم من جميع ثوابت التسلسل والهوية والقيمة (Q-04).
    /// يمنع بناء كيان بتاريخ متناقض أو نسخ مفقودة أو غير متطابقة مع الحالة الحالية.
    /// </summary>
    public static DomainOutcome<ReviewVariable> Reconstitute(
        ReviewVariableId id,
        AuditCaseId auditCaseId,
        PayrollDisbursementRef disbursementRef,
        VariableKey key,
        ActorScope scope,
        ReviewVariableValue currentValue,
        SourceReference? currentSourceReference,
        int version,
        IEnumerable<ReviewVariableRevision> revisions)
    {
        if (id.Value == Guid.Empty)
        {
            return DomainOutcome<ReviewVariable>.Failure(
                DomainFailureCode.InvalidInput,
                "تعذر إعادة بناء متغير المراجعة: المعرّف فارغ.");
        }

        if (version < 1)
        {
            return DomainOutcome<ReviewVariable>.Failure(
                DomainFailureCode.InvalidInput,
                "تعذر إعادة بناء متغير المراجعة: رقم النسخة يجب أن يكون 1 على الأقل.");
        }

        if (revisions is null)
        {
            return DomainOutcome<ReviewVariable>.Failure(
                DomainFailureCode.InvalidInput,
                "تعذر إعادة بناء متغير المراجعة: قائمة النسخ التاريخية مفقودة.");
        }

        var revisionList = revisions.OrderBy(r => r.RevisionNumber).ToList();
        if (revisionList.Count != version)
        {
            return DomainOutcome<ReviewVariable>.Failure(
                DomainFailureCode.InvalidInput,
                $"تعذر إعادة بناء متغير المراجعة: عدد النسخ التاريخية ({revisionList.Count}) لا يطابق رقم النسخة ({version}).");
        }

        for (var i = 0; i < revisionList.Count; i++)
        {
            var rev = revisionList[i];
            var expectedRevNumber = i + 1;

            if (rev.VariableId != id)
            {
                return DomainOutcome<ReviewVariable>.Failure(
                    DomainFailureCode.InvalidInput,
                    "تعذر إعادة بناء متغير المراجعة: توجد نسخة تاريخية تابعة لمعرّف متغير مختلف.");
            }

            if (rev.RevisionNumber != expectedRevNumber)
            {
                return DomainOutcome<ReviewVariable>.Failure(
                    DomainFailureCode.InvalidInput,
                    $"تعذر إعادة بناء متغير المراجعة: تسلسل النسخ منقطع أو مكرر عند النسخة المتوقعة ({expectedRevNumber}).");
            }

            if (i == 0 && rev.PreviousValue is not null)
            {
                return DomainOutcome<ReviewVariable>.Failure(
                    DomainFailureCode.InvalidInput,
                    "تعذر إعادة بناء متغير المراجعة: النسخة الأولى لا يجوز أن تحمل قيمة سابقة.");
            }

            if (i > 0 && !Equals(rev.PreviousValue, revisionList[i - 1].NewValue))
            {
                return DomainOutcome<ReviewVariable>.Failure(
                    DomainFailureCode.InvalidInput,
                    $"تعذر إعادة بناء متغير المراجعة: القيمة السابقة للنسخة ({expectedRevNumber}) لا تطابق القيمة الجديدة للنسخة السابقة.");
            }
        }

        var latestRevision = revisionList[^1];
        if (!Equals(latestRevision.NewValue, currentValue) ||
            !Equals(latestRevision.SourceReference, currentSourceReference))
        {
            return DomainOutcome<ReviewVariable>.Failure(
                DomainFailureCode.InvalidInput,
                "تعذر إعادة بناء متغير المراجعة: القيمة أو مرجع المصدر الحالي لا يطابق آخر نسخة تاريخية.");
        }

        return DomainOutcome<ReviewVariable>.Success(new ReviewVariable(
            id,
            auditCaseId,
            disbursementRef,
            key,
            scope,
            currentValue,
            currentSourceReference,
            version,
            revisionList));
    }
}

