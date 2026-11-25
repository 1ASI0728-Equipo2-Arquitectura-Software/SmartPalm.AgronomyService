using System.Text;

namespace SmartPalm.AgronomyService.Infrastructure.Configuration;

public static class DeploymentConfiguration
{
    public static void Validate(IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("AgronomyDatabase")))
            throw new InvalidOperationException("ConnectionStrings:AgronomyDatabase must be configured.");

        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("Jwt:Issuer must be configured.");
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Jwt:Audience must be configured.");
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("Jwt:Key must be configured with at least 32 bytes.");

        if (configuration.GetValue<bool>("RabbitMq:Enabled"))
            RequireAbsoluteUri(configuration["RabbitMq:Uri"], "RabbitMq:Uri", "RabbitMq__Uri");

        RequireAbsoluteHttpUrl(configuration["Reconciliation:IdentityBaseUrl"], "Reconciliation:IdentityBaseUrl");
        RequireAbsoluteHttpUrl(configuration["Reconciliation:CropBaseUrl"], "Reconciliation:CropBaseUrl");
    }

    private static void RequireAbsoluteHttpUrl(string? value, string key)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new InvalidOperationException($"{key} must be an absolute HTTP(S) URL.");
    }

    private static void RequireAbsoluteUri(string? value, string key, string environmentName)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            throw new InvalidOperationException($"{key} must be an absolute URI (set {environmentName}).");
    }
}
