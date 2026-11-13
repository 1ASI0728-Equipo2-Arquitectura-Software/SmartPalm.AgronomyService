namespace SmartPalm.AgronomyService.Infrastructure.Configuration;

public sealed class ReconciliationOptions
{
    public const string SectionName = "Reconciliation";
    public string IdentityBaseUrl { get; set; } = string.Empty;
    public string CropBaseUrl { get; set; } = string.Empty;
    public string InternalBearerToken { get; set; } = string.Empty;
}
