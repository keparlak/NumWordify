using NumWordify.Converters;
using NumWordify.Exceptions;
using NumWordify.Extensions;
using NumWordify.Models;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Each of these is a model that used to pass validation and then misbehave at
/// conversion time — a crash, or worse, a silently dropped word.
/// </summary>
public class ValidationGapTests
{
    [Fact]
    public void A_null_entry_in_the_plural_scale_table_is_rejected()
    {
        // Used to construct fine and throw NullReferenceException on 2000 but not on 1000,
        // because the plural table is only consulted when the group is greater than one.
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Numbers!.ScalesPlural = ["", null!, "MILLIONS"];

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("scalesPlural[1]", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void An_empty_entry_in_the_plural_scale_table_falls_back()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Numbers!.ScalesPlural = ["", "", "MILLIONS"];

        var converter = new NumberToWordsConverter(localization);

        Assert.Equal("TWO THOUSAND POINT ZERO", converter.ConvertWithoutCurrency(2000m));
        Assert.Equal("TWO MILLIONS POINT ZERO", converter.ConvertWithoutCurrency(2_000_000m));
    }

    [Fact]
    public void An_empty_scale_word_is_rejected_instead_of_vanishing()
    {
        // Used to make 2000 and 2 produce the same words.
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Numbers!.Scales = ["", "", "MILLION"];

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("scales[1]", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_scale_kind_table_of_the_wrong_length_is_rejected()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Numbers!.ScaleKinds = [ScaleKind.Adjective];

        Assert.Throws<InvalidLocalizationException>(() => new NumberToWordsConverter(localization));
    }

    [Fact]
    public void A_currency_supplied_to_the_constructor_is_validated_too()
    {
        // Used to render "TWO ZERO C": a null major became an empty placeholder and the
        // whitespace collapse hid the gap.
        var currency = new CurrencyModel { Major = null!, Minor = "C" };

        Assert.Throws<InvalidLocalizationException>(() => new NumberToWordsConverter("en-US", currency));
    }

    [Fact]
    public void Zero_decimal_places_and_a_decimal_placeholder_cannot_be_combined()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Settings.DecimalPlaces = 0;

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("decimalPlaces", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_misspelled_placeholder_is_rejected_instead_of_printed_literally()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Settings.NumberFormat = "{hole} POINT {decimal}";

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("{hole}", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_format_without_the_whole_placeholder_is_rejected()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Settings.NumberFormat = "POINT {decimal}";

        Assert.Throws<InvalidLocalizationException>(() => new NumberToWordsConverter(localization));
    }

    [Fact]
    public void The_number_format_cannot_reference_currency_placeholders()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Settings.NumberFormat = "{whole} {major}";

        Assert.Throws<InvalidLocalizationException>(() => new NumberToWordsConverter(localization));
    }

    [Fact]
    public void Naming_the_default_currency_twice_is_rejected()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Currency = new CurrencyModel { Major = "DOLLARS", Minor = "CENTS" };
        localization.Currencies = new Dictionary<string, CurrencyModel>
        {
            ["USD"] = new() { Major = "DOLLARS", Minor = "CENTS" },
        };
        localization.DefaultCurrency = "USD";

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("defaultCurrency", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_default_currency_that_names_nothing_is_rejected()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.DefaultCurrency = "XYZ";

        Assert.Throws<InvalidLocalizationException>(() => new NumberToWordsConverter(localization));
    }

    [Fact]
    public void Partial_exact_hundreds_fall_back_entry_by_entry()
    {
        // Spanish only overrides CIEN; requiring all nine entries forced eight copies of
        // numbers.hundreds that then had to be kept in sync by hand.
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Numbers!.ExactHundreds = ["", "A HUNDRED", "", "", "", "", "", "", "", ""];

        var converter = new NumberToWordsConverter(localization);

        Assert.Equal("A HUNDRED POINT ZERO", converter.ConvertWithoutCurrency(100m));
        Assert.Equal("ONE HUNDRED ONE POINT ZERO", converter.ConvertWithoutCurrency(101m));
        Assert.Equal("TWO HUNDRED POINT ZERO", converter.ConvertWithoutCurrency(200m));
    }

    [Fact]
    public void Meaningful_whitespace_in_a_locale_survives_rendering()
    {
        // A blanket whitespace collapse used to rewrite separators the locale chose.
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.SpecialNumbers!.CompoundSeparator = " -- ";

        Assert.Equal("TWENTY -- ONE POINT ZERO", 21m.ToWordsWithoutCurrency(localization));
    }

    [Fact]
    public void Options_do_not_silently_drop_a_currency_code()
    {
        var localization = TestLocalizations.English(new CurrencyModel { Major = "DOLLARS", Minor = "CENTS" });
        localization.Currencies = new Dictionary<string, CurrencyModel>
        {
            ["EUR"] = new() { Major = "EUROS", Minor = "CENTS" },
        };

        Assert.Equal(
            "ONE EUROS ZERO CENTS",
            1m.ToWords(new WordifyOptions { Localization = localization, CurrencyCode = "EUR" }));

        Assert.Throws<ArgumentException>(
            () => 1m.ToWords(new WordifyOptions { Localization = localization, CurrencyCode = "ZZZ" }));
    }
}
