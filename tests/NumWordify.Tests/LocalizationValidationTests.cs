using NumWordify.Converters;
using NumWordify.Exceptions;
using NumWordify.Extensions;
using NumWordify.Models;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// A model that passes validation must not be able to fail later with a
/// <see cref="NullReferenceException"/> or an <see cref="IndexOutOfRangeException"/>
/// from inside the conversion loop. These tests pin the error to the mistake.
/// </summary>
public class LocalizationValidationTests
{
    [Fact]
    public void A_null_model_is_an_argument_error_not_a_null_reference()
    {
        Assert.Throws<ArgumentNullException>(() => new NumberToWordsConverter((LocalizationModel)null!));
        Assert.Throws<ArgumentNullException>(() => 1m.ToWords((LocalizationModel)null!));
    }

    [Fact]
    public void An_empty_model_says_what_is_missing()
    {
        var exception = Assert.Throws<InvalidLocalizationException>(() => 1m.ToWords(new LocalizationModel()));

        Assert.Contains("numbers", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_digit_array_of_the_wrong_length_is_rejected()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Numbers!.Ones = ["", "ONE", "TWO"];

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("numbers.ones", exception.Message, StringComparison.Ordinal);
        Assert.Contains("10", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_short_teens_array_is_rejected_instead_of_indexing_out_of_range()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.SpecialNumbers!.Teens = ["ELEVEN"];

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("teens", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Teens_are_required_once_they_are_switched_on()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.SpecialNumbers!.Teens = null;
        localization.Settings.UseTeens = true;

        Assert.Throws<InvalidLocalizationException>(() => new NumberToWordsConverter(localization));
    }

    [Fact]
    public void Teens_switch_themselves_on_when_supplied_and_off_when_not()
    {
        var withTeens = TestLocalizations.EnglishWithoutCurrency();
        var withoutTeens = TestLocalizations.EnglishWithoutCurrency();
        withoutTeens.SpecialNumbers!.Teens = null;

        Assert.Equal("ELEVEN POINT ZERO", new NumberToWordsConverter(withTeens).ConvertWithoutCurrency(11m));
        Assert.Equal("TEN-ONE POINT ZERO", new NumberToWordsConverter(withoutTeens).ConvertWithoutCurrency(11m));
    }

    [Fact]
    public void An_override_keyed_on_zero_is_rejected()
    {
        // Key 0 is unreachable by construction. Accepting it silently is how en-US ended
        // up appending "ZERO" to every round hundred.
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.SpecialNumbers!.Special = new Dictionary<int, string> { [0] = "ZERO" };

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("between 1 and 99", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_format_string_is_rejected()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Settings.NumberFormat = string.Empty;

        var exception = Assert.Throws<InvalidLocalizationException>(
            () => new NumberToWordsConverter(localization));

        Assert.Contains("numberFormat", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void An_out_of_range_decimal_precision_is_rejected()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Settings.DecimalPlaces = 9;

        Assert.Throws<InvalidLocalizationException>(() => new NumberToWordsConverter(localization));
    }

    [Fact]
    public void Every_library_exception_shares_one_base_type()
    {
        Assert.IsAssignableFrom<NumWordifyException>(
            Assert.Throws<LocalizationNotFoundException>(() => 1m.ToWords("ja-JP")));

        Assert.IsAssignableFrom<NumWordifyException>(
            Assert.Throws<InvalidLocalizationException>(() => 1m.ToWords(new LocalizationModel())));
    }
}
