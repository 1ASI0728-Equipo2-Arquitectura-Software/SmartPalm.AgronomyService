using System.Net;
using Microsoft.Extensions.Configuration;
using SmartPalm.AgronomyService.Domain.Services;
using SmartPalm.AgronomyService.Infrastructure.ExternalServices;
using Xunit;

namespace SmartPalm.AgronomyService.API.Tests.Domain;

public class MiniMaxDiagnosisProviderTests
{
    [Fact]
    public async Task SendsSummaryWithRequestedModelAndReturnsAnswer()
    {
        var handler = new StubHandler(HttpStatusCode.OK, "{\"base_resp\":{\"status_code\":0},\"choices\":[{\"finish_reason\":\"stop\",\"message\":{\"content\":\"Resumen: revisar en campo.\"}}]}");
        var provider = CreateProvider(handler);
        var result = await provider.GenerateAsync(Summary(), CancellationToken.None);
        Assert.Equal("Resumen: revisar en campo.", result);
        Assert.Equal("Bearer test-key", handler.Authorization);
        Assert.Contains("MiniMax-M3.1-Flash", handler.Body);
        Assert.Contains("soilMoisture", handler.Body);
        Assert.DoesNotContain("test-key", handler.Body);
    }

    [Fact]
    public async Task ProviderFailuresDoNotExposeResponseOrCredential()
    {
        var provider = CreateProvider(new StubHandler(HttpStatusCode.Unauthorized, "test-key sensitive details"));
        var error = await Assert.ThrowsAsync<DiagnosisProviderException>(() => provider.GenerateAsync(Summary(), CancellationToken.None));
        Assert.DoesNotContain("test-key", error.Message);
        Assert.DoesNotContain("sensitive details", error.Message);
    }

    private static MiniMaxDiagnosisProvider CreateProvider(StubHandler handler) => new(new HttpClient(handler), new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["MINIMAX_API_KEY"] = "test-key", ["MINIMAX_MODEL"] = "MiniMax-M3.1-Flash" }).Build());
    private static DiagnosisSummary Summary() { var from = DateTimeOffset.Parse("2026-06-18T00:00:00Z"); return DiagnosisSummary.Create(from, from.AddDays(1), [new("soilMoisture", 2, "%", from)]); }
    private sealed class StubHandler(HttpStatusCode status, string response) : HttpMessageHandler
    {
        public string? Body { get; private set; }
        public string? Authorization { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization?.ToString();
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status) { Content = new StringContent(response) };
        }
    }
}
