using System.Globalization;

namespace NumWordify.Models;

/// <summary>
/// Everything that shapes one conversion, gathered into a single object.
/// </summary>
/// <remarks>
/// The extension methods offer an overload per combination of culture, currency and
/// localization, which stops scaling once a third dimension appears. Prefer this type
/// when you need a combination the overloads do not cover, or when the settings come
/// from configuration rather than from a call site.
/// </remarks>
public sealed class WordifyOptions
{
    /// <summary>
    /// Gets or sets the culture code to convert with. Ignored when
    /// <see cref="Localization"/> is set. Defaults to <c>"en-US"</c>.
    /// </summary>
    public string Culture { get; set; } = "en-US";

    /// <summary>
    /// Gets or sets a localization to use instead of an embedded one. Takes precedence
    /// over <see cref="Culture"/>.
    /// </summary>
    public LocalizationModel? Localization { get; set; }

    /// <summary>
    /// Gets or sets a currency that replaces the locale default. Takes precedence over
    /// <see cref="CurrencyCode"/>.
    /// </summary>
    public CurrencyModel? Currency { get; set; }

    /// <summary>
    /// Gets or sets an ISO 4217 code selecting one of the currencies the locale — or
    /// <see cref="Localization"/>, when set — defines, for example <c>"EUR"</c>.
    /// Ignored when <see cref="Currency"/> is set.
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Sets <see cref="Culture"/> from a <see cref="CultureInfo"/>.
    /// </summary>
    /// <param name="cultureInfo">The culture to use.</param>
    /// <returns>The same options instance, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cultureInfo"/> is <see langword="null"/>.</exception>
    public WordifyOptions WithCulture(CultureInfo cultureInfo)
    {
        if (cultureInfo is null)
            throw new ArgumentNullException(nameof(cultureInfo));

        Culture = cultureInfo.Name;
        return this;
    }
}
