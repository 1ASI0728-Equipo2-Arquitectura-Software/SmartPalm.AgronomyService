using Microsoft.EntityFrameworkCore;
using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Application.Reconciliation;
using SmartPalm.AgronomyService.Infrastructure.Persistence;

namespace SmartPalm.AgronomyService.Application.Services;

public sealed class AgronomyProjectionReconciliationService(
    IIdentitySnapshotClient identity,
    ICropSnapshotClient crop,
    AgronomyDbContext db,
    ILogger<AgronomyProjectionReconciliationService> logger)
{
    public async Task<ProjectionReconciliationResult> ReconcileAsync(CancellationToken token)
    {
        var users = await identity.ListUsersAsync(token);
        var cropSnapshot = await crop.GetSnapshotAsync(token);
        var plantations = cropSnapshot.Plantations;
        var sectors = cropSnapshot.Sectors;
        var affiliations = cropSnapshot.ActiveAgronomistAffiliations;

        var usersCreated = 0; var usersUpdated = 0; var usersRemoved = 0;
        var plantationsCreated = 0; var plantationsUpdated = 0; var plantationsRemoved = 0;
        var sectorsCreated = 0; var sectorsUpdated = 0; var sectorsRemoved = 0;
        var affiliationsCreated = 0; var affiliationsUpdated = 0; var affiliationsRemoved = 0;

        var userIds = users.Select(x => x.UserId).ToHashSet();
        foreach (var existing in await db.Users.ToArrayAsync(token))
        {
            if (!userIds.Contains(existing.UserId)) { db.Users.Remove(existing); usersRemoved++; }
        }
        foreach (var snapshot in users)
        {
            var activeSubscription = string.Equals(snapshot.SubscriptionStatus, "Active", StringComparison.OrdinalIgnoreCase);
            var existing = await db.Users.FindAsync([snapshot.UserId], token);
            if (existing is null)
            {
                db.Users.Add(new Domain.Model.Entities.UserProjection
                {
                    UserId = snapshot.UserId, Role = snapshot.Role, HasActiveSubscription = activeSubscription, UpdatedAtUtc = DateTime.UtcNow
                });
                usersCreated++;
            }
            else if (existing.Role != snapshot.Role || existing.HasActiveSubscription != activeSubscription)
            {
                existing.Role = snapshot.Role; existing.HasActiveSubscription = activeSubscription; existing.UpdatedAtUtc = DateTime.UtcNow; usersUpdated++;
            }
        }

        var plantationIds = plantations.Select(x => x.Id).ToHashSet();
        foreach (var existing in await db.Plantations.ToArrayAsync(token))
        {
            if (!plantationIds.Contains(existing.PlantationId)) { db.Plantations.Remove(existing); plantationsRemoved++; }
        }
        foreach (var snapshot in plantations)
        {
            const bool active = true;
            var updatedAt = DateTime.UtcNow;
            var existing = await db.Plantations.FindAsync([snapshot.Id], token);
            if (existing is null)
            {
                db.Plantations.Add(new Domain.Model.Entities.PlantationProjection
                {
                    PlantationId = snapshot.Id, PalmGrowerId = snapshot.PalmGrowerId, Name = snapshot.Name, Active = active, UpdatedAtUtc = updatedAt
                });
                plantationsCreated++;
            }
            else if (existing.PalmGrowerId != snapshot.PalmGrowerId || existing.Name != snapshot.Name || existing.Active != active)
            {
                existing.PalmGrowerId = snapshot.PalmGrowerId; existing.Name = snapshot.Name; existing.Active = active; existing.UpdatedAtUtc = updatedAt; plantationsUpdated++;
            }
        }

        var sectorIds = sectors.Select(x => x.Id).ToHashSet();
        foreach (var existing in await db.Sectors.ToArrayAsync(token))
        {
            if (!sectorIds.Contains(existing.SectorId)) { db.Sectors.Remove(existing); sectorsRemoved++; }
        }
        foreach (var snapshot in sectors)
        {
            var plantationId = snapshot.PlantationId;
            const bool active = true;
            var existing = await db.Sectors.FindAsync([snapshot.Id], token);
            if (existing is null)
            {
                db.Sectors.Add(new Domain.Model.Entities.SectorProjection
                {
                    SectorId = snapshot.Id, PlantationId = plantationId, Name = snapshot.Name, Active = active,
                    UpdatedAtUtc = DateTime.UtcNow
                });
                sectorsCreated++;
            }
            else if (existing.Name != snapshot.Name || existing.Active != active || existing.PlantationId != plantationId)
            {
                existing.PlantationId = plantationId; existing.Name = snapshot.Name; existing.Active = active;
                existing.UpdatedAtUtc = DateTime.UtcNow; sectorsUpdated++;
            }
        }

        var affiliationIds = affiliations.Select(x => x.Id).ToHashSet();
        foreach (var existing in await db.Affiliations.ToArrayAsync(token))
        {
            if (!affiliationIds.Contains(existing.AffiliationId)) { db.Affiliations.Remove(existing); affiliationsRemoved++; }
        }
        foreach (var snapshot in affiliations)
        {
            const bool active = true;
            var updatedAt = DateTime.UtcNow;
            var existing = await db.Affiliations.FindAsync([snapshot.Id], token);
            if (existing is null)
            {
                db.Affiliations.Add(new Domain.Model.Entities.AgronomistAffiliationProjection
                {
                    AffiliationId = snapshot.Id, AgronomistId = snapshot.AgronomistId, PlantationId = snapshot.PlantationId,
                    Active = active, UpdatedAtUtc = updatedAt
                });
                affiliationsCreated++;
            }
            else if (existing.AgronomistId != snapshot.AgronomistId || existing.PlantationId != snapshot.PlantationId || existing.Active != active)
            {
                existing.AgronomistId = snapshot.AgronomistId; existing.PlantationId = snapshot.PlantationId; existing.Active = active; existing.UpdatedAtUtc = updatedAt; affiliationsUpdated++;
            }
        }

        await db.SaveChangesAsync(token);
        var result = new ProjectionReconciliationResult(
            usersCreated, usersUpdated, usersRemoved, plantationsCreated, plantationsUpdated, plantationsRemoved,
            sectorsCreated, sectorsUpdated, sectorsRemoved, affiliationsCreated, affiliationsUpdated, affiliationsRemoved);
        logger.LogInformation("Agronomy projection reconciliation completed with {TotalChanges} changes: {@Result}", result.TotalChanges, result);
        return result;
    }
}
