# NumWordify

NumWordify is a .NET library that converts decimal numbers into words with multi-language and multi-currency support.

## Features

- Convert numbers to words with currency support
- Convert decimal numbers to words without currency
- Multi-language support (currently supports English, Turkish, French, and Spanish)
- Multi-currency support (USD, TRY, EUR, and custom currencies)
- Culture-based conversions (supports both string culture codes and CultureInfo)
- Special number handling (e.g., "eleven", "twelve" in English)
- Compound number formatting (e.g., "twenty-one" vs "twenty one")
- Customizable formatting and separators
- Negative number support
- Zero handling
- Extensible localization through JSON files
- Caching support for better performance

## Installation

You can install NumWordify via NuGet:

```bash
dotnet add package NumWordify
```

## Usage

### Basic Usage

```csharp
using NumWordify.Extensions;
using System.Globalization;

decimal amount = 1234.56M;

// Using string culture code
string result1 = amount.ToWords("tr-TR");
// Output: "BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr"

// Using CultureInfo
var culture = new CultureInfo("tr-TR");
string result2 = amount.ToWords(culture);
// Output: "BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr"

// Using current culture
string result3 = amount.ToWords(CultureInfo.CurrentCulture);

// Convert to words with custom currency
var euroCurrency = new CurrencyModel { Major = "EURO", Minor = "CENT" };
string euroResult = amount.ToWords("tr-TR", euroCurrency);
// Output: "BİN İKİ YÜZ OTUZ DÖRT EURO ELLİ ALTI CENT"

// Convert to words without currency
string numberOnly = amount.ToWordsWithoutCurrency("tr-TR");
// Output: "BİN İKİ YÜZ OTUZ DÖRT NOKTA ELLİ ALTI"
```

### Special Number Handling

NumWordify supports special number cases in different languages:

```csharp
decimal amount = 11234.56M;
string result = amount.ToWords("en-US");
// Output: "ELEVEN THOUSAND TWO HUNDRED THIRTY-FOUR DOLLARS FIFTY-SIX CENTS"

// Note how it uses:
// - Special word "ELEVEN" instead of "TEN ONE"
// - Hyphenated compound numbers "THIRTY-FOUR"
```

### Negative Numbers

```csharp
decimal amount = -1234.56M;
string result = amount.ToWords("tr-TR");
// Output: "EKSİ BİN İKİ YÜZ OTUZ DÖRT TL ELLİ ALTI Kr"
```

### Zero Handling

```csharp
decimal amount = 0M;
string result = amount.ToWords("tr-TR");
// Output: "SIFIR TL SIFIR Kr"
```

### Custom Localization

You can create your own localization model for any language and currency. Here's an example for Japanese Yen:

```csharp
using NumWordify.Extensions;
using NumWordify.Models;

// Create custom localization model
var japaneseLocalization = new LocalizationModel
{
    Currency = new CurrencyModel
    {
        Major = "YEN",
        Minor = "SEN"
    },
    Numbers = new NumbersModel
    {
        Ones = new[] { "", "ICHI", "NI", "SAN", "YON", "GO", "ROKU", "NANA", "HACHI", "KYU" },
        Tens = new[] { "", "JU", "NIJU", "SANJU", "YONJU", "GOJU", "ROKUJU", "NANAJU", "HACHIJU", "KYUJU" },
        Hundreds = new[] { "", "HYAKU", "NIHYAKU", "SANBYAKU", "YONHYAKU", "GOHYAKU", "ROPPYAKU", "NANAHYAKU", "HAPPYAKU", "KYUHYAKU" },
        Scales = new[] { "", "SEN", "MAN", "OKU", "CHO", "KEI" }
    },
    Settings = new SettingsModel
    {
        SkipOneForThousand = true,
        SkipOneForHundred = true,
        NegativeWord = "MAINASU",
        ZeroWord = "ZERO",
        CurrencyFormat = "{whole} {major}",
        NumberFormat = "{whole} TEN {decimal}",
        UseTeens = false,
        UseCompoundNumbers = false
    }
};

decimal amount = 1234.56M;
string result = amount.ToWords(japaneseLocalization);
// Output: "SEN NIHYAKU SANJU YON YEN"
```

