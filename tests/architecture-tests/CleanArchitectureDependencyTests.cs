using System.Reflection;
using System.Xml.Linq;
using PayrollReviewSystem.Application;
using PayrollReviewSystem.Core;
using PayrollReviewSystem.Infrastructure;
using Xunit;

namespace PayrollReviewSystem.ArchitectureTests;

/// <summary>
/// اختبارات معمارية تلقائية للتحقق الصارم من قواعد اتجاه التبعيات في Clean Architecture.
/// تجمع هذه الاختبارات بين:
/// 1. الفحص الثابت لملفات المشاريع (*.csproj) للتأكد من عدم إضافة ProjectReference أو PackageReference محظورة حتى لو لم تُستخدم أنواعها في الكود.
/// 2. حالات اختبار سلبية اصطناعية تثبت أن أداة الفحص تفشل فورًا عند إدخال أي تبعية ممنوعة.
/// 3. فحص بيانات التجميع المترجمة (Assembly.GetReferencedAssemblies) كمكمّل للتحقق وقت التشغيل.
/// </summary>
public class CleanArchitectureDependencyTests
{
    private static readonly string RepoRoot = FindRepositoryRoot();

    private static readonly string[] ForbiddenBoot001Packages =
    [
        "EntityFramework",
        "Microsoft.EntityFrameworkCore",
        "Dapper",
        "Microsoft.Data.SqlClient",
        "System.Data.SqlClient"
    ];

    private readonly Assembly _coreAssembly = typeof(CoreMarker).Assembly;
    private readonly Assembly _applicationAssembly = typeof(ApplicationMarker).Assembly;
    private readonly Assembly _infrastructureAssembly = typeof(InfrastructureMarker).Assembly;
    private readonly Assembly _apiAssembly = typeof(Program).Assembly;

    // =========================================================================
    // أولاً: فحص التجميعات المترجمة (Assembly Metadata Inspection)
    // =========================================================================

    [Fact]
    public void CoreLayer_ShouldNotDependOn_AnyOtherLayer()
    {
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
        var referencedAssemblyNames = _applicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.DoesNotContain("infrastructure", referencedAssemblyNames);
        Assert.DoesNotContain("api", referencedAssemblyNames);
    }

    [Fact]
    public void ApplicationLayer_ShouldDependOn_CoreLayer()
    {
        var referencedAssemblyNames = _applicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.Contains("core", referencedAssemblyNames);
    }

    [Fact]
    public void InfrastructureLayer_ShouldNotDependOn_Api()
    {
        var referencedAssemblyNames = _infrastructureAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.DoesNotContain("api", referencedAssemblyNames);
    }

    [Fact]
    public void InfrastructureLayer_ShouldDependOn_CoreAndApplication()
    {
        var referencedAssemblyNames = _infrastructureAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.Contains("core", referencedAssemblyNames);
        Assert.Contains("application", referencedAssemblyNames);
    }

    [Fact]
    public void ApiLayer_ShouldDependOn_ApplicationAndInfrastructure()
    {
        var referencedAssemblyNames = _apiAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet();

        Assert.Contains("application", referencedAssemblyNames);
        Assert.Contains("infrastructure", referencedAssemblyNames);
    }

    // =========================================================================
    // ثانياً: الفحص الثابت لملفات المشاريع (*.csproj Static Guard)
    // =========================================================================

    [Fact]
    public void CoreCsproj_ShouldHaveZeroProjectAndPackageReferences()
    {
        var csprojPath = Path.Combine(RepoRoot, "src", "core", "core.csproj");
        var doc = XDocument.Load(csprojPath);

        var violations = ValidateCsprojDependencies(
            projectName: "core",
            csprojXml: doc,
            allowedProjectRefs: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            requiredProjectRefs: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            allowExternalPackages: false,
            forbiddenPackageTokens: ForbiddenBoot001Packages);

        Assert.Empty(violations);
    }

    [Fact]
    public void ApplicationCsproj_ShouldOnlyReferenceCore_AndZeroExternalPackages()
    {
        var csprojPath = Path.Combine(RepoRoot, "src", "application", "application.csproj");
        var doc = XDocument.Load(csprojPath);

        var allowedAndRequired = new HashSet<string>(["core"], StringComparer.OrdinalIgnoreCase);

        var violations = ValidateCsprojDependencies(
            projectName: "application",
            csprojXml: doc,
            allowedProjectRefs: allowedAndRequired,
            requiredProjectRefs: allowedAndRequired,
            allowExternalPackages: false,
            forbiddenPackageTokens: ForbiddenBoot001Packages);

        Assert.Empty(violations);
    }

    [Fact]
    public void InfrastructureCsproj_ShouldOnlyReferenceCoreAndApplication_AndZeroDatabaseOrOrmPackages()
    {
        var csprojPath = Path.Combine(RepoRoot, "src", "infrastructure", "infrastructure.csproj");
        var doc = XDocument.Load(csprojPath);

        var allowedAndRequired = new HashSet<string>(["core", "application"], StringComparer.OrdinalIgnoreCase);

        var violations = ValidateCsprojDependencies(
            projectName: "infrastructure",
            csprojXml: doc,
            allowedProjectRefs: allowedAndRequired,
            requiredProjectRefs: allowedAndRequired,
            allowExternalPackages: false,
            forbiddenPackageTokens: ForbiddenBoot001Packages);

        Assert.Empty(violations);
    }

