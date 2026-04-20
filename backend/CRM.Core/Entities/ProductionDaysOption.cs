namespace CRM.Core.Entities;

public class ProductionDaysOption : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Days { get; set; }
    public bool IsActive { get; set; } = true;
}
