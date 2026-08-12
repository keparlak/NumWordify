using System.Globalization;
using System.Text.RegularExpressions;
using NumWordify.Exceptions;
using NumWordify.Models;

namespace NumWordify.Converters;

/// <summary>
/// Checks that a localization model is complete enough to convert every number the
/// public API accepts.
/// </summary>
/// <remarks>
/// Validation runs once, at construction time. The point is that a model which passes
/// here can never fail later with a <see cref="NullReferenceException"/> or an
/// <see cref="IndexOutOfRangeException"/> from deep inside the conversion loop, and can
/// never silently drop a word — the error is reported next to the mistake that caused it.
/// </remarks>
internal static class LocalizationValidator
{
    private const int DigitCount = 10;
    private const int TeenCount = 9;
    private const int MaxDecimalPlaces = 6;

    private static readonly Regex PlaceholderPattern = new(
        @"\{([^}]*)\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly string[] CurrencyPlaceholders = ["whole", "decimal", "major", "minor"];
    private static readonly string[] NumberPlaceholders = ["whole", "decimal"];

    public static void Validate(LocalizationModel? localization, string source)
    {
        if (localization is null)
            throw new ArgumentNullException(nameof(localization), $"The localization model ({source}) is null.");

        var numbers = localization.Numbers
            ?? throw Invalid(source, "'numbers' is missing.");

        ValidateDigitArray(numbers.Ones, "numbers.ones", source);
        ValidateDigitArray(numbers.Tens, "numbers.tens", source);
        ValidateDigitArray(numbers.Hundreds, "numbers.hundreds", source);

        // Empty entries here mean "fall back to numbers.hundreds", which is how Spanish
        // overrides only CIEN without restating the other eight.
        if (numbers.ExactHundreds is not null)
            ValidateDigitArray(numbers.ExactHundreds, "numbers.exactHundreds", source, allowEmptyEntries: true);

        ValidateScales(numbers, source);

        var settings = localization.Settings
            ?? throw Invalid(source, "'settings' is missing.");

        ValidateFormat(settings.CurrencyFormat, "settings.currencyFormat", CurrencyPlaceholders, settings, source);
        ValidateFormat(settings.NumberFormat, "settings.numberFormat", NumberPlaceholders, settings, source);

        RequireText(settings.ZeroWord, "settings.zeroWord", source);
        RequireText(settings.NegativeWord, "settings.negativeWord", source);

        if (settings.DecimalPlaces is < 0 or > MaxDecimalPlaces)
        {
            throw Invalid(source,
                $"'settings.decimalPlaces' must be between 0 and {MaxDecimalPlaces}, " +
                $"but is {settings.DecimalPlaces.ToString(CultureInfo.InvariantCulture)}.");
        }

        ValidateSpecialNumbers(localization, settings, source);
        ValidateCurrencies(localization, source);
    }

    private static void ValidateScales(NumbersModel numbers, string source)
    {
        if (numbers.Scales is null || numbers.Scales.Length == 0)
            throw Invalid(source, "'numbers.scales' must contain at least one entry.");

        for (var i = 0; i < numbers.Scales.Length; i++)
        {
            if (numbers.Scales[i] is null)
                throw Invalid(source, $"'numbers.scales[{i}]' is null. Use an empty string instead.");

            // Index 0 is the units group and carries no word; every other index must, or
            // the scale would be dropped from the output without any error.
            if (i > 0 && numbers.Scales[i].Length == 0)
            {
                throw Invalid(source,
                    $"'numbers.scales[{i}]' is empty, so numbers of that magnitude would silently " +
                    "lose their scale word. Shorten the array instead if the locale cannot express it.");
            }
        }

        ValidateParallelToScales(numbers.ScalesPlural, "numbers.scalesPlural", numbers.Scales.Length, source);

        if (numbers.ScaleKinds is not null && numbers.ScaleKinds.Length != numbers.Scales.Length)
        {
            throw Invalid(source,
                $"'numbers.scaleKinds' must have the same length as 'numbers.scales' " +
                $"({numbers.Scales.Length}), but has {numbers.ScaleKinds.Length}.");
        }
    }

    private static void ValidateParallelToScales(string[]? values, string path, int expectedLength, string source)
    {
        if (values is null)
            return;

        if (values.Length != expectedLength)
        {
            throw Invalid(source,
                $"'{path}' must have the same length as 'numbers.scales' ({expectedLength}), " +
                $"but has {values.Length}.");
        }

        for (var i = 0; i < values.Length; i++)
        {
            // Empty means "fall back to numbers.scales"; null would reach the conversion
            // loop and throw there instead of here.
            if (values[i] is null)
                throw Invalid(source, $"'{path}[{i}]' is null. Use an empty string to fall back instead.");
        }
    }

    private static void ValidateSpecialNumbers(LocalizationModel localization, SettingsModel settings, string source)
    {
        var special = localization.SpecialNumbers;
        var teensRequested = settings.UseTeens ?? special?.Teens is not null;

        if (teensRequested)
        {
            if (special?.Teens is null)
            {
                throw Invalid(source,
                    "'settings.useTeens' is true but 'specialNumbers.teens' is missing.");
            }

            if (special.Teens.Length != TeenCount)
            {
                throw Invalid(source,
                    $"'specialNumbers.teens' must contain exactly {TeenCount} entries (11 through 19), " +
                    $"but contains {special.Teens.Length}.");
            }

            for (var i = 0; i < special.Teens.Length; i++)
            {
                if (string.IsNullOrEmpty(special.Teens[i]))
                    throw Invalid(source, $"'specialNumbers.teens[{i}]' (the word for {i + 11}) is empty.");
            }
        }

        ValidateOverrides(special?.Special, "specialNumbers.special", source);
        ValidateOverrides(special?.SpecialBeforeScale, "specialNumbers.specialBeforeScale", source);

        if (special is not null && special.CompoundSeparator is null)
            throw Invalid(source, "'specialNumbers.compoundSeparator' is null. Use an empty string instead.");
    }

    private static void ValidateCurrencies(LocalizationModel localization, string source)
    {
        if (localization.Currencies is { } currencies)
        {
            foreach (var pair in currencies)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    throw Invalid(source, "'currencies' contains an empty currency code.");

                if (pair.Value is null)
                    throw Invalid(source, $"'currencies.{pair.Key}' is null.");

                ValidateCurrency(pair.Value, $"currencies.{pair.Key}", source);
            }
        }

        if (localization.DefaultCurrency is { } key)
        {
            if (localization.Currencies is null ||
                !localization.Currencies.Keys.Any(code => string.Equals(code, key, StringComparison.OrdinalIgnoreCase)))
            {
                var available = localization.Currencies is null || localization.Currencies.Count == 0
                    ? "none"
                    : string.Join(", ", localization.Currencies.Keys.OrderBy(code => code, StringComparer.Ordinal));

                throw Invalid(source,
                    $"'defaultCurrency' is '{key}' but 'currencies' defines: {available}.");
            }
        }
    }

