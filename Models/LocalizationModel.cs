using System.Text.Json.Serialization;

namespace NumWordify.Models;

/// <summary>
/// Represents the complete localization ruleset for one language: its number
/// words, its formatting settings and the currencies it knows how to name.
/// </summary>
public class LocalizationModel
{
    /// <summary>
    /// The key the single-currency constructor files its currency under, for a model that
    /// names no ISO 4217 code of its own.
    /// </summary>
    public const string DefaultCurrencyKey = "DEFAULT";

    /// <summary>
    /// Gets or sets the key into <see cref="Currencies"/> naming the currency <c>Convert</c>
    /// uses when no override is supplied. A locale that only ever names one currency still
    /// lists it in <see cref="Currencies"/> and points at it from here.
    /// </summary>
    [JsonPropertyName("defaultCurrency")]
    public string? DefaultCurrency { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this localization is kept only for
    /// backwards compatibility. Deprecated localizations still resolve when named
    /// exactly, but are excluded from
    /// <see cref="Converters.NumberToWordsConverter.SupportedCultures"/> and never
    /// selected by language fallback.
    /// </summary>
    [JsonPropertyName("deprecated")]
    public bool Deprecated { get; set; }

    /// <summary>
    /// Gets or sets additional currencies this locale can name, keyed by ISO 4217 code
    /// (for example <c>"EUR"</c>). Lets a single locale file serve several currencies
    /// instead of duplicating the whole file per currency.
    /// </summary>
    [JsonPropertyName("currencies")]
    public Dictionary<string, CurrencyModel>? Currencies { get; set; }

    /// <summary>
    /// Gets or sets the number words for this locale.
    /// </summary>
    [JsonPropertyName("numbers")]
    public NumbersModel? Numbers { get; set; }

    /// <summary>
    /// Gets or sets the formatting settings for this locale.
    /// </summary>
    [JsonPropertyName("settings")]
    public SettingsModel Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets the irregular number words for this locale.
    /// </summary>
    [JsonPropertyName("specialNumbers")]
    public SpecialNumbersModel? SpecialNumbers { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationModel"/> class.
    /// </summary>
    public LocalizationModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationModel"/> class with a
    /// single currency, registered in <see cref="Currencies"/> under
    /// <see cref="DefaultCurrencyKey"/>.
    /// </summary>
    /// <param name="currency">The default currency.</param>
    /// <param name="numbers">The number words.</param>
    /// <param name="settings">Optional formatting settings.</param>
    public LocalizationModel(CurrencyModel currency, NumbersModel numbers, SettingsModel? settings = null)
    {
        Currencies = new Dictionary<string, CurrencyModel> { [DefaultCurrencyKey] = currency };
        DefaultCurrency = DefaultCurrencyKey;
        Numbers = numbers;
        Settings = settings ?? new SettingsModel();
    }
}
