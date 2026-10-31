namespace ShoeStore.Core.Models;

public sealed record ProductInput(
    string Sku,
    string Name,
    int UnitId,
    decimal Price,
    int SupplierId,
    int ManufacturerId,
    int CategoryId,
    decimal DiscountPercent,
    int StockQty,
    string? Description);
