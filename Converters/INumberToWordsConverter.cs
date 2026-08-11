namespace NumWordify.Converters;

/// <summary>
/// Converts numbers to their word representation for a single locale.
/// </summary>
/// <remarks>
/// Implementations are expected to be immutable and safe to share between threads, so a
/// converter can be registered as a singleton. See <see cref="NumberToWordsConverter"/>.
/// </remarks>
public interface INumberToWordsConverter
{
    /// <summary>
    /// Converts a number to words including its currency units.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <returns>The number in words, with currency.</returns>
    string Convert(decimal number);

    /// <summary>
    /// Converts a number to words without any currency units.
    /// </summary>
    /// <param name="number">The number to convert.</param>
    /// <returns>The number in words, without currency.</returns>
    string ConvertWithoutCurrency(decimal number);
}
