using System.Globalization;
using NumWordify.Converters;
using NumWordify.Models;

namespace NumWordify.Extensions;

/// <summary>
/// Converts <see cref="decimal"/> values to words.
/// </summary>
/// <remarks>
/// Each call builds a converter, but the parsed localization behind it is cached for the
/// process, so this is cheap enough for per-row use. Constructing one
/// <see cref="NumberToWordsConverter"/> and reusing it is still slightly faster and is
/// safe from any number of threads.
/// </remarks>
public static class DecimalExtensions
{
    /// <summary>
    /// Converts a number to words with currency, using the given culture.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="culture">The culture code. Defaults to <c>"en-US"</c>.</param>
    /// <returns>The number in words, with currency.</returns>
    public static string ToWords(this decimal number, string culture = "en-US") =>
        new NumberToWordsConverter(culture).Convert(number);

    /// <summary>
    /// Converts a number to words without currency, using the given culture.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="culture">The culture code. Defaults to <c>"en-US"</c>.</param>
    /// <returns>The number in words, without currency.</returns>
    public static string ToWordsWithoutCurrency(this decimal number, string culture = "en-US") =>
        new NumberToWordsConverter(culture).ConvertWithoutCurrency(number);

    /// <summary>
    /// Converts a number to words with currency, using the given culture.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="cultureInfo">The culture to use.</param>
    /// <returns>The number in words, with currency.</returns>
    public static string ToWords(this decimal number, CultureInfo cultureInfo) =>
        new NumberToWordsConverter(cultureInfo).Convert(number);

    /// <summary>
    /// Converts a number to words without currency, using the given culture.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="cultureInfo">The culture to use.</param>
    /// <returns>The number in words, without currency.</returns>
    public static string ToWordsWithoutCurrency(this decimal number, CultureInfo cultureInfo) =>
        new NumberToWordsConverter(cultureInfo).ConvertWithoutCurrency(number);

    /// <summary>
    /// Converts a number to words with currency, overriding the locale currency.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="currency">The currency that replaces the locale default.</param>
    /// <returns>The number in words, with currency.</returns>
    public static string ToWords(this decimal number, string culture, CurrencyModel currency) =>
        new NumberToWordsConverter(culture, currency).Convert(number);

    /// <summary>
    /// Converts a number to words with currency, overriding the locale currency.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="cultureInfo">The culture to use.</param>
    /// <param name="currency">The currency that replaces the locale default.</param>
    /// <returns>The number in words, with currency.</returns>
    public static string ToWords(this decimal number, CultureInfo cultureInfo, CurrencyModel currency) =>
        new NumberToWordsConverter(cultureInfo, currency).Convert(number);

    /// <summary>
    /// Converts a number to words with currency, selecting one of the currencies the
    /// locale defines.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="currencyCode">An ISO 4217 code such as <c>"EUR"</c>.</param>
    /// <returns>The number in words, with currency.</returns>
    public static string ToWords(this decimal number, string culture, string currencyCode) =>
        new NumberToWordsConverter(culture, currencyCode).Convert(number);

    /// <summary>
    /// Converts a number to words with currency, selecting one of the currencies the
    /// locale defines.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="cultureInfo">The culture to use.</param>
    /// <param name="currencyCode">An ISO 4217 code such as <c>"EUR"</c>.</param>
    /// <returns>The number in words, with currency.</returns>
    public static string ToWords(this decimal number, CultureInfo cultureInfo, string currencyCode) =>
        new NumberToWordsConverter(cultureInfo, currencyCode).Convert(number);

    /// <summary>
    /// Converts a number to words with currency, using a localization you supply.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="localization">The localization ruleset.</param>
    /// <returns>The number in words, with currency.</returns>
    public static string ToWords(this decimal number, LocalizationModel localization) =>
        new NumberToWordsConverter(localization).Convert(number);

    /// <summary>
    /// Converts a number to words with currency, using a localization you supply and
    /// overriding its currency.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="localization">The localization ruleset.</param>
    /// <param name="currency">The currency that replaces the model default.</param>
    /// <returns>The number in words, with currency.</returns>
    public static string ToWords(this decimal number, LocalizationModel localization, CurrencyModel currency) =>
        new NumberToWordsConverter(localization, currency).Convert(number);

    /// <summary>
    /// Converts a number to words without currency, using a localization you supply.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="localization">The localization ruleset.</param>
    /// <returns>The number in words, without currency.</returns>
    public static string ToWordsWithoutCurrency(this decimal number, LocalizationModel localization) =>
        new NumberToWordsConverter(localization).ConvertWithoutCurrency(number);

    /// <summary>
    /// Converts a number to words with currency, using the given options.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="options">Culture, localization and currency settings.</param>
    /// <returns>The number in words, with currency.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static string ToWords(this decimal number, WordifyOptions options) =>
        new NumberToWordsConverter(options).Convert(number);

    /// <summary>
    /// Converts a number to words without currency, using the given options.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <param name="options">Culture and localization settings. Currency settings are ignored.</param>
    /// <returns>The number in words, without currency.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static string ToWordsWithoutCurrency(this decimal number, WordifyOptions options) =>
        new NumberToWordsConverter(options).ConvertWithoutCurrency(number);
}
