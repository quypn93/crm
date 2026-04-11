using CRM.Core.Enums;

namespace CRM.Application.DTOs.Design;

public class ShirtComponentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? WomenImageUrl { get; set; }
    public ComponentType Type { get; set; }
    public string TypeName => Type.ToString();
    public bool IsDeleted { get; set; }
    public Guid? ColorFabricId { get; set; }
    public string? ColorFabricName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateShirtComponentDto
{
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? WomenImageUrl { get; set; }
    public ComponentType Type { get; set; }
    public Guid? ColorFabricId { get; set; }
}

public class UpdateShirtComponentDto : CreateShirtComponentDto
{
    public Guid Id { get; set; }
}

public class ShirtComponentFilterDto
{
    public string? Search { get; set; }
    public ComponentType? Type { get; set; }
    public Guid? ColorFabricId { get; set; }
    public bool? IncludeDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "asc";
}

// Helper to get ComponentType display names in Vietnamese
public static class ComponentTypeHelper
{
    public static readonly Dictionary<ComponentType, string> DisplayNames = new()
    {
        { ComponentType.Collar, "Cổ áo" },
        { ComponentType.Sleeve, "Tay áo" },
        { ComponentType.Logo, "Logo" },
        { ComponentType.Button, "Nút áo" },
        { ComponentType.Fabric, "Chất liệu vải" },
        { ComponentType.Color, "Màu sắc" },
        { ComponentType.Body, "Thân áo" },
        { ComponentType.Stripe, "Sọc" },
        { ComponentType.CollarStripe, "Sọc cổ áo" }
    };

    public static string GetDisplayName(ComponentType type)
    {
        return DisplayNames.TryGetValue(type, out var name) ? name : type.ToString();
    }
}
