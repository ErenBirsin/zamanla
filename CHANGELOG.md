# Değişiklik günlüğü

## [0.15.0] - 2026-10-28

Aşama 6.2: Herkese açık sayfalar.

### Eklendi

- Giriş, kayıt ve şifre sayfalarındaki koyu panele ürünün küçük bir önizlemesi eklendi. Önizlemede örnek bir güne ait randevu listesi ve yeni bir online randevu bildirimi gösteriliyor. Panelin altında da üç kısa özellik maddesi yer alıyor. Geniş ekranlarda içerik panelin ortasında toplanırken telefonlarda panel yalnızca logonun bulunduğu ince bir şerit olarak gösteriliyor.
- Geçersiz şifre sıfırlama bağlantısı, açılamayan davet ve bulunamayan sayfa, salon veya randevu durumları için ikonlu ortak bir durum kartı oluşturuldu. Davet ekranında bağlantının süresinin dolmuş olması ile daha önce kullanılmış ya da geri alınmış olması ayrı mesajlarla belirtiliyor.
- Randevu oluşturma ve randevu yönetimi sayfaları bilgisayarlarda iki sütunlu hale getirildi. Sol tarafta salonun adı, adresi, telefonu ve haftalık çalışma saatleri sabit bir kartta gösteriliyor. İçinde bulunulan gün kalın yazıyla belirtiliyor, salonun kapalı olduğu günlerde ise "Kapalı" yazıyor. Sağ tarafta randevu adımları yer alıyor. Alt çubuk bu sütunun genişliğinde bir kart olarak ekranın altına sabitlendi. Telefon görünümünde değişiklik yapılmadı.
- Tüm telefon alanlarına `+90` ülke kodu kutusu eklendi. Numara girilirken rakamlar `532 123 45 67` biçiminde gruplandırılıyor. Baştaki `0` veya `90` alana yazılmıyor ve 10 haneden fazla rakam girilmesi engelleniyor. Kayıtlı numaralar düzenleme formlarında da bu biçimde gösteriliyor. Liste ekranlarında ise `0532 123 45 67` formatı korunuyor.

## [0.14.1] - 2026-10-27

Paket güncellemesi: .NET 10.0.13.

### Güvenlik

- .NET paketleri 10.0.12'den 10.0.13'e yükseltildi. Entegrasyon testlerinde kullanılan SignalR istemcisindeki bilinen açık bu sürümle kapandı.
- SQL Server istemcisinin dolaylı olarak getirdiği `System.Security.Cryptography.Pkcs` paketi, bilinen iki açığın kapatıldığı 10.0.13 sürümüne sabitlendi. Bu açıklar için daha önce eklenen uyarı bastırma satırları kaldırıldı.

## [0.14.0] - 2026-10-26

Aşama 6.1: Görünüm iyileştirmeleri.

### Eklendi

- Silme, davet iptali ve hesabı aktif veya pasif yapma işlemlerinde tarayıcının kendi onay kutusu yerine uygulamanın tasarımına uygun bir onay penceresi kullanılmaya başlandı.
- Liste, ayarlar, genel bakış ve takvim sayfalarında veriler yüklenirken "Yükleniyor" yazısı yerine içeriklerin yerini alan iskeletler gösteriliyor. API'ye ulaşılamadığında boş bir liste yerine "Veriler yüklenemedi" kartı ve "Tekrar dene" düğmesi gösteriliyor.
- Tablolardaki Hesap, Düzenle ve Sil düğmeleri ortak bir bileşende toplandı. Düğmeler ikonlu ve çerçeveli hale getirildi. Personel tablosunda dar ekranlarda yalnızca ikonlar gösteriliyor; üzerine gelindiğinde düğmenin adı görünüyor.
- Takvimdeki randevu kartlarına personelin rengiyle uyumlu bir sol şerit ve hafif gölge eklendi. Geçmiş saatler daha soluk gösteriliyor, mesai öncesi ve sonrası alanlar "Mesai dışı" olarak işaretleniyor. Tamamlanan randevular ise beyaz kart ve "Tamamlandı" etiketiyle diğerlerinden ayrılıyor.
- Sol menünün altındaki kullanıcı bilgisi baş harflerin gösterildiği bir karta dönüştürüldü. Şifre değiştirme ve çıkış seçenekleri, bu karta tıklanınca açılan menüye taşındı. Sol üstteki logoya tıklandığında Genel bakış sayfası açılıyor.
- Mobil görünümde sol menü, üst çubuktaki ☰ düğmesiyle soldan açılan bir panele dönüştürüldü. Hizmetler, Personel, Müşteriler ve İzinler sayfalarındaki tablolar telefon ekranlarında kart listesi olarak gösteriliyor.
- Davet, şifre sıfırlama, randevu onayı ve hatırlatma e-postaları için ortak bir HTML şablonu oluşturuldu. Şablonda logo, düğme ve randevu bilgilerinin bulunduğu bir kutu yer alıyor. HTML desteklemeyen e-posta programları için düz metin içeriği de eklendi. Müşteri ve salon adı gibi kullanıcıdan gelen bilgiler, HTML kodu olarak yorumlanmalarını önlemek için kodlanıyor. E-posta testlerine HTML içerik kontrolleri eklendi.

