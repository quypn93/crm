namespace CRM.Application.DTOs.Deal;

public class DealStageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Color { get; set; }
    public int Probability { get; set; }
    public bool IsDefault { get; set; }
    public bool IsWonStage { get; set; }
    public bool IsLostStage { get; set; }
}

public class DealDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "VND";
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid StageId { get; set; }
    public string? StageName { get; set; }
    public string? StageColor { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public int Probability { get; set; }
    public string? Notes { get; set; }
    public string? LostReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public int TasksCount { get; set; }
}

public class CreateDealDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "VND";
    public Guid CustomerId { get; set; }
    public Guid? StageId { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? Notes { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class UpdateDealDto : CreateDealDto
{
    public Guid Id { get; set; }
}

public class UpdateDealStageDto
{
    public Guid DealId { get; set; }
    public Guid StageId { get; set; }
}

public class MarkDealWonDto
{
    public DateTime? ActualCloseDate { get; set; }
}

public class MarkDealLostDto
{
    public string LostReason { get; set; } = string.Empty;
}

public class DealFilterDto
{
    public string? Search { get; set; }
    public Guid? StageId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? AssignedTo { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public DateTime? CloseDateFrom { get; set; }
    public DateTime? CloseDateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "desc";
}

public class DealsByStageDto
{
    public DealStageDto Stage { get; set; } = null!;
    public List<DealDto> Deals { get; set; } = new();
    public decimal TotalValue { get; set; }
    public int Count { get; set; }
}
