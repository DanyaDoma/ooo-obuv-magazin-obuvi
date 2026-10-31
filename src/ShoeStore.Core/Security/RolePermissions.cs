using ShoeStore.Core.Models;

namespace ShoeStore.Core.Security;

public static class RolePermissions
{
    public static bool CanUseProductFilters(string? roleCode) =>
        roleCode is Role.AdminCode or Role.ManagerCode;

    public static bool CanManageProducts(string? roleCode) =>
        roleCode == Role.AdminCode;

    public static bool CanViewOrders(string? roleCode) =>
        roleCode is Role.AdminCode or Role.ManagerCode;

    public static bool CanManageOrders(string? roleCode) =>
        roleCode == Role.AdminCode;
}
