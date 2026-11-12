using PayrollReviewSystem.Core;

namespace PayrollReviewSystem.Application;

/// <summary>
/// علامة مرجعية لطبقة التطبيق (Application) في Clean Architecture.
/// تعتمد هذه الطبقة حصريًا على طبقة النواة (Core) وتنسق حالات الاستخدام (Use Cases) والعقود والواجهات (Interfaces).
/// </summary>
public static class ApplicationMarker
{
    public static readonly string LayerName = "Application";
    public static readonly string CoreLayerName = CoreMarker.LayerName;
    public static readonly Type CoreDependency = typeof(CoreMarker);
}
