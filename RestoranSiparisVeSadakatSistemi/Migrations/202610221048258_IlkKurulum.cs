namespace RestoranSiparisVeSadakatSistemi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IlkKurulum : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Kategoris",
                c => new
                    {
                        KategoriID = c.Int(nullable: false, identity: true),
                        KategoriAdi = c.String(),
                    })
                .PrimaryKey(t => t.KategoriID);
            
            CreateTable(
                "dbo.Uruns",
                c => new
                    {
                        UrunID = c.Int(nullable: false, identity: true),
                        KategoriID = c.Int(nullable: false),
                        UrunAdi = c.String(),
                        Fiyat = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.UrunID)
                .ForeignKey("dbo.Kategoris", t => t.KategoriID, cascadeDelete: true)
                .Index(t => t.KategoriID);
            
            CreateTable(
                "dbo.SiparisDetays",
                c => new
                    {
                        DetayID = c.Int(nullable: false, identity: true),
                        SiparisID = c.Int(nullable: false),
                        UrunID = c.Int(nullable: false),
                        Adet = c.Int(nullable: false),
                        BirimFiyat = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.DetayID)
                .ForeignKey("dbo.Siparis", t => t.SiparisID, cascadeDelete: true)
                .ForeignKey("dbo.Uruns", t => t.UrunID, cascadeDelete: true)
                .Index(t => t.SiparisID)
                .Index(t => t.UrunID);
            
            CreateTable(
                "dbo.Siparis",
                c => new
                    {
                        SiparisID = c.Int(nullable: false, identity: true),
                        MusteriID = c.Int(nullable: false),
                        Tarih = c.DateTime(nullable: false),
                        ToplamTutar = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.SiparisID)
                .ForeignKey("dbo.Musteris", t => t.MusteriID, cascadeDelete: true)
                .Index(t => t.MusteriID);
            
            CreateTable(
                "dbo.Musteris",
                c => new
                    {
                        MusteriID = c.Int(nullable: false, identity: true),
                        Ad = c.String(),
                        Soyad = c.String(),
                        Telefon = c.String(),
                        Toplampuan = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MusteriID);
            
            CreateTable(
                "dbo.PuanHarekets",
                c => new
                    {
                        HareketID = c.Int(nullable: false, identity: true),
                        MusteriID = c.Int(nullable: false),
                        SiparisID = c.Int(nullable: false),
                        Puan = c.Int(nullable: false),
                        Tarih = c.DateTime(nullable: false),
                        Siparis_SiparisID = c.Int(),
                    })
                .PrimaryKey(t => t.HareketID)
                .ForeignKey("dbo.Musteris", t => t.MusteriID, cascadeDelete: true)
                .ForeignKey("dbo.Siparis", t => t.SiparisID)
                .ForeignKey("dbo.Siparis", t => t.Siparis_SiparisID)
                .Index(t => t.MusteriID)
                .Index(t => t.SiparisID)
                .Index(t => t.Siparis_SiparisID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SiparisDetays", "UrunID", "dbo.Uruns");
            DropForeignKey("dbo.SiparisDetays", "SiparisID", "dbo.Siparis");
            DropForeignKey("dbo.PuanHarekets", "Siparis_SiparisID", "dbo.Siparis");
            DropForeignKey("dbo.Siparis", "MusteriID", "dbo.Musteris");
            DropForeignKey("dbo.PuanHarekets", "SiparisID", "dbo.Siparis");
            DropForeignKey("dbo.PuanHarekets", "MusteriID", "dbo.Musteris");
            DropForeignKey("dbo.Uruns", "KategoriID", "dbo.Kategoris");
            DropIndex("dbo.PuanHarekets", new[] { "Siparis_SiparisID" });
            DropIndex("dbo.PuanHarekets", new[] { "SiparisID" });
            DropIndex("dbo.PuanHarekets", new[] { "MusteriID" });
            DropIndex("dbo.Siparis", new[] { "MusteriID" });
            DropIndex("dbo.SiparisDetays", new[] { "UrunID" });
            DropIndex("dbo.SiparisDetays", new[] { "SiparisID" });
            DropIndex("dbo.Uruns", new[] { "KategoriID" });
            DropTable("dbo.PuanHarekets");
            DropTable("dbo.Musteris");
            DropTable("dbo.Siparis");
            DropTable("dbo.SiparisDetays");
            DropTable("dbo.Uruns");
            DropTable("dbo.Kategoris");
        }
    }
}
