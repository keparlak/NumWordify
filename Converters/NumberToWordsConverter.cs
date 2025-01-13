using System.Text.Json;
using NumWordify.Models;

namespace NumWordify.Converters;

/// <summary>
/// Provides functionality to convert numbers to their word representation based on localization settings.
/// </summary>
public class NumberToWordsConverter
{
    private readonly LocalizationModel _localization;

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class using the specified culture.
    /// </summary>
    /// <param name="culture">The culture code to use for localization (e.g., "en-US").</param>
    /// <exception cref="FileNotFoundException">Thrown when the localization file for the specified culture is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization of localization data fails.</exception>
    public NumberToWordsConverter(string culture)
    {
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{culture}.json");
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"Localization file not found for culture: {culture}");

        var jsonContent = File.ReadAllText(jsonPath);
        _localization = JsonSerializer.Deserialize<LocalizationModel>(jsonContent)
            ?? throw new InvalidOperationException("Failed to deserialize localization data");

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
            .Replace("{major}", _localization.Currency.Major)
            .Replace("{decimal}", decimalWords)
            .Replace("{minor}", _localization.Currency.Minor)
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
        var tens = remainder / 10;
        var ones = remainder % 10;

        if (tens > 0)
            result.Add(_localization.Numbers.Tens[tens]);

        // "Bir Bin" kontrolü
        if (ones > 0 && !(_localization.Settings.SkipOneForThousand &&
            ones == 1 && number == 1 && currentScale == 1))
            result.Add(_localization.Numbers.Ones[ones]);

        return string.Join(" ", result);
    }

    private int currentScale = 0;
}