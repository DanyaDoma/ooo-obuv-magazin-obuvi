namespace ShoeStore.Core.Models;

public sealed record Role(int Id, string Code, string Name)
{
    public const string AdminCode = "admin";
    public const string ManagerCode = "manager";
    public const string ClientCode = "client";
}
