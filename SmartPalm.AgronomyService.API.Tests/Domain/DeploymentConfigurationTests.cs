using Microsoft.Extensions.Configuration;
using SmartPalm.AgronomyService.Infrastructure.Configuration;
using Xunit;

namespace SmartPalm.AgronomyService.API.Tests.Domain;

public class DeploymentConfigurationTests
{
    [Fact]
    public void ValidConfigurationPasses()
    {
        var exception = Record.Exception(() => DeploymentConfiguration.Validate(Configuration()));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("Reconciliation:IdentityBaseUrl", "identity-api:8080")]
    [InlineData("Reconciliation:CropBaseUrl", "")]
    public void InvalidReconciliationUrlFails(string key, string value)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => DeploymentConfiguration.Validate(Configuration((key, value))));

        Assert.Contains(key, exception.Message);
    }

    [Fact]
    public void ShortJwtKeyFails()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => DeploymentConfiguration.Validate(Configuration(("Jwt:Key", "too-short"))));

        Assert.Contains("Jwt:Key", exception.Message);
    }

    private static IConfiguration Configuration(params (string Key, string? Value)[] overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:AgronomyDatabase"] = "Host=postgres;Port=5432;Database=smartpalm_agronomy",
            ["Jwt:Issuer"] = "smartpalm.identity",
            ["Jwt:Audience"] = "smartpalm.clients",
            ["Jwt:Key"] = "agronomy-tests-signing-key-with-at-least-32-characters",
            ["RabbitMq:Enabled"] = "true",
            ["RabbitMq:Uri"] = "amqp://smartpalm:secret@rabbitmq:5672/",
            ["Reconciliation:IdentityBaseUrl"] = "http://identity-api:8080/",
            ["Reconciliation:CropBaseUrl"] = "http://crop-api:8080/"
        };
        foreach (var (key, value) in overrides)
            values[key] = value;
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
