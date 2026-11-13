using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.Internal;

/// <summary>
/// مولّد بصمة معيارية حتمية (SHA-256) لحمولة الطلبات لاستخدامها في منع التكرار (Idempotency).
/// يكتب كل حقل مسبوقًا بطوله لتفادي تصادم الحدود (Delimiter Collision) بين النصوص المتقاربة.
/// </summary>
internal static class CanonicalPayloadHasher
{
    /// <summary>
    /// يحسب بصمة سداسية عشريّة ثابتة لأمر تسجيل متغير جديد.
    /// </summary>
    public static string HashRecordCommand(
        AuditCaseId auditCaseId,
        PayrollDisbursementRef disbursementRef,
        VariableKey key,
        ActorScope scope,
        ReviewVariableValue value,
        string reason,
        SourceReference? sourceReference)
    {
        var parts = new[]
        {
            "RECORD_V1",
            auditCaseId.Value,
            disbursementRef.Value,
            key.Value,
            scope.UnitId,
            scope.ReviewGroupId,
            SerializeValue(value),
            reason,
            SerializeSource(sourceReference)
        };

        return ComputeSha256Hex(parts);
    }

    /// <summary>
    /// يحسب بصمة سداسية عشريّة ثابتة لأمر تعديل متغير قائم.
    /// </summary>
    public static string HashReviseCommand(
        ReviewVariableId variableId,
        int expectedVersion,
        ReviewVariableValue newValue,
        string changeReason,
        SourceReference? sourceReference)
    {
        var parts = new[]
        {
            "REVISE_V1",
            variableId.ToString(),
            expectedVersion.ToString(CultureInfo.InvariantCulture),
            SerializeValue(newValue),
            changeReason,
            SerializeSource(sourceReference)
        };

        return ComputeSha256Hex(parts);
    }

    private static string SerializeValue(ReviewVariableValue value)
        => value.IsKnown && value.Amount.HasValue
            ? "KNOWN:" + value.Amount.Value.ToString("G29", CultureInfo.InvariantCulture)
            : "UNKNOWN";

    private static string SerializeSource(SourceReference? source)
    {
        if (source is null)
        {
            return "NO_SOURCE";
        }

        return $"{(int)source.Kind}:{source.SourceVersionId}:{source.SourceCaptureRef ?? string.Empty}";
    }

    private static string ComputeSha256Hex(IReadOnlyList<string> fields)
    {
        using var sha = SHA256.Create();
        var buffer = new MemoryStream();
        using (var writer = new BinaryWriter(buffer, Encoding.UTF8, leaveOpen: true))
        {
            foreach (var field in fields)
            {
                var bytes = Encoding.UTF8.GetBytes(field);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }

        buffer.Position = 0;
        var hash = sha.ComputeHash(buffer);
        return Convert.ToHexString(hash);
    }
}
