# Changelog

All notable changes to this project are documented in this file.
This project follows [Semantic Versioning](https://semver.org/).

## [2.4.0] - 2026-08-14

German, and the two settings it needed. Output for the six existing locales is unchanged.

### Added

- **`de-DE`**, the seventh shipped locale. Default currency EUR; also defines CHF and USD.
  Range 10^18 - 1.
- **`settings.onesBeforeTens`.** German reads the last two digits backwards:
  `EINUNDZWANZIG`, "one and twenty". Only the order changes; the separator is still
  `specialNumbers.compoundSeparator`.
- **`settings.adjectiveScaleSeparator`.** German writes everything below a million as one
  word and everything above it separately - `EINHUNDERTZWANZIGTAUSENDVIERHUNDERTNEUNZEHN`,
  but `ZWEI MILLIONEN` with a space. That is the same split this library already calls
  adjective and noun, so the setting applies only to adjective scale words; a noun always
  takes a space.

### Fixed

- The README listed inverted word order, "such as German", as a limitation of the model.
  It is not one any more.

### Note on the architecture question

2.2.0 measured Portuguese at two new concepts and 2.3.0 measured Russian at six. German
cost two, and reused gender agreement unchanged - `EINE MILLION` needs exactly what
`ОДНА ТЫСЯЧА` needed. Three points rather than two, and the curve is not the one the
second point suggested: Russian is the outlier, not the trend. The design note is updated
accordingly.

## [2.3.0] - 2026-08-13

Russian, and the two mechanisms it needed. Output for the five existing locales is
unchanged — their approved snapshots are byte-identical.

### Added

- **`ru-RU`**, the sixth shipped locale. Default currency RUB; also defines USD and EUR.
- **Grammatical number beyond singular and plural.** `settings.pluralRule` selects the
  family (`OneOther`, the default, or `EastSlavic` for Russian, Ukrainian and Belarusian),
  and `numbers.scaleForms` plus `currencies.*.majorForms`/`minorForms` carry the words:
  `ОДИН РУБЛЬ`, `ДВА РУБЛЯ`, `ПЯТЬ РУБЛЕЙ`. A rule is code rather than data on purpose —
  `n % 10 == 1 and n % 100 != 11` expressed in JSON would make the locale file a small
  programming language, which cannot be validated up front the way the rest of the schema
  is.
- **Gender agreement.** A numeral agrees with the word after it, which is a scale word or
  a currency unit. `numbers.scaleGenders` and `currencies.*.majorGender`/`minorGender`
  declare the gender; `specialNumbers.byGender` supplies the forms. Russian needs it in
  both places at once: `ОДИН РУБЛЬ ОДНА КОПЕЙКА` — the rouble is masculine, the kopeck is
  feminine, and the same digit reads differently in the same sentence.

### Changed

- `settings.pluralRule` is honoured wherever a word was previously chosen by
  "equals one or not". `OneOther` reproduces that exactly, which is why nothing changed
  for the other five locales.

### Known limitations

- Arabic-style duals are still not expressible: there is no `Two` category. Adding a
  plural-rule family is a code change by design.
- French `d'euros` elision is still not expressible, and plural categories did not help.
  A review had predicted they would; they do not, because elision depends on the sound of
  the following word rather than on the count.

### Note on the architecture question

2.2.0 recorded that Portuguese cost two new concepts. Russian cost six, of which the
schema carries seven new fields. More importantly the growth is one-way: `scalesPlural`,
`majorSingular` and `minorSingular` are now special cases of the general mechanism and
could be expressed through it, but they are published API and stay. The measurement is
`+63 / -3` lines for the plural half alone. The next language should be designed against
a model that can drop the old fields, which means a major version.

## [2.2.0] - 2026-08-13

European Portuguese, and the two schema concepts it turned out to need. Output for the
four existing locales is unchanged — their approved snapshots are byte-identical, which
is the point of having them.

### Added

- **`pt-PT`**, the fifth shipped locale. Default currency EUR; also defines USD and BRL.
  Covers `CEM`/`CENTO`, the teens, plural scale words, and `de` before a currency name —
  the last of those reusing the setting Spanish already had, unchanged.
- **`settings.hundredsSeparator`** (defaults to a space): what goes between the hundreds
  word and the rest of the same group. Portuguese reads `CENTO E VINTE`; Spanish, which
  uses `Y` between tens and ones, reads `CIENTO VEINTE`. Two positions, two rules, two
  settings.
- **`settings.finalGroupSeparator`** (optional): what goes in front of the last group when
  that group is a single term — below one hundred, or a whole number of hundreds. This is
  the first setting whose effect depends on the *value* rather than on the locale alone.
  Portuguese requires exactly that distinction: `MIL E OITOCENTOS` (1800) and
  `MIL E VINTE E DOIS` (1022), but `MIL OITOCENTOS E NOVENTA E DOIS` (1892). The rule is
  Cunha and Cintra, *Nova Gramática do Português Contemporâneo* (1984, p. 372).

### Changed

- The golden snapshots record a locale's ceiling instead of assuming every locale shares
  one. Values a locale cannot express are written as `<out of range>` rather than dropped
  from the ladder, so a changed ceiling shows up as a reviewable diff. The same assumption
  is removed from `EmbeddedResourceTests`, which now asserts a floor every locale must
  reach (10^9 − 1) and allows a documented refusal above it.

### Known limitations

- `pt-PT` stops at 10^9 − 1. European Portuguese reads that value as *mil milhões* — two
  words, with the "um" dropped — and the scale table holds one word per step with no way
  to drop it, so defining it would produce "UM MIL MILHÕES" for 10^9 itself. Left
  undefined rather than made wrong.

### Note on the architecture question

Two earlier reviews proposed replacing the converter's branching with a rule engine; the
third measured the code and argued the real constraint was schema expressiveness, and
proposed a test: add a fifth language and count what it costs. Portuguese cost two new
concepts, and the second is not a boolean — it is a rule that depends on the value being
converted. A flag table cannot hold that shape. The count is at the threshold that was set
in advance, so the question is open again rather than settled, and the next language
should be treated as evidence rather than as a chore.

## [2.1.0] - 2026-08-13

Three validation gaps, each of which let a custom localization construct successfully and
then misbehave later. Nothing changes for the bundled locales, and conversion output is
unchanged. This is a minor rather than a patch because two of the rules reject models that
2.0.x accepted.

### Added

- **`settings.decimalPlaces` is now checked against `numbers.scales`.** The fraction is
  read as a number in its own right, through the same routine as the whole part, so it
  needs one scale word per three decimal places. A locale with `decimalPlaces: 6` and a
  single scale entry used to construct fine and then fail on `1.123456` with
  `NumberOutOfRangeException: The number 1.123456 is too large for this locale` — blaming
  a whole part of 1. The mistake is in the model, so it is now reported against the model.
- **`numbers.scaleKinds` entries are now checked against the enum.** A JSON resource could
  never carry an undefined kind, because the string converter rejects an unknown name, but
  a model built in C# could: an enum is only an int. The conversion loop asks "noun or
  not", so `(ScaleKind)7` silently read as an adjective and produced grammar the locale
  never asked for. This was found while reviewing a proposed refactor whose correctness
  depended on it — see below.

### Changed

- `LocalizationLoader.Load` takes the embedded resource name rather than rebuilding it
  from a culture name. Both call sites already held the real name, and reconstructing it
  as `culture + ".json"` worked only because every shipped resource happens to be
  lowercase — the manifest lookup is case-sensitive, so a single mis-cased file would have
  keyed the cache differently from the two paths that read it and thrown for every caller.
  No behaviour change on the shipped set.

### Not in this release

- Collapsing the converter's three "what follows this group" derivations into one enum.
  The `scaleKinds` rule above was its blocker: with an undefined kind the refactor changed
  the words produced. That is now unrepresentable, so the refactor is unblocked — but it
  is a change to the conversion core with no user-visible benefit, and it should not ride
  along in the same diff as new validation rules.

## [2.0.1] - 2026-08-13

Two defects a consumer of 2.0.0 can reach through the public API. Conversion output is
unchanged — every one of the 296 tests that shipped with 2.0.0 still produces the same
words.

### Fixed

- **Exception messages printed numbers in the machine's culture, so the same failure read
  differently on different machines.** `NumberOutOfRangeException` interpolated the
  offending value directly: a Turkish machine reported `1000000000000000000,5`, a Swedish
  one wrote the minus sign as U+2212 and an Arabic one used U+066B as the separator. A
  diagnostic is not localized output. Two validation messages that can interpolate a
  negative `decimalPlaces` or override key are fixed the same way. Conversion output was
  never affected; the library does no culture-sensitive formatting on that path, and a
  new test now pins that rather than leaving it true by luck.
- **`SupportedCultures` handed out the library's own cached array, so any caller could
  permanently corrupt it.** `NumberToWordsConverter.SupportedCultures` and
  `LocalizationNotFoundException.AvailableCultures` returned the live `string[]` behind a
  process-wide cache. Casting the declared `IReadOnlyList<string>` back to `string[]` and
  writing through it rewrote the supported-culture listing *and* the text of every
  subsequent "localization not found" message, for the life of the process. Both are now
  genuinely read-only.

### Changed

- `NumberToWordsConverter.SupportedCultures` and
  `LocalizationNotFoundException.AvailableCultures` still declare `IReadOnlyList<string>`,
  but their runtime type is now `ReadOnlyCollection<string>` rather than `string[]`. Code
  that cast the result to `string[]` compiled against 2.0.0 and will now throw
  `InvalidCastException`. That cast was never supported and was the defect above, but the
  change is invisible to the compiler and to package validation, so it is called out here
  rather than shipped silently.
- The build now validates the public API against the published 2.0.0 package
  (`PackageValidationBaselineVersion`), so an accidental break fails the build instead of
  reaching a consumer.

### Not in this release

Deliberately held back, so the next one has a record of why:

- A validator rule cross-checking `settings.decimalPlaces` against `numbers.scales.Length`
  would reject custom localizations that 2.0.0 accepted. Rejecting previously valid input
  is minor-version behaviour — held for 2.1.0.
- Replacing the converter's three "what follows this group" derivations with a single enum
  was proposed and rejected during review: with a `scaleKinds` entry outside the declared
  enum values — which the validator does not currently reject — the refactor changes the
  words produced. Held until the validator checks those values.
- The SDK pin in `global.json`, the GitHub Actions major bumps and .NET 6/7 test legs are
  build-only, are not part of the package, and land on `master` without a release.

## [2.0.0] - 2026-08-12

A correctness release. Three defects made the library produce silently wrong words, and
because nothing verified the output they shipped in 1.0.0. Conversion output changes for
four of the five bundled locales, so this is a major version even though most of it is
bug fixes. See "Migrating from 1.x" in the README before upgrading.

### Fixed

- **The ones digit was dropped whenever a tens digit was present.** `useCompoundNumbers`
  gated whether the ones word was written at all, and the key was absent from `tr-TR`,
  `tr-TR-EUR`, `fr-FR` and `es-ES`, so it defaulted to off. Roughly four of every five
  values in those languages were wrong: `1234.56` in Turkish read
  "BİN İKİ YÜZ OTUZ TL ELLİ Kr" instead of "BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr".
  The ones digit is now always written; `specialNumbers.compoundSeparator` controls the
  separator, which is all the flag was ever meant to do.
- **`en-US` appended a stray "ZERO" to every value ending in two zeros.** A `"0": "ZERO"`
  entry in the special-numbers map was matched against the last two digits of each group,
  so `100` read "ONE HUNDRED ZERO USD ZERO Cents". A group whose last two digits are zero
  now contributes no tens/ones word at all, and the entry has been removed.
- **The converter was not thread safe.** The current scale lived in an instance field, so
  a shared instance produced wrong words and spurious "number is too large" failures under
  concurrency. Scale is now a local, making instances genuinely immutable.
- **Money rounded to even instead of away from zero**, turning `1.005` into one dollar and
  zero cents.
- **A value that rounded to zero kept its sign**: `-0.001` read "NEGATIVE ZERO ...".
- **A currency name containing a placeholder corrupted the output.** Templates were
  expanded with a chain of `string.Replace` calls, so a `Major` of `"{minor}"` was
  substituted twice. Expansion is now a single pass, and a placeholder that resolves to
  an empty string no longer leaves a double space.
- **`settings.skipOneForHundred` never did anything** — both branches of its condition
  produced the same word.
- **Supplying an incomplete `LocalizationModel` threw `NullReferenceException`** from
  inside the validator, and a `teens` array shorter than nine entries threw
  `IndexOutOfRangeException` mid-conversion. Both are now reported at construction time
  with a message naming the field.
- The original `JsonException` is now preserved as `InnerException` instead of being
  flattened into a string.
- **French dropped the plural of `cent` and `vingt` in front of `million` and `milliard`**,
  producing "DEUX CENT MILLIONS" where French requires "DEUX CENTS MILLIONS". The plural
  drops only before another numeral adjective, and `mille` is one while `million` is a
  noun — a distinction the locale now states through `numbers.scaleKinds`.
- **Spanish omitted the obligatory `de`** after a noun scale word: "UN MILLÓN EUROS"
  rather than "UN MILLÓN DE EUROS". Expressed through `settings.nounScaleLinkWord`, and
  correctly absent when the number ends in `MIL`.
- **A culture that resolved through language fallback borrowed the locale's default
  currency.** `ToWords("es-MX")` printed euros. It now throws
  `AmbiguousCurrencyException`; `ConvertWithoutCurrency` and an explicit currency both
  still work.
- **`WordifyOptions` silently ignored `CurrencyCode`** when `Localization` was also set,
  making the same mistake an error on one path and invisible on the other.
- A `null` entry in `scalesPlural` passed validation and threw `NullReferenceException`
  mid-conversion — and only for groups greater than one, so it hid at 1000 and appeared
  at 2000. An empty entry now means "fall back"; `null` is rejected.
- An empty entry in `scales` passed validation and silently dropped the scale word, so
  2000 and 2 produced the same words.
- A `CurrencyModel` passed to a constructor was never validated, so a `null` major unit
  rendered as an empty string instead of failing.
- `decimalPlaces: 0` combined with a `{decimal}` or `{minor}` placeholder appended the
  zero word to every amount. The combination is now rejected.
- A misspelled placeholder such as `{hole}` was printed literally. Format strings are now
  validated against the placeholders they support.
- `exactHundreds` required all ten entries, forcing Spanish to copy eight values out of
  `hundreds` and keep them in sync by hand. Empty entries now fall back.
- The blanket whitespace collapse that cleaned up after empty placeholders also rewrote
  separators a locale had chosen on purpose. An empty placeholder now takes its own
  preceding space with it instead.
- **The README's own recipe for guarding `CultureInfo.CurrentCulture` threw.**
  `IsCultureSupported` answers "do the number words resolve?", but converting with
  currency also requires the region to match, and the two differ for every culture that
  resolves by language fallback: the guard accepted 187 installed cultures while
  `ToWords` threw for 178 of them. `IsCultureSupported(culture, out bool currencyApplies)`
  now answers both questions, and the README uses it.
- **The test covering that recipe could not fail.** It ran the snippet against the ambient
  `CultureInfo.CurrentCulture` and asserted only that the result was non-empty, so it
  passed on every machine and CI runner in use while the recipe it documented was broken.
  It is parameterised over the machine culture now, including `en-GB` and `es-MX`.

### Added

- Test suite (`tests/NumWordify.Tests`): golden output tables per locale, concurrency
  tests, validation tests and range tests. Every README example is asserted, including
  the JSON schema block, which is parsed out of README.md rather than restated in C#.
- Full-output snapshots in `tests/NumWordify.Tests/Approvals`: every value from 0 to 1000
  plus a magnitude ladder, with and without currency, per locale. The hand-written tables
  stopped at two million, which is exactly where the French pluralisation bug lived.
  Re-approve with `NUMWORDIFY_APPROVE=1` and read the diff.
- CI workflow running build and tests on Linux and Windows for every push and pull
  request. Previously nothing was compiled before a release tag.
- `INumberToWordsConverter`, so consumers can substitute the converter in their own tests.
- `NumberToWordsConverter.SupportedCultures` and `IsCultureSupported`, so a culture can be
  probed without catching an exception. The `out bool currencyApplies` overload also
  answers whether the resolved locale's default currency applies to the requested culture,
  which is the question `Convert` actually asks.
- Localization caching. Each locale is parsed and validated once per process rather than
  on every call; `ToWords` was doing a full manifest scan and JSON parse per invocation.
- Per-locale currency maps and `ToWords(culture, "EUR")`, so one locale file can serve
  several currencies.
- Singular currency forms (`majorSingular`, `minorSingular`): `1.01` now reads
  "ONE DOLLAR ONE CENT".
- `settings.decimalPlaces` (0–6) for currencies without a minor unit or with three.
- `settings.decimalReading` to read the fraction digit by digit.
- `numbers.exactHundreds`, `numbers.scalesPlural`, `numbers.scaleKinds`,
  `specialNumbers.specialBeforeScale`, `settings.useExactHundredsBeforeScale`,
  `settings.apocopateBeforeNoun` and `settings.nounScaleLinkWord` — the rules French and
  Spanish need and a flat digit table cannot express.
- `LocalizationModel.DefaultCurrency`, the single place a locale names its default
  currency: a key into the `currencies` map rather than a second copy beside it.
- `LocalizationModel.Deprecated`, which keeps a redundant locale resolvable by name while
  removing it from `SupportedCultures` and from language fallback.
- `global.json` and a pinned `AnalysisLevel`, so a new SDK band cannot turn a release
  build red without a code change.
- `WordifyOptions`, covering the combinations the overload matrix does not, together with
  `NumberToWordsConverter(WordifyOptions)`. The options are now resolved in the converter
  itself, so culture-versus-localization and currency-versus-currency-code precedence is
  decided in one place instead of being restated in the extension methods.
- Tests that exercise `numbers.scaleKinds`, `settings.useExactHundredsBeforeScale`,
  `settings.apocopateBeforeNoun` and `settings.nounScaleLinkWord` one at a time on a
  synthetic locale. They were previously covered only through the two shipped locales that
  set them, always in the same combination.
- Exception hierarchy rooted at `NumWordifyException`.
- Analyzers, `.editorconfig` and `TreatWarningsAsErrors` across the build.

### Changed

- **French is now grammatically correct** for 21, 31–61, 70–99 (`SOIXANTE ET ONZE`,
  `QUATRE-VINGTS`, `QUATRE-VINGT-ONZE`), for the plural of `cent` and `vingt`
  (`DEUX CENTS` but `DEUX CENT CINQUANTE`), for `MILLE` rather than `UN MILLE`, and for
  plural scale words (`DEUX MILLIONS`). Previously every value from 11 to 19 read as
  "DIX".
- **Spanish is now grammatically correct** for 11–19, the fused twenties
  (`VEINTIUNO`…`VEINTINUEVE`), `CIEN` versus `CIENTO`, the `Y` separator, apocope before
  scale words and currency names (`UN MILLÓN`, `VEINTIÚN EUROS`, `UN EURO`), and plural
  scale words. Previously every value from 11 to 19 read as "DIEZ".
- `en-US` currency names are now `DOLLARS`/`CENTS` with singular forms, matching what the
  documentation always claimed, instead of `USD`/`Cents`.
- Culture lookup is case-insensitive and falls back within a language, so `"EN-US"`,
  `"en"` and `"en-GB"` all resolve to `en-US`.
- `NumberToWordsConverter` is `sealed`.
- `LocalizationModel.Numbers` is nullable, reflecting what was already true at runtime.
- `LocalizationModel(currency, numbers, settings)` files the currency in `Currencies`
  under `LocalizationModel.DefaultCurrencyKey` and points `DefaultCurrency` at it, so the
  convenience constructor builds the same shape a hand-written model would.
- `settings.useTeens` is `bool?`; unset means "use teens when they are supplied".
- The package version comes from the release tag. It was hardcoded to `1.0.0`, so every
  tag produced the same package and `--skip-duplicate` swallowed the push while the job
  reported success. `--skip-duplicate` has been removed.
- Publishing authenticates through NuGet Trusted Publishing (OIDC) instead of a stored
  API key. The workflow exchanges a signed, short-lived GitHub token for a key that lives
  one hour, so the repository holds no long-lived publishing credential. nuget.org is
  cutting new API keys to 30 days from 2026-08-17 and expiring every existing key on
  2026-11-01, which would otherwise have made this release a recurring chore.
- `System.Text.Json` is referenced only for the `netstandard2.0` target; the other targets
  have it in the box.
- `favicon.ico` is no longer packed into consuming projects, and the package icon dropped
  from 640 KB to 29 KB.
- Spanish now defines five scale words rather than six. The previous sixth, `BILLARDO`
  for 10^15, is not a word the RAE recognises; the convertible range stops at 10^15 − 1
  instead of inventing one. The other locales still reach 10^18 − 1.
- Documentation is now in English throughout, the .NET requirement is stated correctly
  (.NET 6.0+ or .NET Standard 2.0, not ".NET 9.0 or higher"), and the README carries a
  Known limitations section naming what the model still cannot express — French `d'`
  elision before a currency name, Spanish gender agreement, dual and paucal currency
  forms, and inverted word order such as German "einundzwanzig".

### Deprecated

- `tr-TR-EUR` is marked deprecated: excluded from `SupportedCultures` and from language
  fallback, resolvable only by exact name. Use `ToWords("tr-TR", "EUR")`, which a test
  proves produces identical output.

### Removed

- `settings.useCompoundNumbers` and `settings.skipOneForHundred`. Both stopped having any
  effect, and a major release is the window in which removing them is free — leaving them
  as `[Obsolete]` properties would have kept two do-nothing members in the public surface
  for the life of 2.x. No bundled locale set either key, and unmapped JSON keys are
  ignored on load, so a 1.x locale file still parses.
- `LocalizationModel.Currency`. A locale lists its currencies in `currencies` and names
  the default with `defaultCurrency`; there is no second way to spell it out, and no
  mutual-exclusion rule left to police. The single-currency constructor is unchanged.
- `FileNotFoundException` is no longer thrown for an unknown culture; catch
  `LocalizationNotFoundException` (or `NumWordifyException`) instead.
- The undocumented claim of caching support is gone from the README — replaced by actual
  caching.

## [1.0.0] - 2024-01-13

### Added

- `CultureInfo` support
- Special-number support (11–19)
- Compound number formatting (for example "twenty-one")
- Customizable separator support
- MIT license

### Changed

- Improved currency support (added a `CurrencyModel` parameter)
- Expanded README
- Improved project structure and documentation

### Fixed

- JSON deserialization issues
- Embedded resource loading issues
- Conversion errors for special numbers (11–19)
- Hyphen usage in compound number formatting

### Security

- Updated `.gitignore` to exclude sensitive files
