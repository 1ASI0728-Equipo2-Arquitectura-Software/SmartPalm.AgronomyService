namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record SubscriptionActivatedIntegrationEvent(int SubscriptionId, int UserId, string PlanType, DateTime StartDate, DateTime EndDate, DateTime OccurredAtUtc);
