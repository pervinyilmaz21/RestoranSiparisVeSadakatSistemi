using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestoranSiparisVeSadakatSistemi.Services
{
   public class OdemeSonucu
    {
        public bool Basarili {  get; set; }
        public string IslemNo {  get; set; }
        public string HataMesaji { get; set; }

    }
    //simüle ödeme servisi geröek para çekmez gerçek birödeme apı si gibi 
    //gecikmeli cevap verir kartı kontrol eder ve işlem numarası üretir.
    public class OdemeServisi
    {
        //test kartları (gerçek ödeme sistemindeki test kartları gibi)
        public const string BasariliTestKarti = "4111111111111111";
        public const string ReddedilenTestKarti = "4000000000000002";
        public const string ZamanAsimiTestKarti = "4000000000000119";
        public async Task<OdemeSonucu>OdemeYapAsync(decimal tutar, string  kartNo)
        {
            //ağ üzerinden bankaya gidip gelme süresini taklit eder.(1,5 saniye)
            await Task.Delay(1500);
            if (tutar <= 0)
                return new OdemeSonucu { Basarili = false, HataMesaji = "Geçersiz tutar" };
            if (string.IsNullOrWhiteSpace(kartNo) || kartNo.Length != 16 || !kartNo.All(char.IsDigit))
                return new OdemeSonucu { Basarili = false, HataMesaji = "Kart numarası 16 haneli olmalıdır." };
            if (kartNo == ReddedilenTestKarti)
                return new OdemeSonucu { Basarili = false, HataMesaji = "Kart reddedildi:Yetersiz bakiye." };
            if (kartNo == ZamanAsimiTestKarti)
                throw new TimeoutException("Ödeme servisi zamanında yanıt vermedi.");

            //Başarılı ödeme:benzersizbir işlem numarası üret
            string islemNo ="ODM-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(1000,9999);
            return new OdemeSonucu { Basarili = true, IslemNo = islemNo };
            
        }
    }

}
