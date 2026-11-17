using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestoranSiparisVeSadakatSistemi.Data;

namespace RestoranSiparisVeSadakatSistemi.Services
{
    public class OneriSonucu
    {
        public bool YapayZekaMi { get; set; }
        public List<string> Urunler { get; set; } = new List<string>();
        public List<string> Nedenler { get; set; } = new List<string>();
        public string Kampanya { get; set; }
        public string Bilgi { get; set; }
    }

    public class OneriServisi
    {
        private static readonly HttpClient istemci = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        private const string VarsayilanModel = "gemini-3.5-flash-lite";
        private const string VarsayilanKampanya = "Bugünkü siparişine önerilen ürünlerden birini eklersen sadakat puanı kazanmaya devam edersin!";

        static OneriServisi()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private class MenuUrunu
        {
            public string UrunAdi { get; set; }
            public string KategoriAdi { get; set; }
            public decimal Fiyat { get; set; }
        }

        public async Task<OneriSonucu> OneriAlAsync(int musteriId)
        {
            List<MenuUrunu> menu;
            List<string> gecmis;
            List<string> enCokSatanlar;

            // 1) Veritabanindan menu, musterinin gecmisi ve en cok satanlar
            using (var db = new AppDbContext())
            {
                menu = (from u in db.Urunler
                        join k in db.Kategoriler on u.KategoriID equals k.KategoriID
                        where u.AktifMi
                        select new MenuUrunu
                        {
                            UrunAdi = u.UrunAdi,
                            KategoriAdi = k.KategoriAdi,
                            Fiyat = u.Fiyat
                        }).ToList();

                var gecmisVerisi = (from d in db.SiparisDetaylari
                                    join s in db.Siparisler on d.SiparisID equals s.SiparisID
                                    join u in db.Urunler on d.UrunID equals u.UrunID
                                    where s.MusteriID == musteriId
                                    group d by u.UrunAdi into g
                                    orderby g.Sum(x => x.Adet) descending
                                    select new { UrunAdi = g.Key, Toplam = g.Sum(x => x.Adet) })
                                    .Take(10).ToList();

                gecmis = gecmisVerisi.Select(x => x.UrunAdi + ": " + x.Toplam + " adet").ToList();

                enCokSatanlar = (from d in db.SiparisDetaylari
                                 join u in db.Urunler on d.UrunID equals u.UrunID
                                 where u.AktifMi
                                 group d by u.UrunAdi into g
                                 orderby g.Sum(x => x.Adet) descending
                                 select g.Key).Take(3).ToList();
            }

            if (menu.Count == 0)
            {
                return new OneriSonucu { Kampanya = VarsayilanKampanya, Bilgi = "Menüde aktif ürün bulunmuyor." };
            }

            // 2) Yeni musteri: yapay zeka yerine en cok satanlar
            if (gecmis.Count == 0)
            {
                return YedekOneri(menu, enCokSatanlar, "Bu müşterinin henüz siparişi yok; en çok satan ürünler önerildi.");
            }

            // 3) API anahtari ve model ortam degiskeninden
            string anahtar = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(anahtar))
            {
                return YedekOneri(menu, enCokSatanlar, "GEMINI_API_KEY bulunamadı; yedek öneri gösterildi.");
            }

            string model = Environment.GetEnvironmentVariable("GEMINI_MODEL");
            if (string.IsNullOrWhiteSpace(model))
            {
                model = VarsayilanModel;
            }

