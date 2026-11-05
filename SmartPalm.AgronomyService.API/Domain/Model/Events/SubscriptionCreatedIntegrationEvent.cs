namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record SubscriptionCreatedIntegrationEvent(int SubscriptionId, int UserId, string PlanType, string Status, DateTime OccurredAtUtc);
