using RestoranSiparisVeSadakatSistemi.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;



namespace RestoranSiparisVeSadakatSistemi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("Server=.\\SQLEXPRESS;Database=RestoranSiparisDb;Trusted_Connection=True;")
        {
        }
        public DbSet<Musteri> Musteriler { get; set; }
        public DbSet<Kategori> Kategoriler { get; set; }
        public DbSet<Urun> Urunler { get; set; }
        public DbSet<Siparis> Siparisler { get; set; }
        public DbSet<SiparisDetay> SiparisDetaylari { get; set; }
        public DbSet<PuanHareket> PuanHareketleri { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PuanHareket>()
                .HasRequired(p => p.Siparis)
                .WithMany()
                .HasForeignKey(p => p.SiparisID)
                .WillCascadeOnDelete(false);
            base.OnModelCreating(modelBuilder);
        }
    }
}

       
