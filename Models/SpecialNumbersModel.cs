using System.Text.Json.Serialization;

namespace NumWordify.Models;

/// <summary>
/// Irregular number words that cannot be built from the ones/tens/hundreds arrays.
/// </summary>
public class SpecialNumbersModel
{
    /// <summary>
    /// Gets or sets the words for 11 through 19, in order. When supplied it must contain
    /// exactly nine entries.
    /// </summary>
    [JsonPropertyName("teens")]
    public string[]? Teens { get; set; }

    /// <summary>
    /// Gets or sets whole-word overrides for the last two digits of a group, keyed by the
    /// value 1–99.
    /// </summary>
    /// <remarks>
    /// This is how vigesimal and fused forms are expressed: French <c>71 = SOIXANTE ET ONZE</c>
    /// and <c>80 = QUATRE-VINGTS</c>, Spanish <c>21 = VEINTIUNO</c>. An entry here wins over
    /// <see cref="Teens"/> and over regular construction. Key 0 is never consulted, because a
    /// group whose last two digits are zero contributes no tens/ones word at all.
    /// </remarks>
    [JsonPropertyName("special")]
    public Dictionary<int, string>? Special { get; set; }

    /// <summary>
    /// Gets or sets overrides that apply only when the group is followed by another word,
    /// keyed by the value 1–99.
    /// </summary>
    /// <remarks>
    /// Spanish apocopates before a scale word: <c>1.000.000 = UN MILLÓN</c> (not <c>UNO</c>)
    /// and French <c>80.000 = QUATRE-VINGT MILLE</c> (not <c>QUATRE-VINGTS</c>). Overrides
    /// always apply in front of an <see cref="ScaleKind.Adjective"/> scale word; set
    /// <see cref="SettingsModel.ApocopateBeforeNoun"/> to extend them to nouns — a
    /// <see cref="ScaleKind.Noun"/> scale word or a currency name. Checked before
    /// <see cref="Special"/>, but after <see cref="SettingsModel.SkipOneForThousand"/>.
    /// </remarks>
    [JsonPropertyName("specialBeforeScale")]
    public Dictionary<int, string>? SpecialBeforeScale { get; set; }

    /// <summary>
    /// Gets or sets the separator placed between the tens and ones word
    /// (<c>"-"</c> for English "TWENTY-ONE", <c>" Y "</c> for Spanish "TREINTA Y UNO").
    /// Defaults to a single space.
    /// </summary>
    [JsonPropertyName("compoundSeparator")]
    public string CompoundSeparator { get; set; } = " ";

    /// <summary>
    /// Gets or sets numeral forms that agree with the gender of the word they stand in
    /// front of, keyed by gender and then by the last two digits of the group.
    /// </summary>
    /// <remarks>
    /// Russian inflects one and two: <c>ОДНА ТЫСЯЧА</c> and <c>ДВЕ ТЫСЯЧИ</c> because the
    /// thousand is feminine, but <c>ОДИН МИЛЛИОН</c> and <c>ДВА МИЛЛИОНА</c> because the
    /// million is not. Consulted before <see cref="SpecialBeforeScale"/>, and only when
    /// the following word has a declared gender — so a locale that sets neither
    /// <see cref="NumbersModel.ScaleGenders"/> nor a currency gender is unaffected.
    /// </remarks>
    [JsonPropertyName("byGender")]
    public Dictionary<Gender, Dictionary<int, string>>? ByGender { get; set; }
}
