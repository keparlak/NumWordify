using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Russian, the first locale that needs more than two grammatical numbers and the first
/// whose numerals agree in gender with the word after them. Both rules interact, which is
/// what the theories below are for: ДВАДЦАТЬ ОДНА ТЫСЯЧА is the feminine form of "one"
/// chosen by the last digit, in front of a scale word chosen by the same digit.
/// </summary>
public class RussianConversionTests
{
    [Theory]
    [InlineData(1, "ОДИН")]
    [InlineData(2, "ДВА")]
    [InlineData(5, "ПЯТЬ")]
    [InlineData(11, "ОДИННАДЦАТЬ")]
    [InlineData(21, "ДВАДЦАТЬ ОДИН")]
    [InlineData(111, "СТО ОДИННАДЦАТЬ")]
    [InlineData(100_000, "СТО ТЫСЯЧ")]
    public void Whole_numbers_are_read_in_full(int value, string expected)
    {
        Assert.Equal(expected + " ЗАПЯТАЯ НОЛЬ", ((decimal)value).ToWordsWithoutCurrency("ru-RU"));
    }

    [Theory]
    // ТЫСЯЧА is feminine, so "one" and "two" take their feminine forms in front of it.
    [InlineData(1_000, "ОДНА ТЫСЯЧА")]
    [InlineData(2_000, "ДВЕ ТЫСЯЧИ")]
    [InlineData(21_000, "ДВАДЦАТЬ ОДНА ТЫСЯЧА")]
    [InlineData(22_000, "ДВАДЦАТЬ ДВЕ ТЫСЯЧИ")]
    // МИЛЛИОН is masculine, so the same digits keep the default forms.
    [InlineData(1_000_000, "ОДИН МИЛЛИОН")]
    [InlineData(2_000_000, "ДВА МИЛЛИОНА")]
    [InlineData(1_000_000_000, "ОДИН МИЛЛИАРД")]
    // Both in one number.
    [InlineData(2_021_000, "ДВА МИЛЛИОНА ДВАДЦАТЬ ОДНА ТЫСЯЧА")]
    public void A_numeral_agrees_with_the_gender_of_the_scale_word_after_it(int value, string expected)
    {
        Assert.Equal(expected + " ЗАПЯТАЯ НОЛЬ", ((decimal)value).ToWordsWithoutCurrency("ru-RU"));
    }

    [Theory]
    // one: last digit 1, except 11.
    [InlineData(1_000, "ОДНА ТЫСЯЧА")]
    [InlineData(11_000, "ОДИННАДЦАТЬ ТЫСЯЧ")]
    // few: last digit 2 to 4, except 12 to 14.
    [InlineData(3_000, "ТРИ ТЫСЯЧИ")]
    [InlineData(13_000, "ТРИНАДЦАТЬ ТЫСЯЧ")]
    [InlineData(23_000, "ДВАДЦАТЬ ТРИ ТЫСЯЧИ")]
    // many: everything else.
    [InlineData(5_000, "ПЯТЬ ТЫСЯЧ")]
    [InlineData(25_000, "ДВАДЦАТЬ ПЯТЬ ТЫСЯЧ")]
    [InlineData(100_000, "СТО ТЫСЯЧ")]
    public void The_scale_word_takes_the_form_the_count_selects(int value, string expected)
    {
        Assert.Equal(expected + " ЗАПЯТАЯ НОЛЬ", ((decimal)value).ToWordsWithoutCurrency("ru-RU"));
    }

    [Theory]
    [InlineData(1, "ОДИН РУБЛЬ НОЛЬ КОПЕЕК")]
    [InlineData(2, "ДВА РУБЛЯ НОЛЬ КОПЕЕК")]
    [InlineData(5, "ПЯТЬ РУБЛЕЙ НОЛЬ КОПЕЕК")]
    [InlineData(21, "ДВАДЦАТЬ ОДИН РУБЛЬ НОЛЬ КОПЕЕК")]
    [InlineData(111, "СТО ОДИННАДЦАТЬ РУБЛЕЙ НОЛЬ КОПЕЕК")]
    public void The_currency_name_takes_the_form_the_count_selects(int value, string expected)
    {
        Assert.Equal(expected, ((decimal)value).ToWords("ru-RU"));
    }

    [Fact]
    public void The_major_and_minor_units_agree_separately()
    {
        // The rouble is masculine and the kopeck is feminine, so one number can carry both
        // agreements at once. This is the case a single "singular or plural" switch cannot
        // express, and the reason the locale exists in this suite.
        Assert.Equal("ОДИН РУБЛЬ ОДНА КОПЕЙКА", 1.01m.ToWords("ru-RU"));
        Assert.Equal("ОДИН РУБЛЬ ДВЕ КОПЕЙКИ", 1.02m.ToWords("ru-RU"));
        Assert.Equal("ОДИН РУБЛЬ ПЯТЬ КОПЕЕК", 1.05m.ToWords("ru-RU"));
        Assert.Equal("ОДИН РУБЛЬ ДВАДЦАТЬ ОДНА КОПЕЙКА", 1.21m.ToWords("ru-RU"));
        Assert.Equal("ОДНА ТЫСЯЧА РУБЛЕЙ НОЛЬ КОПЕЕК", 1000m.ToWords("ru-RU"));
    }

    [Fact]
    public void An_indeclinable_currency_name_needs_no_forms()
    {
        // ЕВРО does not inflect. The locale simply omits the form map, which is the same
        // shape a two-form language uses for a word that happens not to change.
        Assert.Equal("ОДИН ЕВРО ОДИН ЦЕНТ", 1.01m.ToWords("ru-RU", "EUR"));
        Assert.Equal("ДВА ЕВРО ДВА ЦЕНТА", 2.02m.ToWords("ru-RU", "EUR"));
        Assert.Equal("ПЯТЬ ЕВРО ПЯТЬ ЦЕНТОВ", 5.05m.ToWords("ru-RU", "EUR"));
    }

    [Fact]
    public void Negative_and_zero_read_correctly()
    {
        Assert.Equal("НОЛЬ ЗАПЯТАЯ НОЛЬ", 0m.ToWordsWithoutCurrency("ru-RU"));
        Assert.Equal("МИНУС ДВЕ ТЫСЯЧИ ЗАПЯТАЯ НОЛЬ", (-2000m).ToWordsWithoutCurrency("ru-RU"));
    }
}
