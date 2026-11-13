using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Domain.Services;

namespace SmartPalm.AgronomyService.Infrastructure.ExternalServices;

public enum DiagnosisProviderFailureCategory
{
    ConfigurationMissing,
    InstructionsUnavailable,
    HttpFailure,
    ProviderFailure,
    IncompleteResponse,
    Timeout,
    TransportFailure,
    InvalidResponse
}

public sealed class DiagnosisProviderException(
    DiagnosisProviderFailureCategory category,
    string message) : Exception(message)
{
    public DiagnosisProviderFailureCategory Category { get; } = category;
}

public sealed class MiniMaxDiagnosisProvider(
    HttpClient client,
    IConfiguration configuration,
    ILogger<MiniMaxDiagnosisProvider> logger) : IDiagnosisProvider
{
    private const string Endpoint = "https://api.minimax.io/v1/chat/completions";

    public async Task<string> GenerateAsync(DiagnosisSummary summary, CancellationToken cancellationToken)
    {
        var key = configuration["MINIMAX_API_KEY"];
        var model = configuration["MINIMAX_MODEL"] ?? "MiniMax-M3.1-Flash";
        if (string.IsNullOrWhiteSpace(key))
        {
            logger.LogError("Diagnosis provider configuration is missing. Category={Category} Model={Model}",
                DiagnosisProviderFailureCategory.ConfigurationMissing, model);
            throw new DiagnosisProviderException(
                DiagnosisProviderFailureCategory.ConfigurationMissing,
                "The diagnosis provider is not configured.");
        }
        using var skillStream = typeof(MiniMaxDiagnosisProvider).Assembly.GetManifestResourceStream("SmartPalm.AgronomyService.Skills.oil-palm-diagnosis.SKILL.md")
            ?? throw CreateInstructionsUnavailable(model);
        using var reader = new StreamReader(skillStream);
        var instructions = await reader.ReadToEndAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        request.Content = JsonContent.Create(new
        {
            model,
            reasoning_effort = "low",
            reasoning_split = true,
            max_completion_tokens = 2048,
            messages = new[]
            {
                new { role = "system", content = instructions },
                new { role = "user", content = JsonSerializer.Serialize(summary, new JsonSerializerOptions(JsonSerializerDefaults.Web)) }
            }
        });
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Diagnosis provider returned HTTP {StatusCode}. Category={Category} Model={Model}",
                    (int)response.StatusCode, DiagnosisProviderFailureCategory.HttpFailure, model);
                throw new DiagnosisProviderException(
                    DiagnosisProviderFailureCategory.HttpFailure,
                    "The diagnosis provider rejected the request.");
            }
            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = json.RootElement;
            if (root.TryGetProperty("base_resp", out var status) && status.TryGetProperty("status_code", out var code) && code.GetInt32() != 0)
            {
                logger.LogWarning("Diagnosis provider returned an application error. Category={Category} ProviderStatusCode={ProviderStatusCode} Model={Model}",
                    DiagnosisProviderFailureCategory.ProviderFailure, code.GetInt32(), model);
                throw new DiagnosisProviderException(
                    DiagnosisProviderFailureCategory.ProviderFailure,
                    "The diagnosis provider could not complete the request.");
            }
            if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
            {
                logger.LogWarning("Diagnosis provider response has no choices. Category={Category} Model={Model}",
                    DiagnosisProviderFailureCategory.IncompleteResponse, model);
                throw new DiagnosisProviderException(
                    DiagnosisProviderFailureCategory.IncompleteResponse,
                    "The diagnosis provider returned an incomplete response.");
            }
            var choice = choices[0];
            if (!choice.TryGetProperty("finish_reason", out var finish) || finish.GetString() != "stop" ||
                !choice.TryGetProperty("message", out var message) || !message.TryGetProperty("content", out var content) ||
                content.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(content.GetString()))
            {
                logger.LogWarning("Diagnosis provider response is missing completed content. Category={Category} Model={Model}",
                    DiagnosisProviderFailureCategory.IncompleteResponse, model);
                throw new DiagnosisProviderException(
                    DiagnosisProviderFailureCategory.IncompleteResponse,
                    "The diagnosis provider returned an incomplete response.");
            }
            return content.GetString()!;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Diagnosis provider request timed out. Category={Category} Model={Model}",
                DiagnosisProviderFailureCategory.Timeout, model);
            throw new DiagnosisProviderException(
                DiagnosisProviderFailureCategory.Timeout,
                "The diagnosis provider timed out.");
        }
        catch (HttpRequestException)
        {
            logger.LogWarning("Diagnosis provider transport failed. Category={Category} Model={Model}",
                DiagnosisProviderFailureCategory.TransportFailure, model);
            throw new DiagnosisProviderException(
                DiagnosisProviderFailureCategory.TransportFailure,
                "The diagnosis provider is unavailable.");
        }
        catch (JsonException)
        {
            logger.LogWarning("Diagnosis provider returned invalid JSON. Category={Category} Model={Model}",
                DiagnosisProviderFailureCategory.InvalidResponse, model);
            throw new DiagnosisProviderException(
                DiagnosisProviderFailureCategory.InvalidResponse,
                "The diagnosis provider returned an invalid response.");
        }
    }

    private DiagnosisProviderException CreateInstructionsUnavailable(string model)
    {
        logger.LogError("Diagnosis provider instructions are unavailable. Category={Category} Model={Model}",
            DiagnosisProviderFailureCategory.InstructionsUnavailable, model);
        return new DiagnosisProviderException(
            DiagnosisProviderFailureCategory.InstructionsUnavailable,
            "The diagnosis instructions are unavailable.");
    }
}
