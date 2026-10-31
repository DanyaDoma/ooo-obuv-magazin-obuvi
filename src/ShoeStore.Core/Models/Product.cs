using ShoeStore.Core.Utils;

namespace ShoeStore.Core.Models;

public sealed record Product(
    long Id,
    string Sku,
    string Name,
    int UnitId,
    string UnitName,
    decimal Price,
    int SupplierId,
    string SupplierName,
    int ManufacturerId,
    string ManufacturerName,
    int CategoryId,
    string CategoryName,
    decimal DiscountPercent,
    int StockQty,
    string? Description)
{
    public decimal FinalPrice => SqlHelpers.FinalPrice(Price, DiscountPercent);

    public string DisplayName => $"{Sku} — {Name}";
}
