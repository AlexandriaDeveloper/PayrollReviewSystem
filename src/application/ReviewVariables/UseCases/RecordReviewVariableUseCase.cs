using PayrollReviewSystem.Application.ReviewVariables.Contracts;
using PayrollReviewSystem.Application.ReviewVariables.Internal;
using PayrollReviewSystem.Application.ReviewVariables.Ports;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.UseCases;

/// <summary>
/// حالة استخدام إنشاء/تسجيل متغير مراجعة جديد (النسخة 1).
/// تتحقق من هوية المراجع الموثوقة، وتمنع انتحال الفاعل، وتتحقق من نطاق الصلاحية،
/// وتضمن عدم التكرار عبر <c>IdempotencyKey</c>، وتحفظ المتغير وسجل الفاعل وإيصال التكرار ذريًا.
/// </summary>
public sealed class RecordReviewVariableUseCase
{
    private readonly IReviewerContextPort _reviewerContextPort;
    private readonly IAuthorizationPolicyPort _authorizationPolicy;
    private readonly IReviewVariableRepository _variableRepository;
    private readonly IAuditTrailPort _auditTrailPort;
    private readonly IIdempotencyPort _idempotencyPort;
    private readonly IReviewAtomicTransactionPort _atomicTransaction;
    private readonly IClockPort _clockPort;

    /// <summary>
    /// ينشئ حالة الاستخدام بحقن جميع المنافذ المطلوبة وفق مبدأ Dependency Inversion.
    /// </summary>
    public RecordReviewVariableUseCase(
        IReviewerContextPort reviewerContextPort,
        IAuthorizationPolicyPort authorizationPolicy,
        IReviewVariableRepository variableRepository,
        IAuditTrailPort auditTrailPort,
        IIdempotencyPort idempotencyPort,
        IReviewAtomicTransactionPort atomicTransaction,
        IClockPort clockPort)
    {
        _reviewerContextPort = reviewerContextPort ?? throw new ArgumentNullException(nameof(reviewerContextPort));
        _authorizationPolicy = authorizationPolicy ?? throw new ArgumentNullException(nameof(authorizationPolicy));
        _variableRepository = variableRepository ?? throw new ArgumentNullException(nameof(variableRepository));
        _auditTrailPort = auditTrailPort ?? throw new ArgumentNullException(nameof(auditTrailPort));
        _idempotencyPort = idempotencyPort ?? throw new ArgumentNullException(nameof(idempotencyPort));
        _atomicTransaction = atomicTransaction ?? throw new ArgumentNullException(nameof(atomicTransaction));
        _clockPort = clockPort ?? throw new ArgumentNullException(nameof(clockPort));
    }

