using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace RestoranSiparisVeSadakatSistemi.Models
{
    public class PuanHareket
    {
        [Key]
        public int HareketID { get; set; }
        public int MusteriID { get; set; }
        public Musteri Musteri { get; set; }
        public int SiparisID { get; set; }
        public Siparis Siparis { get; set; }
        public int Puan {  get; set; }
        public DateTime Tarih {  get; set; } = DateTime.Now;    
    }
}
