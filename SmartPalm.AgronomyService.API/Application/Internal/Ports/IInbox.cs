namespace SmartPalm.AgronomyService.Application.Ports;

public interface IInbox
{
    Task<bool> HasProcessedAsync(Guid messageId, CancellationToken token);
    void MarkProcessed(Guid messageId, string eventType);
}
