using SmartPalm.AgronomyService.Domain.Model.Entities;

namespace SmartPalm.AgronomyService.Domain.Services;

public class AgronomyAccessPolicy
{
    public void EnsureActiveAgronomist(UserProjection? user)
    {
        if (user is null) throw new ArgumentException("Agronomist does not exist.");
        if (!string.Equals(user.Role, "Agronomist", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(user.Role, "Administrator", StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Only agronomists or administrators can author agronomic content.");
        if (!user.HasActiveSubscription) throw new InvalidOperationException("User does not have an active subscription.");
    }
    public void EnsureSectorAccess(SectorProjection? sector, bool hasAffiliation)
    {
        if (sector is null || !sector.Active) throw new ArgumentException("Sector does not exist.");
        if (!hasAffiliation) throw new UnauthorizedAccessException("Agronomist is not affiliated with the sector plantation.");
    }
}
