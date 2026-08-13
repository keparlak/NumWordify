# One way to name a word form

A design note for 3.0. **Not scheduled.** It records what six releases of measurement
showed, what the fix looks like, and the condition that makes it worth breaking the schema
for — so the next person does not have to re-derive any of it.

## What was measured

Two reviews of 2.0.0 proposed replacing the converter's branching with a rule engine. A
third measured the code instead — roughly 200 lines of algorithm, maximum nesting 4, no
method over 51 lines, and the rule-chain pattern the reviewers wanted already present in
`TensAndOnesWord` — and argued the real constraint was the expressiveness of the
localization schema. It proposed a test: add languages and count what they cost.

| Language | New concepts | Notes |
| --- | --- | --- |
| `pt-PT` (2.2.0) | 2 | `hundredsSeparator`, `finalGroupSeparator` |
| `ru-RU` (2.3.0) | 6 | plural categories and gender agreement, across 7 schema fields |
| `de-DE` (2.4.0) | 2 | `onesBeforeTens`, `adjectiveScaleSeparator` — and gender agreement reused unchanged |

With only the first two rows this looked like an accelerating curve. The third says
otherwise: **Russian is the outlier, not the trend.** It needed two whole mechanisms
because it is the first locale with more than two grammatical numbers; German needed two
settings and then reused Russian's gender agreement without touching it — `EINE MILLION`
is exactly what `ОДНА ТЫСЯЧА` was. That is the "these are grammatical concepts, not
per-language switches" hypothesis holding, on the second language to test it.

So the finding is not that the schema grows quickly. It is the **direction**: every
concept was *added*, and none replaced anything, because the fields they generalise are
published API.

A prediction attached to that review — that a plural-category selector would subsume
`scalesPlural`, `majorSingular` and `minorSingular` and thereby *delete* complexity — is
half right, and the failing half is the one that matters:

- It subsumes them semantically. `pluralRule: OneOther` reproduces their behaviour exactly,
  which is why adding the mechanism moved not one byte of output in five locales.
- It deletes nothing. Measured at the time: **+63 / −3 lines**, plus a new file.

A second prediction from the same review, that French `d'euros` elision would fall out of
the mechanism, is simply wrong: elision follows the sound of the following word, not the
count.

## The actual defect in the schema

There are two ways to say "this word has more than one form", and they disagree with each
other about which form is the base.

| | Base field | Exception field |
| --- | --- | --- |
| Scale words | `scales` — the **singular** | `scalesPlural` |
| Currency units | `major` — the **plural** | `majorSingular` |

So a locale author learns the convention twice, backwards the second time. `scaleForms`
and `majorForms`, added in 2.3.0, then sit on top of both as a third way.

## What 3.0 would do

One convention everywhere: **the base field carries the general form, and a map lists the
exceptions.**

```jsonc
// before — three mechanisms, two conventions
"scales":       ["", "MILLION"],          // singular is the base
"scalesPlural": ["", "MILLIONS"],
"major": "EUROS", "majorSingular": "EURO" // plural is the base

// after — one mechanism, one convention
"scales": ["", "MILLIONS"],               // the general form is always the base
"scaleForms": { "One": ["", "MILLION"] },
"major": "EUROS",
"majorForms": { "One": "EURO" }
```

Removed: `numbers.scalesPlural`, `currencies.*.majorSingular`, `currencies.*.minorSingular`
— three public properties and the JSON keys behind them. `scales`, `major` and `minor`
stay, so the common case in a locale file is unchanged; only the exceptions move.

Deleted from the converter: the three-level fallback in `ScaleWord` (19 lines) and in
`Inflect` (9 lines) collapse to one map lookup with a required base. Both stop having a
special case for "one" that only exists because two-form languages were the only ones the
schema could describe.

Affected resources: `en-US` 6 keys, `es-ES` 7, `fr-FR` 7, `pt-PT` 7, `ru-RU` 5. `tr-TR`
has none — it does not inflect.

## Why not now

Nothing about it is urgent, and three things argue against doing it soon.

1. **No new capability.** Every number this library converts today it would convert
   identically afterwards. A major version that a consumer must migrate for and gets
   nothing from is a tax.
2. **The 2.x line is hours old.** Six releases shipped in one day and no consumer has used
   any of them. Breaking the schema now would break something nobody has built yet, which
   is not a saving — it is churn with the same cost and none of the information.
3. **The right moment is when the fields have to change anyway.** Doing it then makes the
   migration one step instead of two.

## The trigger

Do it when a language needs a form the current shape cannot hold, because that forces a
schema change regardless. The nearest one is known: **Arabic** needs a `Two` plural
category for the dual, which means a new `pluralRule` family and a new `PluralCategory`
member. At that point the fields are being edited anyway, and consolidating is nearly
free.

Two smaller items were queued behind the same decision. Both are now settled:

- Collapsing the converter's "what follows this group" derivations into one concept —
  **done**. 2.3.0's gender work made it four parallel answers rather than three, which
  settled the "do both at once or neither" question by making it one job. Safe because
  2.1.0 validated `scaleKinds` values, so the counterexample that rejected the refactor
  during the 2.0.1 review is now unrepresentable.
- Whether `net6.0` and `net7.0` stay in `TargetFrameworks` — **they stay**. Both are out
  of support upstream, but supporting consumers who have not moved is a stated value of
  this project, not an oversight to be tidied away. That settles the question the other
  way round from the usual reflex, and it has a consequence: a framework that ships must
  be executed, not merely compiled, so the test suite now runs one leg per target
  framework.

**This constrains 3.0.** Dropping a target framework is off the table for the same reason.
A consolidation that breaks a locale file is a migration a maintainer performs once, with
a mechanical diff and an empty-snapshot check to prove it; dropping a framework is a
migration every consumer performs, for nothing they asked for. The two are not comparable
and should not be bundled just because both are labelled "breaking".

## If it is done anyway

The migration is mechanical and the safety net already exists. Change the resources, then
re-approve the golden snapshots with `NUMWORDIFY_APPROVE=1` and read the diff — it must be
**empty**. A correct consolidation changes no output at all, in any of the six locales, at
any of the ~1050 values each snapshot covers. That check is the whole test plan.
