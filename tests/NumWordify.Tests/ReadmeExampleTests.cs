using System.Globalization;
using NumWordify.Converters;
using NumWordify.Exceptions;
using NumWordify.Extensions;
using NumWordify.Models;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Every output shown in README.md, asserted. Seven of the eight examples in the previous
/// README did not match what the code produced, so the documentation is executable now.
/// </summary>
public class ReadmeExampleTests
{
    [Fact]
    public void Basic_usage()
    {
        const decimal amount = 1234.56M;

        Assert.Equal("BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr", amount.ToWords("tr-TR"));
        Assert.Equal("BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr", amount.ToWords(new CultureInfo("tr-TR")));
        Assert.Equal("BİN İKİ YÜZ OTUZ DÖRT NOKTA ELLİ ALTI", amount.ToWordsWithoutCurrency("tr-TR"));

        Assert.Equal(
            "ELEVEN THOUSAND TWO HUNDRED THIRTY-FOUR DOLLARS FIFTY-SIX CENTS",
            11234.56M.ToWords("en-US"));

        Assert.Equal(
            "MILLE DEUX CENT TRENTE-QUATRE EUROS CINQUANTE-SIX CENTIMES",
            amount.ToWords("fr-FR"));

        Assert.Equal(
            "MIL DOSCIENTOS TREINTA Y CUATRO EUROS CINCUENTA Y SEIS CÉNTIMOS",
            amount.ToWords("es-ES"));
    }

    [Fact]
    public void Currency_examples()
    {
        Assert.Equal("BİN İKİ YÜZ OTUZ DÖRT EURO ELLİ ALTI SENT", 1234.56M.ToWords("tr-TR", "EUR"));

        var currency = new CurrencyModel { Major = "EURO", Minor = "SENT" };
        Assert.Equal("BİN İKİ YÜZ OTUZ DÖRT EURO ELLİ ALTI SENT", 1234.56M.ToWords("tr-TR", currency));

        Assert.Equal("ONE DOLLAR ONE CENT", 1.01M.ToWords("en-US"));
        Assert.Equal("TWO DOLLARS TWO CENTS", 2.02M.ToWords("en-US"));
    }

    [Fact]
    public void Negative_numbers_and_zero()
    {
        Assert.Equal("EKSİ BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr", (-1234.56M).ToWords("tr-TR"));
        Assert.Equal("SIFIR TL SIFIR Kr", 0M.ToWords("tr-TR"));
        Assert.Equal("ZERO DOLLARS ZERO CENTS", (-0.001M).ToWords("en-US"));
    }

    [Fact]
    public void Options_example()
    {
        var options = new WordifyOptions { Culture = "tr-TR", CurrencyCode = "USD" };

        Assert.Equal("BİR DOLAR SIFIR SENT", 1M.ToWords(options));
        Assert.Equal("BİR NOKTA SIFIR", 1M.ToWordsWithoutCurrency(options));
    }

    [Fact]
    public void Supported_cultures_listing()
    {
        Assert.Equal(
            ["en-US", "es-ES", "fr-FR", "pt-PT", "ru-RU", "tr-TR"],
            NumberToWordsConverter.SupportedCultures);
    }

    [Fact]
    public void Precision_and_rounding_examples()
    {
        Assert.Equal("ONE DOLLAR ONE CENT", 1.005M.ToWords("en-US"));
        Assert.Equal("ONE DOLLAR TWENTY-THREE CENTS", 1.234567M.ToWords("en-US"));
        Assert.Equal("ONE DOLLAR TWENTY-FOUR CENTS", 1.235M.ToWords("en-US"));
        Assert.Equal("ONE DOLLAR ZERO CENTS", 0.999M.ToWords("en-US"));

        var digits = TestLocalizations.EnglishWithoutCurrency();
        digits.Settings.DecimalReading = DecimalReadingMode.Digits;

        Assert.Equal("ONE POINT FIFTY", 1.5M.ToWordsWithoutCurrency(TestLocalizations.EnglishWithoutCurrency()));
        Assert.Equal("ONE POINT FIVE", 1.5M.ToWordsWithoutCurrency(digits));
        Assert.Equal("ONE POINT TWO FIVE", 1.25M.ToWordsWithoutCurrency(digits));
    }

