using PayrollReviewSystem.Application.ReviewVariables.Contracts;
using PayrollReviewSystem.Application.ReviewVariables.Policies;
using PayrollReviewSystem.Application.ReviewVariables.Ports;
using PayrollReviewSystem.Application.ReviewVariables.UseCases;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.UnitTests.ReviewVariables;

/// <summary>
/// بيئة اختبار مزيفة في الذاكرة (In-Memory Fakes) معزولة تمامًا داخل مشروع اختبارات الوحدة فقط.
/// تحاكي هذه البيئة:
/// 1. عزل اللقطات (<see cref="ReviewVariable.Reconstitute"/>) لاختبار التزامن الحقيقي دون مشاركة مراجع الذاكرة.
/// 2. الذرية الكاملة والتراجع (Rollback) عند فشل حفظ المتغير أو سجل التدقيق أو سجل عدم التكرار.
/// 3. ساعة حتمية وسياق مراجع موثوق قابل للتبديل لمحاكاة تعدد المراجعين وانتحال الهوية.
/// </summary>
internal sealed class InMemoryReviewVariableTestHarness
{
    private readonly object _syncLock = new();
    private Dictionary<ReviewVariableId, StoredVariableRecord> _variables = new();
    private List<ReviewerActionLogEntry> _actionLogs = [];
    private Dictionary<IdempotencyRecordKey, (string Hash, ReviewVariableReceipt Receipt)> _idempotencyRecords = new();

    public FakeReviewerContextPort ReviewerContext { get; } = new();
    public FakeClockPort Clock { get; } = new(new DateTimeOffset(2026, 11, 13, 18, 0, 0, TimeSpan.Zero));
    public IAuthorizationPolicyPort AuthorizationPolicy { get; } = new DefaultReviewerAuthorizationPolicy();
    public FakeReviewVariableRepository VariableRepository { get; }
    public FakeAuditTrailPort AuditTrail { get; }
    public FakeIdempotencyPort Idempotency { get; }
    public FakeAtomicTransactionPort AtomicTransaction { get; }

    // أعلام حقن الأعطال لاختبار فشل التخزين الذري (CODE001-NEG-06)
    public bool FailOnVariableSave { get; set; }
    public bool FailOnAuditAppend { get; set; }
    public bool FailOnIdempotencySave { get; set; }

    public InMemoryReviewVariableTestHarness()
    {
        VariableRepository = new FakeReviewVariableRepository(this);
        AuditTrail = new FakeAuditTrailPort(this);
        Idempotency = new FakeIdempotencyPort(this);
        AtomicTransaction = new FakeAtomicTransactionPort(this);
    }

    public RecordReviewVariableUseCase CreateRecordUseCase()
        => new(
            ReviewerContext,
            AuthorizationPolicy,
            VariableRepository,
            AuditTrail,
            Idempotency,
            AtomicTransaction,
            Clock);

    public ReviseReviewVariableUseCase CreateReviseUseCase()
        => new(
            ReviewerContext,
            AuthorizationPolicy,
            VariableRepository,
            AuditTrail,
            Idempotency,
            AtomicTransaction,
            Clock);

    public GetReviewVariableHistoryUseCase CreateHistoryUseCase()
        => new(
            ReviewerContext,
            AuthorizationPolicy,
            VariableRepository,
            AuditTrail);

    public int StoredVariableCount
    {
        get { lock (_syncLock) return _variables.Count; }
    }

    public IReadOnlyList<ReviewerActionLogEntry> GetAllActionLogs()
    {
        lock (_syncLock) return _actionLogs.ToList();
    }

    public int IdempotencyRecordCount
    {
        get { lock (_syncLock) return _idempotencyRecords.Count; }
    }

    /// <summary>
    /// يبني سياق مراجع اصطناعي موثوق خالٍ من أي بيانات موظفين حقيقية.
    /// </summary>
    public static TrustedReviewerContext BuildReviewerContext(
        string actorId,
        string unitId = "UNIT-DEMO-01",
        string groupId = "GRP-DEMO-A",
        string roleName = "UnitReviewer",
        bool canModify = true)
    {
        ActorId.TryCreate(actorId, out var actor);
        ActorScope.TryCreate(unitId, groupId, out var scope);

        return new TrustedReviewerContext(
            actor!,
            roleName,
            new HashSet<ActorScope> { scope! },
            canModify);
    }

