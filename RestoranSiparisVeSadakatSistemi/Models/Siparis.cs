using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RestoranSiparisVeSadakatSistemi.Models
{
    public class Siparis
    {
        public int SiparisID { get; set; }
        public int MusteriID { get; set; }
        public Musteri Musteri { get; set; }

        public DateTime Tarih {  get; set; }= DateTime.Now;
        public decimal ToplamTutar {  get; set; }

        public ICollection<SiparisDetay> SiparisDetayları { get; set; }
        public ICollection<PuanHareket> PuanHareketleri{ get; set; }
    }
}
