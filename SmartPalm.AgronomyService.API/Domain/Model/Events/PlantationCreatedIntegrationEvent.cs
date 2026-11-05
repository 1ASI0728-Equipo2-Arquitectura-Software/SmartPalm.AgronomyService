namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record PlantationCreatedIntegrationEvent(long PlantationId, long PalmGrowerId, string Name, decimal Hectares, DateTime OccurredAtUtc);
