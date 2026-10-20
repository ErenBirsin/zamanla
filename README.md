# Zamanla

[![CI](https://github.com/ErenBirsin/zamanla/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/ErenBirsin/zamanla/actions/workflows/ci.yml)

Berber, kuaför ve güzellik salonları için çok kiracılı (multi-tenant) online randevu uygulaması. Salon sahibi hizmetlerini, personelini ve çalışma saatlerini panelden yönetir; müşteriler salonun kendine ait linkinden giriş yapmadan randevu alır.

## Özellikler

### Salon paneli

- İşletme kaydı ve giriş. Her salonun verisi diğerlerinden ayrı tutulur.
- Hizmetler: süre, hazırlık payı, fiyat ve kategori.
- Personel: verdiği hizmetler ve haftalık çalışma saatleri. Aynı güne öğle arası gibi birden fazla aralık girilebilir.
- Müşteriler: isim ya da telefonla arama. Silinen müşterinin kişisel bilgileri KVKK'ya uygun şekilde anonimleştirilir.
- Günlük takvim: personel sütunları, mola ve kapalı saatler, izinler, "şu an" çizgisi.
- Randevular: birden fazla hizmet, tamamlandı / gelmedi / iptal / yeniden planlama ve randevu geçmişi.
- İzinler: personel izni ve bayram gibi salonun kapalı olduğu günler, tüm gün ya da saat aralığıyla.
- Ayarlar: salon adı, adres, telefon ve online randevu kuralları.

### Müşteri tarafı

- Salonun linkinden (`/b/salon-adi`) hizmet, personel ve saat seçerek randevu alma. Sayfa telefon için tasarlandı.
- Randevuyu görüntüleme, iptal etme ve saatini değiştirme için kişiye özel yönetim linki.
- Randevuyu telefonun takvimine ekleme (.ics, 1 saat önce hatırlatma).
- Salon adres ve telefon girdiyse randevu sayfasında ve randevu linkinde gösterilir; telefon tıklanınca arama başlar.

### Kurallar ve güvenlik

- Boş saatler çalışma saatlerine, molalara, izinlere ve diğer randevulara göre 15 dakikalık adımlarla hesaplanır.
- Onaylı randevusu olan bir aralığa izin ya da kapalı gün girilemez; etkilenen randevular listelenir.
- Aynı saate aynı anda gelen iki randevudan yalnızca biri kaydedilir.
- Online randevu kuralları her salon için ayarlardan değiştirilebilir: kaç gün ilerisi (varsayılan 60), linkten iptal için son süre (varsayılan 3 saat) ve aynı telefonla en fazla ileri tarihli randevu (varsayılan 5).
- Yönetim linkindeki anahtar veritabanında açık haliyle değil, SHA-256 özetiyle saklanır.
- JWT access token (15 dk) ve refresh token (14 gün). Kullanılmış bir refresh token tekrar gelirse açık oturumlar kapatılır.
- Giriş, kayıt ve herkese açık uçlarda IP bazlı istek sınırı.

## Mimari ve teknolojiler

### Backend

- **Mimari:** Clean Architecture (Domain, Application, Infrastructure, WebAPI)
- **Desenler:** CQRS, Result, Repository, Unit of Work
- **.NET 10, ASP.NET Core Web API:** Minimal API modülleri ve OData uçları
- **Entity Framework Core + SQL Server:** veri erişimi, global query filter ile salon ve silinmiş kayıt ayrımı
- **TS.MediatR:** CQRS ve pipeline davranışları için
- **TS.Result:** standart sonuç modeli için
- **TS.EntityFrameworkCore.GenericRepository:** genel amaçlı repository işlemleri için
- **FluentValidation:** istek doğrulama için
- **Scrutor:** servislerin otomatik kaydı için
- **JWT Bearer:** kimlik doğrulama için
- **Scalar:** API dokümantasyonu ve deneme için

### Frontend

- **React 19, TypeScript, Vite**
- **Tailwind CSS:** arayüz
- **TanStack Query:** sunucu verisi ve önbellek
- **React Router:** sayfa yönlendirme
- **React Hook Form + Zod:** form ve doğrulama
- **Zustand:** oturum durumu
- **Axios:** API istekleri, token yenileme

### Test ve CI

- **xUnit v3, Shouldly:** birim ve entegrasyon testleri
- **WebApplicationFactory + Respawn:** gerçek SQL Server üzerinde, her testten önce temizlenen veritabanı
- **GitHub Actions:** her push ve PR'da backend testleri (SQL Server container ile) ve client lint/build

## Kurulum

### Gerekenler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 24](https://nodejs.org)
- SQL Server (Express yeterli). Örneklerde `.\SQLEXPRESS` kullanılıyor.

### 1. Depoyu klonla

```bash
git clone https://github.com/ErenBirsin/zamanla.git
cd zamanla
```

### 2. HTTPS geliştirme sertifikasına güven

```bash
dotnet dev-certs https --trust
```

### 3. Gizli ayarları User Secrets'a ekle

Bağlantı cümlesi ve JWT anahtarı repoda tutulmaz, her geliştiricinin kendi bilgisayarındaki User Secrets'ta durur.

```bash
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=.\SQLEXPRESS;Database=Zamanla;Trusted_Connection=True;TrustServerCertificate=True;" --project src/Zamanla.WebAPI
```

JWT anahtarı en az 64 karakter olmalı. PowerShell'de rastgele bir anahtar üretip ekranda göstermeden kaydetmek için:

```powershell
$bytes = New-Object byte[] 64
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Jwt:SecretKey" ([Convert]::ToBase64String($bytes)) --project src/Zamanla.WebAPI
```

macOS / Linux için:

```bash
dotnet user-secrets set "Jwt:SecretKey" "$(openssl rand -hex 48)" --project src/Zamanla.WebAPI
```

### 4. Paketleri yükle ve veritabanını oluştur

```bash
dotnet restore
dotnet tool install --global dotnet-ef --version 10.*
dotnet ef database update --project src/Zamanla.Infrastructure --startup-project src/Zamanla.WebAPI
```

### 5. API'yi çalıştır

```bash
dotnet run --project src/Zamanla.WebAPI --launch-profile https
```

API `https://localhost:7080` adresinde açılır, dokümantasyon `https://localhost:7080/scalar` adresindedir. İlk çalıştırmada boş veritabanına demo salon eklenir.

### 6. Client'ı çalıştır

Yeni bir terminalde:

```bash
cd client
npm ci
npm run dev
```

Uygulama `http://localhost:5173` adresinde açılır. Vite, `/api` isteklerini API'ye yönlendirir; bu yüzden API'nin de çalışıyor olması gerekir.

## Demo

Geliştirme ortamında API ilk açıldığında "Demo Kuaför" adında dolu bir salon oluşturulur: 6 hizmet, 3 personel, 8 müşteri ve bugünden başlayan bir haftalık randevu. Demo salon zaten varsa tekrar eklenmez.

| | |
| --- | --- |
| Panel | `http://localhost:5173` |
| E-posta | `demo@zamanla.test` |
| Şifre | `Demo12345` |
| Randevu sayfası | `http://localhost:5173/b/demo-kuafor` |

Bu hesap yalnızca yerel geliştirme içindir.

Demo verisini sıfırdan oluşturmak için veritabanını silip tekrar kurabilirsin:

```bash
dotnet ef database drop --project src/Zamanla.Infrastructure --startup-project src/Zamanla.WebAPI
dotnet ef database update --project src/Zamanla.Infrastructure --startup-project src/Zamanla.WebAPI
```

## Testler

```bash
dotnet test
```

Entegrasyon testleri gerçek bir SQL Server kullanır. Varsayılan olarak `.\SQLEXPRESS` üzerindeki `Zamanla_Test` veritabanına bağlanır; veritabanı yoksa migration'larla oluşturulur ve her testten önce Respawn ile temizlenir. Geliştirme veritabanına dokunulmaz.

Farklı bir SQL Server kullanmak için bağlantı cümlesini `ZAMANLA_TEST_CONNECTION` ortam değişkeniyle verebilirsin.

## Proje yapısı

```text
src/
├── Zamanla.Domain           Entity'ler, value object'ler, iş kuralları
├── Zamanla.Application      Use case'ler (CQRS), validasyon
├── Zamanla.Infrastructure   EF Core, migration'lar, kimlik doğrulama
└── Zamanla.WebAPI           HTTP uç noktaları, demo verisi
tests/
├── Zamanla.Application.Tests
└── Zamanla.WebAPI.IntegrationTests
client/
└── src/
    ├── features             Sayfa ve özellik bazlı klasörler (auth, calendar, booking...)
    └── shared               Ortak bileşenler, API istemcisi
```

## Yol haritası

- [x] Aşama 0: Proje kurulumu ve CI
- [x] Aşama 1: İşletme kaydı, giriş, JWT ve refresh token
- [x] Aşama 2: Hizmetler, personel ve müşteriler
- [x] Aşama 3: Randevular ve günlük takvim
- [x] Aşama 4: Herkese açık randevu sayfası
- [x] MVP: Kurulum rehberi ve demo verisi
- [ ] Aşama 5: v1 katmanı
  - [x] 5.1 Salon ayarları
  - [x] 5.2 Personel izni ve kapalı günler
  - [ ] 5.3 Hatırlatmalar (Hangfire, e-posta)
  - [ ] 5.4 Canlı takvim ve bildirim zili (SignalR)
  - [ ] 5.5 Günlük özet
  - [ ] 5.6 Personel hesapları ve yetkiler

Sürüm notları için [CHANGELOG.md](CHANGELOG.md).
