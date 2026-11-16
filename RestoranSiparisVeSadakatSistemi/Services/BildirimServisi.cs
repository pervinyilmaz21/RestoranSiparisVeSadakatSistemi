using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RestoranSiparisVeSadakatSistemi.Services
{
    public class BildirimSonucu
    {
        public bool Basarili { get; set; }
        public string Mesaj { get; set; }
    }

    // Mailtrap Email Sandbox API'si ile müşteriye sipariş bildirimi (e-posta) gönderir
    public class BildirimServisi
    {
        private const string ApiAdresi = "https://sandbox.api.mailtrap.io/api/send/";

        // HttpClient tek sefer oluşturulup tekrar kullanılır; 10 saniye zaman aşımı
        private static readonly HttpClient istemci = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        static BildirimServisi()
        {
            // Güvenli bağlantı için TLS 1.2 kullan
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public async Task<BildirimSonucu> EpostaGonderAsync(string aliciEposta, string aliciAd, string konu, string metin)
        {
            // 1) API anahtarı ve Inbox ID kodda değil, ortam değişkenlerinden okunur
            string token = Environment.GetEnvironmentVariable("MAILTRAP_API_TOKEN");
            string inboxId = Environment.GetEnvironmentVariable("MAILTRAP_INBOX_ID");

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(inboxId))
                return new BildirimSonucu { Basarili = false, Mesaj = "API anahtarı veya Inbox ID ortam değişkenlerinde bulunamadı." };

            if (string.IsNullOrWhiteSpace(aliciEposta))
                return new BildirimSonucu { Basarili = false, Mesaj = "Müşterinin e-posta adresi kayıtlı değil." };

            // 2) Gönderilecek veriyi JSON'a çevir
            var govde = new
            {
                from = new { email = "siparis@restoran-sadakat.test", name = "Restoran Sadakat" },
                to = new[] { new { email = aliciEposta, name = aliciAd } },
                subject = konu,
                text = metin,
                category = "Siparis Bildirimi"
            };
            string json = JsonConvert.SerializeObject(govde);

            // 3) HTTP isteğini hazırla
            var istek = new HttpRequestMessage(HttpMethod.Post, ApiAdresi + inboxId.Trim());
            istek.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
            istek.Headers.UserAgent.ParseAdd("RestoranSadakat/1.0");
            istek.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // 4) İsteği gönder ve yanıtı oku
                HttpResponseMessage yanit = await istemci.SendAsync(istek);
                string yanitMetni = await yanit.Content.ReadAsStringAsync();

                // 5) Yanıtı (JSON) ayrıştır: { "success": true, "message_ids": [...] }
                //    veya hata: { "success": false, "errors": ["..."] }
                JObject sonuc = null;
                try { sonuc = JObject.Parse(yanitMetni); }
                catch (JsonReaderException) { /* yanıt JSON değilse aşağıda durum koduna bakılır */ }

                if (yanit.IsSuccessStatusCode && sonuc != null && (bool?)sonuc["success"] == true)
                    return new BildirimSonucu { Basarili = true, Mesaj = "Bildirim e-postası gönderildi." };

                string hata = sonuc?["errors"]?.First?.ToString() ?? yanit.ReasonPhrase;

                switch ((int)yanit.StatusCode)
                {
                    case 401:
                        return new BildirimSonucu { Basarili = false, Mesaj = "Yetkisiz istek (401): API anahtarı hatalı." };
                    case 429:
                        return new BildirimSonucu { Basarili = false, Mesaj = "Çok fazla istek (429): gönderim sınırı aşıldı, biraz sonra tekrar deneyin." };
                    default:
                        return new BildirimSonucu { Basarili = false, Mesaj = $"Bildirim gönderilemedi ({(int)yanit.StatusCode}): {hata}" };
                }
            }
            catch (TaskCanceledException)
            {
                return new BildirimSonucu { Basarili = false, Mesaj = "Bildirim servisi 10 saniye içinde yanıt vermedi (zaman aşımı)." };
            }
            catch (HttpRequestException)
            {
                return new BildirimSonucu { Basarili = false, Mesaj = "Bildirim servisine bağlanılamadı. İnternet bağlantınızı kontrol edin." };
            }
        }
    }
}