    [Fact]
    public void ApiCsproj_ShouldReferenceApplicationAndInfrastructure_AndZeroForbiddenDatabasePackages()
    {
        var csprojPath = Path.Combine(RepoRoot, "src", "api", "api.csproj");
        var doc = XDocument.Load(csprojPath);

        var allowed = new HashSet<string>(["core", "application", "infrastructure"], StringComparer.OrdinalIgnoreCase);
        var required = new HashSet<string>(["application", "infrastructure"], StringComparer.OrdinalIgnoreCase);

        var violations = ValidateCsprojDependencies(
            projectName: "api",
            csprojXml: doc,
            allowedProjectRefs: allowed,
            requiredProjectRefs: required,
            allowExternalPackages: true,
            forbiddenPackageTokens: ForbiddenBoot001Packages);

        Assert.Empty(violations);
    }

    [Fact]
    public void ClientIndexHtml_ShouldDeclareArabicLanguageAndRtlDirection()
    {
        var indexHtmlPath = Path.Combine(RepoRoot, "client", "src", "index.html");
        var htmlContent = File.ReadAllText(indexHtmlPath);

        Assert.Contains("lang=\"ar\"", htmlContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dir=\"rtl\"", htmlContent, StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================================
    // ثالثاً: حالات اختبار سلبية اصطناعية لإثبات فاعلية الحارس المعماري
    // =========================================================================

    [Theory]
    [InlineData(
        "core",
        """<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><ProjectReference Include="..\application\application.csproj" /></ItemGroup></Project>""",
        new string[0],
        false,
        "Forbidden ProjectReference in 'core': 'application'")]
    [InlineData(
        "core",
        """<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" /></ItemGroup></Project>""",
        new string[0],
        false,
        "Forbidden PackageReference in 'core': 'Microsoft.EntityFrameworkCore'")]
    [InlineData(
        "application",
        """<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><ProjectReference Include="..\core\core.csproj" /><ProjectReference Include="..\infrastructure\infrastructure.csproj" /></ItemGroup></Project>""",
        new[] { "core" },
        false,
        "Forbidden ProjectReference in 'application': 'infrastructure'")]
    [InlineData(
        "infrastructure",
        """<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><ProjectReference Include="..\core\core.csproj" /><ProjectReference Include="..\application\application.csproj" /><ProjectReference Include="..\api\api.csproj" /></ItemGroup></Project>""",
        new[] { "core", "application" },
        false,
        "Forbidden ProjectReference in 'infrastructure': 'api'")]
    [InlineData(
        "api",
        """<Project Sdk="Microsoft.NET.Sdk.Web"><ItemGroup><ProjectReference Include="..\application\application.csproj" /><ProjectReference Include="..\infrastructure\infrastructure.csproj" /><PackageReference Include="Dapper" Version="2.1.35" /></ItemGroup></Project>""",
        new[] { "application", "infrastructure" },
        true,
        "Forbidden ORM/DB PackageReference in 'api': 'Dapper'")]
    public void StaticCsprojGuard_ShouldDetect_ForbiddenProjectAndPackageReferences(
        string projectName,
        string syntheticCsprojXml,
        string[] allowedProjects,
        bool allowExternalPackages,
        string expectedViolationSubstring)
    {
        var doc = XDocument.Parse(syntheticCsprojXml);
        var allowedSet = new HashSet<string>(allowedProjects, StringComparer.OrdinalIgnoreCase);

        var violations = ValidateCsprojDependencies(
            projectName: projectName,
            csprojXml: doc,
            allowedProjectRefs: allowedSet,
            requiredProjectRefs: allowedSet,
            allowExternalPackages: allowExternalPackages,
            forbiddenPackageTokens: ForbiddenBoot001Packages);

        Assert.NotEmpty(violations);
        Assert.Contains(violations, v => v.Contains(expectedViolationSubstring, StringComparison.OrdinalIgnoreCase));
    }

    // =========================================================================
    // أدوات التحقق الثابت المساعدة
    // =========================================================================

    public static IReadOnlyList<string> ValidateCsprojDependencies(
        string projectName,
        XDocument csprojXml,
        IReadOnlySet<string> allowedProjectRefs,
        IReadOnlySet<string> requiredProjectRefs,
        bool allowExternalPackages,
        IReadOnlyList<string> forbiddenPackageTokens)
    {
        var violations = new List<string>();

        var projectRefs = csprojXml.Descendants("ProjectReference")
            .Select(el => el.Attribute("Include")?.Value)
            .Where(val => !string.IsNullOrWhiteSpace(val))
            .Select(val => Path.GetFileNameWithoutExtension(val!.Replace('\\', '/')))
            .ToList();

        foreach (var projRef in projectRefs)
        {
            if (!allowedProjectRefs.Contains(projRef))
            {
                violations.Add($"Forbidden ProjectReference in '{projectName}': '{projRef}'");
            }
        }

        foreach (var reqRef in requiredProjectRefs)
        {
            if (!projectRefs.Contains(reqRef, StringComparer.OrdinalIgnoreCase))
            {
                violations.Add($"Missing required ProjectReference in '{projectName}': '{reqRef}'");
            }
        }

        var packageRefs = csprojXml.Descendants("PackageReference")
            .Select(el => el.Attribute("Include")?.Value ?? el.Attribute("Update")?.Value)
            .Where(val => !string.IsNullOrWhiteSpace(val))
            .Select(val => val!)
            .ToList();

        foreach (var pkgRef in packageRefs)
        {
            if (!allowExternalPackages)
            {
                violations.Add($"Forbidden PackageReference in '{projectName}': '{pkgRef}'");
            }
            else if (forbiddenPackageTokens.Any(token => pkgRef.Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                violations.Add($"Forbidden ORM/DB PackageReference in '{projectName}': '{pkgRef}'");
            }
        }

        return violations;
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "PayrollReviewSystem.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing PayrollReviewSystem.sln.");
    }
}
