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
    public void Decimal_places_the_scale_words_cannot_reach_are_rejected()
    {
        // Used to construct fine and then throw "The number 1.123456 is too large for this
        // locale" — blaming a whole part of 1. The fraction is what needed the scale word.
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Numbers!.Scales = [""];
        localization.Settings.DecimalPlaces = 6;

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("decimalPlaces", exception.Message, StringComparison.Ordinal);
        Assert.Contains("numbers.scales", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    [InlineData(5, 2)]
    [InlineData(6, 2)]
    public void A_fraction_needs_one_scale_word_per_three_decimal_places(int decimalPlaces, int requiredScales)
    {
        // Exactly enough is accepted, and one short is not. Pinning both sides is the
        // point: a rule that only rejects is free to be too strict.
        Assert.Null(Record.Exception(() => new NumberToWordsConverter(WithScales(requiredScales))));

        if (requiredScales > 1)
            Assert.Throws<InvalidLocalizationException>(() => new NumberToWordsConverter(WithScales(requiredScales - 1)));

        LocalizationModel WithScales(int count)
        {
            var localization = TestLocalizations.EnglishWithoutCurrency();
            // Not a range expression: the suite also runs on net48, which has no System.Range.
            localization.Numbers!.Scales = new[] { "", "THOUSAND", "MILLION" }.Take(count).ToArray();
            localization.Settings.DecimalPlaces = decimalPlaces;

            // A locale with no decimal places must not reference the fraction either —
            // a separate rule, and this theory is not about that one.
            if (decimalPlaces == 0)
            {
                localization.Settings.NumberFormat = "{whole}";
                localization.Settings.CurrencyFormat = "{whole} {major}";
            }

            return localization;
        }
    }

    [Fact]
    public void A_scale_kind_outside_the_enum_is_rejected()
    {
        // An enum in C# is only an int, so a model built in code can carry a value the
        // JSON converter would never produce. The conversion loop asks "noun or not", so
        // an undefined kind silently reads as an adjective.
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Numbers!.ScaleKinds = [ScaleKind.Adjective, (ScaleKind)7, ScaleKind.Noun];

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("scaleKinds[1]", exception.Message, StringComparison.Ordinal);
        Assert.Contains("7", exception.Message, StringComparison.Ordinal);
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
    public void The_single_currency_constructor_registers_it_as_the_default()
    {
        // There is one place a default currency can be named. The convenience constructor
        // does not open a second one: it files the currency in the same map and points
        // defaultCurrency at it, so the model it builds is the model you would write.
        var currency = new CurrencyModel { Major = "DOLLARS", Minor = "CENTS" };
        var localization = new LocalizationModel(
            currency,
            TestLocalizations.English(),
            TestLocalizations.EnglishSettings())
        {
            SpecialNumbers = TestLocalizations.EnglishSpecials(),
        };

        Assert.Equal(LocalizationModel.DefaultCurrencyKey, localization.DefaultCurrency);
        Assert.Same(currency, localization.Currencies![LocalizationModel.DefaultCurrencyKey]);
        Assert.Equal("ONE DOLLARS ZERO CENTS", 1m.ToWords(localization));
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
        localization.Currencies!["EUR"] = new CurrencyModel { Major = "EUROS", Minor = "CENTS" };

        Assert.Equal(
            "ONE EUROS ZERO CENTS",
            1m.ToWords(new WordifyOptions { Localization = localization, CurrencyCode = "EUR" }));

        Assert.Throws<ArgumentException>(
            () => 1m.ToWords(new WordifyOptions { Localization = localization, CurrencyCode = "ZZZ" }));
    }
}
