namespace SmartPalm.AgronomyService.Domain.Model.Entities;

public class PlantationProjection
{
    public long PlantationId { get; set; }
    public long PalmGrowerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; }
}
