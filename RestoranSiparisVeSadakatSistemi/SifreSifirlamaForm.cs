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
    public partial class SifreSifirlamaForm : Form
    {
        public SifreSifirlamaForm()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSifreGuncelle_Click(object sender, EventArgs e)
        {
            string eposta = txtEpostaKullaniciAdi.Text.Trim();
            string telefon=txtTelefon2.Text.Trim();
            string yeniSifre=txtYeniSifre.Text.Trim();

            if (string.IsNullOrEmpty(eposta) || string.IsNullOrEmpty(telefon) || string.IsNullOrEmpty(yeniSifre))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            using (var db=new AppDbContext())
            {
                var musteri=db.Musteriler.FirstOrDefault(m => m.Eposta == eposta && m.Telefon == telefon);

                if (musteri != null)
                {
                    musteri.SifreHash = SifreYardimcisi.SifreHashle(yeniSifre);
                    db.SaveChanges();

                    MessageBox.Show("Şifreniz başarıyla güncellendi! Giriş yapabilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                
                }

                else
                {
                    MessageBox.Show("Girilen bilgilere ait kullanıcı bulunamadı! E-Posta veya telefon hatalı.", "Hata", MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }
    }
}
