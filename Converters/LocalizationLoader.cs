using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;
using NumWordify.Exceptions;
using NumWordify.Models;

namespace NumWordify.Converters;

/// <summary>
/// A localization together with how confidently it was matched to the requested culture.
/// </summary>
internal sealed class ResolvedLocalization
{
    public ResolvedLocalization(LocalizationModel model, string cultureName, bool regionMatches)
    {
        Model = model;
        CultureName = cultureName;
        RegionMatches = regionMatches;
    }

    /// <summary>Gets the parsed, validated localization.</summary>
    public LocalizationModel Model { get; }

    /// <summary>Gets the culture the localization actually belongs to.</summary>
    public string CultureName { get; }

    /// <summary>
    /// Gets a value indicating whether the requested culture named the same region as the
    /// resolved one, or named no region at all. When false the number words are still
    /// right but the locale's default currency is not: <c>es-MX</c> resolves to the
    /// Spanish number words, whose default currency is the euro.
    /// </summary>
    public bool RegionMatches { get; }
}

/// <summary>
/// Loads embedded localization resources and caches the parsed, validated models.
/// </summary>
/// <remarks>
/// Parsing a locale costs an assembly manifest scan, a stream read and a full JSON
/// deserialization. Doing that per call made <c>ToWords</c> roughly twenty-five times
/// slower than necessary, so every parsed model is cached here for the lifetime of the
/// process. The cached instances are never handed out to callers, which is what makes
/// sharing them safe.
/// </remarks>
internal static class LocalizationLoader
{
    private const string ResourceSuffix = ".json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly Lazy<string[]> ResourceNames = new(() =>
        typeof(LocalizationLoader).GetTypeInfo().Assembly
            .GetManifestResourceNames()
            .Where(name => name.EndsWith(ResourceSuffix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray());

    private static readonly ConcurrentDictionary<string, LocalizationModel> Cache =
        new(StringComparer.Ordinal);

    private static readonly Lazy<IReadOnlyList<string>> SupportedCultureNames = new(() =>
        ResourceNames.Value
            .Select(CultureNameOf)
            .Where(culture => !Load(culture).Deprecated)
            .OrderBy(culture => culture, StringComparer.Ordinal)
            .ToArray());

    /// <summary>
    /// Gets the culture names shipped with the library, excluding deprecated ones.
    /// </summary>
    public static IReadOnlyList<string> SupportedCultures => SupportedCultureNames.Value;

    /// <summary>
    /// Determines whether a localization resource can be resolved for the given culture.
    /// </summary>
    public static bool IsSupported(string? culture) =>
        !string.IsNullOrWhiteSpace(culture) && TryResolveResourceName(culture!, out _, out _);

    /// <summary>
    /// Returns the cached, validated localization for the given culture, together with
    /// how closely it matched.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="culture"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The culture is empty or whitespace.</exception>
    /// <exception cref="LocalizationNotFoundException">No resource matches the culture.</exception>
    /// <exception cref="InvalidLocalizationException">The resource could not be parsed or is invalid.</exception>
    public static ResolvedLocalization Resolve(string culture)
    {
        if (culture is null)
            throw new ArgumentNullException(nameof(culture));

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException(
                "Culture must be a non-empty culture name such as \"en-US\". " +
                "CultureInfo.InvariantCulture has an empty name and cannot be used; " +
                $"supported cultures are: {string.Join(", ", SupportedCultures)}.",
                nameof(culture));
        }

        if (!TryResolveResourceName(culture, out var resourceName, out var regionMatches))
            throw new LocalizationNotFoundException(culture, SupportedCultures);

        return new ResolvedLocalization(
            Cache.GetOrAdd(resourceName, Parse),
            CultureNameOf(resourceName),
            regionMatches);
    }

    private static LocalizationModel Load(string culture) =>
        Cache.GetOrAdd(culture + ResourceSuffix, Parse);

    private static string CultureNameOf(string resourceName) =>
        resourceName.Substring(0, resourceName.Length - ResourceSuffix.Length);

    /// <summary>
    /// Resolves a culture name to an embedded resource name. Matching is case-insensitive
    /// and falls back from a specific culture to another variant of the same language, so
    /// <c>"EN-US"</c>, <c>"en"</c> and <c>"en-GB"</c> all resolve to <c>en-US.json</c>.
    /// </summary>
    /// <remarks>
    /// Deprecated localizations resolve only on an exact name, so a redundant file such as
    /// <c>tr-TR-EUR</c> can never be picked up by a language fallback.
    /// </remarks>
    private static bool TryResolveResourceName(string culture, out string resourceName, out bool regionMatches)
    {
        var names = ResourceNames.Value;

        var exact = culture + ResourceSuffix;
        foreach (var name in names)
        {
            if (string.Equals(name, exact, StringComparison.OrdinalIgnoreCase))
            {
                resourceName = name;
                regionMatches = true;
                return true;
            }
        }

        var requestedLanguage = LanguageOf(culture);
        var requestedRegion = RegionOf(culture);

        // Prefer a locale whose region also matches, then the canonical variant of the
        // language. Ordering by culture name keeps the choice deterministic; region
        // matching keeps it correct when a language ships more than one region.
        string? languageMatch = null;

        foreach (var name in names)
        {
            var candidate = CultureNameOf(name);

            if (!string.Equals(LanguageOf(candidate), requestedLanguage, StringComparison.OrdinalIgnoreCase))
                continue;

            if (Load(candidate).Deprecated)
                continue;

            if (requestedRegion is not null &&
                string.Equals(RegionOf(candidate), requestedRegion, StringComparison.OrdinalIgnoreCase))
            {
                resourceName = name;
                regionMatches = true;
                return true;
            }

            languageMatch ??= name;
        }

        if (languageMatch is not null)
        {
            resourceName = languageMatch;
            // A request that named no region accepts whatever the language's default is;
            // a request that named a different region does not, at least for currency.
            regionMatches = requestedRegion is null;
            return true;
        }

        resourceName = string.Empty;
        regionMatches = false;
        return false;
    }

    private static string LanguageOf(string culture)
    {
        var separatorIndex = culture.IndexOf("-", StringComparison.Ordinal);
        return separatorIndex < 0 ? culture : culture.Substring(0, separatorIndex);
    }

    private static string? RegionOf(string culture)
    {
        var parts = culture.Split('-');
        return parts.Length < 2 || parts[1].Length == 0 ? null : parts[1];
    }

    private static LocalizationModel Parse(string resourceName)
    {
        var assembly = typeof(LocalizationLoader).GetTypeInfo().Assembly;

        string json;
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream is null)
            {
                throw new InvalidLocalizationException(
                    $"The embedded resource '{resourceName}' could not be opened.");
            }

            using var reader = new StreamReader(stream, Encoding.UTF8);
            json = reader.ReadToEnd();
        }

        LocalizationModel? model;
        try
        {
            model = JsonSerializer.Deserialize<LocalizationModel>(json, SerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidLocalizationException(
                $"The embedded resource '{resourceName}' is not valid JSON: {ex.Message}", ex);
        }

        if (model is null)
        {
            throw new InvalidLocalizationException(
                $"The embedded resource '{resourceName}' deserialized to null.");
        }

        LocalizationValidator.Validate(model, resourceName);
        return model;
    }
}
