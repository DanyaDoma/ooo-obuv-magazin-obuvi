using ShoeStore.Core.Utils;

namespace ShoeStore.Tests;

public class SqlHelpersTests
{
    [Fact]
    public void EscapeLike_EscapesBackslashPercentAndUnderscore()
    {
        Assert.Equal(@"50\%\_off\\", SqlHelpers.EscapeLike(@"50%_off\"));
    }

    [Fact]
    public void LikeContains_WrapsEscapedValue()
    {
        Assert.Equal(@"%Иван\_%", SqlHelpers.LikeContains("Иван_"));
    }

    [Theory]
    [InlineData(10.005, 10.01)]
    [InlineData(10.004, 10.00)]
    [InlineData(0, 0)]
    public void RoundMoney_RoundsToTwoDigitsAwayFromZero(double value, double expected)
    {
        Assert.Equal((decimal)expected, SqlHelpers.RoundMoney((decimal)value));
    }

    [Theory]
    [InlineData(1000, 10, 900)]
    [InlineData(199.99, 15, 169.99)]
    [InlineData(500, 0, 500)]
    public void FinalPrice_AppliesDiscount(double price, double discount, double expected)
    {
        Assert.Equal((decimal)expected, SqlHelpers.FinalPrice((decimal)price, (decimal)discount));
    }
}
