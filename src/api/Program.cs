using PayrollReviewSystem.Application;
using PayrollReviewSystem.Core;
using PayrollReviewSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// إعداد سياسة CORS للواجهة الأمامية (Angular Client)
const string AllowClientOrigin = "AllowClientApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowClientOrigin, policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors(AllowClientOrigin);

/// <summary>
/// نقطة فحص الصحة التشغيلية (Health Endpoint)
/// لا تتطلب اتصالاً بقاعدة البيانات أو بيانات حساسة أو أسرار.
/// </summary>
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "PayrollReviewSystem.Api",
    version = "1.0.0",
    scaffold = "BOOT-001",
    timestamp = DateTimeOffset.UtcNow
}));

/// <summary>
/// نقطة الاستعلام عن معلومات الهيكل التقني الأولي
/// </summary>
app.MapGet("/api/info", () => Results.Ok(new
{
    system = "PayrollReviewSystem",
    architecture = "Clean Architecture / Modular Monolith",
    coreLayer = CoreMarker.LayerName,
    applicationLayer = ApplicationMarker.LayerName,
    infrastructureLayer = InfrastructureMarker.LayerName,
    targetDatabase = InfrastructureMarker.TargetDatabase,
    databaseStatus = InfrastructureMarker.DatabaseStatus,
    stage = "Technical Scaffold Initialized (BOOT-001)"
}));

app.Run();

// إتاحة الوصول لـ Program من مشاريع الاختبارات
public partial class Program { }
