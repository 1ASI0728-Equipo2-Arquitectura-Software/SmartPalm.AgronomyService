namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record SectorRemovedIntegrationEvent(long SectorId, long PlantationId, string DeviceMacAddress, DateTime OccurredAtUtc);
