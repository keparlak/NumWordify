using System.Collections.ObjectModel;
using NumWordify.Converters;
using NumWordify.Exceptions;
using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// The supported-culture list is a process-wide cached instance. Handing it out as the
/// array it is built from let any caller rewrite it permanently — the listing itself and
/// the text of every exception thrown afterwards, for the life of the process.
/// </summary>
public class CachedListImmutabilityTests
{
    [Fact]
    public void The_supported_culture_list_cannot_be_cast_back_to_an_array()
    {
        var cultures = NumberToWordsConverter.SupportedCultures;

        Assert.IsNotType<string[]>(cultures);
        Assert.Throws<InvalidCastException>(() => (string[])cultures);
    }

    [Fact]
    public void The_supported_culture_list_rejects_writes_through_IList()
    {
        // The cast is not the only write vector, so checking the runtime type alone
        // would leave this one open.
        var cultures = NumberToWordsConverter.SupportedCultures;

        Assert.Throws<NotSupportedException>(() => ((IList<string>)cultures)[0] = "PWNED");
        Assert.Equal(["en-US", "es-ES", "fr-FR", "tr-TR"], NumberToWordsConverter.SupportedCultures);
    }

    [Fact]
    public void The_cultures_carried_by_the_not_found_exception_cannot_be_cast_back_to_an_array()
    {
        var exception = Assert.Throws<LocalizationNotFoundException>(() => 1m.ToWords("de-DE"));

        Assert.IsNotType<string[]>(exception.AvailableCultures);
        Assert.Throws<InvalidCastException>(() => (string[])exception.AvailableCultures);
        Assert.Contains("en-US", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_caller_supplied_culture_array_is_snapshotted_by_the_exception()
    {
        var supplied = new[] { "aa-AA", "bb-BB" };
        var exception = new LocalizationNotFoundException("xx-XX", supplied);

        supplied[0] = "MUTATED";

        Assert.Equal(["aa-AA", "bb-BB"], exception.AvailableCultures);
        Assert.IsType<ReadOnlyCollection<string>>(exception.AvailableCultures);
    }

    [Fact]
    public void A_caller_supplied_read_only_view_is_snapshotted_too()
    {
        // ReadOnlyCollection is a read-only view, not an immutable collection. Storing one
        // by reference would leave the exception's payload writable through the list the
        // caller kept — the same aliasing this class exists to rule out, one level down.
        var backing = new List<string> { "aa-AA", "bb-BB" };
        var exception = new LocalizationNotFoundException("xx-XX", new ReadOnlyCollection<string>(backing));

        backing[0] = "MUTATED";
        backing.Add("cc-CC");

        Assert.Equal(["aa-AA", "bb-BB"], exception.AvailableCultures);
    }
}
