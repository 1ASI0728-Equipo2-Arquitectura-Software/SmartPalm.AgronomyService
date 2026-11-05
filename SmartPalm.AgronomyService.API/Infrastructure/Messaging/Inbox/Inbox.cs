using Microsoft.EntityFrameworkCore;
using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Infrastructure.Persistence;

namespace SmartPalm.AgronomyService.Infrastructure.Messaging;
internal class Inbox(AgronomyDbContext db) : IInbox
{
    public Task<bool> HasProcessedAsync(Guid id, CancellationToken token) => db.InboxMessages.AnyAsync(x => x.Id == id, token);
    public void MarkProcessed(Guid id, string type) => db.InboxMessages.Add(new InboxMessage { Id = id, EventType = type, ProcessedOnUtc = DateTime.UtcNow });
}
