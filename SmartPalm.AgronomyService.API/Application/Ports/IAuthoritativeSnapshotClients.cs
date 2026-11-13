using SmartPalm.AgronomyService.Application.Reconciliation;

namespace SmartPalm.AgronomyService.Application.Ports;

public interface IIdentitySnapshotClient
{
    Task<IReadOnlyList<IdentityUserSnapshot>> ListUsersAsync(CancellationToken token);
}

public interface ICropSnapshotClient
{
    Task<CropReconciliationSnapshot> GetSnapshotAsync(CancellationToken token);
}
