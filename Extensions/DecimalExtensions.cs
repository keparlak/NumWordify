using NumWordify.Converters;
using NumWordify.Models;
using System.Globalization;

namespace NumWordify.Extensions;

/// <summary>
/// Provides extension methods for converting decimal numbers to words.
/// </summary>
public static class DecimalExtensions
{
    /// <summary>
    /// Converts a decimal number to words using the specified culture.
    /// </summary>
    /// <param name="number">The decimal number to convert.</param>
    /// <param name="culture">The culture to use for conversion. Default is "en-US".</param>
    /// <returns>A string representing the number in words.</returns>
    public static string ToWords(this decimal number, string culture = "en-US")
    {
        var converter = new NumberToWordsConverter(culture);
        return converter.Convert(number);
    }

    /// <summary>
    /// Converts a decimal number to words without currency using the specified culture.
    /// </summary>
    /// <param name="number">The decimal number to convert.</param>
    /// <param name="culture">The culture to use for conversion. Default is "en-US".</param>
    /// <returns>A string representing the number in words without currency.</returns>
    public static string ToWordsWithoutCurrency(this decimal number, string culture = "en-US")
    {
        var converter = new NumberToWordsConverter(culture);
        return converter.ConvertWithoutCurrency(number);
    }

    /// <summary>
    /// Converts a decimal number to words using the specified localization model.
    /// </summary>
    /// <param name="number">The decimal number to convert.</param>
    /// <param name="localization">The localization model to use for conversion.</param>
    /// <returns>A string representing the number in words.</returns>
    public static string ToWords(this decimal number, LocalizationModel localization)
    {
        var converter = new NumberToWordsConverter(localization);
        return converter.Convert(number);
    }

    /// <summary>
    /// Converts a decimal number to words without currency using the specified localization model.
    /// </summary>
    /// <param name="number">The decimal number to convert.</param>
    /// <param name="localization">The localization model to use for conversion.</param>
    /// <returns>A string representing the number in words without currency.</returns>
    public static string ToWordsWithoutCurrency(this decimal number, LocalizationModel localization)
    {
        var converter = new NumberToWordsConverter(localization);
        return converter.ConvertWithoutCurrency(number);
    }

    /// <summary>
    /// Converts a decimal number to words using the specified CultureInfo.
    /// </summary>
    /// <param name="number">The decimal number to convert.</param>
    /// <param name="cultureInfo">The CultureInfo to use for conversion.</param>
    /// <returns>A string representing the number in words.</returns>
    public static string ToWords(this decimal number, CultureInfo cultureInfo)
    {
        var converter = new NumberToWordsConverter(cultureInfo);
        return converter.Convert(number);
    }

    /// <summary>
    /// Converts a decimal number to words without currency using the specified CultureInfo.
    /// </summary>
    /// <param name="number">The decimal number to convert.</param>
    /// <param name="cultureInfo">The CultureInfo to use for conversion.</param>
    /// <returns>A string representing the number in words without currency.</returns>
    public static string ToWordsWithoutCurrency(this decimal number, CultureInfo cultureInfo)
    {
        var converter = new NumberToWordsConverter(cultureInfo);
        return converter.ConvertWithoutCurrency(number);
    }
}