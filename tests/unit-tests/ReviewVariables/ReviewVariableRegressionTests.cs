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

    [Theory]
    [Trait("CaseId", "CODE001-REG-QA01")]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public async Task CODE001_REG_QA01_RecordPersistenceFailureInAnyAtomicStep_RollsBackCompletely_WithZeroOrphanRecords(
        bool failVariableSave,
        bool failAuditAppend,
        bool failIdempotencySave)
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);
        harness.FailOnVariableSave = failVariableSave;
        harness.FailOnAuditAppend = failAuditAppend;
        harness.FailOnIdempotencySave = failIdempotencySave;

        var result = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-QA01",
            "DISB-QA01",
            "VAR-QA01",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(450m),
            "محاولة إنشاء تفشل في التخزين",
            null,
            "IDEMP-QA01-FAIL"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCode.PersistenceFailure, result.Error!.Code);
        Assert.Equal(0, harness.StoredVariableCount);
        Assert.Empty(harness.GetAllActionLogs());
        Assert.Equal(0, harness.IdempotencyRecordCount);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-QA02")]
    public async Task CODE001_REG_QA02_ConcurrentConflictingReviseRequests_MustCommitOneAndRejectSecondWithIdempotencyConflict()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);

        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-QA02",
            "DISB-QA02",
            "VAR-QA02",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(100m),
            "تسجيل أولي",
            null,
            "IDEMP-QA02-CREATE"));
        Assert.True(created.IsSuccess);

        var yieldingIdempotency = new YieldingIdempotencyPort(harness.Idempotency, delayMs: 25);
        var reviseUseCase = new ReviseReviewVariableUseCase(
            harness.ReviewerContext,
            harness.AuthorizationPolicy,
            harness.VariableRepository,
            harness.AuditTrail,
            yieldingIdempotency,
            harness.AtomicTransaction,
            harness.Clock);

        var cmd1 = new ReviseReviewVariableCommand(
            created.Value!.VariableId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(220m),
            "تعديل أول",
            null,
            "IDEMP-QA02-SHARED");

        var cmd2 = cmd1 with
        {
            NewValue = ReviewVariableValueInput.Known(330m),
            ChangeReason = "تعديل ثانٍ بحمولة مختلفة"
        };

        var results = await Task.WhenAll(
            Task.Run(() => reviseUseCase.ExecuteAsync(cmd1)),
            Task.Run(() => reviseUseCase.ExecuteAsync(cmd2)));

        Assert.Single(results, r => r.IsSuccess && !r.IsReplayed);
        var conflict = Assert.Single(results, r => !r.IsSuccess);
        Assert.Equal(ApplicationErrorCode.IdempotencyConflict, conflict.Error!.Code);

        var history = await harness.CreateHistoryUseCase().ExecuteAsync(new GetReviewVariableHistoryQuery(created.Value.VariableId));
        Assert.True(history.IsSuccess);
        Assert.Equal(2, history.Value!.CurrentVersion);
        Assert.Equal(2, history.Value.ActionLogs.Count);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-QA03")]
    public async Task CODE001_REG_QA03_HistoryIsImmutable_AndTransitionsAcrossKnownZeroUnknownAndNegativePreservePriorRevisions()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        var t0 = new DateTimeOffset(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);
        harness.Clock.UtcNow = t0;
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);

        var rec = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-QA03",
            "DISB-QA03",
            "VAR-QA03",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(500m),
            "قيمة ابتدائية موجبة",
            new SourceReferenceInput(SourceKind.DirectForm, "DF-1", "CAP-1"),
            "IDEMP-QA03-1"));
        Assert.True(rec.IsSuccess);
        var varId = rec.Value!.VariableId;

        // مراجع آخر يغيّر القيمة إلى Unknown ثم مراجع ثالث يغيّرها إلى 0 ثم إلى قيمة سالبة
        harness.Clock.UtcNow = t0.AddMinutes(5);
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-C", UnitId, GroupId);
        var rev2 = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId, 1, ReviewVariableValueInput.Unknown(), "البيان مفقود في المستند", null, "IDEMP-QA03-2"));
        Assert.True(rev2.IsSuccess);

        harness.Clock.UtcNow = t0.AddMinutes(10);
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-D", UnitId, GroupId);
        var rev3 = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId, 2, ReviewVariableValueInput.Known(0m), "تثبيت القيمة الصفرية الفعلية", null, "IDEMP-QA03-3"));
        Assert.True(rev3.IsSuccess);

        harness.Clock.UtcNow = t0.AddMinutes(15);
        var rev4 = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId, 3, ReviewVariableValueInput.Known(-150m), "قيمة تسوية سالبة", null, "IDEMP-QA03-4"));
        Assert.True(rev4.IsSuccess);

        // محاولة تعديل فاشلة لا تغيّر السجل التاريخي السابق
        var staleFail = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId, 2, ReviewVariableValueInput.Known(999m), "نسخة قديمة", null, "IDEMP-QA03-FAIL"));
        Assert.False(staleFail.IsSuccess);

        // التحقق أن قائمة Revisions في الكيان محمية ضد التعديل المباشر أو الحذف
        Assert.True(ReviewVariableId.TryCreate(varId, out var domainId));
        var domainObj = await harness.VariableRepository.GetByIdAsync(domainId);
        Assert.NotNull(domainObj);
        var revisionListAsIList = Assert.IsAssignableFrom<IList<ReviewVariableRevision>>(domainObj!.Revisions);
        Assert.True(revisionListAsIList.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => revisionListAsIList.Clear());

        var history = await harness.CreateHistoryUseCase().ExecuteAsync(new GetReviewVariableHistoryQuery(varId));
        Assert.True(history.IsSuccess);
        var dto = history.Value!;
        Assert.Equal(4, dto.CurrentVersion);
        Assert.Equal(-150m, dto.CurrentValue.Amount);
        Assert.Equal(4, dto.Revisions.Count);

        // التحقق من بقاء كل النسخ السابقة دون استبدال أو مسح
        Assert.Null(dto.Revisions[0].PreviousValue);
        Assert.Equal(500m, dto.Revisions[0].NewValue.Amount);
        Assert.Equal("REV-B", dto.Revisions[0].PerformedByActorId);

        Assert.Equal(500m, dto.Revisions[1].PreviousValue!.Amount);
        Assert.False(dto.Revisions[1].NewValue.IsKnown);
        Assert.Null(dto.Revisions[1].NewValue.Amount);

        Assert.False(dto.Revisions[2].PreviousValue!.IsKnown);
        Assert.True(dto.Revisions[2].NewValue.IsKnown);
        Assert.Equal(0m, dto.Revisions[2].NewValue.Amount);

        Assert.Equal(0m, dto.Revisions[3].PreviousValue!.Amount);
        Assert.Equal(-150m, dto.Revisions[3].NewValue.Amount);
        Assert.Equal("REV-D", dto.Revisions[3].PerformedByActorId);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-QA04")]
    public async Task CODE001_REG_QA04_DecimalScaleEquivalenceAndSourceColonDelimiters_PreserveDeterministicIdempotency()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);
        var recordUseCase = harness.CreateRecordUseCase();

        var cmdOriginal = new RecordReviewVariableCommand(
            "CASE-QA04",
            "DISB-QA04",
            "VAR-QA04-SCALE",
            UnitId,
            GroupId,
            ReviewVariableValueInput.Known(100m),
            "توحيد مقياس decimal",
            new SourceReferenceInput(SourceKind.ExcelImport, "A:B", null),
            "IDEMP-QA04-SCALE");

        var first = await recordUseCase.ExecuteAsync(cmdOriginal);
        Assert.True(first.IsSuccess);

        // إعادة الطلب بقيمة 100.00m (مطابقة حسابيًا لـ 100m) يجب أن تُعيد Replay لا IdempotencyConflict
        var sameMeaningDifferentScale = await recordUseCase.ExecuteAsync(cmdOriginal with
        {
            Value = ReviewVariableValueInput.Known(100.00m)
        });
        Assert.True(sameMeaningDifferentScale.IsSuccess, sameMeaningDifferentScale.Error?.Message);
        Assert.True(sameMeaningDifferentScale.IsReplayed);

        // اختلاف تقسيم ':' داخل SourceReference بنفس المفتاح يجب أن يُكشف كـ IdempotencyConflict لا Replay
        var sourceColonCollisionAttempt = await recordUseCase.ExecuteAsync(cmdOriginal with
        {
            Source = new SourceReferenceInput(SourceKind.ExcelImport, "A", "B:")
        });
        Assert.False(sourceColonCollisionAttempt.IsSuccess);
        Assert.Equal(ApplicationErrorCode.IdempotencyConflict, sourceColonCollisionAttempt.Error!.Code);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-QA05")]
    public async Task CODE001_REG_QA05_DefensiveGuards_RejectEmptyExplicitId_NullRevisionElement_MalformedContext_AndMapReadFailure()
    {
        Assert.True(AuditCaseId.TryCreate("CASE-QA05", out var auditCaseId));
        Assert.True(PayrollDisbursementRef.TryCreate("DISB-QA05", out var disbRef));
        Assert.True(VariableKey.TryCreate("VAR-QA05", out var varKey));
        Assert.True(ActorScope.TryCreate(UnitId, GroupId, out var scope));
        Assert.True(ActorId.TryCreate("REV-QA05", out var actorId));
        var corr = CorrelationId.NewId();
        var now = DateTimeOffset.UtcNow;

        // 1. منع تمرير ReviewVariableId فارغ عبر explicitId
        var emptyIdOutcome = ReviewVariable.Create(
            auditCaseId!,
            disbRef!,
            varKey!,
            scope!,
            ReviewVariableValue.Known(10m),
            actorId!,
            now,
            "سبب صالح",
            null,
            corr,
            explicitId: default(ReviewVariableId));
        Assert.False(emptyIdOutcome.IsSuccess);
        Assert.Equal(DomainFailureCode.InvalidInput, emptyIdOutcome.FailureCode);

        // 2. منع انهيار Reconstitute بـ NullReferenceException عند وجود عنصر null في قائمة النسخ
        var validId = ReviewVariableId.NewId();
        var rev1 = ReviewVariableRevision.Reconstitute(
            ReviewVariableRevisionId.NewId(), validId, 1, null, ReviewVariableValue.Known(10m), actorId!, now, "إنشاء", null, corr);
        var nullElementOutcome = ReviewVariable.Reconstitute(
            validId, auditCaseId!, disbRef!, varKey!, scope!, ReviewVariableValue.Known(10m), null, 2, [rev1, null!]);
        Assert.False(nullElementOutcome.IsSuccess);
        Assert.Equal(DomainFailureCode.InvalidInput, nullElementOutcome.FailureCode);

        // 3. رفض سياق مراجع تالف (AuthorizedScopes = null) برفض مصنف دون استثناء
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = new TrustedReviewerContext(actorId!, "UnitReviewer", null!, true);
        var contextDenied = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-QA05", "DISB-QA05", "VAR-QA05", UnitId, GroupId,
            ReviewVariableValueInput.Known(10m), "اختبار سياق تالف", null, "IDEMP-QA05-CTX"));
        Assert.False(contextDenied.IsSuccess);
        Assert.Equal(ApplicationErrorCode.PermissionDenied, contextDenied.Error!.Code);

        // 4. GetReviewVariableHistoryUseCase يعيد PersistenceFailure عند فشل القراءة من المستودع دون تسريب الاستثناء
        var throwingUseCase = new GetReviewVariableHistoryUseCase(
            harness.ReviewerContext,
            harness.AuthorizationPolicy,
            new ThrowingVariableRepository(),
            harness.AuditTrail);
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-QA05", UnitId, GroupId);

        var readFailure = await throwingUseCase.ExecuteAsync(new GetReviewVariableHistoryQuery(Guid.NewGuid()));
        Assert.False(readFailure.IsSuccess);
        Assert.Equal(ApplicationErrorCode.PersistenceFailure, readFailure.Error!.Code);
    }

    private sealed class ThrowingVariableRepository : IReviewVariableRepository
    {
        public Task<ReviewVariable?> GetByIdAsync(ReviewVariableId id, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated sensitive storage read failure.");

        public Task<ReviewVariable?> GetByNaturalKeyAsync(
            AuditCaseId auditCaseId,
            PayrollDisbursementRef disbursementRef,
            VariableKey key,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated sensitive storage read failure.");

        public Task SaveNewAsync(ReviewVariable variable, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<RepositorySaveResult> SaveRevisionAsync(
            ReviewVariable variable,
            ReviewVariableRevision newRevision,
            int expectedVersion,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
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
