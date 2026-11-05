using SmartPalm.AgronomyService.Domain.Repositories;
using SmartPalm.AgronomyService.Domain.Services;

namespace SmartPalm.AgronomyService.Application.DomainServices;

public class AgronomyAuthorizationService(IAgronomyProjectionRepository projections, AgronomyAccessPolicy policy)
{
    public async Task EnsureAuthorAsync(long agronomistId, long? sectorId, CancellationToken token)
    {
        policy.EnsureActiveAgronomist(await projections.FindUserAsync(agronomistId, token));
        if (sectorId is null) return;
        var sector = await projections.FindSectorAsync(sectorId.Value, token);
        var affiliated = sector is not null && await projections.HasActiveAffiliationAsync(agronomistId, sector.PlantationId, token);
        policy.EnsureSectorAccess(sector, affiliated);
    }
}
