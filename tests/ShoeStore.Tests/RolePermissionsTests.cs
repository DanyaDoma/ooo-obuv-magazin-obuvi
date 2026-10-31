using ShoeStore.Core.Security;

namespace ShoeStore.Tests;

public class RolePermissionsTests
{
    [Theory]
    [InlineData("admin", true, true, true, true)]
    [InlineData("manager", true, false, true, false)]
    [InlineData("client", false, false, false, false)]
    [InlineData(null, false, false, false, false)]
    public void Permissions_MatchRoleMatrix(
        string? roleCode,
        bool filters,
        bool manageProducts,
        bool viewOrders,
        bool manageOrders)
    {
        Assert.Equal(filters, RolePermissions.CanUseProductFilters(roleCode));
        Assert.Equal(manageProducts, RolePermissions.CanManageProducts(roleCode));
        Assert.Equal(viewOrders, RolePermissions.CanViewOrders(roleCode));
        Assert.Equal(manageOrders, RolePermissions.CanManageOrders(roleCode));
    }
}
