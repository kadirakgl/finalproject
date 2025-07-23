# TaskManagement Mezuniyet Projesi

## Son Güncellemeler

- **Kullanıcıya Özel Dashboard:** Dashboard’daki grafikler artık giriş yapan kullanıcının kendi görev/proje/ticket durumlarını gösterir. Admin ve yöneticiler ise tüm sistemin özetini görebilir.
- **E-posta ile Tekil Kayıt:** Her e-posta adresiyle yalnızca bir kullanıcı kaydı yapılabilir. Kayıt sırasında aynı e-posta ile ikinci bir kullanıcı oluşturulamaz.
- **Gelişmiş Filtreleme ve Raporlama:** Görevler sayfasında modern, profesyonel ve kullanışlı bir filtreleme/raporlama paneli bulunur.
- **Modern Grafikler:** Dashboard’da görev, proje ve ticket durumlarını gösteren üçlü donut grafik paneli yer alır.
- **Bildirim Saatleri Düzeltildi:** Tüm bildirimler artık Türkiye saatine göre kaydediliyor ve arayüzde doğru şekilde gösteriliyor.
- **Kanban Sadece Adminlere Açık:** Kanban Board menüde ve erişimde sadece admin kullanıcılarına görünür ve erişilebilirdir.
- **İletişim Formu Gönderen Adresi:** İletişim formundan gönderilen e-postalarda gönderen adresi artık doğrudan admin adresi (kadirak.2001@hotmail.com) olarak ayarlanıyor.

## Yeni Özellikler ve Son Güncellemeler

- **İletişim Formu:** Anasayfadaki iletişim formu ile gönderilen mesajlar, sistem yöneticisinin e-posta adresine otomatik olarak iletilir. E-posta adresi güvenlik amacıyla kullanıcıya gösterilmez.
- **WhatsApp Entegrasyonu:** İletişim ve footer kısmındaki telefon numarasına tıklandığında, doğrudan WhatsApp sohbeti başlatılır.
- **Modern ve Renkli Arayüz:** Anasayfa, footer ve iletişim bölümü modern ve canlı renklerle yeniden tasarlandı.
- **Kullanıcı Onayı:** Yeni kayıt olan kullanıcılar, admin onayı olmadan giriş yapamaz.
- **Şifre Sıfırlama:** Kullanıcılar, e-posta ile token tabanlı şifre sıfırlama işlemi gerçekleştirebilir.
- **Brevo (Sendinblue) ile E-posta:** Tüm sistem e-posta bildirimleri Brevo API üzerinden gönderilir.

> Not: E-posta gönderimi için `appsettings.json` dosyasındaki Brevo API anahtarınızı girmeniz gerekmektedir.

## Proje Açıklaması
Kullanıcı, proje ve görev yönetimi sağlayan, modern arayüze sahip, gerçek zamanlı bildirim destekli bir iş yönetim sistemi.

## Mimari Diyagram
```
flowchart TD
    A[Kullanıcı] -->|Web| B[ASP.NET Core MVC/API]
    B --> C[Service Katmanı]
    C --> D[Repository/DataAccess Katmanı]
    D --> E[SQLite Veritabanı]
    B --> F[SignalR Bildirim Hub]
    B --> G[Swagger/OpenAPI]
```

## Kullanılan Teknolojiler
- ASP.NET Core MVC & Web API
- Dapper (Veritabanı erişimi)
- SQLite
- SignalR (Gerçek zamanlı bildirim)
- JWT & Cookie Authentication
- Swagger (API dokümantasyonu)
- Bootstrap, SB Admin 2 (Arayüz)
- xUnit (Test)

## Ana Özellikler
- Dashboard: Görev/proje/kullanıcı özetleri, görev dağılımı tablosu, bildirimler
- Gerçek zamanlı bildirimler (SignalR)
- Rol bazlı yetkilendirme (Admin, Manager, User)
- Görev ve proje bitirme akışı, bildirim iletimi
- Dosya yükleme ve önizleme
- Kanban, takvim, raporlama
- Modern ve mobil uyumlu arayüz

## Demo Kullanıcılar
| Kullanıcı Adı   | Şifre | Rol    |
|----------------|-------|---------|
| admin          | 1234  | Admin   |
| demo_user      | 1234  | User    |

## Demo Veri
- Sistem ilk açıldığında admin kullanıcısı otomatik eklenir.
- Demo projeler/görevler için seed fonksiyonu eklenmiştir (bkz: DapperContext.cs).

## Kurulum ve Çalıştırma
1. Projeyi çalıştır: `dotnet run --project TaskManagement/TaskManagement.csproj`
2. http://localhost:5284 adresine git
3. Yukarıdaki demo kullanıcılarla giriş yapabilirsin.

## API ve Swagger
- API endpointleri için: `http://localhost:5284/swagger`
- Swagger arayüzü ile tüm API'leri test edebilirsin.

## Testler
- Testler xUnit ile yazılmıştır.
- Testleri çalıştırmak için:
  ```
  dotnet test TaskManagement.Tests/TaskManagement.Tests.csproj
  ```
- Test kapsamı: TaskService, UserService, ProjectService birim testleri.

## Deployment
- Publish için:
  ```
  dotnet publish TaskManagement/TaskManagement.csproj -c Release -o ./publish
  ```
- Ortam değişkenleri ve veritabanı dosyası için `appsettings.json` ve `TaskManagement.db` dosyalarını kontrol edin.

## Katkılar ve Öğrendiklerim
- Katmanlı mimari, gerçek zamanlı bildirim, JWT/Cookie authentication, test yazımı, Swagger/OpenAPI, responsive arayüz, seed/migration işlemleri.
- Proje boyunca .NET Core ekosisteminde tam yığın geliştirme deneyimi kazandım.

## Ekran Görüntüsü/Demo
- Sunumda canlı demo veya kısa bir video ile gösterilmesi önerilir.
- Örnek ekran görüntüleri: (buraya sunumdan önce ekleyebilirsin)

## İletişim Formu Kullanımı

- Anasayfadaki iletişim formunu dolduran kullanıcıların mesajları, sistem yöneticisinin e-posta adresine iletilir.
- Telefon numarasına tıklandığında WhatsApp sohbeti başlatılır.
- E-posta adresi güvenlik nedeniyle sayfada gösterilmez.

## Notlar
- Dashboard ana ekrandır ve sunumda ilk gösterilmesi önerilir.
- Demo içerik yüklüdür, sistemde örnek görev/proje/kullanıcı gözükecektir.