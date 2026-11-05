using Microsoft.EntityFrameworkCore;
using SmartPalm.AgronomyService.Domain.Model.Entities;
using SmartPalm.AgronomyService.Domain.Repositories;

namespace SmartPalm.AgronomyService.Infrastructure.Persistence;

internal class AgronomyProjectionRepository(AgronomyDbContext db) : IAgronomyProjectionRepository
{
    public Task<UserProjection?> FindUserAsync(long id, CancellationToken t) => db.Users.SingleOrDefaultAsync(x => x.UserId == id, t);
    public Task<SectorProjection?> FindSectorAsync(long id, CancellationToken t) => db.Sectors.SingleOrDefaultAsync(x => x.SectorId == id, t);
    public Task<bool> HasActiveAffiliationAsync(long user, long plantation, CancellationToken t) => db.Affiliations.AnyAsync(x => x.AgronomistId == user && x.PlantationId == plantation && x.Active, t);
    public async Task<UserProjection> GetOrCreateUserAsync(long id, CancellationToken t) { var x = await db.Users.FindAsync([id], t); if (x is not null) return x; x = new UserProjection { UserId = id }; db.Users.Add(x); return x; }
    public async Task<PlantationProjection> GetOrCreatePlantationAsync(long id, CancellationToken t) { var x = await db.Plantations.FindAsync([id], t); if (x is not null) return x; x = new PlantationProjection { PlantationId = id }; db.Plantations.Add(x); return x; }
    public async Task<SectorProjection> GetOrCreateSectorAsync(long id, CancellationToken t) { var x = await db.Sectors.FindAsync([id], t); if (x is not null) return x; x = new SectorProjection { SectorId = id }; db.Sectors.Add(x); return x; }
    public async Task<AgronomistAffiliationProjection> GetOrCreateAffiliationAsync(long id, long user, long plantation, CancellationToken t) { var x = await db.Affiliations.FindAsync([id], t); if (x is not null) return x; x = new AgronomistAffiliationProjection { AffiliationId = id, AgronomistId = user, PlantationId = plantation }; db.Affiliations.Add(x); return x; }
    public async Task<IReadOnlyList<AgronomistAffiliationProjection>> ListAffiliationsAsync(long user, long plantation, CancellationToken t) => await db.Affiliations.Where(x => x.AgronomistId == user && x.PlantationId == plantation).ToArrayAsync(t);
}
