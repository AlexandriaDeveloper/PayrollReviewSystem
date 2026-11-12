using System.Reflection;
using PayrollReviewSystem.Application;
using PayrollReviewSystem.Core;
using PayrollReviewSystem.Infrastructure;
using Xunit;

namespace PayrollReviewSystem.ArchitectureTests;

/// <summary>
/// اختبارات معمارية تلقائية للتحقق الصارم من قواعد اتجاه التبعيات في Clean Architecture.
/// تمنع هذه الاختبارات حدوث أي كسر معماري أو تسريب تبعيات أو تدوير (Circular Dependencies).
/// </summary>
public class CleanArchitectureDependencyTests
{
    private readonly Assembly _coreAssembly = typeof(CoreMarker).Assembly;
    private readonly Assembly _applicationAssembly = typeof(ApplicationMarker).Assembly;
    private readonly Assembly _infrastructureAssembly = typeof(InfrastructureMarker).Assembly;
    private readonly Assembly _apiAssembly = typeof(Program).Assembly;

    [Fact]
    public void CoreLayer_ShouldNotDependOn_AnyOtherLayer()
    {
        // النواة (Core) يجب ألا تعتمد على أي طبقة أخرى
        var referencedAssemblyNames = _coreAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.DoesNotContain("application", referencedAssemblyNames);
        Assert.DoesNotContain("infrastructure", referencedAssemblyNames);
        Assert.DoesNotContain("api", referencedAssemblyNames);
    }

    [Fact]
    public void ApplicationLayer_ShouldNotDependOn_InfrastructureOrApi()
    {
        // طبقة التطبيق (Application) لا تعتمد على البنية التحتية أو الواجهة
        var referencedAssemblyNames = _applicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.DoesNotContain("infrastructure", referencedAssemblyNames);
        Assert.DoesNotContain("api", referencedAssemblyNames);
    }

    [Fact]
    public void ApplicationLayer_ShouldDependOn_CoreLayer()
    {
        // طبقة التطبيق (Application) تعتمد على طبقة النواة (Core)
        var referencedAssemblyNames = _applicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.Contains("core", referencedAssemblyNames);
    }

    [Fact]
    public void InfrastructureLayer_ShouldNotDependOn_Api()
    {
        // طبقة البنية التحتية (Infrastructure) لا تعتمد على طبقة الواجهة (API)
        var referencedAssemblyNames = _infrastructureAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.DoesNotContain("api", referencedAssemblyNames);
    }

    [Fact]
    public void InfrastructureLayer_ShouldDependOn_CoreAndApplication()
    {
        // طبقة البنية التحتية تعتمد على النواة والتطبيق
        var referencedAssemblyNames = _infrastructureAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.Contains("core", referencedAssemblyNames);
        Assert.Contains("application", referencedAssemblyNames);
    }

    [Fact]
    public void ApiLayer_ShouldDependOn_ApplicationAndInfrastructure()
    {
        // طبقة الواجهة تعتمد على التطبيق والبنية التحتية
        var referencedAssemblyNames = _apiAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.Contains("application", referencedAssemblyNames);
        Assert.Contains("infrastructure", referencedAssemblyNames);
    }
}
