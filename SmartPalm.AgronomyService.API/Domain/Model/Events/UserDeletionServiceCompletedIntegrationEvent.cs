namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record UserDeletionServiceCompletedIntegrationEvent(Guid OperationId, int UserId, string ServiceName, int DeletedRecords, DateTime OccurredAtUtc);
