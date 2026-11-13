using PayrollReviewSystem.Application.ReviewVariables.Contracts;
using PayrollReviewSystem.Application.ReviewVariables.Internal;
using PayrollReviewSystem.Application.ReviewVariables.Ports;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.UseCases;

/// <summary>
/// حالة استخدام عرض السجل التاريخي للنسخ وأفعال المراجعين لمتغير مراجعة محدد (قراءة محايدة لا تعدل الحالة).
/// </summary>
public sealed class GetReviewVariableHistoryUseCase
{
    private readonly IReviewerContextPort _reviewerContextPort;
    private readonly IAuthorizationPolicyPort _authorizationPolicy;
    private readonly IReviewVariableRepository _variableRepository;
    private readonly IAuditTrailPort _auditTrailPort;

    /// <summary>
    /// ينشئ حالة الاستعلام بحقن منافذ السياق والتفويض والقراءة فقط.
    /// </summary>
    public GetReviewVariableHistoryUseCase(
        IReviewerContextPort reviewerContextPort,
        IAuthorizationPolicyPort authorizationPolicy,
        IReviewVariableRepository variableRepository,
        IAuditTrailPort auditTrailPort)
    {
        _reviewerContextPort = reviewerContextPort ?? throw new ArgumentNullException(nameof(reviewerContextPort));
        _authorizationPolicy = authorizationPolicy ?? throw new ArgumentNullException(nameof(authorizationPolicy));
        _variableRepository = variableRepository ?? throw new ArgumentNullException(nameof(variableRepository));
        _auditTrailPort = auditTrailPort ?? throw new ArgumentNullException(nameof(auditTrailPort));
    }

    /// <summary>
    /// يرجع التسلسل الكامل للنسخ وسجل تدقيق أفعال المراجعين للمتغير المطلوب بعد التحقق من الصلاحية والنطاق.
    /// </summary>
    public async Task<ApplicationResult<ReviewVariableHistoryDto>> ExecuteAsync(
        GetReviewVariableHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actorCheck = UseCaseSupport.ResolveAndVerifyTrustedActor(_reviewerContextPort, query.ClaimedActorId);
        if (!actorCheck.IsSuccess || actorCheck.Value is null)
        {
            return ApplicationResult<ReviewVariableHistoryDto>.Failure(
                actorCheck.Error!.Code,
                actorCheck.Error.Message);
        }

        if (!ReviewVariableId.TryCreate(query.VariableId, out var variableId))
        {
            return ApplicationResult<ReviewVariableHistoryDto>.Failure(
                ApplicationErrorCode.InvalidVariableValue,
                "معرّف متغير المراجعة غير صالح.");
        }

        try
        {
            var variable = await _variableRepository.GetByIdAsync(variableId, cancellationToken);
            if (variable is null)
            {
                return ApplicationResult<ReviewVariableHistoryDto>.Failure(
                    ApplicationErrorCode.NotFound,
                    "متغير المراجعة المطلوب غير موجود.");
            }

            var authDecision = _authorizationPolicy.Evaluate(
                actorCheck.Value,
                ReviewVariableOperation.ViewReviewVariableHistory,
                variable.Scope);
            if (!authDecision.IsAllowed)
            {
                return ApplicationResult<ReviewVariableHistoryDto>.Failure(
                    ApplicationErrorCode.PermissionDenied,
                    authDecision.DenialReason ?? "المراجع غير مخوّل للاطلاع على سجل متغير المراجعة خارج نطاقه.");
            }

            var actionLogs = await _auditTrailPort.GetByVariableIdAsync(variable.Id, cancellationToken);

            var revisionDtos = variable.Revisions
                .OrderBy(r => r.RevisionNumber)
                .Select(r => new ReviewVariableRevisionDto(
                    r.RevisionId.Value,
                    r.RevisionNumber,
                    r.PreviousValue is null ? null : UseCaseSupport.ToSnapshot(r.PreviousValue),
                    UseCaseSupport.ToSnapshot(r.NewValue),
                    r.PerformedByActorId.Value,
                    r.OccurredAtUtc,
                    r.Reason,
                    UseCaseSupport.ToSnapshot(r.SourceReference),
                    r.CorrelationId.Value))
                .ToList();

            var actionDtos = actionLogs
                .OrderBy(a => a.OccurredAtUtc)
                .ThenBy(a => a.TargetVersion)
                .Select(a => new ReviewerActionLogDto(
                    a.ActionId.Value,
                    a.TargetVersion,
                    a.Operation,
                    a.PerformedByActorId.Value,
                    a.Scope.UnitId,
                    a.Scope.ReviewGroupId,
                    a.OccurredAtUtc,
                    a.OldValue is null ? null : UseCaseSupport.ToSnapshot(a.OldValue),
                    UseCaseSupport.ToSnapshot(a.NewValue),
                    a.Reason,
                    UseCaseSupport.ToSnapshot(a.SourceReference),
                    a.CorrelationId.Value))
                .ToList();

            var history = new ReviewVariableHistoryDto(
                variable.Id.Value,
                variable.AuditCaseId.Value,
                variable.DisbursementRef.Value,
                variable.Key.Value,
                variable.Scope.UnitId,
                variable.Scope.ReviewGroupId,
                variable.Version,
                UseCaseSupport.ToSnapshot(variable.CurrentValue),
                UseCaseSupport.ToSnapshot(variable.CurrentSourceReference),
                revisionDtos,
                actionDtos);

            return ApplicationResult<ReviewVariableHistoryDto>.Success(history);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return ApplicationResult<ReviewVariableHistoryDto>.Failure(
                ApplicationErrorCode.PersistenceFailure,
                "تعذر استرجاع السجل التاريخي لمتغير المراجعة حاليًا؛ لم يتم كشف أي تفاصيل تقنية داخلية.");
        }
    }
}
