namespace NumWordify.Models;

/// <summary>
/// Represents localization settings for number and currency conversion.
/// </summary>
public class LocalizationModel
{
    /// <summary>
    /// Gets or sets the currency model for localization.
    /// </summary>
    public CurrencyModel Currency { get; set; } = null!;
    /// <summary>
    /// Gets or sets the numbers model for localization.
    /// </summary>
    public NumbersModel Numbers { get; set; } = null!;
    /// <summary>
    /// Gets or sets the settings model for localization.
    /// </summary>
    public SettingsModel Settings { get; set; } = new();

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
    public string Major { get; set; } = null!;

    /// <summary>
    /// Gets or sets the minor currency unit.
    /// </summary>
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
    public string[] Ones { get; set; } = null!;

    /// <summary>
    /// Gets or sets the words for tens.
    /// </summary>
    public string[] Tens { get; set; } = null!;

    /// <summary>
    /// Gets or sets the words for hundreds.
    /// </summary>
    public string[] Hundreds { get; set; } = null!;

    /// <summary>
    /// Gets or sets the words for scales.
    /// </summary>
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
    public bool SkipOneForThousand { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to skip the word "one" for hundreds.
    /// </summary>
    public bool SkipOneForHundred { get; set; }

    /// <summary>
    /// Gets or sets the word used for negative numbers.
    /// </summary>
    public string NegativeWord { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the word used for zero.
    /// </summary>
    public string ZeroWord { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format string for currency.
    /// </summary>
    public string CurrencyFormat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format string for numbers.
    /// </summary>
    public string NumberFormat { get; set; } = string.Empty;
}