using NumWordify.Extensions;
using NumWordify.Models;
using Xunit;

namespace NumWordify.Tests;

/// <summary>
/// Exercises each grammar setting on its own. The shipped locales only ever set these
/// flags in one fixed combination, so covering them through French and Spanish leaves
/// every other combination untested — and these four settings interact.
/// </summary>
public class GrammarSettingsTests
{
    [Fact]
    public void Exact_hundreds_always_apply_when_the_numeral_phrase_ends_there()
    {
        // No scale word follows, so the question the flag asks does not arise.
        var locale = TestLocalizations.GrammarProbe();
        locale.Settings.UseExactHundredsBeforeScale = false;

        Assert.Equal("TWO HUNDREDS POINT ZERO", 200m.ToWordsWithoutCurrency(locale));
    }

    [Theory]
    [InlineData(true, "TWO HUNDREDS THOUSAND POINT ZERO")]
    [InlineData(false, "TWO HUNDRED THOUSAND POINT ZERO")]
    public void Use_exact_hundreds_before_scale_decides_the_form_in_front_of_an_adjective_scale(
        bool useExactHundreds,
        string expected)
    {
        var locale = TestLocalizations.GrammarProbe();
        locale.Settings.UseExactHundredsBeforeScale = useExactHundreds;

        Assert.Equal(expected, 200_000m.ToWordsWithoutCurrency(locale));
    }

    [Fact]
    public void A_noun_scale_restores_the_exact_hundreds_form_the_flag_switched_off()
    {
        // The numeral phrase ends in front of a noun, so the flag no longer applies —
        // this is what keeps French "DEUX CENTS MILLIONS" plural while "DEUX CENT MILLE"
        // is not. Same locale, same flag, opposite result: only the scale kind changed.
        var locale = TestLocalizations.GrammarProbe([ScaleKind.Adjective, ScaleKind.Noun, ScaleKind.Noun]);
        locale.Settings.UseExactHundredsBeforeScale = false;

        Assert.Equal("TWO HUNDREDS THOUSAND POINT ZERO", 200_000m.ToWordsWithoutCurrency(locale));
    }

    [Theory]
    [InlineData(true, "A DOLLAR ZERO CENTS")]
    [InlineData(false, "ONE DOLLAR ZERO CENTS")]
    public void Apocopate_before_noun_decides_the_form_in_front_of_a_currency_name(
        bool apocopate,
        string expected)
    {
        var locale = TestLocalizations.GrammarProbe();
        locale.Settings.ApocopateBeforeNoun = apocopate;

        Assert.Equal(expected, 1m.ToWords(locale));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void An_override_in_front_of_an_adjective_scale_applies_whatever_apocopation_says(
        bool apocopate)
    {
        // Documented on SettingsModel.ApocopateBeforeNoun: the setting governs nouns only.
        var locale = TestLocalizations.GrammarProbe();
        locale.Settings.ApocopateBeforeNoun = apocopate;

        Assert.Equal("A THOUSAND DOLLARS ZERO CENTS", 1000m.ToWords(locale));
    }

    [Theory]
    [InlineData(true, "A THOUSAND DOLLARS ZERO CENTS")]
    [InlineData(false, "ONE THOUSAND DOLLARS ZERO CENTS")]
    public void Marking_a_scale_word_a_noun_hands_it_to_the_apocopation_setting(
        bool apocopate,
        string expected)
    {
        // The same 1000 as the test above, and the same override table. Declaring the
        // thousands scale a noun moves it from the always-apply side of the split to the
        // side ApocopateBeforeNoun controls.
        var locale = TestLocalizations.GrammarProbe([ScaleKind.Adjective, ScaleKind.Noun, ScaleKind.Noun]);
        locale.Settings.ApocopateBeforeNoun = apocopate;

        Assert.Equal(expected, 1000m.ToWords(locale));
    }

    [Fact]
    public void The_noun_scale_link_word_is_inserted_only_when_the_number_ends_in_a_noun_scale()
    {
        // "UN MILLÓN DE EUROS", but "UN MILLÓN QUINIENTOS MIL EUROS".
        var locale = TestLocalizations.GrammarProbe([ScaleKind.Adjective, ScaleKind.Noun, ScaleKind.Noun]);
        locale.Settings.NounScaleLinkWord = "OF";

        Assert.Equal("ONE THOUSAND OF DOLLARS ZERO CENTS", 1000m.ToWords(locale));
        Assert.Equal("ONE THOUSAND ONE DOLLARS ZERO CENTS", 1001m.ToWords(locale));
    }

    [Fact]
    public void The_noun_scale_link_word_is_left_out_when_no_currency_follows()
    {
        var locale = TestLocalizations.GrammarProbe([ScaleKind.Adjective, ScaleKind.Noun, ScaleKind.Noun]);
        locale.Settings.NounScaleLinkWord = "OF";

        Assert.Equal("ONE THOUSAND POINT ZERO", 1000m.ToWordsWithoutCurrency(locale));
    }

    [Fact]
    public void A_noun_scale_without_a_link_word_joins_the_currency_directly()
    {
        var locale = TestLocalizations.GrammarProbe([ScaleKind.Adjective, ScaleKind.Noun, ScaleKind.Noun]);

        Assert.Equal("ONE THOUSAND DOLLARS ZERO CENTS", 1000m.ToWords(locale));
    }

    [Fact]
    public void Omitting_scale_kinds_leaves_every_scale_an_adjective()
    {
        var implicitKinds = TestLocalizations.GrammarProbe();
        var explicitKinds = TestLocalizations.GrammarProbe(
            [ScaleKind.Adjective, ScaleKind.Adjective, ScaleKind.Adjective]);

        implicitKinds.Settings.NounScaleLinkWord = "OF";
        explicitKinds.Settings.NounScaleLinkWord = "OF";

        Assert.Equal(1000m.ToWords(explicitKinds), 1000m.ToWords(implicitKinds));
        Assert.Equal(200_000m.ToWordsWithoutCurrency(explicitKinds), 200_000m.ToWordsWithoutCurrency(implicitKinds));
    }
}
