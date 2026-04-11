namespace CRM.Application.DTOs.Design;

public class DesignDto
{
    public Guid Id { get; set; }
    public string DesignName { get; set; } = string.Empty;
    public string? DesignData { get; set; }
    public string? SelectedComponents { get; set; }
    public string? Designer { get; set; }
    public string? CustomerFullName { get; set; }

    // Size quantities
    public int? Total { get; set; }
    public string? SizeMan { get; set; }
    public string? SizeWomen { get; set; }
    public string? SizeKid { get; set; }
    public string? Oversized { get; set; }

    // Production info
    public DateTime? FinishedDate { get; set; }
    public string? NoteConfection { get; set; }
    public string? NoteOldCodeOrder { get; set; }
    public string? NoteAttachTagLabel { get; set; }
    public string? NoteOther { get; set; }
    public string? SaleStaff { get; set; }

    // Foreign keys
    public Guid? ColorFabricId { get; set; }
    public string? ColorFabricName { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DesignDetailDto : DesignDto
{
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
}

public class CreateDesignDto
{
    public string DesignName { get; set; } = string.Empty;
    public string? DesignData { get; set; }
    public string? SelectedComponents { get; set; }
    public string? Designer { get; set; }
    public string? CustomerFullName { get; set; }

    // Size quantities
    public int? Total { get; set; }
    public string? SizeMan { get; set; }
    public string? SizeWomen { get; set; }
    public string? SizeKid { get; set; }
    public string? Oversized { get; set; }

    // Production info
    public DateTime? FinishedDate { get; set; }
    public string? NoteConfection { get; set; }
    public string? NoteOldCodeOrder { get; set; }
    public string? NoteAttachTagLabel { get; set; }
    public string? NoteOther { get; set; }
    public string? SaleStaff { get; set; }

    // Foreign keys
    public Guid? ColorFabricId { get; set; }
    public Guid? OrderId { get; set; }
}

public class UpdateDesignDto : CreateDesignDto
{
    public Guid Id { get; set; }
}

public class DesignFilterDto
{
    public string? Search { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? ColorFabricId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "desc";
}

public class DuplicateDesignDto
{
    public string? NewDesignName { get; set; }
    public Guid? NewOrderId { get; set; }
}
