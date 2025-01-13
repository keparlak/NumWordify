# NumWordify

NumWordify is a .NET library that converts decimal numbers into words with multi-language and multi-currency support.

## Features

- Convert numbers to words with currency support
- Convert decimal numbers to words without currency
- Multi-language support (currently supports English and Turkish)
- Multi-currency support (USD, TRY, EUR)
- Culture-based conversions (supports both string culture codes and CultureInfo)
- Customizable formatting
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
// Convert to words with different currency
string euroResult = amount.ToWords("tr-TR-EUR");
// Output: "BİN İKİ YÜZ OTUZ DÖRT EURO ELLİ ALTI SENT"
// Convert to words without currency
string numberOnly = amount.ToWordsWithoutCurrency("tr-TR");
// Output: "BİN İKİ YÜZ OTUZ DÖRT NOKTA ELLİ ALTI"
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

You can create your own localization model for any currency. Here's an example for Japanese Yen:

```csharp
using NumWordify.Extensions;
using NumWordify.Models;
// Create custom localization model
var japaneseLocalization = new LocalizationModel {
  Currency = new CurrencyModel {
      Major = "YEN",
        Minor = "SEN"
    },
    Numbers = new NumbersModel {
      Ones = new ["", "ICHI", "NI", "SAN", "YON", "GO", "ROKU", "NANA", "HACHI", "KYU"],
        Tens = new ["", "JU", "NIJU", "SANJU", "YONJU", "GOJU", "ROKUJU", "NANAJU", "HACHIJU", "KYUJU"],
        Hundreds = new ["", "HYAKU", "NIHYAKU", "SANBYAKU", "YONHYAKU", "GOHYAKU", "ROPPYAKU", "NANAHYAKU", "HAPPYAKU", "KYUHYAKU"],
        Scales = new ["", "SEN", "MAN", "OKU", "CHO", "KEI"]
    },
    Settings = new SettingsModel {
      SkipOneForThousand = true,
        SkipOneForHundred = true,
        NegativeWord = "MAINASU",
        ZeroWord = "ZERO",
        CurrencyFormat = "{whole} {major}",
        NumberFormat = "{whole} TEN {decimal}"
    }
};
decimal amount = 1234.56 M;
string result = amount.ToWords(japaneseLocalization);
// Output: "SEN NIHYAKU SANJU YON YEN"
```

## Supported Cultures

| Culture Code | Language | Currency           |
| ------------ | -------- | ------------------ |
| en-US        | English  | US Dollar (USD)    |
| tr-TR        | Turkish  | Turkish Lira (TRY) |
| tr-TR-EUR    | Turkish  | Euro (EUR)         |
| fr-FR        | French   | Euro (EUR)         |
| es-ES        | Spanish  | Euro (EUR)         |


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
    "numberFormat": "{whole} POINT {decimal}"
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
  }
}
```

## Requirements

- .NET 9.0 or higher

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Kadir Emre Parlak

## Support

If you encounter any issues or have questions, please create an issue on GitHub.
