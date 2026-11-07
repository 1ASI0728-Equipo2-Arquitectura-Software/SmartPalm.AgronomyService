namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record UserDeletionRequestedIntegrationEvent(Guid OperationId, int UserId, DateTime OccurredAtUtc);