### Kaldırıldı

- Genel bakış sayfasındaki geliştirme amaçlı "Backend durumu" kartı ve sol menüdeki "Randevular · Yakında" yer tutucusu kaldırıldı.

### Düzeltildi

- Aynı anda başarısız olan isteklerde aynı hata mesajı birden fazla kez gösterilebiliyordu. Artık aynı mesaj için tek bir bildirim gösteriliyor.
- Takvimdeki "şu an" çizgisi gerçek saatin birkaç dakika aşağısında duruyordu. Bu sorun düzeltildi.
- Personel düzenleme gibi uzun pencerelerde pencerenin tamamı kaydığı için başlık ve Kaydet düğmesi görünümden çıkabiliyordu. Artık pencere ekran boyutuna sığıyor ve yalnızca içerik bölümü kayıyor. Başlık üstte, personel ve randevu formlarındaki düğmeler ise altta sabit kalıyor. Pencere açıkken arka plandaki sayfanın kayması da engellendi.

## [0.13.0] - 2026-10-23

Aşama 5.7: Şifre sıfırlama ve şifre değiştirme.

### Eklendi

- Şifremi unuttum özelliği eklendi. `POST /api/auth/forgot-password` üzerinden kullanıcının e-posta adresine şifre sıfırlama bağlantısı gönderiliyor. Bağlantı 1 saat geçerli ve yalnızca bir kez kullanılabiliyor. Güvenlik için bağlantının kendisi yerine SHA-256 özeti veritabanında saklanıyor. Yeni bir sıfırlama isteği oluşturulduğunda önceki bağlantı geçersiz oluyor. E-posta adresi kayıtlı olsun veya olmasın aynı cevap dönüyor. Pasif hesaplar ve pasif salonlar için sıfırlama bağlantısı gönderilmiyor.
- Sıfırlama bağlantısını kontrol etmek için `GET /api/auth/check-forgot-password-code/{code}`, yeni şifreyi kaydetmek için `POST /api/auth/reset-password` uçları eklendi. Şifre sıfırlandığında kullanıcının tüm oturumları kapatılıyor.
- Giriş yapmış kullanıcıların şifrelerini değiştirebilmesi için `PUT /api/auth/password` ucu eklendi. İşlem sırasında mevcut şifre isteniyor ve yeni şifrenin mevcut şifreyle aynı olmasına izin verilmiyor. Şifre değiştirildiğinde diğer cihazlardaki oturumlar kapatılıyor, işlemin yapıldığı cihaza yeni token veriliyor.
- Kayıt, davet kabulü, şifre sıfırlama ve şifre değiştirme işlemlerindeki şifre kuralları güncellendi. Şifrenin en az 8 karakter olması, büyük ve küçük harf, rakam ve özel karakter içermesi gerekiyor. Türkçe harfler de büyük ve küçük harf kontrolüne dahil edildi. Kurallar hem formlarda hem de API tarafında kontrol ediliyor. Mevcut şifreler bu değişiklikten etkilenmiyor. Demo salonun şifresi `Demo12345!` olarak güncellendi.
- Giriş sayfasına "Şifremi unuttum" bağlantısı eklendi. Şifre sıfırlama isteği gönderme ve yeni şifre belirleme sayfaları da hazırlandı. Bu sayfalar, salonun ortak bilgisayarında başka bir kullanıcı oturum açmış olsa bile kullanılabiliyor.
- Şifre alanlarına göster/gizle düğmesi eklendi. Kayıt, davet kabulü ve şifre sıfırlama sayfalarında ve şifre değiştirme penceresinde şifre gücünü gösteren çubuk ve şifre kurallarının listesi de yer alıyor.
- Panelde şifre değiştirmek için "Şifreyi değiştir" penceresi eklendi.
- Şifre sıfırlama, şifre değiştirme ve şifre kurallarıyla ilgili birim ve entegrasyon testleri eklendi. Toplam test sayısı 159'a ulaştı.

