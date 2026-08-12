using NumWordify.Converters;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// The README claims localizations are parsed once and cached. Every <c>ToWords</c> call
/// used to scan the assembly manifest, read a stream and deserialize the whole file, so
/// this pins the claim to something checkable rather than a timing measurement.
/// </summary>
public class LocalizationCacheTests
{
    [Fact]
    public void A_locale_is_parsed_once_and_reused()
    {
        var first = LocalizationLoader.Resolve("en-US");
        var second = LocalizationLoader.Resolve("en-US");

        Assert.Same(first.Model, second.Model);
    }

    [Fact]
    public void Culture_spellings_that_resolve_to_the_same_file_share_one_entry()
    {
        // The cache is keyed on the resolved resource, not on the string the caller
        // passed, so spelling variants cannot each pay for their own parse.
        var canonical = LocalizationLoader.Resolve("en-US").Model;

        Assert.Same(canonical, LocalizationLoader.Resolve("EN-US").Model);
        Assert.Same(canonical, LocalizationLoader.Resolve("en").Model);
        Assert.Same(canonical, LocalizationLoader.Resolve("en-GB").Model);
    }
}
