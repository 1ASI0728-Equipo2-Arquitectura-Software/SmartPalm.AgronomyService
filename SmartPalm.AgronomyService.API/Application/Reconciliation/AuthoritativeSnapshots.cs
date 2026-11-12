namespace SmartPalm.AgronomyService.Application.Reconciliation;

public sealed record IdentityUserSnapshot(long UserId, string Role, string Status, string? SubscriptionStatus);
public sealed record CropPlantationSnapshot(long Id, long PalmGrowerId, string Name);
public sealed record CropSectorSnapshot(long Id, long PlantationId, string Name);
public sealed record CropAffiliationSnapshot(long Id, long AgronomistId, long PlantationId);
public sealed record CropReconciliationSnapshot(
    IReadOnlyList<CropPlantationSnapshot> Plantations,
    IReadOnlyList<CropSectorSnapshot> Sectors,
    IReadOnlyList<CropAffiliationSnapshot> ActiveAgronomistAffiliations,
    long? NextPlantationId);

public sealed record ProjectionReconciliationResult(
    int UsersCreated, int UsersUpdated, int UsersRemoved,
    int PlantationsCreated, int PlantationsUpdated, int PlantationsRemoved,
    int SectorsCreated, int SectorsUpdated, int SectorsRemoved,
    int AffiliationsCreated, int AffiliationsUpdated, int AffiliationsRemoved)
{
    public int TotalChanges => UsersCreated + UsersUpdated + UsersRemoved
        + PlantationsCreated + PlantationsUpdated + PlantationsRemoved
        + SectorsCreated + SectorsUpdated + SectorsRemoved
        + AffiliationsCreated + AffiliationsUpdated + AffiliationsRemoved;
}
