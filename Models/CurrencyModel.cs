using System.Text.Json.Serialization;

namespace NumWordify.Models;

/// <summary>
/// Names the major and minor unit of a currency.
/// </summary>
/// <remarks>
/// <see cref="MajorSingular"/> and <see cref="MinorSingular"/> are optional. When set,
/// they are used whenever the corresponding numeric part equals one, so that
/// <c>1.01</c> reads "ONE DOLLAR ONE CENT" instead of "ONE DOLLARS ONE CENTS".
/// Languages that do not inflect currency names (Turkish, for example) simply omit them.
/// </remarks>
public class CurrencyModel
{
    /// <summary>
    /// Gets or sets the major currency unit (for example <c>"DOLLARS"</c>).
    /// </summary>
    [JsonPropertyName("major")]
    public string Major { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minor currency unit (for example <c>"CENTS"</c>).
    /// </summary>
    [JsonPropertyName("minor")]
    public string Minor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the singular form of <see cref="Major"/>, used when the whole part is one.
    /// Falls back to <see cref="Major"/> when not set.
    /// </summary>
    [JsonPropertyName("majorSingular")]
    public string? MajorSingular { get; set; }

    /// <summary>
    /// Gets or sets the singular form of <see cref="Minor"/>, used when the fractional part is one.
    /// Falls back to <see cref="Minor"/> when not set.
    /// </summary>
    [JsonPropertyName("minorSingular")]
    public string? MinorSingular { get; set; }

    /// <summary>
    /// Gets or sets the major unit per grammatical number, for locales whose plural rule
    /// has more than two forms. Falls back to <see cref="MajorSingular"/> and
    /// <see cref="Major"/>.
    /// </summary>
    [JsonPropertyName("majorForms")]
    public Dictionary<PluralCategory, string>? MajorForms { get; set; }

    /// <summary>
    /// Gets or sets the minor unit per grammatical number. Falls back to
    /// <see cref="MinorSingular"/> and <see cref="Minor"/>.
    /// </summary>
    [JsonPropertyName("minorForms")]
    public Dictionary<PluralCategory, string>? MinorForms { get; set; }
}
