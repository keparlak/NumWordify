using System.Text.Json.Serialization;

namespace NumWordify.Models;

/// <summary>
/// Represents localization settings for number and currency conversion.
/// </summary>
public class LocalizationModel
{
    /// <summary>
    /// Gets or sets the currency model for localization.
    /// </summary>
    [JsonPropertyName("currency")]
    public CurrencyModel Currency { get; set; } = null!;
    /// <summary>
    /// Gets or sets the numbers model for localization.
    /// </summary>
    [JsonPropertyName("numbers")]
    public NumbersModel Numbers { get; set; } = null!;
    /// <summary>
    /// Gets or sets the settings model for localization.
    /// </summary>
    [JsonPropertyName("settings")]
    public SettingsModel Settings { get; set; } = new();
    /// <summary>
    /// Gets or sets the special numbers model for localization.
    /// </summary>
    [JsonPropertyName("specialNumbers")]
    public SpecialNumbersModel? SpecialNumbers { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationModel"/> class with default settings.
    /// </summary>
    public LocalizationModel() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationModel"/> class.
    /// </summary>
    /// <param name="currency">The currency model for localization.</param>
    /// <param name="numbers">The numbers model for localization.</param>
    /// <param name="settings">Optional settings model for localization.</param>
    public LocalizationModel(CurrencyModel currency, NumbersModel numbers, SettingsModel? settings = null)
    {
        Currency = currency;
        Numbers = numbers;
        Settings = settings ?? new SettingsModel();
    }
}

/// <summary>
/// Represents a currency model with major and minor units.
/// </summary>
public class CurrencyModel
{
    /// <summary>
    /// Gets or sets the major currency unit.
    /// </summary>
    [JsonPropertyName("major")]
    public string Major { get; set; } = null!;

    /// <summary>
    /// Gets or sets the minor currency unit.
    /// </summary>
    [JsonPropertyName("minor")]
    public string Minor { get; set; } = null!;
}

/// <summary>
/// Represents a model for number words.
/// </summary>
public class NumbersModel
{
    /// <summary>
    /// Gets or sets the words for ones.
    /// </summary>
    [JsonPropertyName("ones")]
    public string[] Ones { get; set; } = null!;

    /// <summary>
    /// Gets or sets the words for tens.
    /// </summary>
    [JsonPropertyName("tens")]
    public string[] Tens { get; set; } = null!;

    /// <summary>
    /// Gets or sets the words for hundreds.
    /// </summary>
    [JsonPropertyName("hundreds")]
    public string[] Hundreds { get; set; } = null!;

    /// <summary>
    /// Gets or sets the words for scales.
    /// </summary>
    [JsonPropertyName("scales")]
    public string[] Scales { get; set; } = null!;
}

/// <summary>
/// Represents settings for number and currency formatting.
/// </summary>
public class SettingsModel
{
    /// <summary>
    /// Gets or sets a value indicating whether to skip the word "one" for thousands.
    /// </summary>
    [JsonPropertyName("skipOneForThousand")]
    public bool SkipOneForThousand { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to skip the word "one" for hundreds.
    /// </summary>
    [JsonPropertyName("skipOneForHundred")]
    public bool SkipOneForHundred { get; set; }

    /// <summary>
    /// Gets or sets the word used for negative numbers.
    /// </summary>
    [JsonPropertyName("negativeWord")]
    public string NegativeWord { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the word used for zero.
    /// </summary>
    [JsonPropertyName("zeroWord")]
    public string ZeroWord { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format string for currency.
    /// </summary>
    [JsonPropertyName("currencyFormat")]
    public string CurrencyFormat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format string for numbers.
    /// </summary>
    [JsonPropertyName("numberFormat")]
    public string NumberFormat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to use special number formatting for teens (11-19).
    /// </summary>
    [JsonPropertyName("useTeens")]
    public bool UseTeens { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use compound number formatting (e.g. twenty-one vs twenty one).
    /// </summary>
    [JsonPropertyName("useCompoundNumbers")]
    public bool UseCompoundNumbers { get; set; }
}

/// <summary>
/// Represents special number cases for different languages.
/// </summary>
public class SpecialNumbersModel
{
    /// <summary>
    /// Gets or sets special words for numbers from 11 to 19.
    /// If not provided, numbers will be constructed using regular rules.
    /// </summary>
    [JsonPropertyName("teens")]
    public string[]? Teens { get; set; }

    /// <summary>
    /// Gets or sets special words for specific numbers.
    /// Key is the number, value is the word representation.
    /// </summary>
    [JsonPropertyName("special")]
    public Dictionary<int, string>? Special { get; set; }

    /// <summary>
    /// Gets or sets the separator for compound numbers (e.g. "-" for "twenty-one" in English).
    /// </summary>
    [JsonPropertyName("compoundSeparator")]
    public string CompoundSeparator { get; set; } = " ";
}