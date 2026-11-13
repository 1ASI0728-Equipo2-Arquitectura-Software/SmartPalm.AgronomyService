using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartPalm.AgronomyService.Application.EventHandlers;
using SmartPalm.AgronomyService.Domain.Model.Events;
using SmartPalm.AgronomyService.Infrastructure.Configuration;

namespace SmartPalm.AgronomyService.Infrastructure.Messaging;
internal class IntegrationEventConsumer(IServiceScopeFactory scopes, IOptions<RabbitMqOptions> options, ILogger<IntegrationEventConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;
    private static readonly string[] Keys =
    [
        "smartpalm.identity.user-created.v1", "smartpalm.identity.subscription-created.v1", "smartpalm.identity.subscription-activated.v1",
        "smartpalm.identity.subscription-cancelled.v1", "crop.plantation-created.v1", "crop.sector-assigned.v1", "crop.sector-removed.v1",
        "crop.agronomist-affiliated.v1", "crop.agronomist-detached.v1",
        "smartpalm.identity.user-deletion-requested.v1"
    ];
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        if (!_options.Enabled) return;
        while (!token.IsCancellationRequested)
        {
            try { await ConsumeAsync(token); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex) { logger.LogWarning(ex, "Integration consumer disconnected; retrying."); await Task.Delay(TimeSpan.FromSeconds(3), token); }
        }
    }
    private async Task ConsumeAsync(CancellationToken token)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_options.Uri), AutomaticRecoveryEnabled = true };
        await using var connection = await factory.CreateConnectionAsync(token);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: token);
        await channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic, true, false, cancellationToken: token);
        await channel.QueueDeclareAsync(_options.Queue, true, false, false, cancellationToken: token);
        foreach (var key in Keys) await channel.QueueBindAsync(_options.Queue, _options.Exchange, key, cancellationToken: token);
        await channel.BasicQosAsync(0, 10, false, token);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var type = args.BasicProperties.Type ?? args.RoutingKey;
                var id = Guid.TryParse(args.BasicProperties.MessageId, out var parsed) ? parsed : Guid.NewGuid();
                var body = args.Body.ToArray(); using var scope = scopes.CreateScope();
                var handlers = scope.ServiceProvider.GetRequiredService<IntegrationEventHandlers>();
                switch (type)
                {
                    case "smartpalm.identity.user-created.v1": await handlers.HandleAsync(id, Read<UserCreatedIntegrationEvent>(body), token); break;
                    case "smartpalm.identity.subscription-created.v1": await handlers.HandleAsync(id, Read<SubscriptionCreatedIntegrationEvent>(body), token); break;
                    case "smartpalm.identity.subscription-activated.v1": await handlers.HandleAsync(id, Read<SubscriptionActivatedIntegrationEvent>(body), token); break;
                    case "smartpalm.identity.subscription-cancelled.v1": await handlers.HandleAsync(id, Read<SubscriptionCancelledIntegrationEvent>(body), token); break;
                    case "crop.plantation-created.v1": await handlers.HandleAsync(id, Read<PlantationCreatedIntegrationEvent>(body), token); break;
                    case "crop.sector-assigned.v1": await handlers.HandleAsync(id, Read<SectorAssignedIntegrationEvent>(body), token); break;
                    case "crop.sector-removed.v1": await handlers.HandleAsync(id, Read<SectorRemovedIntegrationEvent>(body), token); break;
                    case "crop.agronomist-affiliated.v1": await handlers.HandleAsync(id, Read<AgronomistAffiliatedIntegrationEvent>(body), token); break;
                    case "crop.agronomist-detached.v1": await handlers.HandleAsync(id, Read<AgronomistDetachedIntegrationEvent>(body), token); break;
                    case "smartpalm.identity.user-deletion-requested.v1": await handlers.HandleAsync(
                        ReadPayload<UserDeletionRequestedIntegrationEvent>(body), token); break;
                }
                await channel.BasicAckAsync(args.DeliveryTag, false, token);
            }
            catch (Exception ex) { logger.LogError(ex, "Integration event {RoutingKey} failed.", args.RoutingKey); await channel.BasicNackAsync(args.DeliveryTag, false, true, token); }
        };
        await channel.BasicConsumeAsync(_options.Queue, false, consumer, token); await Task.Delay(Timeout.InfiniteTimeSpan, token);
    }
    private static T Read<T>(byte[] body) => ReadPayload<T>(body);

    private static T ReadPayload<T>(byte[] body)
    {
        using var document = JsonDocument.Parse(body);
        var payload = document.RootElement.TryGetProperty("payload", out var value)
            ? value
            : document.RootElement;
        return payload.Deserialize<T>(new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new JsonException($"Invalid {typeof(T).Name} payload.");
    }
}
