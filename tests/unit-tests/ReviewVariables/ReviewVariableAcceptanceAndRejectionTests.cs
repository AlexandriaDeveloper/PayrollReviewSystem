using PayrollReviewSystem.Application.ReviewVariables.Contracts;
using PayrollReviewSystem.Core.ReviewVariables;
using Xunit;

namespace PayrollReviewSystem.UnitTests.ReviewVariables;

/// <summary>
/// اختبارات القبول والرفض الإلزامية لشريحة CODE-001 (<c>CODE001-ACC-01..04</c> و<c>CODE001-NEG-01..09</c>).
/// تعتمد جميع الحالات على بيانات اصطناعية تمامًا خالية من أي بيانات موظفين حقيقية.
/// </summary>
public class ReviewVariableAcceptanceAndRejectionTests
{
    private const string DemoAuditCaseId = "CASE-DEMO-202611-01";
    private const string DemoDisbursementId = "DISB-DEMO-MAIN-01";
    private const string DemoVariableKey = "INCENTIVE_ADJ_DEMO";
    private const string DemoUnitId = "UNIT-DEMO-01";
    private const string DemoGroupId = "GRP-DEMO-A";

    [Fact]
    [Trait("CaseId", "CODE001-ACC-01")]
    public async Task CODE001_ACC_01_AuthorizedReviewerB_CanReviseVariable_WhileReviewerAIsAbsent_AttributedOnlyToB()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");

        var recordUseCase = harness.CreateRecordUseCase();
        var reviseUseCase = harness.CreateReviseUseCase();
        var historyUseCase = harness.CreateHistoryUseCase();

