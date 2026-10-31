using ShoeStore.Core;
using ShoeStore.Core.Utils;

namespace ShoeStore.Tests;

public class SkuTests
{
    [Theory]
    [InlineData("АВС1234", "ABC1234")]
    [InlineData("тЕСТ01", "TECT01")]
    [InlineData("  abcd1234  ", "ABCD1234")]
    public void Normalize_ReturnsCanonicalValue(string input, string expected)
    {
        Assert.Equal(expected, Sku.Normalize(input));
    }

    [Fact]
    public void NormalizeOrThrow_AcceptsLatinAndDigits()
    {
        Assert.Equal("AB12CD34", Sku.NormalizeOrThrow("ab12cd34"));
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("AB-1234")]
    [InlineData("АБВГ1234")]
    public void NormalizeOrThrow_RejectsInvalidValue(string input)
    {
        Assert.Throws<DomainException>(() => Sku.NormalizeOrThrow(input));
    }
}