### Düzeltildi

- Şifre değiştirildiğinde veya hesap pasif yapıldığında oturumlar kapatılırken refresh token'lar kullanılmış olarak işaretleniyordu. Eski bir cihazdan gelen yenileme isteği, token'ın çalındığı şeklinde algılanabiliyor ve yeni açılan oturumun da kapanmasına neden oluyordu. Artık bu token'lar siliniyor.

## [0.12.0] - 2026-10-22

Aşama 5.6: Personel hesapları ve yetkilendirme.

### Eklendi

- Personel hesapları için 7 farklı yetki eklendi. Bunlar başka personelin randevularını yönetme, ciroyu görme, müşteri silme, hizmetleri yönetme, personel ve çalışma saatlerini düzenleme, izinleri yönetme ve salon ayarlarını değiştirme yetkileri. Salon sahibi tüm yetkilere sahip. Yetki kontrolü için komutlara `[Permission]` etiketi eklendi ve kontroller doğrulama işleminden önce ortak bir pipeline adımında yapılıyor. Personel davet etme ve hesap yönetimi yalnızca salon sahibine açık. Yetkisiz isteklerde `403` yanıtı dönüyor. Yetkilerin listesi ve Türkçe karşılıkları `GET /api/permissions` üzerinden alınabiliyor.
- Kullanıcının yetkileri ve hesap durumu her istekte veritabanından kontrol ediliyor. Böylece bir yetki kaldırıldığında veya hesap pasif yapıldığında, personelin token süresinin dolması beklenmeden değişiklik uygulanıyor.
- Personel davet işlemleri eklendi. Salon sahibi `POST /api/invitations` üzerinden e-posta adresi ve verilecek yetkilerle davet oluşturabiliyor. Davet linki 7 gün geçerli ve yalnızca bir kez kullanılabiliyor. Güvenlik için linkin kendisi yerine SHA-256 özeti saklanıyor. E-posta gönderilemese bile davet linki salon sahibine gösteriliyor. Yeni davet gönderildiğinde önceki link geçersiz oluyor. Bekleyen davetler `DELETE /api/invitations/{staffId}` ile iptal edilebiliyor.
- Daveti kabul etmek için herkese açık `GET /api/public/{slug}/invitations/{token}` ve `POST /api/public/{slug}/invitations/{token}/accept` uçları eklendi. Personel en az 8 karakterli şifresini belirledikten sonra hesabı oluşturuluyor ve doğrudan panele giriş yapmış oluyor.
- Personel hesaplarını yönetmek için `GET /api/staff/accounts`, `PUT /api/staff/accounts/permissions` ve `PUT /api/staff/accounts/status` uçları eklendi. Hesap listesinde personelin hesabı olup olmadığı, davetin gönderilip gönderilmediği veya süresinin dolup dolmadığı ve hesabın aktif ya da pasif durumu gösteriliyor. Hesap pasif yapıldığında açık oturumlar kapatılıyor. Personel silindiğinde veya pasif yapıldığında hesabı da kapatılıyor ve bekleyen davetleri kaldırılıyor. Personelin adı değiştirildiğinde hesap bilgisi de güncelleniyor.
- Personel kendi randevularını oluşturabiliyor, tamamlandı veya gelmedi olarak işaretleyebiliyor, iptal edebiliyor ve saatini değiştirebiliyor. Başka bir personelin randevusunda bu işlemler için ek yetki gerekiyor. Randevu başka bir personele aktarıldığında hem mevcut hem de yeni personel için yetki kontrolü yapılıyor. Ciro yetkisi olmayan personele genel bakış API'sinden ciro bilgisi gönderilmiyor.
- Paneldeki menüler, düğmeler ve ciro bilgileri kullanıcının yetkilerine göre gösteriliyor. Yetkisi olmayan bir personel, Ayarlar sayfasının adresini doğrudan yazarak da sayfaya erişemiyor.
- Davet bağlantısından açılan `/davet/{slug}/{token}` sayfası eklendi. Personel bu sayfada salonun adını ve davetin geçerlilik süresini görebiliyor, ardından şifresini belirleyebiliyor. Süresi dolmuş, kullanılmış veya iptal edilmiş bağlantılar için ayrı mesajlar gösteriliyor.
- Personel sayfasına, yalnızca salon sahibinin görebildiği "Panel hesabı" sütunu eklendi. Hesap penceresinden davet gönderilebiliyor, davet geri alınabiliyor, bağlantı kopyalanabiliyor, yetkiler değiştirilebiliyor ve hesap aktif veya pasif yapılabiliyor.
- Takvim de personelin yetkilerine göre düzenlendi. Başka personelin randevularını yönetme yetkisi olmayan kullanıcılar bu randevuları yalnızca görüntüleyebiliyor. Takvimde boş bir alana tıklayarak sadece kendi sütunlarına randevu ekleyebiliyorlar. Yeni randevu ve yeniden planlama pencerelerinde de yalnızca kendileri personel olarak seçilebiliyor.
- Yetkilendirme, davet ve personel hesapları için birim ve entegrasyon testleri eklendi. Yetki etiketlerinin tanımlı yetkilerle eşleşmesi, pasif hesabın giriş yapamaması, davet linkinin ikinci kez kullanılamaması ve başka bir salon sahibinin farklı bir salonun personel yetkilerini değiştirememesi test edildi. Toplam test sayısı 142'ye ulaştı.

