using NumWordify.Converters;
using NumWordify.Extensions;
using NumWordify.Models;
using Xunit;

namespace NumWordify.Tests;

public class CurrencyTests
{
    [Fact]
    public void An_overridden_currency_reproduces_the_dedicated_locale_file()
    {
        // tr-TR-EUR.json exists only because the currency used to be baked into the
        // locale file. Overriding the currency has to give exactly the same answer,
        // which is what makes the extra file redundant.
        var viaOverride = 1234.56m.ToWords("tr-TR", new CurrencyModel { Major = "EURO", Minor = "SENT" });
        var viaCurrencyCode = 1234.56m.ToWords("tr-TR", "EUR");
        var viaDedicatedLocale = 1234.56m.ToWords("tr-TR-EUR");

        Assert.Equal(viaDedicatedLocale, viaOverride);
        Assert.Equal(viaDedicatedLocale, viaCurrencyCode);
        Assert.Equal("BİN İKİ YÜZ OTUZ DÖRT EURO ELLİ ALTI SENT", viaOverride);
    }

    [Fact]
    public void An_unknown_currency_code_names_the_ones_that_are_defined()
    {
        var exception = Assert.Throws<ArgumentException>(() => 1m.ToWords("tr-TR", "JPY"));

        Assert.Contains("JPY", exception.Message, StringComparison.Ordinal);
        Assert.Contains("EUR", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_currency_name_containing_a_placeholder_is_emitted_literally()
    {
        // The template is expanded in a single pass, so text substituted for {major}
        // is never rescanned for further placeholders.
        var currency = new CurrencyModel { Major = "{minor}", Minor = "KURUS" };

        var actual = 1.50m.ToWords("tr-TR", currency);

        Assert.Equal("BİR {minor} ELLİ KURUS", actual);
    }

    [Fact]
    public void An_empty_currency_name_does_not_leave_a_double_space()
    {
        var currency = new CurrencyModel { Major = string.Empty, Minor = "KURUS" };

        var actual = 1.50m.ToWords("tr-TR", currency);

        Assert.Equal("BİR ELLİ KURUS", actual);
        Assert.DoesNotContain("  ", actual, StringComparison.Ordinal);
    }

    [Fact]
    public void Converting_without_a_currency_needs_no_currency_at_all()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        var converter = new NumberToWordsConverter(localization);

        Assert.Equal("TWENTY-ONE POINT ZERO", converter.ConvertWithoutCurrency(21m));
        Assert.Throws<Exceptions.InvalidLocalizationException>(() => converter.Convert(21m));
    }

    [Fact]
    public void A_null_currency_override_is_rejected_at_the_call_site()
    {
        Assert.Throws<ArgumentNullException>(() => new NumberToWordsConverter("tr-TR", (CurrencyModel)null!));
    }
}
