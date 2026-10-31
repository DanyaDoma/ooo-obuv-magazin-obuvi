namespace ShoeStore.Core.Utils;

public static class SqlHelpers
{
    public static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    public static string LikeContains(string value) => $"%{EscapeLike(value)}%";

    public static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public static decimal FinalPrice(decimal price, decimal discountPercent) =>
        RoundMoney(price * (1 - discountPercent / 100m));
}
