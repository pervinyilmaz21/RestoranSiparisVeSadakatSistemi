using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using RestoranSiparisVeSadakatSistemi.Data;
using RestoranSiparisVeSadakatSistemi.Models;
using RestoranSiparisVeSadakatSistemi.Services;

namespace RestoranSiparisVeSadakatSistemi
{
    public partial class SiparisYonetimForm : Form
    {
        DataTable sepetTablosu = new DataTable();
        private readonly OdemeServisi odemeServisi = new OdemeServisi();
        private readonly BildirimServisi bildirimServisi = new BildirimServisi();

        public SiparisYonetimForm()
        {
            InitializeComponent();
        }

        private void SiparisYonetimForm_Load(object sender, EventArgs e)
        {
            SepetTablosunuOlustur();
            UrunleriGetir();
            MusterileriGetir();
        }

        private void SepetTablosunuOlustur()
        {
            sepetTablosu.Columns.Add("UrunID", typeof(int));
            sepetTablosu.Columns.Add("UrunAdi", typeof(string));
            sepetTablosu.Columns.Add("BirimFiyat", typeof(decimal));
            sepetTablosu.Columns.Add("Adet", typeof(int));
            sepetTablosu.Columns.Add("ToplamTutar", typeof(decimal));

            dgwSepet.DataSource = sepetTablosu;
        }

        private void UrunleriGetir()
        {
            using (var db = new AppDbContext()) 
            {
                
                dgwUrunler.DataSource = db.Urunler.Select(u => new {
                    u.UrunID,
                    u.UrunAdi,
                    u.Fiyat,
                    u.KategoriID
                }).ToList();
            }
        }

        private void MusterileriGetir()
        {
            using (var db = new AppDbContext())
            {
                cmbMusteriler.DataSource = db.Musteriler.ToList();
                cmbMusteriler.DisplayMember = "AdSoyad";
                cmbMusteriler.ValueMember = "MusteriID";
            }
        }

       
        

        private void btnSepetiTemizle_Click(object sender, EventArgs e)
        {
            sepetTablosu.Rows.Clear();
            numericUpDownAdet.Value = 1;
            MessageBox.Show("Sepet temizlendi.");

        }

