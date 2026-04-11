namespace CRM.Application.DTOs.Production;

public class ProductionStageDto
{
    public Guid Id { get; set; }
    public int StageOrder { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ResponsibleRole { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductionStageDto
{
    public int StageOrder { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ResponsibleRole { get; set; }
}

public class UpdateProductionStageDto
{
    public int StageOrder { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ResponsibleRole { get; set; }
    public bool IsActive { get; set; }
}

public class ReorderStageItem
{
    public Guid Id { get; set; }
    public int NewOrder { get; set; }
}

public class ReorderProductionStagesDto
{
    public List<ReorderStageItem> Stages { get; set; } = new();
}