## [0.11.0] - 2026-10-21

Aşama 5.5: Günlük özet.

### Eklendi

- Genel bakış sayfasında kullanılmak üzere `GET /api/dashboard` ucu eklendi. Bu uç bugünkü randevu sayısını, beklenen ciroyu, tamamlanan, gelmeyen ve iptal edilen randevuları, bugün sıradaki en fazla 5 randevuyu ve son 7 günün randevu ve ciro bilgilerini tek istekte getiriyor. Günler salonun saat dilimine göre hesaplanıyor. Randevu olmayan günler de sonuçlarda sıfır değerle yer alıyor.
- Randevu ve ciro hesaplamaları durumlarına göre ayrıldı. İptal edilen randevular toplam sayıya dahil edilmiyor. Beklenen ciro hesaplanırken gelmeyen randevular sayılmıyor; gerçekleşen ciro ise tamamlanan randevular üzerinden hesaplanıyor.
- Genel bakış sayfasına bugünün bilgilerini gösteren özet kartları ve sıradaki randevuların listesi eklendi. Takvimde bir değişiklik olduğunda bu bilgiler sayfayı yenilemeden güncelleniyor. Saati geçen randevular da dakikada bir yapılan kontrolle listeden kaldırılıyor.
- Son 7 günün randevu ve ciro bilgilerini gösteren sütun grafiği eklendi. İki veri arasında geçiş yapılabiliyor ve bugünün sütunu farklı renkte gösteriliyor. Grafik için Chart.js kullanıldı.
- Özet ekranı için entegrasyon testleri yazıldı. Randevuların durumlarına göre doğru sayılması, son 7 günün doğru gruplanması ve başka bir salonun randevularının sonuçlara dahil edilmemesi kontrol ediliyor. Toplam test sayısı 124'e ulaştı.

## [0.10.0] - 2026-10-21

Aşama 5.4: Bildirimler ve canlı takvim güncellemeleri.

### Eklendi

