using System.Globalization;
using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Golden table for es-ES, covering the fused twenties, the <c>CIEN</c>/<c>CIENTO</c>
/// split and the apocope that turns <c>UNO</c> into <c>UN</c> in front of a scale word
/// or a currency name.
/// </summary>
public class SpanishConversionTests
{
    private const string Culture = "es-ES";
    private const string ZeroFraction = " COMA CERO";

    [Theory]
    [InlineData(0, "CERO")]
    [InlineData(1, "UNO")]
    [InlineData(10, "DIEZ")]
    [InlineData(11, "ONCE")]
    [InlineData(15, "QUINCE")]
    [InlineData(16, "DIECISÉIS")]
    [InlineData(19, "DIECINUEVE")]
    [InlineData(20, "VEINTE")]
    [InlineData(21, "VEINTIUNO")]
    [InlineData(22, "VEINTIDÓS")]
    [InlineData(29, "VEINTINUEVE")]
    [InlineData(31, "TREINTA Y UNO")]
    [InlineData(99, "NOVENTA Y NUEVE")]
    [InlineData(100, "CIEN")]
    [InlineData(101, "CIENTO UNO")]
    [InlineData(150, "CIENTO CINCUENTA")]
    [InlineData(200, "DOSCIENTOS")]
    [InlineData(250, "DOSCIENTOS CINCUENTA")]
    [InlineData(500, "QUINIENTOS")]
    [InlineData(1000, "MIL")]
    [InlineData(2000, "DOS MIL")]
    [InlineData(21000, "VEINTIÚN MIL")]
    [InlineData(100000, "CIEN MIL")]
    [InlineData(1000000, "UN MILLÓN")]
    [InlineData(2000000, "DOS MILLONES")]
    [InlineData(21000000, "VEINTIÚN MILLONES")]
    [InlineData(1234, "MIL DOSCIENTOS TREINTA Y CUATRO")]
    public void Whole_numbers_are_read_in_full(long value, string expected)
    {
        var actual = ((decimal)value).ToWordsWithoutCurrency(Culture);

        Assert.Equal(expected + ZeroFraction, actual);
    }

    [Theory]
    [InlineData("0", "CERO EUROS CERO CÉNTIMOS")]
    [InlineData("1.01", "UN EURO UN CÉNTIMO")]
    [InlineData("21", "VEINTIÚN EUROS CERO CÉNTIMOS")]
    [InlineData("1234.56", "MIL DOSCIENTOS TREINTA Y CUATRO EUROS CINCUENTA Y SEIS CÉNTIMOS")]
    public void Currency_amounts_are_read_in_full(string value, string expected)
    {
        var actual = decimal.Parse(value, CultureInfo.InvariantCulture).ToWords(Culture);

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// A noun scale word takes <c>de</c> before the currency name, but only when the
    /// number actually ends there: "un millón de euros" versus "un millón quinientos mil
    /// euros".
    /// </summary>
    [Theory]
    [InlineData("1000000", "UN MILLÓN DE EUROS CERO CÉNTIMOS")]
    [InlineData("2000000", "DOS MILLONES DE EUROS CERO CÉNTIMOS")]
    [InlineData("21000000", "VEINTIÚN MILLONES DE EUROS CERO CÉNTIMOS")]
    [InlineData("1500000", "UN MILLÓN QUINIENTOS MIL EUROS CERO CÉNTIMOS")]
    [InlineData("1000000.50", "UN MILLÓN DE EUROS CINCUENTA CÉNTIMOS")]
    public void A_noun_scale_word_takes_a_linking_word_before_the_currency(string value, string expected)
    {
        var actual = decimal.Parse(value, CultureInfo.InvariantCulture).ToWords(Culture);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void The_linking_word_is_not_added_when_no_currency_follows()
    {
        Assert.Equal("UN MILLÓN COMA CERO", 1_000_000m.ToWordsWithoutCurrency(Culture));
    }

    [Theory]
    [InlineData(100_000_000L, "CIEN MILLONES")]
    [InlineData(100_000L, "CIEN MIL")]
    [InlineData(200_000_000L, "DOSCIENTOS MILLONES")]
    public void The_exact_hundreds_form_survives_every_scale(long value, string expected)
    {
        Assert.Equal(expected + ZeroFraction, ((decimal)value).ToWordsWithoutCurrency(Culture));
    }

    [Fact]
    public void One_stays_uninflected_when_no_noun_follows()
    {
        // "UNO" on its own, "UN EURO" in front of a currency name.
        Assert.Equal("UNO COMA CERO", 1m.ToWordsWithoutCurrency(Culture));
        Assert.Equal("UN EURO CERO CÉNTIMOS", 1m.ToWords(Culture));
    }
}
