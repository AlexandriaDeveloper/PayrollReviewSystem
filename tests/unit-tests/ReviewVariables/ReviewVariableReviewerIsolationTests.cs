using PayrollReviewSystem.Application.ReviewVariables.Contracts;
using PayrollReviewSystem.UnitTests.ReviewVariables;
using Xunit;

namespace PayrollReviewSystem.UnitTests.ReviewVariables;

/// <summary>
/// اختبارات عدم تسريب إيصالات العمليات أو المفاتيح بين مراجعَين مخوّلَين في نفس النطاق.
/// تستخدم هويات اصطناعية، وتفصل عدم التكرار لكل هوية موثوقة عن فرادة المتغير نفسه.
/// </summary>
public sealed class ReviewVariableReviewerIsolationTests
{
    private const string UnitId = "UNIT-ISOLATION";
    private const string GroupId = "GROUP-ISOLATION";
    private const string SharedIdempotencyKey = "SAME-CLIENT-RETRY-KEY";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("CaseId", "CODE001-REG-REVIEWER-RECORD")]
    public async Task Record_SameKeyByDifferentReviewers_DoesNotReplayOtherActorReceipt(bool differentPayload)
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);

        var command = new RecordReviewVariableCommand(
            "CASE-ACTOR-ISOLATION", "DISBURSEMENT-ISOLATION", "VARIABLE-ISOLATION",
            UnitId, GroupId, ReviewVariableValueInput.Known(100m),
            "تسجيل مراجع ب", null, SharedIdempotencyKey);

        var recorded = await harness.CreateRecordUseCase().ExecuteAsync(command);
        Assert.True(recorded.IsSuccess, recorded.Error?.Message);
        Assert.Equal("REV-B", recorded.Value!.PerformedByActorId);

        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-C", UnitId, GroupId);
        var otherCommand = differentPayload
            ? command with { Value = ReviewVariableValueInput.Known(200m) }
            : command;
        var otherActorResult = await harness.CreateRecordUseCase().ExecuteAsync(otherCommand);

        // لا يُعاد إيصال ب إلى ج. فرادة المفتاح الطبيعي تمنع إنشاء نفس المتغير مرتين.
        Assert.False(otherActorResult.IsSuccess);
        Assert.False(otherActorResult.IsReplayed);
        Assert.Equal(ApplicationErrorCode.DuplicateVariable, otherActorResult.Error!.Code);
        Assert.Null(otherActorResult.Value);
        Assert.Single(harness.GetAllActionLogs());
        Assert.Equal(1, harness.IdempotencyRecordCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("CaseId", "CODE001-REG-REVIEWER-REVISE")]
    public async Task Revise_SameKeyAndStaleVersionByDifferentReviewers_DoesNotReplayOtherActorReceipt(bool differentPayload)
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);
        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-CROSS-REVISE", "DISBURSEMENT-CROSS-REVISE", "VARIABLE-CROSS-REVISE",
            UnitId, GroupId, ReviewVariableValueInput.Known(100m),
            "تأسيس نسخة", null, "CREATE-ISOLATED"));
        Assert.True(created.IsSuccess, created.Error?.Message);

        var command = new ReviseReviewVariableCommand(
            created.Value!.VariableId, 1, ReviewVariableValueInput.Known(120m),
            "تعديل متزامن", null, SharedIdempotencyKey);
        var revisedByB = await harness.CreateReviseUseCase().ExecuteAsync(command);
        Assert.True(revisedByB.IsSuccess, revisedByB.Error?.Message);
        Assert.Equal("REV-B", revisedByB.Value!.PerformedByActorId);

        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-C", UnitId, GroupId);
        var otherCommand = differentPayload
            ? command with { NewValue = ReviewVariableValueInput.Known(140m) }
            : command;
        var otherActorResult = await harness.CreateReviseUseCase().ExecuteAsync(otherCommand);

        // ج يرى تعارض النسخة القديمة، لا نجاحًا معادًا باسم ب ولا تعارض مفتاح مشترك.
        Assert.False(otherActorResult.IsSuccess);
        Assert.False(otherActorResult.IsReplayed);
        Assert.Equal(ApplicationErrorCode.VersionConflict, otherActorResult.Error!.Code);
        Assert.Null(otherActorResult.Value);
        Assert.Equal(2, harness.GetAllActionLogs().Count);
        Assert.Equal(2, harness.IdempotencyRecordCount);
    }

    [Fact]
    [Trait("CaseId", "CODE001-REG-REVIEWER-INDEPENDENCE")]
    public async Task Revise_AuthorizedSecondReviewerCanCommitNewVersion_UsingSameRetryKey()
    {
        var harness = new InMemoryReviewVariableTestHarness();
        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-B", UnitId, GroupId);
        var created = await harness.CreateRecordUseCase().ExecuteAsync(new RecordReviewVariableCommand(
            "CASE-CROSS-INDEPENDENCE", "DISB-CROSS-INDEPENDENCE", "VAR-CROSS-INDEPENDENCE",
            UnitId, GroupId, ReviewVariableValueInput.Known(100m),
            "تسجيل أولي", null, "CREATE-INDEPENDENCE"));
        Assert.True(created.IsSuccess, created.Error?.Message);

        var actorB = await harness.CreateReviseUseCase().ExecuteAsync(new ReviseReviewVariableCommand(
            created.Value!.VariableId, 1, ReviewVariableValueInput.Known(120m),
            "تعديل ب", null, SharedIdempotencyKey));
        Assert.True(actorB.IsSuccess, actorB.Error?.Message);

        harness.ReviewerContext.Current = InMemoryReviewVariableTestHarness.BuildReviewerContext("REV-C", UnitId, GroupId);
        var commandC = new ReviseReviewVariableCommand(
            created.Value.VariableId, 2, ReviewVariableValueInput.Known(140m),
            "تعديل ج", null, SharedIdempotencyKey);
        var actorC = await harness.CreateReviseUseCase().ExecuteAsync(commandC);
        Assert.True(actorC.IsSuccess, actorC.Error?.Message);
        Assert.False(actorC.IsReplayed);
        Assert.Equal(3, actorC.Value!.Version);
        Assert.Equal("REV-C", actorC.Value.PerformedByActorId);

        // إعادة طلب ج فقط تُعيد إيصال ج وتحتفظ بسجل تاريخي واحد لكل تنفيذ.
        var replayC = await harness.CreateReviseUseCase().ExecuteAsync(commandC);
        Assert.True(replayC.IsSuccess, replayC.Error?.Message);
        Assert.True(replayC.IsReplayed);
        Assert.Equal("REV-C", replayC.Value!.PerformedByActorId);
        Assert.Equal(3, harness.GetAllActionLogs().Count);
        Assert.Equal(3, harness.IdempotencyRecordCount);
    }
}
