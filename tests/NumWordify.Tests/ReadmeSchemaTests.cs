using System.Reflection;
using System.Text;
using System.Text.Json;
using NumWordify.Converters;
using NumWordify.Models;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Deserializes the JSON block the README tells contributors to copy when adding a
/// language, straight out of README.md.
/// </summary>
/// <remarks>
/// Rebuilding that block as a C# model in a test proves the model works, not that the
/// documented JSON does — a misspelled key in the README would pass. Adding a language is
/// the README's most important path, so it is read from the file itself.
/// </remarks>
public class ReadmeSchemaTests
{
    private const string SectionHeading = "## Adding a new language";
    private const string FenceOpen = "```json";
    private const string FenceClose = "```";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly string RepositoryRoot =
        typeof(ReadmeSchemaTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "RepositoryRoot")
            .Value!;

    [Fact]
    public void The_documented_localization_schema_deserializes_and_converts()
    {
        var json = ExtractFirstJsonBlockAfter(SectionHeading);

        var localization = JsonSerializer.Deserialize<LocalizationModel>(json, SerializerOptions);

        Assert.NotNull(localization);

        // Construction runs the full validator, so an unknown or misspelled key that
        // silently deserialized to a default still surfaces here.
        var converter = new NumberToWordsConverter(localization!);

        Assert.Equal("ELEVEN DOLLARS ZERO CENTS", converter.Convert(11m));
        Assert.Equal("TWENTY-ONE POINT FIFTY", converter.ConvertWithoutCurrency(21.5m));
        Assert.Equal("ONE DOLLAR ONE CENT", converter.Convert(1.01m));
    }

    private static string ExtractFirstJsonBlockAfter(string heading)
    {
        var readmePath = Path.Combine(RepositoryRoot, "README.md");
        Assert.True(File.Exists(readmePath), $"README.md not found at {readmePath}.");

        var lines = File.ReadAllLines(readmePath, Encoding.UTF8);

        var headingIndex = Array.FindIndex(
            lines,
            line => line.StartsWith(heading, StringComparison.Ordinal));
        Assert.True(headingIndex >= 0, $"README.md has no '{heading}' section.");

        var openIndex = Array.FindIndex(
            lines,
            headingIndex,
            line => line.Trim().StartsWith(FenceOpen, StringComparison.Ordinal));
        Assert.True(openIndex >= 0, $"No ```json block follows '{heading}' in README.md.");

        var builder = new StringBuilder();
        for (var i = openIndex + 1; i < lines.Length; i++)
        {
            if (string.Equals(lines[i].Trim(), FenceClose, StringComparison.Ordinal))
                return builder.ToString();

            builder.AppendLine(lines[i]);
        }

        Assert.Fail("The ```json block in README.md is not closed.");
        return string.Empty;
    }
}
