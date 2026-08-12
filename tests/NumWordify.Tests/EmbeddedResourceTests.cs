using NumWordify.Converters;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// A smoke test over every shipped locale: each one loads, passes validation, and
/// converts a wide spread of values without throwing or producing an empty string.
/// </summary>
/// <remarks>
/// This checks that nothing crashes; it does not check that the words are right. The
/// per-locale golden tables and <see cref="GoldenSnapshotTests"/> do that.
/// </remarks>
public class EmbeddedResourceTests
{
    /// <summary>
    /// Every embedded locale, including deprecated ones — those still have to work.
    /// </summary>
    public static TheoryData<string> Cultures()
    {
        var data = new TheoryData<string>();
        foreach (var culture in NumberToWordsConverter.SupportedCultures)
            data.Add(culture);

        data.Add("tr-TR-EUR");

        return data;
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Every_shipped_locale_loads_and_validates(string culture)
    {
        // Construction runs the full validator, so this covers array lengths, override
        // key ranges, teens length and format strings for every file at once.
        var converter = new NumberToWordsConverter(culture);

        Assert.NotNull(converter);
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Every_shipped_locale_converts_the_whole_range_without_throwing(string culture)
    {
        var converter = new NumberToWordsConverter(culture);

        for (var value = 0; value <= 1_000; value++)
        {
            Assert.False(string.IsNullOrWhiteSpace(converter.ConvertWithoutCurrency(value)));
            Assert.False(string.IsNullOrWhiteSpace(converter.Convert(value)));
        }

        // Capped at 10^15 − 1, the lowest ceiling among the shipped locales: Spanish
        // defines five scale words because there is no single word for 10^15.
        foreach (var value in new[]
                 {
                     1_001m, 12_345m, 999_999m, 1_000_000m, 123_456_789m,
                     1_000_000_000m, 987_654_321_012_345m,
                 })
        {
            Assert.False(string.IsNullOrWhiteSpace(converter.Convert(value)));
        }
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Every_shipped_locale_defines_a_default_currency(string culture)
    {
        var converter = new NumberToWordsConverter(culture);

        // Convert throws InvalidLocalizationException when no currency is available.
        Assert.False(string.IsNullOrWhiteSpace(converter.Convert(1234.56m)));
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void No_conversion_produces_repeated_or_edge_whitespace(string culture)
    {
        var converter = new NumberToWordsConverter(culture);

        foreach (var value in new[] { 0m, 1m, 100m, 1_000m, 1_000_000m, -1234.56m, 0.01m })
        {
            var withCurrency = converter.Convert(value);
            var withoutCurrency = converter.ConvertWithoutCurrency(value);

            Assert.DoesNotContain("  ", withCurrency, StringComparison.Ordinal);
            Assert.DoesNotContain("  ", withoutCurrency, StringComparison.Ordinal);
            Assert.Equal(withCurrency.Trim(), withCurrency);
            Assert.Equal(withoutCurrency.Trim(), withoutCurrency);
        }
    }
}
