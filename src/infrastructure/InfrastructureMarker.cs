using PayrollReviewSystem.Application;
using PayrollReviewSystem.Core;

namespace PayrollReviewSystem.Infrastructure;

/// <summary>
/// علامة مرجعية لطبقة البنية التحتية (Infrastructure) في Clean Architecture.
/// تعتمد هذه الطبقة على طبقتي النواة (Core) والتطبيق (Application).
/// تظل خالية تمامًا في مرحلة BOOT-001 من أي اتصالات بقواعد البيانات أو مكتبات ORM.
/// </summary>
public static class InfrastructureMarker
{
    public static readonly string LayerName = "Infrastructure";
    public static readonly Type CoreDependency = typeof(CoreMarker);
    public static readonly Type ApplicationDependency = typeof(ApplicationMarker);
    public static readonly string TargetDatabase = "SQL Server 2014 (12.x)";
    public static readonly string DatabaseStatus = "NOT_DESIGNED / NO_TABLES";
}
