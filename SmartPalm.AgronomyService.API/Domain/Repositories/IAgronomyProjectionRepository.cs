using SmartPalm.AgronomyService.Domain.Model.Entities;

namespace SmartPalm.AgronomyService.Domain.Repositories;

public interface IAgronomyProjectionRepository
{
    Task<UserProjection?> FindUserAsync(long userId, CancellationToken token);
    Task<SectorProjection?> FindSectorAsync(long sectorId, CancellationToken token);
    Task<bool> HasActiveAffiliationAsync(long agronomistId, long plantationId, CancellationToken token);
    Task<UserProjection> GetOrCreateUserAsync(long userId, CancellationToken token);
    Task<PlantationProjection> GetOrCreatePlantationAsync(long plantationId, CancellationToken token);
    Task<SectorProjection> GetOrCreateSectorAsync(long sectorId, CancellationToken token);
    Task<AgronomistAffiliationProjection> GetOrCreateAffiliationAsync(long affiliationId, long agronomistId, long plantationId, CancellationToken token);
    Task<IReadOnlyList<AgronomistAffiliationProjection>> ListAffiliationsAsync(long agronomistId, long plantationId, CancellationToken token);
}