## Supported Cultures

| Culture Code | Language | Currency           | Special Features                |
| ------------ | -------- | ------------------ | ------------------------------- |
| en-US        | English  | US Dollar (USD)    | Teens (11-19), Compound numbers |
| tr-TR        | Turkish  | Turkish Lira (TRY) | Skip "bir" for hundreds         |
| tr-TR-EUR    | Turkish  | Euro (EUR)         | Skip "bir" for hundreds         |
| fr-FR        | French   | Euro (EUR)         | -                               |
| es-ES        | Spanish  | Euro (EUR)         | -                               |

## Adding New Languages

To add a new language, create a JSON file in the Resources folder with the following structure:

```json
{
  "currency": {
    "major": "USD",
    "minor": "Cents"
  },
  "settings": {
    "skipOneForThousand": false,
    "skipOneForHundred": false,
    "negativeWord": "NEGATIVE",
    "zeroWord": "ZERO",
    "currencyFormat": "{whole} {major} {decimal} {minor}",
    "numberFormat": "{whole} POINT {decimal}",
    "useTeens": true,
    "useCompoundNumbers": true
  },
  "numbers": {
    "ones": [
      "",
      "ONE",
      "TWO",
      "THREE",
      "FOUR",
      "FIVE",
      "SIX",
      "SEVEN",
      "EIGHT",
      "NINE"
    ],
    "tens": [
      "",
      "TEN",
      "TWENTY",
      "THIRTY",
      "FORTY",
      "FIFTY",
      "SIXTY",
      "SEVENTY",
      "EIGHTY",
      "NINETY"
    ],
    "hundreds": [
      "",
      "ONE HUNDRED",
      "TWO HUNDRED",
      "THREE HUNDRED",
      "FOUR HUNDRED",
      "FIVE HUNDRED",
      "SIX HUNDRED",
      "SEVEN HUNDRED",
      "EIGHT HUNDRED",
      "NINE HUNDRED"
    ],
    "scales": ["", "THOUSAND", "MILLION", "BILLION", "TRILLION", "QUADRILLION"]
  },
  "specialNumbers": {
    "teens": [
      "ELEVEN",
      "TWELVE",
      "THIRTEEN",
      "FOURTEEN",
      "FIFTEEN",
      "SIXTEEN",
      "SEVENTEEN",
      "EIGHTEEN",
      "NINETEEN"
    ],
    "compoundSeparator": "-",
    "special": {
      "0": "ZERO"
    }
  }
}
```

### Localization Settings Explained

- `skipOneForThousand`: Skip "one" word for thousands (e.g., "bir bin" -> "bin")
- `skipOneForHundred`: Skip "one" word for hundreds (e.g., "bir yüz" -> "yüz")
- `useTeens`: Use special words for numbers 11-19
- `useCompoundNumbers`: Use compound number format (e.g., "twenty-one" vs "twenty one")
- `compoundSeparator`: Separator for compound numbers (e.g., "-" for English)
- `currencyFormat`: Format string for currency representation
- `numberFormat`: Format string for number-only representation

## Project Structure

```
NumWordify/
├── Converters/
│   └── NumberToWordsConverter.cs    # Core conversion logic
├── Extensions/
│   └── DecimalExtensions.cs         # Extension methods for decimal
├── Models/
│   └── LocalizationModel.cs         # Data models for localization
└── Resources/                       # Language resource files
    ├── en-US.json
    ├── tr-TR.json
    ├── fr-FR.json
    └── es-ES.json
```

## Requirements

- .NET 9.0 or higher

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. Here are some ways you can contribute:

- Add support for new languages
- Improve existing language support
- Add new features
- Fix bugs
- Improve documentation
- Add tests

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Kadir Emre Parlak

## Support

If you encounter any issues or have questions:

1. Check the [GitHub Issues](https://github.com/parlak/NumWordify/issues) for existing problems or solutions
2. Create a new issue if your problem is not already reported
3. For usage questions, provide a minimal code example that demonstrates the issue

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a list of changes in each version.
