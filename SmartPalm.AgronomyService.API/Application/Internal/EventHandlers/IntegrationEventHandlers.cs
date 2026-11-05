using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Domain.Model.Events;
using SmartPalm.AgronomyService.Domain.Repositories;

namespace SmartPalm.AgronomyService.Application.EventHandlers;

public class IntegrationEventHandlers(IAgronomyProjectionRepository projections, IInbox inbox, IUnitOfWork unitOfWork)
{
    public Task HandleAsync(Guid id, UserCreatedIntegrationEvent e, CancellationToken token) => OnceAsync(id, "smartpalm.identity.user-created.v1", async ct =>
    { var user = await projections.GetOrCreateUserAsync(e.UserId, ct); user.Role = e.Role; user.UpdatedAtUtc = e.OccurredAtUtc; }, token);
    public Task HandleAsync(Guid id, SubscriptionCreatedIntegrationEvent e, CancellationToken token) => OnceAsync(id, "smartpalm.identity.subscription-created.v1", async ct =>
    { var user = await projections.GetOrCreateUserAsync(e.UserId, ct); user.HasActiveSubscription = string.Equals(e.Status, "Active", StringComparison.OrdinalIgnoreCase); user.UpdatedAtUtc = e.OccurredAtUtc; }, token);
    public Task HandleAsync(Guid id, SubscriptionActivatedIntegrationEvent e, CancellationToken token) => OnceAsync(id, "smartpalm.identity.subscription-activated.v1", async ct =>
    { var user = await projections.GetOrCreateUserAsync(e.UserId, ct); user.HasActiveSubscription = true; user.UpdatedAtUtc = e.OccurredAtUtc; }, token);
    public Task HandleAsync(Guid id, SubscriptionCancelledIntegrationEvent e, CancellationToken token) => OnceAsync(id, "smartpalm.identity.subscription-cancelled.v1", async ct =>
    { var user = await projections.GetOrCreateUserAsync(e.UserId, ct); user.HasActiveSubscription = false; user.UpdatedAtUtc = e.OccurredAtUtc; }, token);
    public Task HandleAsync(Guid id, PlantationCreatedIntegrationEvent e, CancellationToken token) => OnceAsync(id, "crop.plantation-created.v1", async ct =>
    { var p = await projections.GetOrCreatePlantationAsync(e.PlantationId, ct); p.PalmGrowerId = e.PalmGrowerId; p.Name = e.Name; p.Active = true; p.UpdatedAtUtc = e.OccurredAtUtc; }, token);
    public Task HandleAsync(Guid id, SectorAssignedIntegrationEvent e, CancellationToken token) => OnceAsync(id, "crop.sector-assigned.v1", async ct =>
    { var s = await projections.GetOrCreateSectorAsync(e.SectorId, ct); s.PlantationId = e.PlantationId; s.Name = e.Name; s.Active = true; s.UpdatedAtUtc = e.OccurredAtUtc; }, token);
    public Task HandleAsync(Guid id, SectorRemovedIntegrationEvent e, CancellationToken token) => OnceAsync(id, "crop.sector-removed.v1", async ct =>
    { var s = await projections.GetOrCreateSectorAsync(e.SectorId, ct); s.PlantationId = e.PlantationId; s.Active = false; s.UpdatedAtUtc = e.OccurredAtUtc; }, token);
    public Task HandleAsync(Guid id, AgronomistAffiliatedIntegrationEvent e, CancellationToken token) => OnceAsync(id, "crop.agronomist-affiliated.v1", async ct =>
    { var a = await projections.GetOrCreateAffiliationAsync(e.AffiliationId, e.AgronomistId, e.PlantationId, ct); a.Active = true; a.UpdatedAtUtc = e.OccurredAtUtc; }, token);
    public Task HandleAsync(Guid id, AgronomistDetachedIntegrationEvent e, CancellationToken token) => OnceAsync(id, "crop.agronomist-detached.v1", async ct =>
    { foreach (var a in await projections.ListAffiliationsAsync(e.AgronomistId, e.PlantationId, ct)) { a.Active = false; a.UpdatedAtUtc = e.OccurredAtUtc; } }, token);

    private Task HandleProjectionAsync(Func<CancellationToken, Task> handler, CancellationToken token) => handler(token);
    private Task OnceAsync(Guid id, string type, Func<CancellationToken, Task> handler, CancellationToken token) => unitOfWork.ExecuteAsync(async ct =>
    {
        if (await inbox.HasProcessedAsync(id, ct)) return false;
        await HandleProjectionAsync(handler, ct); inbox.MarkProcessed(id, type); await unitOfWork.SaveChangesAsync(ct); return true;
    }, token);
}
