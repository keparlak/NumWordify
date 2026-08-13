using System.Text.Json.Serialization;

namespace NumWordify.Models;

/// <summary>
/// The grammatical number a count selects. The names are CLDR's, so a locale author can
/// look the rules up rather than reverse-engineer them.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PluralCategory
{
    /// <summary>The form used when nothing more specific applies. English "DOLLARS".</summary>
    Other = 0,

    /// <summary>The singular. English "DOLLAR", Russian РУБЛЬ (1, 21, 31 …).</summary>
    One = 1,

    /// <summary>The paucal. Russian РУБЛЯ (2–4, 22–24 …). Unused by <see cref="PluralRule.OneOther"/>.</summary>
    Few = 2,

    /// <summary>The plural proper. Russian РУБЛЕЙ (0, 5–20, 25–30 …).</summary>
    Many = 3,
}

/// <summary>
/// How a count maps to a <see cref="PluralCategory"/>. Named after the language family
/// rather than one language, because the rules are shared: East Slavic covers Russian,
/// Ukrainian and Belarusian.
/// </summary>
/// <remarks>
/// A rule is code rather than data on purpose. Expressing "n % 10 == 1 and n % 100 != 11"
/// in JSON would make the locale file a small programming language, which cannot be
/// validated up front the way the rest of the schema is — and the validator's guarantee
/// that a model which passes can never fail mid-conversion is the thing worth protecting.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PluralRule
{
    /// <summary>
    /// Two forms: <see cref="PluralCategory.One"/> for exactly one, otherwise
    /// <see cref="PluralCategory.Other"/>. English, Turkish, French, Spanish, Portuguese.
    /// </summary>
    OneOther = 0,

    /// <summary>
    /// Three forms, selected on the last digit and the last two digits. Russian,
    /// Ukrainian, Belarusian.
    /// </summary>
    EastSlavic = 1,
}
