using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Golden table for tr-TR. The ones digit used to be dropped whenever a tens digit was
/// present, so every entry from 21 upwards is a regression test.
/// </summary>
public class TurkishConversionTests
{
    private const string Culture = "tr-TR";
    private const string ZeroFraction = " NOKTA SIFIR";

    [Theory]
    [InlineData(0, "SIFIR")]
    [InlineData(1, "BİR")]
    [InlineData(9, "DOKUZ")]
    [InlineData(10, "ON")]
    [InlineData(11, "ON BİR")]
    [InlineData(19, "ON DOKUZ")]
    [InlineData(20, "YİRMİ")]
    [InlineData(21, "YİRMİ BİR")]
    [InlineData(42, "KIRK İKİ")]
    [InlineData(47, "KIRK YEDİ")]
    [InlineData(99, "DOKSAN DOKUZ")]
    [InlineData(100, "YÜZ")]
    [InlineData(101, "YÜZ BİR")]
    [InlineData(110, "YÜZ ON")]
    [InlineData(111, "YÜZ ON BİR")]
    [InlineData(199, "YÜZ DOKSAN DOKUZ")]
    [InlineData(200, "İKİ YÜZ")]
    [InlineData(250, "İKİ YÜZ ELLİ")]
    [InlineData(900, "DOKUZ YÜZ")]
    [InlineData(1000, "BİN")]
    [InlineData(1001, "BİN BİR")]
    [InlineData(1100, "BİN YÜZ")]
    [InlineData(1234, "BİN İKİ YÜZ OTUZ DÖRT")]
    [InlineData(10000, "ON BİN")]
    [InlineData(20000, "YİRMİ BİN")]
    [InlineData(21000, "YİRMİ BİR BİN")]
    [InlineData(100000, "YÜZ BİN")]
    [InlineData(101000, "YÜZ BİR BİN")]
    [InlineData(999999, "DOKUZ YÜZ DOKSAN DOKUZ BİN DOKUZ YÜZ DOKSAN DOKUZ")]
    [InlineData(1000000, "BİR MİLYON")]
    [InlineData(1000001, "BİR MİLYON BİR")]
    [InlineData(1001000, "BİR MİLYON BİN")]
    [InlineData(123456789, "YÜZ YİRMİ ÜÇ MİLYON DÖRT YÜZ ELLİ ALTI BİN YEDİ YÜZ SEKSEN DOKUZ")]
    [InlineData(1000000000, "BİR MİLYAR")]
    public void Whole_numbers_are_read_in_full(long value, string expected)
    {
        var actual = ((decimal)value).ToWordsWithoutCurrency(Culture);

        Assert.Equal(expected + ZeroFraction, actual);
    }

    [Theory]
    [InlineData("0", "SIFIR TL SIFIR Kr")]
    [InlineData("1", "BİR TL SIFIR Kr")]
    [InlineData("8.75", "SEKİZ TL YETMİŞ BEŞ Kr")]
    [InlineData("47.83", "KIRK YEDİ TL SEKSEN ÜÇ Kr")]
    [InlineData("1234.56", "BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr")]
    [InlineData("99999.99", "DOKSAN DOKUZ BİN DOKUZ YÜZ DOKSAN DOKUZ TL DOKSAN DOKUZ Kr")]
    [InlineData("-1234.56", "EKSİ BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr")]
    public void Currency_amounts_are_read_in_full(string value, string expected)
    {
        var actual = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture)
            .ToWords(Culture);

        Assert.Equal(expected, actual);
    }
}
