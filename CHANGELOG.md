# Changelog

All notable changes to this project are documented in this file.
This project follows [Semantic Versioning](https://semver.org/).

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
