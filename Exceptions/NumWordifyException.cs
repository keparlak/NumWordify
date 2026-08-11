namespace NumWordify.Exceptions;

/// <summary>
/// Base class for every error raised by NumWordify. Catch this type to handle
/// all library failures without also catching unrelated framework exceptions.
/// </summary>
public class NumWordifyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NumWordifyException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NumWordifyException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumWordifyException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    public NumWordifyException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when no embedded localization resource matches the requested culture.
/// </summary>
public sealed class LocalizationNotFoundException : NumWordifyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationNotFoundException"/> class.
    /// </summary>
    /// <param name="culture">The culture that could not be resolved.</param>
    /// <param name="availableCultures">The cultures shipped with the library.</param>
    public LocalizationNotFoundException(string culture, IReadOnlyList<string> availableCultures)
        : base($"Localization not found for culture '{culture}'. Available cultures: {string.Join(", ", availableCultures)}.")
    {
        Culture = culture;
        AvailableCultures = availableCultures;
    }

    /// <summary>Gets the culture that could not be resolved.</summary>
    public string Culture { get; }

    /// <summary>Gets the cultures shipped with the library.</summary>
    public IReadOnlyList<string> AvailableCultures { get; }
}

/// <summary>
/// Thrown when a localization model is structurally invalid — either because an
/// embedded resource could not be parsed or because a caller-supplied
/// <see cref="Models.LocalizationModel"/> is incomplete.
/// </summary>
public sealed class InvalidLocalizationException : NumWordifyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidLocalizationException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidLocalizationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidLocalizationException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    public InvalidLocalizationException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when a culture resolved to another region's localization, so the number words
/// are right but the locale's default currency cannot be assumed to be.
/// </summary>
/// <remarks>
/// <c>es-MX</c> resolves to the Spanish localization, whose default currency is the euro.
/// Printing euros for a Mexican amount would be wrong in a way nobody notices, so the
/// currency has to be named explicitly — either as a <see cref="Models.CurrencyModel"/>
/// or as a code from the locale's currency map.
/// </remarks>
public sealed class AmbiguousCurrencyException : NumWordifyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousCurrencyException"/> class.
    /// </summary>
    /// <param name="requestedCulture">The culture the caller asked for.</param>
    /// <param name="resolvedCulture">The localization that was used for the number words.</param>
    public AmbiguousCurrencyException(string requestedCulture, string resolvedCulture)
        : base($"Culture '{requestedCulture}' has no localization of its own and fell back to " +
               $"'{resolvedCulture}' for its number words. The default currency of " +
               $"'{resolvedCulture}' does not necessarily apply to '{requestedCulture}', so it " +
               "will not be assumed. Pass a CurrencyModel or a currency code explicitly, or " +
               "call ConvertWithoutCurrency.")
    {
        RequestedCulture = requestedCulture;
        ResolvedCulture = resolvedCulture;
    }

    /// <summary>Gets the culture the caller asked for.</summary>
    public string RequestedCulture { get; }

    /// <summary>Gets the localization that was used for the number words.</summary>
    public string ResolvedCulture { get; }
}

/// <summary>
/// Thrown when a number is larger than the scale words configured for the locale
/// can express. See <see cref="Models.NumbersModel.Scales"/>.
/// </summary>
public sealed class NumberOutOfRangeException : NumWordifyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NumberOutOfRangeException"/> class.
    /// </summary>
    /// <param name="value">The number that could not be converted.</param>
    /// <param name="maximumScaleCount">The number of scale words the locale defines.</param>
    public NumberOutOfRangeException(decimal value, int maximumScaleCount)
        : base($"The number {value} is too large for this locale: it defines {maximumScaleCount} scale word(s), " +
               $"so the largest convertible value is 10^{maximumScaleCount * 3} - 1.")
    {
        Value = value;
        MaximumScaleCount = maximumScaleCount;
    }

    /// <summary>Gets the number that could not be converted.</summary>
    public decimal Value { get; }

    /// <summary>Gets the number of scale words the locale defines.</summary>
    public int MaximumScaleCount { get; }
}
