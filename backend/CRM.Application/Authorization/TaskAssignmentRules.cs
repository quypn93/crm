using CRM.Core.Entities;

namespace CRM.Application.Authorization;

public static class TaskAssignmentRules
{
    private static readonly Dictionary<string, string[]> AssignableTargets = new()
    {
        [RoleNames.Admin] = new[]
        {
            RoleNames.SalesManager, RoleNames.SalesRep,
            RoleNames.ProductionManager, RoleNames.ProductionStaff,
            RoleNames.CuttingStaff, RoleNames.SewingStaff, RoleNames.PrintingStaff,
            RoleNames.FinishingStaff, RoleNames.PackagingStaff,
            RoleNames.QualityManager, RoleNames.QualityControl,
            RoleNames.DeliveryManager, RoleNames.DeliveryStaff,
            RoleNames.DesignManager, RoleNames.Designer,
            RoleNames.ContentManager, RoleNames.ContentStaff
        },
        [RoleNames.SalesManager] = new[] { RoleNames.SalesRep },
        [RoleNames.ProductionManager] = new[]
        {
            RoleNames.ProductionStaff,
            RoleNames.CuttingStaff, RoleNames.SewingStaff, RoleNames.PrintingStaff,
            RoleNames.FinishingStaff, RoleNames.PackagingStaff
        },
        [RoleNames.QualityManager] = new[] { RoleNames.QualityControl },
        [RoleNames.DeliveryManager] = new[] { RoleNames.DeliveryStaff },
        [RoleNames.DesignManager] = new[] { RoleNames.Designer },
        [RoleNames.ContentManager] = new[] { RoleNames.ContentStaff, RoleNames.Designer },
        [RoleNames.ContentStaff] = new[] { RoleNames.Designer }
    };

    public static HashSet<string> GetAssignableTargetRoles(IEnumerable<string> currentUserRoles)
    {
        var set = new HashSet<string>();
        foreach (var role in currentUserRoles)
        {
            if (AssignableTargets.TryGetValue(role, out var targets))
            {
                foreach (var t in targets) set.Add(t);
            }
        }
        return set;
    }

    public static bool CanAssignTo(IEnumerable<string> currentUserRoles, IEnumerable<string> targetUserRoles)
    {
        var allowed = GetAssignableTargetRoles(currentUserRoles);
        return allowed.Count > 0 && targetUserRoles.Any(allowed.Contains);
    }
}
