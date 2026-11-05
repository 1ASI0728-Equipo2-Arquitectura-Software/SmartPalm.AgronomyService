namespace SmartPalm.AgronomyService.Domain.Model.Entities;

public class AgronomistAffiliationProjection
{
    public long AffiliationId { get; set; }
    public long AgronomistId { get; set; }
    public long PlantationId { get; set; }
    public bool Active { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; }
}
