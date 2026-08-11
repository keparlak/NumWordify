# NumWordify

NumWordify converts decimal numbers into words, with multi-language and multi-currency support.

Every example on this page is asserted by a test in `tests/NumWordify.Tests`, including the JSON schema block, which is parsed straight out of this file. The outputs shown here are the outputs you get.

## Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Precision and rounding](#precision-and-rounding)
- [Range](#range)
- [Error handling](#error-handling)
- [Supported cultures](#supported-cultures)
- [Known limitations](#known-limitations)
- [Custom localization](#custom-localization)
- [Adding a new language](#adding-a-new-language)
- [Schema reference](#schema-reference)
- [Migrating from 1.x](#migrating-from-1x)
- [Project structure](#project-structure)

## Features

- Numbers to words, with or without currency
- Four languages: English, Turkish, French, Spanish
- Per-locale currency maps (`"EUR"`, `"USD"`, …) plus arbitrary custom currencies
- Irregular number handling: teens, vigesimal forms (French 70–99), fused forms (Spanish 21–29), apocope (`UN MILLÓN`, `UN EURO`), plural scale words (`DEUX MILLIONS`), and the adjective/noun split that decides French `DEUX CENT MILLE` versus `DEUX CENTS MILLIONS`
- Currency names that agree in number (`ONE DOLLAR` / `TWO DOLLARS`)
- Configurable fractional precision (0–6 digits) with away-from-zero rounding
- Negative numbers and zero
- Immutable, thread-safe converters; localization files are parsed once and cached
- Fully extensible through JSON files or a localization model you build in code

## Installation

```bash
dotnet add package NumWordify
```

Targets `net9.0`, `net8.0`, `net7.0`, `net6.0` and `netstandard2.0`.

## Usage

### Basic usage

```csharp
using NumWordify.Extensions;
using NumWordify.Models;
using System.Globalization;

decimal amount = 1234.56M;

amount.ToWords("tr-TR");
// "BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr"

amount.ToWords(new CultureInfo("tr-TR"));
// "BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr"

amount.ToWordsWithoutCurrency("tr-TR");
// "BİN İKİ YÜZ OTUZ DÖRT NOKTA ELLİ ALTI"

11234.56M.ToWords("en-US");
// "ELEVEN THOUSAND TWO HUNDRED THIRTY-FOUR DOLLARS FIFTY-SIX CENTS"

1234.56M.ToWords("fr-FR");
// "MILLE DEUX CENT TRENTE-QUATRE EUROS CINQUANTE-SIX CENTIMES"

1234.56M.ToWords("es-ES");
// "MIL DOSCIENTOS TREINTA Y CUATRO EUROS CINCUENTA Y SEIS CÉNTIMOS"
```

Culture matching is case-insensitive and falls back within a language, so `"EN-US"`, `"en"` and `"en-GB"` all resolve to `en-US`.

### Currency

Pick one of the currencies the locale already names:

```csharp
1234.56M.ToWords("tr-TR", "EUR");
// "BİN İKİ YÜZ OTUZ DÖRT EURO ELLİ ALTI SENT"
```

Or supply your own:

```csharp
var currency = new CurrencyModel { Major = "EURO", Minor = "SENT" };
1234.56M.ToWords("tr-TR", currency);
// "BİN İKİ YÜZ OTUZ DÖRT EURO ELLİ ALTI SENT"
```

Set `MajorSingular` / `MinorSingular` when the language inflects the currency name:

```csharp
1.01M.ToWords("en-US");
// "ONE DOLLAR ONE CENT"

2.02M.ToWords("en-US");
// "TWO DOLLARS TWO CENTS"
```

### Cultures without their own localization

A culture that resolves through language fallback gets the right number words, but its currency is a different question — `es-MX` borrows Spanish number words, and the Spanish locale's default currency is the euro. Assuming it would be wrong in a way nobody notices, so it is refused:

```csharp
1M.ToWordsWithoutCurrency("es-MX");   // "UNO COMA CERO"
1M.ToWords("es-MX", "MXN");           // "UN PESO CERO CENTAVOS"
1M.ToWords("es-MX");                  // throws AmbiguousCurrencyException
```

A culture that names no region at all (`"es"`) is not contradicting the locale, so it takes the default currency.

### Checking a culture before using it

`CultureInfo.CurrentCulture` is whatever the machine is set to, and only four languages ship with the library:

```csharp
var culture = NumberToWordsConverter.IsCultureSupported(CultureInfo.CurrentCulture)
    ? CultureInfo.CurrentCulture.Name
    : "en-US";

amount.ToWords(culture);

NumberToWordsConverter.SupportedCultures;
// ["en-US", "es-ES", "fr-FR", "tr-TR"]
```

### Negative numbers and zero

```csharp
(-1234.56M).ToWords("tr-TR");
// "EKSİ BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr"

0M.ToWords("tr-TR");
// "SIFIR TL SIFIR Kr"

(-0.001M).ToWords("en-US");
// "ZERO DOLLARS ZERO CENTS"  — a value that rounds to zero is not reported as negative
```

### Options

The overloads cover the common combinations; `WordifyOptions` covers the rest and is the better fit when the settings come from configuration.

```csharp
var options = new WordifyOptions { Culture = "tr-TR", CurrencyCode = "USD" };

1M.ToWords(options);                 // "BİR DOLAR SIFIR SENT"
1M.ToWordsWithoutCurrency(options);  // "BİR NOKTA SIFIR"
```

### Reusing a converter

`NumberToWordsConverter` is immutable and safe to share across threads, so a single instance can be registered as a singleton:

```csharp
services.AddSingleton<INumberToWordsConverter>(_ => new NumberToWordsConverter("tr-TR"));
```

Localizations are parsed once per culture and cached for the process, so the extension methods are cheap too; reusing a converter is a small extra saving rather than a requirement.

If you pass your own `LocalizationModel`, the converter keeps a reference rather than a copy — do not mutate the model afterwards.

## Precision and rounding

- The fractional part is rounded to `settings.decimalPlaces` digits (2 by default) using `MidpointRounding.AwayFromZero`, which is what money expects: `1.005` becomes one dollar **one** cent, not zero cents.
- Digits beyond that precision are rounded away, not truncated: `1.234567` becomes twenty-three cents, `1.235` becomes twenty-four.
- A fraction that rounds up carries into the whole part: `0.999` becomes "ONE DOLLAR ZERO CENTS".
- `decimalPlaces` accepts 0 through 6. Use 0 for currencies without a minor unit, 3 for the Tunisian dinar. With 0, the format strings must not reference `{decimal}` or `{minor}` — validation enforces this, because they would print the zero word on every amount.

By default `ConvertWithoutCurrency` reads the fraction the way money is read, so `1.5` is "ONE POINT FIFTY" (fifty hundredths). Set `"decimalReading": "Digits"` in a locale to read it digit by digit instead — `1.5` becomes "ONE POINT FIVE" and `1.25` becomes "ONE POINT TWO FIVE".

## Range

The largest convertible value is determined by the number of scale words a locale defines:

| Locale | Scale words | Largest value |
| --- | --- | --- |
| `en-US`, `tr-TR`, `fr-FR` | 6 | 10^18 − 1 |
| `es-ES` | 5 | 10^15 − 1 |

Spanish stops one scale earlier because 10^15 has no single-word name — see [Known limitations](#known-limitations). Anything larger, including `decimal.MaxValue`, throws `NumberOutOfRangeException` rather than producing a wrong answer.

## Error handling

Every failure is a `NumWordifyException`:

| Exception | Raised when |
| --- | --- |
| `LocalizationNotFoundException` | No localization resolves for the requested culture. Carries `Culture` and `AvailableCultures`. |
| `AmbiguousCurrencyException` | The culture resolved to another region's localization, so its default currency cannot be assumed. Carries `RequestedCulture` and `ResolvedCulture`. |
| `InvalidLocalizationException` | A localization is incomplete or inconsistent. The message names the offending JSON path. |
| `NumberOutOfRangeException` | The number needs more scale words than the locale defines. |

Argument mistakes (`null` culture, `null` model, unknown currency code, empty culture name) surface as `ArgumentNullException` / `ArgumentException` as usual.

## Supported cultures

| Culture | Language | Default currency | Also defines | Notable rules handled |
| --- | --- | --- | --- | --- |
| `en-US` | English | USD | EUR, GBP, TRY | Teens, hyphenated compounds, singular/plural currency |
| `tr-TR` | Turkish | TRY | EUR, USD, GBP | "BİN" rather than "BİR BİN" |
| `fr-FR` | French | EUR | USD, CHF | 70–99 vigesimal forms, `cent`/`vingt` plural including the adjective/noun split, plural scale words |
| `es-ES` | Spanish | EUR | USD, MXN | `CIEN`/`CIENTO`, fused twenties, apocope, plural scale words, `de` before a currency name |

`tr-TR-EUR` also ships, but is deprecated: it is excluded from `SupportedCultures`, is never chosen by language fallback, and resolves only when named exactly. Its output is identical to `ToWords("tr-TR", "EUR")`, which a test enforces. Use the currency code instead.

### Known limitations

- **French** does not insert `de` / `d'` between a noun scale word and a currency name: `1_000_000M.ToWords("fr-FR")` yields "UN MILLION EUROS" where correct French is "un million d'euros". The elision depends on the following word, which the template model cannot express. Supply your own `currencyFormat`, or post-process, if you need it. Spanish, where no elision occurs, is handled: "UN MILLÓN DE EUROS".
- **Spanish** stops at 10^15 − 1. `MILLARDO` (10^9) is accepted by the RAE but uncommon — "mil millones" is the usual form and cannot be expressed as a single scale word here. 10^15 has no accepted single word at all, so the scale is not defined rather than invented. Gender agreement (`DOSCIENTAS`) is not modelled.
- **Currency names** distinguish only "one" from "not one". Languages with dual or paucal forms need a custom localization.
- **Word order within a group** is fixed as hundreds → tens → ones. Languages that invert it, such as German "einundzwanzig", cannot be expressed without enumerating 21–99 in `specialNumbers.special`.
- **Ordinals** ("twenty-first") are out of scope.

## Custom localization

Build a model in code for any language and currency:

```csharp
using NumWordify.Extensions;
using NumWordify.Models;

var japanese = new LocalizationModel
{
    Currency = new CurrencyModel { Major = "YEN", Minor = "SEN" },
    Numbers = new NumbersModel
    {
        Ones = ["", "ICHI", "NI", "SAN", "YON", "GO", "ROKU", "NANA", "HACHI", "KYU"],
        Tens = ["", "JU", "NIJU", "SANJU", "YONJU", "GOJU", "ROKUJU", "NANAJU", "HACHIJU", "KYUJU"],
        Hundreds =
        [
            "", "HYAKU", "NIHYAKU", "SANBYAKU", "YONHYAKU",
            "GOHYAKU", "ROPPYAKU", "NANAHYAKU", "HAPPYAKU", "KYUHYAKU"
        ],
        Scales = ["", "SEN", "MAN", "OKU", "CHO", "KEI"],
    },
    Settings = new SettingsModel
    {
        SkipOneForThousand = true,
        NegativeWord = "MAINASU",
        ZeroWord = "ZERO",
        CurrencyFormat = "{whole} {major}",
        NumberFormat = "{whole} TEN {decimal}",
    },
};

1234.56M.ToWords(japanese);
// "SEN NIHYAKU SANJU YON YEN"
```

Incomplete models are rejected at construction time with a message naming the missing field, so a broken localization never reaches the conversion loop.

## Adding a new language

Drop a JSON file into `Resources/`. The file name is the culture name. This block is parsed by a test, so it is always valid:

```json
{
  "defaultCurrency": "USD",
  "currencies": {
    "USD": {
      "major": "DOLLARS",
      "majorSingular": "DOLLAR",
      "minor": "CENTS",
      "minorSingular": "CENT"
    },
    "EUR": {
      "major": "EUROS",
      "majorSingular": "EURO",
      "minor": "CENTS",
      "minorSingular": "CENT"
    }
  },
  "settings": {
    "skipOneForThousand": false,
    "useTeens": true,
    "negativeWord": "NEGATIVE",
    "zeroWord": "ZERO",
    "currencyFormat": "{whole} {major} {decimal} {minor}",
    "numberFormat": "{whole} POINT {decimal}",
    "decimalPlaces": 2
  },
  "numbers": {
    "ones": ["", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE"],
    "tens": ["", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"],
    "hundreds": [
      "", "ONE HUNDRED", "TWO HUNDRED", "THREE HUNDRED", "FOUR HUNDRED",
      "FIVE HUNDRED", "SIX HUNDRED", "SEVEN HUNDRED", "EIGHT HUNDRED", "NINE HUNDRED"
    ],
    "scales": ["", "THOUSAND", "MILLION", "BILLION", "TRILLION", "QUADRILLION"]
  },
  "specialNumbers": {
    "teens": [
      "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN",
      "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN"
    ],
    "compoundSeparator": "-"
  }
}
```

Then approve the snapshot: run the test suite with `NUMWORDIFY_APPROVE=1`, which writes `tests/NumWordify.Tests/Approvals/<culture>.approved.txt` — every value from 0 to 1000 plus a magnitude ladder, with and without currency. Read that file before committing it; it is the review.

## Schema reference

`numbers`

| Field | Required | Meaning |
| --- | --- | --- |
| `ones`, `tens`, `hundreds` | yes | Exactly ten entries, indexed by digit; index 0 is unused and must be empty. |
| `exactHundreds` | no | Hundreds words used when the last two digits are zero. Ten entries, but only the ones that differ need a value — Spanish fills `CIEN` at index 1 and leaves the rest empty. |
| `scales` | yes | Index 0 is the units group, 1 the thousands, and so on. Index 0 must be empty; every other entry must have a value. Length caps the convertible range at 10^(3 × length) − 1. |
| `scalesPlural` | no | Scale words used when the preceding group is greater than one (`DEUX MILLIONS`). Same length as `scales`; empty entries fall back. |
| `scaleKinds` | no | `"Adjective"` or `"Noun"` per scale word, same length as `scales`. Defaults to all `Adjective`. This is what distinguishes French `DEUX CENT MILLE` from `DEUX CENTS MILLIONS`. |

`settings`

| Field | Default | Meaning |
| --- | --- | --- |
| `skipOneForThousand` | `false` | Drop the word for "one" before the thousands scale word (`BİN`, `MILLE`). |
| `useTeens` | auto | Use `specialNumbers.teens` for 11–19. Left unset it turns itself on whenever `teens` is supplied. |
| `useExactHundredsBeforeScale` | `true` | Whether `exactHundreds` also applies before an `Adjective` scale word. Before a `Noun` scale word the exact form is always used. Spanish `CIEN MIL` keeps it on; French `DEUX CENT MILLE` turns it off. |
| `apocopateBeforeNoun` | `false` | Apply `specialBeforeScale` in front of a noun — a `Noun` scale word or a currency name (Spanish `UN MILLÓN`, `UN EURO`). |
| `nounScaleLinkWord` | — | Word inserted between the number and the currency name when the number ends in a `Noun` scale word (Spanish `UN MILLÓN DE EUROS`). |
| `negativeWord`, `zeroWord` | — | Required. |
| `currencyFormat` | — | Required. Placeholders: `{whole}`, `{major}`, `{decimal}`, `{minor}`. Must contain `{whole}`; unknown placeholders are rejected. |
| `numberFormat` | — | Required. Placeholders: `{whole}`, `{decimal}`. |
| `decimalPlaces` | `2` | Fractional digits kept, 0 through 6. |
| `decimalReading` | `Fraction` | `Fraction` or `Digits`; see [Precision and rounding](#precision-and-rounding). |

`specialNumbers`

| Field | Meaning |
| --- | --- |
| `teens` | Exactly nine entries, for 11 through 19. |
| `special` | Whole-word overrides for the last two digits of a group, keyed 1–99. This is where French 70–99 and Spanish 21–29 live. |
| `specialBeforeScale` | Overrides that apply in front of an `Adjective` scale word, and — with `apocopateBeforeNoun` — in front of a noun. |
| `compoundSeparator` | Placed between the tens and ones word. `"-"` for English, `" Y "` for Spanish, `" "` (the default) for Turkish. |

Top level

| Field | Meaning |
| --- | --- |
| `currencies` | Currencies this locale can name, keyed by ISO 4217 code. |
| `defaultCurrency` | Key into `currencies` naming the default. Mutually exclusive with `currency`. |
| `currency` | The default currency spelled out. Prefer `defaultCurrency` when the currency is already in the map; naming it twice is how the two copies drift apart. |
| `deprecated` | Excludes the locale from `SupportedCultures` and from language fallback. It still resolves when named exactly. |

Templates are expanded in a single pass, so a currency name that happens to contain `{minor}` is emitted literally rather than being treated as another placeholder. A placeholder that resolves to an empty string takes its preceding space with it; whitespace a locale wrote on purpose is left alone.

## Migrating from 1.x

Version 2.0 changes output for four of the five shipped locales, because that output was wrong. Re-check any golden files or stored strings.

- **The ones digit is no longer dropped.** `1234.56M.ToWords("tr-TR")` was "BİN İKİ YÜZ OTUZ TL ELLİ Kr" and is now "BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr". The same fix applies to `fr-FR` and `es-ES`.
- **Round hundreds no longer gain a trailing "ZERO"** in `en-US`: `100M` was "ONE HUNDRED ZERO USD ZERO Cents", now "ONE HUNDRED DOLLARS ZERO CENTS".
- **`en-US` currency names** changed from `USD`/`Cents` to `DOLLARS`/`CENTS`, with singular forms.
- **French and Spanish** are now grammatically correct for 0–99, for the `cent`/`ciento` forms and for scale agreement; almost every value in those languages changes.
- **Rounding** is away from zero rather than to even, so some values gain a minor unit.
- **`NumberToWordsConverter` is now `sealed`.** Code that derived from it will not compile; depend on `INumberToWordsConverter` instead.
- **Exceptions** moved to the `NumWordify.Exceptions` namespace. `FileNotFoundException` became `LocalizationNotFoundException` and most `InvalidOperationException` cases became `InvalidLocalizationException`; both derive from `NumWordifyException` and neither derives from the old types, so 1.x `catch` blocks stop matching.
- **A culture that resolves through language fallback no longer borrows the locale's default currency** — `ToWords("es-MX")` now throws `AmbiguousCurrencyException` instead of silently printing euros.
- **`tr-TR-EUR` is no longer listed in `SupportedCultures`** and is never chosen by fallback, though naming it exactly still works.
- **`settings.useCompoundNumbers` and `settings.skipOneForHundred` no longer do anything.** They still exist as `[Obsolete]` properties so code keeps compiling. The separator comes from `specialNumbers.compoundSeparator`; the hundreds wording comes from `numbers.hundreds[1]`.
- **`settings.useTeens` is now `bool?`.** Assignments still compile; comparisons such as `== true` still work.
- `LocalizationModel.Currency` and `.Numbers` are now nullable, which is what they always were at runtime.

## Project structure

```
NumWordify/
├── Converters/
│   ├── INumberToWordsConverter.cs   # Public conversion contract
│   ├── LocalizationLoader.cs        # Embedded resource lookup, parsing, caching
│   ├── LocalizationValidator.cs     # Fail-at-construction validation
│   └── NumberToWordsConverter.cs    # Conversion algorithm
├── Exceptions/
│   └── NumWordifyException.cs       # Exception hierarchy
├── Extensions/
│   └── DecimalExtensions.cs         # Extension methods for decimal
├── Models/                          # Localization data model + WordifyOptions
└── Resources/                       # en-US, tr-TR, tr-TR-EUR, fr-FR, es-ES
tests/
└── NumWordify.Tests/
    ├── Approvals/                   # Full-output snapshots, one file per locale
    └── *.cs                         # Golden tables, concurrency, validation, schema
```

## Requirements

.NET 6.0 or later, or any runtime supporting .NET Standard 2.0 (including .NET Framework 4.6.1+).

## Contributing

Contributions are welcome. A pull request that changes conversion output must come with the regenerated snapshot, and the diff has to be readable line by line.

- Add support for new languages
- Improve existing language support
- Fix bugs
- Improve documentation

## License

MIT — see [LICENSE](LICENSE).

## Author

Kadir Emre Parlak — [@kep_dev](https://x.com/kep_dev)

## Support

1. Check the [GitHub Issues](https://github.com/keparlak/NumWordify/issues)
2. Open a new issue if yours is not there
3. For usage questions, include the culture, the input value and the output you got

## Changelog

See [CHANGELOG.md](CHANGELOG.md).
