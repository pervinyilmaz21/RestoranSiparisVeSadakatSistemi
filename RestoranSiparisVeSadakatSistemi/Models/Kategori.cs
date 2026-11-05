using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RestoranSiparisVeSadakatSistemi.Models
{
    public class Kategori
    {
        [Key]
        public int  KategoriID { get; set; }
        [Required,MaxLength(50)]
        public String KategoriAdi { get; set; }
        public virtual ICollection<Urun>Urunler { get; set; }
    }
}
