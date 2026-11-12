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

    [Fact]
    public void Identity_and_crop_payloads_preserve_the_reconciliation_entities()
    {
        const string identityUser = """
            {"userId":9,"username":"pedro01","email":"prueba02@gmail.com","fullName":"Pedro","role":"Agronomist","status":"Active","subscriptionStatus":null}
            """;
        const string cropSnapshot = """
            {
              "plantations":[{"id":6,"palmGrowerId":11,"name":"Finca02"}],
              "sectors":[{"id":6,"plantationId":6,"name":"Sector Norte"}],
              "activeAgronomistAffiliations":[{"id":2,"agronomistId":9,"plantationId":6}],
              "nextPlantationId":null
            }
            """;

        var user = JsonSerializer.Deserialize<IdentityUserSnapshot>(
            identityUser, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var crop = JsonSerializer.Deserialize<CropReconciliationSnapshot>(
            cropSnapshot, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(user);
        Assert.Equal(9, user.UserId);
        Assert.Equal("Agronomist", user.Role);
        Assert.Null(user.SubscriptionStatus);
        Assert.NotNull(crop);
        Assert.Equal(11, crop.Plantations.Single().PalmGrowerId);
        Assert.Equal(6, crop.Sectors.Single().PlantationId);
        var affiliation = Assert.Single(crop.ActiveAgronomistAffiliations);
        Assert.Equal(2, affiliation.Id);
        Assert.Equal(9, affiliation.AgronomistId);
        Assert.Equal(6, affiliation.PlantationId);
    }
}
