namespace ShoeStore.Core.Models;

public sealed record PickupPointInfo(int Id, string PostalCode, string City, string Address)
{
    public string Display => $"{City}, {Address} (индекс {PostalCode})";
}