    public static void ValidateCurrency(CurrencyModel currency, string path, string source)
    {
        if (currency.Major is null)
            throw Invalid(source, $"'{path}.major' is null. Use an empty string instead.");

        if (currency.Minor is null)
            throw Invalid(source, $"'{path}.minor' is null. Use an empty string instead.");
    }

    private static void ValidateOverrides(Dictionary<int, string>? overrides, string path, string source)
    {
        if (overrides is null)
            return;

        foreach (var pair in overrides)
        {
            if (pair.Key is < 1 or > 99)
            {
                throw Invalid(source,
                    $"'{path}' contains the key {pair.Key.ToString(CultureInfo.InvariantCulture)}. " +
                    "Keys override the last two digits of a group " +
                    "and must be between 1 and 99; key 0 is never consulted.");
            }

            if (string.IsNullOrEmpty(pair.Value))
                throw Invalid(source, $"'{path}[{pair.Key}]' is empty.");
        }
    }

    private static void ValidateDigitArray(
        string[]? values,
        string path,
        string source,
        bool allowEmptyEntries = false)
    {
        if (values is null)
            throw Invalid(source, $"'{path}' is missing.");

        if (values.Length != DigitCount)
        {
            throw Invalid(source,
                $"'{path}' must contain exactly {DigitCount} entries (index 0 unused), but contains {values.Length}.");
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] is null)
                throw Invalid(source, $"'{path}[{i}]' is null. Use an empty string instead.");
        }

        if (allowEmptyEntries)
            return;

        for (var i = 1; i < values.Length; i++)
        {
            if (values[i].Length == 0)
                throw Invalid(source, $"'{path}[{i}]' is empty, so the digit {i} would produce no word.");
        }
    }

    private static void ValidateFormat(
        string? format,
        string path,
        string[] allowed,
        SettingsModel settings,
        string source)
    {
        RequireText(format, path, source);

        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (Match match in PlaceholderPattern.Matches(format!))
        {
            var name = match.Groups[1].Value;

            if (Array.IndexOf(allowed, name) < 0)
            {
                throw Invalid(source,
                    $"'{path}' contains the unknown placeholder '{{{name}}}'. " +
                    $"Supported here: {string.Join(", ", allowed.Select(p => "{" + p + "}"))}.");
            }

            seen.Add(name);
        }

        if (!seen.Contains("whole"))
            throw Invalid(source, $"'{path}' must contain the '{{whole}}' placeholder.");

        // With no fractional digits the fraction is always zero, so a template that still
        // prints it would append "ZERO CENTS" to every single amount.
        if (settings.DecimalPlaces == 0 && (seen.Contains("decimal") || seen.Contains("minor")))
        {
            throw Invalid(source,
                $"'settings.decimalPlaces' is 0, so '{path}' must not use '{{decimal}}' or '{{minor}}' — " +
                "they would render the zero word on every conversion.");
        }
    }

    private static void RequireText(string? value, string path, string source)
    {
        if (string.IsNullOrEmpty(value))
            throw Invalid(source, $"'{path}' must be specified.");
    }

    private static InvalidLocalizationException Invalid(string source, string problem) =>
        new($"Invalid localization ({source}): {problem}");
}
