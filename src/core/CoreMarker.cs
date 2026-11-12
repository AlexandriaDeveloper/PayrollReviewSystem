namespace PayrollReviewSystem.Core;

/// <summary>
/// علامة مرجعية لطبقة النواة (Core) في Clean Architecture.
/// تظل هذه الطبقة نقية تمامًا وخالية من أي تبعيات لمكتبات خارجية، أو قواعد بيانات، أو أدوات استخراج، أو منطق الويب.
/// </summary>
public static class CoreMarker
{
    public static readonly string LayerName = "Core";
    public static readonly string Status = "Technical Scaffold Initialized";
}