        // إنشاء متغير مبدئي بقيمة 1000
        var created = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1000m),
            "إثبات القيمة المبدئية للمراجعة",
            new SourceReferenceInput(SourceKind.DirectForm, "DF-V1"),
            "IDEMP-REC-ACC01"));

        Assert.True(created.IsSuccess);
        Assert.NotNull(created.Value);

        // غياب المراجع A: المراجع المخوّل B يعدل القيمة مباشرة من 1000 إلى 1200 دون انتظار موافقة A
        harness.Clock.UtcNow = harness.Clock.UtcNow.AddMinutes(10);
        var revised = await reviseUseCase.ExecuteAsync(new ReviseReviewVariableCommand(
            created.Value.VariableId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(1200m),
            "تعديل استحقاق بناءً على مستند المراجعة الاصطناعي",
            new SourceReferenceInput(SourceKind.DirectForm, "DF-V2"),
            "IDEMP-REV-ACC01"));

        Assert.True(revised.IsSuccess);
        Assert.False(revised.IsReplayed);
        Assert.NotNull(revised.Value);
        Assert.Equal(2, revised.Value.Version);
        Assert.Equal(1200m, revised.Value.CurrentValue.Amount);
        Assert.Equal("REV-B", revised.Value.PerformedByActorId);

        var history = await historyUseCase.ExecuteAsync(new GetReviewVariableHistoryQuery(created.Value.VariableId));
        Assert.True(history.IsSuccess);
        Assert.Equal(2, history.Value!.Revisions.Count);
        Assert.All(history.Value.ActionLogs, log => Assert.Equal("REV-B", log.PerformedByActorId));
        Assert.DoesNotContain(history.Value.ActionLogs, log => log.PerformedByActorId == "REV-A");
    }

    [Fact]
    [Trait("CaseId", "CODE001-ACC-02")]
    public async Task CODE001_ACC_02_SequentialRevisionsByBThenC_PreserveFullHistory1000To1200To1350_WithActorsAndUtcTimes()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        var t0 = new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero);
        var t1 = t0.AddMinutes(15);
        var t2 = t0.AddMinutes(30);

        // البداية بقيمة 1000 بواسطة B
        harness.Clock.UtcNow = t0;
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");
        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1000m),
            "إدخال أولي",
            null,
            "IDEMP-ACC02-1"));

        Assert.True(created.IsSuccess);
        var varId = created.Value!.VariableId;

        // تعديل B: 1000 -> 1200
        harness.Clock.UtcNow = t1;
        var revB = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(1200m),
            "تصحيح B",
            null,
            "IDEMP-ACC02-2"));

        Assert.True(revB.IsSuccess);
        Assert.Equal(2, revB.Value!.Version);

        // قراءة النسخة الجديدة وتعديل C: 1200 -> 1350
        harness.Clock.UtcNow = t2;
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-C");
        var revC = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId,
            ExpectedVersion: 2,
            ReviewVariableValueInput.Known(1350m),
            "تصحيح C",
            null,
            "IDEMP-ACC02-3"));

        Assert.True(revC.IsSuccess);
        Assert.Equal(3, revC.Value!.Version);

        var history = await harness.CreateHistoryUseCase().ExecuteAsync(new GetReviewVariableHistoryQuery(varId));
        Assert.True(history.IsSuccess);
        var dto = history.Value!;

        Assert.Equal(3, dto.CurrentVersion);
        Assert.Equal(1350m, dto.CurrentValue.Amount);
        Assert.Equal(3, dto.Revisions.Count);

        Assert.Null(dto.Revisions[0].PreviousValue);
        Assert.Equal(1000m, dto.Revisions[0].NewValue.Amount);
        Assert.Equal("REV-B", dto.Revisions[0].PerformedByActorId);
        Assert.Equal(t0, dto.Revisions[0].OccurredAtUtc);

        Assert.Equal(1000m, dto.Revisions[1].PreviousValue!.Amount);
        Assert.Equal(1200m, dto.Revisions[1].NewValue.Amount);
        Assert.Equal("REV-B", dto.Revisions[1].PerformedByActorId);
        Assert.Equal(t1, dto.Revisions[1].OccurredAtUtc);

        Assert.Equal(1200m, dto.Revisions[2].PreviousValue!.Amount);
        Assert.Equal(1350m, dto.Revisions[2].NewValue.Amount);
        Assert.Equal("REV-C", dto.Revisions[2].PerformedByActorId);
        Assert.Equal(t2, dto.Revisions[2].OccurredAtUtc);

        Assert.Equal(3, dto.ActionLogs.Count);
        Assert.Equal(["REV-B", "REV-B", "REV-C"], dto.ActionLogs.Select(x => x.PerformedByActorId).ToArray());
    }

    [Fact]
    [Trait("CaseId", "CODE001-ACC-03")]
    public async Task CODE001_ACC_03_KnownZero_IsDistinctFromUnknown_AndUnknownNeverBecomesZero()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");
        var recordUseCase = harness.CreateRecordUseCase();

        // متغير بقيمة معلومة تساوي صفرًا
        var knownZeroResult = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_KNOWN_ZERO",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(0m),
            "قيمة صفرية معلومة وموثقة",
            null,
            "IDEMP-ZERO-01"));

        // متغير آخر بقيمة مجهولة
        var unknownResult = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_UNKNOWN_MISSING",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Unknown(),
            "البيان غير متوفر في المستند (INPUT_MISSING)",
            null,
            "IDEMP-UNK-01"));

        Assert.True(knownZeroResult.IsSuccess);
        Assert.True(unknownResult.IsSuccess);

        Assert.True(knownZeroResult.Value!.CurrentValue.IsKnown);
        Assert.Equal(0m, knownZeroResult.Value.CurrentValue.Amount);

        Assert.False(unknownResult.Value!.CurrentValue.IsKnown);
        Assert.Null(unknownResult.Value.CurrentValue.Amount);

        // التحقق على مستوى كائن القيمة في النواة نفسه
        Assert.True(ReviewVariableValue.TryCreateKnown(0m, out var knownZeroCore, out _));
        Assert.NotNull(knownZeroCore);
        Assert.NotEqual(knownZeroCore, ReviewVariableValue.Unknown);
        Assert.False(ReviewVariableValue.Unknown.TryGetKnownAmount(out _));
    }

    [Theory]
    [Trait("CaseId", "CODE001-ACC-04")]
    [InlineData(SourceKind.ExcelImport, "EXCEL-BATCH-2026-11")]
    [InlineData(SourceKind.DirectForm, "DIRECT-ENTRY-SESSION-07")]
    public async Task CODE001_ACC_04_AlternativeSources_KeepExactSourceKind_AndNeverClaimOfficialOracle(
        SourceKind alternativeKind,
        string sourceVersionId)
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");

        var result = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            $"VAR_{alternativeKind}",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(500m),
            "تسجيل من قناة بديلة",
            new SourceReferenceInput(alternativeKind, sourceVersionId, "CAPTURE-REF-01"),
            $"IDEMP-{alternativeKind}"));

        Assert.True(result.IsSuccess);
        var source = result.Value!.SourceReference;
        Assert.NotNull(source);
        Assert.Equal(alternativeKind, source.Kind);
        Assert.NotEqual(SourceKind.OracleOfficial, source.Kind);
        Assert.False(source.IsOfficialOracle);
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-01")]
    public async Task CODE001_NEG_01_UnauthenticatedOrOutOfScopeReviewer_ReturnsPermissionDenied_WithZeroSideEffects()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        var recordUseCase = harness.CreateRecordUseCase();

        // 1. جلسة غير موثقة (null)
        harness.ReviewerContext.Current = null;
        var unauthenticated = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1000m),
            "محاولة بلا مصادقة",
            null,
            "IDEMP-NEG01-A"));

        Assert.False(unauthenticated.IsSuccess);
        Assert.Equal(ApplicationErrorCode.PermissionDenied, unauthenticated.Error!.Code);

        // 2. مراجع موثق لكن في مجموعة أخرى (GRP-DEMO-B بدل GRP-DEMO-A)
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext(
            "REV-OUTSIDE",
            DemoUnitId,
            "GRP-DEMO-B");

        var outOfScope = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1000m),
            "محاولة خارج النطاق",
            null,
            "IDEMP-NEG01-B"));

        Assert.False(outOfScope.IsSuccess);
        Assert.Equal(ApplicationErrorCode.PermissionDenied, outOfScope.Error!.Code);
        Assert.Equal(0, harness.StoredVariableCount);
        Assert.Empty(harness.GetAllActionLogs());
        Assert.Equal(0, harness.IdempotencyRecordCount);
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-02")]
    public async Task CODE001_NEG_02_ActorSpoofingAttempt_ClaimingActorAInSessionB_IsRejectedAndNeverAttributedToA()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");

        var spoofedRecord = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(900m),
            "محاولة انتحال هوية المراجع A",
            null,
            "IDEMP-SPOOF-01",
            ClaimedActorId: "REV-A"));

        Assert.False(spoofedRecord.IsSuccess);
        Assert.Equal(ApplicationErrorCode.AttributionMismatchRejected, spoofedRecord.Error!.Code);
        Assert.Equal(0, harness.StoredVariableCount);
        Assert.Empty(harness.GetAllActionLogs());
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-03")]
    public async Task CODE001_NEG_03_StaleExpectedVersion_ReturnsVersionConflict_AndPreservesWinningRevision()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");

        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1000m),
            "القيمة الأولى",
            null,
            "IDEMP-CONC-REC"));

        var varId = created.Value!.VariableId;

        // B و C يقرآن Version = 1؛ ثم B يحفظ أولًا (Version -> 2)
        var reviseB = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(1200m),
            "تحديث B الفائز",
            null,
            "IDEMP-CONC-B"));

        Assert.True(reviseB.IsSuccess);
        Assert.Equal(2, reviseB.Value!.Version);

        // C يحاول التحديث بنسخة قديمة ExpectedVersion = 1
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-C");
        var reviseCStale = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(1400m),
            "تحديث C بنسخة قديمة",
            null,
            "IDEMP-CONC-C"));

        Assert.False(reviseCStale.IsSuccess);
        Assert.Equal(ApplicationErrorCode.VersionConflict, reviseCStale.Error!.Code);
        Assert.NotNull(reviseCStale.Error.ConflictInfo);
        Assert.Equal(1, reviseCStale.Error.ConflictInfo.ExpectedVersion);
        Assert.Equal(2, reviseCStale.Error.ConflictInfo.ActualVersion);

        // بقاء نسخة B سليمة دون كتابة فوقها
        var history = await harness.CreateHistoryUseCase().ExecuteAsync(new GetReviewVariableHistoryQuery(varId));
        Assert.Equal(2, history.Value!.CurrentVersion);
        Assert.Equal(1200m, history.Value.CurrentValue.Amount);
        Assert.Equal(2, history.Value.Revisions.Count);
        Assert.Equal(2, history.Value.ActionLogs.Count);
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-04")]
    public async Task CODE001_NEG_04_SameIdempotencyKeyAndSamePayload_ReplaysOriginalResult_WithoutDuplicateRevisionsOrLogs()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");
        var recordUseCase = harness.CreateRecordUseCase();
        var reviseUseCase = harness.CreateReviseUseCase();

        var recordCmd = new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1000m),
            "إدخال مبدئي",
            new SourceReferenceInput(SourceKind.ExcelImport, "XLS-01"),
            "IDEMP-REPLAY-REC");

        var firstRecord = await recordUseCase.ExecuteAsync(recordCmd);
        var replayRecord = await recordUseCase.ExecuteAsync(recordCmd);

        Assert.True(firstRecord.IsSuccess);
        Assert.False(firstRecord.IsReplayed);
        Assert.True(replayRecord.IsSuccess);
        Assert.True(replayRecord.IsReplayed);
        Assert.Equal(firstRecord.Value, replayRecord.Value);
        Assert.Equal(1, harness.StoredVariableCount);
        Assert.Single(harness.GetAllActionLogs());

        var reviseCmd = new ReviseReviewVariableCommand(
            firstRecord.Value!.VariableId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(1250m),
            "تعديل قابل للإعادة",
            null,
            "IDEMP-REPLAY-REV");

        var firstRevise = await reviseUseCase.ExecuteAsync(reviseCmd);
        var replayRevise = await reviseUseCase.ExecuteAsync(reviseCmd);

        Assert.True(firstRevise.IsSuccess);
        Assert.False(firstRevise.IsReplayed);
        Assert.True(replayRevise.IsSuccess);
        Assert.True(replayRevise.IsReplayed);
        Assert.Equal(firstRevise.Value, replayRevise.Value);

        var history = await harness.CreateHistoryUseCase().ExecuteAsync(
            new GetReviewVariableHistoryQuery(firstRecord.Value.VariableId));
        Assert.Equal(2, history.Value!.CurrentVersion);
        Assert.Equal(2, history.Value.Revisions.Count);
        Assert.Equal(2, history.Value.ActionLogs.Count);
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-05")]
    public async Task CODE001_NEG_05_SameIdempotencyKeyWithDifferentValueOrReason_ReturnsIdempotencyConflict_WithNoSideEffects()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");
        var recordUseCase = harness.CreateRecordUseCase();
        var reviseUseCase = harness.CreateReviseUseCase();

        var created = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1000m),
            "سبب أصلي",
            null,
            "SHARED-KEY-01"));

        Assert.True(created.IsSuccess);

        // نفس مفتاح التسجيل بقيمة مختلفة لنفس الهدف
        var conflictingRecord = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1500m),
            "سبب أصلي",
            null,
            "SHARED-KEY-01"));

        Assert.False(conflictingRecord.IsSuccess);
        Assert.Equal(ApplicationErrorCode.IdempotencyConflict, conflictingRecord.Error!.Code);

        // تجربة تعارض الحمولة في التعديل (قيمة مختلفة أو سبب مختلف بنفس مفتاح التعديل)
        var firstRevise = await reviseUseCase.ExecuteAsync(new ReviseReviewVariableCommand(
            created.Value!.VariableId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(1100m),
            "تعديل أول",
            null,
            "SHARED-REV-KEY"));
        Assert.True(firstRevise.IsSuccess);

        var conflictingRevise = await reviseUseCase.ExecuteAsync(new ReviseReviewVariableCommand(
            created.Value.VariableId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(1100m),
            "تغيير السبب بنفس المفتاح",
            null,
            "SHARED-REV-KEY"));

        Assert.False(conflictingRevise.IsSuccess);
        Assert.Equal(ApplicationErrorCode.IdempotencyConflict, conflictingRevise.Error!.Code);

        var history = await harness.CreateHistoryUseCase().ExecuteAsync(
            new GetReviewVariableHistoryQuery(created.Value.VariableId));
        Assert.Equal(2, history.Value!.CurrentVersion);
        Assert.Equal(1100m, history.Value.CurrentValue.Amount);
        Assert.Equal(2, history.Value.ActionLogs.Count);
    }

    [Theory]
    [Trait("CaseId", "CODE001-NEG-06")]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public async Task CODE001_NEG_06_PersistenceFailureInAnyAtomicStep_RollsBackCompletely_WithoutPartialHistory(
        bool failVariableSave,
        bool failAuditAppend,
        bool failIdempotencySave)
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");

        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1000m),
            "بداية سليمة",
            null,
            "IDEMP-ATOMIC-INIT"));
        Assert.True(created.IsSuccess);
        var varId = created.Value!.VariableId;

        // تفعيل العطل المتعمد في إحدى خطوات الكتابة الذرية
        harness.FailOnVariableSave = failVariableSave;
        harness.FailOnAuditAppend = failAuditAppend;
        harness.FailOnIdempotencySave = failIdempotencySave;

        var failedRevise = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(1300m),
            "تعديل يُفترض تراجعه عند العطل",
            null,
            "IDEMP-ATOMIC-FAIL"));

        Assert.False(failedRevise.IsSuccess);
        Assert.Equal(ApplicationErrorCode.PersistenceFailure, failedRevise.Error!.Code);

        // التحقق من التراجع الكامل: لا نسخة ثانية، لا سجل فعل ثانٍ، ولا سجل idempotency عالق
        var history = await harness.CreateHistoryUseCase().ExecuteAsync(new GetReviewVariableHistoryQuery(varId));
        Assert.True(history.IsSuccess);
        Assert.Equal(1, history.Value!.CurrentVersion);
        Assert.Equal(1000m, history.Value.CurrentValue.Amount);
        Assert.Single(history.Value.Revisions);
        Assert.Single(history.Value.ActionLogs);
        Assert.Equal(1, harness.IdempotencyRecordCount);
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-08")]
    public async Task CODE001_NEG_08_NoMandatoryPrimaryReviewerQueue_ReviewerCProceedsImmediatelyWhenAIsAbsent()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        var varId = Guid.Empty;

        // المراجع B ينشئ ويعدّل بدون المراجع A
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");
        var rec = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(800m),
            "إدخال B أثناء غياب A",
            null,
            "IDEMP-NEG08-1"));
        Assert.True(rec.IsSuccess);
        varId = rec.Value!.VariableId;

        // المراجع C المخوّل في نفس النطاق يكمل فورًا دون أي طابور اعتماد أو توقف على A أو B
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-C");
        var rev = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(950m),
            "متابعة C مباشرةً دون انتظار مراجع أساسي",
            null,
            "IDEMP-NEG08-2"));

        Assert.True(rev.IsSuccess);
        Assert.Equal(2, rev.Value!.Version);
        Assert.Equal(950m, rev.Value.CurrentValue.Amount);
        Assert.Equal("REV-C", rev.Value.PerformedByActorId);
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-09")]
    public async Task CODE001_NEG_09_RejectsInvalidInputs_NegativeAmounts_AndNonDeterministicFloats_WithZeroPii()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");
        var recordUseCase = harness.CreateRecordUseCase();

        // 1. رفض double وfloat غير الحتميين
        var doubleRejected = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_DOUBLE",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.FromExternalInput(1200.50d),
            "اختبار رفض double",
            null,
            "IDEMP-NEG09-DBL"));

        var floatRejected = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_FLOAT",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.FromExternalInput(1200.50f),
            "اختبار رفض float",
            null,
            "IDEMP-NEG09-FLT"));

        // 2. رفض المبلغ السالب
        var negativeRejected = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_NEG",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(-50m),
            "اختبار رفض قيمة سالبة",
            null,
            "IDEMP-NEG09-MINUS"));

        // 3. رفض مدخل مفقود بدون استدعاء Unknown()
        var nullValueRejected = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_NULL",
            DemoUnitId,
            DemoGroupId,
            null,
            "اختبار رفض قيمة مفقودة غير مصرح بها",
            null,
            "IDEMP-NEG09-NULL"));

        // 4. رفض سبب فارغ
        var emptyReasonRejected = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_EMPTY_REASON",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(100m),
            "   ",
            null,
            "IDEMP-NEG09-RSN"));

        Assert.Equal(ApplicationErrorCode.InvalidVariableValue, doubleRejected.Error!.Code);
        Assert.Equal(ApplicationErrorCode.InvalidVariableValue, floatRejected.Error!.Code);
        Assert.Equal(ApplicationErrorCode.InvalidVariableValue, negativeRejected.Error!.Code);
        Assert.Equal(ApplicationErrorCode.InputMissing, nullValueRejected.Error!.Code);
        Assert.Equal(ApplicationErrorCode.InvalidVariableValue, emptyReasonRejected.Error!.Code);

        // قبول decimal الحتمي بدقة
        var deterministicDecimal = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_DEC_EXACT",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.FromExternalInput(0.10m + 0.20m),
            "اختبار دقة decimal",
            null,
            "IDEMP-NEG09-DEC"));

        Assert.True(deterministicDecimal.IsSuccess);
        Assert.Equal(0.30m, deterministicDecimal.Value!.CurrentValue.Amount);
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-03")]
    public async Task CODE001_NEG_03_ParallelConcurrentRevisions_WithSameExpectedVersion_AllowsOneAndRejectsOtherWithVersionConflict()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");

        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(1000m),
            "متغير لاختبار التزامن المتوازي",
            null,
            "IDEMP-PAR-INIT"));
        Assert.True(created.IsSuccess);
        var varId = created.Value!.VariableId;

        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<ApplicationResult<ReviewVariableReceipt>> RunRevisionAsync(string actorId, decimal amount, string idempKey)
        {
            var localHarnessContext = InMemoryReviewVariableTestHarness.BuildReviewerContext(actorId);
            var useCase = new PayrollReviewSystem.Application.ReviewVariables.UseCases.ReviseReviewVariableUseCase(
                new StaticReviewerContextPort(localHarnessContext),
                harness.AuthorizationPolicy,
                harness.VariableRepository,
                harness.AuditTrail,
                harness.Idempotency,
                harness.AtomicTransaction,
                harness.Clock);

            await barrier.Task;
            return await useCase.ExecuteAsync(new ReviseReviewVariableCommand(
                varId,
                ExpectedVersion: 1,
                ReviewVariableValueInput.Known(amount),
                $"تعديل متوازٍ من {actorId}",
                null,
                idempKey));
        }

        var taskB = RunRevisionAsync("REV-B", 1200m, "IDEMP-PAR-B");
        var taskC = RunRevisionAsync("REV-C", 1300m, "IDEMP-PAR-C");
        barrier.SetResult();

        var results = await Task.WhenAll(taskB, taskC);

        Assert.Single(results, r => r.IsSuccess);
        var failed = Assert.Single(results, r => !r.IsSuccess);
        Assert.Equal(ApplicationErrorCode.VersionConflict, failed.Error!.Code);

        var history = await harness.CreateHistoryUseCase().ExecuteAsync(new GetReviewVariableHistoryQuery(varId));
        Assert.Equal(2, history.Value!.CurrentVersion);
        Assert.Equal(2, history.Value.Revisions.Count);
        Assert.Equal(2, history.Value.ActionLogs.Count);
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-01")]
    public async Task CODE001_NEG_01_ReadOnlyActor_CanViewHistoryInScope_ButCannotRecordOrRevise_AndOutOfScopeCannotViewHistory()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");

        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            DemoVariableKey,
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(200m),
            "إنشاء أولي",
            null,
            "IDEMP-RO-1"));
        var varId = created.Value!.VariableId;

        // دور قراءة فقط داخل نفس النطاق: يُسمح بالعرض ويُرفض التعديل
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext(
            "AUDITOR-VIEW",
            DemoUnitId,
            DemoGroupId,
            roleName: "ReadOnlyObserver",
            canModify: false);

        var viewOk = await harness.CreateHistoryUseCase().ExecuteAsync(new GetReviewVariableHistoryQuery(varId));
        var reviseDenied = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            varId,
            ExpectedVersion: 1,
            ReviewVariableValueInput.Known(250m),
            "محاولة تعديل من دور قراءة فقط",
            null,
            "IDEMP-RO-2"));

        Assert.True(viewOk.IsSuccess);
        Assert.False(reviseDenied.IsSuccess);
        Assert.Equal(ApplicationErrorCode.PermissionDenied, reviseDenied.Error!.Code);

        // مستخدم خارج النطاق يُرفض عند محاولة قراءة السجل (حماية IDOR)
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext(
            "REV-OTHER-GRP",
            DemoUnitId,
            "GRP-DEMO-Z");
        var viewDenied = await harness.CreateHistoryUseCase().ExecuteAsync(new GetReviewVariableHistoryQuery(varId));
        Assert.False(viewDenied.IsSuccess);
        Assert.Equal(ApplicationErrorCode.PermissionDenied, viewDenied.Error!.Code);
    }

    [Fact]
    [Trait("CaseId", "CODE001-ACC-01")]
    public async Task CODE001_TwoDistinctVariables_WithSameAmount_DoNotCollide_WhileDuplicateNaturalKeyIsRejected()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B");
        var recordUseCase = harness.CreateRecordUseCase();

        var first = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_FIRST_200",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(200m),
            "بند أول بقيمة 200",
            null,
            "IDEMP-SAMEVAL-1"));

        var secondDifferentKey = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_SECOND_200",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(200m),
            "بند ثانٍ مشروع بنفس القيمة 200",
            null,
            "IDEMP-SAMEVAL-2"));

        // نفس المفتاح الطبيعي بمفتاح عدم تكرار جديد يجب رفضه لمنع الازدواج
        var duplicateNaturalKey = await recordUseCase.ExecuteAsync(new RecordReviewVariableCommand(
            DemoAuditCaseId,
            DemoDisbursementId,
            "VAR_FIRST_200",
            DemoUnitId,
            DemoGroupId,
            ReviewVariableValueInput.Known(300m),
            "محاولة إعادة إنشاء نفس المتغير",
            null,
            "IDEMP-DUP-NATKEY"));

        Assert.True(first.IsSuccess);
        Assert.True(secondDifferentKey.IsSuccess);
        Assert.NotEqual(first.Value!.VariableId, secondDifferentKey.Value!.VariableId);
        Assert.False(duplicateNaturalKey.IsSuccess);
        Assert.Equal(ApplicationErrorCode.DuplicateVariable, duplicateNaturalKey.Error!.Code);
    }

    private sealed class StaticReviewerContextPort(PayrollReviewSystem.Application.ReviewVariables.Ports.TrustedReviewerContext context)
        : PayrollReviewSystem.Application.ReviewVariables.Ports.IReviewerContextPort
    {
        public PayrollReviewSystem.Application.ReviewVariables.Ports.TrustedReviewerContext? GetCurrentReviewer() => context;
    }
}
