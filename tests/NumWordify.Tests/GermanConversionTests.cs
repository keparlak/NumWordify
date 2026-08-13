using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// German, the locale the README used to list as unsupported. Two rules make it awkward,
/// and both are spelling rules as much as grammar ones:
/// <list type="number">
/// <item>the last two digits are read backwards — 21 is EINUNDZWANZIG, "one and twenty";</item>
/// <item>everything below a million is written as a single word and everything above it
/// separately, which is the same split this library already calls adjective and noun.</item>
/// </list>
/// Duden, <i>Schreibung von Zahlen</i>.
/// </summary>
public class GermanConversionTests
{
    [Theory]
    [InlineData(1, "EINS")]
    [InlineData(11, "ELF")]
    [InlineData(12, "ZWÖLF")]
    [InlineData(20, "ZWANZIG")]
    [InlineData(21, "EINUNDZWANZIG")]
    [InlineData(22, "ZWEIUNDZWANZIG")]
    [InlineData(31, "EINUNDDREISSIG")]
    [InlineData(99, "NEUNUNDNEUNZIG")]
    public void The_last_two_digits_are_read_backwards(int value, string expected)
    {
        Assert.Equal(expected + " KOMMA NULL", ((decimal)value).ToWordsWithoutCurrency("de-DE"));
    }

    [Theory]
    [InlineData(100, "EINHUNDERT")]
    [InlineData(101, "EINHUNDERTEINS")]
    [InlineData(121, "EINHUNDERTEINUNDZWANZIG")]
    [InlineData(999, "NEUNHUNDERTNEUNUNDNEUNZIG")]
    [InlineData(1_000, "EINTAUSEND")]
    [InlineData(1_001, "EINTAUSENDEINS")]
    [InlineData(1_965, "EINTAUSENDNEUNHUNDERTFÜNFUNDSECHZIG")]
    [InlineData(21_000, "EINUNDZWANZIGTAUSEND")]
    [InlineData(120_000, "EINHUNDERTZWANZIGTAUSEND")]
    public void Everything_below_a_million_is_one_word(int value, string expected)
    {
        Assert.Equal(expected + " KOMMA NULL", ((decimal)value).ToWordsWithoutCurrency("de-DE"));
    }

    [Theory]
    [InlineData(1_000_000, "EINE MILLION")]
    [InlineData(2_000_000, "ZWEI MILLIONEN")]
    [InlineData(1_000_000_000, "EINE MILLIARDE")]
    // Duden's own example of the rule, verbatim.
    [InlineData(2_120_419, "ZWEI MILLIONEN EINHUNDERTZWANZIGTAUSENDVIERHUNDERTNEUNZEHN")]
    public void A_million_and_above_stands_apart(int value, string expected)
    {
        Assert.Equal(expected + " KOMMA NULL", ((decimal)value).ToWordsWithoutCurrency("de-DE"));
    }

    [Fact]
    public void One_takes_the_form_the_word_after_it_asks_for()
    {
        // EINS on its own, EIN in front of a masculine noun, EINE in front of a feminine
        // one — MILLION is feminine. The same three-way choice Russian needs, expressed
        // with the settings Russian brought.
        Assert.Equal("EINS KOMMA NULL", 1m.ToWordsWithoutCurrency("de-DE"));
        Assert.Equal("EINTAUSEND KOMMA NULL", 1_000m.ToWordsWithoutCurrency("de-DE"));
        Assert.Equal("EINE MILLION KOMMA NULL", 1_000_000m.ToWordsWithoutCurrency("de-DE"));
        Assert.Equal("EIN EURO NULL CENT", 1m.ToWords("de-DE"));
    }

    [Fact]
    public void Currency_amounts_read_the_way_money_is_read()
    {
        Assert.Equal("ZWEI EURO NULL CENT", 2m.ToWords("de-DE"));
        Assert.Equal("EINUNDZWANZIG EURO NULL CENT", 21m.ToWords("de-DE"));
        Assert.Equal("EIN EURO EIN CENT", 1.01m.ToWords("de-DE"));
        Assert.Equal("EINE MILLION EURO NULL CENT", 1_000_000m.ToWords("de-DE"));
    }

    [Fact]
    public void Negative_and_zero_read_correctly()
    {
        Assert.Equal("NULL KOMMA NULL", 0m.ToWordsWithoutCurrency("de-DE"));
        Assert.Equal("MINUS EINUNDZWANZIG KOMMA NULL", (-21m).ToWordsWithoutCurrency("de-DE"));
    }
}
