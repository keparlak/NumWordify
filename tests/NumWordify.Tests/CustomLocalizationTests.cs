using NumWordify.Extensions;
using NumWordify.Models;
using Xunit;

namespace NumWordify.Tests;

public class CustomLocalizationTests
{
    [Fact]
    public void The_documented_japanese_example_produces_the_documented_output()
    {
        var japanese = new LocalizationModel
        {
            Currencies = new Dictionary<string, CurrencyModel>
            {
                ["JPY"] = new() { Major = "YEN", Minor = "SEN" },
            },
            DefaultCurrency = "JPY",
            Numbers = new NumbersModel
            {
                Ones = ["", "ICHI", "NI", "SAN", "YON", "GO", "ROKU", "NANA", "HACHI", "KYU"],
                Tens = ["", "JU", "NIJU", "SANJU", "YONJU", "GOJU", "ROKUJU", "NANAJU", "HACHIJU", "KYUJU"],
                Hundreds =
                [
                    "", "HYAKU", "NIHYAKU", "SANBYAKU", "YONHYAKU", "GOHYAKU",
                    "ROPPYAKU", "NANAHYAKU", "HAPPYAKU", "KYUHYAKU"
                ],
                Scales = ["", "SEN", "MAN", "OKU", "CHO", "KEI"],
            },
            Settings = new SettingsModel
            {
                SkipOneForThousand = true,
                NegativeWord = "MAINASU",
                ZeroWord = "ZERO",
                CurrencyFormat = "{whole} {major}",
                NumberFormat = "{whole} TEN {decimal}",
            },
        };

        Assert.Equal("SEN NIHYAKU SANJU YON YEN", 1234.56m.ToWords(japanese));
    }

    [Fact]
    public void A_currency_without_minor_units_can_drop_the_fraction_entirely()
    {
        var yen = new LocalizationModel
        {
            Currencies = new Dictionary<string, CurrencyModel> { ["JPY"] = new() { Major = "YEN" } },
            DefaultCurrency = "JPY",
            Numbers = TestLocalizations.English(),
            Settings = new SettingsModel
            {
                NegativeWord = "MINUS",
                ZeroWord = "ZERO",
                CurrencyFormat = "{whole} {major}",
                NumberFormat = "{whole}",
                DecimalPlaces = 0,
            },
            SpecialNumbers = TestLocalizations.EnglishSpecials(),
        };

        // With no minor unit the value rounds to a whole number, away from zero.
        Assert.Equal("ONE HUNDRED TWENTY-THREE YEN", 123.4m.ToWords(yen));
        Assert.Equal("ONE HUNDRED TWENTY-FOUR YEN", 123.5m.ToWords(yen));
    }

    [Fact]
    public void A_three_decimal_currency_reads_all_three_digits()
    {
        var dinar = new LocalizationModel
        {
            Currencies = new Dictionary<string, CurrencyModel>
            {
                ["TND"] = new() { Major = "DINARS", Minor = "MILLIMES" },
            },
            DefaultCurrency = "TND",
            Numbers = TestLocalizations.English(),
            Settings = new SettingsModel
            {
                NegativeWord = "MINUS",
                ZeroWord = "ZERO",
                CurrencyFormat = "{whole} {major} {decimal} {minor}",
                NumberFormat = "{whole} POINT {decimal}",
                DecimalPlaces = 3,
            },
            SpecialNumbers = TestLocalizations.EnglishSpecials(),
        };

        Assert.Equal("ONE DINARS ONE HUNDRED TWENTY-FIVE MILLIMES", 1.125m.ToWords(dinar));
    }

    [Fact]
    public void Digit_reading_mode_spells_the_fraction_out_digit_by_digit()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();
        localization.Settings.DecimalReading = DecimalReadingMode.Digits;

        Assert.Equal("ONE POINT FIVE", 1.5m.ToWordsWithoutCurrency(localization));
        Assert.Equal("ONE POINT TWO FIVE", 1.25m.ToWordsWithoutCurrency(localization));
        Assert.Equal("ONE POINT ZERO FIVE", 1.05m.ToWordsWithoutCurrency(localization));
        Assert.Equal("ONE POINT ZERO", 1m.ToWordsWithoutCurrency(localization));
    }

    [Fact]
    public void Fraction_reading_mode_stays_the_default()
    {
        var localization = TestLocalizations.EnglishWithoutCurrency();

        Assert.Equal("ONE POINT FIFTY", 1.5m.ToWordsWithoutCurrency(localization));
    }

    [Fact]
    public void Options_cover_combinations_the_overloads_do_not()
    {
        var options = new WordifyOptions { Culture = "tr-TR", CurrencyCode = "USD" };

        Assert.Equal("BİR DOLAR SIFIR SENT", 1m.ToWords(options));

        // Currency settings are ignored when no currency is being printed.
        Assert.Equal("BİR NOKTA SIFIR", 1m.ToWordsWithoutCurrency(options));
    }

    [Fact]
    public void Options_reject_a_null_instance()
    {
        Assert.Throws<ArgumentNullException>(() => 1m.ToWords((WordifyOptions)null!));
    }
}
