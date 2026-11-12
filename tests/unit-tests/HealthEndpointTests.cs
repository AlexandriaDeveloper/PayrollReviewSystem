using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PayrollReviewSystem.UnitTests;

/// <summary>
/// اختبارات تشغيلية لنقاط فحص الصحة والمعلومات الفنية في API.
/// تتأكد الاختبارات من استجابة الخادم بنجاح دون الحاجة لقاعدة بيانات أو أسرار أو اتصال خارجي،
/// وعدم كشف أسماء الأنواع الداخلية (CLR Types) علنًا.
/// </summary>
public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk_WithHealthyStatus()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(result);
        Assert.Equal("Healthy", result.Status);
        Assert.Equal("PayrollReviewSystem.Api", result.Service);
        Assert.Equal("BOOT-001", result.Scaffold);
    }

    [Fact]
    public async Task GetApiInfo_ReturnsOk_WithTechnicalScaffoldDetails()
    {
        var response = await _client.GetAsync("/api/info");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiInfoResponse>();
        Assert.NotNull(result);
        Assert.Equal("PayrollReviewSystem", result.System);
        Assert.Equal("SQL Server 2014 (12.x)", result.TargetDatabase);
        Assert.Equal("NOT_DESIGNED / NO_TABLES", result.DatabaseStatus);
    }

    [Fact]
    public async Task GetApiInfo_DoesNotExpose_InternalClrTypeNames()
    {
        var response = await _client.GetAsync("/api/info");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = jsonDoc.RootElement;

        Assert.False(root.TryGetProperty("applicationType", out _));
        Assert.False(root.TryGetProperty("infrastructureType", out _));
    }

    private sealed record HealthResponse(string Status, string Service, string Version, string Scaffold, DateTimeOffset Timestamp);
    private sealed record ApiInfoResponse(string System, string Architecture, string CoreLayer, string ApplicationLayer, string InfrastructureLayer, string TargetDatabase, string DatabaseStatus, string Stage);
}
