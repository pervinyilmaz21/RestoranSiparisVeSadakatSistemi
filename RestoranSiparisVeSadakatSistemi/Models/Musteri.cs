using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace RestoranSiparisVeSadakatSistemi.Models
{
    public class Musteri
    {
        [Key]
        public int MusteriID { get; set; }
        [Required]
        public String AdSoyad {  get; set; }
        public string Eposta { get; set; }
        public string Telefon { get; set; }
        public string SifreHash {  get; set; }
        public string Rol { get; set; } = "Musteri";
        public int Toplampuan { get; set; } = 0;

        public virtual ICollection<Siparis> Siparisler { get; set; }
        public virtual ICollection<PuanHareket> PuanHareketleri { get; set; }
    }
    
}