- Müşteri online randevu aldığında, randevusunu iptal ettiğinde veya saatini değiştirdiğinde salon paneline bildirim düşmesi sağlandı. Panel üzerinden yapılan randevu işlemleri bildirim oluşturmuyor.
- Bildirimlerde müşteri adı ve randevu saati ayrıca saklanmıyor. Bu bilgiler listeleme sırasında randevudan alınıyor. Böylece randevu saati değiştiğinde bildirimde de güncel saat gösteriliyor, müşteri silindiğinde adı bildirimlerde kalmıyor.
- Bildirimleri listelemek için OData `notifications` ucu, tek bir bildirimi okundu olarak işaretlemek için `PUT /api/notifications/{id}/read` ve tüm bildirimleri okundu yapmak için `PUT /api/notifications/read-all` uçları eklendi.
- Canlı güncellemeler için `/api/hubs/notifications` adresinde bir SignalR hub oluşturuldu. Randevu veya izin değiştiğinde ve yeni bildirim geldiğinde açık olan ilgili salon panellerine haber gönderiliyor. Bu mesajlarda veri taşınmıyor; panel güncel bilgileri mevcut API uçlarından alıyor. Değişiklikleri yakalamak için her komuta ayrı kod eklemek yerine kayıt işlemi sırasında çalışan ortak bir mekanizma kullanıldı.
- Takvimde randevu veya izin değiştiğinde sayfanın yenilenmesine gerek kalmıyor. Bağlantı kesilirse yeniden bağlanılıyor ve bağlantının koptuğu sırada kaçırılan değişiklikler tekrar yükleniyor.
- Panele bildirim zili eklendi. Okunmamış bildirim sayısı gösteriliyor ve yeni bildirim geldiğinde sayı güncellenerek kısa bir uyarı veriliyor. Bildirime tıklandığında ilgili randevunun bulunduğu gün takvimde açılıyor. Bildirimler tek tek veya topluca okundu olarak işaretlenebiliyor.
- Bildirimler ve canlı güncellemeler için entegrasyon testleri yazıldı. Testlerde SignalR hub'ına bağlanılarak olayların yalnızca ilgili salona gönderildiği de kontrol ediliyor. Toplam test sayısı 120 oldu.

### Güvenlik

- Testlerde kullanılan `Microsoft.AspNetCore.SignalR.Client` 10.0.12 paketindeki CVE-2026-70159 uyarısı geçici olarak susturuldu. Uyarının ilgili olduğu stateful reconnect özelliği hub'da kullanılmıyor. Paket yalnızca testlerde kullanılıyor. 10.0.13 sürümüne geçildiğinde bu istisnanın kaldırılması planlandı.

## [0.9.0] - 2026-10-20

Aşama 5.3: Hatırlatmalar.

### Eklendi

- Randevu oluşturulduğunda müşteriye onay e-postası ve SMS bildirimi eklendi. E-postada randevu tarihi, hizmet, personel, randevu numarası ve salonun iletişim bilgileri yer alıyor. Online randevularda müşterinin randevusunu görüntüleyebileceği, saatini değiştirebileceği veya iptal edebileceği link de e-postaya ekleniyor.
- E-posta gönderimi için FluentEmail ve SMTP kullanıldı. Geliştirme ortamında gönderilen e-postaları kontrol etmek için smtp4dev kullanılıyor. SMS için henüz bir servis seçilmediğinden mesajlar log'a yazılıyor; telefon numarasının bir kısmı gizleniyor.
- Bildirim gönderilirken hata oluşsa bile randevu oluşturma işlemi devam ediyor. Oluşan hata log'a kaydediliyor.
- Randevudan 24 saat önce hatırlatma e-postası ve SMS gönderilmesi sağlandı. Arka plan servisi dakikada bir randevuları kontrol ediyor. Randevuya 24 saatten az kala oluşturulan randevular için hatırlatma gönderilmiyor. Randevu saati değiştirilirse hatırlatma yeni saate göre planlanıyor. E-posta gönderilemediğinde ise sonraki kontrolde tekrar deneniyor.
- Online randevu formuna isteğe bağlı e-posta alanı eklendi. Müşteri e-posta adresini girerse bu bilgi müşteri kaydına da kaydediliyor.
- Süresi dolmuş refresh token'ları temizlemek için günlük bir işlem eklendi. Kullanılmış ancak henüz süresi dolmamış token'lar, tekrar kullanım kontrolünün yapılabilmesi için saklanmaya devam ediyor.
- Bildirimlerle ilgili entegrasyon testleri yazıldı. Testlerde gerçek e-posta gönderimi yerine sahte bir servis kullanılıyor ve arka plan servisleri çalıştırılmıyor. Toplam test sayısı 109'a ulaştı.

## [0.8.0] - 2026-10-20

Aşama 5.2: Personel izinleri ve kapalı günler.

### Eklendi

