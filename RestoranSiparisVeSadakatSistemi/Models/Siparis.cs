using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace RestoranSiparisVeSadakatSistemi.Models
{
    public class Siparis
    {
        [Key]
        public int SiparisID { get; set; }
        public int? MasaID { get; set; }
        public int? MusteriID { get; set; }
        public virtual Musteri Musteri { get; set; }

        public DateTime Tarih {  get; set; }= DateTime.Now;
        public decimal ToplamTutar {  get; set; }
        public string Durum { get; set; } = "Hazırlanıyor";
        public int KazanilanPuan { get; set; } = 0;

        public virtual ICollection<SiparisDetay> SiparisDetayları { get; set; }
        public virtual ICollection<PuanHareket> PuanHareketleri{ get; set; }
    }
}
