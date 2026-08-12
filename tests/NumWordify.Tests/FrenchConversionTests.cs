using System.Globalization;
using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Golden table for fr-FR, covering the two things a flat tens/hundreds table cannot
/// express on its own: the vigesimal forms from 70 to 99, and the plural of <c>cent</c>
/// and <c>vingt</c>, which only appears when the word ends the number.
/// </summary>
public class FrenchConversionTests
{
    private const string Culture = "fr-FR";
    private const string ZeroFraction = " VIRGULE ZÉRO";

    [Theory]
    [InlineData(0, "ZÉRO")]
    [InlineData(1, "UN")]
    [InlineData(10, "DIX")]
    [InlineData(11, "ONZE")]
    [InlineData(16, "SEIZE")]
    [InlineData(17, "DIX-SEPT")]
    [InlineData(19, "DIX-NEUF")]
    [InlineData(20, "VINGT")]
    [InlineData(21, "VINGT ET UN")]
    [InlineData(22, "VINGT-DEUX")]
    [InlineData(31, "TRENTE ET UN")]
    [InlineData(61, "SOIXANTE ET UN")]
    [InlineData(70, "SOIXANTE-DIX")]
    [InlineData(71, "SOIXANTE ET ONZE")]
    [InlineData(72, "SOIXANTE-DOUZE")]
    [InlineData(77, "SOIXANTE-DIX-SEPT")]
    [InlineData(79, "SOIXANTE-DIX-NEUF")]
    [InlineData(80, "QUATRE-VINGTS")]
    [InlineData(81, "QUATRE-VINGT-UN")]
    [InlineData(90, "QUATRE-VINGT-DIX")]
    [InlineData(91, "QUATRE-VINGT-ONZE")]
    [InlineData(99, "QUATRE-VINGT-DIX-NEUF")]
    [InlineData(100, "CENT")]
    [InlineData(101, "CENT UN")]
    [InlineData(180, "CENT QUATRE-VINGTS")]
    [InlineData(200, "DEUX CENTS")]
    [InlineData(201, "DEUX CENT UN")]
    [InlineData(250, "DEUX CENT CINQUANTE")]
    [InlineData(1000, "MILLE")]
    [InlineData(2000, "DEUX MILLE")]
    [InlineData(180000, "CENT QUATRE-VINGT MILLE")]
    [InlineData(200000, "DEUX CENT MILLE")]
    [InlineData(1000000, "UN MILLION")]
    [InlineData(2000000, "DEUX MILLIONS")]
    [InlineData(1234, "MILLE DEUX CENT TRENTE-QUATRE")]
    public void Whole_numbers_are_read_in_full(long value, string expected)
    {
        var actual = ((decimal)value).ToWordsWithoutCurrency(Culture);

        Assert.Equal(expected + ZeroFraction, actual);
    }

    /// <summary>
    /// <c>cent</c> and <c>vingt</c> take a plural -s unless another numeral adjective
    /// follows. <c>mille</c> is a numeral adjective, so the -s drops; <c>million</c> and
    /// <c>milliard</c> are nouns, so it stays. The golden table above stops below 10^6,
    /// which is exactly where that distinction starts to matter.
    /// </summary>
    [Theory]
    [InlineData(200_000L, "DEUX CENT MILLE")]
    [InlineData(80_000L, "QUATRE-VINGT MILLE")]
    [InlineData(180_000L, "CENT QUATRE-VINGT MILLE")]
    [InlineData(200_000_000L, "DEUX CENTS MILLIONS")]
    [InlineData(80_000_000L, "QUATRE-VINGTS MILLIONS")]
    [InlineData(180_000_000L, "CENT QUATRE-VINGTS MILLIONS")]
    [InlineData(300_000_000_000L, "TROIS CENTS MILLIARDS")]
    [InlineData(201_000_000L, "DEUX CENT UN MILLIONS")]
    [InlineData(200_500_000L, "DEUX CENTS MILLIONS CINQ CENT MILLE")]
    public void Plurals_depend_on_whether_the_scale_word_is_a_noun(long value, string expected)
    {
        var actual = ((decimal)value).ToWordsWithoutCurrency(Culture);

        Assert.Equal(expected + ZeroFraction, actual);
    }

    [Theory]
    [InlineData(1_000_000L, "UN MILLION")]
    [InlineData(2_000_000L, "DEUX MILLIONS")]
    [InlineData(1_000_000_000L, "UN MILLIARD")]
    public void French_does_not_apocopate_before_a_noun_scale(long value, string expected)
    {
        Assert.Equal(expected + ZeroFraction, ((decimal)value).ToWordsWithoutCurrency(Culture));
    }

    [Theory]
    [InlineData("0", "ZÉRO EUROS ZÉRO CENTIMES")]
    [InlineData("1.01", "UN EURO UN CENTIME")]
    [InlineData("1234.56", "MILLE DEUX CENT TRENTE-QUATRE EUROS CINQUANTE-SIX CENTIMES")]
    [InlineData("-71.71", "MOINS SOIXANTE ET ONZE EUROS SOIXANTE ET ONZE CENTIMES")]
    public void Currency_amounts_are_read_in_full(string value, string expected)
    {
        var actual = decimal.Parse(value, CultureInfo.InvariantCulture).ToWords(Culture);

        Assert.Equal(expected, actual);
    }
}