- Personel izinleri ve salonun kapalı olduğu günler için izin sistemi eklendi. İzin belirli bir personele veya tüm salona verilebiliyor. Tam gün izin girilebildiği gibi belirli saatler için de izin oluşturulabiliyor. Bir izin en fazla 90 gün sürebiliyor.
- İzin ekleme, silme ve listeleme işlemleri için `POST /api/time-offs`, `DELETE /api/time-offs/{id}` ve OData üzerinden `timeOffs` uçları eklendi. Geçmiş tarihlere izin girilemiyor ve aynı personel için çakışan izinler kaydedilemiyor. İzin verilecek aralıkta onaylanmış randevular varsa işlem engelleniyor ve etkilenen randevular gösteriliyor.
- Boş saat hesaplamasına izinler de dahil edildi. Hem panelden hem de online randevu sayfasından, izinli bir personele veya salonun kapalı olduğu bir güne randevu oluşturulamıyor. Mevcut bir randevunun saati değiştirilirken de aynı kontrol yapılıyor.
- Panele İzinler sayfası eklendi. Yaklaşan izinler burada listeleniyor. İzin eklerken tüm gün seçeneğiyle tarih aralığı girilebiliyor veya tek bir gün için saat aralığı belirlenebiliyor. Randevuyla çakışma varsa uyarı form üzerinde gösteriliyor.
- Takvimde personel izinleri "İzinli", salonun kapalı olduğu günler ise "Salon kapalı" etiketiyle taralı bloklar şeklinde gösteriliyor. İzin açıklaması varsa blokta yer alıyor. Salonun kapalı olduğu günün özetinde de "Salon kapalı" rozeti gösteriliyor.
- İzinlerle ilgili entegrasyon testleri eklendi. Başka bir salonun izin kayıtlarına erişilemediği ve bir salonun kapalı günlerinin diğer salonların müsait saatlerini etkilemediği de test edildi. Toplam test sayısı 98'e ulaştı.

## [0.7.0] - 2026-10-19

Aşama 5.1: Salon ayarları.

### Eklendi

- Salonun adresi, telefonu ve online randevu kuralları için ayarlar eklendi. En fazla kaç gün sonrası için randevu alınabileceği, randevuya kaç saat kalana kadar linkten iptal edilebileceği ve aynı telefon numarasıyla en fazla kaç ileri tarihli online randevu alınabileceği ayarlanabiliyor. Mevcut salonlar 60 gün, 3 saat ve 5 randevu varsayılanlarıyla devam ediyor.
- Ayarları görüntülemek ve güncellemek için `GET` ve `PUT /api/settings` uçları eklendi. Girilebilecek değerler de sınırlandırıldı: 1–180 gün, 0–72 saat ve 1–20 randevu. Daha önce müşterilerle paylaşılmış linklerin değişmemesi için salonun kısa linki ayarlar üzerinden değiştirilemiyor.
- Randevu alırken ve boş saatleri listelerken salonun kendi kuralları kullanılmaya başlandı. İptal ve saat değiştirme işlemleri de aynı ayarlara göre çalışıyor.
- Panele Ayarlar sayfası eklendi. Salon adı, telefon, adres ve online randevu kuralları buradan değiştirilebiliyor. Değişiklik yapılmadığında Kaydet düğmesi aktif olmuyor. Kayıt sonrasında yan menüdeki salon adı da güncelleniyor.
- Salonun adres ve telefon bilgileri girilmişse online randevu sayfasında da gösteriliyor. Başlıkta ilçe ve il bilgisi, randevu linkinin sayfasında açık adres yer alıyor. Telefon numarasına tıklanınca arama başlatılabiliyor. Takvime eklenen `.ics` dosyasına da adres bilgisi eklendi.
- Demo salon için örnek adres ve telefon bilgileri tanımlandı.
- Ayarlar için entegrasyon testleri eklendi. Bir salonun ayarlarının diğer salonları etkilemediği ve online randevu sayfasının ilgili salonun kurallarına göre çalıştığı da kontrol ediliyor. Toplam test sayısı 86 oldu.

### Düzeltildi

- Hizmetler sayfasında başlıktaki bir yazım hatası nedeniyle "Hizmet ekle" düğmesi sağa hizalanmıyordu. Bu sorun düzeltildi.
- Randevu formunda iptal süresi her salon için sabit olarak 3 saat gösteriliyordu. Artık salonun belirlediği süre gösteriliyor. Süre 0 saat olduğunda ise "randevu saatine kadar" yazıyor.

## [0.6.0] - 2026-10-18

MVP: Kurulum rehberi ve demo verisi.

