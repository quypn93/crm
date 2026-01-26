namespace CRM.Application.DTOs.Customer;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? CompanyName { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public int DealsCount { get; set; }
    public int TasksCount { get; set; }
    public decimal TotalDealsValue { get; set; }
}

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? CompanyName { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class UpdateCustomerDto : CreateCustomerDto
{
    public Guid Id { get; set; }
}

public class CustomerFilterDto
{
    public string? Search { get; set; }
    public Guid? AssignedTo { get; set; }
    public bool? IsActive { get; set; }
    public string? Industry { get; set; }
    public string? City { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "desc";
}

// Danh sách ngành nghề phổ biến cho Đồng Phục Bốn Mùa
public static class CustomerIndustries
{
    public const string Corporate = "Doanh nghiệp";
    public const string Restaurant = "Nhà hàng";
    public const string Hotel = "Khách sạn";
    public const string School = "Trường học";
    public const string Healthcare = "Y tế";
    public const string Spa = "Spa & Thẩm mỹ";
    public const string Security = "Bảo vệ";
    public const string Kindergarten = "Mầm non";
    public const string Factory = "Nhà máy";
    public const string Retail = "Bán lẻ";
    public const string Other = "Khác";

    public static readonly List<string> All = new()
    {
        Corporate, Restaurant, Hotel, School, Healthcare,
        Spa, Security, Kindergarten, Factory, Retail, Other
    };
}
