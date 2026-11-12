using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Domain.Services;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using RabbitMQ.Client;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SmartPalm.AgronomyService.API.Tests.Integration;

public class AgronomyApiContainerTests : IAsyncLifetime
{
    private const string JwtKey = "agronomy-tests-signing-key-with-at-least-32-characters";
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").WithDatabase("smartpalm_agronomy").WithUsername("smartpalm_agronomy").WithPassword("tests").Build();
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder("rabbitmq:4-management-alpine").Build();
    public async Task InitializeAsync() => await Task.WhenAll(_postgres.StartAsync(), _rabbit.StartAsync());
    public async Task DisposeAsync() => await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbit.DisposeAsync().AsTask());

    [DockerRequiredFact, Trait("Category", "Integration")]
    public async Task ProjectionEvents_EnableRecommendationApprovalAndPublication()
    {
        using var factory = CreateFactory(); using var client = factory.CreateClient();
        await using var connection = await new ConnectionFactory { Uri = new Uri(_rabbit.GetConnectionString()) }.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(); await channel.ExchangeDeclareAsync("smartpalm.events", ExchangeType.Topic, true, false);
        var userMessage = Guid.NewGuid();
        await PublishUntilAsync(channel, "smartpalm.identity.user-created.v1", userMessage, new { eventType = "smartpalm.identity.user-created.v1", payload = new { userId = 7, username = "agronomist", email = "a@test.local", fullName = "Test Agronomist", role = "Agronomist", occurredAtUtc = DateTime.UtcNow } }, "SELECT COUNT(*) FROM agronomy.user_projections", 1);
        await PublishUntilAsync(channel, "smartpalm.identity.user-created.v1", userMessage, new { userId = 7, username = "agronomist", email = "a@test.local", fullName = "Test Agronomist", role = "Agronomist", occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.inbox_messages", 1);
        await PublishUntilAsync(channel, "smartpalm.identity.subscription-activated.v1", Guid.NewGuid(), new { subscriptionId = 2, userId = 7, planType = "Premium", startDate = DateTime.UtcNow, endDate = DateTime.UtcNow.AddMonths(1), occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.user_projections WHERE has_active_subscription", 1);
        await PublishUntilAsync(channel, "crop.plantation-created.v1", Guid.NewGuid(), new { plantationId = 20L, palmGrowerId = 3L, name = "Palm Estate", hectares = 12m, occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.plantation_projections", 1);
        await PublishUntilAsync(channel, "crop.sector-assigned.v1", Guid.NewGuid(), new { sectorId = 30L, plantationId = 20L, deviceMacAddress = "DEVICE-1", name = "North", occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.sector_projections", 1);
        await PublishUntilAsync(channel, "crop.agronomist-affiliated.v1", Guid.NewGuid(), new { affiliationId = 40L, agronomistId = 7L, plantationId = 20L, occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.agronomist_affiliation_projections WHERE active", 1);

        Assert.Equal(5, await ScalarAsync("SELECT COUNT(*) FROM agronomy.inbox_messages"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(7, "Agronomist"));
        var create = await client.PostAsync("/api/v1/sectors/30/recommendations", Json(new { agronomistId = 7, content = "Increase irrigation frequency", reportId = (long?)null }));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode); var payload = JsonDocument.Parse(await create.Content.ReadAsStringAsync()); var id = payload.RootElement.GetProperty("id").GetInt64();
        Assert.Equal(HttpStatusCode.OK, (await client.PatchAsync($"/api/v1/recommendations/{id}/approval", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PatchAsync($"/api/v1/recommendations/{id}/publication", null)).StatusCode);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM agronomy.outbox_messages WHERE event_type = 'agronomy.recommendation-published.v1'"));
        var publishedEvent = JsonDocument.Parse(await TextAsync("SELECT content FROM agronomy.outbox_messages WHERE event_type = 'agronomy.recommendation-published.v1' LIMIT 1"));
        Assert.Equal(id, publishedEvent.RootElement.GetProperty("recommendationId").GetInt64());
        Assert.Equal("SectorSpecific", publishedEvent.RootElement.GetProperty("type").GetString());
        Assert.Equal(1, publishedEvent.RootElement.GetProperty("version").GetInt32());
    }

    [DockerRequiredFact, Trait("Category", "Integration")]
    public async Task Recommendations_WithoutJwt_ReturnUnauthorized()
    { using var factory = CreateFactory(); using var client = factory.CreateClient(); Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/recommendations/general")).StatusCode); }

    [DockerRequiredFact, Trait("Category", "Integration")]
    public async Task Diagnosis_RequiresAnAuthorizedAgronomistAndSummarizesReadings()
    {
        using var factory = CreateFactory().WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDiagnosisProvider>();
            services.AddSingleton<IDiagnosisProvider, FakeDiagnosisProvider>();
        }));
        using var client = factory.CreateClient();
        var from = DateTimeOffset.UtcNow.AddHours(-2);
        var payload = new { from, to = from.AddHours(1), readings = new[] { new { variable = "temperature", value = 27.0, unit = "C", measuredAt = from.AddMinutes(10) } } };
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync("/api/v1/sectors/30/recommendations/diagnosis", Json(payload))).StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(7, "PalmGrower"));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/v1/sectors/30/recommendations/diagnosis", Json(payload))).StatusCode);
        await using var connection = await new ConnectionFactory { Uri = new Uri(_rabbit.GetConnectionString()) }.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(); await channel.ExchangeDeclareAsync("smartpalm.events", ExchangeType.Topic, true, false);
        await PublishUntilAsync(channel, "smartpalm.identity.user-created.v1", Guid.NewGuid(), new { userId = 7, role = "Agronomist", occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.user_projections", 1);
        await PublishUntilAsync(channel, "smartpalm.identity.subscription-activated.v1", Guid.NewGuid(), new { userId = 7, occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.user_projections WHERE has_active_subscription", 1);
        await PublishUntilAsync(channel, "crop.plantation-created.v1", Guid.NewGuid(), new { plantationId = 20L, palmGrowerId = 3L, name = "Palm Estate", occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.plantation_projections", 1);
        await PublishUntilAsync(channel, "crop.sector-assigned.v1", Guid.NewGuid(), new { sectorId = 30L, plantationId = 20L, name = "North", occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.sector_projections", 1);
        await PublishUntilAsync(channel, "crop.agronomist-affiliated.v1", Guid.NewGuid(), new { affiliationId = 40L, agronomistId = 7L, plantationId = 20L, occurredAtUtc = DateTime.UtcNow }, "SELECT COUNT(*) FROM agronomy.agronomist_affiliation_projections WHERE active", 1);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(7, "Agronomist"));
        var response = await client.PostAsync("/api/v1/sectors/30/recommendations/diagnosis", Json(payload));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Test diagnosis", body.RootElement.GetProperty("diagnosis").GetString());
        Assert.Equal(27, body.RootElement.GetProperty("summary").GetProperty("metrics")[0].GetProperty("average").GetDouble());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/api/v1/sectors/30/recommendations/diagnosis", Json(new { from, to = from.AddHours(1), readings = Array.Empty<object>() }))).StatusCode);
    }

    private sealed class FakeDiagnosisProvider : IDiagnosisProvider
    {
        public Task<string> GenerateAsync(DiagnosisSummary summary, CancellationToken cancellationToken) => Task.FromResult("Test diagnosis");
    }

    private WebApplicationFactory<Program> CreateFactory() => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Testing"); builder.UseSetting("ConnectionStrings:AgronomyDatabase", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Issuer", "smartpalm.identity.tests"); builder.UseSetting("Jwt:Audience", "smartpalm.clients.tests"); builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("RabbitMq:Enabled", "true"); builder.UseSetting("RabbitMq:Uri", _rabbit.GetConnectionString()); builder.UseSetting("RabbitMq:Queue", $"smartpalm.agronomy.tests.{Guid.NewGuid():N}");
    });
    private async Task PublishUntilAsync(IChannel channel, string type, Guid id, object payload, string sql, long expected)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        for (var attempt = 0; attempt < 15; attempt++)
        {
            await channel.BasicPublishAsync("smartpalm.events", type, false, new BasicProperties { Persistent = true, MessageId = id.ToString(), Type = type, ContentType = "application/json" }, body);
            await Task.Delay(500); if (await ScalarAsync(sql) >= expected) return;
        }
        Assert.Fail($"Event {type} was not processed.");
    }
    private async Task<long> ScalarAsync(string sql)
    { await using var connection = new NpgsqlConnection(_postgres.GetConnectionString()); await connection.OpenAsync(); await using var command = new NpgsqlCommand(sql, connection); return Convert.ToInt64(await command.ExecuteScalarAsync()); }
    private async Task<string> TextAsync(string sql)
    { await using var connection = new NpgsqlConnection(_postgres.GetConnectionString()); await connection.OpenAsync(); await using var command = new NpgsqlCommand(sql, connection); return Convert.ToString(await command.ExecuteScalarAsync()) ?? throw new InvalidOperationException("Expected text result."); }
    private static StringContent Json(object value) => new(JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json");
    private static string CreateToken(long userId, string role)
    {
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken("smartpalm.identity.tests", "smartpalm.clients.tests", [new Claim("sub", userId.ToString()), new Claim("name", "integration-user"), new Claim("role", role)], expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
