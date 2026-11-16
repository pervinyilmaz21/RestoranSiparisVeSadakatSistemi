namespace RestoranSiparisVeSadakatSistemi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OdemeTablosu : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Odemes",
                c => new
                    {
                        OdemeID = c.Int(nullable: false, identity: true),
                        SiparisID = c.Int(nullable: false),
                        Tutar = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Durum = c.String(nullable: false, maxLength: 20),
                        IslemNo = c.String(maxLength: 50),
                        HataMesaji = c.String(maxLength: 250),
                        Tarih = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OdemeID)
                .ForeignKey("dbo.Siparis", t => t.SiparisID, cascadeDelete: true)
                .Index(t => t.SiparisID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Odemes", "SiparisID", "dbo.Siparis");
            DropIndex("dbo.Odemes", new[] { "SiparisID" });
            DropTable("dbo.Odemes");
        }
    }
}
