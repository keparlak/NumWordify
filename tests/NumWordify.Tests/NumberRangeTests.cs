using NumWordify.Exceptions;
using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// The API takes a <see cref="decimal"/> but the shipped locales define six scale words,
/// so the convertible range stops at 10^18 − 1. That ceiling has to be a documented,
/// typed failure rather than a surprise.
/// </summary>
public class NumberRangeTests
{
    [Fact]
    public void The_largest_value_the_scale_words_can_express_converts()
    {
        var actual = 999_999_999_999_999_999m.ToWordsWithoutCurrency("en-US");

        Assert.StartsWith("NINE HUNDRED NINETY-NINE QUADRILLION", actual, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("1000000000000000000")]
    [InlineData("79228162514264337593543950335")]
    [InlineData("-79228162514264337593543950335")]
    public void Anything_larger_reports_the_limit(string value)
    {
        var number = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        var exception = Assert.Throws<NumberOutOfRangeException>(() => number.ToWords("en-US"));

        Assert.Equal(6, exception.MaximumScaleCount);
        Assert.Contains("10^18", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Decimal_extremes_fail_the_same_way_in_both_directions()
    {
        Assert.Throws<NumberOutOfRangeException>(() => decimal.MaxValue.ToWords("tr-TR"));
        Assert.Throws<NumberOutOfRangeException>(() => decimal.MinValue.ToWords("tr-TR"));
    }
}
