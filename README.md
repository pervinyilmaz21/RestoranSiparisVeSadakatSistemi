\#P40 - Restoran Sipariş ve Sadakat Sistemi



Küçük restoranlar için menü, online sipariş ve puan tabanlı sadakat programını yöneten masaüstü uygulaması.



\##Amaç 

menü, online sipariş ve sadakat puanını tek bir sistemde yönetmek.



\##Hedef Kullanıcılar

Restoranlar ve Müşteriler.



\##Temel Modüller

\-Menü yönetimi

\-Sipariş/sepet

\-Sadakat puanı

\-Ödeme

\-Bildirim

\-Satış raporu



\##Yapay Zeka Bileşeni

Geçmiş siparişlere göre kişisel menü ve kampanya önerisi.



\##Teknolojiler

\-Dil: C# (.NET Framework)

\-Arayüz: Windows Forms

Veritabanı:Microsoft SQL Server

\- ORM: Entity Framework (Code First, Migrations)

\- Geliştirme Ortamı: Visual Studio

\- Sürüm Kontrolü: Git \& GitHub



\## Mimari

Katmanlı yapı:

\- \*\*Models:\*\* Varlık sınıfları (Musteri, Siparis, Urun, Kategori vb.)

\- \*\*Data:\*\* Veritabanı bağlantısı (AppDbContext)

\- \*\*Migrations:\*\* Veritabanı şeması değişiklikleri

\- \*\*Formlar:\*\* Kullanıcı arayüzü



\## Haftalık İlerleme

| Hafta | Konu | Yapılanlar |

|---|---|---|

| 3 | Kurulum ve Veri Modeli | Proje iskeleti, klasör yapısı, GitHub deposu, ER diyagramı |

| 4 | Veritabanı ve Veri Erişimi | Tablolar, ilişkiler, kısıtlar, örnek veri, CRUD işlemleri |

| 5 | Kullanıcı Yönetimi | Kayıt/giriş, parola hash'leme, şifre sıfırlama, rol tabanlı erişim |

| 6 | Menü Yönetimi | Kategori ve ürün modeli, fiyat, ürün uygunluğu, doğrulama |



