using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

    [Fact]
    public async Task HttpFailuresLogStatusAndCategoryWithoutProviderBody()
    {
        var logger = new CaptureLogger<MiniMaxDiagnosisProvider>();
        var provider = CreateProvider(new StubHandler(HttpStatusCode.Unauthorized, "sensitive provider body"), logger);

        await Assert.ThrowsAsync<DiagnosisProviderException>(() => provider.GenerateAsync(Summary(), CancellationToken.None));

        var entry = Assert.Single(logger.Messages);
        Assert.Contains("401", entry);
        Assert.Contains(nameof(DiagnosisProviderFailureCategory.HttpFailure), entry);
        Assert.Contains("MiniMax-M3.1-Flash", entry);
        Assert.DoesNotContain("sensitive provider body", entry);
    }

    private static MiniMaxDiagnosisProvider CreateProvider(
        StubHandler handler,
        ILogger<MiniMaxDiagnosisProvider>? logger = null) =>
        new(new HttpClient(handler),
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MINIMAX_API_KEY"] = "test-key",
                ["MINIMAX_MODEL"] = "MiniMax-M3.1-Flash"
            }).Build(),
            logger ?? new CaptureLogger<MiniMaxDiagnosisProvider>());

    private static DiagnosisSummary Summary() { var from = DateTimeOffset.Parse("2026-06-18T00:00:00Z"); return DiagnosisSummary.Create(from, from.AddDays(1), [new("soilMoisture", 2, "%", from)]); }
    private sealed class CaptureLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();
            public void Dispose() { }
        }
    }

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
