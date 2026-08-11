using System.Globalization;
using System.Text.RegularExpressions;
using NumWordify.Exceptions;
using NumWordify.Models;

namespace NumWordify.Converters;

/// <summary>
/// Converts numbers to their word representation using a localization ruleset.
/// </summary>
/// <remarks>
/// <para>
/// Instances are immutable and stateless: conversion keeps no per-call state on the
/// object, so a single converter can be shared freely between threads and registered as
/// a singleton. Localizations loaded from a culture name are parsed once and cached for
/// the process, which makes constructing a converter cheap enough to do per call.
/// </para>
/// <para>
/// The one caveat applies to <see cref="LocalizationModel"/> instances you build
/// yourself: the converter keeps a reference rather than a copy, so the model must not
/// be mutated after it has been handed over.
/// </para>
/// </remarks>
public sealed class NumberToWordsConverter : INumberToWordsConverter
{
    private const string CustomLocalizationSource = "caller-supplied LocalizationModel";

    // The captured leading space lets an empty placeholder take its own separator with
    // it, instead of leaving a double space behind for a blanket whitespace collapse to
    // clean up — which would also eat separators a locale meant to keep.
    private static readonly Regex PlaceholderPattern = new(
        @"( ?)\{(whole|decimal|major|minor)\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly LocalizationModel _localization;
    private readonly CurrencyModel? _overriddenCurrency;
    private readonly string _requestedCulture;
    private readonly string _resolvedCulture;
    private readonly bool _localeCurrencyApplies;

    /// <summary>
    /// Gets the culture names shipped with the library, excluding deprecated ones.
    /// </summary>
    public static IReadOnlyList<string> SupportedCultures => LocalizationLoader.SupportedCultures;

    /// <summary>
    /// Determines whether the library can convert numbers for the given culture.
    /// </summary>
    /// <param name="culture">The culture code to test, for example <c>"en-US"</c>.</param>
    /// <returns><see langword="true"/> when a localization resolves for the culture.</returns>
    /// <remarks>
    /// Useful before handing over <see cref="CultureInfo.CurrentCulture"/>, which is not
    /// guaranteed to be one of the supported cultures. A culture can resolve through
    /// language fallback — <c>es-MX</c> resolves to the Spanish number words — in which
    /// case the locale's default currency does not apply and must be supplied explicitly.
    /// </remarks>
    public static bool IsCultureSupported(string? culture) => LocalizationLoader.IsSupported(culture);

    /// <summary>
    /// Determines whether the library can convert numbers for the given culture.
    /// </summary>
    /// <param name="cultureInfo">The culture to test.</param>
    /// <returns><see langword="true"/> when a localization resolves for the culture.</returns>
    public static bool IsCultureSupported(CultureInfo? cultureInfo) =>
        cultureInfo is not null && LocalizationLoader.IsSupported(cultureInfo.Name);

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class for a culture.
    /// </summary>
    /// <param name="culture">The culture code, for example <c>"en-US"</c>. Matching is
    /// case-insensitive and falls back to another variant of the same language.</param>
    /// <exception cref="ArgumentNullException"><paramref name="culture"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="culture"/> is empty or whitespace.</exception>
    /// <exception cref="LocalizationNotFoundException">No localization matches the culture.</exception>
    /// <exception cref="InvalidLocalizationException">The localization resource is invalid.</exception>
    public NumberToWordsConverter(string culture)
    {
        var resolved = LocalizationLoader.Resolve(culture);

        _localization = resolved.Model;
        _requestedCulture = culture;
        _resolvedCulture = resolved.CultureName;
        _localeCurrencyApplies = resolved.RegionMatches;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class for a culture.
    /// </summary>
    /// <param name="cultureInfo">The culture to use.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cultureInfo"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The culture has an empty name, as
    /// <see cref="CultureInfo.InvariantCulture"/> does.</exception>
    /// <exception cref="LocalizationNotFoundException">No localization matches the culture.</exception>
    public NumberToWordsConverter(CultureInfo cultureInfo)
        : this((cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo))).Name)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class for a
    /// culture, overriding the currency.
    /// </summary>
    /// <param name="culture">The culture code to use.</param>
    /// <param name="currency">The currency that replaces the locale default.</param>
    /// <exception cref="ArgumentNullException"><paramref name="currency"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidLocalizationException">The currency is incomplete.</exception>
    public NumberToWordsConverter(string culture, CurrencyModel currency)
        : this(culture)
    {
        _overriddenCurrency = ValidatedOverride(currency);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class for a
    /// culture, overriding the currency.
    /// </summary>
    /// <param name="cultureInfo">The culture to use.</param>
    /// <param name="currency">The currency that replaces the locale default.</param>
    /// <exception cref="ArgumentNullException"><paramref name="currency"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidLocalizationException">The currency is incomplete.</exception>
    public NumberToWordsConverter(CultureInfo cultureInfo, CurrencyModel currency)
        : this(cultureInfo)
    {
        _overriddenCurrency = ValidatedOverride(currency);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class for a
    /// culture, selecting one of the currencies the locale defines.
    /// </summary>
    /// <param name="culture">The culture code to use.</param>
    /// <param name="currencyCode">An ISO 4217 code listed in the locale's <c>currencies</c> map,
    /// for example <c>"EUR"</c>.</param>
    /// <exception cref="ArgumentException">The locale does not define the currency code.</exception>
    public NumberToWordsConverter(string culture, string currencyCode)
        : this(culture)
    {
        _overriddenCurrency = ResolveCurrencyCode(_localization, _resolvedCulture, currencyCode);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class for a
    /// culture, selecting one of the currencies the locale defines.
    /// </summary>
    /// <param name="cultureInfo">The culture to use.</param>
    /// <param name="currencyCode">An ISO 4217 code listed in the locale's <c>currencies</c> map.</param>
    /// <exception cref="ArgumentException">The locale does not define the currency code.</exception>
    public NumberToWordsConverter(CultureInfo cultureInfo, string currencyCode)
        : this(cultureInfo)
    {
        _overriddenCurrency = ResolveCurrencyCode(_localization, _resolvedCulture, currencyCode);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class from a
    /// localization you supply.
    /// </summary>
    /// <param name="localization">The localization ruleset. Must not be mutated afterwards.</param>
    /// <exception cref="ArgumentNullException"><paramref name="localization"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidLocalizationException">The localization is incomplete or inconsistent.</exception>
    public NumberToWordsConverter(LocalizationModel localization)
    {
        if (localization is null)
            throw new ArgumentNullException(nameof(localization));

        LocalizationValidator.Validate(localization, CustomLocalizationSource);

        _localization = localization;
        _requestedCulture = CustomLocalizationSource;
        _resolvedCulture = CustomLocalizationSource;
        _localeCurrencyApplies = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class from a
    /// localization you supply, overriding the currency.
    /// </summary>
    /// <param name="localization">The localization ruleset. Must not be mutated afterwards.</param>
    /// <param name="currency">The currency that replaces the model default.</param>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    /// <exception cref="InvalidLocalizationException">The localization or currency is incomplete.</exception>
    public NumberToWordsConverter(LocalizationModel localization, CurrencyModel currency)
        : this(localization)
    {
        _overriddenCurrency = ValidatedOverride(currency);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberToWordsConverter"/> class from a
    /// localization you supply, selecting one of the currencies it defines.
    /// </summary>
    /// <param name="localization">The localization ruleset. Must not be mutated afterwards.</param>
    /// <param name="currencyCode">A key into the model's <see cref="LocalizationModel.Currencies"/>.</param>
    /// <exception cref="ArgumentException">The model does not define the currency code.</exception>
    public NumberToWordsConverter(LocalizationModel localization, string currencyCode)
        : this(localization)
    {
        _overriddenCurrency = ResolveCurrencyCode(_localization, CustomLocalizationSource, currencyCode);
    }

    /// <summary>
    /// Converts a number to words including its currency units.
    /// </summary>
    /// <param name="number">The number to convert. The fractional part is rounded to
    /// <see cref="SettingsModel.DecimalPlaces"/> digits, away from zero.</param>
    /// <returns>The number in words, with currency.</returns>
    /// <exception cref="NumberOutOfRangeException">The number needs more scale words than the
    /// locale defines.</exception>
    /// <exception cref="AmbiguousCurrencyException">The culture resolved through language
    /// fallback, so the locale's default currency cannot be assumed to be the right one.</exception>
    /// <exception cref="InvalidLocalizationException">The localization has no currency and none
    /// was supplied to the constructor.</exception>
    public string Convert(decimal number) =>
        Render(number, _localization.Settings.CurrencyFormat, ResolveCurrency(), DecimalReadingMode.Fraction);

    /// <summary>
    /// Converts a number to words without any currency units.
    /// </summary>
    /// <param name="number">The number to convert. The fractional part is rounded to
    /// <see cref="SettingsModel.DecimalPlaces"/> digits, away from zero.</param>
    /// <returns>The number in words, without currency.</returns>
    /// <exception cref="NumberOutOfRangeException">The number needs more scale words than the
    /// locale defines.</exception>
    public string ConvertWithoutCurrency(decimal number) =>
        Render(number, _localization.Settings.NumberFormat, currency: null, _localization.Settings.DecimalReading);

    private CurrencyModel ResolveCurrency()
    {
        if (_overriddenCurrency is { } overridden)
            return overridden;

        // es-MX resolves to the Spanish number words, whose default currency is the euro.
        // Printing "EUROS" for a Mexican amount is exactly the kind of silent wrongness
        // this library exists to avoid, so it has to be an error.
        if (!_localeCurrencyApplies)
            throw new AmbiguousCurrencyException(_requestedCulture, _resolvedCulture);

        if (_localization.Currency is { } declared)
            return declared;

        if (_localization.DefaultCurrency is { } key)
            return ResolveCurrencyCode(_localization, _resolvedCulture, key);

        throw new InvalidLocalizationException(
            "This localization defines no currency. Supply a CurrencyModel to the constructor " +
            "or call ConvertWithoutCurrency instead.");
    }

    private static CurrencyModel ValidatedOverride(CurrencyModel currency)
    {
        if (currency is null)
            throw new ArgumentNullException(nameof(currency));

        LocalizationValidator.ValidateCurrency(currency, "currency", "caller-supplied CurrencyModel");
        return currency;
    }

    private string Render(decimal number, string template, CurrencyModel? currency, DecimalReadingMode readingMode)
    {
        var settings = _localization.Settings;
        var (isNegative, wholePart, fractionPart) = SplitNumberParts(number, settings.DecimalPlaces);
        var currencyFollows = currency is not null;

        var wholeWords = ConvertWholeNumber(wholePart, number, currencyFollows, out var endsWithNounScale);
        if (wholeWords.Length == 0)
        {
            wholeWords = settings.ZeroWord;
        }
        else if (currencyFollows && endsWithNounScale && !string.IsNullOrEmpty(settings.NounScaleLinkWord))
        {
            // "UN MILLÓN DE EUROS", but "UN MILLÓN QUINIENTOS MIL EUROS".
            wholeWords += " " + settings.NounScaleLinkWord;
        }

        var fractionWords = readingMode == DecimalReadingMode.Digits
            ? ReadFractionDigits(fractionPart, settings.DecimalPlaces)
            : ConvertWholeNumber(fractionPart, number, currencyFollows, out _);
        if (fractionWords.Length == 0)
            fractionWords = settings.ZeroWord;

        // One pass over the template, so a currency name that happens to contain "{minor}"
        // is emitted literally instead of being treated as another placeholder.
        var rendered = PlaceholderPattern.Replace(template, match =>
        {
            var value = match.Groups[2].Value switch
            {
                "whole" => wholeWords,
                "decimal" => fractionWords,
                "major" => currency is null ? string.Empty : Inflect(currency.Major, currency.MajorSingular, wholePart),
                "minor" => currency is null ? string.Empty : Inflect(currency.Minor, currency.MinorSingular, fractionPart),
                _ => match.Value,
            };

            return value.Length == 0 ? string.Empty : match.Groups[1].Value + value;
        });

        var result = rendered.Trim();

        if (!isNegative)
            return result;

        return result.Length == 0
            ? settings.NegativeWord
            : settings.NegativeWord + " " + result;
    }

    private static string Inflect(string plural, string? singular, decimal value) =>
        value == 1m && !string.IsNullOrEmpty(singular) ? singular! : plural;

    private ScaleKind KindOf(int scale)
    {
        var kinds = _localization.Numbers!.ScaleKinds;
        return kinds is not null && scale < kinds.Length ? kinds[scale] : ScaleKind.Adjective;
    }

    private string ConvertWholeNumber(
        decimal value,
        decimal originalNumber,
        bool currencyFollows,
        out bool endsWithNounScale)
    {
        endsWithNounScale = false;

        if (value == 0m)
            return string.Empty;

        var scales = _localization.Numbers!.Scales;
        var groups = new List<string>();
        var remaining = value;
        var scale = 0;
        var isLowestGroup = true;

        while (remaining > 0m)
        {
            var group = (int)(remaining % 1000m);
            if (group > 0)
            {
                if (scale >= scales.Length)
                    throw new NumberOutOfRangeException(originalNumber, scales.Length);

                var scaleIsNoun = scale > 0 && KindOf(scale) == ScaleKind.Noun;
                var followedByNoun = scale > 0 ? scaleIsNoun : currencyFollows;

                var text = ConvertGroup(group, scale, followedByNoun);
                if (scale > 0)
                {
                    var scaleWord = ScaleWord(scale, group);
                    text = text.Length == 0 ? scaleWord : text + " " + scaleWord;
                }

                if (text.Length > 0)
                    groups.Insert(0, text);

                if (isLowestGroup)
                {
                    endsWithNounScale = scaleIsNoun;
                    isLowestGroup = false;
                }
            }

            remaining = Math.Floor(remaining / 1000m);
            scale++;
        }

        return string.Join(" ", groups);
    }

    private string ScaleWord(int scale, int group)
    {
        var numbers = _localization.Numbers!;

        if (group > 1 && numbers.ScalesPlural is { } plural && plural[scale].Length > 0)
            return plural[scale];

        return numbers.Scales[scale];
    }

    private string ConvertGroup(int number, int scale, bool followedByNoun)
    {
        var hundreds = number / 100;
        var remainder = number % 100;
        var parts = new List<string>(2);

        if (hundreds > 0)
            parts.Add(HundredsWord(hundreds, remainder, scale));

        // A group whose last two digits are zero contributes no tens/ones word at all,
        // which is why the override maps are never consulted with a key of 0.
        if (remainder > 0)
        {
            var word = TensAndOnesWord(remainder, number, scale, followedByNoun);
            if (word.Length > 0)
                parts.Add(word);
        }

        return string.Join(" ", parts);
    }

    private string HundredsWord(int hundreds, int remainder, int scale)
    {
        var numbers = _localization.Numbers!;

        if (remainder == 0 && numbers.ExactHundreds is { } exact && exact[hundreds].Length > 0)
        {
            // A noun scale word ends the numeral phrase, so French keeps the plural in
            // "DEUX CENTS MILLIONS" but drops it in "DEUX CENT MILLE".
            var endsNumeralPhrase = scale == 0 || KindOf(scale) == ScaleKind.Noun;

            if (endsNumeralPhrase || _localization.Settings.UseExactHundredsBeforeScale)
                return exact[hundreds];
        }

        return numbers.Hundreds[hundreds];
    }

    /// <summary>
    /// Resolves the last two digits of a group. The order of the checks is the priority
    /// order: dropping "one" before the thousands scale wins over every override, an
    /// override that only applies before a following word wins over a general one, a
    /// general override wins over the teens table, and regular construction is the
    /// fallback.
    /// </summary>
    private string TensAndOnesWord(int remainder, int groupValue, int scale, bool followedByNoun)
    {
        return DropOneBeforeThousand(remainder, groupValue, scale)
            ?? OverrideBeforeFollowingWord(remainder, scale, followedByNoun)
            ?? GeneralOverride(remainder)
            ?? Teen(remainder)
            ?? Regular(remainder);
    }

    // "bin" rather than "bir bin", "mille" rather than "un mille".
    private string? DropOneBeforeThousand(int remainder, int groupValue, int scale) =>
        scale == 1 && groupValue == 1 && remainder == 1 && _localization.Settings.SkipOneForThousand
            ? string.Empty
            : null;

    private string? OverrideBeforeFollowingWord(int remainder, int scale, bool followedByNoun)
    {
        if (_localization.SpecialNumbers?.SpecialBeforeScale is not { } overrides)
            return null;

        // French drops the plural of "vingt" only before a numeral adjective, Spanish
        // apocopates only before a noun. Both are "before a following word", but they
        // pick opposite sides of that split.
        var beforeNumeralAdjective = scale > 0 && KindOf(scale) == ScaleKind.Adjective;
        var beforeNoun = followedByNoun && _localization.Settings.ApocopateBeforeNoun;

        if (!beforeNumeralAdjective && !beforeNoun)
            return null;

        return overrides.TryGetValue(remainder, out var word) ? word : null;
    }

    private string? GeneralOverride(int remainder) =>
        _localization.SpecialNumbers?.Special is { } overrides && overrides.TryGetValue(remainder, out var word)
            ? word
            : null;

    private string? Teen(int remainder)
    {
        var special = _localization.SpecialNumbers;
        var useTeens = _localization.Settings.UseTeens ?? special?.Teens is not null;

        return useTeens && remainder is >= 11 and <= 19 && special?.Teens is { } teens
            ? teens[remainder - 11]
            : null;
    }

    private string Regular(int remainder)
    {
        var numbers = _localization.Numbers!;
        var tens = remainder / 10;
        var ones = remainder % 10;

        if (tens == 0)
            return numbers.Ones[ones];

        var word = numbers.Tens[tens];
        if (ones > 0)
            word += (_localization.SpecialNumbers?.CompoundSeparator ?? " ") + numbers.Ones[ones];

        return word;
    }

    private string ReadFractionDigits(decimal fraction, int decimalPlaces)
    {
        if (decimalPlaces == 0 || fraction == 0m)
            return string.Empty;

        var digits = ((long)fraction)
            .ToString(CultureInfo.InvariantCulture)
            .PadLeft(decimalPlaces, '0')
            .TrimEnd('0');

        if (digits.Length == 0)
            return string.Empty;

        var numbers = _localization.Numbers!;
        var words = new List<string>(digits.Length);

        foreach (var character in digits)
        {
            var digit = character - '0';
            words.Add(digit == 0 ? _localization.Settings.ZeroWord : numbers.Ones[digit]);
        }

        return string.Join(" ", words);
    }

    private static (bool IsNegative, decimal Whole, decimal Fraction) SplitNumberParts(
        decimal number,
        int decimalPlaces)
    {
        var isNegative = number < 0m;
        var magnitude = Math.Abs(number);

        if (decimalPlaces == 0)
        {
            var rounded = Math.Round(magnitude, 0, MidpointRounding.AwayFromZero);
            return (isNegative && rounded != 0m, rounded, 0m);
        }

        var whole = Math.Floor(magnitude);
        var factor = Pow10(decimalPlaces);

        // Money rounds away from zero; the framework default (to even) would turn
        // 1.005 into one dollar zero cents.
        var fraction = Math.Round((magnitude - whole) * factor, MidpointRounding.AwayFromZero);

        if (fraction >= factor)
        {
            whole += 1m;
            fraction = 0m;
        }

        // -0.001 rounds to zero, and "negative zero" is not a thing anyone wants printed.
        if (whole == 0m && fraction == 0m)
            isNegative = false;

        return (isNegative, whole, fraction);
    }

    private static decimal Pow10(int exponent) => exponent switch
    {
        0 => 1m,
        1 => 10m,
        2 => 100m,
        3 => 1_000m,
        4 => 10_000m,
        5 => 100_000m,
        6 => 1_000_000m,
        _ => throw new InvalidLocalizationException(
            $"decimalPlaces must be between 0 and 6, but is {exponent}. " +
            "A localization must not be mutated after it has been given to a converter."),
    };

    private static CurrencyModel ResolveCurrencyCode(
        LocalizationModel localization,
        string culture,
        string currencyCode)
    {
        if (currencyCode is null)
            throw new ArgumentNullException(nameof(currencyCode));

        if (localization.Currencies is { } currencies)
        {
            foreach (var pair in currencies)
            {
                if (string.Equals(pair.Key, currencyCode, StringComparison.OrdinalIgnoreCase))
                    return pair.Value;
            }
        }

        var available = localization.Currencies is null || localization.Currencies.Count == 0
            ? "none"
            : string.Join(", ", localization.Currencies.Keys.OrderBy(key => key, StringComparer.Ordinal));

        throw new ArgumentException(
            $"Culture '{culture}' does not define the currency '{currencyCode}'. Currencies defined: {available}. " +
            "Pass a CurrencyModel instead to use a currency the locale does not know.",
            nameof(currencyCode));
    }
}
