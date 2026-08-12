using System.Globalization;
using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Golden table for en-US. Every value whose last two digits are zero is a regression
/// test: a <c>"0": "ZERO"</c> entry in the special-numbers map used to append a stray
/// "ZERO" to them.
/// </summary>
public class EnglishConversionTests
{
    private const string Culture = "en-US";
    private const string ZeroFraction = " POINT ZERO";

    [Theory]
    [InlineData(0, "ZERO")]
    [InlineData(1, "ONE")]
    [InlineData(10, "TEN")]
    [InlineData(11, "ELEVEN")]
    [InlineData(15, "FIFTEEN")]
    [InlineData(19, "NINETEEN")]
    [InlineData(20, "TWENTY")]
    [InlineData(21, "TWENTY-ONE")]
    [InlineData(34, "THIRTY-FOUR")]
    [InlineData(99, "NINETY-NINE")]
    [InlineData(100, "ONE HUNDRED")]
    [InlineData(101, "ONE HUNDRED ONE")]
    [InlineData(111, "ONE HUNDRED ELEVEN")]
    [InlineData(115, "ONE HUNDRED FIFTEEN")]
    [InlineData(200, "TWO HUNDRED")]
    [InlineData(999, "NINE HUNDRED NINETY-NINE")]
    [InlineData(1000, "ONE THOUSAND")]
    [InlineData(1100, "ONE THOUSAND ONE HUNDRED")]
    [InlineData(2400, "TWO THOUSAND FOUR HUNDRED")]
    [InlineData(100000, "ONE HUNDRED THOUSAND")]
    [InlineData(1234567, "ONE MILLION TWO HUNDRED THIRTY-FOUR THOUSAND FIVE HUNDRED SIXTY-SEVEN")]
    [InlineData(1000000000, "ONE BILLION")]
    public void Whole_numbers_are_read_in_full(long value, string expected)
    {
        var actual = ((decimal)value).ToWordsWithoutCurrency(Culture);

        Assert.Equal(expected + ZeroFraction, actual);
    }

    [Theory]
    [InlineData("0", "ZERO DOLLARS ZERO CENTS")]
    [InlineData("100", "ONE HUNDRED DOLLARS ZERO CENTS")]
    [InlineData("500.25", "FIVE HUNDRED DOLLARS TWENTY-FIVE CENTS")]
    [InlineData("11234.56", "ELEVEN THOUSAND TWO HUNDRED THIRTY-FOUR DOLLARS FIFTY-SIX CENTS")]
    public void Currency_amounts_are_read_in_full(string value, string expected)
    {
        var actual = decimal.Parse(value, CultureInfo.InvariantCulture).ToWords(Culture);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("1.01", "ONE DOLLAR ONE CENT")]
    [InlineData("1.02", "ONE DOLLAR TWO CENTS")]
    [InlineData("2.01", "TWO DOLLARS ONE CENT")]
    [InlineData("2.02", "TWO DOLLARS TWO CENTS")]
    public void Currency_names_agree_in_number(string value, string expected)
    {
        var actual = decimal.Parse(value, CultureInfo.InvariantCulture).ToWords(Culture);

        Assert.Equal(expected, actual);
    }
}
