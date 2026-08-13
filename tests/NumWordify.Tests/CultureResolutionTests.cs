using System.Globalization;
using NumWordify.Converters;
using NumWordify.Exceptions;
using NumWordify.Models;
using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

public class CultureResolutionTests
{
    [Theory]
    [InlineData("en-US")]
    [InlineData("EN-US")]
    [InlineData("en-us")]
    [InlineData("en")]
    [InlineData("en-GB")]
    public void A_culture_resolves_regardless_of_casing_or_region(string culture)
    {
        var actual = 21m.ToWordsWithoutCurrency(culture);

        Assert.Equal("TWENTY-ONE POINT ZERO", actual);
    }

    [Fact]
    public void A_neutral_culture_picks_the_canonical_variant_not_a_currency_variant()
    {
        // Both tr-TR.json and tr-TR-EUR.json start with "tr-"; "tr" must land on the
        // plain Turkish locale rather than the euro one.
        Assert.Equal("BİR TL SIFIR Kr", 1m.ToWords("tr"));
    }

    [Fact]
    public void An_unsupported_culture_lists_the_supported_ones()
    {
        var exception = Assert.Throws<LocalizationNotFoundException>(() => 1m.ToWords("de-DE"));

        Assert.Equal("de-DE", exception.Culture);
        Assert.Contains("en-US", exception.AvailableCultures);
        Assert.Contains("tr-TR", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void The_invariant_culture_is_rejected_with_an_explanation()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new NumberToWordsConverter(CultureInfo.InvariantCulture));

        Assert.Contains("InvariantCulture", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_null_culture_is_rejected_as_a_null_argument()
    {
        Assert.Throws<ArgumentNullException>(() => new NumberToWordsConverter((string)null!));
        Assert.Throws<ArgumentNullException>(() => new NumberToWordsConverter((CultureInfo)null!));
    }

    [Theory]
    [InlineData("en-US", true)]
    [InlineData("tr", true)]
    [InlineData("de-DE", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Support_can_be_probed_without_catching_an_exception(string? culture, bool expected)
    {
        // The README used to suggest CultureInfo.CurrentCulture directly, which throws on
        // any machine whose culture is not one of the four shipped locales.
        Assert.Equal(expected, NumberToWordsConverter.IsCultureSupported(culture));
    }

    [Fact]
    public void The_supported_culture_list_excludes_deprecated_locales()
    {
        // tr-TR-EUR is a currency variant, not a culture. Listing it invited callers to
        // treat "locale per currency" as the supported pattern.
        Assert.Equal(
            ["en-US", "es-ES", "fr-FR", "pt-PT", "ru-RU", "tr-TR"],
            NumberToWordsConverter.SupportedCultures);
    }

    [Fact]
    public void A_deprecated_locale_still_resolves_when_named_exactly()
    {
        Assert.Equal("BİR EURO SIFIR SENT", 1m.ToWords("tr-TR-EUR"));
    }

    [Fact]
    public void A_culture_that_resolves_to_another_region_refuses_to_guess_the_currency()
    {
        // es-MX has no localization of its own. Borrowing Spanish number words is right;
        // borrowing the euro along with them is not.
        var exception = Assert.Throws<AmbiguousCurrencyException>(() => 1m.ToWords("es-MX"));

        Assert.Equal("es-MX", exception.RequestedCulture);
        Assert.Equal("es-ES", exception.ResolvedCulture);

        // The number words themselves are fine, and naming the currency resolves it.
        Assert.Equal("UNO COMA CERO", 1m.ToWordsWithoutCurrency("es-MX"));
        Assert.Equal("UN PESO CERO CENTAVOS", 1m.ToWords("es-MX", "MXN"));
    }

    [Fact]
    public void A_neutral_culture_accepts_the_language_default_currency()
    {
        // "es" names no region, so it is not claiming a currency the locale contradicts.
        Assert.Equal("UN EURO CERO CÉNTIMOS", 1m.ToWords("es"));
    }
}
