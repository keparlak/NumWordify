using System.Text.Json.Serialization;

namespace NumWordify.Models;

/// <summary>
/// Represents the complete localization ruleset for one language: its number
/// words, its formatting settings and the currencies it knows how to name.
/// </summary>
public class LocalizationModel
{
    /// <summary>
    /// Gets or sets the default currency used by <c>Convert</c> when no override is supplied.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="DefaultCurrency"/> when the currency is already listed in
    /// <see cref="Currencies"/>; naming it twice is how the two copies drift apart.
    /// </remarks>
    [JsonPropertyName("currency")]
    public CurrencyModel? Currency { get; set; }

    /// <summary>
    /// Gets or sets the key into <see cref="Currencies"/> naming the default currency, as
    /// an alternative to spelling it out again in <see cref="Currency"/>. Ignored when
    /// <see cref="Currency"/> is set.
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
    /// Initializes a new instance of the <see cref="LocalizationModel"/> class.
    /// </summary>
    /// <param name="currency">The default currency.</param>
    /// <param name="numbers">The number words.</param>
    /// <param name="settings">Optional formatting settings.</param>
    public LocalizationModel(CurrencyModel currency, NumbersModel numbers, SettingsModel? settings = null)
    {
        Currency = currency;
        Numbers = numbers;
        Settings = settings ?? new SettingsModel();
    }
}
