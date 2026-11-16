using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RestoranSiparisVeSadakatSistemi.Models
{
    public class Odeme
    {
        [Key]
        public int OdemeID { get; set; }
        //Ödemenin ait olduğu sipariş(yabancı anahtar)
        public int SiparisID { get; set; }
        public virtual Siparis Siparis { get; set; }

        public decimal Tutar {  get; set; }

        //Başarili veya başarısız durumu
        [Required,MaxLength(20)]
        public string Durum { get; set; }

        //Ödeme servisinin verdiği işlem numarası (başarısızsa boş kalabilir)
        [MaxLength(50)]
        public string IslemNo { get; set; }

        //Başarısız ödemede hata açıklaması
        [MaxLength(250)]
        public string HataMesaji { get; set; }

        public DateTime Tarih {  get; set; }= DateTime.Now;
    }
}
