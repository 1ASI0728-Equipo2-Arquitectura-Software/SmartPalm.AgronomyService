using System.Text.Json;
using SmartPalm.AgronomyService.Application.Reconciliation;

namespace SmartPalm.AgronomyService.API.Tests.Domain;

public class AuthoritativeSnapshotContractTests
{
    [Fact]
    public void Identity_snapshot_allows_users_without_a_subscription()
    {
        var snapshot = JsonSerializer.Deserialize<IdentityUserSnapshot>(
            """{"userId":1,"role":"PalmGrower","status":"Active","subscriptionStatus":null}""",
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(snapshot);
        Assert.Null(snapshot.SubscriptionStatus);
    }
}
