using System.Globalization;
using NumWordify.Converters;
using NumWordify.Exceptions;
using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Runs the library under cultures that break in different ways. Nothing else in the
/// suite ever leaves the developer's culture, which is exactly why a diagnostic that
/// formatted its number with <see cref="CultureInfo.CurrentCulture"/> shipped unnoticed.
/// </summary>
/// <remarks>
/// The cultures are chosen to fail differently: tr-TR uses a comma for the decimal
/// separator, sv-SE uses U+2212 for the minus sign rather than the ASCII hyphen, and
/// ar-SA uses U+066B for the separator and prefixes negatives with U+061C. Every
/// assertion below is on the invariant text, never on what the hostile culture produces,
/// so the NLS/ICU differences between the .NET Framework and .NET legs cannot make these
/// flaky.
/// </remarks>
[Collection("Culture")]
public class InvariantMessageTests
{
    [Theory]
    [InlineData("tr-TR")]
    [InlineData("sv-SE")]
    [InlineData("ar-SA")]
    [InlineData("de-DE")]
    public void The_out_of_range_message_reads_the_same_on_every_machine(string machineCulture)
    {
        var exception = UnderCulture(machineCulture, () =>
            Assert.Throws<NumberOutOfRangeException>(() => 1000000000000000000.5m.ToWords("en-US")));

        Assert.Contains(
            "The number 1000000000000000000.5 is too large",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("sv-SE")]
    [InlineData("ar-SA")]
    public void The_out_of_range_message_uses_an_ascii_minus_sign(string machineCulture)
    {
        // sv-SE renders the sign as U+2212 and ar-SA prefixes U+061C, so a positive value
        // alone would not catch this half.
        var exception = UnderCulture(machineCulture, () =>
            Assert.Throws<NumberOutOfRangeException>(() => (-1000000000000000000.5m).ToWords("en-US")));

        Assert.Contains("-1000000000000000000.5", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validation_messages_use_an_ascii_minus_sign()
    {
        var decimalPlaces = UnderCulture("sv-SE", () =>
        {
            var localization = TestLocalizations.EnglishWithoutCurrency();
            localization.Settings.DecimalPlaces = -3;

            return Assert.Throws<InvalidLocalizationException>(
                () => new NumberToWordsConverter(localization));
        });

        Assert.Contains("but is -3.", decimalPlaces.Message, StringComparison.Ordinal);

        var overrideKey = UnderCulture("sv-SE", () =>
        {
            var localization = TestLocalizations.EnglishWithoutCurrency();
            localization.SpecialNumbers!.Special = new Dictionary<int, string> { [-5] = "x" };

            return Assert.Throws<InvalidLocalizationException>(
                () => new NumberToWordsConverter(localization));
        });

        Assert.Contains("contains the key -5.", overrideKey.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("sv-SE")]
    [InlineData("ar-SA")]
    public void Conversion_output_does_not_depend_on_the_machine_culture(string machineCulture)
    {
        // True today only because every comparison in the library carries an explicit
        // Ordinal and the one number-to-string on the output path is invariant. Nothing
        // pinned that, so a future culture-sensitive call would go unnoticed.
        var invariant = UnderCulture(CultureInfo.InvariantCulture.Name, Convert);
        var hostile = UnderCulture(machineCulture, Convert);

        Assert.Equal(invariant, hostile);
        Assert.Equal("BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr", hostile[1]);

        static string[] Convert() =>
        [
            1234.56m.ToWords("en-US"),
            1234.56m.ToWords("tr-TR"),
            1234.56m.ToWords("fr-FR"),
            1234.56m.ToWords("es-ES"),
            1234.56m.ToWordsWithoutCurrency("es-ES"),
        ];
    }

    private static T UnderCulture<T>(string culture, Func<T> action)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}

/// <summary>
/// Serialises the culture-mutating tests. <see cref="CultureInfo.CurrentCulture"/> is
/// per-thread rather than global, so this is belt and braces — but the alternative,
/// <c>DefaultThreadCurrentCulture</c>, would leak across the whole run.
/// </summary>
[CollectionDefinition("Culture", DisableParallelization = true)]
public class CultureBound
{
}
