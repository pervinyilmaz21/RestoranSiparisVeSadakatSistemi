using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using RestoranSiparisVeSadakatSistemi.Data;
using RestoranSiparisVeSadakatSistemi.Models; // Kendi projenin namespace'i

namespace RestoranSiparisVeSadakatSistemi
{
    public partial class SiparisYonetimForm : Form
    {
        DataTable sepetTablosu = new DataTable();

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
            using (var db = new AppDbContext()) // Context sınıf adını projene göre kontrol et
            {
                // Menüdeki ürünleri grid'e dolduruyoruz
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

        private void btnSiparisiTamamla_Click_1(object sender, EventArgs e)
        {
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

            try
            {
                using (var db = new AppDbContext())
                {
                    // 1. Yeni Sipariş Nesnesi Oluşturuyoruz
                    Siparis yeniSiparis = new Siparis();
                    yeniSiparis.MusteriID = Convert.ToInt32(cmbMusteriler.SelectedValue);
                    yeniSiparis.Tarih = DateTime.Now;
                    yeniSiparis.Durum = "Hazırlanıyor";
                    yeniSiparis.SiparisDetayları = new System.Collections.Generic.List<SiparisDetay>();

                    decimal genelToplam = 0;

                    // 2. Sepetteki ürünleri SiparisDetay olarak ekliyoruz
                    foreach (DataRow row in sepetTablosu.Rows)
                    {
                        int urunId = Convert.ToInt32(row["UrunID"]);
                        int adet = Convert.ToInt32(row["Adet"]);
                        decimal birimFiyat = Convert.ToInt32(row["BirimFiyat"]); // modelindeki alan adları

                        SiparisDetay detay = new SiparisDetay
                        {
                            UrunID = urunId,
                            Adet = adet,
                            BirimFiyat = birimFiyat
                        };

                        yeniSiparis.SiparisDetayları.Add(detay);
                        genelToplam += (adet * birimFiyat);
                    }

                    yeniSiparis.ToplamTutar = genelToplam;

                    // 3. Sadakat Puanı Hesaplama (Örn: Her 10 TL'ye 1 Puan)
                    int kazanilanPuan = (int)(genelToplam / 10);
                    yeniSiparis.KazanilanPuan = kazanilanPuan;

                    // Müşterinin puanını güncelliyoruz
                    int musteriId = Convert.ToInt32(cmbMusteriler.SelectedValue);
                    var musteri = db.Musteriler.Find(musteriId);
                    if (musteri != null)
                    {
                        musteri.Toplampuan += kazanilanPuan;

                        // Puan Hareketleri tablosuna kayıt atıyoruz
                        PuanHareket puanHareketi = new PuanHareket
                        {
                            MusteriID = musteri.MusteriID,
                            SiparisID = yeniSiparis.SiparisID,
                            Puan = kazanilanPuan,
                            Tarih = DateTime.Now
                        };
                        db.PuanHareketleri.Add(puanHareketi);
                    }

                    db.Siparisler.Add(yeniSiparis);
                    db.SaveChanges();

                    MessageBox.Show("Sipariş başarıyla oluşturuldu ve sadakat puanları işlendi!");
                    sepetTablosu.Rows.Clear(); // Sepeti sıfırla
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }
        }

        private void btnSepeteEkle_Click_1(object sender, EventArgs e)
        {
            if (dgwUrunler.SelectedRows.Count > 0)
            {
                int urunId = Convert.ToInt32(dgwUrunler.SelectedRows[0].Cells["UrunID"].Value);
                string urunAdi = dgwUrunler.SelectedRows[0].Cells["UrunAdi"].Value.ToString();
                decimal fiyat = Convert.ToDecimal(dgwUrunler.SelectedRows[0].Cells["Fiyat"].Value);
                int adet = Convert.ToInt32(numericUpDownAdet.Value); // NumericUpDown kontrolü olduğunu varsayıyoruz

                decimal toplamTutar = fiyat * adet;

                // Sepete satır ekle
                sepetTablosu.Rows.Add(urunId, urunAdi, fiyat, adet, toplamTutar);
            }
            else
            {
                MessageBox.Show("Lütfen sepete eklemek için bir ürün seçin!");
            }
        }
    }
}