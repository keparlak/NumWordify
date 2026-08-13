using System.Text.Json.Serialization;

namespace NumWordify.Models;

/// <summary>
/// How the fractional part of a number is read out by
/// <see cref="Converters.INumberToWordsConverter.ConvertWithoutCurrency(decimal)"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DecimalReadingMode
{
    /// <summary>
    /// Read the fraction as a whole number of <see cref="SettingsModel.DecimalPlaces"/> digits,
    /// the way money is read: <c>1.5</c> becomes "ONE POINT FIFTY" (fifty hundredths).
    /// This is the default and matches how the currency format reads minor units.
    /// </summary>
    Fraction = 0,

    /// <summary>
    /// Read the fraction digit by digit, the way a plain number is normally spoken:
    /// <c>1.5</c> becomes "ONE POINT FIVE" and <c>1.25</c> becomes "ONE POINT TWO FIVE".
    /// Digits beyond <see cref="SettingsModel.DecimalPlaces"/> are still rounded away.
    /// </summary>
    Digits = 1,
}

/// <summary>
/// Formatting rules for a locale.
/// </summary>
public class SettingsModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the word for "one" is dropped in front of the
    /// thousands scale word (Turkish <c>BİN</c> rather than <c>BİR BİN</c>, French <c>MILLE</c>
    /// rather than <c>UN MILLE</c>). Only affects the thousands group.
    /// </summary>
    [JsonPropertyName("skipOneForThousand")]
    public bool SkipOneForThousand { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether special words are used for 11–19.
    /// </summary>
    /// <remarks>
    /// Leave unset (the default) to decide automatically: teens are used whenever
    /// <see cref="SpecialNumbersModel.Teens"/> is supplied. Set it to <see langword="false"/>
    /// to force regular construction even though teens are present.
    /// </remarks>
    [JsonPropertyName("useTeens")]
    public bool? UseTeens { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="NumbersModel.ExactHundreds"/> also
    /// applies to groups followed by a scale word. Defaults to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// Only applies to <see cref="ScaleKind.Adjective"/> scale words; in front of a
    /// <see cref="ScaleKind.Noun"/> the exact form is always used, because the numeral
    /// phrase ends there. Spanish keeps the exact form everywhere (<c>CIEN MIL</c>), so it
    /// leaves this on. French drops the plural before a numeral adjective (<c>DEUX CENTS</c>
    /// but <c>DEUX CENT MILLE</c>), so it sets this to <see langword="false"/> — and still
    /// gets <c>DEUX CENTS MILLIONS</c>.
    /// </remarks>
    [JsonPropertyName("useExactHundredsBeforeScale")]
    public bool UseExactHundredsBeforeScale { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="SpecialNumbersModel.SpecialBeforeScale"/>
    /// also applies in front of a noun — a scale word of kind <see cref="ScaleKind.Noun"/>,
    /// or a currency name. Defaults to <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// Spanish apocopates in front of any noun: <c>1 = UNO</c> on its own but
    /// <c>UN MILLÓN</c> and <c>UN EURO</c>, <c>21 = VEINTIUNO</c> but
    /// <c>VEINTIÚN MILLONES</c> and <c>VEINTIÚN EUROS</c>. French does not, which is what
    /// keeps <c>QUATRE-VINGTS MILLIONS</c> plural. Overrides in front of an
    /// <see cref="ScaleKind.Adjective"/> scale word apply regardless of this setting.
    /// </remarks>
    [JsonPropertyName("apocopateBeforeNoun")]
    public bool ApocopateBeforeNoun { get; set; }

    /// <summary>
    /// Gets or sets a word inserted between the number and the currency name when the
    /// number ends in a <see cref="ScaleKind.Noun"/> scale word. Optional.
    /// </summary>
    /// <remarks>
    /// Spanish requires it: <c>UN MILLÓN DE EUROS</c>, but <c>UN MILLÓN QUINIENTOS MIL
    /// EUROS</c> without it, because there the number ends in <c>MIL</c>.
    /// </remarks>
    [JsonPropertyName("nounScaleLinkWord")]
    public string? NounScaleLinkWord { get; set; }

    /// <summary>
    /// Gets or sets how a count selects a grammatical number. Defaults to
    /// <see cref="Models.PluralRule.OneOther"/>, which is what English, Turkish, French,
    /// Spanish and Portuguese use.
    /// </summary>
    [JsonPropertyName("pluralRule")]
    public PluralRule PluralRule { get; set; } = PluralRule.OneOther;

    /// <summary>
    /// Gets or sets what goes between the hundreds word and the rest of the same group.
    /// Defaults to a single space.
    /// </summary>
    /// <remarks>
    /// Portuguese joins them with a conjunction — <c>CENTO E VINTE</c>, and
    /// <c>DUZENTOS E TRINTA E QUATRO</c> where <see cref="SpecialNumbersModel.CompoundSeparator"/>
    /// supplies the second <c>E</c>. Spanish, which uses <c>Y</c> between tens and ones,
    /// does not: <c>CIENTO VEINTE</c>. The two positions are separate rules, so they are
    /// separate settings.
    /// </remarks>
    [JsonPropertyName("hundredsSeparator")]
    public string HundredsSeparator { get; set; } = " ";

    /// <summary>
    /// Gets or sets what goes in front of the last group of a number when that group is a
    /// single term — below one hundred, or a whole number of hundreds. Optional; a plain
    /// space is used when it is unset, and whenever the last group is not a single term.
    /// </summary>
    /// <remarks>
    /// This is the one place where the separator depends on the value rather than on the
    /// locale alone. Portuguese requires it: <c>MIL E OITOCENTOS</c> (1800) and
    /// <c>MIL E VINTE E DOIS</c> (1022), but <c>MIL OITOCENTOS E NOVENTA E DOIS</c> (1892),
    /// where the last group is not a single term. Cunha and Cintra, <i>Nova Gramática do
    /// Português Contemporâneo</i> (1984, p. 372).
    /// </remarks>
    [JsonPropertyName("finalGroupSeparator")]
    public string? FinalGroupSeparator { get; set; }

    /// <summary>
    /// Gets or sets the word placed in front of negative numbers.
    /// </summary>
    [JsonPropertyName("negativeWord")]
    public string NegativeWord { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the word used for zero.
    /// </summary>
    [JsonPropertyName("zeroWord")]
    public string ZeroWord { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template used by <c>Convert</c>. Supports the placeholders
    /// <c>{whole}</c>, <c>{major}</c>, <c>{decimal}</c> and <c>{minor}</c>.
    /// </summary>
    [JsonPropertyName("currencyFormat")]
    public string CurrencyFormat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template used by <c>ConvertWithoutCurrency</c>. Supports the
    /// placeholders <c>{whole}</c> and <c>{decimal}</c>.
    /// </summary>
    [JsonPropertyName("numberFormat")]
    public string NumberFormat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how many fractional digits are kept. Defaults to 2 (minor currency
    /// units). Use 0 for currencies without a minor unit and 3 for currencies such as
    /// the Tunisian dinar. Must be between 0 and 6.
    /// </summary>
    [JsonPropertyName("decimalPlaces")]
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// Gets or sets how the fractional part is read by <c>ConvertWithoutCurrency</c>.
    /// Defaults to <see cref="DecimalReadingMode.Fraction"/>. The currency format always
    /// reads the fraction as minor units regardless of this setting.
    /// </summary>
    [JsonPropertyName("decimalReading")]
    public DecimalReadingMode DecimalReading { get; set; } = DecimalReadingMode.Fraction;
}
