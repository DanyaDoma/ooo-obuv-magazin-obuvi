namespace ShoeStore.Core.Models;

public sealed record ProductFilter(
    string? Search,
    int? CategoryId,
    int? SupplierId,
    int? ManufacturerId,
    bool DiscountOnly,
    string? Sort);
