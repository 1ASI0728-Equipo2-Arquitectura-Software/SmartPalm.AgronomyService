using System.Text.Json;
using System.Text.Json.Serialization;
using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Infrastructure.Persistence;

namespace SmartPalm.AgronomyService.Infrastructure.Messaging;
internal class IntegrationEventWriter(AgronomyDbContext db) : IIntegrationEventWriter
{
    private static readonly JsonSerializerOptions JsonOptions = CreateOptions();
    public void Enqueue(string type, object payload) => db.OutboxMessages.Add(new OutboxMessage
    { Id = Guid.NewGuid(), EventType = type, Content = JsonSerializer.Serialize(payload, payload.GetType(), JsonOptions), OccurredOnUtc = DateTime.UtcNow });
    private static JsonSerializerOptions CreateOptions() { var options = new JsonSerializerOptions(JsonSerializerDefaults.Web); options.Converters.Add(new JsonStringEnumConverter()); return options; }
}
