namespace CRM.API.Authorization;

public static class Policies
{
    // Order Management Policies
    public const string CanManageOrders = "CanManageOrders";
    public const string CanUpdatePayment = "CanUpdatePayment";
    public const string CanDeleteOrders = "CanDeleteOrders";
    public const string CanViewOrderSummary = "CanViewOrderSummary";

    // Customer Management Policies
    public const string CanDeleteCustomers = "CanDeleteCustomers";

    // Deal Management Policies
    public const string CanDeleteDeals = "CanDeleteDeals";
    public const string CanCloseDeal = "CanCloseDeal";

    // Dashboard Policies
    public const string CanViewFullDashboard = "CanViewFullDashboard";
    public const string CanViewProductionDashboard = "CanViewProductionDashboard";
    public const string CanViewQCDashboard = "CanViewQCDashboard";
    public const string CanViewDeliveryDashboard = "CanViewDeliveryDashboard";

    // Reports Policies
    public const string CanViewReports = "CanViewReports";
    public const string CanExportReports = "CanExportReports";

    // Design Management Policies
    public const string CanManageDesigns = "CanManageDesigns";
    public const string CanDeleteDesigns = "CanDeleteDesigns";
    public const string CanManageColorFabrics = "CanManageColorFabrics";
    public const string CanManageShirtComponents = "CanManageShirtComponents";
}
