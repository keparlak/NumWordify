using NumWordify.Converters;
using NumWordify.Exceptions;
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

        // Every shipped locale has to reach at least 10^9 − 1. That is the floor, and it
        // is asserted rather than assumed, so a locale that silently lost scale words
        // fails here instead of somewhere downstream.
        foreach (var value in new[] { 1_001m, 12_345m, 999_999m, 1_000_000m, 123_456_789m, 999_999_999m })
        {
            Assert.False(string.IsNullOrWhiteSpace(converter.Convert(value)));
        }

        // Above that the locales diverge and there is no shared cap: pt-PT stops right
        // there, because a thousand million is two words in European Portuguese and the
        // scale table holds one word per step; es-ES stops at 10^15 − 1. A locale may
        // refuse these, but only by saying so.
        foreach (var value in new[] { 1_000_000_000m, 987_654_321_012_345m })
        {
            var exception = Record.Exception(() => converter.Convert(value));

            if (exception is null)
                Assert.False(string.IsNullOrWhiteSpace(converter.Convert(value)));
            else
                Assert.IsType<NumberOutOfRangeException>(exception);
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
