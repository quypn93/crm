namespace CRM.Core.Entities;

public class DealStage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Color { get; set; }
    public int Probability { get; set; } = 0;
    public bool IsDefault { get; set; } = false;
    public bool IsWonStage { get; set; } = false;
    public bool IsLostStage { get; set; } = false;

    // Navigation properties
    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
}
