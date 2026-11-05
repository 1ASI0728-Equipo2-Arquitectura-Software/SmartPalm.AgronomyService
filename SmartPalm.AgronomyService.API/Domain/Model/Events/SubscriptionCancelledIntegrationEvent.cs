namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record SubscriptionCancelledIntegrationEvent(int SubscriptionId, int UserId, DateTime OccurredAtUtc);
