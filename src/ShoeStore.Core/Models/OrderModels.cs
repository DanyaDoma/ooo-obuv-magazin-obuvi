namespace ShoeStore.Core.Models;

public sealed record OrderSummary(
    long Id,
    long UserId,
    int PickupPointId,
    int StatusId,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string PickupCode,
    decimal TotalAmount,
    string StatusName,
    string UserFullName,
    PickupPointInfo PickupPoint,
    int ItemsCount);

public sealed record OrderItemLine(
    long ProductId,
    string Sku,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal LineTotal);

public sealed record OrderDetails(OrderSummary Summary, IReadOnlyList<OrderItemLine> Items);

public sealed record OrderItemInput(long ProductId, int Quantity);

public sealed record OrderInput(
    long UserId,
    int PickupPointId,
    int StatusId,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string PickupCode,
    IReadOnlyList<OrderItemInput> Items);
