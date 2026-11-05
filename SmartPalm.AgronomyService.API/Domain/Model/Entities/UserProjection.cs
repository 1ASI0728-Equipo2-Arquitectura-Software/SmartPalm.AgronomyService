namespace SmartPalm.AgronomyService.Domain.Model.Entities;

public class UserProjection
{
    public long UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool HasActiveSubscription { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
