using System.Reflection;
using PayrollReviewSystem.Application;
using PayrollReviewSystem.Application.ReviewVariables.Ports;
using PayrollReviewSystem.Core;
using PayrollReviewSystem.Core.ReviewVariables;
using Xunit;

namespace PayrollReviewSystem.ArchitectureTests;

/// <summary>
/// اختبارات معمارية وحدود تصميمية خاصة بشريحة CODE-001 (<c>CODE001-ARCH-01</c> و<c>CODE001-NEG-07</c>).
/// تتحقق هذه الاختبارات برمجيًا عبر Reflection من:
/// 1. نقاء طبقتي Core وApplication وخلوهما من ORM أو SQL أو HTTP أو Infrastructure/API.
/// 2. الالتزام بمبدأ التغليف (Encapsulation) في الكيانات التجميعية ومنع <c>public setters</c>.
/// 3. عدم تسرب أي قدرات خارج نطاق CODE-001 (مثل VerifiedPaidAmount أو SettlementOrder أو بوابة موافقة المراجع الأساسي).
/// </summary>
public class Code001ArchitectureTests
{
    private readonly Assembly _coreAssembly = typeof(CoreMarker).Assembly;
    private readonly Assembly _applicationAssembly = typeof(ApplicationMarker).Assembly;

    [Fact]
    [Trait("CaseId", "CODE001-ARCH-01")]
    public void CoreAndApplication_ShouldHaveZeroDependencies_OnDatabaseOrmWebOrOuterLayers()
    {
        var forbiddenTokens = new[]
        {
            "EntityFramework",
            "Dapper",
            "SqlClient",
            "System.Data",
            "AspNetCore",
            "Microsoft.Extensions.DependencyInjection"
        };

        var coreRefs = _coreAssembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();
        var appRefs = _applicationAssembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();

        Assert.DoesNotContain("application", coreRefs, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("infrastructure", coreRefs, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("api", coreRefs, StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("infrastructure", appRefs, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("api", appRefs, StringComparer.OrdinalIgnoreCase);

        foreach (var token in forbiddenTokens)
        {
            Assert.DoesNotContain(coreRefs, r => r.Contains(token, StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(appRefs, r => r.Contains(token, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    [Trait("CaseId", "CODE001-ARCH-01")]
    public void ReviewVariableAggregate_ShouldEnforceEncapsulation_WithoutPublicSetters()
    {
        var aggregateType = typeof(ReviewVariable);
        var properties = aggregateType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var setMethod = property.GetSetMethod(nonPublic: false);
            Assert.Null(setMethod);
        }

        var constructors = aggregateType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        Assert.Empty(constructors);
    }

    [Fact]
    [Trait("CaseId", "CODE001-ARCH-01")]
    public void ApplicationPorts_ShouldOnlyExposeInterfacesNeededForCode001_WithoutGenericRepository()
    {
        var portTypes = _applicationAssembly.GetTypes()
            .Where(t => t.IsInterface && t.Namespace == typeof(IReviewVariableRepository).Namespace)
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToArray();

        Assert.Equal(
            [
                nameof(IAuditTrailPort),
                nameof(IAuthorizationPolicyPort),
                nameof(IClockPort),
                nameof(IIdempotencyPort),
                nameof(IReviewAtomicTransactionPort),
                nameof(IReviewerContextPort),
                nameof(IReviewVariableRepository)
            ],
            portTypes);

        // التأكد من عدم وجود Generic Repository عام لكل جدول (Anti-pattern في CODE-001)
        Assert.DoesNotContain(_applicationAssembly.GetTypes(), t => t.IsInterface && t.IsGenericTypeDefinition);
    }

    [Fact]
    [Trait("CaseId", "CODE001-NEG-07")]
    public void Code001Types_ShouldNotExposeOutOfScopeFinancialOrPrimaryApprovalCapabilities()
    {
        var allTypes = _coreAssembly.GetTypes().Concat(_applicationAssembly.GetTypes()).ToList();

        var forbiddenSymbolTokens = new[]
        {
            "VerifiedPaidAmount",
            "OutstandingAmount",
            "SettlementOrder",
            "PrimaryReviewerApproval",
            "MandatoryPrimaryApproval",
            "ExecuteOracle",
            "BankTransfer",
            "BankPayment",
            "IsOfficialOracle"
        };

        foreach (var type in allTypes)
        {
            foreach (var token in forbiddenSymbolTokens)
            {
                Assert.DoesNotContain(token, type.Name, StringComparison.OrdinalIgnoreCase);

                var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var member in members)
                {
                    Assert.DoesNotContain(token, member.Name, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}

