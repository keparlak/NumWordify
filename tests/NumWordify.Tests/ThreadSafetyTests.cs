using System.Collections.Concurrent;
using NumWordify.Converters;
using NumWordify.Extensions;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// The converter used to keep the current scale in an instance field, so a shared
/// instance produced wrong words — and spurious "number is too large" failures — under
/// concurrency. Anything that looks stateless will be registered as a singleton sooner
/// or later, so this has to hold.
/// </summary>
public class ThreadSafetyTests
{
    [Fact]
    public void A_shared_converter_gives_the_same_answer_from_every_thread()
    {
        var converter = new NumberToWordsConverter("en-US");
        var expected = converter.ConvertWithoutCurrency(1234567.89m);
        var results = new ConcurrentBag<string>();
        var failures = new ConcurrentBag<Exception>();

        Parallel.For(0, 20_000, _ =>
        {
            try
            {
                results.Add(converter.ConvertWithoutCurrency(1234567.89m));
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        });

        Assert.Empty(failures);
        Assert.Equal(20_000, results.Count);
        Assert.All(results, result => Assert.Equal(expected, result));
    }

    [Fact]
    public void Concurrent_conversions_of_different_magnitudes_do_not_interfere()
    {
        var converter = new NumberToWordsConverter("tr-TR");
        var cases = new[]
        {
            (Value: 1m, Expected: "BİR TL SIFIR Kr"),
            (Value: 1_000m, Expected: "BİN TL SIFIR Kr"),
            (Value: 1_000_000m, Expected: "BİR MİLYON TL SIFIR Kr"),
            (Value: 123_456_789m, Expected: "YÜZ YİRMİ ÜÇ MİLYON DÖRT YÜZ ELLİ ALTI BİN YEDİ YÜZ SEKSEN DOKUZ TL SIFIR Kr"),
        };
        var mismatches = new ConcurrentBag<string>();

        Parallel.For(0, 20_000, index =>
        {
            var (value, expected) = cases[index % cases.Length];
            var actual = converter.Convert(value);
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
                mismatches.Add($"{value}: {actual}");
        });

        Assert.Empty(mismatches);
    }

    [Fact]
    public void The_localization_cache_is_safe_to_prime_from_many_threads()
    {
        var results = new ConcurrentBag<string>();

        Parallel.For(0, 5_000, _ => results.Add(21m.ToWords("es-ES")));

        Assert.All(results, result => Assert.Equal("VEINTIÚN EUROS CERO CÉNTIMOS", result));
    }
}
