# Train Reservation API

Minimal bir .NET 9 API; verilen tren ve vagon bilgilerine göre rezervasyonun kurallara uyup uymadığını hesaplar ve hangi vagona kaç kişinin yerleşeceğini döner. Girdi/çıktı şeması tamamen Türkçe tutuldu.

## Çalıştırma
- .NET 9 SDK yüklü olmalı.
- Lokal:
  ```bash
  dotnet run --project src/TrainReservation.Api
  ```
  Varsayılan adres: `http://localhost:5078` (launchSettings varsa değişebilir).
- Testler: `dotnet test --no-build`
- Docker:
  ```bash
  docker build -t train-reservation .
  docker run -p 8080:8080 train-reservation
  ```

## Örnek İstek
`POST /reservations`
```http
{
  "Tren": {
    "Ad": "Başkent Ekspres",
    "Vagonlar": [
      { "Ad": "Vagon 1", "Kapasite": 100, "DoluKoltukAdet": 68 },
      { "Ad": "Vagon 2", "Kapasite": 90, "DoluKoltukAdet": 50 },
      { "Ad": "Vagon 3", "Kapasite": 80, "DoluKoltukAdet": 80 }
    ]
  },
  "RezervasyonYapilacakKisiSayisi": 3,
  "KisilerFarkliVagonlaraYerlestirilebilir": true
}
```
Örnek cevap:
```http
{
  "RezervasyonYapilabilir": true,
  "YerlesimAyrinti": [
    { "VagonAdi": "Vagon 1", "KisiSayisi": 2 },
    { "VagonAdi": "Vagon 2", "KisiSayisi": 1 }
  ]
}
```

## Kurallar ve Algoritma
- Her vagon için çevrimiçi doluluk üst sınırı: `floor(Kapasite * 0.7)`. Bu sınırı aşan koltuk sayısı rezerve edilemez.
- `KisilerFarkliVagonlaraYerlestirilebilir = false` ise tüm yolcuları tek bir vagonda karşılayabilen ilk uygun vagon seçilir.
- Farklı vagonlara dağıtım serbestse, istenen kişi sayısı vagonların giriş sırası ile paylaştırılır; kalan kişi kalmazsa rezervasyon başarılıdır.
- İstek veya vagon listesi boş/geçersizse rezervasyon reddedilir.

## Vercel / Deploy Notu
- Depoda Dockerfile var; Vercel Docker deploylarını destekliyor. SSH anahtar ve repo push sonrası:
  ```bash
  vercel --prod --confirm
  ```
  Vercel Docker builder, konteyneri 8080 portundan dinler; `ASPNETCORE_URLS` buna göre ayarlandı.
- Alternatif olarak herhangi bir container platformuna (Fly.io, Azure Container Apps vb.) Docker imajını göndererek koşabilirsiniz.

## Yapı
- `src/TrainReservation.Api`: Minimal API, `/reservations` endpoint, Türkçe contract'lar ve ayarlanmış JSON isimlendirme politikası.
- `tests/TrainReservation.Tests`: Rezervasyon senaryoları için xUnit testleri.
- `TrainReservation.Api.http`: VS Code/HTTP client ile hızlı deneme dosyası.
