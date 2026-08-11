using NumWordify.Models;

namespace NumWordify.Tests;

/// <summary>
/// Hand-built localizations for the cases the shipped resources deliberately do not cover.
/// </summary>
internal static class TestLocalizations
{
    public static NumbersModel English() => new()
    {
        Ones = ["", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE"],
        Tens = ["", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"],
        Hundreds =
        [
            "", "ONE HUNDRED", "TWO HUNDRED", "THREE HUNDRED", "FOUR HUNDRED",
            "FIVE HUNDRED", "SIX HUNDRED", "SEVEN HUNDRED", "EIGHT HUNDRED", "NINE HUNDRED"
        ],
        Scales = ["", "THOUSAND", "MILLION"],
    };

    public static SettingsModel EnglishSettings() => new()
    {
        NegativeWord = "NEGATIVE",
        ZeroWord = "ZERO",
        CurrencyFormat = "{whole} {major} {decimal} {minor}",
        NumberFormat = "{whole} POINT {decimal}",
    };

    public static SpecialNumbersModel EnglishSpecials() => new()
    {
        Teens =
        [
            "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN",
            "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN"
        ],
        CompoundSeparator = "-",
    };

    public static LocalizationModel EnglishWithoutCurrency() => new()
    {
        Numbers = English(),
        Settings = EnglishSettings(),
        SpecialNumbers = EnglishSpecials(),
    };

    public static LocalizationModel English(CurrencyModel currency) => new()
    {
        Currency = currency,
        Numbers = English(),
        Settings = EnglishSettings(),
        SpecialNumbers = EnglishSpecials(),
    };
}
