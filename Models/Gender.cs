using System.Text.Json.Serialization;

namespace NumWordify.Models;

/// <summary>
/// The grammatical gender of a word a numeral can stand in front of — a scale word or a
/// currency unit. Numerals in many languages agree with it.
/// </summary>
/// <remarks>
/// Russian is the reason this exists: <c>ТЫСЯЧА</c> is feminine and <c>МИЛЛИОН</c> is
/// masculine, so 1000 is <c>ОДНА ТЫСЯЧА</c> while 1000000 is <c>ОДИН МИЛЛИОН</c> — the
/// same digit, a different word. A locale that does not inflect its numerals leaves the
/// gender fields unset and nothing changes.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    /// <summary>The default when a locale says nothing.</summary>
    Masculine = 0,

    /// <summary>Russian <c>ТЫСЯЧА</c>, <c>КОПЕЙКА</c>.</summary>
    Feminine = 1,

    /// <summary>Neuter, for the languages that distinguish all three.</summary>
    Neuter = 2,
}