### Eklendi

- API geliştirme ortamında ilk kez çalıştırıldığında veritabanı boşsa "Demo Kuaför" adında örnek bir salon oluşturuluyor. Salonda 6 hizmet, 3 personel, 8 müşteri ve bir haftalık randevu verisi bulunuyor. Online alınan randevular da örneklere dahil edildi. Geçmiş randevuların bir kısmı tamamlandı, bir kısmı gelmedi olarak işaretlendi. Demo salon daha önce oluşturulmuşsa tekrar eklenmiyor.
- Demo salon oluşturulurken kayıt işlemlerinde kullanılan komut kullanıldı. Böylece şifre hash'leme ve salonla kayıtların ilişkilendirilmesi mevcut yapıyla aynı şekilde yapılıyor.
- README dosyasına proje özellikleri, kullanılan teknolojiler, kurulum adımları, demo hesap bilgileri, test komutları, klasör yapısı ve yapılması planlanan geliştirmeler eklendi.
- Proje, GitHub'dan yeni klonlanmış bir repo ve boş veritabanıyla kurularak test edildi. Kurulum sırasında eksik olduğu fark edilen `dotnet restore` adımı da README'ye eklendi.

### Düzeltildi

- Genel bakış sayfasındaki randevu linki kartında bulunan "yakında açılıyor" yazısı güncellendi. Kurulum henüz tamamlanmadıysa buna uygun bir açıklama gösteriliyor. Randevu sayfasını yeni sekmede açmak için "Aç" düğmesi eklendi.
- Randevu linki, dar kartlarda düzgün görünmüyordu. Kutunun çok fazla küçülmesi engellendi; alan yetmediğinde düğmelerin alt satıra geçmesi sağlandı.

## [0.5.0] - 2026-10-18

Aşama 4: Herkese açık randevu sayfası.

### Eklendi

- Müşterilerin giriş yapmadan, salonun kendine ait linki üzerinden randevu alabilmesi sağlandı. Müşteri hizmeti, personeli ve saati seçtikten sonra adını, soyadını ve telefon numarasını girerek randevusunu oluşturabiliyor. Sayfa telefon kullanımına uygun olacak şekilde hazırlandı.
- Salon bilgileri kısa link üzerinden getiriliyor. Panele başka bir salon hesabıyla giriş yapılmış olsa bile link hangi salona aitse o salonun bilgileri gösteriliyor.
- Online randevular en fazla 60 gün sonrası için alınabiliyor. Ayrıca takvimin gereksiz randevularla doldurulmasını önlemek için aynı telefon numarasıyla en fazla 5 ileri tarihli online randevu oluşturulabiliyor.
- Girilen telefon numarası ve ad soyad bilgileri mevcut bir müşteriyle eşleşirse o kayıt kullanılıyor. Eşleşme yoksa yeni müşteri oluşturuluyor. Böylece aynı telefon numarası kullanılarak farklı kişiler için de randevu alınabiliyor.
- Randevu oluşturulduktan sonra müşteriye randevusunu yönetebileceği özel bir link veriliyor. Bu link üzerinden randevu görüntülenebiliyor, randevuya 3 saat kalana kadar iptal edilebiliyor ve randevu başlamadan önce saat değiştirilebiliyor. Güvenlik için linkte kullanılan anahtar veritabanında doğrudan değil, özeti alınarak saklanıyor.
- Randevuyu telefonun takvimine eklemek için bir .ics dosyası oluşturuldu. Takvim kaydına randevudan 1 saat önce hatırlatma da eklendi.
- Herkese açık API uçlarına IP başına dakikada 60 istek sınırı getirildi.
- Online randevu oluşturma ve randevu yönetim linki işlemleri için entegrasyon testleri yazıldı. Toplam test sayısı 80'e çıktı.

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

[0.5.0]: https://github.com/ErenBirsin/zamanla/compare/v0.4.0...v0.5.0

[0.4.0]: https://github.com/ErenBirsin/zamanla/compare/v0.3.0...v0.4.0

[0.3.0]: https://github.com/ErenBirsin/zamanla/compare/v0.2.0...v0.3.0

[0.2.0]: https://github.com/ErenBirsin/zamanla/compare/v0.1.0...v0.2.0

[0.1.0]: https://github.com/ErenBirsin/zamanla/releases/tag/v0.1.0
