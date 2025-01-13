# Changelog

Tüm önemli değişiklikler bu dosyada belgelenecektir.

## [1.0.0] - 2024-01-13

### Eklenen Özellikler (Added)

- CultureInfo desteği eklendi
- Özel sayı durumları için destek eklendi (11-19 arası sayılar)
- Compound number formatı desteği eklendi (örn: twenty-one)
- Özelleştirilebilir ayraç desteği eklendi
- MIT lisansı eklendi

### Değişiklikler (Changed)

- Para birimi desteği geliştirildi (CurrencyModel parametresi eklendi)
- README.md dosyası geliştirildi ve daha detaylı hale getirildi
- Proje yapısı ve dokümantasyon iyileştirildi

### Düzeltmeler (Fixed)

- JSON deserialization sorunları çözüldü
- Embedded resource yükleme sorunları giderildi
- Özel sayıların (11-19) dönüşüm hataları düzeltildi
- Compound number formatındaki tire (-) kullanımı düzeltildi

### Güvenlik (Security)

- .gitignore dosyası güncellendi ve hassas dosyalar eklendi

## [Sürüm Notları]

- Kütüphane .NET 9.0 veya üstü gerektirir
- Şu anda desteklenen diller: İngilizce, Türkçe, Fransızca ve İspanyolca
- Her dil için özel durumlar ve formatlar desteklenir
- Özelleştirilebilir para birimi desteği
