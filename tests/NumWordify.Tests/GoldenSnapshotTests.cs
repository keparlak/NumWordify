using System.Globalization;
using System.Reflection;
using System.Text;
using NumWordify.Converters;
using NumWordify.Exceptions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Full-output snapshots: every value from 0 to 1000 plus a magnitude ladder, rendered
/// both with and without currency, for every shipped locale.
/// </summary>
/// <remarks>
/// <para>
/// The hand-written golden tables cover the cases someone thought to write down. That is
/// what let a French pluralisation bug above 10^6 ship: the tables stopped at two million.
/// A snapshot covers everything, and a regression shows up as a reviewable diff rather
/// than as silence.
/// </para>
/// <para>
/// To re-approve after an intentional change, run the suite with
/// <c>NUMWORDIFY_APPROVE=1</c> and read the resulting diff line by line. The files live in
/// the source tree precisely so that reading them is unavoidable.
/// </para>
/// </remarks>
public class GoldenSnapshotTests
{
    private static readonly string ApprovalsDirectory =
        typeof(GoldenSnapshotTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "ApprovalsDirectory")
            .Value!;

    /// <summary>
    /// Magnitudes chosen to exercise scale boundaries, carries, and the interaction
    /// between a hundreds digit and the scale word that follows it. The ladder runs to
    /// 10^15 − 1, past the ceiling of some locales on purpose: a value a locale cannot
    /// express is recorded as such in its snapshot rather than dropped from the ladder.
    /// </summary>
    private static readonly decimal[] Magnitudes =
    [
        1_001m, 1_010m, 1_100m, 1_234m, 9_999m,
        10_000m, 10_101m, 12_345m, 80_000m, 99_999m,
        100_000m, 100_001m, 123_456m, 180_000m, 200_000m, 999_999m,
        1_000_000m, 1_000_001m, 1_010_101m, 2_000_000m, 21_000_000m,
        80_000_000m, 100_000_000m, 123_456_789m, 180_000_000m, 200_000_000m,
        200_500_000m, 999_999_999m,
        1_000_000_000m, 2_000_000_000m, 21_000_000_000m, 80_000_000_000m,
        300_000_000_000m, 999_999_999_999m,
        1_000_000_000_000m, 123_456_789_012_345m, 999_999_999_999_999m,
    ];

    /// <summary>
    /// Fractional and signed values, where rounding and the negative-zero rule live.
    /// </summary>
    private static readonly decimal[] Fractions =
    [
        0.01m, 0.05m, 0.10m, 0.50m, 0.99m, 0.995m, 0.999m,
        1.005m, 1.01m, 1.5m, 1.234567m,
        21.21m, 99.99m, 1_234.56m, 1_000_000.50m,
        -0.001m, -0.01m, -1.005m, -21.05m, -1_234.56m,
    ];

    public static TheoryData<string> Cultures()
    {
        var data = new TheoryData<string>();
        foreach (var culture in NumberToWordsConverter.SupportedCultures)
            data.Add(culture);

        return data;
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Output_matches_the_approved_snapshot(string culture)
    {
        var actual = Render(culture);
        var path = Path.Combine(ApprovalsDirectory, culture + ".approved.txt");

        if (string.Equals(Environment.GetEnvironmentVariable("NUMWORDIFY_APPROVE"), "1", StringComparison.Ordinal))
        {
            Directory.CreateDirectory(ApprovalsDirectory);
            File.WriteAllText(path, actual, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return;
        }

        Assert.True(
            File.Exists(path),
            $"No approved snapshot for '{culture}'. Run the suite with NUMWORDIFY_APPROVE=1 to create {path}.");

        AssertSameLines(File.ReadAllText(path), actual, culture);
    }

    private static string Render(string culture)
    {
        var converter = new NumberToWordsConverter(culture);
        var builder = new StringBuilder();

        for (var value = 0; value <= 1_000; value++)
            AppendCase(builder, converter, value);

        foreach (var value in Magnitudes)
            AppendCase(builder, converter, value);

        foreach (var value in Fractions)
            AppendCase(builder, converter, value);

        return builder.ToString();
    }

    private static void AppendCase(StringBuilder builder, NumberToWordsConverter converter, decimal value)
    {
        builder
            .Append(value.ToString(CultureInfo.InvariantCulture))
            .Append(" | ")
            .Append(Render(converter.Convert, value))
            .Append(" | ")
            .Append(Render(converter.ConvertWithoutCurrency, value))
            .Append('\n');
    }

    /// <summary>
    /// Records a locale's ceiling in the snapshot rather than capping the ladder at the
    /// lowest one. Locales do not share a range — pt-PT stops at 10^9 − 1 because it has
    /// no single word for a thousand million, es-ES at 10^15 − 1 — and capping everyone at
    /// the smallest would drop coverage from the four locales that reach further. Writing
    /// the refusal into the file makes a changed ceiling show up as a reviewable diff,
    /// which is the same reason the rest of this file exists.
    /// </summary>
    private static string Render(Func<decimal, string> convert, decimal value)
    {
        try
        {
            return convert(value);
        }
        catch (NumberOutOfRangeException)
        {
            return "<out of range>";
        }
    }

    private static void AssertSameLines(string expected, string actual, string culture)
    {
        var expectedLines = SplitLines(expected);
        var actualLines = SplitLines(actual);

        for (var i = 0; i < Math.Min(expectedLines.Length, actualLines.Length); i++)
        {
            if (!string.Equals(expectedLines[i], actualLines[i], StringComparison.Ordinal))
            {
                Assert.Fail(
                    $"Snapshot mismatch for '{culture}' on line {i + 1}." +
                    $"{Environment.NewLine}approved: {expectedLines[i]}" +
                    $"{Environment.NewLine}actual:   {actualLines[i]}" +
                    $"{Environment.NewLine}Re-approve with NUMWORDIFY_APPROVE=1 once the change is intended.");
            }
        }

        Assert.Equal(expectedLines.Length, actualLines.Length);
    }

    // Git may normalise line endings on checkout, so compare line by line rather than
    // byte for byte.
    private static string[] SplitLines(string text) =>
        text.Split(["\r\n", "\n"], StringSplitOptions.None);
}