    /// <summary>
    /// ينفّذ تسجيل متغير مراجعة جديد وإصدار النسخة الأولى باسم المراجع الموثق.
    /// </summary>
    public async Task<ApplicationResult<ReviewVariableReceipt>> ExecuteAsync(
        RecordReviewVariableCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 1. استخراج هوية المراجع الموثوقة من الخادم ومنع انتحال الهوية
        var actorCheck = UseCaseSupport.ResolveAndVerifyTrustedActor(_reviewerContextPort, command.ClaimedActorId);
        if (!actorCheck.IsSuccess || actorCheck.Value is null)
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                actorCheck.Error!.Code,
                actorCheck.Error.Message);
        }

        var actorContext = actorCheck.Value;

        // 2. التحقق من صحة المدخلات الأساسية وتحويلها إلى كائنات قيمة نقية
        if (!AuditCaseId.TryCreate(command.AuditCaseId, out var auditCaseId) || auditCaseId is null ||
            !PayrollDisbursementRef.TryCreate(command.PayrollDisbursementId, out var disbursementRef) || disbursementRef is null ||
            !VariableKey.TryCreate(command.VariableKey, out var variableKey) || variableKey is null ||
            !ActorScope.TryCreate(command.UnitId, command.ReviewGroupId, out var scope) || scope is null)
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                ApplicationErrorCode.InvalidVariableValue,
                "معرّفات الحالة أو الصرفية أو مفتاح المتغير أو نطاق الوحدة/المجموعة غير صالحة.");
        }

        if (string.IsNullOrWhiteSpace(command.IdempotencyKey) || string.IsNullOrWhiteSpace(command.Reason))
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                ApplicationErrorCode.InvalidVariableValue,
                "مفتاح عدم التكرار (IdempotencyKey) وسبب التسجيل مطلوبان.");
        }

        var valueValidation = UseCaseSupport.ValidateValueInput(command.Value);
        if (!valueValidation.IsSuccess || valueValidation.Value is null)
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                valueValidation.Error!.Code,
                valueValidation.Error.Message);
        }

        if (!UseCaseSupport.TryConvertSource(command.Source, out var sourceReference, out var sourceError))
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(sourceError!.Code, sourceError.Message);
        }

        // 3. فحص الصلاحية داخل النطاق المستهدف (دون أي اشتراط لموافقة مراجع أساسي وفق AI-0093)
        var authDecision = _authorizationPolicy.Evaluate(
            actorContext,
            ReviewVariableOperation.RecordReviewVariable,
            scope);
        if (!authDecision.IsAllowed)
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                ApplicationErrorCode.PermissionDenied,
                authDecision.DenialReason ?? "المراجع غير مخوّل لتسجيل متغير مراجعة في هذا النطاق.");
        }

        // 4. تجهيز المفتاح الطبيعي المركب بترميز مقاوم لتصادم الفواصل وبصمة الحمولة المعيارية
        var targetNaturalKey = CanonicalPayloadHasher.EncodeCompositeTargetKey(
            auditCaseId.Value,
            disbursementRef.Value,
            variableKey.Value);
        var idempotencyKey = new IdempotencyRecordKey(
            ReviewVariableOperation.RecordReviewVariable,
            targetNaturalKey,
            scope,
            actorContext.ActorId,
            command.IdempotencyKey.Trim());

        var normalizedReason = command.Reason.Trim();
        var payloadHash = CanonicalPayloadHasher.HashRecordCommand(
            auditCaseId,
            disbursementRef,
            variableKey,
            scope,
            valueValidation.Value,
            normalizedReason,
            sourceReference);

        var correlationId = UseCaseSupport.ResolveCorrelationId(command.CorrelationId);
        var nowUtc = _clockPort.UtcNow.ToUniversalTime();

        // 5. التنفيذ الذري: فحص عدم التكرار + إنشاء المتغير + إلحاق سجل التدقيق + حفظ إيصال عدم التكرار
        try
        {
            return await _atomicTransaction.ExecuteAtomicallyAsync(async ct =>
            {
                var idempotencyCheck = await _idempotencyPort.CheckAsync(idempotencyKey, payloadHash, ct);
                if (idempotencyCheck.Status == IdempotencyCheckStatus.MatchedReplay && idempotencyCheck.StoredReceipt is not null)
                {
                    return ApplicationResult<ReviewVariableReceipt>.Replayed(idempotencyCheck.StoredReceipt);
                }

                if (idempotencyCheck.Status == IdempotencyCheckStatus.PayloadMismatchConflict)
                {
                    return ApplicationResult<ReviewVariableReceipt>.Failure(
                        ApplicationErrorCode.IdempotencyConflict,
                        "تعارض عدم التكرار: نفس IdempotencyKey استُخدم مع حمولة أو قيمة أو سبب مختلف.");
                }

                var existing = await _variableRepository.GetByNaturalKeyAsync(
                    auditCaseId,
                    disbursementRef,
                    variableKey,
                    ct);

                if (existing is not null)
                {
                    return ApplicationResult<ReviewVariableReceipt>.Failure(
                        ApplicationErrorCode.DuplicateVariable,
                        $"متغير المراجعة '{variableKey.Value}' موجود بالفعل لهذه الصرفية بالنسخة ({existing.Version})؛ استخدم أمر التعديل برقم النسخة.");
                }

                var domainOutcome = ReviewVariable.Create(
                    auditCaseId,
                    disbursementRef,
                    variableKey,
                    scope,
                    valueValidation.Value,
                    actorContext.ActorId,
                    nowUtc,
                    normalizedReason,
                    sourceReference,
                    correlationId);

                if (!domainOutcome.IsSuccess || domainOutcome.Value is null)
                {
                    return UseCaseSupport.MapDomainFailure<ReviewVariableReceipt>(
                        domainOutcome.FailureCode,
                        domainOutcome.FailureMessage,
                        domainOutcome.CurrentVersion);
                }

                var variable = domainOutcome.Value;
                var initialRevision = variable.Revisions[0];

                var actionEntry = ReviewerActionLogEntry.Create(
                    variable.Id,
                    variable.Version,
                    ReviewVariableOperation.RecordReviewVariable,
                    actorContext.ActorId,
                    scope,
                    nowUtc,
                    oldValue: null,
                    newValue: variable.CurrentValue,
                    normalizedReason,
                    sourceReference,
                    correlationId);

                var receipt = UseCaseSupport.BuildReceipt(variable, initialRevision, actionEntry);

                await _variableRepository.SaveNewAsync(variable, ct);
                await _auditTrailPort.AppendAsync(actionEntry, ct);
                await _idempotencyPort.SaveAsync(idempotencyKey, payloadHash, receipt, ct);

                return ApplicationResult<ReviewVariableReceipt>.Success(receipt);
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // منع تسريب استثناء تقني أو إظهار نجاح كاذب عند فشل التخزين الذري
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                ApplicationErrorCode.PersistenceFailure,
                "تعذر إتمام حفظ متغير المراجعة وسجل التدقيق ذريًا؛ تم التراجع عن العملية دون أثر جزئي.");
        }
    }
}