            // 4) Istem (prompt) - kisisel veri YOK, sadece menu ve urun adetleri
            string sistemIstemi =
                "Sen bir restoranın sadakat programı için çalışan öneri asistanısın. " +
                "Sana restoranın menüsü ve bir müşterinin geçmiş sipariş özeti verilecek. " +
                "Kurallar: " +
                "1) Yalnızca menüde yazan ürünleri, adlarını menüdeki gibi aynen yazarak öner. " +
                "2) En fazla 3 farklı ürün öner, aynı ürünü iki kez yazma; menüde 3'ten az ürün varsa olanları öner. müşterinin sevdiği kategorilere yakın ama mümkünse daha önce denemediği ürünleri seç. " +
                "3) Her öneri için en fazla 15 kelimelik Türkçe bir neden yaz. " +
                "4) Müşteriye özel, en fazla 2 cümlelik Türkçe bir kampanya metni yaz; indirim oranı en fazla %10 olsun. " +
                "5) Yanıtı yalnızca şu JSON biçiminde ver, başka hiçbir şey yazma: " +
                "{\"oneriler\":[{\"urun\":\"...\",\"neden\":\"...\"}],\"kampanya\":\"...\"}";

            var sb = new StringBuilder();
            sb.AppendLine("Menü:");
            foreach (MenuUrunu m in menu)
            {
                sb.AppendLine("- " + m.UrunAdi + " (" + m.KategoriAdi + ") - " + m.Fiyat.ToString("0.00") + " TL");
            }
            sb.AppendLine();
            sb.AppendLine("Müşterinin geçmiş siparişleri (ürün: toplam adet):");
            foreach (string satir in gecmis)
            {
                sb.AppendLine("- " + satir);
            }
            string kullaniciIstemi = sb.ToString();

            System.Diagnostics.Debug.WriteLine("=== GEMINI ISTEM ===\n" + kullaniciIstemi);