    [Theory]
    [InlineData("en-US", true, true)]
    [InlineData("en-GB", true, false)]
    [InlineData("es-MX", true, false)]
    [InlineData("es", true, true)]
    [InlineData("de-DE", false, false)]
    public void Culture_guard_example(string machineCulture, bool supported, bool currencyApplies)
    {
        // Parameterised over the machine culture on purpose. The previous version of this
        // test ran the README recipe against CultureInfo.CurrentCulture and asserted only
        // that the output was non-empty, so it passed on every machine and CI runner the
        // project has ever used — while the recipe it documented threw for 178 of the 187
        // cultures that IsCultureSupported accepts.
        var machine = new CultureInfo(machineCulture);

        Assert.Equal(supported, NumberToWordsConverter.IsCultureSupported(machine, out var actual));
        Assert.Equal(currencyApplies, actual);

        var culture = NumberToWordsConverter.IsCultureSupported(machine, out var applies) && applies
            ? machine.Name
            : "en-US";

        Assert.False(string.IsNullOrWhiteSpace(1234.56M.ToWords(culture)));
    }

    [Fact]
    public void Culture_without_a_matching_region_examples()
    {
        Assert.Equal("ONE POINT ZERO", 1M.ToWordsWithoutCurrency("en-GB"));
        Assert.Equal("ONE POUND ZERO PENCE", 1M.ToWords("en-GB", "GBP"));
        Assert.Throws<AmbiguousCurrencyException>(() => 1M.ToWords("en-GB"));
    }

    [Fact]
    public void The_deprecated_euro_locale_matches_the_currency_code_form()
    {
        Assert.Equal(1234.56M.ToWords("tr-TR-EUR"), 1234.56M.ToWords("tr-TR", "EUR"));
    }

    [Fact]
    public void Custom_localization_example()
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
                    "", "HYAKU", "NIHYAKU", "SANBYAKU", "YONHYAKU",
                    "GOHYAKU", "ROPPYAKU", "NANAHYAKU", "HAPPYAKU", "KYUHYAKU"
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

        Assert.Equal("SEN NIHYAKU SANJU YON YEN", 1234.56M.ToWords(japanese));
    }

    [Fact]
    public void Adding_a_new_language_schema_example_is_valid()
    {
        // The JSON block in the README, expressed as the model it deserializes into.
        var localization = new LocalizationModel
        {
            DefaultCurrency = "USD",
            Currencies = new Dictionary<string, CurrencyModel>
            {
                ["USD"] = new()
                {
                    Major = "DOLLARS",
                    MajorSingular = "DOLLAR",
                    Minor = "CENTS",
                    MinorSingular = "CENT",
                },
                ["EUR"] = new()
                {
                    Major = "EUROS",
                    MajorSingular = "EURO",
                    Minor = "CENTS",
                    MinorSingular = "CENT",
                },
            },
            Numbers = new NumbersModel
            {
                Ones = ["", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE"],
                Tens = ["", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"],
                Hundreds =
                [
                    "", "ONE HUNDRED", "TWO HUNDRED", "THREE HUNDRED", "FOUR HUNDRED",
                    "FIVE HUNDRED", "SIX HUNDRED", "SEVEN HUNDRED", "EIGHT HUNDRED", "NINE HUNDRED"
                ],
                Scales = ["", "THOUSAND", "MILLION", "BILLION", "TRILLION", "QUADRILLION"],
            },
            Settings = new SettingsModel
            {
                SkipOneForThousand = false,
                UseTeens = true,
                NegativeWord = "NEGATIVE",
                ZeroWord = "ZERO",
                CurrencyFormat = "{whole} {major} {decimal} {minor}",
                NumberFormat = "{whole} POINT {decimal}",
                DecimalPlaces = 2,
            },
            SpecialNumbers = TestLocalizations.EnglishSpecials(),
        };

        Assert.Equal("ELEVEN DOLLARS ZERO CENTS", 11M.ToWords(localization));
    }
}
