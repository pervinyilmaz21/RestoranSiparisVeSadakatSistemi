using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestoranSiparisVeSadakatSistemi.Data;

namespace RestoranSiparisVeSadakatSistemi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnKayitOl_Click(object sender, EventArgs e)
        {
            KayitForm kayit = new KayitForm();
            kayit.ShowDialog();
        }

        private void btnGirisYap_Click(object sender, EventArgs e)
        {
            using (var db = new AppDbContext())

            {
                string girilenEposta = txtKullaniciAdi.Text.Trim();
                string girilenSifre = txtSifre.Text.Trim();
                string sifrelenmisHali = SifreYardimcisi.SifreHashle(girilenSifre);
                var kullanici = db.Musteriler
                    .FirstOrDefault(m => m.Eposta == girilenEposta && m.SifreHash == sifrelenmisHali);
                if (kullanici != null)
                {
                    MessageBox.Show($"Hoş geldiniz, {kullanici.AdSoyad}!", "Giriş Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UrunYonetimForm yonetimForm= new UrunYonetimForm();
                    yonetimForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        
        }

        private void lnkSifremiUnuttum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SifreSifirlamaForm sifreFormu = new SifreSifirlamaForm();
            sifreFormu.ShowDialog();
        }
    }
}
