using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestoranSiparisVeSadakatSistemi.Models
{
    public class SiparisDetay
    {
        [Key]
        public int DetayID { get; set; }
        public int SiparisID { get; set; }
        public Siparis Siparis { get; set; }
        public int UrunID { get; set; }
        public Urun Urun { get; set; }
        public int Adet {  get; set; }
        public decimal BirimFiyat { get; set; }

    }
}