        private async void btnSiparisiTamamla_Click_1(object sender, EventArgs e)
        {
            // ---- 1) Kontroller ----
            if (cmbMusteriler.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir müşteri seçin!");
                return;
            }
            if (sepetTablosu.Rows.Count == 0)
            {
                MessageBox.Show("Sepetiniz boş!");
                return;
            }
            if (!mskKartNo.MaskCompleted) // Hafta 9: kart numarası 16 hane girilmeli
            {
                MessageBox.Show("Lütfen 16 haneli kart numarasını girin!");
                return;
            }

            // ---- 2) Toplam tutarı hesapla (ödemeden önce lazım) ----
            decimal genelToplam = 0;
            foreach (DataRow row in sepetTablosu.Rows)
            {
                genelToplam += Convert.ToInt32(row["Adet"]) * Convert.ToDecimal(row["BirimFiyat"]);
            }

            btnSiparisiTamamla.Enabled = false; // Ödeme sürerken tekrar basılmasın
            try
            {
                // ---- 3) Ödemeyi al (Hafta 9) ----
                OdemeSonucu odeme;
                try
                {
                    odeme = await odemeServisi.OdemeYapAsync(genelToplam, mskKartNo.Text);
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show("Ödeme alınamadı: " + ex.Message + " Lütfen tekrar deneyin.");
                    return;
                }

                if (!odeme.Basarili)
                {
                    MessageBox.Show("Ödeme başarısız: " + odeme.HataMesaji);
                    return; // Ödeme yoksa veritabanına hiçbir şey kaydedilmez
                }

                // ---- 4) Ödeme başarılı: sipariş, detaylar, puan ve ödemeyi kaydet ----
                int siparisNo;
                int kazanilanPuan;
                int toplamPuan;
                string musteriAd;
                string musteriEposta;

                using (var db = new AppDbContext())
                {
                    Siparis yeniSiparis = new Siparis();
                    yeniSiparis.MusteriID = Convert.ToInt32(cmbMusteriler.SelectedValue);
                    yeniSiparis.Tarih = DateTime.Now;
                    yeniSiparis.Durum = "Ödendi"; // Hafta 9: ödeme alındı
                    yeniSiparis.SiparisDetayları = new System.Collections.Generic.List<SiparisDetay>();

                    foreach (DataRow row in sepetTablosu.Rows)
                    {
                        SiparisDetay detay = new SiparisDetay
                        {
                            UrunID = Convert.ToInt32(row["UrunID"]),
                            Adet = Convert.ToInt32(row["Adet"]),
                            BirimFiyat = Convert.ToDecimal(row["BirimFiyat"])
                        };
                        yeniSiparis.SiparisDetayları.Add(detay);
                    }

                    yeniSiparis.ToplamTutar = genelToplam;

                    kazanilanPuan = (int)(genelToplam / 10);
                    yeniSiparis.KazanilanPuan = kazanilanPuan;

                    int musteriId = Convert.ToInt32(cmbMusteriler.SelectedValue);
                    var musteri = db.Musteriler.Find(musteriId);
                    if (musteri != null)
                    {
                        musteri.Toplampuan += kazanilanPuan;

                        PuanHareket puanHareketi = new PuanHareket
                        {
                            MusteriID = musteri.MusteriID,
                            Siparis = yeniSiparis, // ID yerine nesne: EF önce siparişi kaydedip oluşan SiparisID'yi buraya yazar
                            Puan = kazanilanPuan,
                            Tarih = DateTime.Now
                        };
                        db.PuanHareketleri.Add(puanHareketi);
                    }

                    // Hafta 9: ödeme kaydı (kart numarası kaydedilmez)
                    Odeme odemeKaydi = new Odeme
                    {
                        Siparis = yeniSiparis,
                        Tutar = genelToplam,
                        Durum = "Basarili",
                        IslemNo = odeme.IslemNo,
                        Tarih = DateTime.Now
                    };
                    db.Odemeler.Add(odemeKaydi);

                    db.Siparisler.Add(yeniSiparis);
                    db.SaveChanges(); // Hepsi tek seferde kaydedilir

                    siparisNo = yeniSiparis.SiparisID;
                    musteriAd = musteri?.AdSoyad;
                    musteriEposta = musteri?.Eposta;
                    toplamPuan = musteri?.Toplampuan ?? 0;
                }

                sepetTablosu.Rows.Clear();
                mskKartNo.Clear();

                // ---- 5) Müşteriye bildirim gönder (Mailtrap API) ----
                string konu = "Siparişiniz alındı - #" + siparisNo;
                string metin =
                    $"Merhaba {musteriAd},\n\n" +
                    $"#{siparisNo} numaralı siparişiniz alındı ve ödemeniz onaylandı.\n" +
                    $"Tutar: {genelToplam:N2} TL\n" +
                    $"İşlem No: {odeme.IslemNo}\n" +
                    $"Sipariş durumu: Ödendi\n" +
                    $"Kazandığınız puan: {kazanilanPuan}\n" +
                    $"Toplam puanınız: {toplamPuan}\n\n" +
                    "Afiyet olsun!";

                BildirimSonucu bildirim = await bildirimServisi.EpostaGonderAsync(musteriEposta, musteriAd, konu, metin);

                if (bildirim.Basarili)
                {
                    MessageBox.Show($"Sipariş oluşturuldu, ödeme alındı (İşlem No: {odeme.IslemNo}) ve müşteriye bildirim gönderildi.");
                }
                else
                {
                    // Bildirim gitmese de sipariş ve ödeme geçerlidir
                    MessageBox.Show("Sipariş oluşturuldu ve ödeme alındı, ancak bildirim gönderilemedi:\n" + bildirim.Mesaj,
                                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }
            finally
            {
                btnSiparisiTamamla.Enabled = true;
            }
        }

        private void btnSepeteEkle_Click_1(object sender, EventArgs e)
        {
            if (dgwUrunler.SelectedRows.Count > 0)
            {
                int urunId = Convert.ToInt32(dgwUrunler.SelectedRows[0].Cells["UrunID"].Value);
                string urunAdi = dgwUrunler.SelectedRows[0].Cells["UrunAdi"].Value.ToString();
                decimal fiyat = Convert.ToDecimal(dgwUrunler.SelectedRows[0].Cells["Fiyat"].Value);
                int adet = Convert.ToInt32(numericUpDownAdet.Value); 

                if (adet <= 0) // Hafta 8: adet seçilmeden sepete eklenmesin
                {
                    MessageBox.Show("Lütfen 0'dan büyük bir adet girin!");
                    return;
                }

                decimal toplamTutar = fiyat * adet;

                
                sepetTablosu.Rows.Add(urunId, urunAdi, fiyat, adet, toplamTutar);
            }
            else
            {
                MessageBox.Show("Lütfen sepete eklemek için bir ürün seçin!");
            }
        }
    }
}