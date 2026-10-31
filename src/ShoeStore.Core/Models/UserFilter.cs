namespace ShoeStore.Core.Models;

public sealed record UserFilter(
    string? Search,
    int? RoleId,
    int? DepartmentId);
