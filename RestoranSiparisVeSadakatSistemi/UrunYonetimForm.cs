using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestoranSiparisVeSadakatSistemi.Data;
using RestoranSiparisVeSadakatSistemi.Models;

namespace RestoranSiparisVeSadakatSistemi
{
    public partial class UrunYonetimForm : Form
    {
        public UrunYonetimForm()
        {
            InitializeComponent();
        }

        private void UrunYonetimForm_Load(object sender, EventArgs e)
        {
            KategorileriYukle();
            UrunleriListele();
        }

        private void KategorileriYukle()
        {
            using (var db = new AppDbContext())
            {
                if (!db.Kategoriler.Any())
                {
                    db.Kategoriler.Add(new Kategori { KategoriAdi = "Ana Yemekler" });
                    db.Kategoriler.Add(new Kategori { KategoriAdi = "Çorbalar" });
                    db.Kategoriler.Add(new Kategori { KategoriAdi = "İçecekler" });
                    db.Kategoriler.Add(new Kategori { KategoriAdi = "Tatlılar" });
                    db.SaveChanges();
                }

                cmbKategoriler.DataSource = db.Kategoriler.ToList();
                cmbKategoriler.DisplayMember = "KategoriAdi";
                cmbKategoriler.ValueMember = "KategoriID";
                cmbKategoriler.SelectedIndex = -1;

            }


        }

        private void UrunleriListele()
        {
            using (var db = new AppDbContext())
            {
                var liste = db.Urunler
                    .Select(u => new
                    {
                        u.UrunID,
                        u.UrunAdi,
                        u.Aciklama,
                        u.Fiyat,
                        Kategori = u.Kategori.KategoriAdi,
                        u.AktifMi
                    })
                .ToList();

                dataGridViewUrunler.DataSource = liste;
            }
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUrunAdi.Text) || string.IsNullOrWhiteSpace(txtFiyat.Text))
                {
                    MessageBox.Show("Ürün adı ve fiyat alanları boş bırakılamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!decimal.TryParse(txtFiyat.Text, out decimal fiyat) || fiyat <= 0)
                {
                    MessageBox.Show("Lütfen geçerli ve 0'dan büyük bir fiyat giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using (var db = new AppDbContext())
                {
                    var yeniUrun = new Urun
                    {
                        UrunAdi = txtUrunAdi.Text.Trim(),
                        Aciklama = txtAciklama.Text.Trim(),
                        Fiyat = fiyat,
                        KategoriID = (int)cmbKategoriler.SelectedValue,
                        AktifMi = chkAktif.Checked
                    };

                    db.Urunler.Add(yeniUrun);
                    db.SaveChanges();

                    MessageBox.Show("Ürün başarıyla elendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtUrunAdi.Clear();
                    txtAciklama.Clear();
                    txtFiyat.Clear();
                    chkAktif.Checked = true;
                    UrunleriListele();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu:" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    


        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (dataGridViewUrunler.CurrentRow != null)
            {
                int secilenID = Convert.ToInt32(dataGridViewUrunler.CurrentRow.Cells["UrunID"].Value);

                using (var db = new AppDbContext())

                {
                    var urun = db.Urunler.Find(secilenID);
                    if (urun != null)
                    {
                        urun.UrunAdi = txtUrunAdi.Text;
                        if(decimal.TryParse(txtFiyat.Text,out decimal fiyat))
                        {
                            urun.Fiyat = fiyat;
                        }
                        else
                        {
                            MessageBox.Show("Lütfen geçerli bir fiyat giriniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (cmbKategoriler.SelectedValue != null && int.TryParse(cmbKategoriler.SelectedValue.ToString(),out int KategoriID))
                        {
                            urun.KategoriID= KategoriID;
                        }
                       
                        

                        db.SaveChanges();
                        MessageBox.Show("Ürün başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UrunleriListele();
                    }
                }
            }
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (dataGridViewUrunler.CurrentRow != null)
            {
                DialogResult onay = MessageBox.Show("Bu ürünü silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (onay== DialogResult.Yes)
                {
                    int secilenID = Convert.ToInt32(dataGridViewUrunler.CurrentRow.Cells["UrunID"].Value);

                    using(var db = new AppDbContext())
                    {
                        var urun =db.Urunler.Find( secilenID);
                        if (urun != null)
                        {
                            db.Urunler.Remove(urun);
                            db.SaveChanges();
                            MessageBox.Show("Ürün silindi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            UrunleriListele();
                        }
                    }
                }

             }
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            txtUrunAdi.Clear();
            txtAciklama.Clear();
            txtFiyat.Clear();
            cmbKategoriler.SelectedIndex = -1;
            chkAktif.Checked = false;
            dataGridViewUrunler.ClearSelection();
        }

        private void dataGridViewUrunler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridViewUrunler.Rows[e.RowIndex];

                if (row.Cells["UrunID"].Value != null && int.TryParse(row.Cells["UrunID"].Value.ToString(),out int secilenID))
                {
                    using(var db = new AppDbContext())
                    {
                        var urun = db.Urunler.Find(secilenID);
                        if (urun != null)
                        {
                            txtUrunAdi.Text = urun.UrunAdi;
                            txtAciklama.Text = urun.Aciklama ?? "";
                            txtFiyat.Text = urun.Fiyat.ToString();
                            cmbKategoriler.SelectedValue = urun.KategoriID;
                            chkAktif.Checked = urun.AktifMi;
                        }
                    }
                }

                
            }
        }

        private void btnSipariseGec_Click(object sender, EventArgs e)
        {
            SiparisYonetimForm SiparisFormu=new SiparisYonetimForm();
            SiparisFormu.Show();
            this.Hide();
        }
    }
}
