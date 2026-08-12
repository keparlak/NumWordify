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
        Currencies = new Dictionary<string, CurrencyModel> { ["USD"] = currency },
        DefaultCurrency = "USD",
        Numbers = English(),
        Settings = EnglishSettings(),
        SpecialNumbers = EnglishSpecials(),
    };

    /// <summary>
    /// A locale built to isolate one grammar setting at a time. The number words are
    /// deliberately unlike any real language — <c>exactHundreds</c> and
    /// <c>specialBeforeScale</c> differ visibly from their regular forms — so a flag that
    /// stops working shows up as a changed word rather than as an identical string.
    /// </summary>
    /// <param name="scaleKinds">The grammatical class of each scale word, or
    /// <see langword="null"/> to leave every scale an adjective.</param>
    public static LocalizationModel GrammarProbe(ScaleKind[]? scaleKinds = null) => new()
    {
        Currencies = new Dictionary<string, CurrencyModel>
        {
            ["USD"] = new() { Major = "DOLLARS", MajorSingular = "DOLLAR", Minor = "CENTS" },
        },
        DefaultCurrency = "USD",
        Numbers = new NumbersModel
        {
            Ones = ["", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE"],
            Tens = ["", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"],
            Hundreds =
            [
                "", "ONE HUNDRED", "TWO HUNDRED", "THREE HUNDRED", "FOUR HUNDRED",
                "FIVE HUNDRED", "SIX HUNDRED", "SEVEN HUNDRED", "EIGHT HUNDRED", "NINE HUNDRED"
            ],
            ExactHundreds = ["", "", "TWO HUNDREDS", "", "", "", "", "", "", ""],
            Scales = ["", "THOUSAND", "MILLION"],
            ScaleKinds = scaleKinds,
        },
        Settings = new SettingsModel
        {
            NegativeWord = "NEGATIVE",
            ZeroWord = "ZERO",
            CurrencyFormat = "{whole} {major} {decimal} {minor}",
            NumberFormat = "{whole} POINT {decimal}",
        },
        SpecialNumbers = new SpecialNumbersModel
        {
            SpecialBeforeScale = new Dictionary<int, string> { [1] = "A" },
            CompoundSeparator = "-",
        },
    };
}
