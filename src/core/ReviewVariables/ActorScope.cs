namespace PayrollReviewSystem.Core.ReviewVariables;

/// <summary>
/// نطاق عمل المراجع (الوحدة + مجموعة المراجعة الشهرية).
/// يُستخدم لمنع وصول أو تعديل أي مراجع خارج وحدته أو مجموعته المصرح بها، مع تطبيق المقارنة الحساسة لقيم النطاق.
/// </summary>
public sealed record ActorScope
{
    /// <summary>معرّف الوحدة التنظيمية/الحسابية المصرح بها.</summary>
    public string UnitId { get; }

    /// <summary>معرّف مجموعة المراجعة الشهرية المصرح بها.</summary>
    public string ReviewGroupId { get; }

    private ActorScope(string unitId, string reviewGroupId)
    {
        UnitId = unitId;
        ReviewGroupId = reviewGroupId;
    }

    /// <summary>
    /// ينشئ نطاق مراجعة موثقًا بعد التحقق من وجود معرّف الوحدة والمجموعة.
    /// </summary>
    public static bool TryCreate(string? unitId, string? reviewGroupId, out ActorScope? scope)
    {
        if (string.IsNullOrWhiteSpace(unitId) || string.IsNullOrWhiteSpace(reviewGroupId))
        {
            scope = null;
            return false;
        }

        scope = new ActorScope(unitId.Trim(), reviewGroupId.Trim());
        return true;
    }

    /// <summary>
    /// يرجع تمثيلًا نصيًا قياسيًا للنطاق لاستخدامه في مفاتيح عدم التكرار (Idempotency) والمقارنة.
    /// </summary>
    public string ToCanonicalString() => $"{UnitId}::{ReviewGroupId}";

    /// <inheritdoc />
    public override string ToString() => ToCanonicalString();
}
