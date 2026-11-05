using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RestoranSiparisVeSadakatSistemi.Models
{
    public class Urun
    {
        [Key]
        public int UrunID { get; set; }
        public int KategoriID {  get; set; }
        [ForeignKey("KategoriID")]
        public virtual Kategori Kategori { get; set; }
        [Required,MaxLength(100)]
        public string UrunAdi { get; set; }
        public string Aciklama { get; set; }
        public decimal Fiyat {  get; set; }
        public bool AktifMi {  get; set; }
        public virtual ICollection<SiparisDetay> SiparisDetaylari { get; set; }
    }
}
