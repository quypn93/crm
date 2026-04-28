using CRM.Core.Enums;

namespace CRM.Application.DTOs.Design;

public class DesignDto
{
    public Guid Id { get; set; }
    public string DesignName { get; set; } = string.Empty;
    public string? DesignData { get; set; }
    public string? SelectedComponents { get; set; }
    public string? Designer { get; set; }
    public string? CustomerFullName { get; set; }

    // Assignment flow
    public DesignStatus Status { get; set; } = DesignStatus.Assigned;
    public string StatusName => Status switch
    {
        DesignStatus.Assigned   => "Đã giao",
        DesignStatus.InProgress => "Đang làm",
        DesignStatus.Completed  => "Hoàn thành",
        DesignStatus.Cancelled  => "Đã huỷ",
        _ => "Không xác định"
    };
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public Guid? ShirtFormId { get; set; }
    public string? ShirtFormName { get; set; }
    public Guid? AccentColorFabricId { get; set; }
    public string? AccentColorFabricName { get; set; }
    public string? ChestLogoUrl { get; set; }
    public string? BackLogoUrl { get; set; }
    public string? CompletedImageUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? AssignmentNotes { get; set; }

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

// Sale tạo assignment cho designer — form gọn, chỉ các field cần thiết.
public class CreateDesignAssignmentDto
{
    public string DesignName { get; set; } = string.Empty;   // Tên gợi nhớ, VD "Áo polo ABC - Batch 1"
    public Guid AssignedToUserId { get; set; }               // Designer được giao (bắt buộc)
    public Guid? ShirtFormId { get; set; }                   // Mẫu áo
    public Guid? ColorFabricId { get; set; }                 // Màu áo chính
    public Guid? AccentColorFabricId { get; set; }           // Màu phối
    public string? ChestLogoUrl { get; set; }                // URL logo ngực (upload trước)
    public string? BackLogoUrl { get; set; }                 // URL logo lưng
    public string? AssignmentNotes { get; set; }
    public Guid? OrderId { get; set; }                       // Optional: gắn vào đơn
    public string? CustomerFullName { get; set; }
}

public class CompleteDesignDto
{
    public string CompletedImageUrl { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class UploadImageResultDto
{
    public string Url { get; set; } = string.Empty;
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
    public DesignStatus? Status { get; set; }
    public Guid? AssignedToUserId { get; set; }
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
