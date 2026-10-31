namespace ShoeStore.Core.Models;

public sealed record References(
    IReadOnlyList<LookupItem> Categories,
    IReadOnlyList<LookupItem> Suppliers,
    IReadOnlyList<LookupItem> Manufacturers,
    IReadOnlyList<LookupItem> Units,
    IReadOnlyList<LookupItem> OrderStatuses,
    IReadOnlyList<PickupPointInfo> PickupPoints,
    IReadOnlyList<Role> Roles,
    IReadOnlyList<LookupItem> Departments);
