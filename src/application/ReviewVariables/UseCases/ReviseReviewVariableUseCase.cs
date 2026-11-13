using PayrollReviewSystem.Application.ReviewVariables.Contracts;
using PayrollReviewSystem.Application.ReviewVariables.Internal;
using PayrollReviewSystem.Application.ReviewVariables.Ports;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.UseCases;

/// <summary>
/// حالة استخدام تعديل قيمة متغير مراجعة قائم وفق التزامن المتفائل (<c>ExpectedVersion</c>).
/// تمكّن أي مراجع مخوّل داخل النطاق من التعديل مباشرةً وحفظ النسخة السابقة والجديدة باسمه
/// دون انتظار موافقة «مراجع أساسي» (AI-0093 / D-035).
/// </summary>
public sealed class ReviseReviewVariableUseCase
{
    private readonly IReviewerContextPort _reviewerContextPort;
    private readonly IAuthorizationPolicyPort _authorizationPolicy;
    private readonly IReviewVariableRepository _variableRepository;
    private readonly IAuditTrailPort _auditTrailPort;
    private readonly IIdempotencyPort _idempotencyPort;
    private readonly IReviewAtomicTransactionPort _atomicTransaction;
    private readonly IClockPort _clockPort;

    /// <summary>
    /// ينشئ حالة الاستخدام بحقن جميع المنافذ اللازمة فقط.
    /// </summary>
    public ReviseReviewVariableUseCase(
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
    /// ينفّذ تعديل متغير المراجعة بعد التحقق من الفاعل والنطاق و<c>Idempotency</c> والنسخة المتوقعة.
    /// </summary>
    public async Task<ApplicationResult<ReviewVariableReceipt>> ExecuteAsync(
        ReviseReviewVariableCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 1. التحقق من هوية المراجع الموثوقة على الخادم ومنع انتحال الهوية
        var actorCheck = UseCaseSupport.ResolveAndVerifyTrustedActor(_reviewerContextPort, command.ClaimedActorId);
        if (!actorCheck.IsSuccess || actorCheck.Value is null)
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                actorCheck.Error!.Code,
                actorCheck.Error.Message);
        }

        var actorContext = actorCheck.Value;

        // 2. التحقق من صلاحية مدخلات التعديل
        if (!ReviewVariableId.TryCreate(command.VariableId, out var variableId) ||
            command.ExpectedVersion < 1 ||
            string.IsNullOrWhiteSpace(command.IdempotencyKey) ||
            string.IsNullOrWhiteSpace(command.ChangeReason))
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                ApplicationErrorCode.InvalidVariableValue,
                "معرّف المتغير أو النسخة المتوقعة أو مفتاح عدم التكرار أو سبب التعديل غير صالح.");
        }

        var valueValidation = UseCaseSupport.ValidateValueInput(command.NewValue);
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

        // 3. استرجاع متغير المراجعة لمعرفة نطاقه المالك وحالته الحالية
        var variable = await _variableRepository.GetByIdAsync(variableId, cancellationToken);
        if (variable is null)
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                ApplicationErrorCode.NotFound,
                "متغير المراجعة المطلوب غير موجود.");
        }

        // 4. فحص الصلاحية داخل النطاق قبل السماح بأي تعديل أو استرجاع إيصال عدم التكرار
        var authDecision = _authorizationPolicy.Evaluate(
            actorContext,
            ReviewVariableOperation.ReviseReviewVariable,
            variable.Scope);
        if (!authDecision.IsAllowed)
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                ApplicationErrorCode.PermissionDenied,
                authDecision.DenialReason ?? "المراجع غير مخوّل لتعديل متغيرات المراجعة في هذا النطاق.");
        }

        // 5. فحص Idempotency قبل فحص Version حتى لا تُرفض إعادة المحاولة المتطابقة على أنها VersionConflict
        var idempotencyKey = new IdempotencyRecordKey(
            ReviewVariableOperation.ReviseReviewVariable,
            variable.Id.ToString(),
            variable.Scope,
            command.IdempotencyKey.Trim());

        var normalizedReason = command.ChangeReason.Trim();
        var payloadHash = CanonicalPayloadHasher.HashReviseCommand(
            variableId,
            command.ExpectedVersion,
            valueValidation.Value,
            normalizedReason,
            sourceReference);

        var idempotencyCheck = await _idempotencyPort.CheckAsync(idempotencyKey, payloadHash, cancellationToken);
        if (idempotencyCheck.Status == IdempotencyCheckStatus.MatchedReplay && idempotencyCheck.StoredReceipt is not null)
        {
            return ApplicationResult<ReviewVariableReceipt>.Replayed(idempotencyCheck.StoredReceipt);
        }

        if (idempotencyCheck.Status == IdempotencyCheckStatus.PayloadMismatchConflict)
        {
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                ApplicationErrorCode.IdempotencyConflict,
                "تعارض عدم التكرار: نفس IdempotencyKey استُخدم مع قيمة أو سبب أو نسخة مختلفة.");
        }

        var correlationId = UseCaseSupport.ResolveCorrelationId(command.CorrelationId);
        var nowUtc = _clockPort.UtcNow.ToUniversalTime();
        var previousValueSnapshot = variable.CurrentValue;

        // 6. تطبيق القاعدة داخل الكيان والتحقق من النسخة المتوقعة
        var revisionOutcome = variable.Revise(
            command.ExpectedVersion,
            valueValidation.Value,
            actorContext.ActorId,
            variable.Scope,
            nowUtc,
            normalizedReason,
            sourceReference,
            correlationId);

        if (!revisionOutcome.IsSuccess || revisionOutcome.Value is null)
        {
            return UseCaseSupport.MapDomainFailure<ReviewVariableReceipt>(
                revisionOutcome.FailureCode,
                revisionOutcome.FailureMessage,
                revisionOutcome.CurrentVersion,
                command.ExpectedVersion);
        }

        var newRevision = revisionOutcome.Value;
        var actionEntry = ReviewerActionLogEntry.Create(
            variable.Id,
            variable.Version,
            ReviewVariableOperation.ReviseReviewVariable,
            actorContext.ActorId,
            variable.Scope,
            nowUtc,
            oldValue: previousValueSnapshot,
            newValue: variable.CurrentValue,
            normalizedReason,
            sourceReference,
            correlationId);

        var receipt = UseCaseSupport.BuildReceipt(variable, newRevision, actionEntry);

        // 7. الحفظ الذري في المستودع وسجل التدقيق وسجل عدم التكرار مع حماية التزامن في التخزين
        try
        {
            return await _atomicTransaction.ExecuteAtomicallyAsync(async ct =>
            {
                var saveStatus = await _variableRepository.SaveRevisionAsync(
                    variable,
                    newRevision,
                    command.ExpectedVersion,
                    ct);

                if (saveStatus == RepositorySaveStatus.VersionConflict)
                {
                    return ApplicationResult<ReviewVariableReceipt>.Failure(
                        ApplicationErrorCode.VersionConflict,
                        "تعارض تزامن أثناء الحفظ: سبق مراجع آخر إلى تحديث المتغير بنسخة أحدث.",
                        new VersionConflictInfo(command.ExpectedVersion, variable.Version));
                }

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
            return ApplicationResult<ReviewVariableReceipt>.Failure(
                ApplicationErrorCode.PersistenceFailure,
                "تعذر إتمام حفظ تعديل المتغير وسجل التدقيق ذريًا؛ تم التراجع عن العملية دون أثر جزئي.");
        }
    }
}