    private sealed record StoredVariableRecord(
        ReviewVariableId Id,
        AuditCaseId AuditCaseId,
        PayrollDisbursementRef DisbursementRef,
        VariableKey Key,
        ActorScope Scope,
        ReviewVariableValue CurrentValue,
        SourceReference? CurrentSourceReference,
        int Version,
        List<ReviewVariableRevision> Revisions)
    {
        public ReviewVariable ToIsolatedDomainCopy()
            => ReviewVariable.Reconstitute(
                Id,
                AuditCaseId,
                DisbursementRef,
                Key,
                Scope,
                CurrentValue,
                CurrentSourceReference,
                Version,
                Revisions.ToList());

        public static StoredVariableRecord FromDomain(ReviewVariable variable)
            => new(
                variable.Id,
                variable.AuditCaseId,
                variable.DisbursementRef,
                variable.Key,
                variable.Scope,
                variable.CurrentValue,
                variable.CurrentSourceReference,
                variable.Version,
                variable.Revisions.ToList());
    }

    internal sealed class FakeReviewerContextPort : IReviewerContextPort
    {
        public TrustedReviewerContext? Current { get; set; }
        public TrustedReviewerContext? GetCurrentReviewer() => Current;
    }

    internal sealed class FakeClockPort : IClockPort
    {
        public FakeClockPort(DateTimeOffset initialUtc) => UtcNow = initialUtc;
        public DateTimeOffset UtcNow { get; set; }
    }

    internal sealed class FakeReviewVariableRepository(InMemoryReviewVariableTestHarness harness) : IReviewVariableRepository
    {
        public Task<ReviewVariable?> GetByIdAsync(ReviewVariableId id, CancellationToken cancellationToken = default)
        {
            lock (harness._syncLock)
            {
                return Task.FromResult(
                    harness._variables.TryGetValue(id, out var record)
                        ? record.ToIsolatedDomainCopy()
                        : null);
            }
        }

        public Task<ReviewVariable?> GetByNaturalKeyAsync(
            AuditCaseId auditCaseId,
            PayrollDisbursementRef disbursementRef,
            VariableKey key,
            CancellationToken cancellationToken = default)
        {
            lock (harness._syncLock)
            {
                var found = harness._variables.Values.FirstOrDefault(v =>
                    v.AuditCaseId.Equals(auditCaseId) &&
                    v.DisbursementRef.Equals(disbursementRef) &&
                    v.Key.Equals(key));

                return Task.FromResult(found?.ToIsolatedDomainCopy());
            }
        }

        public Task SaveNewAsync(ReviewVariable variable, CancellationToken cancellationToken = default)
        {
            if (harness.FailOnVariableSave)
            {
                throw new InvalidOperationException("Simulated variable repository save failure.");
            }

            lock (harness._syncLock)
            {
                harness._variables[variable.Id] = StoredVariableRecord.FromDomain(variable);
            }

            return Task.CompletedTask;
        }

        public Task<RepositorySaveStatus> SaveRevisionAsync(
            ReviewVariable variable,
            ReviewVariableRevision newRevision,
            int expectedVersion,
            CancellationToken cancellationToken = default)
        {
            if (harness.FailOnVariableSave)
            {
                throw new InvalidOperationException("Simulated revision repository save failure.");
            }

            lock (harness._syncLock)
            {
                if (!harness._variables.TryGetValue(variable.Id, out var current))
                {
                    return Task.FromResult(RepositorySaveStatus.VersionConflict);
                }

                // التحقق الذري من تطابق النسخة المتوقعة ضد النسخة المخزنة فعليًا
                if (current.Version != expectedVersion)
                {
                    return Task.FromResult(RepositorySaveStatus.VersionConflict);
                }

                harness._variables[variable.Id] = StoredVariableRecord.FromDomain(variable);
                return Task.FromResult(RepositorySaveStatus.Saved);
            }
        }
    }

