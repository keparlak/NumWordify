using System.Text.Json.Serialization;

namespace NumWordify.Models;

/// <summary>
/// Grammatical class of a scale word, which decides how the group in front of it behaves.
/// </summary>
/// <remarks>
/// French needs this distinction: <c>cent</c> and <c>vingt</c> take a plural -s unless
/// another numeral adjective follows. <c>mille</c> is a numeral adjective, so the -s
/// drops (<c>DEUX CENT MILLE</c>); <c>million</c> is a noun, so it stays
/// (<c>DEUX CENTS MILLIONS</c>). Spanish needs the same distinction in the other
/// direction: the apocope in <c>UN MILLÓN</c> is the same rule as in <c>UN EURO</c>,
/// because both are nouns.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScaleKind
{
    /// <summary>
    /// The scale word behaves as a numeral adjective (French <c>mille</c>, Spanish <c>mil</c>).
    /// This is the default when a locale does not say otherwise.
    /// </summary>
    Adjective = 0,

    /// <summary>
    /// The scale word behaves as a noun (French <c>million</c>, Spanish <c>millón</c>).
    /// </summary>
    Noun = 1,
}

/// <summary>
/// The number words of a locale. <see cref="Ones"/>, <see cref="Tens"/> and
/// <see cref="Hundreds"/> are indexed by digit and must contain exactly ten entries,
/// with index zero left empty.
/// </summary>
public class NumbersModel
{
    /// <summary>
    /// Gets or sets the words for the ones digit. Index 0 must be empty.
    /// </summary>
    [JsonPropertyName("ones")]
    public string[] Ones { get; set; } = [];

    /// <summary>
    /// Gets or sets the words for the tens digit. Index 0 must be empty.
    /// </summary>
    [JsonPropertyName("tens")]
    public string[] Tens { get; set; } = [];

    /// <summary>
    /// Gets or sets the words for the hundreds digit. Index 0 must be empty.
    /// </summary>
    [JsonPropertyName("hundreds")]
    public string[] Hundreds { get; set; } = [];

    /// <summary>
    /// Gets or sets the hundreds words used when the last two digits are zero.
    /// Optional; entries left empty fall back to <see cref="Hundreds"/>.
    /// </summary>
    /// <remarks>
    /// Spanish needs this for <c>100 = CIEN</c> versus <c>101 = CIENTO UNO</c>, and French
    /// for <c>200 = DEUX CENTS</c> versus <c>250 = DEUX CENT CINQUANTE</c>. When set it must
    /// contain exactly ten entries, but only the ones that actually differ need a value —
    /// Spanish fills index 1 and leaves the rest empty. See
    /// <see cref="SettingsModel.UseExactHundredsBeforeScale"/> for how it interacts with
    /// scale words.
    /// </remarks>
    [JsonPropertyName("exactHundreds")]
    public string[]? ExactHundreds { get; set; }

    /// <summary>
    /// Gets or sets the scale words: index 0 is the units group, index 1 the thousands
    /// group, and so on. The array length caps the largest convertible number at
    /// 10^(3 × length) − 1. Index 0 must be empty; every other entry must have a value.
    /// </summary>
    [JsonPropertyName("scales")]
    public string[] Scales { get; set; } = [];

    /// <summary>
    /// Gets or sets the plural scale words, used when the group preceding the scale is
    /// greater than one (French <c>DEUX MILLIONS</c>, Spanish <c>DOS MILLONES</c>).
    /// Optional; entries left empty fall back to <see cref="Scales"/>. When set it must
    /// have the same length as <see cref="Scales"/>.
    /// </summary>
    [JsonPropertyName("scalesPlural")]
    public string[]? ScalesPlural { get; set; }

    /// <summary>
    /// Gets or sets the grammatical class of each scale word, parallel to
    /// <see cref="Scales"/>. Optional; every scale is treated as
    /// <see cref="ScaleKind.Adjective"/> when omitted. When set it must have the same
    /// length as <see cref="Scales"/>.
    /// </summary>
    [JsonPropertyName("scaleKinds")]
    public ScaleKind[]? ScaleKinds { get; set; }

    /// <summary>
    /// Gets or sets scale words per grammatical number, for locales whose plural rule has
    /// more than two forms. Each array is parallel to <see cref="Scales"/>; empty entries
    /// fall back to <see cref="ScalesPlural"/> and then to <see cref="Scales"/>.
    /// </summary>
    /// <remarks>
    /// Russian needs three: ТЫСЯЧА, ТЫСЯЧИ, ТЫСЯЧ. A two-form locale has no reason to set
    /// this — <see cref="Scales"/> and <see cref="ScalesPlural"/> say the same thing with
    /// less ceremony.
    /// </remarks>
    [JsonPropertyName("scaleForms")]
    public Dictionary<PluralCategory, string[]>? ScaleForms { get; set; }
}
