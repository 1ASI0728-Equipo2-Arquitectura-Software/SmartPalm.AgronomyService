using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Domain.Services;

namespace SmartPalm.AgronomyService.Infrastructure.ExternalServices;

public sealed class DiagnosisProviderException(string message) : Exception(message);

public sealed class MiniMaxDiagnosisProvider(HttpClient client, IConfiguration configuration) : IDiagnosisProvider
{
    private const string Endpoint = "https://api.minimax.io/v1/chat/completions";

    public async Task<string> GenerateAsync(DiagnosisSummary summary, CancellationToken cancellationToken)
    {
        var key = configuration["MINIMAX_API_KEY"];
        if (string.IsNullOrWhiteSpace(key)) throw new DiagnosisProviderException("The diagnosis provider is not configured.");
        var model = configuration["MINIMAX_MODEL"] ?? "MiniMax-M3.1-Flash";
        using var skillStream = typeof(MiniMaxDiagnosisProvider).Assembly.GetManifestResourceStream("SmartPalm.AgronomyService.Skills.oil-palm-diagnosis.SKILL.md")
            ?? throw new DiagnosisProviderException("The diagnosis instructions are unavailable.");
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
            if (!response.IsSuccessStatusCode) throw new DiagnosisProviderException("The diagnosis provider rejected the request.");
            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = json.RootElement;
            if (root.TryGetProperty("base_resp", out var status) && status.TryGetProperty("status_code", out var code) && code.GetInt32() != 0)
                throw new DiagnosisProviderException("The diagnosis provider could not complete the request.");
            if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
                throw new DiagnosisProviderException("The diagnosis provider returned an incomplete response.");
            var choice = choices[0];
            if (!choice.TryGetProperty("finish_reason", out var finish) || finish.GetString() != "stop" ||
                !choice.TryGetProperty("message", out var message) || !message.TryGetProperty("content", out var content) ||
                content.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(content.GetString()))
                throw new DiagnosisProviderException("The diagnosis provider returned an incomplete response.");
            return content.GetString()!;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw new DiagnosisProviderException("The diagnosis provider timed out."); }
        catch (HttpRequestException)
        { throw new DiagnosisProviderException("The diagnosis provider is unavailable."); }
        catch (JsonException)
        { throw new DiagnosisProviderException("The diagnosis provider returned an invalid response."); }
    }
}
