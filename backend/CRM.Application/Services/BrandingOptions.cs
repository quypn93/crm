namespace CRM.Application.Services;

// Brand-specific text/identity được override theo từng deployment qua appsettings.{Environment}.json.
// Default values áp dụng cho deployment Đồng Phục Bốn Mùa (xanhuniform.com).
public class BrandingOptions
{
    public string CompanyName { get; set; } = "Đồng Phục Bốn Mùa";
    public string ApiTitle { get; set; } = "CRM API - Đồng Phục Bốn Mùa";
    public string ApiDescription { get; set; } = "API cho hệ thống quản lý khách hàng và đơn hàng đồng phục";
    public string ApiContactName { get; set; } = "Đồng Phục Bốn Mùa";
    public string ApiContactEmail { get; set; } = "dongphucbonmua@gmail.com";
    public string EmailFooter { get; set; } = "CRM Đồng Phục Bốn Mùa — vui lòng không trả lời email này.";
}
