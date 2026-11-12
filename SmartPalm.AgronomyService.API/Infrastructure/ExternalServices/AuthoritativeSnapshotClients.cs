using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Application.Reconciliation;
using SmartPalm.AgronomyService.Infrastructure.Configuration;

namespace SmartPalm.AgronomyService.Infrastructure.ExternalServices;

internal sealed class IdentitySnapshotClient(HttpClient http, IOptions<ReconciliationOptions> options) : IIdentitySnapshotClient
{
    private readonly ReconciliationOptions _options = options.Value;

    public async Task<IReadOnlyList<IdentityUserSnapshot>> ListUsersAsync(CancellationToken token)
    {
        var users = await GetAsync<IReadOnlyList<IdentityUserListItem>>("api/v1/users", token);
        var snapshots = new List<IdentityUserSnapshot>(users.Count);
        foreach (var user in users)
            snapshots.Add(await GetAsync<IdentityUserSnapshot>($"api/v1/reconciliation/users/{user.Id}/snapshot", token));
        return snapshots;
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        AddBearer(request);
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: token)
            ?? throw new InvalidOperationException($"IdentityService returned an empty response for {path}.");
    }

    private void AddBearer(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_options.InternalBearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.InternalBearerToken);
    }

    private sealed record IdentityUserListItem(long Id);
}

internal sealed class CropSnapshotClient(HttpClient http, IOptions<ReconciliationOptions> options) : ICropSnapshotClient
{
    private readonly ReconciliationOptions _options = options.Value;

    public async Task<CropReconciliationSnapshot> GetSnapshotAsync(CancellationToken token)
    {
        var plantations = new List<CropPlantationSnapshot>();
        var sectors = new List<CropSectorSnapshot>();
        var affiliations = new List<CropAffiliationSnapshot>();
        long? cursor = null;
        do
        {
            var path = "api/v1/internal/reconciliation/snapshot?limit=500"
                + (cursor.HasValue ? $"&afterPlantationId={cursor.Value}" : string.Empty);
            var page = await GetAsync<CropReconciliationSnapshot>(path, token);
            plantations.AddRange(page.Plantations);
            sectors.AddRange(page.Sectors);
            affiliations.AddRange(page.ActiveAgronomistAffiliations);
            cursor = page.NextPlantationId;
        } while (cursor.HasValue);
        return new CropReconciliationSnapshot(plantations, sectors, affiliations, null);
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (!string.IsNullOrWhiteSpace(_options.InternalBearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.InternalBearerToken);
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: token)
            ?? throw new InvalidOperationException($"CropService returned an empty response for {path}.");
    }
}
