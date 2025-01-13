using System.Text.Json;
using System.Reflection;
using System.Text;
using System.Globalization;
using NumWordify.Models;

namespace NumWordify.Converters;

/// <summary>
/// Provides functionality to convert numbers to their word representation based on localization settings.
/// </summary>
public class NumberToWordsConverter
{
    private readonly LocalizationModel _localization;
    private CurrencyModel? _overriddenCurrency;

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class using the specified culture.
    /// </summary>
    /// <param name="culture">The culture code to use for localization (e.g., "en-US").</param>
    /// <exception cref="FileNotFoundException">Thrown when the localization file for the specified culture is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization of localization data fails.</exception>
    public NumberToWordsConverter(string culture)
    {
        var assembly = typeof(NumberToWordsConverter).Assembly;
        var resourceName = $"{culture}.json";

        // List all resources for debugging
        var resources = assembly.GetManifestResourceNames();

        if (!resources.Contains(resourceName))
        {
            var availableCultures = resources
                .Where(r => r.EndsWith(".json"))
                .Select(r => r.Replace(".json", ""))
                .ToList();

            throw new FileNotFoundException(
                $"Localization file not found for culture: {culture}. " +
                $"Available cultures: {string.Join(", ", availableCultures)}");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Failed to load resource stream for culture: {culture}");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var jsonContent = reader.ReadToEnd();

        try
        {
            _localization = JsonSerializer.Deserialize<LocalizationModel>(jsonContent)
                ?? throw new InvalidOperationException("Failed to deserialize localization data");

            // Validate individual properties
            if (_localization.Numbers == null)
                throw new InvalidOperationException("Numbers property is null");
            if (_localization.Currency == null)
                throw new InvalidOperationException("Currency property is null");
            if (_localization.Settings == null)
                throw new InvalidOperationException("Settings property is null");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse localization data for culture {culture}: {ex.Message}");
        }

        ValidateLocalization();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class using the specified localization model.
    /// </summary>
    /// <param name="localization">The localization model to use for number conversion.</param>
    /// <exception cref="InvalidOperationException">Thrown when the localization model is invalid.</exception>
    public NumberToWordsConverter(LocalizationModel localization)
    {
        _localization = localization;
        ValidateLocalization();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class using the specified CultureInfo.
    /// </summary>
    /// <param name="cultureInfo">The CultureInfo to use for localization.</param>
    /// <exception cref="FileNotFoundException">Thrown when the localization file for the specified culture is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization of localization data fails.</exception>
    public NumberToWordsConverter(CultureInfo cultureInfo) : this(cultureInfo.Name)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class using the specified culture and currency.
    /// </summary>
    /// <param name="culture">The culture code to use for localization (e.g., "tr-TR").</param>
    /// <param name="currency">The currency model to override the default currency.</param>
    public NumberToWordsConverter(string culture, CurrencyModel currency) : this(culture)
    {
        _overriddenCurrency = currency;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class using the specified CultureInfo and currency.
    /// </summary>
    /// <param name="cultureInfo">The CultureInfo to use for localization.</param>
    /// <param name="currency">The currency model to override the default currency.</param>
    public NumberToWordsConverter(CultureInfo cultureInfo, CurrencyModel currency) : this(cultureInfo)
    {
        _overriddenCurrency = currency;
    }

    /// <summary>
    /// Gets the current currency model being used for conversion.
    /// </summary>
    private CurrencyModel CurrentCurrency => _overriddenCurrency ?? _localization.Currency;

    private void ValidateLocalization()
    {
        if (_localization.Numbers.Ones.Length != 10)
            throw new InvalidOperationException("Ones array must contain exactly 10 elements");

        if (_localization.Numbers.Tens.Length != 10)
            throw new InvalidOperationException("Tens array must contain exactly 10 elements");

        if (_localization.Numbers.Hundreds.Length != 10)
            throw new InvalidOperationException("Hundreds array must contain exactly 10 elements");

        if (string.IsNullOrEmpty(_localization.Settings.CurrencyFormat))
            throw new InvalidOperationException("Currency format must be specified");

        if (string.IsNullOrEmpty(_localization.Settings.ZeroWord))
            throw new InvalidOperationException("Zero word must be specified");

        if (string.IsNullOrEmpty(_localization.Settings.NegativeWord))
            throw new InvalidOperationException("Negative word must be specified");

        if (string.IsNullOrEmpty(_localization.Settings.NumberFormat))
            throw new InvalidOperationException("Number format must be specified");
    }

    /// <summary>
    /// Converts a decimal number to its word representation including currency.
    /// </summary>
    /// <param name="number">The decimal number to convert.</param>
    /// <returns>A string representing the number in words with currency.</returns>
    public string Convert(decimal number)
    {
        var isNegative = number < 0;
        number = Math.Abs(number);

        var wholePart = Math.Floor(number);
        var decimalPart = Math.Round((number - wholePart) * 100);

        var wholeWords = ConvertWholeNumber(wholePart);
        var decimalWords = ConvertWholeNumber(decimalPart);

        if (string.IsNullOrEmpty(wholeWords))
            wholeWords = _localization.Settings.ZeroWord;

        if (string.IsNullOrEmpty(decimalWords))
            decimalWords = _localization.Settings.ZeroWord;

        var result = _localization.Settings.CurrencyFormat
            .Replace("{whole}", wholeWords)
            .Replace("{major}", CurrentCurrency.Major)
            .Replace("{decimal}", decimalWords)
            .Replace("{minor}", CurrentCurrency.Minor)
            .Trim();

        return isNegative ? $"{_localization.Settings.NegativeWord} {result}" : result;
    }

    /// <summary>
    /// Converts a decimal number to its word representation without including currency.
    /// </summary>
    /// <param name="number">The decimal number to convert.</param>
    /// <returns>A string representing the number in words without currency.</returns>
    public string ConvertWithoutCurrency(decimal number)
    {
        var isNegative = number < 0;
        number = Math.Abs(number);

        var wholePart = Math.Floor(number);
        var decimalPart = Math.Round((number - wholePart) * 100);

        var wholeWords = ConvertWholeNumber(wholePart);
        var decimalWords = ConvertWholeNumber(decimalPart);

        if (string.IsNullOrEmpty(wholeWords))
            wholeWords = _localization.Settings.ZeroWord;

        if (string.IsNullOrEmpty(decimalWords))
            decimalWords = _localization.Settings.ZeroWord;

        var result = _localization.Settings.NumberFormat
            .Replace("{whole}", wholeWords)
            .Replace("{decimal}", decimalWords)
            .Trim();

        return isNegative ? $"{_localization.Settings.NegativeWord} {result}" : result;
    }

    private string ConvertWholeNumber(decimal number)
    {
        if (number < 0)
            throw new ArgumentException("Number should be positive in ConvertWholeNumber");

        if (number == 0)
            return "";

        var groups = new List<string>();
        var currentNumber = number;
        currentScale = 0;

        while (currentNumber > 0)
        {
            var group = (int)(currentNumber % 1000);
            if (group > 0)
            {
                var groupText = ConvertGroup(group);
                if (currentScale > 0)
                    groupText += " " + _localization.Numbers.Scales[currentScale];
                groups.Insert(0, groupText.Trim());
            }

            currentNumber = Math.Floor(currentNumber / 1000);
            currentScale++;
        }

        return string.Join(" ", groups);
    }

    private string ConvertGroup(int number)
    {
        var result = new List<string>();

        // Yüzler basamağı
        var hundreds = number / 100;
        if (hundreds > 0)
        {
            if (_localization.Settings.SkipOneForHundred && hundreds == 1)
                result.Add("YÜZ");
            else
                result.Add(_localization.Numbers.Hundreds[hundreds]);
        }

        // Onlar ve birler basamağı
        var remainder = number % 100;

        // Özel sayılar için kontrol (11-19 arası)
        if (remainder >= 11 && remainder <= 19 &&
            _localization.Settings.UseTeens &&
            _localization.SpecialNumbers?.Teens != null)
        {
            result.Add(_localization.SpecialNumbers.Teens[remainder - 11]);
            return string.Join(" ", result);
        }

        // Özel sayılar sözlüğünde varsa direkt kullan
        if (_localization.SpecialNumbers?.Special?.ContainsKey(remainder) == true)
        {
            result.Add(_localization.SpecialNumbers.Special[remainder]);
            return string.Join(" ", result);
        }

        var tens = remainder / 10;
        var ones = remainder % 10;

        if (tens > 0)
        {
            var tensWord = _localization.Numbers.Tens[tens];

            // Birler basamağı varsa ve compound numbers kullanılıyorsa
            if (ones > 0 && _localization.Settings.UseCompoundNumbers)
            {
                var separator = _localization.SpecialNumbers?.CompoundSeparator ?? " ";
                tensWord += separator + _localization.Numbers.Ones[ones];
                result.Add(tensWord);
            }
            else
            {
                result.Add(tensWord);
            }
        }
        // Sadece birler basamağı varsa
        else if (ones > 0 && !(_localization.Settings.SkipOneForThousand &&
            ones == 1 && number == 1 && currentScale == 1))
        {
            result.Add(_localization.Numbers.Ones[ones]);
        }

        return string.Join(" ", result).Trim();
    }

    private int currentScale = 0;
}