            var govde = new
            {
                system_instruction = new { parts = new[] { new { text = sistemIstemi } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = kullaniciIstemi } } } }
            };
            string json = JsonConvert.SerializeObject(govde);
            string url = "https://generativelanguage.googleapis.com/v1beta/models/" + model + ":generateContent";

            // 5) Gemini API cagrisi
            try
            {
                using (var istek = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    istek.Headers.Add("x-goog-api-key", anahtar);
                    istek.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    using (HttpResponseMessage yanit = await istemci.SendAsync(istek))
                    {
                        string yanitMetni = await yanit.Content.ReadAsStringAsync();

                        if (!yanit.IsSuccessStatusCode)
                        {
                            int kod = (int)yanit.StatusCode;
                            string neden;
                            switch (kod)
                            {
                                case 400: neden = "İstek geçersiz ya da API anahtarı hatalı (400)."; break;
                                case 401:
                                case 403: neden = "API anahtarı geçersiz veya yetkisiz (" + kod + ")."; break;
                                case 404: neden = "Model bulunamadı (404): " + model; break;
                                case 429: neden = "Ücretsiz kullanım sınırı doldu (429)."; break;
                                default: neden = "Yapay zekâ servisi hata verdi (" + kod + ")."; break;
                            }
                            return YedekOneri(menu, enCokSatanlar, neden + " Yedek öneri gösterildi.");
                        }

                        string cikti = MetniAl(yanitMetni);
                        System.Diagnostics.Debug.WriteLine("=== GEMINI CIKTI ===\n" + cikti);

                        OneriSonucu sonuc = CiktiyiDogrula(cikti, menu);
                        if (sonuc == null)
                        {
                            return YedekOneri(menu, enCokSatanlar, "Yapay zekâ çıktısı geçersizdi; yedek öneri gösterildi.");
                        }
                        return sonuc;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return YedekOneri(menu, enCokSatanlar, "Yapay zekâ servisi zamanında yanıt vermedi (zaman aşımı); yedek öneri gösterildi.");
            }
            catch (HttpRequestException)
            {
                return YedekOneri(menu, enCokSatanlar, "İnternet bağlantısı yok veya servise ulaşılamadı; yedek öneri gösterildi.");
            }
        }

        // Gemini yanitindan metni cikarir: candidates[0].content.parts[].text
        private static string MetniAl(string yanitMetni)
        {
            try
            {
                JObject kok = JObject.Parse(yanitMetni);
                JArray parcalar = kok.SelectToken("candidates[0].content.parts") as JArray;
                if (parcalar == null)
                {
                    return null;
                }

                var sb = new StringBuilder();
                foreach (JToken p in parcalar)
                {
                    if ((bool?)p["thought"] == true)
                    {
                        continue;
                    }
                    sb.Append((string)p["text"]);
                }
                return sb.ToString();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        // Yapay zeka ciktisini dogrular: sadece menude olan urunler, en fazla 3, indirim en fazla %10
        private static OneriSonucu CiktiyiDogrula(string cikti, List<MenuUrunu> menu)
        {
            if (string.IsNullOrWhiteSpace(cikti))
            {
                return null;
            }

            int bas = cikti.IndexOf('{');
            int son = cikti.LastIndexOf('}');
            if (bas < 0 || son <= bas)
            {
                return null;
            }

            JObject veri;
            try
            {
                veri = JObject.Parse(cikti.Substring(bas, son - bas + 1));
            }
            catch (JsonException)
            {
                return null;
            }

            var sonuc = new OneriSonucu { YapayZekaMi = true };

            JArray oneriler = veri["oneriler"] as JArray;
            if (oneriler != null)
            {
                foreach (JToken o in oneriler)
                {
                    if (o.Type != JTokenType.Object)
                    {
                        continue;
                    }

                    string ad = ((string)o["urun"] ?? "").Trim();
                    MenuUrunu eslesen = menu.FirstOrDefault(m =>
                        string.Equals(m.UrunAdi.Trim(), ad, StringComparison.OrdinalIgnoreCase));

                    if (eslesen == null || sonuc.Urunler.Contains(eslesen.UrunAdi))
                    {
                        continue;
                    }

                    sonuc.Urunler.Add(eslesen.UrunAdi);
                    sonuc.Nedenler.Add(((string)o["neden"] ?? "").Trim());

                    if (sonuc.Urunler.Count == 3)
                    {
                        break;
                    }
                }
            }

            if (sonuc.Urunler.Count == 0)
            {
                return null;
            }

            string kampanya = ((string)veri["kampanya"] ?? "").Trim();
            if (kampanya.Length > 300)
            {
                kampanya = kampanya.Substring(0, 300);
            }

            Match eslesme = Regex.Match(kampanya, @"%\s*(\d+)|(\d+)\s*%");
            if (eslesme.Success)
            {
                string sayi = eslesme.Groups[1].Success ? eslesme.Groups[1].Value : eslesme.Groups[2].Value;
                if (int.Parse(sayi) > 10)
                {
                    kampanya = "";
                }
            }

            sonuc.Kampanya = kampanya == "" ? VarsayilanKampanya : kampanya;
            sonuc.Bilgi = "Öneriler yapay zekâ ile üretildi ve menüye göre kontrol edildi.";
            return sonuc;
        }

        // Yedek (kural tabanli) oneri: en cok satanlar, eksikse menuden tamamla
        private static OneriSonucu YedekOneri(List<MenuUrunu> menu, List<string> enCokSatanlar, string bilgi)
        {
            var sonuc = new OneriSonucu
            {
                YapayZekaMi = false,
                Kampanya = VarsayilanKampanya,
                Bilgi = bilgi
            };

            foreach (string ad in enCokSatanlar)
            {
                sonuc.Urunler.Add(ad);
                sonuc.Nedenler.Add("En çok tercih edilen ürünlerimizden.");
            }

            foreach (MenuUrunu m in menu)
            {
                if (sonuc.Urunler.Count >= 3)
                {
                    break;
                }
                if (sonuc.Urunler.Contains(m.UrunAdi))
                {
                    continue;
                }
                sonuc.Urunler.Add(m.UrunAdi);
                sonuc.Nedenler.Add("Menümüzden bir öneri.");
            }

            return sonuc;
        }
    }
}