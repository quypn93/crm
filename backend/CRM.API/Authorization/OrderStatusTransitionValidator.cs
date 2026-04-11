using CRM.Core.Entities;
using CRM.Core.Enums;

namespace CRM.API.Authorization;

public static class OrderStatusTransitionValidator
{
    // Define which roles can perform each status transition
    private static readonly Dictionary<(OrderStatus From, OrderStatus To), string[]> AllowedTransitions = new()
    {
        // Draft transitions
        { (OrderStatus.Draft, OrderStatus.Confirmed), new[] { RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep } },
        { (OrderStatus.Draft, OrderStatus.Cancelled), new[] { RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep } },

        // Confirmed transitions
        { (OrderStatus.Confirmed, OrderStatus.InProduction), new[] { RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep, RoleNames.ProductionManager } },
        { (OrderStatus.Confirmed, OrderStatus.Cancelled), new[] { RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep } },

        // InProduction transitions
        { (OrderStatus.InProduction, OrderStatus.QualityCheck), new[] { RoleNames.Admin, RoleNames.ProductionManager } },
        { (OrderStatus.InProduction, OrderStatus.Cancelled), new[] { RoleNames.Admin, RoleNames.SalesManager } },

        // QualityCheck transitions
        { (OrderStatus.QualityCheck, OrderStatus.ReadyToShip), new[] { RoleNames.Admin, RoleNames.QualityControl } },
        { (OrderStatus.QualityCheck, OrderStatus.InProduction), new[] { RoleNames.Admin, RoleNames.ProductionManager, RoleNames.QualityControl } }, // Rework

        // ReadyToShip transitions
        { (OrderStatus.ReadyToShip, OrderStatus.Shipping), new[] { RoleNames.Admin, RoleNames.DeliveryManager } },

        // Shipping transitions
        { (OrderStatus.Shipping, OrderStatus.Delivered), new[] { RoleNames.Admin, RoleNames.DeliveryManager } },

        // Delivered transitions
        { (OrderStatus.Delivered, OrderStatus.Completed), new[] { RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep } },
    };

    /// <summary>
    /// Check if the user with given roles can perform the status transition
    /// </summary>
    public static bool CanTransition(OrderStatus fromStatus, OrderStatus toStatus, IEnumerable<string> userRoles)
    {
        // Admin can do anything
        if (userRoles.Contains(RoleNames.Admin))
            return true;

        var key = (fromStatus, toStatus);
        if (AllowedTransitions.TryGetValue(key, out var allowedRoles))
        {
            return userRoles.Any(role => allowedRoles.Contains(role));
        }

        return false;
    }

    /// <summary>
    /// Get the list of allowed next statuses for a given current status and user roles
    /// </summary>
    public static IEnumerable<OrderStatus> GetAllowedNextStatuses(OrderStatus currentStatus, IEnumerable<string> userRoles)
    {
        var rolesList = userRoles.ToList();

        return AllowedTransitions
            .Where(kvp => kvp.Key.From == currentStatus)
            .Where(kvp => rolesList.Any(role => kvp.Value.Contains(role)) || rolesList.Contains(RoleNames.Admin))
            .Select(kvp => kvp.Key.To)
            .Distinct();
    }

    /// <summary>
    /// Get user-friendly error message for invalid transition
    /// </summary>
    public static string GetTransitionErrorMessage(OrderStatus fromStatus, OrderStatus toStatus, IEnumerable<string> userRoles)
    {
        var key = (fromStatus, toStatus);
        if (!AllowedTransitions.ContainsKey(key))
        {
            return $"Khong the chuyen trang thai tu '{fromStatus}' sang '{toStatus}'. Chuyen doi nay khong hop le.";
        }

        var allowedRoles = AllowedTransitions[key];
        return $"Ban khong co quyen chuyen trang thai tu '{fromStatus}' sang '{toStatus}'. Yeu cau role: {string.Join(", ", allowedRoles)}.";
    }
}
