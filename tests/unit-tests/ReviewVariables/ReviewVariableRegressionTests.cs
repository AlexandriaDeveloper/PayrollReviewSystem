using PayrollReviewSystem.Application.ReviewVariables.Contracts;
using PayrollReviewSystem.Application.ReviewVariables.Ports;
using PayrollReviewSystem.Application.ReviewVariables.UseCases;
using PayrollReviewSystem.Core.ReviewVariables;
using Xunit;

namespace PayrollReviewSystem.UnitTests.ReviewVariables;

/// <summary>
/// اختبارات Regression لملاحظات مراجعة ChatGPT (B-01..B-03 وQ-04..Q-06).
/// </summary>
public class ReviewVariableRegressionTests
{
    private const string UnitId = "UNIT-DEMO-01";
    private const string GroupId = "GRP-DEMO-A";

    [Fact]
    [Trait("CaseId", "CODE001-REG-B01-RECORD")]
    public async Task CODE001_REG_B01_ConcurrentIdenticalRecordRequests_MustBothSucceedAndReplay_WithSingleRevisionAndAction()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);

        // غلاف يضمن تزامنًا حتميًا عبر تعليق CheckAsync قليلًا لكشف أي فحص خارج حد الذرية
        var yieldingIdempotency = new YieldingIdempotencyPort(harness.Idempotency, delayMs: 25);

        var useCase = new RecordReviewVariableUseCase(
            harness.ReviewerContext,
            harness.AuthorizationPolicy,
            harness.VariableRepository,
            harness.AuditTrail,
            yieldingIdempotency,
            harness.AtomicTransaction,
            harness.Clock);

        var cmd = new RecordReviewVariableCommand(
            "CASE-DEMO-B01",
            "DISB-DEMO-B01",
            "VAR-B01",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(500m),
            "تسجيل متزامن بنفس المفتاح",
            null,
            "IDEMP-CONC-REC-01");

        var startBarrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<ApplicationResult<ReviewVariableReceipt>> InvokeAsync()
        {
            await startBarrier.Task;
            return await useCase.ExecuteAsync(cmd);
        }

        var t1 = InvokeAsync();
        var t2 = InvokeAsync();
        startBarrier.SetResult();

        var results = await Task.WhenAll(t1, t2);

        Assert.All(results, r => Assert.True(r.IsSuccess, r.Error?.Message));
        Assert.Single(results, r => !r.IsReplayed);
        Assert.Single(results, r => r.IsReplayed);
        Assert.Equal(results[0].Value, results[1].Value);
        Assert.Equal(1, harness.StoredVariableCount);
        Assert.Single(harness.GetAllActionLogs());
        Assert.Equal(1, harness.IdempotencyRecordCount);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-B01-REVISE")]
    public async Task CODE001_REG_B01_ConcurrentIdenticalReviseRequests_MustBothSucceedAndReplay_WithSingleNewRevisionAndAction()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);

        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-DEMO-B01-R",
            "DISB-DEMO-B01-R",
            "VAR-B01-R",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(1000m),
            "تأسيس أولي",
            null,
            "IDEMP-B01R-INIT"));
        Assert.True(created.IsSuccess);
        var varId = created.Value!.VariableId;

        var yieldingIdempotency = new YieldingIdempotencyPort(harness.Idempotency, delayMs: 25);
        var reviseUseCase = new ReviseReviewVariableUseCase(
            harness.ReviewerContext,
            harness.AuthorizationPolicy,
            harness.VariableRepository,
            harness.AuditTrail,
            yieldingIdempotency,
            harness.AtomicTransaction,
            harness.Clock);

        var reviseCmd = new ReviseReviewVariableCommand(
            varId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(1200m),
            "تعديل متزامن معاد بنفس المفتاح",
            null,
            "IDEMP-CONC-REV-01");

        var startBarrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<ApplicationResult<ReviewVariableReceipt>> InvokeAsync()
        {
            await startBarrier.Task;
            return await reviseUseCase.ExecuteAsync(reviseCmd);
        }

        var t1 = InvokeAsync();
        var t2 = InvokeAsync();
        startBarrier.SetResult();

        var results = await Task.WhenAll(t1, t2);

        Assert.All(results, r => Assert.True(r.IsSuccess, r.Error?.Message));
        Assert.Single(results, r => !r.IsReplayed);
        Assert.Single(results, r => r.IsReplayed);
        Assert.Equal(results[0].Value, results[1].Value);

        var history = await harness.CreateHistoryUseCase().ExecuteAsync(new GetReviewVariableHistoryQuery(varId));
        Assert.Equal(2, history.Value!.CurrentVersion);
        Assert.Equal(2, history.Value.Revisions.Count);
        Assert.Equal(2, history.Value.ActionLogs.Count);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-B03")]
    public async Task CODE001_REG_B03_NegativeDecimalValue_IsAcceptedDeterministically_WithoutArbitraryGlobalSignBan()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);

        var result = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-DEMO-B03",
            "DISB-DEMO-B03",
            "VAR-SIGNED-DELTA",
            UnitId,
            GroupId,
            ReviewVariableValueInput.FromExternalInput(-75.50m),
            "تسجيل فرق سالب اصطناعي دون فرض قاعدة حظر عامة بلا سند",
            null,
            "IDEMP-NEG-DECIMAL-01"));

        Assert.True(result.IsSuccess, result.Error?.Message);
        Assert.True(result.Value!.CurrentValue.IsKnown);
        Assert.Equal(-75.50m, result.Value.CurrentValue.Amount);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-Q05")]
    public async Task CODE001_REG_Q05_NaturalKeysWithPipeDelimiter_DoNotCollide()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);
        var recordUseCase = harness.CreateRecordUseCase();

        // متغيران مختلفان تمامًا، لكن ربطهما المباشر بعلامة '|' كان سينتج السلسلة نفسها "CASE-A|B|C|VAR-D"
        var first = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-A|B",
            "C",
            "VAR-D",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(100m),
            "حالة أولى تحتوي محرف |",
            null,
            "SAME-KEY-DIFFERENT-TARGET"));

        var secondDifferentTarget = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-A",
            "B|C",
            "VAR-D",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(999m),
            "حالة ثانية مختلفة تحتوي محرف |",
            null,
            "SAME-KEY-DIFFERENT-TARGET"));

        Assert.True(first.IsSuccess);
        Assert.True(secondDifferentTarget.IsSuccess, secondDifferentTarget.Error?.Message);
        Assert.False(secondDifferentTarget.IsReplayed);
        Assert.NotEqual(first.Value!.VariableId, secondDifferentTarget.Value!.VariableId);
        Assert.Equal(2, harness.StoredVariableCount);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-B01-CONFLICT")]
    public async Task CODE001_REG_B01_ConcurrentConflictingRecordRequests_MustCommitOneAndRejectSecondWithIdempotencyConflict()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);

        var yieldingIdempotency = new YieldingIdempotencyPort(harness.Idempotency, delayMs: 25);

        var useCase = new RecordReviewVariableUseCase(
            harness.ReviewerContext,
            harness.AuthorizationPolicy,
            harness.VariableRepository,
            harness.AuditTrail,
            yieldingIdempotency,
            harness.AtomicTransaction,
            harness.Clock);

        var firstCmd = new RecordReviewVariableCommand(
            "CASE-DEMO-B01-CONF",
            "DISB-DEMO-B01-CONF",
            "VAR-B01-CONF",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(500m),
            "القيمة الأولى",
            null,
            "IDEMP-CONC-CONFLICT-01");

        var secondCmdDifferentPayload = firstCmd with
        {
            Value = ReviewVariableValueInput.Known(750m),
            Reason = "قيمة مختلفة بنفس مفتاح عدم التكرار"
        };

        var task1 = Task.Run(() => useCase.ExecuteAsync(firstCmd));
        var task2 = Task.Run(() => useCase.ExecuteAsync(secondCmdDifferentPayload));
        var results = await Task.WhenAll(task1, task2);

        Assert.Single(results, r => r.IsSuccess && !r.IsReplayed);
        var rejected = Assert.Single(results, r => !r.IsSuccess);
        Assert.Equal(ApplicationErrorCode.IdempotencyConflict, rejected.Error!.Code);
        Assert.Equal(1, harness.StoredVariableCount);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-B02")]
    public async Task CODE001_REG_B02_ClaimingOracleOfficialWithArbitraryVersionId_RemainsUnverifiedDeclaredMetadata()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);
        var recordUseCase = harness.CreateRecordUseCase();

        var oracleClaim = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-DEMO-B02",
            "DISB-DEMO-B02",
            "VAR-ORACLE-CLAIM",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(300m),
            "ادعاء قناة أوراكل من العميل دون تحقق سجل رسمي",
            new SourceReferenceInput(SourceKind.OracleOfficial, "FAKE-ORACLE-VER-999", "CAP-01"),
            "IDEMP-B02-ORACLE"));

        var excelClaim = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-DEMO-B02",
            "DISB-DEMO-B02",
            "VAR-EXCEL-CLAIM",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(300m),
            "قناة إكسل",
            new SourceReferenceInput(SourceKind.ExcelImport, "EXCEL-2026-11", null),
            "IDEMP-B02-EXCEL"));

        Assert.True(oracleClaim.IsSuccess);
        Assert.NotNull(oracleClaim.Value!.SourceReference);
        Assert.Equal(SourceKind.OracleOfficial, oracleClaim.Value.SourceReference!.DeclaredKind);
        Assert.False(oracleClaim.Value.SourceReference.IsAuthenticityVerified);

        Assert.True(excelClaim.IsSuccess);
        Assert.Equal(SourceKind.ExcelImport, excelClaim.Value!.SourceReference!.DeclaredKind);
        Assert.False(excelClaim.Value.SourceReference.IsAuthenticityVerified);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-Q04")]
    public void CODE001_REG_Q04_Reconstitute_RejectsCorruptedOrInconsistentRevisions()
    {
        var id = ReviewVariableId.NewId();
        Assert.True(AuditCaseId.TryCreate("CASE-Q04", out var auditCaseId));
        Assert.True(PayrollDisbursementRef.TryCreate("DISB-Q04", out var disbRef));
        Assert.True(VariableKey.TryCreate("VAR-Q04", out var varKey));
        Assert.True(ActorScope.TryCreate(UnitId, GroupId, out var scope));
        Assert.True(ActorId.TryCreate("REV-Q04", out var actorId));

        var v1Val = ReviewVariableValue.Known(100m);
        var v2Val = ReviewVariableValue.Known(200m);
        var corr = CorrelationId.NewId();
        var now = DateTimeOffset.UtcNow;

        var rev1 = ReviewVariableRevision.Reconstitute(
            ReviewVariableRevisionId.NewId(), id, 1, null, v1Val, actorId!, now, "إنشاء", null, corr);
        var rev2 = ReviewVariableRevision.Reconstitute(
            ReviewVariableRevisionId.NewId(), id, 2, v1Val, v2Val, actorId!, now.AddMinutes(1), "تعديل", null, corr);

        // 0. الحالة السليمة تنجح
        var validOutcome = ReviewVariable.Reconstitute(id, auditCaseId!, disbRef!, varKey!, scope!, v2Val, null, 2, [rev1, rev2]);
        Assert.True(validOutcome.IsSuccess);

        // 1. معرّف فارغ أو نسخة < 1
        Assert.False(ReviewVariable.Reconstitute(default, auditCaseId!, disbRef!, varKey!, scope!, v1Val, null, 1, [rev1]).IsSuccess);
        Assert.False(ReviewVariable.Reconstitute(id, auditCaseId!, disbRef!, varKey!, scope!, v1Val, null, 0, []).IsSuccess);

        // 2. اختلاف عدد النسخ عن رقم النسخة
        Assert.False(ReviewVariable.Reconstitute(id, auditCaseId!, disbRef!, varKey!, scope!, v2Val, null, 2, [rev1]).IsSuccess);

        // 3. فجوة أو تكرار في ترقيم النسخ (1، 3 بدل 1، 2)
        var rev3Gap = ReviewVariableRevision.Reconstitute(
            ReviewVariableRevisionId.NewId(), id, 3, v1Val, v2Val, actorId!, now.AddMinutes(2), "فجوة", null, corr);
        Assert.False(ReviewVariable.Reconstitute(id, auditCaseId!, disbRef!, varKey!, scope!, v2Val, null, 2, [rev1, rev3Gap]).IsSuccess);

        // 4. نسخة تابعة لمتغير مختلف
        var revWrongVar = ReviewVariableRevision.Reconstitute(
            ReviewVariableRevisionId.NewId(), ReviewVariableId.NewId(), 2, v1Val, v2Val, actorId!, now.AddMinutes(1), "معرف مختلف", null, corr);
        Assert.False(ReviewVariable.Reconstitute(id, auditCaseId!, disbRef!, varKey!, scope!, v2Val, null, 2, [rev1, revWrongVar]).IsSuccess);

        // 5. كسر سلسلة PreviousValue (النسخة 1 لها PreviousValue غير null، أو النسخة 2 لا تطابق NewValue للنسخة 1)
        var rev1WithPrev = ReviewVariableRevision.Reconstitute(
            ReviewVariableRevisionId.NewId(), id, 1, ReviewVariableValue.Known(50m), v1Val, actorId!, now, "كسر أول", null, corr);
        Assert.False(ReviewVariable.Reconstitute(id, auditCaseId!, disbRef!, varKey!, scope!, v1Val, null, 1, [rev1WithPrev]).IsSuccess);

        var rev2BrokenChain = ReviewVariableRevision.Reconstitute(
            ReviewVariableRevisionId.NewId(), id, 2, ReviewVariableValue.Known(999m), v2Val, actorId!, now.AddMinutes(1), "سلسلة مكسورة", null, corr);
        Assert.False(ReviewVariable.Reconstitute(id, auditCaseId!, disbRef!, varKey!, scope!, v2Val, null, 2, [rev1, rev2BrokenChain]).IsSuccess);

        // 6. عدم تطابق القيمة الحالية أو المصدر الحالي مع آخر نسخة
        Assert.False(ReviewVariable.Reconstitute(id, auditCaseId!, disbRef!, varKey!, scope!, ReviewVariableValue.Known(777m), null, 2, [rev1, rev2]).IsSuccess);

        Assert.True(SourceReference.TryCreate(SourceKind.DirectForm, "SRC-Q04", null, out var otherSource));
        Assert.False(ReviewVariable.Reconstitute(id, auditCaseId!, disbRef!, varKey!, scope!, v2Val, otherSource, 2, [rev1, rev2]).IsSuccess);
    }

    [Theory]
    [Trait("CaseId", "CODE001-REG-Q06")]
    [InlineData(3)]
    [InlineData(null)]
    public async Task CODE001_REG_Q06_SaveRevisionConflict_ReturnsRealStorageVersionOrNull_NotLocalMutatedVersion(int? storageActualVersion)
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);

        // إنشاء المتغير أولًا على النسخة 1 داخل المخزن
        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-Q06",
            "DISB-Q06",
            "VAR-Q06",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(100m),
            "تسجيل أولي",
            null,
            "IDEMP-Q06-REC"));
        Assert.True(created.IsSuccess);

        // مستودع يحاكي اكتشاف نسخة أحدث في التخزين أثناء SaveRevisionAsync (3 أو غير معروفة null)
        // بينما الكيان المحلي كان قد ارتفع محليًا إلى النسخة 2 أثناء Revise
        var conflictRepo = new StubConflictVariableRepository(harness.VariableRepository, storageActualVersion);
        var reviseUseCase = new ReviseReviewVariableUseCase(
            harness.ReviewerContext,
            harness.AuthorizationPolicy,
            conflictRepo,
            harness.AuditTrail,
            harness.Idempotency,
            harness.AtomicTransaction,
            harness.Clock);

        var conflictResult = await reviseUseCase.ExecuteAsync(new ReviseReviewVariableCommand(
            created.Value!.VariableId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(250m),
            "محاولة تعديل تصطدم بنسخة مخزن أحدث",
            null,
            "IDEMP-Q06-REV"));

        Assert.False(conflictResult.IsSuccess);
        Assert.Equal(ApplicationErrorCode.VersionConflict, conflictResult.Error!.Code);
        Assert.NotNull(conflictResult.Error.ConflictInfo);
        Assert.Equal(1, conflictResult.Error.ConflictInfo!.ExpectedVersion);
        Assert.Equal(storageActualVersion, conflictResult.Error.ConflictInfo.ActualVersion);
    }

    private sealed class StubConflictVariableRepository(IReviewVariableRepository inner, int? actualVersionOnConflict)
        : IReviewVariableRepository
    {
        public Task<ReviewVariable?> GetByIdAsync(ReviewVariableId id, CancellationToken cancellationToken = default)
            => inner.GetByIdAsync(id, cancellationToken);

        public Task<ReviewVariable?> GetByNaturalKeyAsync(
            AuditCaseId auditCaseId,
            PayrollDisbursementRef disbursementRef,
            VariableKey key,
            CancellationToken cancellationToken = default)
            => inner.GetByNaturalKeyAsync(auditCaseId, disbursementRef, key, cancellationToken);

        public Task SaveNewAsync(ReviewVariable variable, CancellationToken cancellationToken = default)
            => inner.SaveNewAsync(variable, cancellationToken);

        public Task<RepositorySaveResult> SaveRevisionAsync(
            ReviewVariable variable,
            ReviewVariableRevision newRevision,
            int expectedVersion,
            CancellationToken cancellationToken = default)
            => Task.FromResult(RepositorySaveResult.VersionConflict(actualVersionOnConflict));
    }

    private sealed class YieldingIdempotencyPort(IIdempotencyPort inner, int delayMs) : IIdempotencyPort
    {
        public async Task<IdempotencyCheckResult> CheckAsync(
            IdempotencyRecordKey key,
            string requestPayloadHash,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(delayMs, cancellationToken);
            return await inner.CheckAsync(key, requestPayloadHash, cancellationToken);
        }

        public Task SaveAsync(
            IdempotencyRecordKey key,
            string requestPayloadHash,
            ReviewVariableReceipt receipt,
            CancellationToken cancellationToken = default)
            => inner.SaveAsync(key, requestPayloadHash, receipt, cancellationToken);
    }
}