    internal sealed class FakeAuditTrailPort(InMemoryReviewVariableTestHarness harness) : IAuditTrailPort
    {
        public Task AppendAsync(ReviewerActionLogEntry entry, CancellationToken cancellationToken = default)
        {
            if (harness.FailOnAuditAppend)
            {
                throw new InvalidOperationException("Simulated audit trail append failure.");
            }

            lock (harness._syncLock)
            {
                harness._actionLogs.Add(entry);
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ReviewerActionLogEntry>> GetByVariableIdAsync(
            ReviewVariableId variableId,
            CancellationToken cancellationToken = default)
        {
            lock (harness._syncLock)
            {
                IReadOnlyList<ReviewerActionLogEntry> entries = harness._actionLogs
                    .Where(a => a.TargetVariableId == variableId)
                    .OrderBy(a => a.OccurredAtUtc)
                    .ThenBy(a => a.TargetVersion)
                    .ToList();

                return Task.FromResult(entries);
            }
        }
    }

    internal sealed class FakeIdempotencyPort(InMemoryReviewVariableTestHarness harness) : IIdempotencyPort
    {
        public Task<IdempotencyCheckResult> CheckAsync(
            IdempotencyRecordKey key,
            string requestPayloadHash,
            CancellationToken cancellationToken = default)
        {
            lock (harness._syncLock)
            {
                if (!harness._idempotencyRecords.TryGetValue(key, out var existing))
                {
                    return Task.FromResult(new IdempotencyCheckResult(IdempotencyCheckStatus.NotFound));
                }

                if (string.Equals(existing.Hash, requestPayloadHash, StringComparison.Ordinal))
                {
                    return Task.FromResult(new IdempotencyCheckResult(
                        IdempotencyCheckStatus.MatchedReplay,
                        existing.Receipt));
                }

                return Task.FromResult(new IdempotencyCheckResult(IdempotencyCheckStatus.PayloadMismatchConflict));
            }
        }

        public Task SaveAsync(
            IdempotencyRecordKey key,
            string requestPayloadHash,
            ReviewVariableReceipt receipt,
            CancellationToken cancellationToken = default)
        {
            if (harness.FailOnIdempotencySave)
            {
                throw new InvalidOperationException("Simulated idempotency store save failure.");
            }

            lock (harness._syncLock)
            {
                harness._idempotencyRecords[key] = (requestPayloadHash, receipt);
            }

            return Task.CompletedTask;
        }
    }

    internal sealed class FakeAtomicTransactionPort(InMemoryReviewVariableTestHarness harness) : IReviewAtomicTransactionPort
    {
        private readonly SemaphoreSlim _txMutex = new(1, 1);

        public async Task<TResult> ExecuteAtomicallyAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default)
        {
            await _txMutex.WaitAsync(cancellationToken);
            try
            {
                Dictionary<ReviewVariableId, StoredVariableRecord> varsBackup;
                List<ReviewerActionLogEntry> logsBackup;
                Dictionary<IdempotencyRecordKey, (string Hash, ReviewVariableReceipt Receipt)> idempBackup;

                lock (harness._syncLock)
                {
                    varsBackup = harness._variables.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value with { Revisions = kvp.Value.Revisions.ToList() });
                    logsBackup = harness._actionLogs.ToList();
                    idempBackup = new Dictionary<IdempotencyRecordKey, (string Hash, ReviewVariableReceipt Receipt)>(
                        harness._idempotencyRecords);
                }

                try
                {
                    return await action(cancellationToken);
                }
                catch
                {
                    // محاكاة Rollback كامل لحدود المعاملة الذرية عند أي استثناء
                    lock (harness._syncLock)
                    {
                        harness._variables = varsBackup;
                        harness._actionLogs = logsBackup;
                        harness._idempotencyRecords = idempBackup;
                    }

                    throw;
                }
            }
            finally
            {
                _txMutex.Release();
            }
        }
    }
}
