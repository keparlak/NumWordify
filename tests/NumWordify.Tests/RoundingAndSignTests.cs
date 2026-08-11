using System.Globalization;
using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Money rounds away from zero. The framework default rounds midpoints to even, which
/// silently turned 1.005 into "one dollar zero cents".
/// </summary>
public class RoundingAndSignTests
{
    private const string Culture = "en-US";

    [Theory]
    [InlineData("1.005", "ONE DOLLAR ONE CENT")]
    [InlineData("1.015", "ONE DOLLAR TWO CENTS")]
    [InlineData("2.345", "TWO DOLLARS THIRTY-FIVE CENTS")]
    [InlineData("0.125", "ZERO DOLLARS THIRTEEN CENTS")]
    [InlineData("0.135", "ZERO DOLLARS FOURTEEN CENTS")]
    public void Midpoints_round_away_from_zero(string value, string expected)
    {
        var actual = Parse(value).ToWords(Culture);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("0.999", "ONE DOLLAR ZERO CENTS")]
    [InlineData("1.999", "TWO DOLLARS ZERO CENTS")]
    [InlineData("9.9999", "TEN DOLLARS ZERO CENTS")]
    public void A_fraction_that_rounds_up_carries_into_the_whole_part(string value, string expected)
    {
        var actual = Parse(value).ToWords(Culture);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("1.234567", "ONE DOLLAR TWENTY-THREE CENTS")]
    [InlineData("1.2350", "ONE DOLLAR TWENTY-FOUR CENTS")]
    public void Digits_beyond_the_configured_precision_are_rounded_away(string value, string expected)
    {
        var actual = Parse(value).ToWords(Culture);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("-0.001")]
    [InlineData("-0.004")]
    [InlineData("-0")]
    public void A_value_that_rounds_to_zero_is_not_reported_as_negative(string value)
    {
        var actual = Parse(value).ToWords(Culture);

        Assert.Equal("ZERO DOLLARS ZERO CENTS", actual);
    }

    [Fact]
    public void A_value_that_survives_rounding_keeps_its_sign()
    {
        Assert.Equal("NEGATIVE ZERO DOLLARS ONE CENT", Parse("-0.005").ToWords(Culture));
        Assert.Equal("NEGATIVE ONE DOLLAR ONE CENT", Parse("-1.01").ToWords(Culture));
    }

    private static decimal Parse(string value) =>
        decimal.Parse(value, CultureInfo.InvariantCulture);
}
