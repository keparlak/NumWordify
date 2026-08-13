using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// European Portuguese. The rule this locale exists to exercise is the conjunction
/// <c>E</c>, quoted by Ciberdúvidas from Cunha and Cintra, <i>Nova Gramática do Português
/// Contemporâneo</i> (1984, p. 372):
/// <list type="number">
/// <item>it always goes between the hundreds, the tens and the units;</item>
/// <item>it is not used between the thousands and the hundreds, except when the number
/// ends in a hundred with two zeros — 1892 is MIL OITOCENTOS E NOVENTA E DOIS, but 1800
/// is MIL E OITOCENTOS.</item>
/// </list>
/// The second half also covers a remainder below one hundred: 1022 is MIL E VINTE E DOIS.
/// </summary>
public class PortugueseConversionTests
{
    [Theory]
    [InlineData(1, "UM")]
    [InlineData(15, "QUINZE")]
    [InlineData(16, "DEZASSEIS")]
    [InlineData(21, "VINTE E UM")]
    [InlineData(35, "TRINTA E CINCO")]
    [InlineData(100, "CEM")]
    [InlineData(101, "CENTO E UM")]
    [InlineData(115, "CENTO E QUINZE")]
    [InlineData(123, "CENTO E VINTE E TRÊS")]
    [InlineData(200, "DUZENTOS")]
    [InlineData(349, "TREZENTOS E QUARENTA E NOVE")]
    [InlineData(1000, "MIL")]
    [InlineData(2000, "DOIS MIL")]
    [InlineData(100_000, "CEM MIL")]
    [InlineData(1_000_000, "UM MILHÃO")]
    [InlineData(2_000_000, "DOIS MILHÕES")]
    public void Whole_numbers_are_read_in_full(int value, string expected)
    {
        Assert.Equal(expected + " VÍRGULA ZERO", ((decimal)value).ToWordsWithoutCurrency("pt-PT"));
    }

    [Theory]
    // The last group is a single term, so the conjunction goes in.
    [InlineData(1001, "MIL E UM")]
    [InlineData(1022, "MIL E VINTE E DOIS")]
    [InlineData(1100, "MIL E CEM")]
    [InlineData(1200, "MIL E DUZENTOS")]
    [InlineData(1800, "MIL E OITOCENTOS")]
    [InlineData(2300, "DOIS MIL E TREZENTOS")]
    [InlineData(1_000_500, "UM MILHÃO E QUINHENTOS")]
    [InlineData(2_300_000, "DOIS MILHÕES E TREZENTOS MIL")]
    // The last group is more than one term, so it does not.
    [InlineData(1234, "MIL DUZENTOS E TRINTA E QUATRO")]
    [InlineData(1892, "MIL OITOCENTOS E NOVENTA E DOIS")]
    [InlineData(1_234_567, "UM MILHÃO DUZENTOS E TRINTA E QUATRO MIL QUINHENTOS E SESSENTA E SETE")]
    public void The_conjunction_before_the_last_group_depends_on_that_group(int value, string expected)
    {
        Assert.Equal(expected + " VÍRGULA ZERO", ((decimal)value).ToWordsWithoutCurrency("pt-PT"));
    }

    [Fact]
    public void Currency_amounts_read_the_way_money_is_read()
    {
        Assert.Equal("UM EURO E ZERO CÊNTIMOS", 1m.ToWords("pt-PT"));
        Assert.Equal("CENTO E VINTE E TRÊS EUROS E QUARENTA E CINCO CÊNTIMOS", 123.45m.ToWords("pt-PT"));
        Assert.Equal(
            "MIL DUZENTOS E TRINTA E QUATRO EUROS E CINQUENTA E SEIS CÊNTIMOS",
            1234.56m.ToWords("pt-PT"));
    }

    [Fact]
    public void A_noun_scale_word_takes_de_before_the_currency_name()
    {
        // The same setting Spanish uses for UN MILLÓN DE EUROS, reused unchanged.
        Assert.Equal("UM MILHÃO DE EUROS E ZERO CÊNTIMOS", 1_000_000m.ToWords("pt-PT"));
        Assert.Equal("UM MILHÃO E QUINHENTOS EUROS E ZERO CÊNTIMOS", 1_000_500m.ToWords("pt-PT"));
    }

    [Fact]
    public void Negative_and_zero_read_correctly()
    {
        Assert.Equal("ZERO VÍRGULA ZERO", 0m.ToWordsWithoutCurrency("pt-PT"));
        Assert.Equal("MENOS MIL E OITOCENTOS VÍRGULA ZERO", (-1800m).ToWordsWithoutCurrency("pt-PT"));
    }

    [Fact]
    public void The_range_stops_below_a_thousand_million()
    {
        // pt-PT has no single word for 10^9 — it is MIL MILHÕES, a compound the scale
        // table cannot express without also producing "UM MIL MILHÕES" for 10^9 itself.
        // Documented in the README rather than approximated.
        Assert.Equal(
            "NOVECENTOS E NOVENTA E NOVE MILHÕES NOVECENTOS E NOVENTA E NOVE MIL NOVECENTOS E NOVENTA E NOVE VÍRGULA ZERO",
            999_999_999m.ToWordsWithoutCurrency("pt-PT"));

        Assert.Throws<NumWordify.Exceptions.NumberOutOfRangeException>(
            () => 1_000_000_000m.ToWordsWithoutCurrency("pt-PT"));
    }
}
