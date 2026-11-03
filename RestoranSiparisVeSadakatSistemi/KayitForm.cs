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
using RestoranSiparisVeSadakatSistemi.Models;

namespace RestoranSiparisVeSadakatSistemi
{
    public partial class KayitForm : Form
    {
        public KayitForm()
        {
            InitializeComponent();
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            using (var db = new AppDbContext())
            {
                if(string.IsNullOrWhiteSpace(txtAdSoyad.Text)||
                    string.IsNullOrWhiteSpace(txtKullaniciAdi2.Text)||
                    string.IsNullOrWhiteSpace(txtSifre2.Text))
                {
                    MessageBox.Show("Lütfen zorunlu alanları doldurun!","Uyarı",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Musteri yeniMusteri = new Musteri
                {
                    AdSoyad = txtAdSoyad.Text,
                    Eposta = txtKullaniciAdi2.Text,
                    Telefon = txtTelefon.Text,
                    SifreHash = SifreYardimcisi.SifreHashle(txtSifre2.Text),
                    Rol = "Musteri",
                    Toplampuan = 0
                };
                db.Musteriler.Add(yeniMusteri);
                db.SaveChanges();

                MessageBox.Show("Kayıt başarıyla oluşturuldu! Giriş yapabilirsiniz.","Bilgi",MessageBoxButtons.OK,MessageBoxIcon.Information);
                this.Close();
            }
        }

        private void txtKullaniciAdi2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
