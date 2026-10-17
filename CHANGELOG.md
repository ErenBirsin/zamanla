# Değişiklik günlüğü

## [0.4.0] - 2026-10-17

Aşama 3: Randevu çekirdeği.

### Eklendi

- Randevular eklendi. Bir randevuda birden fazla hizmet olabiliyor, hizmetler aynı personel tarafından arka arkaya yapılıyor. Süre, fiyat ve hazırlık payı randevu alındığı andaki haliyle saklanıyor.
- Boş saatler personelin çalışma saatlerine, öğle arasına ve diğer randevulara göre 15 dakikalık adımlarla hesaplanıyor. Personel seçilmezse ("fark etmez") o gün en az randevusu olan personel atanıyor.
- Aynı saate aynı anda gelen iki randevudan yalnızca biri kaydediliyor. Bu, 20 paralel istekle test edildi.
- Randevu tamamlandı, gelmedi, iptal ve yeniden planlama işlemleri eklendi. Her değişiklik randevunun geçmişine yazılıyor, gelmeyen müşterinin sayacı artıyor.
- Personelin saatleri ya da hizmetleri ileri tarihli randevuları dışarıda bırakacak şekilde değiştirilemiyor. Mesajda hangi randevuların etkilendiği gösteriliyor. Randevusu olan personel silinemiyor.
- Panele günlük takvim eklendi: personel sütunları, mola ve kapalı saatler, randevu durumuna göre renkler ve "şu an" çizgisi.
- Takvimden hızlı randevu eklenebiliyor. Müşteri aranabiliyor ya da yeni müşteri eklenebiliyor. Takvimde boş alana tıklanınca personel ve saat dolu geliyor.
- Randevu detay paneli eklendi. Panelde durum işlemleri, 3 kez gelmeyen müşteri için uyarı, randevu geçmişi ve yeniden planlama var.
- Randevu kuralları için entegrasyon testleri yazıldı. Toplam test sayısı 63.

### Düzeltildi

- Form pencerelerinde hatalar artık pencerenin içinde gösteriliyor. Önceden bildirim pencerenin arkasında kalıyordu.

## [0.3.0] - 2026-10-14

Aşama 2: İşletme kurulumu.

### Eklendi

- Hizmetler eklendi. Her hizmetin süresi, hazırlık payı, fiyatı ve kategorisi var. Aynı salonda aynı isimde iki hizmet olamıyor.
- Personel eklendi. Her personelin verdiği hizmetler ve haftalık çalışma saatleri tutuluyor. Aynı güne öğle arası gibi birden fazla saat aralığı girilebiliyor ve aralıkların çakışması engelleniyor.
- Müşteriler eklendi. Telefon numarası farklı yazılsa da tek bir biçimde saklanıyor. Aynı telefonla farklı kişiler kaydedilebiliyor ama aynı kişi iki kez eklenemiyor.
- Müşteri silindiğinde kayıt tamamen silinmiyor, KVKK'ya uygun şekilde kişisel bilgileri temizlenip anonim hale getiriliyor.
- Listeler OData ile çekiliyor. Müşteriler isim ya da telefon numarasıyla aranabiliyor.
- Panele Hizmetler, Personel ve Müşteriler sayfaları eklendi. Sayfalar ilk açıldıklarında yükleniyor (lazy loading).
- Genel bakış sayfasına salonu randevuya hazırlamak için adım adım ilerleyen bir kurulum listesi ve randevu linki kartı eklendi.
- Hizmet, personel ve müşteri işlemleri için entegrasyon testleri yazıldı. Başka bir salonun verisine erişilemediği de testlerle kontrol ediliyor.

### Düzeltildi

- Hatalı biçimdeki isteklerde (örneğin geçersiz bir Guid) `500` yerine `400` dönülüyor.

## [0.2.0] - 2026-10-12

Aşama 1: İşletme ve kullanıcı işlemleri.

### Eklendi

- İşletme ve kullanıcı modelleri oluşturuldu. Kullanıcı şifreleri ASP.NET Core `PasswordHasher` kullanılarak güvenli şekilde saklanıyor.
- İşletmelere ait verilerin birbirinden ayrılması için global query filter ve `TenantInterceptor` eklendi.
- İşletme kaydı, kullanıcı girişi ve giriş yapan kullanıcının bilgilerini getiren `/api/auth/me` uç noktaları hazırlandı.
- JWT access token 15 dakika, refresh token ise 14 gün geçerli olacak şekilde ayarlandı.
- Refresh token yenileme işlemi eklendi. Daha önce kullanılmış bir token tekrar gönderildiğinde kullanıcının açık oturumları kapatılıyor.
- Giriş ve kayıt işlemlerine IP bazlı rate limiting eklendi. Limit aşıldığında `429` yanıtı dönüyor.
- Scalar üzerinden yetkilendirme gerektiren uç noktaları test edebilmek için Bearer token desteği eklendi.
- Testler için gerçek SQL Server kullanan bir entegrasyon test ortamı kuruldu. Her test öncesinde veritabanı Respawn ile temizleniyor.
- GitHub Actions CI sürecinde kullanılmak üzere SQL Server service container yapılandırıldı.
- React tarafında giriş ve kayıt sayfaları, korumalı panel ve sayfa yönlendirmeleri eklendi.
- Form doğrulamalarında React Hook Form ve Zod kullanıldı. İşletme kaydı sırasında kısa bağlantı (slug) önerisi de eklendi.
- Access token süresi dolduğunda token'ın arka planda yenilenmesi ve hataların toast bildirimleriyle gösterilmesi sağlandı.

## [0.1.0] - 2026-10-09

Aşama 0: Projenin kurulumu ve temel yapı.

### Eklendi

- .NET 10 kullanılarak Domain, Application, Infrastructure ve WebAPI katmanlarından oluşan Clean Architecture yapısı kuruldu.
- SQL Server ve EF Core yapılandırıldı. Ortak audit alanları ve soft delete desteği eklendi.
- CQRS yapısı için TS.MediatR, istek doğrulama işlemleri için FluentValidation kullanıldı.
- Hataların merkezi olarak yönetilmesi sağlandı ve API dokümantasyonu için Scalar eklendi.
- React 19, Vite, TypeScript ve Tailwind CSS ile istemci tarafının temeli oluşturuldu. Tasarım renkleri belirlendi ve API durumunu gösteren bir sağlık kartı eklendi.
- GitHub Actions üzerinden backend build ve test işlemleri ile istemci lint ve build kontrolleri yapılandırıldı.

[0.4.0]: https://github.com/ErenBirsin/zamanla/compare/v0.3.0...v0.4.0

[0.3.0]: https://github.com/ErenBirsin/zamanla/compare/v0.2.0...v0.3.0

[0.2.0]: https://github.com/ErenBirsin/zamanla/compare/v0.1.0...v0.2.0

[0.1.0]: https://github.com/ErenBirsin/zamanla/releases/tag/v0.1.0
